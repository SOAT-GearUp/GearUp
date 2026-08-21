# Conteinerização

## Objetivo

Garantir que a API GearUp possa ser empacotada e executada em container Docker, mantendo um ambiente local reproduzível para desenvolvimento e testes.

## Dockerfile da API

O Dockerfile da API está em:

```text
src/GearUp.Api/Dockerfile
```

A imagem foi configurada com build multi-stage:

- `mcr.microsoft.com/dotnet/sdk:10.0` para restore, build e publish;
- `mcr.microsoft.com/dotnet/aspnet:10.0` para execução da aplicação.

Essa escolha evita o uso de imagens com tag `latest`, reduzindo risco de mudanças inesperadas no build.

Também foi mantida a execução da aplicação com usuário não-root por meio do usuário padrão das imagens oficiais .NET:

```dockerfile
USER $APP_UID
```

## Docker Compose

O arquivo `docker-compose.yml` orquestra o ambiente local com:

- API GearUp;
- PostgreSQL;
- variáveis de ambiente;
- healthcheck do banco;
- volume persistente para os dados do PostgreSQL.

O volume do PostgreSQL utiliza o diretório recomendado pela imagem oficial:

```yaml
volumes:
  - gearup-postgres-data:/var/lib/postgresql/data
```

## Executar localmente

Na raiz do projeto:

```powershell
docker compose up --build
```

A API ficará disponível em:

```text
http://localhost:8080/swagger
```

## Validar apenas o build da API

```powershell
docker compose build api
```

## Encerrar ambiente local

```powershell
docker compose down
```

Para remover também o volume do banco:

```powershell
docker compose down -v
```

## Observações

O uso de Docker Compose é voltado para desenvolvimento local. Para a execução em ambiente orquestrado, serão utilizados manifests Kubernetes na pasta `/k8s`.
