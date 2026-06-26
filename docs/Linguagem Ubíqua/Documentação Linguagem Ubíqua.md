# Documentação Linguagem Ubíqua

> **Projeto:** GearUp — Sistema de Gestão de Oficina Mecânica  
> **Fase:** FIAP SOAT — Fase 1  
> **Data de criação:** 11/06/2026  
> **Status:** Em evolução

---

## Sobre este documento

Este documento registra o dicionário de termos da **Linguagem Ubíqua** do projeto GearUp. Seu objetivo é garantir que desenvolvedores, stakeholders e Domain Experts compartilhem um vocabulário comum e sem ambiguidades, tanto nas conversas quanto no código-fonte.

Toda vez que um novo termo surgir nas interações com o negócio — ou quando um termo existente mudar de significado — este documento deve ser atualizado.

---

## Contextos Delimitados (Bounded Contexts)

O sistema GearUp foi organizado em quatro contextos delimitados, cada um com seu próprio vocabulário. Termos que aparecem em mais de um contexto possuem significado específico por contexto e estão identificados abaixo.

```
┌─────────────────────────┐   ┌─────────────────────────┐
│   ATENDIMENTO           │   │   DIAGNÓSTICO &          │
│                         │   │   ORÇAMENTO              │
│  Cliente                │   │                          │
│  Veículo                │   │  Diagnóstico             │
│  Ordem de Serviço       │   │  Orçamento               │
│  Solicitação Inicial    │   │  Item de Orçamento       │
│  Histórico do Cliente   │   │  Aprovação               │
└─────────────────────────┘   └─────────────────────────┘

┌─────────────────────────┐   ┌─────────────────────────┐
│   EXECUÇÃO              │   │   ESTOQUE                │
│                         │   │                          │
│  Mecânico               │   │  Peça                   │
│  Status da OS           │   │  Insumo                  │
│  Prioridade             │   │  Entrada de Estoque      │
│  Serviço                │   │  Saída de Estoque        │
│  Finalização            │   │  Controle de Estoque     │
└─────────────────────────┘   └─────────────────────────┘
```

---

## Glossário de Termos

### A

---

#### Aprovação

**Contexto:** Diagnóstico & Orçamento  
**Definição:** Ato formal pelo qual o **Cliente** aceita ou recusa um **Orçamento** gerado para a sua **Ordem de Serviço**. Sem aprovação, nenhum serviço de execução pode ser iniciado.  
**Status possíveis:** `Aprovado` | `Rejeitado`  
**Personas envolvidas:** Cliente, Atendente  
**Referência nos requisitos:** RF16  
**No código:** `AprovarOrcamentoUseCase`

> ⚠️ **Atenção — Termo Ambíguo:** "Aprovação" pode ser confundida com aprovação de acesso ao sistema (autenticação/autorização). Neste projeto, sempre que usada isoladamente no contexto de negócio, refere-se à aprovação de orçamento pelo cliente.

---

#### Atendente

**Contexto:** Atendimento  
**Definição:** Persona responsável por registrar clientes, abrir **Ordens de Serviço**, comunicar-se com o **Cliente** e validar **Orçamentos**. É o ponto de contato entre o cliente e a oficina.  
**Permissões de acesso:** Cadastro de clientes e veículos, criação e atualização de OS, consulta de orçamentos.  
**Referência nos requisitos:** RF03, RF04, RF07

---

#### Atualização de Status

**Contexto:** Execução  
**Definição:** Ação de alterar o **Status da OS** para refletir o momento atual do fluxo de trabalho da oficina. Cada mudança de status é registrada em **Histórico de Eventos**.  
**Referência nos requisitos:** RF08

---

#### Auxiliar

**Contexto:** Atendimento / Estoque  
**Definição:** Persona responsável pelo apoio operacional interno, incluindo controle de **Peças** e **Insumos** e suporte aos processos da oficina.  
**Referência nos requisitos:** RF03

