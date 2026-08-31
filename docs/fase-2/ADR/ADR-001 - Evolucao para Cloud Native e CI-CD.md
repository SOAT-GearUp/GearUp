# ADR-001 - Evolução para Cloud Native e CI/CD

## Status

Aceita.

## Contexto

A Fase 2 do Tech Challenge exige a evolução da aplicação GearUp para um modelo mais resiliente, escalável e automatizado.

Na Fase 1, o projeto foi estruturado como uma API REST em .NET, com DDD, Clean Architecture, PostgreSQL, Docker Compose, testes automatizados e documentação de domínio.

Para a Fase 2, o sistema precisa evoluir para atender aos seguintes pontos:

- execução da aplicação em containers;
- orquestração com Kubernetes;
- uso de ConfigMaps e Secrets para configuração;
- escalabilidade horizontal por HPA;
- provisionamento de infraestrutura com Terraform;
- pipeline CI/CD com build, testes, criação de imagem Docker e deploy;
- preparação para deploy em ambiente cloud.

Além disso, a aplicação precisa continuar respeitando a separação de responsabilidades entre API, Application, Domain e Infrastructure.

## Decisão

Decidimos evoluir o GearUp para uma abordagem cloud native, mantendo a arquitetura interna baseada em Clean Architecture e adicionando uma camada operacional voltada para containers, Kubernetes, infraestrutura como código e automação de deploy.

A solução adotada será composta por:

- Docker para empacotar a API;
- Docker Compose para desenvolvimento local;
- Kubernetes para orquestração da aplicação;
- manifests YAML organizados em `/k8s`;
- ConfigMaps para configurações não sensíveis;
- Secrets para configurações sensíveis;
- HPA para escalabilidade horizontal;
- Terraform para provisionamento da infraestrutura;
- GitHub Actions para pipeline CI/CD;
- AWS ECR como registry de imagens Docker;
- AWS EKS como cluster Kubernetes em cloud;
- banco PostgreSQL local para desenvolvimento e RDS PostgreSQL para ambiente cloud.

No código da aplicação, os novos fluxos da Fase 2 devem continuar sendo implementados dentro da camada Application, mantendo os controllers apenas como adaptadores HTTP.

## Alternativas Consideradas

### Manter apenas Docker Compose

Essa alternativa simplificaria o ambiente, mas não atenderia ao requisito de orquestração com Kubernetes nem demonstraria escalabilidade horizontal.

### Fazer deploy manual na AWS

Essa alternativa reduziria a complexidade inicial, mas não atenderia ao requisito de CI/CD e diminuiria a rastreabilidade do processo de entrega.

### Criar infraestrutura manualmente pelo Console AWS

Essa alternativa facilitaria a criação inicial dos recursos, mas dificultaria reprodução, versionamento e auditoria da infraestrutura.

### Usar outro provedor cloud

Seria tecnicamente possível usar Azure, GCP ou outro provedor. A AWS foi escolhida por oferecer integração direta com ECR, EKS, RDS e por atender bem ao escopo do desafio.

## Consequências Positivas

- Deploy mais padronizado e reproduzível.
- Separação clara entre código da aplicação e infraestrutura.
- Maior rastreabilidade do que foi publicado por meio de imagens versionadas.
- Possibilidade de escalar a API horizontalmente conforme demanda.
- Ambiente mais próximo de uma arquitetura usada em produção.
- Pipeline automatizada reduzindo execução manual de build, testes e deploy.

## Consequências Negativas

- Aumenta a complexidade operacional do projeto.
- Exige configuração de credenciais, secrets, permissões IAM e acesso ao cluster.
- Kubernetes e Terraform adicionam curva de aprendizado.
- O ambiente local e o ambiente cloud passam a ter configurações diferentes, exigindo documentação clara.

## Critérios de Aceite

A decisão será considerada implementada quando o projeto possuir:

- Dockerfile funcional para a API;
- docker-compose funcional para desenvolvimento local;
- manifests Kubernetes em `/k8s`;
- scripts Terraform em `/infra`;
- pipeline CI/CD executando build e testes;
- etapa de build e push da imagem Docker;
- etapa de deploy em Kubernetes;
- documentação de execução local, deploy e provisionamento.

## Observações

Para desenvolvimento local, o banco pode ser executado em container. Para ambiente AWS, a recomendação é usar RDS PostgreSQL, evitando manter o banco como pod dentro do cluster Kubernetes.

As imagens Docker devem usar tags versionadas, preferencialmente associadas ao commit da pipeline, evitando depender apenas de `latest`.
