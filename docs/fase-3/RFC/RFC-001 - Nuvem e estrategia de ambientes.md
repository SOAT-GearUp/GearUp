# RFC-001 — Escolha da nuvem e estratégia de ambientes

| Campo | Valor |
|---|---|
| Status | Aceita |
| Data | 2026-09-29 |
| Contexto | Tech Challenge Fase 3 |

## Problema

A Fase 3 exige API Gateway, função serverless, banco gerenciado, Kubernetes com escalabilidade, Terraform e deploy automático de **homologação e produção**. Precisamos escolher onde isso roda e como os dois ambientes coexistem sem estourar o orçamento.

## Restrições

- A conta disponível é um **AWS Academy Learner Lab**: US$ 50 não renováveis; estourar desativa a conta e apaga tudo.
- Não é possível criar usuários, grupos ou roles IAM — apenas usar `LabRole`, `LabEksClusterRole` e afins.
- Credenciais temporárias (STS) que expiram a cada sessão de até 4 h.
- Apenas `us-east-1`/`us-west-2`, instâncias até `large`, máximo de 9 EC2 simultâneas.

## Opções avaliadas

| Opção | Prós | Contras |
|---|---|---|
| **AWS (Learner Lab)** | já usada na Fase 2 (EKS + Terraform); todos os serviços exigidos existem; custo zero para o grupo | restrições de IAM e orçamento |
| Azure (créditos de estudante) | AKS sem custo de control plane | reescrever toda a infraestrutura da Fase 2; Functions + APIM com curva nova |
| GCP (trial) | GKE Autopilot | cartão de crédito obrigatório; nenhum membro com experiência |
| Kubernetes local (kind/minikube) + Kong | custo zero | não atende "banco gerenciado" nem "function serverless" em nuvem |

## Proposta

**AWS no Learner Lab**, com os serviços:

| Requisito | Serviço |
|---|---|
| API Gateway | Amazon API Gateway (HTTP API) |
| Function serverless | AWS Lambda (Node.js 22) |
| Banco gerenciado | Amazon RDS for PostgreSQL |
| Kubernetes com escalabilidade | Amazon EKS + HPA + managed node group (1–3 nós) |
| IaC | Terraform com state em S3 |

### Estratégia de ambientes

Duplicar tudo por ambiente dobraria o custo fixo (EKS US$ 0,10/h e RDS US$ 0,02/h cobram 24/7). A proposta separa o que é **custo fixo** do que é **pay-per-use**:

| Camada | Isolamento entre homolog e production |
|---|---|
| Cluster EKS | compartilhado; **namespaces** `gearup-homolog` e `gearup-production` |
| Instância RDS | compartilhada; **databases** `gearup_homolog` e `gearup_production` |
| API Gateway + Lambdas | **pilhas separadas** (cobram por requisição: homolog parado custa zero) |
| Segredos (JWT, admin) | separados por ambiente no SSM |

Consequência nas pipelines de infraestrutura compartilhada (EKS e RDS): a branch `homolog` executa `terraform plan` (valida a mudança contra o ambiente real) e somente `main` executa `apply`. As pipelines da aplicação e da Lambda fazem deploy real nos dois ambientes.

## Riscos e mitigação

| Risco | Mitigação |
|---|---|
| Esquecer recursos ligados entre sessões | workflows de `destroy` em todos os repositórios; ordem documentada no guia de operação |
| Credenciais expiradas no meio do pipeline | passo "Verificar credenciais" com mensagem clara; `scripts/atualizar-secrets-aws.ps1` atualiza os 4 repositórios de uma vez |
| Mudança de homolog afetar produção no cluster compartilhado | isolamento por namespace, quotas de réplicas por ambiente (HPA 2 × 4) |
| Um ambiente consumir conexões do outro no RDS | pool de conexões pequeno na Lambda (`max: 2`); `idle_in_transaction_session_timeout` |

## Fora de escopo

Multi-região, Multi-AZ no RDS e contas separadas por ambiente — corretos para produção real, inviáveis no orçamento do lab.
