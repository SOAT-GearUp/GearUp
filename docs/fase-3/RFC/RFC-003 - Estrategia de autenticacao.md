# RFC-003 — Estratégia de autenticação por CPF

| Campo | Valor |
|---|---|
| Status | Aceita (revisada em 2026-09-30: CPF + senha) |
| Data | 2026-09-29 |
| Implementação | [gearup-lambda-auth](https://github.com/SOAT-GearUp/gearup-lambda-auth) |

## Problema

Clientes da oficina precisam consultar suas ordens de serviço e aprovar orçamentos. O requisito é autenticar **pelo CPF**, numa **function serverless** que valide o CPF, consulte existência e status do cliente e gere um **JWT** para as APIs protegidas.

Ao mesmo tempo, funcionários já se autenticam com usuário e senha na API (`/api/autenticacao/login`), e a API já valida JWT HS256. Clientes também podiam ter usuário e senha (perfil `Cliente`, criado pelo atendente em `POST /api/usuarios`), mas precisavam lembrar de um nome de usuário arbitrário.

## Opções avaliadas

| Opção | Avaliação |
|---|---|
| **Lambda própria emitindo o mesmo JWT da API + Lambda authorizer no gateway** | reaproveita toda a autorização existente (roles, dono da OS); sem IAM novo; custo zero |
| Amazon Cognito com fluxo customizado (CUSTOM_AUTH) | exigiria 3 triggers Lambda e um user pool sincronizado com a tabela Clientes; a API teria de aceitar dois emissores; Cognito cria roles IAM em alguns fluxos |
| JWT authorizer nativo do HTTP API | exige issuer OIDC com JWKS (chaves assimétricas publicadas por URL); nossos tokens são HS256 |
| Autenticação por CPF dentro da própria API | não atende o requisito de function serverless |

### Fator de autenticação: só CPF × CPF + senha

| | Só CPF | **CPF + senha (escolhida)** |
|---|---|---|
| Atende "validar CPF, consultar existência e status, gerar JWT" | sim | sim — os três passos continuam, a senha é um passo adicional |
| Quem sabe o CPF de alguém entra como essa pessoa | **sim** (CPF não é segredo: aparece em nota fiscal, cadastro, boleto) | não |
| Exige usuário cadastrado antes | não | sim (o mesmo `POST /api/usuarios` que já existia) |
| Custo na Lambda | ~5 ms | ~200 ms de PBKDF2 (proposital, contra força bruta) |

Só CPF identifica, mas não autentica: qualquer pessoa que conhecesse o CPF de um cliente veria as ordens de serviço dele e poderia aprovar orçamentos em seu nome. A senha fecha essa porta sem mudar o contrato do token nem a API.

## Proposta

1. **`POST /auth/cpf`** `{ "cpf", "senha" }` (rota pública do API Gateway) → Lambda `gearup-auth-cpf-<amb>`:
   - valida formato e dígitos verificadores do CPF com o mesmo algoritmo do value object `Documento` e exige a senha (`400`);
   - consulta `"Clientes"` por `"Documento"` (índice único) no RDS, via TLS, junto com os hashes dos usuários ativos de perfil `Cliente` ligados a ele (`"Usuarios"."ClienteId"`);
   - `404` se o cliente não existir, `403` se `Ativo = false`, `503` se o banco falhar;
   - verifica a senha no mesmo formato do `PasswordHasher` da API (`PBKDF2-SHA256$210000$salt$hash`, comparação em tempo constante); `401 CREDENCIAIS_INVALIDAS` se errada **ou** se não houver usuário — mesma resposta e mesmo tempo (um PBKDF2 fictício é calculado quando não há hash);
   - emite JWT HS256 com `iss=GearUp`, `aud=GearUp.Clients`, `sub`/`cliente_id` = id do cliente, `role=Cliente`, `amr=cpf`, `exp` = 60 min.
2. **Lambda authorizer** (`gearup-autorizador-<amb>`) em `ANY /api/{proxy+}`: valida assinatura, issuer, audience e expiração; resultado em cache por 5 min por token.
3. **A API continua validando** o token (defesa em profundidade) e decide o que cada perfil pode fazer. `PodeAcessarOrdemServico` garante que um cliente só vê as próprias OS.

A Lambda **só lê** clientes e hashes; criar usuário e trocar senha continuam sendo responsabilidade da API. A compatibilidade do hash é garantida por um teste que verifica, na Lambda, um hash gerado pelo .NET (`gearup-lambda-auth/test/senha.test.mjs`).

A chave de assinatura é única por ambiente, gerada pela pipeline da API e guardada em SSM SecureString; a Lambda a recebe no deploy.

### Por que HS256 compartilhado e não RS256

RS256 permitiria que a API validasse sem conhecer a chave privada — melhor em sistemas com muitos consumidores. Aqui há um único emissor (GearUp) e um único validador lógico (a própria plataforma), o login de funcionários já é HS256 e a troca exigiria migrar tokens existentes. Fica registrado como evolução natural caso surjam serviços de terceiros consumindo os tokens.

## Segurança

| Ameaça | Controle |
|---|---|
| Alguém que conhece o CPF se passar pelo cliente | senha obrigatória, verificada com PBKDF2-SHA256 de 210.000 iterações |
| Força bruta de senha | PBKDF2 lento por design + throttling do gateway (20 req/s, rajada 40) + alarme de erros no CloudWatch |
| Descobrir se o cliente tem usuário | "sem usuário" e "senha errada" devolvem o mesmo `401` no mesmo tempo |
| Token roubado | expiração de 60 min; HTTPS obrigatório no gateway |
| Vazamento de CPF ou senha em logs (LGPD) | CPF mascarado (`***.982.247-**`), senha nunca registrada — ambos cobertos por teste |
| SQL injection | consulta parametrizada (`$1`) e validação estrita do formato antes da consulta |
| Cliente acessando dados de outro | `cliente_id` vem do token, nunca da query string (`OrdensServicoController.Listar`) |

## Trade-offs assumidos

- **Existência do CPF ainda é revelada** (404 × 403 × 401). O requisito pede explicitamente "consultar a existência e o status do cliente"; mantivemos respostas distintas pela clareza e mitigamos com throttling. Em produção real, a resposta seria única.
- **Duas portas para o cliente:** o login antigo por usuário e senha em `/api/autenticacao/login` continua aceitando o perfil `Cliente`. Bloqueá-lo é uma regra simples no `AutenticarUsuarioUseCase`, deixada como decisão do time.
- **Cliente sem usuário não entra:** o atendente precisa criar o usuário (com senha inicial) ao cadastrar o cliente. Um fluxo de primeiro acesso/troca de senha é evolução natural.
