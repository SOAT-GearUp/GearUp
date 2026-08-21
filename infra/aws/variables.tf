variable "project_name" {
  description = "Nome do projeto usado na composição dos recursos."
  type        = string
  default     = "gearup"
}

variable "environment" {
  description = "Ambiente provisionado."
  type        = string
  default     = "dev"
}

variable "aws_region" {
  description = "Região AWS onde os recursos serão criados."
  type        = string
  default     = "us-east-1"
}

variable "aws_profile" {
  description = "Profile local da AWS CLI usado pelo provider."
  type        = string
  default     = "gearup"
}

variable "vpc_cidr" {
  description = "CIDR principal da VPC."
  type        = string
  default     = "10.20.0.0/16"
}

variable "az_count" {
  description = "Quantidade de zonas de disponibilidade usadas."
  type        = number
  default     = 2
}

variable "cluster_name" {
  description = "Nome do cluster EKS."
  type        = string
  default     = "gearup-dev"
}

variable "cluster_version" {
  description = "Versão Kubernetes do EKS."
  type        = string
  default     = "1.36"
}

variable "eks_node_instance_types" {
  description = "Tipos de instância usados pelo node group gerenciado do EKS."
  type        = list(string)
  default     = ["t3.small"]
}

variable "ecr_repository_name" {
  description = "Nome do repositório ECR da API."
  type        = string
  default     = "gearup"
}

variable "ecr_force_delete" {
  description = "Permite excluir o repositório ECR mesmo contendo imagens."
  type        = bool
  default     = false
}

variable "db_name" {
  description = "Nome do banco de dados PostgreSQL."
  type        = string
  default     = "GearUp"
}

variable "db_username" {
  description = "Usuário master do banco PostgreSQL."
  type        = string
  default     = "gearup"
}

variable "db_password" {
  description = "Senha master do banco PostgreSQL."
  type        = string
  sensitive   = true
}

variable "db_engine_version" {
  description = "Versão do PostgreSQL no RDS."
  type        = string
  default     = "18.4"
}

variable "db_instance_class" {
  description = "Classe da instância RDS."
  type        = string
  default     = "db.t4g.micro"
}

variable "db_allocated_storage" {
  description = "Armazenamento inicial do RDS em GB."
  type        = number
  default     = 20
}

variable "db_backup_retention_days" {
  description = "Quantidade de dias de retenção de backup do RDS."
  type        = number
  default     = 1
}
