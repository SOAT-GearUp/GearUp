# Configuração do GitHub Actions

Passo a passo pro lado GitHub do deploy automático: secrets/variables do
repositório, permissões do workflow e como acompanhar/depurar uma execução do
job `deploy-prod` (`.github/workflows/ci.yml`).

Pré-requisito: os recursos AWS (IAM Role, OIDC provider, cluster EKS, ECR)
já criados — ver [Configuração AWS](Configuração%20AWS.md). Este documento só
cobre a configuração do lado do GitHub.

## 1. Visão geral do workflow

`.github/workflows/ci.yml` tem dois jobs:

| Job | Quando roda | O que faz |
|---|---|---|
| `build-test` | todo push e pull request | restore/build/test, cobertura de `GearUp.Domain` (≥80%), `docker build` de validação |
| `deploy-prod` | só push em `main`/`master` (nunca em PR) | build + push da imagem pro ECR, `kubectl apply` em `k8s/prod/`, `rollout status` |

`deploy-prod` depende de `build-test` (`needs: build-test`) — só roda se o
build/testes passarem. A condição `if:` no job garante que PRs de fora nunca
disparam deploy, mesmo se alguém tentar rodar o workflow manualmente numa
branch de PR.

O bloco `permissions: id-token: write` no job `deploy-prod` é obrigatório —
sem ele, o token OIDC que o `aws-actions/configure-aws-credentials` usa pra
assumir a IAM Role não é emitido, e o passo falha com erro de autenticação.

## 2. Secrets e Variables do repositório

O workflow lê:

- `secrets.AWS_DEPLOY_ROLE_ARN` — ARN da IAM Role criada em
  [Configuração AWS](Configuração%20AWS.md#8-irsaoidc--github-actions-faz-deploy-job-deploy-prod).
- `vars.AWS_REGION` — região do cluster EKS e do ECR.
- `vars.EKS_CLUSTER_NAME` — nome do cluster.

**Pela UI:** no repositório, **Settings → Secrets and variables → Actions**.

- Aba **Secrets** → **New repository secret** → nome `AWS_DEPLOY_ROLE_ARN`,
  valor o ARN da role (ex.: `arn:aws:iam::123456789012:role/gearup-github-deploy`).
- Aba **Variables** → **New repository variable** → `AWS_REGION` (ex.:
  `us-east-1`) e `EKS_CLUSTER_NAME` (ex.: `gearup-prod`).

**Pelo `gh` CLI:**

```bash
gh secret set AWS_DEPLOY_ROLE_ARN --body "arn:aws:iam::<AWS_ACCOUNT_ID>:role/gearup-github-deploy"
gh variable set AWS_REGION --body "<AWS_REGION>"
gh variable set EKS_CLUSTER_NAME --body "<EKS_CLUSTER_NAME>"
```

Use **Secret** pro ARN da Role só por convenção de "não é algo público" —
tecnicamente um ARN de IAM Role não é segredo por si só (quem realmente
autoriza é a trust policy da Role, que exige o token OIDC assinado do
GitHub). Ainda assim, manter como secret evita expor detalhes da conta AWS em
logs de PR.

## 3. (Recomendado) Proteger o deploy com um GitHub Environment

Por padrão, qualquer push em `main`/`master` dispara o deploy sem revisão
humana. Pra exigir aprovação manual antes de aplicar em produção:

1. **Settings → Environments → New environment**, nome `production`.
2. Em **Deployment protection rules**, marque **Required reviewers** e
   adicione quem pode aprovar.
3. Mova `AWS_DEPLOY_ROLE_ARN`/`AWS_REGION`/`EKS_CLUSTER_NAME` pra dentro
   desse environment (**Environment secrets**/**Environment variables**) em
   vez de repository-level — assim só o job que declarar esse environment
   consegue lê-los.
4. No `ci.yml`, adicione `environment: production` no job `deploy-prod`:

```yaml
  deploy-prod:
    needs: build-test
    if: github.event_name == 'push' && (github.ref == 'refs/heads/main' || github.ref == 'refs/heads/master')
    runs-on: ubuntu-latest
    environment: production
    permissions:
      id-token: write
      contents: read
```

Com isso, o job fica pendente em **Actions** até alguém aprovar, antes de
rodar qualquer comando contra a AWS.

## 4. (Opcional) Branch protection

**Settings → Branches → Add branch protection rule**, padrão `main` (ou
`master`): marque **Require status checks to pass before merging** e
selecione `build-test`. Isso impede merge de PR com build/teste quebrado ou
cobertura de `GearUp.Domain` abaixo de 80%, antes mesmo de chegar perto do
deploy.

## 5. Rodando e acompanhando

Deploy dispara sozinho a cada push em `main`/`master`. Pra acompanhar:

- Aba **Actions** do repositório → selecione a execução → job `deploy-prod`
  → expanda cada step.
- Localmente, com o cluster já configurado (`aws eks update-kubeconfig`):

```bash
kubectl -n gearup-prod get pods,svc,hpa,ingress
kubectl -n gearup-prod rollout status deployment/gearup-api
kubectl -n gearup-prod logs -l app=gearup-api --tail=100
```

## 6. Troubleshooting

| Sintoma | Causa provável | Onde olhar |
|---|---|---|
| `Not authorized to perform sts:AssumeRoleWithWebIdentity` | Trust policy da IAM Role não bate com o repo/branch, ou `id-token: write` faltando | Trust policy (`token.actions.githubusercontent.com:sub`) — confira se é exatamente `repo:<org>/<repo>:ref:refs/heads/main` |
| `no identity-based policy allows the ecr:GetAuthorizationToken/PutImage action` | Permission policy da Role sem as ações de ECR, ou nome do repositório ECR diferente de `gearup-api` | Policy `gearup-github-deploy` (passo 8 de [Configuração AWS](Configuração%20AWS.md)) |
| `error: You must be logged in to the server (Unauthorized)` no `kubectl apply` | Role de deploy sem EKS Access Entry associada | `aws eks list-access-entries --cluster-name <EKS_CLUSTER_NAME>` |
| `externalsecret/gearup-secrets` nunca fica `Ready` (timeout no `wait`) | ESO não instalado, `SecretStore`/IRSA do pod mal configurado, ou secret `gearup/prod/api` não existe no Secrets Manager | `kubectl -n gearup-prod describe externalsecret gearup-secrets` |
| `Job.spec.template: field is immutable` ao aplicar o migrations job | Não deveria acontecer — o workflow já apaga o Job (`kubectl delete -f - --ignore-not-found`) antes de reaplicar; se aparecer, alguém rodou `kubectl apply` manual sem esse passo | Rode `kubectl -n gearup-prod delete job/gearup-migrations` antes de reaplicar |
