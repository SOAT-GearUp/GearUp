# RFC-003 — Estratégia de autenticação por CPF

| Campo | Valor |
|---|---|
| Status | Aceita |
| Data | 2026-09-29 |
| Implementação | [gearup-lambda-auth](https://github.com/SOAT-GearUp/gearup-lambda-auth) |

## Problema

Clientes da oficina precisam consultar suas ordens de serviço e aprovar orçamentos. Eles não têm usuário e senha — o requisito é autenticar **pelo CPF**, numa **function serverless** que valide o CPF, consulte existência e status do cliente e gere um **JWT** para as APIs protegidas.

Ao mesmo tempo, funcionários já se autenticam com usuário e senha na API (`/api/autenticacao/login`), e a API já valida JWT HS256.

## Opções avaliadas

| Opção | Avaliação |
|---|---|
| **Lambda própria emitindo o mesmo JWT da API + Lambda authorizer no gateway** | reaproveita toda a autorização existente (roles, dono da OS); sem IAM novo; custo zero |
| Amazon Cognito com fluxo customizado (CUSTOM_AUTH) | exigiria 3 triggers Lambda e um user pool sincronizado com a tabela Clientes; a API teria de aceitar dois emissores; Cognito cria roles IAM em alguns fluxos |
| JWT authorizer nativo do HTTP API | exige issuer OIDC com JWKS (chaves assimétricas publicadas por URL); nossos tokens são HS256 |
| Autenticação por CPF dentro da própria API | não atende o requisito de function serverless |

## Proposta

1. **`POST /auth/cpf`** (rota pública do API Gateway) → Lambda `gearup-auth-cpf-<amb>`:
   - valida formato e dígitos verificadores com o mesmo algoritmo do value object `Documento`;
   - consulta `"Clientes"` por `"Documento"` (índice único) no RDS, via TLS;
   - `404` se não existir, `403` se `Ativo = false`, `503` se o banco falhar;
   - emite JWT HS256 com `iss=GearUp`, `aud=GearUp.Clients`, `sub`/`cliente_id` = id do cliente, `role=Cliente`, `exp` = 60 min.
2. **Lambda authorizer** (`gearup-autorizador-<amb>`) em `ANY /api/{proxy+}`: valida assinatura, issuer, audience e expiração; resultado em cache por 5 min por token.
3. **A API continua validando** o token (defesa em profundidade) e decide o que cada perfil pode fazer. O perfil `Cliente` já existia: `PodeAcessarOrdemServico` garante que um cliente só vê as próprias OS.

A chave de assinatura é única por ambiente, gerada pela pipeline da API e guardada em SSM SecureString; a Lambda a recebe no deploy.

### Por que HS256 compartilhado e não RS256

RS256 permitiria que a API validasse sem conhecer a chave privada — melhor em sistemas com muitos consumidores. Aqui há um único emissor (GearUp) e um único validador lógico (a própria plataforma), o login de funcionários já é HS256 e a troca exigiria migrar tokens existentes. Fica registrado como evolução natural caso surjam serviços de terceiros consumindo os tokens.

## Segurança

| Ameaça | Controle |
|---|---|
| Enumeração de CPFs | throttling do gateway (20 req/s, rajada 40); logs com CPF mascarado permitem detectar abuso sem expor dados |
| Força bruta no endpoint | mesmo throttling; resposta rápida sem revelar detalhes internos |
| Token roubado | expiração de 60 min; HTTPS obrigatório no gateway |
| Vazamento de CPF em logs (LGPD) | CPF nunca registrado inteiro (`***.982.247-**`) — coberto por teste |
| SQL injection | consulta parametrizada (`$1`) e validação estrita do formato antes da consulta |
| Cliente acessando dados de outro | `cliente_id` vem do token, nunca da query string (`OrdensServicoController.Listar`) |

## Trade-off assumido

Saber que um CPF existe (404 × 403 × 200) é uma forma de enumeração. O requisito pede explicitamente "consultar a existência e o status do cliente"; mantivemos respostas distintas pela clareza para o cliente final e mitigamos com throttling. Em produção real, trocaríamos por uma resposta única + segundo fator (código por SMS/e-mail).
