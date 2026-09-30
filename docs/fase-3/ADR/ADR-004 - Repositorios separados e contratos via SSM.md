# ADR-004 — Quatro repositórios com contratos via SSM Parameter Store

| Campo | Valor |
|---|---|
| Status | Aceita |
| Data | 2026-09-29 |

## Contexto

O Tech Challenge exige quatro repositórios (Lambda, infraestrutura Kubernetes, infraestrutura do banco, aplicação), cada um com CI/CD e deploy automático. Eles dependem uns dos outros: o banco precisa da VPC, a Lambda precisa do banco e da chave JWT da API, o gateway precisa do endereço da API.

## Decisão

1. Cada repositório tem **state Terraform próprio** no mesmo bucket S3 (`gearup-tfstate-<account_id>`), com chave própria e lock nativo do S3 (`use_lockfile`). O bucket é criado idempotentemente por `scripts/tf-init.sh` — ninguém precisa criá-lo à mão numa conta nova.
2. Nenhum repositório lê o state de outro (`terraform_remote_state` **não** é usado). As dependências são:
   - **tags** para rede (`Name=gearup-vpc`, `Camada=privada`);
   - **nomes fixos** para cluster e ECR (`gearup-eks`, `gearup-api`);
   - **parâmetros no SSM** para dados que mudam a cada criação (endpoint do RDS, senhas, chave JWT, URL do NLB, URL do gateway).
3. Segredos são **gerados por máquina** (Terraform `random_password` ou `openssl rand` na pipeline) e ficam apenas em SSM SecureString. Os GitHub Secrets se limitam às credenciais temporárias do lab.

## Alternativas consideradas

- **Monorepo com pastas** — contraria o requisito de quatro repositórios.
- **`terraform_remote_state`** — acopla o formato interno do state entre equipes; renomear um output quebra outro repositório sem aviso.
- **GitHub Secrets replicados** (senha do banco, chave JWT em cada repositório) — quatro cópias para girar a cada recriação do RDS; alguém teria que ler e colar a senha.
- **AWS Secrets Manager** — rotação automática, mas US$ 0,40/segredo/mês e, sem NAT, exigiria VPC endpoint (US$ 0,01/h por AZ) para a Lambda ler em tempo de execução.

## Consequências

- (+) Ciclos de vida independentes: dá para recriar a Lambda sem tocar no cluster.
- (+) Uma conta de lab nova (a do professor, por exemplo) funciona sem editar nomes de bucket ou ARNs.
- (−) Existe **ordem de deploy** na primeira vez (k8s → db → app → lambda). Cada pipeline falha cedo com mensagem clara quando a dependência não existe.
- (−) A Lambda recebe a senha do banco como variável de ambiente no deploy (criptografada em repouso pela AWS). Girar a senha exige re-deploy da Lambda.
