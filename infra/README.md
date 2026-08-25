# Infraestrutura (Terraform) — GearUp Fase 2

Provisiona na AWS (`us-east-1`) tudo o que a API do GearUp precisa:

| Arquivo | O que cria |
|---|---|
| `versions.tf` | Versões do Terraform/provider e o bloco de backend S3 (comentado, com instruções) |
| `providers.tf` | Provider AWS fixo em `us-east-1` + tags padrão em todos os recursos |
| `variables.tf` | Variáveis de entrada, com validações que barram configurações proibidas no Learner Lab |
| `data.tf` | AZs disponíveis e **descoberta** das roles IAM pré-existentes do lab (nunca cria IAM) |
| `vpc.tf` | VPC, Internet Gateway, 2 subnets públicas (nós/LB), 2 subnets privadas (RDS), route tables |
| `security-groups.tf` | SG do RDS liberando 5432 **somente** para o security group do cluster |
| `eks.tf` | Cluster EKS, managed node group (EC2) e addon `metrics-server` (pré-requisito do HPA) |
| `rds.tf` | Subnet group e instância PostgreSQL gerenciada no RDS |
| `outputs.tf` | Endpoints, comando do kubeconfig e a connection string (também em base64, pronta para o secret) |
| `.env.aws.example` | Modelo das credenciais temporárias do lab (o `.env.aws` preenchido não é versionado) |
| `terraform.tfvars.example` | Modelo da configuração da infra (`senha_banco` e os defaults que valem ajustar) |
| `scripts/carregar-credenciais.ps1` / `.sh` | Carregam o `.env.aws`, gravam `~/.aws/credentials` e validam a sessão |

## Arquitetura provisionada

```
                        Internet
                            |
                    [Internet Gateway]
                            |
   VPC 10.0.0.0/16 ---------+------------------------------------
     |                                                          |
     | Subnets PÚBLICAS (10.0.0.0/20, 10.0.16.0/20)             |
     |   - Worker nodes EKS (t3.medium, On-Demand)              |
     |   - Load Balancer do Service gearup-api-lb               |
     |                                                          |
     | Subnets PRIVADAS (10.0.128.0/20, 10.0.144.0/20)          |
     |   - RDS PostgreSQL (sem rota default, sem NAT Gateway)   |
     |     ingress 5432 apenas do SG do cluster                 |
     ------------------------------------------------------------
```

## Pré-requisitos

1. Terraform >= 1.6 e AWS CLI v2 instalados.

2. **Credenciais da sessão atual do lab.** Copie `.env.aws.example` para
   `.env.aws` (que está no `.gitignore`) e cole nele o bloco de
   **Start Lab → AWS Details → AWS CLI → Show**. Depois carregue:

   ```powershell
   # PowerShell (o ponto inicial é obrigatório: mantém as variáveis na sessão)
   . .\scripts\carregar-credenciais.ps1
   ```

   ```bash
   # Linux / macOS
   source ./scripts/carregar-credenciais.sh
   ```

   O script aceita o bloco no formato original do lab (`aws_access_key_id=...`),
   exporta as variáveis, grava `~/.aws/credentials` e valida com
   `aws sts get-caller-identity`. Se preferir, faça à mão: cole o bloco
   diretamente em `~/.aws/credentials` — dá no mesmo.

   As credenciais são temporárias (expiram com a sessão, máx. 4h). Ao aparecer
   `ExpiredToken` no meio de um apply: Start Lab, recopie o bloco, rode o script
   e o `terraform apply` de novo — o state local preserva o progresso.

3. **Roles IAM: nada a fazer.** O sufixo das roles do EKS muda em cada conta de
   Learner Lab (ex.: `c2205...-LabEksClusterRole-rPckPp64hltK`), então o
   Terraform as descobre por padrão de nome, com fallback para `LabRole`.
   Confira o que foi resolvido no output `roles_iam_utilizadas` do `plan`.
   Para forçar uma role específica, use `role_cluster`/`role_nos`.

4. **Configuração da infraestrutura.** Copie `terraform.tfvars.example` para
   `terraform.tfvars` (também no `.gitignore`) e preencha `senha_banco`. O resto
   das variáveis tem default e só precisa ser tocado para economizar
   (`tipo_instancia_nos`, `criar_banco_rds`).

   A divisão entre os dois arquivos é por responsabilidade, para não haver dois
   lugares definindo a mesma coisa:

   | Arquivo | Contém | Muda quando |
   |---|---|---|
   | `.env.aws` | credenciais da sessão (`AWS_*`) | a cada Start Lab (máx. 4h) |
   | `terraform.tfvars` | configuração da infra (`senha_banco`, tipos de instância) | quase nunca |

   Não use `TF_VAR_senha_banco` no `.env.aws`: o `terraform.tfvars` tem
   precedência maior e sobrescreveria o valor silenciosamente.

