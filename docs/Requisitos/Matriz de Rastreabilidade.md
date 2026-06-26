# Matriz de Rastreabilidade

Data da revisão: 17/06/2026.

| Requisito | Situação | Evidência principal |
|---|---|---|
| RF01-RF03 | Implementado | JWT, login/logout, perfis e policies nos controllers |
| RF04 | Implementado | `ClientesController`, exclusão lógica e `Cliente` |
| RF05-RF06 | Implementado | veículos vinculados ao cliente e consultas de OS por cliente/veículo |
| RF07-RF10 | Implementado | agregado `OrdemServico`, prioridade, status e consultas |
| RF11 | Implementado | início e registro de diagnóstico pelo mecânico |
| RF12-RF16 | Implementado | orçamento versionado, itens mutáveis enquanto pendente e decisão |
| RF17-RF18 | Implementado | movimentações auditáveis e baixa automática ao iniciar execução |
| RF19-RF20 | Implementado | notificações persistidas para cliente e atendente |
| RF21-RF22 | Implementado | consulta restrita por `cliente_id` e histórico cronológico da OS |
| RNF01 | Meta operacional | requer monitoramento e SLA do ambiente de produção |
| RNF02 | Meta operacional | requer teste de carga e telemetria p95 no ambiente alvo |
| RNF03 | Implementado | PBKDF2-SHA256, salt aleatório e 210 mil iterações |
| RNF04 | Implementado na aplicação | redirecionamento HTTPS; terminação TLS depende do ambiente |
| RNF05 | Parcial | exclusão lógica e controle de acesso; anonimização/retencão dependem de política LGPD |
| RNF06 | Implementado | histórico de OS/orçamento e movimentações de estoque |
| RNF07 | Meta validável | arquitetura stateless; capacidade deve ser confirmada por teste de carga |

## Decisão de modelagem

Conforme ADR-004 (revisão 24/06/2026) e o Event Storming, os agregados são:

- **Cadastro:** `Cliente`, `Veiculo`
- **Ordem de Serviço:** `OrdemServico`, `Orcamento`
- **Estoque:** `Estoque`
- **Comunicação:** `Notificacao`

`OrdemServico` referencia `Cliente` e `Veiculo` por identificador; `Orcamento` referencia `OrdemServico` por identificador. A orquestração entre agregados ocorre na camada Application.

## Qualidade verificada

- 90,42% de cobertura de linhas no projeto `GearUp.Domain`.
- Pipeline bloqueia cobertura de domínio inferior a 80%.
- Build da solution e da imagem Docker sem avisos ou erros.
- Scan NuGet sem vulnerabilidades conhecidas na data da revisão.
