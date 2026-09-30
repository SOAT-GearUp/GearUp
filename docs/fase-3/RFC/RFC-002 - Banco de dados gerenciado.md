# RFC-002 — Banco de dados gerenciado

| Campo | Valor |
|---|---|
| Status | Aceita |
| Data | 2026-09-29 |
| Relacionada | [Modelagem e justificativa do banco](../Banco%20de%20Dados/Modelagem%20e%20Justificativa.md) |

## Problema

Até a Fase 2 o PostgreSQL rodava como StatefulSet dentro do cluster (ou num RDS criado junto do EKS, no mesmo Terraform). A Fase 3 pede um **banco gerenciado com repositório e pipeline próprios**, com modelagem documentada e justificada.

## Opções avaliadas

| Opção | Consistência | Encaixe no modelo | Custo no lab | Observação |
|---|---|---|---|---|
| **RDS PostgreSQL** | ACID, FKs, check constraints | total: schema e migrations já existem (EF Core + Npgsql) | db.t3.micro ~US$ 0,018/h | backups e patches gerenciados |
| Aurora PostgreSQL Serverless v2 | ACID | total | mínimo 0,5 ACU ~US$ 0,06/h | 3× mais caro; escala desnecessária |
| RDS MySQL / SQL Server | ACID | exigiria trocar o provider e revalidar tipos (`uuid`, `timestamptz`) | similar / SQL Server com licença | sem ganho funcional |
| DynamoDB | eventual/transações limitadas | ruim: o domínio é relacional (OS → orçamento → itens → estoque) | pay-per-request | perderíamos joins, FKs e as migrations |
| PostgreSQL no cluster (StatefulSet) | ACID | total | só EBS | não é "gerenciado": backup, patch e failover ficam conosco |

## Proposta

**Amazon RDS for PostgreSQL 17**, provisionado pelo repositório `gearup-infra-db`:

- `db.t3.micro`, 20 GB gp3 criptografado, single-AZ.
- Subnets privadas sem rota para a internet; security group liberando 5432 apenas para o CIDR da VPC; `publicly_accessible = false`.
- Parameter group próprio: `rds.force_ssl = 1`, `log_min_duration_statement = 500 ms`, `idle_in_transaction_session_timeout = 60 s`, `pg_stat_statements`.
- Backup automático de 1 dia (gratuito até o tamanho do banco) com point-in-time recovery.
- Senha gerada por `random_password` e publicada no SSM (`/gearup/banco/senha`, SecureString) — nenhuma pessoa escolhe ou digita a senha.
- Um database por ambiente, criado pelas migrations do EF Core na primeira subida da API.

### Por que continuar relacional

O agregado `OrdemServico` referencia `Cliente` e `Veiculo`; `Orcamento` pertence a uma OS e tem itens que podem apontar para itens de `Estoque`; o histórico de status alimenta a métrica "tempo médio por status". Todas as consultas críticas (OS do cliente, fila da oficina por status/prioridade, linha do tempo da OS) são joins e ordenações que o PostgreSQL resolve com índices. As invariantes de dinheiro e estoque (quantidade > 0, saldo ≥ 0) ganham uma segunda barreira com check constraints.

### Por que o schema fica no repositório da aplicação

O `gearup-infra-db` provisiona a **instância**; o **schema** evolui com o código, via migrations versionadas no repositório GearUp e aplicadas pela própria API ao subir (`DatabaseInitializer`). Separar schema do código criaria dois lugares para manter sincronizados a cada mudança de entidade.

## Riscos

| Risco | Mitigação |
|---|---|
| Single-AZ: indisponibilidade numa falha de AZ | aceito no lab; em produção real, `multi_az = true` (uma linha no Terraform) |
| RDS cobra fora da sessão | workflow de destroy; alerta no guia de operação |
| Migration com lock longo em tabela grande | migrations pequenas; índices criados em tabelas ainda pequenas; revisar com `CREATE INDEX CONCURRENTLY` quando o volume justificar |
