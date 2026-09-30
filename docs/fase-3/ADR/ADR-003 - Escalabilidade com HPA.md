# ADR-003 — Escalabilidade horizontal com HPA por CPU

| Campo | Valor |
|---|---|
| Status | Aceita |
| Data | 2026-09-29 |
| Evidências | [Deploy AWS com EKS (Fase 2)](../../fase-2/Kubernetes/Deploy%20AWS%20com%20EKS.md) — testes de reescala |

## Contexto

O cluster precisa escalar com a demanda de várias unidades da oficina, dentro de nós fixos pequenos (t3.medium) e do limite de 9 instâncias do Learner Lab.

## Decisão

- **Horizontal Pod Autoscaler** (`autoscaling/v2`) na API, métrica **CPU a 70% do request** (100m).
- Teto por ambiente via overlay Kustomize: **homolog 1–2**, **production 1–4** réplicas.
- Política assimétrica: sobe rápido (até dobrar ou +2 pods a cada 30 s), desce devagar (1 pod/min após 5 min estáveis) para evitar *flapping*.
- `metrics-server` instalado como **addon gerenciado do EKS** pelo `gearup-infra-k8s`.
- Node group gerenciado com `min 1 / desired 2 / max 3` e `max_unavailable = 1`; o teto de nós é validado no Terraform (≤ 4) para nunca se aproximar do limite de instâncias do lab.
- `startupProbe` de até 2 min separa a primeira subida (migrations) da `livenessProbe`, evitando que o HPA/kubelet mate pods que ainda estão migrando o banco.

## Por que não memória

Na validação da Fase 2, a métrica de memória marcou 90% com CPU em 3%, prendendo o HPA no máximo sem carga: o GC do .NET retém memória por design. Memória fica como **alerta** no Datadog (85% do limite), não como gatilho de escala.

## Alternativas consideradas

- **KEDA por requisições/fila** — mais preciso para cargas de I/O, mas exige ScaledObjects e um adapter de métricas; ganho pequeno com o volume atual.
- **Cluster Autoscaler / Karpenter** — precisam de role IAM própria (IRSA) → proibido no lab. O node group fica fixo e o teto do HPA é dimensionado para caber nele.
- **VPA** — conflita com HPA na mesma métrica.

## Consequências

- (+) Absorve picos em ~1 min sem intervenção.
- (+) Réplicas visíveis no dashboard (grupo 5) e alerta de CPU a 80% do limite indica quando o teto está curto.
- (−) Sem autoscaling de nós: acima de ~6 réplicas somadas, novos pods ficariam `Pending`. Aceito e monitorado.
