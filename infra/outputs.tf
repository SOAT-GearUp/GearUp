# ---------------------------------------------------------------------------
# Outputs — a ponte entre o Terraform e os manifests em ../k8s
# ---------------------------------------------------------------------------

output "cluster_nome" {
  description = "Nome do cluster EKS."
  value       = aws_eks_cluster.principal.name
}

output "cluster_endpoint" {
  description = "Endpoint da API do Kubernetes."
  value       = aws_eks_cluster.principal.endpoint
}

output "cluster_versao_kubernetes" {
  description = "Versão do Kubernetes escolhida pela AWS."
  value       = aws_eks_cluster.principal.version
}

output "cluster_security_group_id" {
  description = "Security group gerenciado do cluster (anexado ao control plane e aos nós)."
  value       = aws_eks_cluster.principal.vpc_config[0].cluster_security_group_id
}

output "roles_iam_utilizadas" {
  description = "Roles resolvidas pela descoberta automática — confira antes do apply."
  value = {
    control_plane = local.arn_role_cluster
    worker_nodes  = local.arn_role_nos
  }
}

output "vpc_id" {
  description = "ID da VPC criada."
  value       = aws_vpc.principal.id
}

output "subnets_publicas" {
  description = "Subnets públicas (worker nodes e Load Balancers)."
  value       = aws_subnet.publica[*].id
}

output "subnets_privadas" {
  description = "Subnets privadas (RDS)."
  value       = aws_subnet.privada[*].id
}

output "comando_kubeconfig" {
  description = "Comando para configurar o kubectl neste cluster."
  value       = "aws eks update-kubeconfig --region us-east-1 --name ${aws_eks_cluster.principal.name}"
}

output "banco_endereco" {
  description = "Hostname do RDS (vazio quando criar_banco_rds = false)."
  value       = try(aws_db_instance.postgres[0].address, "")
}

output "banco_porta" {
  description = "Porta do RDS."
  value       = try(aws_db_instance.postgres[0].port, 0)
}

output "connection_string" {
  description = <<-EOT
    Connection string pronta para o secret do Kubernetes.
    `SSL Mode=Require` é obrigatório: o RDS PostgreSQL 15+ vem com
    rds.force_ssl = 1 e recusa conexões em texto claro.
    Leia com: terraform output -raw connection_string
  EOT
  sensitive   = true
  value = var.criar_banco_rds ? format(
    "Host=%s;Port=%d;Database=%s;Username=%s;Password=%s;SSL Mode=Require;Trust Server Certificate=true",
    aws_db_instance.postgres[0].address,
    aws_db_instance.postgres[0].port,
    var.nome_banco,
    var.usuario_banco,
    var.senha_banco,
  ) : ""
}

output "connection_string_base64" {
  description = <<-EOT
    A mesma connection string em base64, pronta para colar em
    ../k8s/secret.yaml (campo data.connection-string).
    Leia com: terraform output -raw connection_string_base64
  EOT
  sensitive   = true
  value = var.criar_banco_rds ? base64encode(format(
    "Host=%s;Port=%d;Database=%s;Username=%s;Password=%s;SSL Mode=Require;Trust Server Certificate=true",
    aws_db_instance.postgres[0].address,
    aws_db_instance.postgres[0].port,
    var.nome_banco,
    var.usuario_banco,
    var.senha_banco,
  )) : ""
}

output "custo_estimado_por_hora_usd" {
  description = <<-EOT
    Estimativa grosseira do custo/hora da stack (on-demand us-east-1), para
    conferência rápida antes do apply. Não inclui storage, LB nem tráfego.
  EOT
  value = format(
    "~US$ %.4f/h (control plane 0.10 + %d no(s) e RDS)",
    0.10 + (var.nos_desejados * 0.0416) + (var.criar_banco_rds ? 0.018 : 0),
    var.nos_desejados
  )
}
