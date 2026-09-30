# ADR-005 — Rede sem NAT Gateway

| Campo | Valor |
|---|---|
| Status | Aceita |
| Data | 2026-09-29 |

## Contexto

O desenho "clássico" coloca nós do EKS, Lambda e RDS em subnets privadas com um NAT Gateway para saída à internet. No Learner Lab, o NAT custa US$ 0,045/h + US$ 0,045/GB — quase metade do control plane do EKS — e **continua cobrando com o lab desligado**.

## Decisão

- **Sem NAT Gateway.**
- Nós do EKS em **subnets públicas** com IP público (para baixar imagens do ECR/Docker Hub e falar com a API do EKS), protegidos pelo security group do cluster, que não abre portas de entrada da internet.
- RDS e Lambda de autenticação em **subnets privadas sem rota default**: só alcançam e são alcançados de dentro da VPC.
- Lambda authorizer **fora da VPC** (não acessa o banco).

## Consequências

- (+) Economia de ~US$ 1,10/dia, sem custo por GB trafegado.
- (+) O banco é isolado por construção (não existe rota para fora).
- (−) A Lambda de autenticação **não alcança a internet**: não pode enviar telemetria ao Datadog nem ler SSM/Secrets Manager em tempo de execução. Por isso ela recebe a configuração no deploy e é monitorada pelo **CloudWatch** (logs JSON, alarmes de erro, latência p95, 5xx do gateway e métrica derivada de log para falhas de banco).
- (−) Nós com IP público exigem disciplina nos security groups. O NLB é a única porta aberta, e só na porta 80 do Service.

## Como reverter

Adicionar `aws_nat_gateway` + rota `0.0.0.0/0` nas subnets privadas do `gearup-infra-k8s`, mover o node group para as privadas e habilitar a extensão do Datadog na Lambda. Nenhuma mudança de código.
