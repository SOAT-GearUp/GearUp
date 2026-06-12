# Documentação ADR

## ADR-001 - Arquitetura Monolítica Modular utilizando DDD e Clean Architecture

**Título:** Arquitetura Monolítica Modular com DDD e Clean Architecture

**Data:** 11/06/2026

**Status:** Aceita

## Contexto

O sistema da oficina mecânica será desenvolvido inicialmente como um MVP para atender aos requisitos de cadastro de clientes, veículos, peças, estoque, orçamento e ordens de serviço.

Embora exista a possibilidade futura de crescimento da solução, o escopo inicial não apresenta volume de processamento, número de usuários ou requisitos de escalabilidade que justifiquem a adoção imediata de uma arquitetura distribuída baseada em microsserviços.

Além disso, o desafio acadêmico recomenda explicitamente a construção de um back-end monolítico para a primeira versão do sistema.

Ao mesmo tempo, deseja-se evitar forte acoplamento entre módulos para permitir evolução futura da arquitetura.

## Decisão

Será adotada uma arquitetura monolítica modular baseada nos princípios de Domain-Driven Design (DDD) e Clean Architecture.

A solução será organizada em camadas:

- Domain
- Application
- Infrastructure
- API

Os módulos de domínio serão separados por contexto de negócio, reduzindo acoplamento e facilitando futura extração para microsserviços caso necessário.

## Consequências

### Positivas

- Menor complexidade operacional.
- Facilidade de desenvolvimento e implantação.
- Menor custo de infraestrutura.
- Maior produtividade da equipe.
- Possibilidade futura de migração gradual para microsserviços.
- Melhor separação de responsabilidades através da Clean Architecture.

### Negativas

- Escalabilidade limitada ao processo único da aplicação.
- Deploy único para todos os módulos.
- Crescimento excessivo do monólito pode gerar aumento de acoplamento caso não seja controlado.

## ADR-002 - Utilização do SQL Server como Banco de Dados

**Título:** Utilização do SQL Server como banco de dados principal

**Data:** 11/06/2026

**Status:** Aceita

## Contexto

O sistema gerenciará informações relacionadas a:

- Clientes
- Veículos
- Ordens de Serviço
- Orçamentos
- Estoque
- Peças

Esses dados possuem forte relacionamento entre si e exigem consistência transacional para garantir a integridade das informações durante a execução dos processos da oficina.

O volume inicial de dados é considerado baixo a moderado e não há requisitos que indiquem necessidade de alta escalabilidade horizontal de leitura.

## Decisão

Será utilizado SQL Server como banco de dados principal da aplicação.

A persistência será implementada através do Entity Framework Core.

## Consequências

### Positivas

- Forte consistência transacional.
- Excelente suporte a relacionamentos complexos.
- Facilidade para consultas analíticas futuras.
- Integração madura com .NET.
- Ferramentas robustas de administração e monitoramento.

### Negativas

- Escalabilidade horizontal mais limitada que algumas soluções NoSQL.
- Maior custo de licenciamento em cenários corporativos.

## **ADR-003 - Autenticação e Autorização utilizando JWT**

**Título:** Utilização de JWT para autenticação das APIs

**Data:** 11/06/2026

**Status:** Aceita

## Contexto

O projeto exige autenticação para APIs administrativas.

A aplicação será consumida por clientes HTTP e futuramente poderá possuir aplicações web ou mobile.

É necessário um mecanismo simples, amplamente adotado e compatível com APIs REST.

## Decisão

Será utilizado JSON Web Token (JWT) para autenticação e autorização.

Após autenticação, o usuário receberá um token assinado digitalmente contendo suas informações e permissões.

## Consequências

### Positivas

- Stateless.
- Boa escalabilidade.
- Compatibilidade com aplicações web e mobile.
- Amplamente suportado pelo ecossistema .NET.

### Negativas

- Revogação de tokens exige estratégia complementar.
- Tokens comprometidos permanecem válidos até expirarem.

## ADR-004 - Modelagem dos Agregados do Domínio

**Título:** Modelagem de agregados baseada no domínio da oficina

**Status:** Aceita

## Contexto

O sistema possui forte dependência do fluxo de execução de Ordens de Serviço, sendo este o principal processo de negócio.

Era necessário definir limites transacionais que garantissem consistência sem gerar excesso de dependências entre entidades.

## Decisão

Os agregados serão modelados da seguinte forma:

- OrdemServico (Aggregate Root)
- Cliente (Aggregate Root)
- Estoque (Aggregate Root)

Veículo e Orçamento serão tratados como entidades pertencentes ao agregado OrdemServico.

## Consequências

### Positivas

- Limites transacionais claros.
- Menor acoplamento entre módulos.
- Melhor aderência aos princípios de DDD.
- Facilita futura separação em microsserviços.

### Negativas

- Consultas envolvendo múltiplos agregados exigirão mecanismos de leitura específicos.

