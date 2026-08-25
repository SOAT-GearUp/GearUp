# Manifestos Kubernetes — GearUp Fase 2

| Arquivo | Objeto | Papel |
|---|---|---|
| `namespace.yaml` | Namespace | Isola a aplicação no namespace `gearup` |
| `configmap.yaml` | ConfigMap | Configuração não sensível (ambiente, porta, JWT issuer/audience, host/porta do banco) |
| `secret.yaml` | Secret | Configuração sensível em base64 (connection string, chave JWT, senhas) |
| `deployment.yaml` | Deployment | Pods da API com requests/limits e as três probes (`startup`, `liveness`, `readiness`) |
| `service.yaml` | Service ×2 | `ClusterIP` interno + `LoadBalancer` (NLB) externo |
| `hpa.yaml` | HorizontalPodAutoscaler | Escala de 2 a 10 pods por CPU (75%) e memória (80%) |
| `kustomization.yaml` | Kustomization | Aplica tudo acima na ordem correta |
| `persistencia/` | PV, PVC, StatefulSet, Service | PostgreSQL **em cluster** — alternativa ao RDS |

## Fluxo do tráfego

```
Internet
   │
   ▼
NLB (Service gearup-api-lb, subnets públicas)
   │  health check -> /health/ready
   ▼
Service gearup-api (ClusterIP :80) ──► pods gearup-api :8080
   │                                        │
   │                                   readinessProbe /health/ready
   │                                   livenessProbe  /health/live
   ▼                                        ▼
HPA observa CPU/memória               RDS PostgreSQL :5432
e ajusta as réplicas (2..10)          (subnets privadas)
```

## Passo a passo

### 1. Publicar a imagem

O `LabRole` tem acesso **somente leitura** ao ECR, então o caminho mais curto no
Learner Lab é o Docker Hub:

```bash
# na raiz do repositório
docker build -f src/GearUp.Api/Dockerfile -t SEU_USUARIO/gearup-api:1.0.0 .
docker push SEU_USUARIO/gearup-api:1.0.0
```

Depois troque a imagem em `deployment.yaml` (campo `image:`).

### 2. Preencher o Secret

```bash
# A connection string do RDS, já em base64, sai do Terraform:
cd ../infra && terraform output -raw connection_string_base64
```

Cole o valor em `secret.yaml` → `data.connection-string` e gere as demais
chaves (instruções no cabeçalho do arquivo). A `jwt-key` precisa de **no mínimo
32 bytes** — abaixo disso a API falha no start com
`Jwt:Key deve possuir pelo menos 32 bytes`.

### 3. Aplicar

```bash
aws eks update-kubeconfig --region us-east-1 --name gearup-eks

# tudo de uma vez, na ordem de dependência
kubectl apply -k .

# ou arquivo por arquivo, se preferir explicitar a ordem
kubectl apply -f namespace.yaml
kubectl apply -f configmap.yaml
kubectl apply -f secret.yaml
kubectl apply -f deployment.yaml
kubectl apply -f service.yaml
kubectl apply -f hpa.yaml
```

### 4. Verificar

```bash
kubectl get pods -n gearup -w
kubectl rollout status deployment/gearup-api -n gearup

# HPA: a coluna TARGETS não pode ficar em <unknown> (falta metrics-server)
kubectl get hpa -n gearup

# Endereço público (o NLB leva ~2-3 min para ficar ativo)
kubectl get svc gearup-api-lb -n gearup -o jsonpath='{.status.loadBalancer.ingress[0].hostname}'

# Sem LoadBalancer, acesse localmente (e economize US$ 0,0225/h):
kubectl port-forward -n gearup svc/gearup-api 8080:80
# -> http://localhost:8080/swagger  |  http://localhost:8080/health/ready
```

### 5. Testar o autoscaling

```bash
# Gera carga contra o Service interno
kubectl run carga --rm -it --image=busybox:1.36 -n gearup --restart=Never -- \
  sh -c 'while true; do wget -q -O- http://gearup-api/health/ready > /dev/null; done'

# Em outro terminal
kubectl get hpa gearup-api -n gearup -w
```

## Endpoints de saúde usados pelas probes

| Endpoint | Verifica | Usado por |
|---|---|---|
| `/health/live` | Apenas se o processo responde HTTP | `livenessProbe`, `startupProbe` |
| `/health/ready` | Conexão com o PostgreSQL | `readinessProbe`, health check do NLB |

A `livenessProbe` **não** checa o banco de propósito: reiniciar o pod não
conserta um banco indisponível e só geraria um ciclo de restarts. Quem reage a
isso é a `readinessProbe`, tirando o pod do balanceamento até o banco voltar.

A `startupProbe` tolera até 5 minutos porque a API roda as migrations do EF Core
e o seed do usuário admin durante a inicialização.

## Problemas comuns

| Sintoma | Causa provável |
|---|---|
| Pod em `CrashLoopBackOff` com erro de `Jwt:Key` | `jwt-key` com menos de 32 bytes |
| Pod nunca fica `Ready`, log com timeout de conexão | Security group do RDS, ou falta `SSL Mode=Require` na connection string (RDS PG 15+ exige TLS) |
| `database "GearUp" does not exist` | Use `gearup` minúsculo — o PostgreSQL rebaixa identificadores não citados |
| HPA com `TARGETS: <unknown>` | metrics-server ausente no cluster |
| Pods em `Pending` | Nós sem CPU/memória livre; reduza `replicas` ou os `requests` (subir mais nós custa dinheiro) |
| `terraform destroy` travado na VPC | Rode `kubectl delete -f service.yaml` para remover o NLB antes |

## Custo

O único objeto deste diretório que gera cobrança direta é o Service
`gearup-api-lb` (NLB, ~US$ 0,0225/h + LCU), que **continua cobrando com a sessão
do lab encerrada**. Remova-o ao terminar:

```bash
kubectl delete -f service.yaml
```
