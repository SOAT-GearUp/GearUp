# Deploy na AWS

## Objetivo

Este documento descreve o caminho recomendado para publicar o GearUp em AWS utilizando Docker, Kubernetes, Terraform e GitHub Actions.

## Arquitetura recomendada

| Recurso AWS | Finalidade |
|---|---|
| Amazon ECR | Armazenar a imagem Docker da API |
| Amazon EKS | Executar a aplicação em Kubernetes |
| Amazon RDS PostgreSQL | Banco de dados da aplicação |
| IAM OIDC Provider | Permitir autenticação segura do GitHub Actions |
| IAM Role | Autorizar a pipeline a acessar ECR, EKS e Terraform |
| Elastic Load Balancer | Expor a API por meio do Service do Kubernetes |

## Pré-requisitos

Na máquina ou ambiente de execução:

- AWS CLI.
- Terraform.
- kubectl.
- Docker.
- Conta AWS com permissão para criar ECR, EKS, RDS e IAM.
- Repositório GitHub com Actions habilitado.

No GitHub:

- Configurar secrets necessários para a pipeline.
- Configurar `SONAR_TOKEN`, caso a análise do SonarCloud faça parte do fluxo.

## Passo 1 - Criar infraestrutura com Terraform

Na pasta `/infra`, criar scripts Terraform para:

- Criar ECR.
- Criar cluster EKS.
- Criar node group.
- Criar RDS PostgreSQL.
- Criar IAM Role para GitHub Actions.
- Exportar outputs úteis.

Fluxo esperado:

```powershell
cd infra
terraform init
terraform plan
terraform apply
```

Outputs esperados:

- Nome do cluster EKS.
- Endpoint do banco.
- Nome do repositório ECR.
- Registry ECR.
- Role ARN para GitHub Actions.

## Passo 2 - Publicar imagem no ECR

Após criar o ECR, a pipeline deve:

1. Autenticar na AWS.
2. Fazer login no ECR.
3. Criar a imagem Docker.
4. Publicar a imagem com tag baseada no commit.

Exemplo de tag:

```text
gearup-api:<github-sha>
```

## Passo 3 - Configurar Kubernetes

Na pasta `/k8s`, criar manifests para:

- Namespace.
- ConfigMap.
- Secret.
- Deployment.
- Service.
- HPA.

Aplicação manual:

```powershell
aws eks update-kubeconfig --region <regiao> --name <cluster>
kubectl apply -f k8s/
kubectl get pods -n gearup
kubectl get svc -n gearup
```

## Passo 4 - Configurar secrets da aplicação

As informações sensíveis devem ser configuradas como Kubernetes Secrets:

- Connection string do PostgreSQL.
- Chave JWT.
- Usuário admin inicial.
- Senha admin inicial.

Exemplo conceitual:

```powershell
kubectl create secret generic gearup-api-secrets `
  --from-literal=ConnectionStrings__GearUpDatabase="<connection-string>" `
  --from-literal=Jwt__Key="<jwt-key>" `
  --from-literal=Seed__AdminUser="admin" `
  --from-literal=Seed__AdminPassword="<senha>" `
  -n gearup
```

Em uma entrega real, o ideal é que esses Secrets sejam criados pelo Terraform ou por um mecanismo seguro de secret management.

## Passo 5 - Fazer deploy pelo GitHub Actions

Depois da infraestrutura criada, o workflow de deploy deve:

1. Receber o evento de push na branch principal.
2. Rodar build e testes.
3. Publicar a imagem no ECR.
4. Atualizar o Deployment no EKS.
5. Aguardar o rollout.

Validação:

```powershell
kubectl rollout status deployment/gearup-api -n gearup
kubectl get pods -n gearup
kubectl get hpa -n gearup
```

## Passo 6 - Acessar a aplicação

Se o Service for do tipo `LoadBalancer`, obter o endereço público:

```powershell
kubectl get svc -n gearup
```

Testar Swagger:

```text
http://<load-balancer>/swagger
```

## Estratégia alternativa para reduzir custo

Caso o custo do EKS/RDS seja alto para demonstração acadêmica, é possível usar:

- Kubernetes local com kind, minikube ou Docker Desktop.
- PostgreSQL em container no cluster.
- Terraform apenas para documentar/provisionar recursos locais ou simplificados.

Porém, para uma entrega mais aderente ao desafio, a recomendação principal é:

```text
ECR + EKS + RDS + GitHub Actions + Terraform
```

## Cuidados importantes

- Não salvar credenciais AWS no repositório.
- Não versionar secrets Kubernetes reais.
- Usar OIDC no GitHub Actions em vez de access keys fixas.
- Definir requests/limits no Deployment para o HPA funcionar.
- Garantir que a API execute migrations ou que exista etapa controlada para banco.
- Documentar todos os recursos criados e o custo esperado.
