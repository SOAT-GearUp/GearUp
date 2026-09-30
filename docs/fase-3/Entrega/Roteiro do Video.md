# Roteiro do Vídeo de Demonstração (≤ 15 min)

Cobre os seis itens obrigatórios: autenticação com CPF, execução da pipeline, deploy automatizado, consumo das APIs protegidas, dashboard ao vivo, logs e traces.

## Antes de gravar (fora do vídeo, ~35 min)

1. Start Lab → credenciais em `~/.aws/credentials` → `.\scripts\atualizar-secrets-aws.ps1 -DatadogApiKey ... -DatadogAppKey ...`
2. Subir a plataforma na ordem do [Guia de Operação](../Operacao/Guia%20de%20Deploy%20e%20Operacao.md) (k8s → db → GearUp → lambda). Deixe **homolog** pronta.
3. Postman: importar a collection da Fase 3, preencher `gatewayUrl` (homolog) e `senhaAdmin` (`aws ssm get-parameter --name /gearup/homolog/api/senha-admin --with-decryption --query Parameter.Value --output text`).
4. Rodar a collection uma vez (gera dados para o dashboard) e deixar abertos: GitHub Actions, Datadog (dashboard "GearUp - Operação", APM, Logs), CloudWatch Logs da Lambda, terminal com `kubectl`.
5. Preparar uma alteração pequena numa branch `feature/demo` (ex.: texto de log) para o PR.

## Roteiro

| Tempo | Cena | O que mostrar / falar |
|---|---|---|
| 0:00–1:00 | Abertura | Objetivo da Fase 3; diagrama de componentes (`docs/fase-3/Arquitetura`); os 4 repositórios na organização |
| 1:00–2:00 | Proteção de branch | Settings → Branches de um repo: `main` protegida, PR obrigatório, status check do CI. Tentar `git push origin main` e mostrar a recusa |
| 2:00–4:30 | **Pipeline CI/CD** | Abrir PR `feature/demo → homolog` no GearUp; CI rodando (build, testes com Testcontainers, cobertura). Merge → workflow **CD** dispara sozinho |
| 4:30–6:00 | **Deploy automatizado** | Acompanhar os passos: imagem no ECR com tag do SHA, Secret montado do SSM, `kubectl apply -k`, rollout, smoke test. No terminal: `kubectl get pods,hpa -n gearup-homolog` mostrando o pod novo. Resumo do job com a URL do gateway |
| 6:00–8:00 | **Autenticação com CPF** | Postman → pasta 2: CPF inválido (400), CPF inexistente (404), CPF válido (200 com `accessToken`). Colar o token em jwt.io: `role=Cliente`, `cliente_id`, `iss`, `aud`, `exp`. CloudWatch: log JSON da Lambda com CPF mascarado e `correlationId` |
| 8:00–9:30 | **APIs protegidas** | Pasta 3: sem token → 401 no gateway (não chega ao cluster); com token do cliente → só as próprias OS; `/api/clientes` → 403 (perfil). Mostrar o authorizer nos access logs do gateway |
| 9:30–12:00 | **Dashboard ao vivo** | Gerar carga (`Run collection` com 20 iterações). Datadog → dashboard: OS criadas no dia, transições/tempo por status, latência p95 por rota, CPU/memória dos pods, réplicas do HPA, uptime. Abrir a lista de monitores (falhas de OS, latência, healthcheck) |
| 12:00–14:00 | **Logs e traces** | Pegar o `X-Correlation-ID` de uma resposta do Postman → Datadog Logs `@CorrelationId:<valor>` → abrir o trace no APM (span HTTP + span do PostgreSQL) → voltar ao log pelo `trace_id` |
| 14:00–15:00 | Encerramento | Decisões de custo (RFC-001, ADR-005), link da documentação, lembrete de destroy |

## Depois de gravar

Rodar o destroy na ordem inversa (lambda → GearUp → db → k8s) e conferir que `aws eks list-clusters` e `aws rds describe-db-instances` voltam vazios.