---

### C

---

#### Cliente

**Contexto:** Atendimento  
**Definição:** Pessoa física ou jurídica que solicita serviços de manutenção ou reparo para um **Veículo** na oficina. O cliente é identificado por CPF ou CNPJ e pode acompanhar o andamento das suas **Ordens de Serviço** e aprovar **Orçamentos**.  
**Atributos-chave:** CPF/CNPJ, nome, contato, histórico de serviços  
**Referência nos requisitos:** RF04, RF06, RF16, RF21  
**Aggregate Root em:** `Cliente`

> ⚠️ **Atenção — Termo Ambíguo:** No contexto de **Autenticação**, "Cliente" também designa a aplicação consumidora da API (cliente HTTP). Sempre que o termo aparecer em discussões de negócio, refere-se à persona humana. Em discussões técnicas de integração, pode referir-se ao cliente HTTP.

---

#### Controle de Estoque

**Contexto:** Estoque  
**Definição:** Conjunto de operações que mantém o registro atualizado da quantidade de **Peças** e **Insumos** disponíveis na oficina. Inclui registrar **Entradas de Estoque**, registrar **Saídas de Estoque** e a atualização automática quando uma peça é utilizada em uma **Ordem de Serviço**.  
**Referência nos requisitos:** RF17, RF18  
**Aggregate Root em:** `Estoque`

---

### D

---

#### Diagnóstico

**Contexto:** Diagnóstico & Orçamento  
**Definição:** Avaliação técnica realizada pelo **Mecânico** sobre o **Veículo** para identificar problemas e definir os serviços necessários. O diagnóstico é o passo que precede a geração do **Orçamento**.  
**Personas envolvidas:** Mecânico  
**Referência nos requisitos:** RF11

---

### E

---

#### Entrada de Estoque

**Contexto:** Estoque  
**Definição:** Registro formal da chegada de **Peças** ou **Insumos** ao inventário da oficina, incrementando a quantidade disponível.  
**Referência nos requisitos:** RF17

> **Diferença com Saída de Estoque:** Entrada adiciona itens; Saída consome itens.

---

#### Estoque

**Contexto:** Estoque  
**Definição:** Inventário de **Peças** e **Insumos** disponíveis na oficina para utilização nas **Ordens de Serviço**.  
**Aggregate Root em:** `Estoque`  
**Referência nos requisitos:** RF17, RF18

---

### F

---

#### Finalização

**Contexto:** Execução  
**Definição:** Etapa em que os serviços de uma **Ordem de Serviço** são declarados concluídos pelo **Atendente**, encerrando a OS e preparando-a para **Entrega** ao **Cliente**.  
**Referência nos requisitos:** RF08  
**No código:** `FinalizarOrdemServicoUseCase`

---

### H

---

#### Histórico de Eventos

**Contexto:** Execução / Diagnóstico & Orçamento  
**Definição:** Registro cronológico das mudanças relevantes ocorridas em uma **Ordem de Serviço**, **Orçamento** ou **Estoque**. Garante rastreabilidade e permite auditoria futura.  
**Exemplos de eventos registrados:**

```
OS Criada
OS Em Diagnóstico
Orçamento Gerado
Orçamento Aprovado
OS Em Execução
OS Finalizada
```

**Referência nos requisitos:** RNF06

> **Diferença com Histórico do Cliente:** Histórico de Eventos refere-se ao log de mudanças de estado de uma OS específica. Histórico do Cliente refere-se ao conjunto de todas as OS e serviços já realizados para um determinado cliente.

---

#### Histórico do Cliente

**Contexto:** Atendimento  
**Definição:** Conjunto de todas as **Ordens de Serviço** e serviços realizados para um determinado **Cliente** e seus **Veículos**. Permite consultas sobre o relacionamento de longo prazo com a oficina.  
**Referência nos requisitos:** RF06, RF22

---

### I

---

#### Insumo

