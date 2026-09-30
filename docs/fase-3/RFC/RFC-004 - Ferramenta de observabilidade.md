# RFC-004 — Ferramenta de observabilidade

| Campo | Valor |
|---|---|
| Status | Aceita |
| Data | 2026-09-29 |
| Relacionada | [OpenTelemetry Collector e Datadog](../Observabilidade/OpenTelemetry%20Collector%20e%20Datadog.md), [Dashboards e Alertas](../Observabilidade/Dashboards%20e%20Alertas.md) |

## Problema

Monitorar latência das APIs, CPU e memória do Kubernetes, healthchecks/uptime, alertar falhas no processamento de OS, ter logs JSON correlacionados e dashboards de negócio (volume diário de OS, tempo médio por status, erros de integração).

## Opções avaliadas

| Opção | Prós | Contras |
|---|---|---|
| **Datadog** | APM, logs, métricas de Kubernetes e monitores num só produto; trial de 14 dias; conector OTel oficial | pago após o trial; linguagem de consulta proprietária |
| New Relic | free tier permanente (100 GB/mês) | agente Kubernetes via Helm mais pesado; o grupo já tinha iniciado com Datadog |
| Prometheus + Grafana + Loki + Tempo no cluster | open source, sem fornecedor | 4 componentes a operar nos nós t3.medium; sem alertas por e-mail sem configurar Alertmanager; consumiria a memória do cluster |
| CloudWatch Container Insights | nativo | exige IAM do agente (IRSA/role de nó com policy) — bloqueado no lab; cobra por métrica |

## Proposta

**Datadog, alimentado por OpenTelemetry**:

- A API usa **apenas o SDK OpenTelemetry** (sem biblioteca do Datadog) e exporta OTLP para um **OpenTelemetry Collector** no namespace `observabilidade`. O Collector aplica `memory_limiter`, `k8sattributes` e `batch` e exporta para o Datadog.
- O **Datadog Agent** (DaemonSet + Cluster Agent) coleta CPU/memória de nós e pods, estado do HPA e executa `http_check` em `/health/ready` de cada ambiente (uptime).
- **Dashboards e monitores como código** (Terraform, provider `DataDog/datadog`) no repositório `gearup-infra-k8s`, pasta `datadog/`.
- A camada serverless (Lambda em subnet privada sem NAT) é monitorada no **CloudWatch** — ver ADR-005.

Trocar de fornecedor exige apenas outro exporter no Collector e recriar dashboards; o código da API não muda.

## Sinais e onde nascem

| Sinal | Origem | Nome no Datadog |
|---|---|---|
| Latência das APIs | instrumentação ASP.NET Core (OTel) | `http.server.request.duration` (p50/p95 por rota) |
| Volume de OS | `OrdemServicoMetrics.RegistrarCriacao` | `gearup.ordens_servico.criadas` |
| Tempo por status | `StatusOrdemServicoAlteradoDomainEvent` | `gearup.ordens_servico.tempo_status` por `gearup.status.anterior` |
| Erros | `GlobalExceptionHandler` → `ApiMetrics` | `gearup.api.erros` por rota e código |
| CPU/memória | Datadog Agent (kubelet) | `kubernetes.cpu.usage.total`, `kubernetes.memory.usage` |
| Uptime | Datadog Agent `http_check` | `http.can_connect`, `network.http.response_time` |
| Logs | `AddJsonConsole` + OTLP logs com `CorrelationId`, `TraceId`, `SpanId` | Log Explorer, `service:gearup-api` |
| Traces | OTel ASP.NET Core + HttpClient | APM, serviço `gearup-api` |

## Custo

Zero na AWS além do consumo dos pods. No Datadog, o trial cobre a entrega; sem chave configurada, o Collector usa o exporter `debug` e a plataforma continua funcional.
