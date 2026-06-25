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

## Executar localmente

Configure `ConnectionStrings__GearUpDatabase`, `Jwt__Key`, `Seed__AdminUser` e
`Seed__AdminPassword`, depois execute:

```powershell
dotnet tool restore
dotnet ef database update --project src/GearUp.Infrastructure --startup-project src/GearUp.Api
dotnet run --project src/GearUp.Api
```

## Testes

```powershell
dotnet test GearUp.slnx --collect:"XPlat Code Coverage"
```

## Camadas

- `GearUp.Domain`: agregados, entidades, value objects e invariantes.
- `GearUp.Application`: use cases e contratos de persistência/serviços.
- `GearUp.Infrastructure`: EF Core, PostgreSQL, JWT e implementações.
- `GearUp.Api`: autenticação, autorização e contratos HTTP.

O fluxo de dependências é `Api -> Application/Infrastructure`,
`Infrastructure -> Application/Domain` e `Application -> Domain`.
