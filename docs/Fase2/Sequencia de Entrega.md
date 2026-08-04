# Sequência de Entrega - Fase 2

Este documento descreve uma sequência prática para organizar a entrega da Fase 2 do GearUp.

## 1. Validar a base da Fase 1

Antes de criar infraestrutura e pipeline, a aplicação precisa estar estável.

Checklist:

- Solution compilando.
- Testes unitários executando.
- Testes de integração executando quando houver Docker disponível.
- Dockerfile funcionando.
- `docker-compose.yml` subindo API e PostgreSQL localmente.
- Swagger disponível localmente.

Comandos úteis:

```powershell
dotnet build GearUp.slnx
dotnet test GearUp.slnx
docker compose up --build
```

## 2. Revisar containerização

Entregáveis esperados:

- `src/GearUp.Api/Dockerfile`
- `docker-compose.yml`
- `.env.example`

O Dockerfile deve publicar a API e expor a porta usada pelo container. O docker-compose deve permitir execução local com API e banco PostgreSQL.

## 3. Criar manifests Kubernetes

Criar a pasta `/k8s` com arquivos YAML para:

- Namespace.
- ConfigMap.
- Secret.
- Deployment da API.
- Service da API.
- HPA.
- Opcionalmente, PostgreSQL local no cluster para ambiente de demonstração.

Validação local sugerida:

```powershell
kubectl apply -f k8s/
kubectl get pods -n gearup
kubectl get svc -n gearup
kubectl get hpa -n gearup
```

## 4. Criar infraestrutura como código

Criar a pasta `/infra` com Terraform para provisionar:

- Registry de imagem Docker.
- Cluster Kubernetes.
- Banco PostgreSQL.
- Permissões de acesso para o GitHub Actions.
- Outputs necessários para o deploy.

Para AWS, a sugestão é:

- Amazon ECR para imagens.
- Amazon EKS para Kubernetes.
- Amazon RDS PostgreSQL para banco.
- IAM Role com OIDC para o GitHub Actions.

## 5. Criar pipeline CI

O pipeline de integração contínua deve rodar em Pull Requests e pushes para a branch principal.

Etapas mínimas:

- Checkout.
- Setup .NET.
- Restore.
- Build.
- Tests.
- Coverage.
- Build da imagem Docker.

Arquivo sugerido:

```text
.github/workflows/ci.yml
```

## 6. Criar pipeline CD

O pipeline de entrega contínua deve rodar após merge na branch principal ou por execução manual.

Etapas mínimas:

- Login na AWS.
- Login no ECR.
- Build da imagem Docker.
- Push da imagem.
- Configuração do `kubectl`.
- Aplicação dos manifests Kubernetes.
- Atualização da imagem do Deployment.
- Validação do rollout.

Arquivo sugerido:

```text
.github/workflows/deploy.yml
```

## 7. Atualizar documentação

O README deve conter:

- Descrição da solução da Fase 2.
- Desenho da arquitetura.
- Como executar localmente.
- Como provisionar a infraestrutura.
- Como fazer deploy em Kubernetes.
- Link da collection de APIs.
- Link do vídeo demonstrativo.

## 8. Preparar vídeo de entrega

O vídeo deve demonstrar:

- Pipeline executando.
- Deploy da aplicação.
- API funcionando.
- Escalabilidade automática.
- Consumo das APIs principais.

Sugestão de roteiro:

1. Mostrar arquitetura.
2. Mostrar GitHub Actions.
3. Mostrar imagem publicada no registry.
4. Mostrar recursos no cluster Kubernetes.
5. Executar uma chamada na API.
6. Mostrar HPA configurado.
