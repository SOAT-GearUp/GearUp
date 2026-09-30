# GearUp - Fase 3

Esta fase eleva o GearUp a um nível de operação corporativa: API Gateway, autenticação serverless por CPF, banco de dados gerenciado, observabilidade com dashboards e alertas, e a plataforma separada em quatro repositórios com CI/CD e deploy automático de homologação e produção.

## Repositórios

| Repositório | Conteúdo |
|---|---|
| [GearUp](https://github.com/SOAT-GearUp/GearUp) | API, manifests Kubernetes, pipeline de deploy e esta documentação |
| [gearup-infra-k8s](https://github.com/SOAT-GearUp/gearup-infra-k8s) | VPC, EKS, ECR, Collector, Datadog Agent, dashboards e monitores |
| [gearup-infra-db](https://github.com/SOAT-GearUp/gearup-infra-db) | RDS PostgreSQL e credenciais no SSM |
| [gearup-lambda-auth](https://github.com/SOAT-GearUp/gearup-lambda-auth) | Lambda de autenticação por CPF, authorizer e API Gateway |

## Documentação

| Tema | Documento |
|---|---|
| Diagrama de componentes (nuvem, APIs, banco, monitoramento) | [Arquitetura da Solução](Arquitetura/Arquitetura%20da%20Solucao.md) |
| Diagramas de sequência (autenticação e abertura de OS) | [Diagramas de Sequência](Arquitetura/Diagramas%20de%20Sequencia.md) |
| RFC — nuvem e ambientes | [RFC-001](RFC/RFC-001%20-%20Nuvem%20e%20estrategia%20de%20ambientes.md) |
| RFC — banco de dados | [RFC-002](RFC/RFC-002%20-%20Banco%20de%20dados%20gerenciado.md) |
| RFC — autenticação | [RFC-003](RFC/RFC-003%20-%20Estrategia%20de%20autenticacao.md) |
| RFC — observabilidade | [RFC-004](RFC/RFC-004%20-%20Ferramenta%20de%20observabilidade.md) |
| ADR — comunicação via API Gateway | [ADR-002](ADR/ADR-002%20-%20Comunicacao%20sincrona%20via%20API%20Gateway.md) |
| ADR — HPA | [ADR-003](ADR/ADR-003%20-%20Escalabilidade%20com%20HPA.md) |
| ADR — repositórios e contratos via SSM | [ADR-004](ADR/ADR-004%20-%20Repositorios%20separados%20e%20contratos%20via%20SSM.md) |
| ADR — rede sem NAT | [ADR-005](ADR/ADR-005%20-%20Rede%20sem%20NAT%20Gateway.md) |
| Banco: justificativa, ER e relacionamentos | [Modelagem e Justificativa](Banco%20de%20Dados/Modelagem%20e%20Justificativa.md) |
| Observabilidade: Collector e Datadog | [OpenTelemetry Collector e Datadog](Observabilidade/OpenTelemetry%20Collector%20e%20Datadog.md) |
| Observabilidade: dashboards e alertas | [Dashboards e Alertas](Observabilidade/Dashboards%20e%20Alertas.md) |
| Operação no Learner Lab | [Guia de Deploy e Operação](Operacao/Guia%20de%20Deploy%20e%20Operacao.md) |
| Roteiro do vídeo | [Roteiro de Demonstração](Entrega/Roteiro%20do%20Video.md) |
| Documento de entrega (base do PDF) | [Documento de Entrega](Entrega/Documento%20de%20Entrega.md) |
| Postman | [GearUp - Fase 3 - Autenticação CPF](Postman/GearUp%20-%20Fase%203%20-%20Autenticacao%20CPF.postman_collection.json) |
| Teste local sem fornecedor | [OpenTelemetry Collector em modo debug](../../infra/observability/collector/README.md) |

A ADR-001 (evolução para cloud native e CI/CD) está na [documentação da Fase 2](../fase-2/ADR/ADR-001%20-%20Evolucao%20para%20Cloud%20Native%20e%20CI-CD.md).

## Mapa dos requisitos

| Requisito do Tech Challenge | Onde está |
|---|---|
| API Gateway | `gearup-lambda-auth/terraform/apigateway.tf` |
| Rotas sensíveis protegidas | Lambda authorizer em `ANY /api/{proxy+}` + `[Authorize]` na API |
| Function serverless: validar CPF, consultar cliente e status, gerar JWT | `gearup-lambda-auth/src/autenticacao.mjs` |
| 4 repositórios com CI/CD e deploy automático | tabela acima; `.github/workflows` de cada um |
| main/master protegida, PR obrigatório | branch protection nos 4 repositórios |
| Deploy de homologação e produção | branches `homolog` e `main`/`master` |
| Banco gerenciado | `gearup-infra-db` (RDS PostgreSQL) |
| Kubernetes com escalabilidade | `gearup-infra-k8s` (EKS) + `k8s/base/hpa.yaml` |
| Terraform | `gearup-infra-k8s`, `gearup-infra-db`, `gearup-lambda-auth` |
| Datadog: latência, CPU/memória, healthchecks, alertas de OS, logs JSON correlacionados | `gearup-infra-k8s/datadog` + `src/GearUp.Api/Observability` |
| Dashboards: volume diário, tempo por status, erros | dashboard "GearUp - Operação" |
| Melhoria e documentação do modelo de dados | migration `Fase3ConsistenciaEPerformance` + [Modelagem](Banco%20de%20Dados/Modelagem%20e%20Justificativa.md) |

## Implementação local da observabilidade

Os arquivos executáveis e as instruções de validação local estão em [`infra/observability/datadog`](../../infra/observability/datadog/README.md).
