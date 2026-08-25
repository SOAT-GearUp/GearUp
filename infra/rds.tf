# ---------------------------------------------------------------------------
# PostgreSQL gerenciado (RDS)
#
# CUSTO: db.t3.micro single-AZ ≈ US$ 0,018/h (US$ 0,43/dia) + US$ 0,115/GB-mês
# de storage, e CONTINUA COBRANDO com a sessão do lab encerrada. Se o
# orçamento apertar, use `criar_banco_rds = false` e suba o PostgreSQL como
# StatefulSet no cluster (../k8s/persistencia), que não gera custo extra além
# do EBS do volume.
#
# Configuração deliberadamente enxuta para não gerar custo escondido:
# single-AZ, sem backups automáticos, sem Performance Insights, sem réplica.
# É a escolha certa para um lab e a errada para produção real.
# ---------------------------------------------------------------------------

# Grupo de subnets: define em quais subnets o RDS pode criar sua ENI.
# Privadas por padrão (isolado); públicas apenas se você explicitamente pedir
# acesso externo, já que `publicly_accessible` exige rota para o IGW.
resource "aws_db_subnet_group" "postgres" {
  count = var.criar_banco_rds ? 1 : 0

  name       = "${local.prefixo}-db-subnets"
  subnet_ids = var.banco_publicamente_acessivel ? aws_subnet.publica[*].id : aws_subnet.privada[*].id

  tags = {
    Name = "${local.prefixo}-db-subnets"
  }
}

resource "aws_db_instance" "postgres" {
  count = var.criar_banco_rds ? 1 : 0

  identifier = "${local.prefixo}-postgres"

  engine = "postgres"
  # Apenas a major version: a AWS aplica o minor mais recente disponível.
  engine_version              = var.versao_postgres
  auto_minor_version_upgrade  = true
  allow_major_version_upgrade = false

  instance_class    = var.classe_instancia_banco
  allocated_storage = var.armazenamento_banco_gb
  storage_type      = "gp3"
  storage_encrypted = true
  # `max_allocated_storage` omitido de propósito: autoscaling de storage é uma
  # porta aberta para custo inesperado.

  # Nome em minúsculas: o PostgreSQL dobra identificadores não citados para
  # minúsculo, então "GearUp" não bateria com a connection string.
  db_name  = var.nome_banco
  username = var.usuario_banco
  password = var.senha_banco
  port     = 5432

  db_subnet_group_name   = aws_db_subnet_group.postgres[0].name
  vpc_security_group_ids = [aws_security_group.banco.id]
  publicly_accessible    = var.banco_publicamente_acessivel
  multi_az               = false # Multi-AZ dobraria o custo

  # Lab: sem backup, sem snapshot final e sem proteção de exclusão — é o que
  # permite `terraform destroy` limpar tudo rapidamente e sem custo residual.
  backup_retention_period      = 0
  skip_final_snapshot          = true
  delete_automated_backups     = true
  deletion_protection          = false
  copy_tags_to_snapshot        = false
  performance_insights_enabled = false

  apply_immediately = true

  tags = {
    Name = "${local.prefixo}-postgres"
  }
}