## ADR-005 - Utilização de Use Cases na Camada de Aplicação

**Título:** Orquestração de regras de negócio através de Use Cases

**Data:** 11/06/2026

**Status:** Aceita

## Contexto

Os fluxos do sistema envolvem múltiplas operações, como:

- Criar Ordem de Serviço.
- Aprovar Orçamento.
- Atualizar Estoque.
- Finalizar Serviço.

Era necessário centralizar a orquestração sem acoplar controladores às regras de negócio.

## Decisão

A camada Application utilizará Use Cases para coordenar operações do domínio.

Exemplos:

- CriarOrdemServicoUseCase
- AprovarOrcamentoUseCase
- FinalizarOrdemServicoUseCase
- AtualizarEstoqueUseCase

## Consequências

### Positivas

- Separação clara de responsabilidades.
- Facilidade de testes unitários.
- Maior aderência à Clean Architecture.
- Regras de negócio independentes do framework.

### Negativas

- Aumento da quantidade de classes e abstrações.

## ADR-006 - Comunicação exclusivamente via API REST

**Título:** Utilização de APIs REST como interface de integração

**Status:** Aceita

## Contexto

O MVP exige que clientes e administradores possam consultar e manipular informações relacionadas às ordens de serviço, estoque e veículos.

Além disso, o projeto solicita APIs documentadas via Swagger.

## Decisão

A aplicação disponibilizará exclusivamente APIs REST utilizando HTTP e JSON.

A documentação será gerada automaticamente através do Swagger/OpenAPI.

## Consequências

### Positivas

- Padrão amplamente conhecido.
- Fácil integração com aplicações web e mobile.
- Baixa curva de aprendizado.
- Excelente suporte no ecossistema .NET.

### Negativas

- Excesso (Overfetching) ou falta de dados (Underfetching) retornados em algumas consultas
- Necessidade de versionamento futuro das APIs.

## ADR-007 - Estratégia de Testes Automatizados

**Título:** Utilização de testes unitários e integração automatizados

**Status:** Aceita

## Contexto

O projeto exige cobertura mínima de 80% nos domínios críticos.

Os fluxos de negócio possuem regras importantes, especialmente:

- Criação da OS.
- Aprovação de orçamento.
- Controle de estoque.
- Alteração de status.

## Decisão

Serão implementados:

- Testes Unitários para entidades, Value Objects e Use Cases.
- Testes de Integração para APIs e persistência.
- Pipeline automatizado para execução dos testes.

## Consequências

### Positivas

- Redução de regressões.
- Maior confiabilidade.
- Facilidade para evolução do sistema.
- Melhor qualidade do código.

### Negativas

- Maior tempo inicial de desenvolvimento.
- Necessidade de manutenção dos testes.

## ADR-008 - Containerização com Docker

**Título:** Padronização do ambiente utilizando Docker

**Status:** Aceita

## Contexto

O desafio exige Dockerfile e docker-compose.

Os desenvolvedores precisam executar a aplicação em ambientes consistentes.

## Decisão

A aplicação será distribuída através de containers Docker.

Serão fornecidos:

- Dockerfile da API.
- docker-compose para API e SQL Server.

## Consequências

### Positivas

- Ambiente padronizado.
- Facilidade de execução local.
- Menor dependência de configurações manuais.
- Preparação para implantação futura em cloud.

### Negativas

- Curva de aprendizado para membros sem experiência em Docker.
- Consumo adicional de recursos.

## **ADR-009 - Estratégia de Tratamento de Exceções**

**Título:** Tratamento centralizado de erros da aplicação

## Contexto

A API precisa fornecer respostas consistentes para erros de negócio, validação e falhas inesperadas.

## Decisão

Será implementado middleware global para tratamento de exceções.

As respostas seguirão um padrão único:

```
{
  "code":"ORCAMENTO_NAO_ENCONTRADO",
  "message":"Orçamento não encontrado."
}
```

## Consequências

### Positivas

- APIs mais previsíveis.
- Melhor experiência para consumidores.
- Facilidade de monitoramento.

### Negativas

- Necessidade de manutenção do catálogo de erros.

## ADR-010 - Estratégia de Auditoria e Histórico

**Título:** Auditoria e histórico de eventos

**Status:** Aceita

## Contexto

O sistema precisa manter histórico de:

- Mudança de status da OS.
- Aprovação de orçamento.
- Alterações de estoque.

A rastreabilidade é importante para evitar divergências operacionais.

## Decisão

As alterações relevantes serão registradas em histórico de eventos de negócio.

Exemplo:

```
OS Criada
OS Em Diagnóstico
Orçamento Gerado
Orçamento Aprovado
OS Em Execução
OS Finalizada
```

## Consequências

### Positivas

- Rastreabilidade.
- Facilidade de auditoria.
- Melhor suporte ao cliente.

### Negativas

- Aumento do volume de dados armazenados.