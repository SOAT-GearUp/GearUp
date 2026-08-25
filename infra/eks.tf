# ---------------------------------------------------------------------------
# Cluster EKS + Node Group
#
# Recursos nativos (`aws_eks_cluster` / `aws_eks_node_group`) em vez do módulo
# da comunidade, porque o módulo cria roles e policies IAM — proibido no
# Learner Lab.
#
# CUSTO: o control plane cobra US$ 0,10/h (US$ 2,40/dia) 24/7, INCLUSIVE com a
# sessão do lab encerrada. Cada nó t3.medium soma US$ 0,0416/h. Rode
# `terraform destroy` ao fim de cada sessão.
# ---------------------------------------------------------------------------

resource "aws_eks_cluster" "principal" {
  name     = var.nome_cluster
  role_arn = local.arn_role_cluster # descoberta automática, ver data.tf

  # `version` deliberadamente omitido: deixamos a AWS escolher a versão mais
  # recente. Fixar uma versão antiga cairia em "extended support", que custa
  # US$ 0,60/h em vez de US$ 0,10/h — 6x mais caro.

  vpc_config {
    # O control plane exige subnets em pelo menos 2 AZs.
    subnet_ids              = concat(aws_subnet.publica[*].id, aws_subnet.privada[*].id)
    endpoint_private_access = true
    endpoint_public_access  = true
    public_access_cidrs     = var.cidrs_acesso_api_kubernetes
  }

  access_config {
    # Modo híbrido: entradas de acesso da API do EKS + aws-auth ConfigMap.
    authentication_mode = "API_AND_CONFIG_MAP"
    # Dá permissão de admin no cluster a quem rodou o `terraform apply`
    # (a role do lab), sem precisar editar o aws-auth manualmente.
    bootstrap_cluster_creator_admin_permissions = true
  }

  # `enabled_cluster_log_types` propositalmente não habilitado: logs do control
  # plane vão para o CloudWatch e passam a cobrar ingestão/retenção.

  tags = {
    Name = var.nome_cluster
  }
}

# --------------------------- Worker Nodes ----------------------------------
resource "aws_eks_node_group" "principal" {
  cluster_name    = aws_eks_cluster.principal.name
  node_group_name = "${local.prefixo}-ng-principal"
  node_role_arn   = local.arn_role_nos # descoberta automática, ver data.tf

  # Nós nas subnets públicas (map_public_ip_on_launch = true) para dispensar
  # o NAT Gateway. O managed node group cria o instance profile a partir da
  # role — por isso nenhum recurso IAM é declarado.
  subnet_ids = aws_subnet.publica[*].id

  instance_types = [var.tipo_instancia_nos]
  ami_type       = "AL2023_x86_64_STANDARD"
  disk_size      = var.disco_nos_gb

  # Somente On-Demand: instâncias Spot são bloqueadas no Learner Lab.
  capacity_type = "ON_DEMAND"

  scaling_config {
    desired_size = var.nos_desejados
    min_size     = var.nos_minimos
    max_size     = var.nos_maximos # teto baixo: 20+ instâncias desativam a conta
  }

  update_config {
    max_unavailable = 1
  }

  labels = {
    workload = "gearup"
  }

  tags = {
    Name = "${local.prefixo}-node"
  }

  # Evita recriação a cada apply quando o HPA/Cluster Autoscaler mexeu no
  # desired_size.
  lifecycle {
    ignore_changes = [scaling_config[0].desired_size]
  }
}

# --------------------------- Addon: metrics-server -------------------------
# Pré-requisito do Horizontal Pod Autoscaler (k8s/hpa.yaml). É gratuito.
# Se a role do lab não tiver permissão para criar addons, use o fallback:
#   kubectl apply -f https://github.com/kubernetes-sigs/metrics-server/releases/latest/download/components.yaml
resource "aws_eks_addon" "metrics_server" {
  count = var.instalar_metrics_server ? 1 : 0

  cluster_name                = aws_eks_cluster.principal.name
  addon_name                  = "metrics-server"
  resolve_conflicts_on_create = "OVERWRITE"
  resolve_conflicts_on_update = "OVERWRITE"

  # Precisa de nós prontos para agendar o pod do metrics-server.
  depends_on = [aws_eks_node_group.principal]
}
