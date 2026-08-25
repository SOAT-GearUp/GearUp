# ---------------------------------------------------------------------------
# Security Groups
#
# O EKS cria e gerencia sozinho o "cluster security group", que é anexado
# automaticamente ao control plane e a todos os nós do managed node group.
# Ele já libera todo o tráfego entre control plane e nós, então não
# reinventamos essa roda: só criamos o SG do banco e o autorizamos a receber
# conexões *daquele* SG.
#
# Como o VPC CNI dá aos pods ENIs que herdam os security groups do nó, liberar
# o SG do cluster é o suficiente para os pods da API alcançarem o RDS.
# ---------------------------------------------------------------------------

resource "aws_security_group" "banco" {
  name        = "${local.prefixo}-rds-sg"
  description = "Permite acesso ao PostgreSQL apenas de dentro do cluster EKS"
  vpc_id      = aws_vpc.principal.id

  tags = {
    Name = "${local.prefixo}-rds-sg"
  }

  lifecycle {
    create_before_destroy = true
  }
}

# Regra principal: 5432 liberada somente para o security group do cluster.
resource "aws_vpc_security_group_ingress_rule" "banco_do_cluster" {
  security_group_id            = aws_security_group.banco.id
  referenced_security_group_id = aws_eks_cluster.principal.vpc_config[0].cluster_security_group_id
  ip_protocol                  = "tcp"
  from_port                    = 5432
  to_port                      = 5432
  description                  = "PostgreSQL a partir dos nos/pods do EKS"
}

# Regras opcionais para depuração local (psql, dotnet ef database update).
# Só têm efeito se `banco_publicamente_acessivel = true`.
resource "aws_vpc_security_group_ingress_rule" "banco_admin" {
  count = length(var.cidrs_admin_banco)

  security_group_id = aws_security_group.banco.id
  cidr_ipv4         = var.cidrs_admin_banco[count.index]
  ip_protocol       = "tcp"
  from_port         = 5432
  to_port           = 5432
  description       = "PostgreSQL a partir de CIDR administrativo"
}

# Egress liberado: o RDS não inicia conexões, mas a AWS exige uma regra de
# saída explícita quando o SG é gerenciado por regras separadas.
resource "aws_vpc_security_group_egress_rule" "banco_saida" {
  security_group_id = aws_security_group.banco.id
  cidr_ipv4         = "0.0.0.0/0"
  ip_protocol       = "-1"
  description       = "Saida liberada"
}
