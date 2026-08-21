# Deploy Local com Kubernetes

## Objetivo

Executar o GearUp em um cluster Kubernetes local, validando a API, o PostgreSQL, os manifests YAML e a configuração inicial de escalabilidade.

## Estrutura

Os manifests locais ficam em:

```text
k8s/local/
├── namespace.yaml
├── configmap.yaml
├── secret.example.yaml
├── postgres.yaml
├── api-deployment.yaml
├── api-service.yaml
└── hpa.yaml
```

## Pré-requisitos

- Docker Desktop;
- Kubernetes local habilitado;
- `kubectl` configurado para o contexto local;
- imagem Docker da API criada localmente.

Verificar o contexto:

```powershell
kubectl config current-context
kubectl get nodes
```

## Criar a imagem local da API

Na raiz do projeto:

```powershell
docker build -f .\src\GearUp.Api\Dockerfile -t gearup-api:local .
```

O manifesto local usa:

```yaml
image: gearup-api:local
imagePullPolicy: Never
```

Assim, o Kubernetes usa a imagem disponível no ambiente local.

## Criar o Secret local

Copie o exemplo:

```powershell
Copy-Item .\k8s\local\secret.example.yaml .\k8s\local\secret.local.yaml
```

Se necessário, ajuste os valores em `secret.local.yaml`.

O arquivo `secret.local.yaml` não deve ser versionado.

## Aplicar os manifests

Crie o namespace:

```powershell
kubectl apply -f .\k8s\local\namespace.yaml
```

Aplique configurações, secrets, banco e API:

```powershell
kubectl apply -f .\k8s\local\configmap.yaml
kubectl apply -f .\k8s\local\secret.local.yaml
kubectl apply -f .\k8s\local\postgres.yaml
kubectl apply -f .\k8s\local\api-deployment.yaml
kubectl apply -f .\k8s\local\api-service.yaml
kubectl apply -f .\k8s\local\hpa.yaml
```

## Verificar execução

```powershell
kubectl get pods -n gearup
kubectl get svc -n gearup
kubectl get hpa -n gearup
```

Logs da API:

```powershell
kubectl logs -n gearup deployment/gearup-api
```

## Acessar a API

Use port-forward:

```powershell
kubectl port-forward -n gearup service/gearup-api 8080:8080
```

Acesse:

```text
http://localhost:8080/swagger
```

## Remover o ambiente

```powershell
kubectl delete namespace gearup
```

## Observações

O PostgreSQL local usa `emptyDir`, portanto os dados são descartados quando o pod é recriado. Essa configuração é adequada para desenvolvimento local e validação dos manifests.

Em ambiente AWS, o banco será externo ao cluster, preferencialmente com RDS PostgreSQL.
