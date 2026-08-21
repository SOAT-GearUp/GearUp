# GearUp - Fase 2

Documentação da evolução da aplicação para a Fase 2 do Tech Challenge.

## Objetivo

Evoluir o GearUp com melhorias na aplicação, conteinerização, Kubernetes, infraestrutura como código e pipeline CI/CD.

## Documentos

| Documento | Status |
|---|---|
| [ADR-001 - Evolução para Cloud Native e CI/CD](ADR/ADR-001%20-%20Evolucao%20para%20Cloud%20Native%20e%20CI-CD.md) | Criado |
| Evolução da aplicação | Em andamento |
| [Docker e execução local](Docker/Conteinerizacao.md) | Criado |
| [Kubernetes local](Kubernetes/Deploy%20Local%20com%20Kubernetes.md) | Criado |
| [Kubernetes AWS com EKS](Kubernetes/Deploy%20AWS%20com%20EKS.md) | Criado |
| [Terraform](Infraestrutura/Provisionamento%20com%20Terraform.md) | Criado |
| [Pipeline CI/CD](Pipeline/Pipeline%20CI-CD.md) | Criado |
| Deploy AWS | Criado |

## Evolução inicial da aplicação

Nesta fase, a aplicação está sendo ajustada para atender aos fluxos exigidos no desafio: abertura de OS com serviços e peças, consulta de status, aprovação externa de orçamento e listagem de ordens de serviço com ordenação por status.

## Observabilidade e disponibilidade

A API possui endpoints de health check para apoiar a execução em Kubernetes:

- `/health/live`: indica se o processo da API está em execução;
- `/health/ready`: indica se a aplicação está pronta para receber tráfego, incluindo validação de conexão com o PostgreSQL.

A API possui probes de liveness e readiness; o Kubernetes só envia tráfego quando a aplicação está pronta.