**Contexto:** Estoque  
**Definição:** Material consumível utilizado durante a execução de serviços na oficina (ex.: óleo, fluidos, filtros) que não é classificado como **Peça** de reposição estrutural. Compõe o **Estoque** e é registrado nos **Orçamentos**.  
**Referência nos requisitos:** RF14, RF17

> **Diferença com Peça:** Peças são componentes físicos instalados no veículo (ex.: pastilha de freio); insumos são materiais consumidos no processo (ex.: óleo do motor).

---

#### Item de Orçamento

**Contexto:** Diagnóstico & Orçamento  
**Definição:** Linha individual dentro de um **Orçamento**, representando uma **Peça**, um **Insumo** ou um **Serviço** específico com seu respectivo custo.  
**Referência nos requisitos:** RF13, RF14

---

### M

---

#### Mecânico

**Contexto:** Execução  
**Definição:** Persona técnica responsável por realizar o **Diagnóstico** dos veículos, executar os serviços definidos no **Orçamento** aprovado e atualizar o **Status da OS** durante a execução.  
**Referência nos requisitos:** RF03, RF11

---

### N

---

#### Notificação

**Contexto:** Comunicação  
**Definição:** Mensagem automática gerada pelo sistema para informar **Clientes** ou **Atendentes** sobre eventos relevantes em uma **Ordem de Serviço**.  
**Exemplos de gatilhos para o Cliente:**

- Orçamento disponível para aprovação
- Orçamento aprovado/rejeitado
- Serviço iniciado
- Serviço finalizado

**Exemplos de gatilhos para o Atendente:**

- Ações pendentes na OS

**Referência nos requisitos:** RF19, RF20  
**Aggregate Root em:** `Notificacao`

---

### O

---

#### Ordem de Serviço (OS)

**Contexto:** Atendimento / Execução  
**Definição:** Documento central do sistema que registra e coordena todo o ciclo de vida de um atendimento na oficina. Criada a partir de uma **Solicitação Inicial**, vincula **Cliente**, **Veículo**, **Diagnóstico**, **Orçamento** e **Serviços** executados.  
**Aggregate Root em:** `OrdemServico`  
**Referência nos requisitos:** RF07, RF08, RF09, RF10  
**No código:** `CriarOrdemServicoUseCase`, `FinalizarOrdemServicoUseCase`

---

#### Orçamento

**Contexto:** Diagnóstico & Orçamento  
**Definição:** Documento vinculado a uma **Ordem de Serviço** que detalha os custos de **Peças**, **Insumos** e **Serviços** necessários para a execução do reparo. Precisa ser aprovado pelo **Cliente** antes de qualquer execução.  
**Regras de negócio:**

- Múltiplos orçamentos podem existir em uma mesma OS (quando novos problemas são identificados)
- A OS só avança para execução após pelo menos um orçamento aprovado

**Referência nos requisitos:** RF12, RF13, RF14, RF15, RF16  
**No código:** `AprovarOrcamentoUseCase`, `AtualizarEstoqueUseCase`

---

### P

---

#### Peça

**Contexto:** Estoque / Diagnóstico & Orçamento  
**Definição:** Componente físico de reposição utilizado na manutenção ou reparo de um **Veículo** (ex.: pastilha de freio, correia dentada). Compõe o **Estoque** e pode ser listada como **Item de Orçamento**.  
**Referência nos requisitos:** RF14, RF17, RF18

---

#### Perfil de Acesso

**Contexto:** Autenticação  
**Definição:** Conjunto de permissões associado a um usuário do sistema que determina quais funcionalidades ele pode acessar e executar. Os perfis existentes são: `Atendente`, `Auxiliar`, `Mecânico` e `Cliente`.  
**Referência nos requisitos:** RF03

---

#### Prioridade

**Contexto:** Execução  
**Definição:** Critério que define a ordem de atendimento entre as **Ordens de Serviço** ativas. Pode ser baseada em urgência, prazo acordado ou ordem de chegada.  
**Referência nos requisitos:** RF09

