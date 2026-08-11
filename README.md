# GearUp

API REST para gestão de oficina mecânica, construída em .NET 10, PostgreSQL,
DDD e Clean Architecture.

## Documentação (DDD)

Artefatos de descoberta, modelagem e decisões arquiteturais desta entrega.

### Descoberta e modelagem

| Artefato | Como acessar |
|---|---|
| **Event Storming** | [Quadro no Miro](https://miro.com/app/board/uXjVHaE40W8=/?share_link_id=725212077212) |
| **Storytelling** | Abra [egon.io](https://egon.io/app/), clique em **Import** e selecione o arquivo [`docs/Storytelling/GearUp - Storytelling da Oficina Mecânica.egn`](docs/Storytelling/GearUp%20-%20Storytelling%20da%20Oficina%20Mec%C3%A2nica.egn) |

O arquivo `.egn` é o mapa narrativo do fluxo da oficina (storytelling). O
[egon.io](https://egon.io/app/) é a ferramenta online para visualizá-lo — não
abre direto no navegador; é preciso importar o arquivo no site.

### Artefatos escritos

| Documento | Descrição |
|---|---|
| [ADRs](docs/ADR/Documentação%20ADR.md) | Registro de decisões arquiteturais (monólito modular, DDD, Clean Architecture, etc.) |
| [Linguagem Ubíqua](docs/Linguagem%20Ubíqua/Documentação%20Linguagem%20Ubíqua.md) | Glossário, bounded contexts e termos do domínio |
| [Requisitos](docs/Requisitos/Documentação%20de%20Requisitos.md) | Personas, problema, requisitos funcionais e não funcionais |
| [Matriz de Rastreabilidade](docs/Requisitos/Matriz%20de%20Rastreabilidade.md) | Rastreio requisito → implementação no código |

### Domínio, subdomínios e bounded contexts

O **domínio** do GearUp é a gestão de oficina mecânica: cadastro de clientes e
veículos, ordens de serviço, diagnóstico, orçamento, execução e estoque.

Em DDD, **subdomínio** e **bounded context** são conceitos distintos:

| Conceito | Espaço | Pergunta que responde |
|---|---|---|
| **Subdomínio** | Problema (negócio) | *Que parte do negócio estamos modelando?* |
| **Bounded context** | Solução (software) | *Onde este modelo e vocabulário valem no código?* |

Um bounded context **não é sinônimo** de subdomínio: ele **implementa** (total ou
parcialmente) um subdomínio, com linguagem ubíqua e limites transacionais
próprios. Um subdomínio pode ser dividido em mais de um bounded context; um
bounded context pode atender mais de um subdomínio — embora, neste projeto, a
relação seja em geral próxima de 1:1.

O GearUp usa **duas nomenclaturas** de bounded context, conforme o artefato de
origem:

| Subdomínio | Tipo | Event Storming | Linguagem Ubíqua | Onde no código |
|---|---|---|---|---|
| Atendimento e cadastro | Supporting | Cadastro | Atendimento | `Cadastro/`, `OrdemDeServico/Ordens/` |
| Diagnóstico e orçamentação | Core | Ordem de Serviço | Diagnóstico & Orçamento | `OrdemDeServico/Diagnosticos/`, `OrdemDeServico/Orcamentos/` |
| Execução de serviços | Core | Ordem de Serviço | Execução | `OrdemDeServico/Execucao/` |
| Controle de estoque | Supporting | Estoque | Estoque | `Estoque/` |
| Comunicação com cliente | Generic | Comunicação | — | `Comunicacao/` |
| Autenticação e autorização | Generic | — (fora do workshop) | — | `Autenticacao/` |

**Referências por nomenclatura:**

- **Event Storming e agregados:** Cadastro, Ordem de Serviço, Estoque,
  Comunicação — ver [ADR-004](docs/ADR/Documentação%20ADR.md#adr-004---modelagem-dos-agregados-do-domínio).
- **Linguagem ubíqua e vocabulário do código:** Atendimento, Diagnóstico &
  Orçamento, Execução, Estoque — ver
  [Linguagem Ubíqua](docs/Linguagem%20Ubíqua/Documentação%20Linguagem%20Ubíqua.md).

O contexto **Ordem de Serviço** (Event Storming) concentra o fluxo central da
oficina; a Linguagem Ubíqua o subdivide em Atendimento, Diagnóstico & Orçamento
e Execução para reduzir ambiguidade de termos. Detalhes e classificação dos
subdomínios estão na [ADR-004](docs/ADR/Documentação%20ADR.md#adr-004---modelagem-dos-agregados-do-domínio).

## Como executar o projeto

### Pre-requisitos

- Docker Desktop
- .NET SDK 10, apenas se for executar a API fora do Docker
- Terminal de sua preferencia, como PowerShell, Bash ou zsh

### Configurar variaveis de ambiente

Crie o arquivo `.env` a partir do exemplo:

```powershell
Copy-Item .env.example .env
```

No Linux/macOS, o comando equivalente e:

```bash
cp .env.example .env
```

Depois preencha `JWT_KEY` no arquivo `.env`. Essa chave deve ter pelo menos 32
caracteres.

Para desenvolvimento local, se nao quiser gerar uma chave, use esta chave
publica e conhecida:

```env
JWT_KEY=gearup-local-development-key-change-me
```

Essa chave e apenas para ambiente local. Nao use em homologacao, producao ou
qualquer ambiente compartilhado.

Se preferir gerar uma chave propria, use qualquer gerador seguro de senha/token.
Exemplos:

```bash
openssl rand -hex 32
```

ou, em PowerShell:

```powershell
[guid]::NewGuid().ToString("N") + [guid]::NewGuid().ToString("N")
```

O `.env` deve ficar com valores parecidos com estes:

```env
POSTGRES_PASSWORD=Your_strong!Pass123
JWT_KEY=gearup-local-development-key-change-me
SEED_ADMIN_USER=admin
SEED_ADMIN_PASSWORD=GearUp@123
```

Nao versionar o arquivo .env, pois ele contem segredos locais.

### Executar com Docker

Na pasta raiz do projeto, onde esta o arquivo `docker-compose.yml`, execute:

```powershell
docker compose up --build
```

A API estara em `http://localhost:8080` e o Swagger em
`http://localhost:8080/swagger`. O banco e as migrations sao inicializados
automaticamente.

As imagens .NET do Dockerfile vem do Docker Hub (`bitnami/dotnet-sdk` e
`bitnami/aspnet-core`), para o build funcionar sem depender do registry da
Microsoft (`mcr.microsoft.com`).

A API expõe dois endpoints de health check, usados pelo `HEALTHCHECK` do
Dockerfile e pelas probes do Kubernetes (veja seção
[Kubernetes](#kubernetes)):

| Endpoint | Verifica | Uso |
|---|---|---|
| `GET /health/live` | Processo no ar (sem checar dependências) | Liveness |
| `GET /health/ready` | Conexão com o PostgreSQL | Readiness |

Para parar os containers:

```powershell
docker compose down
```

Para remover tambem o volume do banco local:

```powershell
docker compose down -v
```

### Executar localmente sem Docker para a API

Se preferir rodar a API pelo `dotnet run`, mantenha pelo menos o PostgreSQL em
execucao pelo Docker:

```powershell
docker compose up postgres
```

Em outro terminal, configure as variaveis esperadas pela API:

```powershell
$env:ASPNETCORE_ENVIRONMENT="Development"
$env:ConnectionStrings__GearUpDatabase="Host=localhost;Port=5433;Database=GearUp;Username=gearup;Password=Your_strong!Pass123"
$env:Jwt__Key="cole_a_chave_gerada_aqui"
$env:Seed__AdminUser="admin"
$env:Seed__AdminPassword="GearUp@123"
dotnet run --project src/GearUp.Api/GearUp.Api.csproj
```

Nesse modo, a API usara a porta configurada pelo perfil local do projeto.

### Debug pelo Visual Studio

Para executar pelo botao **Play** do Visual Studio, o arquivo
`src/GearUp.Api/Properties/launchSettings.json` deve manter apenas configuracoes
locais nao sensiveis, como:

```json
"environmentVariables": {
  "ASPNETCORE_ENVIRONMENT": "Development"
}
```

As configuracoes sensiveis devem ficar no **User Secrets** da sua maquina.
Execute uma vez:

```powershell
dotnet user-secrets set "ConnectionStrings:GearUpDatabase" "Host=localhost;Port=5433;Database=GearUp;Username=gearup;Password=Your_strong!Pass123" --project src/GearUp.Api/GearUp.Api.csproj
dotnet user-secrets set "Jwt:Key" "gearup-local-development-key-change-me" --project src/GearUp.Api/GearUp.Api.csproj
dotnet user-secrets set "Seed:AdminUser" "admin" --project src/GearUp.Api/GearUp.Api.csproj
dotnet user-secrets set "Seed:AdminPassword" "GearUp@123" --project src/GearUp.Api/GearUp.Api.csproj
```

Antes de clicar em **Play**, mantenha o PostgreSQL local rodando:

```powershell
docker compose up postgres
```

Depois selecione o profile `http` ou `https` no Visual Studio e execute a API.
O Swagger ficara disponivel em `/swagger`.

### Usuario inicial

No primeiro boot, se a tabela de usuarios estiver vazia, o sistema cria um
unico usuario **admin** com as credenciais do `.env`:

| Variavel | Descricao |
|---|---|
| `SEED_ADMIN_USER` | Nome de usuario do admin inicial |
| `SEED_ADMIN_PASSWORD` | Senha do admin inicial |

Com o admin logado, use `POST /api/usuarios` para cadastrar os demais perfis
(`Atendente`, `Auxiliar`, `Mecanico`, `Cliente`). O **Atendente** pode criar
apenas usuarios do tipo **Cliente**; o **Admin** pode criar qualquer perfil.

Altere a senha padrao do admin antes de publicar em producao.

## Testes e cobertura

Para executar os testes e conferir a cobertura deste projeto, use os comandos
abaixo na pasta **`GearUp/`** (onde está `GearUp.slnx`).

### Rodar a suíte

```powershell
dotnet test GearUp.slnx
```

Projetos de teste:

| Projeto | O que valida |
|---|---|
| `tests/GearUp.Domain.UnitTests` | Regras de domínio, invariantes e agregados |
| `tests/GearUp.Application.UnitTests` | Casos de uso da camada de aplicação |
| `tests/GearUp.Api.IntegrationTests` | Endpoints HTTP ponta a ponta |

Para um projeto só:

```powershell
dotnet test tests/GearUp.Domain.UnitTests
```

### Ver cobertura no terminal

Exibe uma **tabela por assembly** ao final da execução — forma mais rápida de
ver o percentual:

```powershell
dotnet test GearUp.slnx /p:CollectCoverage=true /p:CoverletOutputFormat=opencover --tl:off
```

O `--tl:off` evita que os logs do build cubram a tabela de cobertura.

### Cobertura em arquivo (como no CI)

Gera XML em `TestResults/` para ferramentas externas ou para conferir o mesmo
formato do pipeline:

```powershell
dotnet test GearUp.slnx --collect:"XPlat Code Coverage"
```

Apenas domínio (escopo verificado no CI):

```powershell
dotnet test tests/GearUp.Domain.UnitTests --collect:"XPlat Code Coverage"
```

O relatório fica em algo como
`TestResults/<guid>/coverage.cobertura.xml`. Abra o XML e procure
`line-rate` no elemento raiz `<coverage>` — o valor é a fração de linhas
cobertas (ex.: `0.85` = 85%).

### Critério do CI

O workflow em `.github/workflows/ci.yml` exige **pelo menos 80% de cobertura
de linhas** em `GearUp.Domain`; abaixo disso o build falha. A cobertura pode
ser conferida pela tabela do Coverlet ou pelo `line-rate` no XML gerado por
`GearUp.Domain.UnitTests`.

### Análise de Vulnerabilidades

[![SonarQube Cloud](https://sonarcloud.io/images/project_badges/sonarcloud-light.svg)](https://sonarcloud.io/summary/new_code?id=SOAT-GearUp_GearUp)

Foi realizada uma análise estática do código utilizando o SonarCloud. O scan avaliou aspectos de segurança, confiabilidade, manutenibilidade e cobertura de testes do projeto.

Durante a análise, foram identificados pontos de segurança e qualidade que foram tratados no código. Após as correções, o projeto apresentou evolução nos indicadores do SonarCloud, incluindo redução de **Security Issues**, melhoria do **Security Rating** e acompanhamento da evolução de cobertura e code smells.

Links relacionados:

| Item | Como acessar |
|---|---|
| Dashboard no SonarCloud | [SOAT-GearUp / GearUp](https://sonarcloud.io/summary/new_code?id=SOAT-GearUp_GearUp) |
| Relatório de análise de vulnerabilidades | [docs/Relatorios/Analise de Vulnerabilidades/Relatorio de Analise de Vulnerabilidades.md](docs/Relatorios/Analise%20de%20Vulnerabilidades/Relatorio%20de%20Analise%20de%20Vulnerabilidades.md) |
| Gráficos utilizados no relatório | [docs/Relatorios/Analise de Vulnerabilidades/imagens](docs/Relatorios/Analise%20de%20Vulnerabilidades/imagens) |

## Camadas

- `GearUp.Domain`: agregados, entidades, value objects e invariantes.
- `GearUp.Application`: use cases e contratos de persistência/serviços.
- `GearUp.Infrastructure`: EF Core, PostgreSQL, JWT e implementações.
- `GearUp.Api`: autenticação, autorização e contratos HTTP.

O fluxo de dependências é `Api -> Application/Infrastructure`,
`Infrastructure -> Application/Domain` e `Application -> Domain`.

## Kubernetes

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

### Local com Minikube

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

### Produção

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

Fora do escopo deste MVP: observabilidade (Prometheus/Grafana, logs
centralizados), GitOps (ArgoCD/Flux), `NetworkPolicy`, `PodDisruptionBudget`,
TLS automático no Ingress (cert-manager) e rotação de segredos.

## Testes de Integração

A pasta **`Docs/Postman/`** contém seis coleções do Postman utilizadas para os testes de integração da API.
Importe todos os arquivos .json dessa pasta no Postman e selecione o `environment`**`Test`**. Em seguida, execute as coleções na sequência numérica definida na nomenclatura (1 - ..., 2 - ..., 3 - ..., etc.), 
respeitando a ordem de execução devido às dependências entre os testes.

## Vídeo de apresentação

Apresentação do projeto GearUp: [https://youtu.be/4VtSqLqZg3I](https://youtu.be/4VtSqLqZg3I)
