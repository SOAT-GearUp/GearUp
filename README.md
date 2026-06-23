# GearUp

API REST para gestão de oficina mecânica, construída em .NET 10, SQL Server,
DDD e Clean Architecture.

## Executar com Docker

```powershell
Copy-Item .env.example .env
docker compose up --build
```

A API estará em `http://localhost:8080` e o Swagger em
`http://localhost:8080/swagger`. O banco e as migrations são inicializados
automaticamente.

Usuários iniciais de desenvolvimento:

| Usuário | Senha | Perfil |
|---|---|---|
| `atendente` | `GearUp@123` | Atendente |
| `auxiliar` | `GearUp@123` | Auxiliar |
| `mecanico` | `GearUp@123` | Mecânico |

Essas credenciais são exclusivas para desenvolvimento e devem ser alteradas
antes de qualquer publicação.

## Executar localmente

Configure `ConnectionStrings__GearUpDatabase` e `Jwt__Key`, depois execute:

```powershell
dotnet tool restore
dotnet ef database update --project src/GearUp.Infrastructure
dotnet run --project src/GearUp.Api
```

## Testes

```powershell
dotnet test GearUp.slnx --collect:"XPlat Code Coverage"
```

## Camadas

- `GearUp.Domain`: agregados, entidades, value objects e invariantes.
- `GearUp.Application`: use cases e contratos de persistência/serviços.
- `GearUp.Infrastructure`: EF Core, SQL Server, JWT e implementações.
- `GearUp.Api`: autenticação, autorização e contratos HTTP.

O fluxo de dependências é `Api -> Application/Infrastructure`,
`Infrastructure -> Application/Domain` e `Application -> Domain`.
