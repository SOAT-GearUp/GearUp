# Deploy AWS com EKS

## Objetivo

Executar a API GearUp em um cluster Kubernetes na AWS, usando EKS para orquestração, ECR para armazenamento da imagem Docker e RDS PostgreSQL como banco de dados.

## Estrutura

Os manifests da AWS ficam em:

```text
k8s/aws/
├── namespace.yaml
├── configmap.yaml
├── secret.example.yaml
├── api-deployment.yaml
├── api-service.yaml
└── hpa.yaml
```

Diferente do ambiente local, a versão AWS não cria PostgreSQL dentro do cluster. A aplicação deve acessar um banco externo, preferencialmente RDS PostgreSQL.

## Componentes

| Manifest | Função |
|---|---|
| `namespace.yaml` | Cria o namespace `gearup` |
| `configmap.yaml` | Define configurações não sensíveis |
| `secret.example.yaml` | Exemplo de secrets esperados |
| `api-deployment.yaml` | Executa a API GearUp no cluster |
| `api-service.yaml` | Expõe a API por Load Balancer |
| `hpa.yaml` | Configura escalabilidade horizontal |

## Imagem Docker

O deployment AWS usa imagem do ECR:

```yaml
image: 469257649079.dkr.ecr.us-east-1.amazonaws.com/gearup:latest
```

Na pipeline CI/CD, a recomendação é substituir `latest` por uma tag versionada, preferencialmente o SHA do commit.

Exemplo:

```yaml
image: 469257649079.dkr.ecr.us-east-1.amazonaws.com/gearup:<commit-sha>
```

## Secret

Crie o secret real a partir do exemplo:

```powershell
Copy-Item .\k8s\aws\secret.example.yaml .\k8s\aws\secret.local.yaml
```

Depois ajuste:

- `Jwt__Key`;
- `Seed__AdminPassword`;
- `ConnectionStrings__GearUpDatabase`.

O arquivo real com secrets não deve ser versionado.

## Aplicar manifests

Com o `kubectl` apontando para o cluster EKS:

```powershell
kubectl apply -f .\k8s\aws\namespace.yaml
kubectl apply -f .\k8s\aws\configmap.yaml
kubectl apply -f .\k8s\aws\secret.local.yaml
kubectl apply -f .\k8s\aws\api-deployment.yaml
kubectl apply -f .\k8s\aws\api-service.yaml
kubectl apply -f .\k8s\aws\hpa.yaml
```

## Verificar execução

```powershell
kubectl get pods -n gearup
kubectl get svc -n gearup
kubectl get hpa -n gearup
```

Para obter o endereço público criado pelo Load Balancer:

```powershell
kubectl get svc gearup-api -n gearup
```

## Health checks

A API expõe endpoints de health check usados pelas probes do Kubernetes:

- `/health/live`: usado pela liveness probe para verificar se o processo da API está em execução;
- `/health/ready`: usado pela readiness probe para verificar se a API está pronta para receber tráfego e se consegue conectar ao PostgreSQL/RDS.

A API possui probes de liveness e readiness; o Kubernetes só envia tráfego quando a aplicação está pronta.

Depois de publicar uma nova imagem com alterações de código, atualize a tag no `api-deployment.yaml` e aplique novamente o deployment.

## Observações

Os nodes do EKS precisam ter permissão para baixar imagens do ECR. Essa permissão pode ser atendida por uma role com acesso de leitura ao ECR.

O banco RDS precisa aceitar conexão a partir da rede usada pelo cluster EKS, respeitando VPC, subnets, security groups e regras de entrada.