## Como aplicar

```bash
cd infra

# 0) Carrega as credenciais do lab (ver Pré-requisitos)
#    PowerShell: . .\scripts\carregar-credenciais.ps1
#    bash/zsh:   source ./scripts/carregar-credenciais.sh

# 1) Baixa o provider AWS e inicializa o backend (state local por padrão)
terraform init

# 2) Valida sintaxe e formata
terraform validate
terraform fmt -check

# 3) Mostra o que será criado — LEIA a lista e confira o custo estimado
terraform plan -out=gearup.tfplan

# 4) Cria a infraestrutura (o cluster EKS leva ~12 min; o RDS ~8 min)
terraform apply gearup.tfplan

# 5) Aponta o kubectl para o cluster novo
aws eks update-kubeconfig --region us-east-1 --name gearup-eks
kubectl get nodes

# 6) Pegue a connection string já em base64 para o secret do Kubernetes
terraform output -raw connection_string_base64
```

Depois disso siga o `../k8s/README.md` para publicar a aplicação.

## Ao terminar — obrigatório

```bash
terraform destroy
```

O control plane do EKS (US$ 0,10/h) e o RDS (US$ 0,018/h) **continuam
cobrando com a sessão do lab encerrada**. Além disso, ao iniciar a próxima
sessão o lab **religa automaticamente** as EC2 que ele havia parado, voltando
a cobrar sem avisar.

Se o `destroy` falhar porque o Load Balancer criado pelo Kubernetes segura a
VPC, remova o Service primeiro:

```bash
kubectl delete -f ../k8s/service.yaml
terraform destroy
```

## Custo estimado

| Recurso | Custo | Cobra fora da sessão? |
|---|---|---|
| EKS control plane | US$ 0,10/h (US$ 2,40/dia) | **Sim** |
| 2 × EC2 t3.medium | US$ 0,0832/h | Não (mas o lab as religa) |
| RDS db.t3.micro + 20 GB gp3 | ~US$ 0,018/h + ~US$ 2,30/mês | **Sim** |
| NLB do Service `LoadBalancer` | ~US$ 0,0225/h + LCU | **Sim** |
| NAT Gateway | **não usado de propósito** | — |

Total com tudo ligado: **~US$ 0,22/h ≈ US$ 5,30/dia**. Num orçamento de US$ 50
isso dá cerca de **9 dias** ininterruptos — daí a insistência no `destroy`.

Formas de economizar:

- `tipo_instancia_nos = "t3.small"` → −US$ 0,0416/h.
- `criar_banco_rds = false` + PostgreSQL como StatefulSet (`../k8s/persistencia`) → −US$ 0,018/h.
- Não aplicar o Service `LoadBalancer`; usar `kubectl port-forward` na demo → −US$ 0,0225/h.

## Decisões e trade-offs

- **Sem NAT Gateway.** Worker nodes ficam em subnets públicas com IP público,
  protegidos pelo security group gerenciado do cluster (que não abre porta de
  entrada para a internet). Um NAT custaria quase metade do control plane.
- **Sem recursos IAM.** O Learner Lab proíbe criar roles/policies/users, então
  as roles do lab são apenas referenciadas via `data "aws_iam_role"`. É também
  o motivo de não usarmos `terraform-aws-modules/eks`.
- **State local.** Sem bucket S3 para não adicionar custo e um ponto de falha.
  Instruções de migração para S3 estão em `versions.tf`. Se o state for
  perdido, ache os recursos órfãos pelo **Resource Groups & Tag Editor** (tag
  `Projeto=GearUp`) em us-east-1 e us-west-2.
- **Versão do Kubernetes não fixada.** Fixar uma versão antiga cairia em
  extended support (US$ 0,60/h).
- **RDS single-AZ, sem backup, sem Performance Insights.** Correto para lab,
  errado para produção — é um trade-off consciente de custo.

### Planos B e C, se o node group falhar por permissão

1. **Fargate profile**: sem nós, sem EBS, sem instance profile.
2. **k3s numa única EC2 t3.small** (~US$ 0,02/h) via `user_data`.

Em nenhum dos casos crie recursos IAM para contornar o erro.
