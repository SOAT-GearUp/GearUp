# Pipeline CI/CD

## Objetivo

Automatizar build, testes, criação da imagem Docker e deploy da API GearUp no Kubernetes da AWS, atendendo aos requisitos de Integração Contínua e Entrega Contínua da Fase 2.

## Workflows

Os workflows ficam em:

```text
.github/workflows/
├── ci.yml
├── build.yml
└── cd-aws.yml
```

| Workflow | Função |
|---|---|
| `ci.yml` | Executa restore, build, testes, valida cobertura mínima do domínio e build da imagem Docker em pushes e pull requests |
| `build.yml` | Executa análise do SonarQube Cloud |
| `cd-aws.yml` | Publica a imagem no ECR e faz deploy no EKS |

## CI

O workflow `ci.yml` executa:

- restore da solution;
- build em `Release`;
- testes automatizados;
- coleta de cobertura do projeto de domínio;
- validação mínima de 80% de cobertura de linhas no domínio;
- build da imagem Docker da API.

## CD AWS

O workflow `cd-aws.yml` pode ser executado manualmente pelo GitHub Actions e também em push para a branch `master`.

Ele executa:

- restore, build e testes;
- autenticação na AWS;
- resolução do Account ID da AWS;
- login no Amazon ECR;
- build da imagem Docker com tag baseada no SHA do commit e número da execução;
- push da imagem para o ECR;
- configuração do `kubectl` para o cluster EKS;
- aplicação de namespace e ConfigMap;
- criação/atualização do Secret da aplicação no cluster;
- aplicação dos manifests Kubernetes;
- atualização da imagem no Deployment;
- espera do rollout da API.

## Deploy do Banco de Dados

O deploy do banco é feito no startup da API pelo `DatabaseInitializer`, que executa:

- `MigrateAsync`, aplicando migrations pendentes no PostgreSQL;
- seed do usuário administrador inicial, quando configurado.

Como a aplicação roda dentro do EKS, ela consegue acessar o RDS pela rede privada provisionada. A pipeline aguarda o rollout do deployment; a readiness probe em `/health/ready` valida que a API está pronta e que consegue conectar ao PostgreSQL.

## Secrets do GitHub Actions

Configure os seguintes secrets no repositório GitHub:

| Secret | Uso |
|---|---|
| `AWS_ACCESS_KEY_ID` | Access Key do usuário ou role de deploy |
| `AWS_SECRET_ACCESS_KEY` | Secret Access Key do usuário ou role de deploy |
| `JWT_KEY` | Chave JWT da aplicação, com pelo menos 32 bytes |
| `SEED_ADMIN_PASSWORD` | Senha inicial do usuário administrador |
| `GEARUP_DATABASE_CONNECTION_STRING` | Connection string do PostgreSQL/RDS |
| `SONAR_TOKEN` | Token do SonarQube Cloud |

Exemplo de connection string:

```text
Host=<endpoint-rds>;Port=5432;Database=GearUp;Username=gearup;Password=<senha>
```

## Variáveis Fixas do Workflow

O workflow `cd-aws.yml` define:

```yaml
AWS_REGION: us-east-1
ECR_REPOSITORY: gearup
EKS_CLUSTER_NAME: gearup-dev
K8S_NAMESPACE: gearup
```

Se o ambiente mudar, esses valores devem ser ajustados no próprio workflow.

## Permissões Necessárias

O usuário ou role usado pela pipeline precisa conseguir:

- autenticar na AWS;
- enviar imagens ao ECR;
- atualizar o kubeconfig do EKS;
- consultar o cluster;
- aplicar manifests Kubernetes;
- criar ou atualizar Secrets, ConfigMaps, Services, HPA e Deployments no namespace `gearup`.

## Execução Manual

Para executar manualmente:

1. Acesse o repositório no GitHub;
2. Vá em `Actions`;
3. Selecione `CD AWS`;
4. Clique em `Run workflow`;
5. Escolha a branch desejada;
6. Execute o workflow.

## Resultado Esperado

Ao final da pipeline:

- a imagem Docker estará publicada no ECR com tag no formato `<commit-sha>-<run-number>`;
- o deployment `gearup-api` estará atualizado no EKS;
- o rollout estará concluído;
- as migrations pendentes terão sido aplicadas no startup da API;
- a API responderá nos endpoints de health check.