---

### S

---

#### Saída de Estoque

**Contexto:** Estoque  
**Definição:** Registro formal da retirada de **Peças** ou **Insumos** do inventário da oficina, reduzindo a quantidade disponível. Pode ser disparada automaticamente quando peças são utilizadas em uma **Ordem de Serviço**.  
**Referência nos requisitos:** RF17, RF18

---

#### Serviço

**Contexto:** Execução / Diagnóstico & Orçamento  
**Definição:** Trabalho técnico realizado pelo **Mecânico** no **Veículo** (ex.: troca de óleo, alinhamento, revisão). Pode ser listado como **Item de Orçamento** e representa o custo de mão de obra.

> **Diferença com Serviço (contexto de TI):** Em discussões técnicas, "serviço" pode referir-se a um microsserviço ou serviço de aplicação. No contexto de negócio, sempre se refere ao trabalho mecânico executado no veículo.

---

#### Solicitação Inicial

**Contexto:** Atendimento  
**Definição:** Descrição do problema ou necessidade informada pelo **Cliente** no momento da abertura da **Ordem de Serviço**. Representa o ponto de partida para o **Diagnóstico**.  
**Referência nos requisitos:** RF07

---

#### Status da OS

**Contexto:** Execução  
**Definição:** Estado atual de uma **Ordem de Serviço** dentro do fluxo de atendimento da oficina. O status evolui sequencialmente conforme o andamento do trabalho.

**Ciclo de vida dos status:**

```
Recebida
    ↓
Em Diagnóstico
    ↓
Aguardando Orçamento
    ↓
Aguardando Aprovação ──→ Cancelada
    ↓
Aguardando Peças e Insumos
    ↓
Aguardando Execução
    ↓
Em Execução
    ↓
Finalizada
    ↓
Entregue
```

**Referência nos requisitos:** RF08  
**No código:** `AtualizarStatusUseCase`

---

### U

---

#### Usuário

**Contexto:** Autenticação  
**Definição:** Entidade do sistema que representa qualquer persona com acesso à plataforma (Atendente, Auxiliar, Mecânico ou Cliente). Identificado por credenciais (usuário e senha) e associado a um **Perfil de Acesso**.  
**Referência nos requisitos:** RF01, RF02, RF03

---

### V

---

#### Veículo

**Contexto:** Atendimento  
**Definição:** Automóvel pertencente a um **Cliente** que é levado à oficina para manutenção ou reparo. Um cliente pode possuir múltiplos veículos, e cada veículo possui seu próprio histórico de serviços.  
**Atributos-chave:** Placa, modelo, marca, ano, CPF/CNPJ do proprietário  
**Referência nos requisitos:** RF05, RF06

> **Nota de modelagem (ADR-004):** Veículo é um Aggregate Root no bounded context **Cadastro**, referenciado por `OrdemServico` via identificador.

---

## Termos Ambíguos e Sinônimos — Mapa de Desambiguação

Esta seção consolida todos os termos que possuem duplo significado ou que são utilizados como sinônimos no dia a dia, mas que devem ser tratados com precisão no sistema.

|Termo usado informalmente|Significado no contexto de negócio|Significado no contexto técnico|Regra de uso|
|---|---|---|---|
|**Cliente**|Pessoa que leva o carro à oficina|Aplicação que consome a API|Em reuniões de negócio: sempre a persona. Em contextos técnicos de integração: especificar "cliente HTTP" ou "aplicação cliente".|
|**Aprovação**|Cliente aprova o orçamento|N/A|Sempre se refere à aprovação de orçamento quando em contexto de negócio.|
|**Serviço**|Trabalho mecânico no veículo|Microsserviço / serviço de aplicação|Qualificar quando necessário: "serviço mecânico" vs. "serviço de aplicação".|
|**Status**|Estado atual da OS no fluxo|Estado HTTP da requisição|Usar "Status da OS" para negócio; "código de status HTTP" para contexto técnico.|
|**Histórico**|Histórico do cliente / Histórico de eventos|Log de sistema|Qualificar: "Histórico do Cliente", "Histórico de Eventos da OS".|
|**OS**|Abreviação de Ordem de Serviço|N/A|Aceito como sinônimo de Ordem de Serviço em todos os contextos.|
|**Login**|Ato de autenticar no sistema|Identificador do usuário (username)|Preferir "autenticação" para o ato; "nome de usuário" para o identificador.|

