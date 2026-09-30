# ADR-002 — Comunicação síncrona REST com API Gateway como entrada única

| Campo | Valor |
|---|---|
| Status | Aceita |
| Data | 2026-09-29 |
| Substitui | acesso direto ao Service `LoadBalancer` do EKS (Fase 2) |

## Contexto

Na Fase 2 os clientes chamavam a API direto no Load Balancer do cluster. A Fase 3 adiciona uma função serverless de autenticação e exige controle e roteamento centralizados. É preciso decidir o padrão de comunicação entre clientes, borda, Lambda e API.

## Decisão

1. **REST síncrono sobre HTTPS** entre clientes e plataforma, com o **Amazon API Gateway (HTTP API)** como **única porta de entrada** de cada ambiente.
2. O gateway roteia por caminho: `/auth/cpf` → Lambda; `/api/*`, `/health/*`, `/swagger/*` → NLB da API no EKS (integração `HTTP_PROXY`).
3. `/api/*` exige JWT válido, checado por um **Lambda authorizer** antes de o tráfego sair do gateway. Exceção: `POST /api/autenticacao/login`.
4. **Throttling** (20 req/s, rajada 40) e **access logs JSON** no estágio do gateway.
5. Dentro da API, a comunicação entre bounded contexts continua **em processo**, via eventos de domínio despachados após o `SaveChanges` — sem broker.
6. Propagação de correlação: `X-Correlation-ID` do cliente atravessa gateway, Lambda e API; a API devolve o mesmo valor e o grava em todos os logs; `traceparent` (W3C) propaga o trace.

## Alternativas consideradas

- **REST API (v1) do API Gateway** — tem validação de payload e API keys, mas custa 3,5× mais por requisição e tem latência maior. Nada do que precisamos exige v1.
- **Ingress NGINX/Kong no cluster** — sem suporte nativo a Lambda authorizer; mais um componente consumindo os nós.
- **Mensageria assíncrona (SQS/EventBridge) entre contextos** — traria resiliência ao processamento de OS, mas o domínio ainda é um monólito modular; dividir em serviços não é requisito da fase. Os eventos de domínio já são o ponto de extensão para isso.

## Consequências

- (+) Um único endereço público por ambiente; o cluster pode sair da internet no futuro sem mudar os clientes.
- (+) Requisições sem token não consomem CPU do cluster (barradas na borda).
- (+) Custo proporcional ao uso (US$ 1 por milhão de requisições).
- (−) O HTTP API tem timeout de integração de 29 s — suficiente para as rotas atuais.
- (−) O NLB continua público (sem VPC Link, que custaria um NLB interno adicional). Mitigação: o valor está no authorizer + validação JWT na API; migrar para VPC Link é uma mudança isolada no `gearup-lambda-auth`.
