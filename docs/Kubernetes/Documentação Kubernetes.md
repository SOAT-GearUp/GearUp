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
| `serviceaccount.yaml` | — | ✅ | `ServiceAccount` com IRSA (`eks.amazonaws.com/role-arn`), usada pelo `SecretStore` e pelo `Deployment`/`Job` |
| `external-secret.yaml` | — | ✅ | `SecretStore` (namespaced, autentica via a `ServiceAccount` acima) + `ExternalSecret` que gera `gearup-secrets` a partir do AWS Secrets Manager |
| `postgres-statefulset.yaml` / `postgres-service.yaml` | ✅ | — | PostgreSQL local com volume persistente — só em dev |
| `migrations-job.yaml` | ✅ | ✅ | `Job` que aplica as migrations antes do rollout da API |
| `api-deployment.yaml` | ✅ (1 réplica, imagem local) | ✅ (2 réplicas, imagem de registry) | `Deployment` da API |
| `api-service.yaml` | ✅ | ✅ | Service `ClusterIP` da API |
| `api-hpa.yaml` | ✅ (1–2 réplicas) | ✅ (2–10 réplicas, CPU 70% / memória 75%) | `HorizontalPodAutoscaler` |
| `ingress.yaml` | ✅ (`dev.gearup.local`, `ingress-nginx`) | ✅ (domínio real, ALB) | Exposição externa |

Ambos os `Deployment`s e o `Job` de migrations rodam com `securityContext`
reforçando o usuário não-root já definido no `Dockerfile` (`USER 1001`):
`runAsNonRoot`, `allowPrivilegeEscalation: false` e `capabilities: drop:
["ALL"]` a nível de container.

Produção requer o **External Secrets Operator** já instalado no cluster e a
IAM Role referenciada em `k8s/prod/serviceaccount.yaml`
(`eks.amazonaws.com/role-arn`) configurada com:

- **Trust policy** liberando o OIDC provider do EKS pro
  `system:serviceaccount:gearup-prod:gearup-api-sa`.
- **Permission policy** least-privilege: `secretsmanager:GetSecretValue` no
  secret `gearup/prod/api`.

Esse secret no AWS Secrets Manager deve conter
`ConnectionStrings__GearUpDatabase` (apontando pro RDS), `Jwt__Key` e
`Seed__AdminPassword`. O `SecretStore` (`k8s/prod/external-secret.yaml`)
autentica via essa `ServiceAccount` (`auth.jwt.serviceAccountRef`) — não há
credencial estática de AWS no cluster.

O Ingress de produção usa a classe `alb` (AWS Load Balancer Controller,
precisa estar instalado no cluster) — annotations em
`k8s/prod/ingress.yaml` cobrem `scheme`, `target-type`, listener HTTPS/HTTP,
certificado ACM e healthcheck em `/health/ready`.

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

O job `deploy-prod` em `.github/workflows/ci.yml` faz o deploy completo a cada
push em `main`/`master` (nunca em PR): build + push da imagem pro ECR
(tag = SHA do commit, nunca `latest`), `aws eks update-kubeconfig` e
`kubectl apply` em `k8s/prod/`, na ordem `namespace` → `serviceaccount` →
`configmap` → `external-secret` (espera `condition=Ready`) → `migrations-job`
(recriado do zero — `Job.spec.template` é imutável, não dá pra reaplicar com
imagem nova) → `api-deployment`/`api-service`/`api-hpa`/`ingress` →
`rollout status`.

Autenticação na AWS via **GitHub OIDC** (`aws-actions/configure-aws-credentials`
com `role-to-assume`) — sem access key/secret estático como secret do
GitHub. Pré-requisitos de configuração da conta AWS (fora deste repositório):

- IAM Role em `secrets.AWS_DEPLOY_ROLE_ARN` com trust policy liberando o OIDC
  provider do GitHub Actions, e permissão pra ECR push + `eks:DescribeCluster`
  + `kubectl` no cluster (via `aws-auth` ConfigMap ou EKS Access Entries).
- Variáveis do repositório (`vars`): `AWS_REGION`, `EKS_CLUSTER_NAME`.
- IAM Role `gearup-secrets-reader` (referenciada em
  `k8s/prod/serviceaccount.yaml`) com trust policy pro OIDC do **cluster EKS**
  (IRSA — diferente da role de deploy acima, que é IRSA do GitHub Actions) e
  permissão `secretsmanager:GetSecretValue` no secret `gearup/prod/api`.
- **AWS Load Balancer Controller** instalado no cluster (pro `Ingress` classe
  `alb` funcionar) e um certificado **ACM** (referenciado em
  `k8s/prod/ingress.yaml`).
- **External Secrets Operator** instalado no cluster e o secret
  `gearup/prod/api` já existente no AWS Secrets Manager (ver seção acima). Sem
  isso, `k8s/prod/external-secret.yaml` fica pendente e o Deployment não sobe
  (não encontra o Secret `gearup-secrets`).
- `metrics-server` instalado, para o HPA receber métricas de CPU/memória (sem
  ele os alvos ficam `<unknown>` e não há escalonamento).

Pra aplicar manualmente (sem o CI, ex.: debug), repita os passos acima com
`kubectl apply -f` direto, substituindo `<ECR_URI>/gearup-api:<tag>` em
`api-deployment.yaml`/`migrations-job.yaml` pela imagem real antes de aplicar
(mesmo padrão do `sed` usado no workflow).

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
