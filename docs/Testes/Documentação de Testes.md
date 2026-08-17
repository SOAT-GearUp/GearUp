# Testes e cobertura

Para executar os testes e conferir a cobertura deste projeto, use os comandos
abaixo na pasta **`GearUp/`** (onde está `GearUp.slnx`).

## Rodar a suíte

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

## Ver cobertura no terminal

Exibe uma **tabela por assembly** ao final da execução — forma mais rápida de
ver o percentual:

```powershell
dotnet test GearUp.slnx /p:CollectCoverage=true /p:CoverletOutputFormat=opencover --tl:off
```

O `--tl:off` evita que os logs do build cubram a tabela de cobertura.

## Cobertura em arquivo (como no CI)

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

## Critério do CI

O workflow em `.github/workflows/ci.yml` exige **pelo menos 80% de cobertura
de linhas** em `GearUp.Domain`; abaixo disso o build falha. A cobertura pode
ser conferida pela tabela do Coverlet ou pelo `line-rate` no XML gerado por
`GearUp.Domain.UnitTests`.

## Análise de Vulnerabilidades

[![SonarQube Cloud](https://sonarcloud.io/images/project_badges/sonarcloud-light.svg)](https://sonarcloud.io/summary/new_code?id=SOAT-GearUp_GearUp)

Foi realizada uma análise estática do código utilizando o SonarCloud. O scan avaliou aspectos de segurança, confiabilidade, manutenibilidade e cobertura de testes do projeto.

Durante a análise, foram identificados pontos de segurança e qualidade que foram tratados no código. Após as correções, o projeto apresentou evolução nos indicadores do SonarCloud, incluindo redução de **Security Issues**, melhoria do **Security Rating** e acompanhamento da evolução de cobertura e code smells.

Links relacionados:

| Item | Como acessar |
|---|---|
| Dashboard no SonarCloud | [SOAT-GearUp / GearUp](https://sonarcloud.io/summary/new_code?id=SOAT-GearUp_GearUp) |
| Relatório de análise de vulnerabilidades | [docs/Relatorios/Analise de Vulnerabilidades/Relatorio de Analise de Vulnerabilidades.md](../Relatorios/Analise%20de%20Vulnerabilidades/Relatorio%20de%20Analise%20de%20Vulnerabilidades.md) |
| Gráficos utilizados no relatório | [docs/Relatorios/Analise de Vulnerabilidades/imagens](../Relatorios/Analise%20de%20Vulnerabilidades/imagens) |