---

## Mapa da Jornada e Linguagem por Etapa

A tabela abaixo conecta cada etapa da Jornada da Solução com os termos ubíquos correspondentes, as personas envolvidas e os Use Cases que orquestram cada momento.

|#|Etapa|Termos-chave|Persona|Use Case|
|---|---|---|---|---|
|01|Solicitação do serviço|Cliente, Solicitação Inicial|Cliente|—|
|02|Cadastro do cliente e veículo|Cliente, Veículo, CPF/CNPJ|Atendente|`CadastrarClienteUseCase`|
|03|Criação da OS|Ordem de Serviço, Status (Recebida)|Atendente|`CriarOrdemServicoUseCase`|
|04|Necessita diagnóstico?|Diagnóstico, Status (Em Diagnóstico)|Sistema / Atendente|`AtualizarStatusUseCase`|
|05|Diagnóstico|Diagnóstico, Mecânico|Mecânico|`RegistrarDiagnosticoUseCase`|
|06|Geração do orçamento|Orçamento, Item de Orçamento, Peça, Insumo, Serviço|Sistema|`GerarOrcamentoUseCase`|
|07|Aprovação|Aprovação, Orçamento, Status (Aguardando Aprovação)|Cliente|`AprovarOrcamentoUseCase`|
|08|Execução|Serviço, Mecânico, Saída de Estoque, Status (Em Execução)|Mecânico|`AtualizarEstoqueUseCase`|
|09|Finalização|Finalização, Status (Finalizada)|Atendente|`FinalizarOrdemServicoUseCase`|
|10|Entrega|Entrega, Histórico do Cliente, Status (Entregue)|Cliente|—|

---

## Convenções de Nomenclatura para o Código

Os termos da linguagem ubíqua devem se refletir diretamente no código-fonte. As convenções abaixo garantem consistência entre o vocabulário do negócio e o vocabulário técnico.

|Termo de negócio|Classe / Entidade|Use Case|Repositório|
|---|---|---|---|
|Ordem de Serviço|`OrdemServico`|`CriarOrdemServicoUseCase`|`IOrdemServicoRepository`|
|Orçamento|`Orcamento`|`AprovarOrcamentoUseCase`|`IOrcamentoRepository`|
|Cliente|`Cliente`|`CadastrarClienteUseCase`|`IClienteRepository`|
|Veículo|`Veiculo`|—|`IVeiculoRepository`|
|Estoque|`Estoque`|`AtualizarEstoqueUseCase`|`IEstoqueRepository`|
|Item de Orçamento|`ItemOrcamento`|—|—|
|Status da OS|`StatusOrdemServico` (enum)|`AtualizarStatusUseCase`|—|
|Histórico de Eventos|`HistoricoEvento`|—|`IHistoricoEventoRepository`|
|Notificação|`Notificacao`|`ListarNotificacaoUseCase`|`INotificacaoRepository`|

---

## Registro de Mudanças

|Data|Versão|Mudança|Responsável|
|---|---|---|---|
|11/06/2026|1.0|Criação inicial do documento com termos extraídos dos documentos de requisitos e ADRs|Vitor Onofre Ramos|

---

> **Lembrete:** Este documento é um artefato vivo. Toda conversa com o negócio que introduza, refine ou deprecie um termo deve resultar em uma atualização aqui, com data e versão registradas na tabela acima.