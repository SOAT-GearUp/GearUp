# OpenTelemetry Collector local

O modo debug permite validar o fluxo de logs, métricas e traces sem depender de uma conta ou chave de fornecedor.

## Executar todo o ambiente no Docker

Na raiz do repositório, execute o Compose principal junto com o override de observabilidade:

```powershell
docker compose `
  -f .\docker-compose.yml `
  -f .\docker-compose.observability.yml `
  up --build -d
```

O override adiciona o Collector à mesma rede da API e configura internamente:

```text
OTEL_EXPORTER_OTLP_ENDPOINT=http://otel-collector:4317
```

Nesse modo, não é necessário configurar `OTEL_EXPORTER_OTLP_ENDPOINT` no PowerShell nem utilizar `host.docker.internal`.

Teste a API e acompanhe a telemetria:

```powershell
Invoke-WebRequest http://localhost:8080/health/live
Invoke-WebRequest http://localhost:8080/health/ready

docker compose `
  -f .\docker-compose.yml `
  -f .\docker-compose.observability.yml `
  logs -f otel-collector
```

Para encerrar o ambiente completo:

```powershell
docker compose `
  -f .\docker-compose.yml `
  -f .\docker-compose.observability.yml `
  down
```

## Executar somente o Collector

Na raiz do repositório:

```powershell
docker compose `
  -f .\infra\observability\collector\docker-compose.debug.yml `
  up -d
```

Configure e execute a API:

```powershell
$env:OTEL_EXPORTER_OTLP_ENDPOINT="http://localhost:4317"
dotnet run --project .\src\GearUp.Api\GearUp.Api.csproj
```

Gere tráfego usando a porta exibida pela API e acompanhe a telemetria:

```powershell
Invoke-WebRequest http://localhost:5037/health/live

docker compose `
  -f .\infra\observability\collector\docker-compose.debug.yml `
  logs -f otel-collector
```

Os registros `ResourceSpans`, `ResourceMetrics` e `ResourceLogs` confirmam o recebimento de traces, métricas e logs.

## Encerrar somente o Collector

```powershell
docker compose `
  -f .\infra\observability\collector\docker-compose.debug.yml `
  down
```
