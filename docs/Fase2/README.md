# GearUp - Fase 2

## Objetivo da fase

A Fase 2 evolui o GearUp para um cenário de maior demanda, disponibilidade e automação. O foco deixa de ser apenas a aplicação e passa a incluir containerização, Kubernetes, infraestrutura como código e pipeline CI/CD.

## Escopo principal

| Área | Objetivo |
|---|---|
| Aplicação | Evoluir APIs, refatorar código, manter Clean Architecture e testes automatizados |
| Docker | Garantir aplicação containerizada com Dockerfile e docker-compose revisados |
| Kubernetes | Criar manifests para Deployment, Service, ConfigMap, Secret e HPA |
| Terraform | Provisionar infraestrutura, cluster Kubernetes e banco de dados |
| CI/CD | Automatizar build, testes, imagem Docker e deploy no Kubernetes |
| Documentação | Explicar arquitetura, execução local, provisionamento e deploy |

## Documentos desta fase

| Documento | Descrição |
|---|---|
| [Sequência de Entrega](Sequencia%20de%20Entrega.md) | Ordem recomendada para implementar e validar os entregáveis |
| [Arquitetura AWS e Kubernetes](Arquitetura%20AWS%20e%20Kubernetes.md) | Desenho lógico da solução em AWS com EKS, ECR e banco |
| [Pipeline CI/CD](Pipeline%20CI-CD.md) | Estratégia de workflows, etapas, secrets e responsabilidades |
| [Deploy na AWS](Deploy%20na%20AWS.md) | Passo a passo conceitual para provisionar e publicar a aplicação |

## Responsabilidade de CI/CD

A parte de pipeline deve entregar os arquivos de configuração responsáveis por:

- Compilar a aplicação.
- Executar testes automatizados.
- Validar cobertura mínima quando aplicável.
- Executar análise de qualidade.
- Criar a imagem Docker da API.
- Publicar a imagem em um registry.
- Aplicar os manifests Kubernetes.
- Executar o deploy da aplicação no cluster.
- Validar o rollout.

## Resultado esperado

Ao final da fase, deve ser possível demonstrar:

1. Alteração no código enviada para o repositório.
2. Pipeline executando build e testes.
3. Imagem Docker publicada.
4. Infraestrutura provisionada.
5. Aplicação publicada em Kubernetes.
6. API funcionando no ambiente publicado.
7. Escalabilidade automática configurada por HPA.
