# GearUp

API REST para gestão de oficina mecânica, construída em .NET 10, PostgreSQL,
DDD e Clean Architecture.

## Executar com Docker

```powershell
Copy-Item .env.example .env
docker compose up --build
```

As imagens .NET do Dockerfile vêm do Docker Hub (`bitnami/dotnet-sdk` e
`bitnami/aspnet-core`), para o build funcionar sem depender do registry da
Microsoft (`mcr.microsoft.com`).

A API estará em `http://localhost:8080` e o Swagger em
`http://localhost:8080/swagger`. O banco e as migrations são inicializados
automaticamente.

No primeiro boot, se a tabela de usuários estiver vazia, o sistema cria um
único usuário **admin** com as credenciais do `.env`:

| Variável | Descrição |
|---|---|
| `SEED_ADMIN_USER` | Nome de usuário do admin inicial |
| `SEED_ADMIN_PASSWORD` | Senha do admin inicial |

Exemplo padrão em `.env.example`:

```
SEED_ADMIN_USER=admin
SEED_ADMIN_PASSWORD=GearUp@123
```

Com o admin logado, use `POST /api/usuarios` para cadastrar os demais perfis
(`Atendente`, `Auxiliar`, `Mecanico`, `Cliente`). O **Atendente** pode criar
apenas usuários do tipo **Cliente**; o **Admin** pode criar qualquer perfil.

Altere a senha padrão do admin antes de publicar em produção.

## Testes e cobertura

Para executar os testes e conferir a cobertura deste projeto, use os comandos
abaixo na pasta **`GearUp/`** (onde está `GearUp.slnx`).

### Rodar a suíte

```powershell
dotnet test GearUp.slnx
```

Projetos de teste:

| Projeto | O que valida |
|---|---|
| `tests/GearUp.Domain.UnitTests` | Regras de domínio, invariantes e agregados |
| `tests/GearUp.Application.UnitTests` | Casos de uso da camada de aplicação |
| `tests/GearUp.Api.IntegrationTests` | Endpoints HTTP ponta a ponta |

Para um projeto só:

```powershell
dotnet test tests/GearUp.Domain.UnitTests
```

### Ver cobertura no terminal

Exibe uma **tabela por assembly** ao final da execução — forma mais rápida de
ver o percentual:

```powershell
dotnet test GearUp.slnx /p:CollectCoverage=true /p:CoverletOutputFormat=opencover --tl:off
```

O `--tl:off` evita que os logs do build cubram a tabela de cobertura.

### Cobertura em arquivo (como no CI)

Gera XML em `TestResults/` para ferramentas externas ou para conferir o mesmo
formato do pipeline:

```powershell
dotnet test GearUp.slnx --collect:"XPlat Code Coverage"
```

Apenas domínio (escopo verificado no CI):

```powershell
dotnet test tests/GearUp.Domain.UnitTests --collect:"XPlat Code Coverage"
```

O relatório fica em algo como
`TestResults/<guid>/coverage.cobertura.xml`. Abra o XML e procure
`line-rate` no elemento raiz `<coverage>` — o valor é a fração de linhas
cobertas (ex.: `0.85` = 85%).

### Critério do CI

O workflow em `.github/workflows/ci.yml` exige **pelo menos 80% de cobertura
de linhas** em `GearUp.Domain`; abaixo disso o build falha. A cobertura pode
ser conferida pela tabela do Coverlet ou pelo `line-rate` no XML gerado por
`GearUp.Domain.UnitTests`.

## Camadas

- `GearUp.Domain`: agregados, entidades, value objects e invariantes.
- `GearUp.Application`: use cases e contratos de persistência/serviços.
- `GearUp.Infrastructure`: EF Core, PostgreSQL, JWT e implementações.
- `GearUp.Api`: autenticação, autorização e contratos HTTP.

O fluxo de dependências é `Api -> Application/Infrastructure`,
`Infrastructure -> Application/Domain` e `Application -> Domain`.
