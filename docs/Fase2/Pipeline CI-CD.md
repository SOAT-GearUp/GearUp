# Pipeline CI/CD

## Objetivo

Automatizar a validação, empacotamento e publicação da aplicação GearUp em um cluster Kubernetes.

## Workflows sugeridos

| Arquivo | Responsabilidade |
|---|---|
| `.github/workflows/ci.yml` | Build, testes, cobertura e validação Docker |
| `.github/workflows/sonar.yml` | Análise de qualidade e segurança no SonarCloud |
| `.github/workflows/deploy.yml` | Build/push da imagem e deploy no Kubernetes |
| `.github/workflows/infra.yml` | Terraform plan/apply para provisionamento |

O projeto já possui workflows de CI e SonarCloud. Na Fase 2, eles devem ser evoluídos para incluir publicação de imagem e deploy.

## Pipeline de CI

Executada em:

- Pull requests.
- Push para `main` ou `master`.

Etapas:

1. Checkout do repositório.
2. Instalação do .NET SDK.
3. Restore.
4. Build em Release.
5. Execução de testes automatizados.
6. Coleta de cobertura.
7. Validação de cobertura mínima.
8. Build da imagem Docker.

Exemplo conceitual:

```yaml
name: CI

on:
  push:
    branches: [main, master]
  pull_request:

jobs:
  build-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: 10.0.x

      - run: dotnet restore GearUp.slnx
      - run: dotnet build GearUp.slnx --configuration Release --no-restore
      - run: dotnet test GearUp.slnx --configuration Release --no-build
      - run: docker build -f src/GearUp.Api/Dockerfile -t gearup-api:ci .
```

## Pipeline de CD

Executada em:

- Push na branch principal.
- Execução manual (`workflow_dispatch`).

Etapas:

1. Checkout.
2. Autenticação na AWS.
3. Login no Amazon ECR.
4. Build da imagem Docker.
5. Push da imagem para o ECR.
6. Configuração do `kubectl`.
7. Aplicação dos manifests Kubernetes.
8. Atualização da imagem no Deployment.
9. Validação do rollout.

Exemplo conceitual:

```yaml
name: Deploy

on:
  workflow_dispatch:
  push:
    branches: [main, master]

permissions:
  id-token: write
  contents: read

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - uses: aws-actions/configure-aws-credentials@v4
        with:
          role-to-assume: ${{ secrets.AWS_ROLE_TO_ASSUME }}
          aws-region: ${{ secrets.AWS_REGION }}

      - uses: aws-actions/amazon-ecr-login@v2

      - name: Build and push image
        run: |
          IMAGE=${{ secrets.ECR_REGISTRY }}/${{ secrets.ECR_REPOSITORY }}:${{ github.sha }}
          docker build -f src/GearUp.Api/Dockerfile -t $IMAGE .
          docker push $IMAGE

      - name: Configure kubectl
        run: |
          aws eks update-kubeconfig \
            --region ${{ secrets.AWS_REGION }} \
            --name ${{ secrets.EKS_CLUSTER_NAME }}

      - name: Deploy manifests
        run: |
          kubectl apply -f k8s/
          kubectl set image deployment/gearup-api gearup-api=${{ secrets.ECR_REGISTRY }}/${{ secrets.ECR_REPOSITORY }}:${{ github.sha }} -n gearup
          kubectl rollout status deployment/gearup-api -n gearup
```

## Secrets necessários

| Secret | Uso |
|---|---|
| `AWS_ROLE_TO_ASSUME` | IAM Role usada pelo GitHub Actions via OIDC |
| `AWS_REGION` | Região AWS, por exemplo `us-east-1` |
| `ECR_REGISTRY` | Registry ECR da conta |
| `ECR_REPOSITORY` | Nome do repositório ECR da API |
| `EKS_CLUSTER_NAME` | Nome do cluster EKS |
| `SONAR_TOKEN` | Token do SonarCloud |

## Variáveis da aplicação no Kubernetes

| Variável | Origem recomendada |
|---|---|
| `ASPNETCORE_ENVIRONMENT` | ConfigMap |
| `ASPNETCORE_HTTP_PORTS` | ConfigMap |
| `ConnectionStrings__GearUpDatabase` | Secret |
| `Jwt__Key` | Secret |
| `Seed__AdminUser` | Secret |
| `Seed__AdminPassword` | Secret |

## Critérios de sucesso

A pipeline deve ser considerada aprovada quando:

- Build finaliza sem erro.
- Testes automatizados passam.
- Cobertura mínima é atendida.
- Imagem Docker é criada.
- Imagem é publicada no registry.
- Manifests são aplicados no cluster.
- Rollout do Deployment finaliza com sucesso.
