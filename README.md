# GearUp

API REST para gestão de oficina mecânica, construída em .NET 10, PostgreSQL, DDD e Clean Architecture.

O projeto evoluiu na Fase 2 do Tech Challenge para uma arquitetura cloud native, com conteinerização, Kubernetes, infraestrutura como código, pipeline CI/CD e escalabilidade automática.

## Objetivos da Fase 2

- Evoluir a aplicação mantendo Clean Architecture, DDD e testes automatizados.
- Containerizar a API com Docker e manter ambiente local com Docker Compose.
- Executar a aplicação em Kubernetes local e em AWS EKS.
- Provisionar infraestrutura AWS com Terraform.
- Automatizar build, testes, publicação da imagem Docker e deploy no cluster com GitHub Actions.
- Validar escalabilidade horizontal com HPA baseado em CPU e memória.

## Arquitetura da Solução

Fluxo principal da arquitetura proposta:

```text
Desenvolvedor
   |
   v
GitHub / GitHub Actions
   |
   |-- CI: restore, build, testes, cobertura e Docker build
   |-- SonarQube Cloud: análise de qualidade e segurança
   |-- CD: build da imagem, push no ECR e deploy no EKS
   v
Amazon ECR
   |
   v
Amazon EKS
   |
   |-- Load Balancer expõe a API
   |-- Deployment executa pods da GearUp API
   |-- ConfigMap e Secret configuram a aplicação
   |-- HPA escala réplicas conforme CPU/memória
   |-- Metrics Server fornece métricas para o HPA
   v
Amazon RDS PostgreSQL
```

O provisionamento da infraestrutura AWS é feito com Terraform, criando VPC, subnets, ECR, EKS, node group, RDS PostgreSQL e regras de rede necessárias.

## Execução Local

Com Docker Compose:

```powershell
Copy-Item .env.example .env
docker compose up --build
```

Com Kubernetes local:

```powershell
docker build -f .\src\GearUp.Api\Dockerfile -t gearup-api:local .
kubectl apply -f .\k8s\local\namespace.yaml
kubectl apply -f .\k8s\local\configmap.yaml
kubectl apply -f .\k8s\local\secret.local.yaml
kubectl apply -f .\k8s\local\postgres.yaml
kubectl apply -f .\k8s\local\api-deployment.yaml
kubectl apply -f .\k8s\local\api-service.yaml
kubectl apply -f .\k8s\local\hpa.yaml
```

## Deploy AWS

O ambiente AWS usa:

- Amazon EKS para orquestração;
- Amazon ECR para armazenamento da imagem Docker;
- Amazon RDS PostgreSQL como banco de dados;
- Load Balancer para expor a API;
- HPA para escalabilidade horizontal;
- GitHub Actions para CI/CD.

O deploy automatizado é executado pelo workflow `CD AWS`, que publica a imagem no ECR e aplica os manifests Kubernetes no EKS.

## Escalabilidade Horizontal

O HPA da API está configurado com:

- mínimo de 1 réplica;
- máximo de 3 réplicas;
- alvo de 70% de CPU;
- alvo de 80% de memória.

Durante a validação em AWS, o Metrics Server foi usado para disponibilizar métricas reais de CPU e memória ao HPA. Com a memória acima do alvo configurado, a API escalou automaticamente para 3 réplicas.

Evidência observada:

```text
gearup-api-hpa   Deployment/gearup-api   cpu: 3%/70%, memory: 90%/80%   1   3   3
```

Pods da API após escala:

```text
gearup-api-5c6b8758ff-9vl85   1/1   Running
gearup-api-5c6b8758ff-wzk82   1/1   Running
gearup-api-5c6b8758ff-zp9rz   1/1   Running
```

## Health Checks

A API possui endpoints de saúde usados pelas probes do Kubernetes:

- `/health/live`: liveness probe;
- `/health/ready`: readiness probe com validação de conexão ao PostgreSQL.

O Kubernetes só envia tráfego para a API quando a aplicação está pronta.

## Documentação

| Fase | Documentação |
|---|---|
| Fase 1 | [Documentação da Fase 1](docs/fase-1/README.md) |
| Fase 2 | [Documentação da Fase 2](docs/fase-2/README.md) |

## Documentos da Fase 2

| Tema | Documento |
|---|---|
| Decisão arquitetural | [ADR-001 - Evolução para Cloud Native e CI/CD](docs/fase-2/ADR/ADR-001%20-%20Evolucao%20para%20Cloud%20Native%20e%20CI-CD.md) |
| Docker | [Conteinerização](docs/fase-2/Docker/Conteinerizacao.md) |
| Kubernetes local | [Deploy Local com Kubernetes](docs/fase-2/Kubernetes/Deploy%20Local%20com%20Kubernetes.md) |
| Kubernetes AWS | [Deploy AWS com EKS](docs/fase-2/Kubernetes/Deploy%20AWS%20com%20EKS.md) |
| Terraform | [Provisionamento com Terraform](docs/fase-2/Infraestrutura/Provisionamento%20com%20Terraform.md) |
| Pipeline CI/CD | [Pipeline CI/CD](docs/fase-2/Pipeline/Pipeline%20CI-CD.md) |

## Código-fonte

| Pasta | Conteúdo |
|---|---|
| `src/` | Projetos da aplicação |
| `tests/` | Testes unitários e de integração |
| `docs/` | Documentação organizada por fase |
| `k8s/` | Manifests Kubernetes local e AWS |
| `infra/` | Scripts Terraform |
| `.github/workflows/` | Pipelines de CI, SonarQube e CD AWS |

## Qualidade

O projeto usa testes automatizados e SonarQube Cloud para acompanhar qualidade, segurança e cobertura.

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=SOAT-GearUp_GearUp&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=SOAT-GearUp_GearUp)
