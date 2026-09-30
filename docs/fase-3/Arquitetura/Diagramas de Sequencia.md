# Diagramas de Sequência

## 1. Autenticação do cliente por CPF e senha

O cliente informa o CPF e a senha. A Lambda valida o documento, confirma que o cliente existe e está ativo, confere a senha do usuário de perfil `Cliente` ligado a ele e devolve um JWT no mesmo formato do login de funcionários.

```mermaid
sequenceDiagram
    autonumber
    actor Cliente
    participant GW as API Gateway<br/>(HTTP API)
    participant L as Lambda<br/>gearup-auth-cpf
    participant DB as RDS PostgreSQL
    participant CW as CloudWatch Logs

    Cliente->>GW: POST /auth/cpf {"cpf": "529.982.247-25", "senha": "***"}<br/>X-Correlation-ID: abc-123
    Note over GW: throttling 20 req/s<br/>rota pública (sem authorizer)
    GW->>L: evento HTTP API 2.0
    L->>L: valida formato e dígitos verificadores do CPF<br/>e presença da senha

    alt CPF inválido ou senha ausente
        L-->>GW: 400 CPF_INVALIDO / SENHA_OBRIGATORIA
        GW-->>Cliente: 400
    else dados válidos
        L->>DB: SELECT cliente + hashes dos usuários ativos de perfil Cliente<br/>FROM "Clientes" LEFT JOIN "Usuarios" WHERE "Documento" = $1<br/>(TLS, índices UX_Clientes_Documento e IX_Usuarios_ClienteId)
        alt banco indisponível
            DB--xL: timeout / erro
            L->>CW: log level=error resultado=erro_banco
            L-->>GW: 503 BANCO_INDISPONIVEL
            GW-->>Cliente: 503
        else cliente não encontrado
            DB-->>L: 0 linhas
            L-->>GW: 404 CLIENTE_NAO_ENCONTRADO
            GW-->>Cliente: 404
        else cliente inativo (excluído)
            DB-->>L: Ativo = false
            L-->>GW: 403 CLIENTE_INATIVO
            GW-->>Cliente: 403
        else cliente ativo
            DB-->>L: Id, Nome, Ativo = true, hashes de senha
            L->>L: PBKDF2-SHA256 (210.000 iterações)<br/>comparação em tempo constante
            alt senha errada ou cliente sem usuário
                L->>CW: log level=warn resultado=credenciais_invalidas
                L-->>GW: 401 CREDENCIAIS_INVALIDAS (mesma resposta e mesmo tempo)
                GW-->>Cliente: 401
            else senha correta
                L->>L: assina JWT HS256<br/>iss=GearUp aud=GearUp.Clients<br/>sub=cliente_id role=Cliente amr=cpf exp=60min
                L->>CW: log level=info resultado=sucesso<br/>cpf=***.982.247-** correlationId=abc-123
                L-->>GW: 200 {accessToken, expiraEm, cliente}
                GW-->>Cliente: 200 + X-Correlation-ID: abc-123
            end
        end
    end
```

A senha é a do usuário criado pelo atendente em `POST /api/usuarios` (perfil `Cliente`, com `clienteId`). A Lambda só lê o hash; criar e trocar senha continua sendo papel da API.

## 2. Abertura de ordem de serviço (funcionário)

A OS é aberta por um atendente autenticado. O fluxo mostra a dupla validação do token (gateway e API), a persistência transacional e a emissão de telemetria.

```mermaid
sequenceDiagram
    autonumber
    actor A as Atendente
    participant GW as API Gateway
    participant Z as Lambda authorizer
    participant NLB as NLB
    participant API as gearup-api (EKS)
    participant DB as RDS PostgreSQL
    participant OT as OTel Collector
    participant DD as Datadog

    A->>GW: POST /api/autenticacao/login {usuario, senha}
    GW->>NLB: rota pública
    NLB->>API: login
    API->>DB: SELECT Usuarios + verificação do hash
    API-->>A: 200 {accessToken} (role=Atendente)

    A->>GW: POST /api/ordens-servico<br/>Authorization: Bearer ...<br/>X-Correlation-ID: os-42
    GW->>Z: identity source = Authorization
    Z->>Z: jwtVerify (HS256, iss, aud, exp)
    Z-->>GW: isAuthorized = true (cache 5 min por token)
    GW->>NLB: HTTP_PROXY /api/ordens-servico
    NLB->>API: requisição
    API->>API: CorrelationIdMiddleware (os-42)<br/>JwtBearer + [Authorize(Roles="Admin,Atendente")]
    API->>API: OrdemServico.Criar(...)<br/>status = Recebida
    API->>DB: INSERT OrdensServico + HistoricoOrdensServico<br/>(SaveChanges, uma transação)
    DB-->>API: ok
    API->>API: OrdemServicoCriadaDomainEvent<br/>→ gearup.ordens_servico.criadas +1
    API-->>A: 201 Created {id} + X-Correlation-ID: os-42
    API--)OT: OTLP: trace (span HTTP + span SQL),<br/>métricas, log JSON com CorrelationId
    OT--)DD: exporter datadog (lote a cada 10s)
    Note over DD: dashboard: volume diário de OS<br/>monitor: falhas em /api/ordens-servico*
```

## 3. Cliente consultando as próprias ordens

```mermaid
sequenceDiagram
    autonumber
    actor Cliente
    participant GW as API Gateway
    participant Z as Lambda authorizer
    participant API as gearup-api
    participant DB as RDS PostgreSQL

    Cliente->>GW: GET /api/ordens-servico<br/>Authorization: Bearer <token do /auth/cpf>
    GW->>Z: valida JWT
    Z-->>GW: autorizado (perfil=Cliente, metodo=cpf)
    GW->>API: GET /api/ordens-servico
    API->>API: User.IsInRole("Cliente") → clienteId = claim cliente_id<br/>(ignora qualquer clienteId da query string)
    API->>DB: SELECT ... WHERE "ClienteId" = @id ORDER BY "CriadaEm" DESC<br/>(índice IX_OrdensServico_ClienteId_CriadaEm)
    DB-->>API: somente as OS do cliente
    API-->>Cliente: 200 [...]

    Cliente->>GW: GET /api/clientes (rota de funcionário)
    GW->>API: token válido passa pelo gateway
    API-->>Cliente: 403 (perfil Cliente não autorizado)

    Cliente->>GW: GET /api/ordens-servico (sem token)
    GW-->>Cliente: 401 (barrado na borda, não chega ao cluster)
```

A aceitação do token da Lambda pela API é garantida pelos testes de contrato em `tests/GearUp.Api.IntegrationTests/Autenticacao/TokenLambdaCpfTests.cs`.
