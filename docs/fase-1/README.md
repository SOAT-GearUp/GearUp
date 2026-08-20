# GearUp - Fase 1

API REST para gestão de oficina mecânica, construída em .NET 10, PostgreSQL,
DDD e Clean Architecture.

## Documentação (DDD)

Artefatos de descoberta, modelagem e decisões arquiteturais desta entrega.

### Descoberta e modelagem

| Artefato | Como acessar |
|---|---|
| **Event Storming** | [Quadro no Miro](https://miro.com/app/board/uXjVHaE40W8=/?share_link_id=725212077212) |
| **Storytelling** | Abra [egon.io](https://egon.io/app/), clique em **Import** e selecione o arquivo [`Storytelling/GearUp - Storytelling da Oficina Mecânica.egn`](Storytelling/GearUp%20-%20Storytelling%20da%20Oficina%20Mec%C3%A2nica.egn) |

O arquivo `.egn` é o mapa narrativo do fluxo da oficina (storytelling). O
[egon.io](https://egon.io/app/) é a ferramenta online para visualizá-lo — não
abre direto no navegador; é preciso importar o arquivo no site.

### Artefatos escritos

| Documento | Descrição |
|---|---|
| [ADRs](ADR/Documentação%20ADR.md) | Registro de decisões arquiteturais (monólito modular, DDD, Clean Architecture, etc.) |
| [Linguagem Ubíqua](Linguagem%20Ubíqua/Documentação%20Linguagem%20Ubíqua.md) | Glossário, bounded contexts e termos do domínio |
| [Requisitos](Requisitos/Documentação%20de%20Requisitos.md) | Personas, problema, requisitos funcionais e não funcionais |
| [Matriz de Rastreabilidade](Requisitos/Matriz%20de%20Rastreabilidade.md) | Rastreio requisito → implementação no código |
| [Fase 2](../fase-2/README.md) | Documentação da evolução para Docker, Kubernetes, Terraform, pipeline CI/CD e deploy AWS |

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
  Comunicação — ver [ADR-004](ADR/Documentação%20ADR.md#adr-004---modelagem-dos-agregados-do-domínio).
- **Linguagem ubíqua e vocabulário do código:** Atendimento, Diagnóstico &
  Orçamento, Execução, Estoque — ver
  [Linguagem Ubíqua](Linguagem%20Ubíqua/Documentação%20Linguagem%20Ubíqua.md).

O contexto **Ordem de Serviço** (Event Storming) concentra o fluxo central da
oficina; a Linguagem Ubíqua o subdivide em Atendimento, Diagnóstico & Orçamento
e Execução para reduzir ambiguidade de termos. Detalhes e classificação dos
subdomínios estão na [ADR-004](ADR/Documentação%20ADR.md#adr-004---modelagem-dos-agregados-do-domínio).

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
| Relatório de análise de vulnerabilidades | [Relatorios/Analise de Vulnerabilidades/Relatorio de Analise de Vulnerabilidades.md](Relatorios/Analise%20de%20Vulnerabilidades/Relatorio%20de%20Analise%20de%20Vulnerabilidades.md) |
| Gráficos utilizados no relatório | [Relatorios/Analise de Vulnerabilidades/imagens](Relatorios/Analise%20de%20Vulnerabilidades/imagens) |

## Camadas

- `GearUp.Domain`: agregados, entidades, value objects e invariantes.
- `GearUp.Application`: use cases e contratos de persistência/serviços.
- `GearUp.Infrastructure`: EF Core, PostgreSQL, JWT e implementações.
- `GearUp.Api`: autenticação, autorização e contratos HTTP.

O fluxo de dependências é `Api -> Application/Infrastructure`,
`Infrastructure -> Application/Domain` e `Application -> Domain`.

## Testes de Integração

A pasta **`Postman/`** contém seis coleções do Postman utilizadas para os testes de integração da API.
Importe todos os arquivos .json dessa pasta no Postman e selecione o `environment`**`Test`**. Em seguida, execute as coleções na sequência numérica definida na nomenclatura (1 - ..., 2 - ..., 3 - ..., etc.), 
respeitando a ordem de execução devido às dependências entre os testes.

## Vídeo de apresentação

Apresentação do projeto GearUp: [https://youtu.be/4VtSqLqZg3I](https://youtu.be/4VtSqLqZg3I)
