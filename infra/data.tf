# ---------------------------------------------------------------------------
# Data sources
#
# IMPORTANTE: nenhuma role/policy IAM é criada aqui. O Learner Lab proíbe
# criar usuários, grupos e roles IAM — por isso as roles pré-existentes do lab
# são apenas *referenciadas*. Isso também é o motivo de não usarmos o módulo
# terraform-aws-modules/eks (ele cria IAM por padrão).
# ---------------------------------------------------------------------------

# AZs realmente disponíveis na região (e habilitadas para a conta).
data "aws_availability_zones" "disponiveis" {
  state = "available"

  filter {
    name   = "opt-in-status"
    values = ["opt-in-not-required"]
  }
}

# ------------------------- Descoberta das roles ----------------------------
# Cada conta de Learner Lab nomeia as roles do EKS com um sufixo próprio, do
# tipo `c220532a5561746l16049283t1w544124-LabEksClusterRole-rPckPp64hltK`.
# Fixar esse nome quebraria o projeto em qualquer outra conta (a do professor,
# por exemplo), então a busca é por padrão de nome.
#
# Ordem de precedência: valor explícito nas variáveis -> role específica de EKS
# encontrada na conta -> LabRole (cuja trust policy cobre eks.amazonaws.com e
# ec2.amazonaws.com).

data "aws_iam_roles" "eks_cluster" {
  name_regex = ".*LabEksClusterRole.*"
}

data "aws_iam_roles" "eks_nos" {
  name_regex = ".*LabEksNodeRole.*"
}

# Sempre presente em qualquer Learner Lab — é o fallback.
data "aws_iam_role" "lab" {
  name = "LabRole"
}

# Consultadas apenas quando o nome é informado explicitamente nas variáveis.
data "aws_iam_role" "cluster_explicita" {
  count = var.role_cluster == "" ? 0 : 1
  name  = var.role_cluster
}

data "aws_iam_role" "nos_explicita" {
  count = var.role_nos == "" ? 0 : 1
  name  = var.role_nos
}

locals {
  # Duas AZs por padrão: mínimo exigido pelo control plane do EKS.
  azs = slice(data.aws_availability_zones.disponiveis.names, 0, var.quantidade_azs)

  prefixo = var.nome_projeto

  # `tolist(...)[0]`: o data source devolve um set. Os regex acima são
  # específicos o suficiente para casar com uma única role por conta.
  arn_role_cluster = coalesce(
    try(data.aws_iam_role.cluster_explicita[0].arn, null),
    try(tolist(data.aws_iam_roles.eks_cluster.arns)[0], null),
    data.aws_iam_role.lab.arn,
  )

  arn_role_nos = coalesce(
    try(data.aws_iam_role.nos_explicita[0].arn, null),
    try(tolist(data.aws_iam_roles.eks_nos.arns)[0], null),
    data.aws_iam_role.lab.arn,
  )
}
