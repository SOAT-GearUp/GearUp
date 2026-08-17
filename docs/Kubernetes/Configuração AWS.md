# Configuração do ambiente AWS

Passo a passo pra provisionar, do zero, tudo que `k8s/prod/` e o job
`deploy-prod` (`.github/workflows/ci.yml`) esperam já existir numa conta AWS:
cluster EKS, ECR, IRSA (pod → Secrets Manager e GitHub Actions → EKS/ECR),
AWS Load Balancer Controller, External Secrets Operator e certificado ACM.

Convenções usadas abaixo (troque pelos valores reais do seu ambiente):

| Placeholder | Exemplo |
|---|---|
| `<AWS_ACCOUNT_ID>` | `123456789012` |
| `<AWS_REGION>` | `us-east-1` |
| `<EKS_CLUSTER_NAME>` | `gearup-prod` |
| `<GITHUB_ORG>/<GITHUB_REPO>` | `sua-org/GearUp` |
| `<DOMINIO_PROD>` | `gearup.example.com` |

## Pré-requisitos locais

- [AWS CLI v2](https://docs.aws.amazon.com/cli/) autenticado (`aws sts get-caller-identity` funcionando)
- [eksctl](https://eksctl.io/)
- `kubectl` e [Helm](https://helm.sh/)
- [gh CLI](https://cli.github.com/) (opcional, pra configurar secrets/vars do GitHub por linha de comando)

## 1. Cluster EKS

Pule este passo se já existir um cluster — só garanta que o **OIDC provider**
do cluster está habilitado (necessário pro IRSA no passo 4).

```bash
eksctl create cluster \
  --name <EKS_CLUSTER_NAME> \
  --region <AWS_REGION> \
  --version 1.31 \
  --nodegroup-name gearup-workers \
  --node-type t3.medium \
  --nodes 2 --nodes-min 2 --nodes-max 4 \
  --managed

eksctl utils associate-iam-oidc-provider \
  --cluster <EKS_CLUSTER_NAME> --region <AWS_REGION> --approve
```

## 2. Repositório ECR

```bash
aws ecr create-repository \
  --repository-name gearup-api \
  --region <AWS_REGION> \
  --image-tag-mutability IMMUTABLE
```

## 3. AWS Load Balancer Controller (pro `Ingress` classe `alb`)

```bash
eksctl create iamserviceaccount \
  --cluster <EKS_CLUSTER_NAME> --region <AWS_REGION> \
  --namespace kube-system --name aws-load-balancer-controller \
  --attach-policy-arn arn:aws:iam::aws:policy/ElasticLoadBalancingFullAccess \
  --approve

helm repo add eks https://aws.github.io/eks-charts
helm repo update
helm install aws-load-balancer-controller eks/aws-load-balancer-controller \
  -n kube-system \
  --set clusterName=<EKS_CLUSTER_NAME> \
  --set serviceAccount.create=false \
  --set serviceAccount.name=aws-load-balancer-controller
```

> `ElasticLoadBalancingFullAccess` é o mínimo pra funcionar rápido em dev. Em
> produção real, troque pela [policy IAM oficial e restrita do
> controller](https://kubernetes-sigs.github.io/aws-load-balancer-controller/latest/deploy/installation/#iam-permissions).

## 4. Certificado ACM (TLS do Ingress)

```bash
aws acm request-certificate \
  --domain-name <DOMINIO_PROD> \
  --validation-method DNS \
  --region <AWS_REGION>
```

Valide o certificado (registro CNAME no seu provedor de DNS — a AWS Console
mostra o registro exato em **Certificate Manager → Certificates**). Anote o
`CertificateArn` retornado; ele vai em
`alb.ingress.kubernetes.io/certificate-arn` (`k8s/prod/ingress.yaml`).

## 5. External Secrets Operator

```bash
helm repo add external-secrets https://charts.external-secrets.io
helm repo update
helm install external-secrets external-secrets/external-secrets \
  -n external-secrets --create-namespace
```

## 6. Secret no AWS Secrets Manager

```bash
aws secretsmanager create-secret \
  --name gearup/prod/api \
  --region <AWS_REGION> \
  --secret-string '{
    "ConnectionStrings__GearUpDatabase": "Host=<RDS_ENDPOINT>;Port=5432;Database=GearUp;Username=gearup;Password=<SENHA_FORTE>",
    "Jwt__Key": "<CHAVE_32_BYTES_MINIMO>",
    "Seed__AdminPassword": "<SENHA_FORTE>"
  }'
```

## 7. IRSA — pod da API lê o secret acima

Cria o namespace primeiro (o mesmo `k8s/prod/namespace.yaml` do repo):

```bash
kubectl apply -f k8s/prod/namespace.yaml
```

IAM policy least-privilege:

```bash
cat > gearup-secrets-reader-policy.json <<'EOF'
{
  "Version": "2012-10-17",
  "Statement": [{
    "Effect": "Allow",
    "Action": ["secretsmanager:GetSecretValue"],
    "Resource": "arn:aws:secretsmanager:<AWS_REGION>:<AWS_ACCOUNT_ID>:secret:gearup/prod/api-*"
  }]
}
EOF

aws iam create-policy \
  --policy-name gearup-secrets-reader \
  --policy-document file://gearup-secrets-reader-policy.json
```

Role + trust policy vinculada à ServiceAccount `gearup-api-sa` (via
`eksctl`, que já cria o trust policy IRSA certo):

```bash
eksctl create iamserviceaccount \
  --cluster <EKS_CLUSTER_NAME> --region <AWS_REGION> \
  --namespace gearup-prod --name gearup-api-sa \
  --attach-policy-arn arn:aws:iam::<AWS_ACCOUNT_ID>:policy/gearup-secrets-reader \
  --approve \
  --override-existing-serviceaccounts
```

Isso cria a IAM Role e já anota a `ServiceAccount` com
`eks.amazonaws.com/role-arn` — copie esse ARN pra
`k8s/prod/serviceaccount.yaml` (substitua `<AWS_ACCOUNT_ID>` no arquivo, ou
deixe o `eksctl` sobrescrever a cada `apply` do CI, já que o nome da
`ServiceAccount` é o mesmo).

Ajuste também `k8s/prod/external-secret.yaml`: troque `region: us-east-1`
pela `<AWS_REGION>` real, se for diferente.

## 8. IRSA/OIDC — GitHub Actions faz deploy (job `deploy-prod`)

Cria o provider OIDC do GitHub na conta (uma vez só por conta AWS):

```bash
aws iam create-open-id-connect-provider \
  --url https://token.actions.githubusercontent.com \
  --client-id-list sts.amazonaws.com \
  --thumbprint-list 6938fd4d98bab03faadb97b34396831e3780aea1
```

Trust policy restrita ao repositório (troque `<GITHUB_ORG>/<GITHUB_REPO>`):

```bash
cat > gearup-github-deploy-trust.json <<EOF
{
  "Version": "2012-10-17",
  "Statement": [{
    "Effect": "Allow",
    "Principal": {
      "Federated": "arn:aws:iam::<AWS_ACCOUNT_ID>:oidc-provider/token.actions.githubusercontent.com"
    },
    "Action": "sts:AssumeRoleWithWebIdentity",
    "Condition": {
      "StringEquals": {
        "token.actions.githubusercontent.com:aud": "sts.amazonaws.com"
      },
      "StringLike": {
        "token.actions.githubusercontent.com:sub": "repo:<GITHUB_ORG>/<GITHUB_REPO>:ref:refs/heads/main"
      }
    }
  }]
}
EOF

aws iam create-role \
  --role-name gearup-github-deploy \
  --assume-role-policy-document file://gearup-github-deploy-trust.json
```

Permission policy (push no ECR + descrever/gerenciar o cluster EKS):

```bash
cat > gearup-github-deploy-policy.json <<'EOF'
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": ["ecr:GetAuthorizationToken"],
      "Resource": "*"
    },
    {
      "Effect": "Allow",
      "Action": [
        "ecr:BatchCheckLayerAvailability",
        "ecr:GetDownloadUrlForLayer",
        "ecr:BatchGetImage",
        "ecr:PutImage",
        "ecr:InitiateLayerUpload",
        "ecr:UploadLayerPart",
        "ecr:CompleteLayerUpload"
      ],
      "Resource": "arn:aws:ecr:<AWS_REGION>:<AWS_ACCOUNT_ID>:repository/gearup-api"
    },
    {
      "Effect": "Allow",
      "Action": ["eks:DescribeCluster"],
      "Resource": "arn:aws:eks:<AWS_REGION>:<AWS_ACCOUNT_ID>:cluster/<EKS_CLUSTER_NAME>"
    }
  ]
}
EOF

aws iam put-role-policy \
  --role-name gearup-github-deploy \
  --policy-name gearup-github-deploy \
  --policy-document file://gearup-github-deploy-policy.json
```

`eks:DescribeCluster` só permite montar o kubeconfig — quem decide o que a
Role pode fazer *dentro* do cluster é o RBAC do Kubernetes. Dê acesso via EKS
Access Entries (substitui o antigo `aws-auth` ConfigMap):

```bash
aws eks create-access-entry \
  --cluster-name <EKS_CLUSTER_NAME> --region <AWS_REGION> \
  --principal-arn arn:aws:iam::<AWS_ACCOUNT_ID>:role/gearup-github-deploy \
  --type STANDARD

aws eks associate-access-policy \
  --cluster-name <EKS_CLUSTER_NAME> --region <AWS_REGION> \
  --principal-arn arn:aws:iam::<AWS_ACCOUNT_ID>:role/gearup-github-deploy \
  --policy-arn arn:aws:eks::aws:cluster-access-policy/AmazonEKSAdminPolicy \
  --access-scope type=namespace,namespaces=gearup-prod
```

## 9. Secrets/vars no repositório GitHub

```bash
gh secret set AWS_DEPLOY_ROLE_ARN --body "arn:aws:iam::<AWS_ACCOUNT_ID>:role/gearup-github-deploy"
gh variable set AWS_REGION --body "<AWS_REGION>"
gh variable set EKS_CLUSTER_NAME --body "<EKS_CLUSTER_NAME>"
```

(Ou **Settings → Secrets and variables → Actions** no GitHub, se preferir pela UI.)

## 10. Ajustar placeholders nos manifestos

Antes do primeiro push pra `main`/`master`:

- `k8s/prod/serviceaccount.yaml`: `<AWS_ACCOUNT_ID>` no `role-arn` (pule se já
  ajustado pelo `eksctl create iamserviceaccount` do passo 7).
- `k8s/prod/external-secret.yaml`: `region: us-east-1` → `<AWS_REGION>`, se
  diferente.
- `k8s/prod/ingress.yaml`: `<ACM_CERTIFICATE_ARN>` (passo 4) e o `host`
  (`gearup.example.com` → `<DOMINIO_PROD>`).

`k8s/prod/api-deployment.yaml` e `migrations-job.yaml` **não precisam** de
edição manual — `<ECR_URI>/gearup-api:<tag>` é substituído automaticamente
pelo `sed` do job `deploy-prod`.

## 11. Primeiro deploy

Dê push pra `main`/`master` — o job `deploy-prod` builda, publica no ECR e
aplica tudo. Acompanhe em **Actions**, ou localmente:

```bash
aws eks update-kubeconfig --name <EKS_CLUSTER_NAME> --region <AWS_REGION>
kubectl -n gearup-prod get pods,svc,hpa,ingress
kubectl -n gearup-prod logs -l app=gearup-api --tail=100
```

DNS: aponte `<DOMINIO_PROD>` (CNAME/ALIAS) pro hostname do ALB, visível em
`kubectl -n gearup-prod get ingress gearup-ingress`.
