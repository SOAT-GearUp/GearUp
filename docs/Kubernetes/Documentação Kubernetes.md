# Kubernetes

A pasta `k8s/` contém dois diretórios independentes e completos — cada um
aplicável isoladamente com `kubectl apply -f`, sem Kustomize/Helm nem
indireção entre eles:

- **`k8s/dev/`** — namespace `gearup-dev`. Pensado pra Minikube/kind: Postgres
  local via `StatefulSet`, imagem `gearup-api:local` (sem registry), 1 réplica.
- **`k8s/prod/`** — namespace `gearup-prod`. Sem Postgres local — conecta em
  banco gerenciado (RDS, ver ADR-002) via secret externo. Imagem publicada em
  registry, 2+ réplicas.

| Arquivo | `dev/` | `prod/` | Função |
|---|---|---|---|
| `namespace.yaml` | ✅ `gearup-dev` | ✅ `gearup-prod` | Namespace do ambiente |
| `configmap.yaml` | ✅ (`ASPNETCORE_ENVIRONMENT=Development`, inclui `POSTGRES_DB`/`POSTGRES_USER`) | ✅ (`ASPNETCORE_ENVIRONMENT=Production`, sem `POSTGRES_DB`/`POSTGRES_USER`) | Variáveis não sensíveis |
| `secret.yaml` | ✅ template `CHANGE_ME` | — | Segredos preenchidos via `kubectl create secret` (nunca versionar com valores reais) |
| `external-secret.yaml` | — | ✅ | `ExternalSecret` (External Secrets Operator) que gera `gearup-secrets` a partir do AWS Secrets Manager |
| `postgres-statefulset.yaml` / `postgres-service.yaml` | ✅ | — | PostgreSQL local com volume persistente — só em dev |
| `migrations-job.yaml` | ✅ | ✅ | `Job` que aplica as migrations antes do rollout da API |
| `api-deployment.yaml` | ✅ (1 réplica, imagem local) | ✅ (2 réplicas, imagem de registry) | `Deployment` da API |
| `api-service.yaml` | ✅ | ✅ | Service `ClusterIP` da API |
| `api-hpa.yaml` | ✅ (1–2 réplicas) | ✅ (2–10 réplicas, CPU 70% / memória 75%) | `HorizontalPodAutoscaler` |
| `ingress.yaml` | ✅ (`dev.gearup.local`) | ✅ (domínio real de produção) | Exposição externa via `ingress-nginx` |

Ambos os `Deployment`s e o `Job` de migrations rodam com `securityContext`
reforçando o usuário não-root já definido no `Dockerfile` (`USER 1001`):
`runAsNonRoot`, `allowPrivilegeEscalation: false` e `capabilities: drop:
["ALL"]` a nível de container.

Produção requer o **External Secrets Operator** já instalado no cluster, com
um `ClusterSecretStore` chamado `aws-secrets-manager` configurado (via IAM
Role/IRSA com permissão `secretsmanager:GetSecretValue`) e um secret
`gearup/prod/api` no AWS Secrets Manager contendo
`ConnectionStrings__GearUpDatabase` (apontando pro RDS), `Jwt__Key` e
`Seed__AdminPassword`. Ver `k8s/prod/external-secret.yaml`.

Para publicar a imagem em um registry (necessário só para `prod/`):

```bash
docker build -t <registry>/gearup-api:<tag> -f src/GearUp.Api/Dockerfile .
docker push <registry>/gearup-api:<tag>
```

As migrations rodam automaticamente no boot da API (comportamento herdado do
Docker Compose), mas com múltiplas réplicas isso cria risco de corrida entre
pods aplicando a mesma migration ao mesmo tempo. Por isso a mesma imagem
suporta um modo dedicado, usado pelo `Job` de migrations:

```bash
dotnet GearUp.Api.dll --migrate-only
```

Esse modo aplica as migrations e faz o seed do usuário admin, sem subir o
Kestrel. Como `Database.MigrateAsync` é idempotente, a auto-migration da API no
boot não conflita com o `Job` — ela simplesmente não encontra nada pendente.

## Local com Minikube

