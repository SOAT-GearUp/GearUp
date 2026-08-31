# Provisionamento com Terraform

## Objetivo

Provisionar a infraestrutura AWS necessária para executar o GearUp em Kubernetes, mantendo os recursos documentados e versionados como código.

## Estrutura

Os arquivos Terraform ficam em:

```text
infra/aws/
├── versions.tf
├── providers.tf
├── variables.tf
├── main.tf
├── outputs.tf
└── terraform.tfvars.example
```

## Recursos Provisionados

| Recurso | Descrição |
|---|---|
| VPC | Rede dedicada para o ambiente GearUp |
| Subnets públicas | Usadas por Load Balancers |
| Subnets privadas | Usadas pelos nodes do EKS |
| Subnets de banco | Usadas pelo RDS |
| NAT Gateway | Saída para internet a partir das subnets privadas |
| ECR | Repositório da imagem Docker da API |
| EKS | Cluster Kubernetes gerenciado |
| Node Group | Grupo de nodes gerenciado para executar pods |
| RDS PostgreSQL | Banco de dados gerenciado |
| Security Groups | Regras de rede entre EKS e RDS |

## Versões

O Terraform foi configurado inicialmente com:

| Item | Versão |
|---|---|
| Kubernetes EKS | `1.36` |
| PostgreSQL RDS | `18.4` |
| Provider AWS | `~> 5.0` |

A versão `1.36` foi selecionada porque está em suporte padrão no Amazon EKS. A AWS recomenda criar clusters com a versão mais recente suportada pelo EKS.

O PostgreSQL `18.4` foi selecionado porque está disponível no Amazon RDS para PostgreSQL.

Referências:

- [Amazon EKS Kubernetes versions](https://docs.aws.amazon.com/eks/latest/userguide/kubernetes-versions.html)
- [Amazon RDS for PostgreSQL versions](https://docs.aws.amazon.com/AmazonRDS/latest/PostgreSQLReleaseNotes/postgresql-versions.html)

## Pré-requisitos

- Terraform instalado;
- AWS CLI instalada;
- profile AWS configurado;
- permissões para criar VPC, EKS, EC2, IAM, RDS e ECR.

Validar profile:

```powershell
aws sts get-caller-identity --profile gearup
```

## Configuração

Copie o exemplo de variáveis:

```powershell
Copy-Item .\infra\aws\terraform.tfvars.example .\infra\aws\terraform.tfvars
```

Edite o arquivo:

```text
infra/aws/terraform.tfvars
```

Configure principalmente:

```hcl
aws_region  = "us-east-1"
aws_profile = "gearup"

cluster_name = "gearup-dev"
eks_node_instance_types = ["t3.small"]

db_username = "gearup"
db_password = "senha-forte-sem-arroba-barra-aspas-ou-espaco"
```

O arquivo `terraform.tfvars` não deve ser versionado.

Em contas com restrição de Free Tier, use instâncias elegíveis ao Free Tier no node group do EKS. O exemplo usa `t3.small`, validado como elegível na região `us-east-1`, para evitar limite de pods durante rolling updates.

Para o RDS, a senha master não pode conter `/`, `@`, aspas duplas ou espaço. Use apenas caracteres permitidos pela AWS.

## ECR já existente

Se o repositório ECR `gearup` já existir, o Terraform precisa importá-lo para o state antes do primeiro `apply`:

```powershell
cd C:\Users\joseh\source\repos\GearUp\infra\aws
terraform init
terraform import aws_ecr_repository.gearup gearup
```

Depois disso, execute o `plan`.

## Inicializar Terraform

```powershell
cd C:\Users\joseh\source\repos\GearUp\infra\aws
terraform init
```

## Validar plano

```powershell
terraform plan
```

O `plan` mostra os recursos que serão criados, alterados ou destruídos.

## Aplicar infraestrutura

```powershell
terraform apply
```

Após a criação do EKS, o Terraform exibirá o comando para configurar o `kubectl`, por exemplo:

```powershell
aws eks update-kubeconfig --region us-east-1 --name gearup-dev --profile gearup
```

## Validar Cluster

```powershell
kubectl get nodes
kubectl get namespaces
```

## Atualizar Secret do Kubernetes

Após o RDS ser criado, use o endpoint exibido no output `rds_endpoint` para montar a connection string da API:

```text
Host=<rds_endpoint>;Port=5432;Database=GearUp;Username=gearup;Password=<senha>
```

Atualize o arquivo local:

```text
k8s/aws/secret.local.yaml
```

## Aplicar manifests AWS

```powershell
kubectl apply -f .\k8s\aws\namespace.yaml
kubectl apply -f .\k8s\aws\configmap.yaml
kubectl apply -f .\k8s\aws\secret.local.yaml
kubectl apply -f .\k8s\aws\api-deployment.yaml
kubectl apply -f .\k8s\aws\api-service.yaml
kubectl apply -f .\k8s\aws\hpa.yaml
```

## Destruir infraestrutura

Para remover os recursos criados:

```powershell
terraform destroy
```

Antes de destruir, valide se não há imagens importantes no ECR e se o ambiente não está sendo usado.

## Observações

Esta configuração prioriza clareza para entrega acadêmica e ambiente de desenvolvimento/homologação.

Para produção real, seria recomendável evoluir com backend remoto para o state, criptografia gerenciada por KMS, logs mais completos, WAF, domínio, certificado TLS, RDS Multi-AZ e política de acesso mais restritiva.
