# OpenTelemetry Collector com Datadog

Este ambiente executa o fluxo de observabilidade local:

```text
GearUp API -> OTLP -> OpenTelemetry Collector -> Datadog
                                      |
Datadog Agent ------------------------+ (infraestrutura e contêineres)
```

A API utiliza somente o protocolo OTLP e não conhece o fornecedor. O OpenTelemetry Collector recebe, processa e exporta logs, métricas e traces. A configuração e a chave do Datadog permanecem na infraestrutura.

O Datadog Agent não recebe a telemetria da aplicação. Ele permanece no ambiente apenas para coletar informações de infraestrutura e dos contêineres locais.

## Pré-requisitos

- Docker Desktop executando com contêineres Linux.
- Conta no Datadog e uma API key.

## Configuração

Na raiz do repositório, copie o arquivo de exemplo:

```powershell
Copy-Item .\infra\observability\datadog\.env.example .\infra\observability\datadog\.env
```

Preencha `DD_API_KEY` no arquivo `.env`. Mantenha `DD_SITE=datadoghq.com`, conforme o site apresentado durante o cadastro. O arquivo real é ignorado pelo Git.

## Executar o ambiente completo

Inicie API, PostgreSQL, Collector e Agent na mesma rede Docker:

```powershell
docker compose `
  --env-file .\.env `
  --env-file .\infra\observability\datadog\.env `
  -f .\docker-compose.yml `
  -f .\docker-compose.observability.yml `
  -f .\docker-compose.datadog.yml `
  up --build -d
```

O Compose configura automaticamente a API para enviar OTLP ao endereço interno:

```text
http://otel-collector:4317
```

As variáveis da aplicação continuam no `.env` da raiz. As credenciais do Datadog ficam no `.env` específico da integração.

## Responsabilidades

- A API instrumenta logs, métricas e traces com OpenTelemetry.
- O Collector recebe os sinais por OTLP, aplica controle de memória e processamento em lote e os exporta.
- O arquivo `otel-collector-config.datadog.yaml` contém a integração específica com o Datadog.
- O Agent coleta somente informações da infraestrutura Docker local.
- A chave `DD_API_KEY` não é disponibilizada para a API.

## Validação

Valide primeiro a saúde do Collector:

```powershell
Invoke-WebRequest http://localhost:13133
```

Gere tráfego nos health checks e nos fluxos de ordem de serviço. No Datadog, verifique:

- APM > Services: serviço `gearup-api`.
- Metrics Explorer: métricas iniciadas por `gearup.`.
- Logs: filtro `service:gearup-api`.
- Infrastructure > Containers: Agent local e contêineres detectados.

Para acompanhar o Collector:

```powershell
docker compose `
  --env-file .\.env `
  --env-file .\infra\observability\datadog\.env `
  -f .\docker-compose.yml `
  -f .\docker-compose.observability.yml `
  -f .\docker-compose.datadog.yml `
  logs -f otel-collector
```

Para acompanhar o Agent:

```powershell
docker compose `
  --env-file .\.env `
  --env-file .\infra\observability\datadog\.env `
  -f .\docker-compose.yml `
  -f .\docker-compose.observability.yml `
  -f .\docker-compose.datadog.yml `
  logs -f datadog-agent
```

Para encerrar:

```powershell
docker compose `
  --env-file .\.env `
  --env-file .\infra\observability\datadog\.env `
  -f .\docker-compose.yml `
  -f .\docker-compose.observability.yml `
  -f .\docker-compose.datadog.yml `
  down
```

## Troca de fornecedor

A instrumentação da API e a variável `OTEL_EXPORTER_OTLP_ENDPOINT` permanecem inalteradas. Para utilizar New Relic, Grafana ou outro backend compatível, adicione um arquivo de configuração do Collector e um override do Compose específicos do novo fornecedor.

Dashboards, monitores, alertas e consultas pertencem ao fornecedor e precisam ser recriados ou migrados separadamente.