Pré-requisitos: [Minikube](https://minikube.sigs.k8s.io/) e `kubectl`
instalados, Docker em execução.

```bash
# 1. Cluster
minikube start

# 2. Build da imagem e carga no node do Minikube (sem precisar de registry)
docker build -t gearup-api:local -f src/GearUp.Api/Dockerfile .
minikube image load gearup-api:local

# 3. Namespace, config e segredo
kubectl apply -f k8s/dev/namespace.yaml
kubectl apply -f k8s/dev/configmap.yaml
kubectl create secret generic gearup-secrets \
  --namespace gearup-dev \
  --from-literal=ConnectionStrings__GearUpDatabase='Host=postgres.gearup-dev.svc.cluster.local;Port=5432;Database=GearUp;Username=gearup;Password=local123' \
  --from-literal=Jwt__Key='gearup-local-development-key-change-me' \
  --from-literal=Seed__AdminPassword='GearUp@123' \
  --from-literal=postgres-password='local123'

# 4. Banco de dados
kubectl apply -f k8s/dev/postgres-statefulset.yaml
kubectl apply -f k8s/dev/postgres-service.yaml
kubectl -n gearup-dev rollout status statefulset/postgres

# 5. Migrations
kubectl apply -f k8s/dev/migrations-job.yaml
kubectl -n gearup-dev wait --for=condition=complete job/gearup-migrations --timeout=120s

# 6. API
kubectl apply -f k8s/dev/api-deployment.yaml
kubectl apply -f k8s/dev/api-service.yaml
kubectl -n gearup-dev rollout status deployment/gearup-api

# 7. Acesso local (mais simples que Ingress para uso local)
kubectl -n gearup-dev port-forward svc/gearup-api 8080:80
```

API em `http://localhost:8080`. Encerre o port-forward com `Ctrl+C` (ou
`pkill -f "port-forward svc/gearup-api"` se rodou em background).

> `GET /` sempre retorna 404 — não existe rota mapeada na raiz, em nenhum
> ambiente. Teste com `curl http://localhost:8080/health/live` para confirmar
> que a API está respondendo.

`k8s/dev/configmap.yaml` já sobe com `ASPNETCORE_ENVIRONMENT: "Development"`,
então o Swagger (`Program.cs`, mapeado só `if
(app.Environment.IsDevelopment())`) fica disponível direto em
`http://localhost:8080/swagger` sem precisar de patch manual no cluster.

Testar HPA e Ingress também localmente:

```bash
minikube addons enable metrics-server   # alimenta o HPA com CPU/memória
kubectl apply -f k8s/dev/api-hpa.yaml

minikube addons enable ingress
kubectl apply -f k8s/dev/ingress.yaml
minikube tunnel                          # em outro terminal, mantenha rodando
# adicione o host de k8s/dev/ingress.yaml (dev.gearup.local) ao /etc/hosts, apontando para 127.0.0.1
```

Depois de alterar código, repita o passo 2 (build + `minikube image load`) e
reinicie o Deployment com `kubectl -n gearup-dev rollout restart
deployment/gearup-api` — a tag `local` não muda, então o cluster não percebe a
imagem nova sem esse restart.

## Produção

A imagem precisa estar publicada em um registry acessível pelo cluster. O CI
(`.github/workflows/ci.yml`) já publica automaticamente em
`ghcr.io/<owner>/<repo>` a cada push em `main`/`master`, taggeada com o SHA do
commit e com `latest` — ou publique manualmente no ECR (ver comando acima) e
edite `image: <ECR_URI>/gearup-api:<tag>` em `k8s/prod/api-deployment.yaml` e
`k8s/prod/migrations-job.yaml` com a tag real antes de aplicar.

Se usar GHCR em vez de ECR: pacotes publicados via `GITHUB_TOKEN` nascem
**privados**, vinculados ao repositório. Para o cluster conseguir puxar a
imagem, torne o pacote público em **Package settings** no GitHub, ou crie um
`imagePullSecret` no namespace `gearup-prod` com um PAT (`read:packages`) e
referencie-o em `imagePullSecrets` nos manifestos que usam a imagem.

Pré-requisito adicional de produção: **External Secrets Operator** instalado
no cluster, com `ClusterSecretStore aws-secrets-manager` configurado (IAM
Role/IRSA com `secretsmanager:GetSecretValue`) e o secret `gearup/prod/api` já
existente no AWS Secrets Manager (ver tabela acima). Sem isso,
`k8s/prod/external-secret.yaml` fica pendente e o Deployment não sobe (não
encontra o Secret `gearup-secrets`).

Ordem de aplicação (sem Postgres local — `k8s/prod/` não tem StatefulSet,
conecta direto no RDS via `ExternalSecret`):

```bash
kubectl apply -f k8s/prod/namespace.yaml

kubectl apply -f k8s/prod/configmap.yaml
kubectl apply -f k8s/prod/external-secret.yaml
kubectl -n gearup-prod wait --for=condition=Ready externalsecret/gearup-secrets --timeout=60s

kubectl apply -f k8s/prod/migrations-job.yaml
kubectl -n gearup-prod wait --for=condition=complete job/gearup-migrations --timeout=120s

kubectl apply -f k8s/prod/api-deployment.yaml
kubectl apply -f k8s/prod/api-service.yaml
kubectl apply -f k8s/prod/api-hpa.yaml
kubectl apply -f k8s/prod/ingress.yaml

kubectl -n gearup-prod get pods,svc,hpa,ingress
```

Pré-requisitos do cluster: um `IngressController` (ex.: ingress-nginx) para o
`Ingress` funcionar, e o `metrics-server` instalado para o HPA receber métricas
de CPU/memória (sem ele os alvos ficam `<unknown>` e não há escalonamento).

## Validação dos manifestos

`scripts/validate-k8s-dev.sh` e `scripts/validate-k8s-prod.sh` sobem um
cluster [k3d](https://k3d.io/) descartável e rodam `kubectl apply --dry-run=server`
em cada manifesto do diretório correspondente — pega erros de schema/API que
`--dry-run=client` não detecta (ex.: `apiVersion` obsoleto). O script de prod
também instala o CRD do External Secrets Operator antes de validar
`external-secret.yaml`.

```bash
./scripts/validate-k8s-dev.sh
./scripts/validate-k8s-prod.sh
```

Pré-requisito: [k3d](https://k3d.io/) instalado. Cada script cria e destrói
seu próprio cluster (`trap cleanup EXIT`) — não afeta cluster local existente
(Minikube, etc).

## Fora de escopo (roadmap)

Fora do escopo deste MVP: observabilidade (Prometheus/Grafana, logs
centralizados), GitOps (ArgoCD/Flux), `NetworkPolicy`, `PodDisruptionBudget`,
TLS automático no Ingress (cert-manager) e rotação de segredos.

A estrutura `k8s/dev` e `k8s/prod` já está pronta para receber, no futuro, um
terceiro ambiente `k8s/staging`, uma pasta `observability/` (Prometheus,
Grafana, Loki, Alertmanager, OTel Collector) e um `load-tests/` com k6, sem
exigir reestruturação — basta acrescentar os diretórios seguindo o mesmo
padrão dev/prod já usado.
