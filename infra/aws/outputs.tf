output "aws_region" {
  description = "Região AWS configurada."
  value       = var.aws_region
}

output "ecr_repository_url" {
  description = "URL do repositório ECR."
  value       = aws_ecr_repository.gearup.repository_url
}

output "eks_cluster_name" {
  description = "Nome do cluster EKS."
  value       = module.eks.cluster_name
}

output "rds_endpoint" {
  description = "Endpoint do banco RDS PostgreSQL."
  value       = module.rds.db_instance_endpoint
}

output "rds_database_name" {
  description = "Nome do banco de dados criado no RDS."
  value       = var.db_name
}

output "update_kubeconfig_command" {
  description = "Comando para configurar kubectl no cluster EKS."
  value       = "aws eks update-kubeconfig --region ${var.aws_region} --name ${module.eks.cluster_name} --profile ${var.aws_profile}"
}
