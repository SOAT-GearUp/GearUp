# ---------------------------------------------------------------------------
# Rede
#
# Topologia: 1 VPC, 2 subnets públicas (worker nodes + Load Balancer) e
# 2 subnets privadas (RDS), uma de cada por AZ.
#
# DECISÃO DE CUSTO (trade-off documentado): NÃO existe NAT Gateway aqui.
# Um NAT custaria US$ 0,045/h + US$ 0,045/GB — quase metade do custo do
# control plane do EKS — e continuaria cobrando fora da sessão do lab. Por
# isso os worker nodes ficam em subnets PÚBLICAS (com IP público, para
# baixar imagens e falar com a API do EKS) e são protegidos pelo security
# group gerenciado do cluster, que não abre nenhuma porta de entrada da
# internet. O RDS fica em subnets privadas sem rota default: ele não precisa
# de internet, apenas de tráfego interno da VPC.
# ---------------------------------------------------------------------------

resource "aws_vpc" "principal" {
  cidr_block           = var.cidr_vpc
  enable_dns_support   = true
  enable_dns_hostnames = true # exigido pelo EKS e pelo RDS

  tags = {
    Name = "${local.prefixo}-vpc"
  }
}

# Porta de saída/entrada da VPC para a internet, usada pelas subnets públicas.
resource "aws_internet_gateway" "principal" {
  vpc_id = aws_vpc.principal.id

  tags = {
    Name = "${local.prefixo}-igw"
  }
}

# --------------------------- Subnets públicas ------------------------------
# Hospedam os worker nodes e os Load Balancers criados por Services do tipo
# LoadBalancer. A tag kubernetes.io/role/elb=1 é o que permite ao
# cloud-controller-manager do EKS escolher estas subnets para o LB público.
resource "aws_subnet" "publica" {
  count = var.quantidade_azs

  vpc_id                  = aws_vpc.principal.id
  availability_zone       = local.azs[count.index]
  cidr_block              = cidrsubnet(var.cidr_vpc, 4, count.index)
  map_public_ip_on_launch = true

  tags = {
    Name                                        = "${local.prefixo}-publica-${local.azs[count.index]}"
    "kubernetes.io/role/elb"                    = "1"
    "kubernetes.io/cluster/${var.nome_cluster}" = "shared"
  }
}

# --------------------------- Subnets privadas ------------------------------
# Hospedam o RDS. Sem rota para a internet (sem NAT) — isolamento por
# construção, custo zero.
resource "aws_subnet" "privada" {
  count = var.quantidade_azs

  vpc_id            = aws_vpc.principal.id
  availability_zone = local.azs[count.index]
  cidr_block        = cidrsubnet(var.cidr_vpc, 4, count.index + 8)

  tags = {
    Name                                        = "${local.prefixo}-privada-${local.azs[count.index]}"
    "kubernetes.io/role/internal-elb"           = "1"
    "kubernetes.io/cluster/${var.nome_cluster}" = "shared"
  }
}

# ---------------------------- Tabelas de rota ------------------------------

# Pública: default route para o Internet Gateway.
resource "aws_route_table" "publica" {
  vpc_id = aws_vpc.principal.id

  route {
    cidr_block = "0.0.0.0/0"
    gateway_id = aws_internet_gateway.principal.id
  }

  tags = {
    Name = "${local.prefixo}-rt-publica"
  }
}

resource "aws_route_table_association" "publica" {
  count = var.quantidade_azs

  subnet_id      = aws_subnet.publica[count.index].id
  route_table_id = aws_route_table.publica.id
}

# Privada: apenas a rota local da VPC (implícita). Nenhuma rota 0.0.0.0/0 —
# é isso que mantém o RDS inacessível de fora sem gastar com NAT Gateway.
resource "aws_route_table" "privada" {
  vpc_id = aws_vpc.principal.id

  tags = {
    Name = "${local.prefixo}-rt-privada"
  }
}

resource "aws_route_table_association" "privada" {
  count = var.quantidade_azs

  subnet_id      = aws_subnet.privada[count.index].id
  route_table_id = aws_route_table.privada.id
}
