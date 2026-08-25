# ---------------------------------------------------------------------------
# Variáveis de entrada
# Os defaults foram escolhidos pelo critério "menor custo que ainda atende o
# Tech Challenge". Alterar para cima significa gastar mais do orçamento fixo.
# ---------------------------------------------------------------------------

variable "nome_projeto" {
  description = "Prefixo usado no nome de todos os recursos."
  type        = string
  default     = "gearup"
}

variable "tags_padrao" {
  description = "Tags aplicadas a todos os recursos (via default_tags do provider)."
  type        = map(string)
  default = {
    Projeto   = "GearUp"
    Fase      = "2"
    Ambiente  = "lab"
    ManagedBy = "Terraform"
  }
}

# --------------------------------- Rede ------------------------------------

variable "cidr_vpc" {
  description = "Bloco CIDR da VPC."
  type        = string
  default     = "10.0.0.0/16"
}

variable "quantidade_azs" {
  description = "Quantidade de Availability Zones. O EKS exige no mínimo 2."
  type        = number
  default     = 2

  validation {
    condition     = var.quantidade_azs >= 2 && var.quantidade_azs <= 3
    error_message = "O EKS exige ao menos 2 AZs; acima de 3 só aumenta custo sem ganho no lab."
  }
}

# --------------------------------- EKS -------------------------------------

variable "nome_cluster" {
  description = "Nome do cluster EKS."
  type        = string
  default     = "gearup-eks"
}

variable "role_cluster" {
  description = <<-EOT
    Nome da role IAM usada pelo control plane do EKS.

    Deixe VAZIO (padrão) para descoberta automática: o Terraform procura uma
    role com "LabEksClusterRole" no nome — cujo sufixo muda em cada conta de
    lab — e cai para `LabRole` se não encontrar. Preencha apenas se quiser
    forçar uma role específica. Para ver as opções da sua conta:
      aws iam list-roles --query 'Roles[?contains(RoleName, `Lab`)].RoleName'
  EOT
  type        = string
  default     = ""
}

variable "role_nos" {
  description = <<-EOT
    Nome da role IAM usada pelos worker nodes. Vazio (padrão) = descoberta
    automática por "LabEksNodeRole", com fallback para `LabRole`.
  EOT
  type        = string
  default     = ""
}

variable "cidrs_acesso_api_kubernetes" {
  description = <<-EOT
    CIDRs autorizados a falar com o endpoint público da API do Kubernetes.
    Restrinja ao seu IP (ex.: ["203.0.113.10/32"]) sempre que possível.
  EOT
  type        = list(string)
  default     = ["0.0.0.0/0"]
}

variable "tipo_instancia_nos" {
  description = <<-EOT
    Tipo de instância dos worker nodes. O Learner Lab só permite tamanhos de
    `nano` até `large` — qualquer coisa maior é terminada automaticamente.
    t3.medium = US$ 0,0416/h por nó | t3.small = US$ 0,0208/h por nó.
  EOT
  type        = string
  default     = "t3.medium"

  validation {
    condition     = can(regex("^[a-z0-9]+\\.(nano|micro|small|medium|large)$", var.tipo_instancia_nos))
    error_message = "O Learner Lab permite apenas tamanhos nano, micro, small, medium ou large."
  }
}

variable "nos_desejados" {
  description = "Quantidade inicial de worker nodes."
  type        = number
  default     = 2
}

variable "nos_minimos" {
  description = "Quantidade mínima de worker nodes."
  type        = number
  default     = 2
}

variable "nos_maximos" {
  description = <<-EOT
    Quantidade máxima de worker nodes. Mantenha baixo: o Learner Lab limita a
    9 instâncias EC2 simultâneas e 32 vCPU, e 20+ instâncias desativam a conta.
  EOT
  type        = number
  default     = 3

  validation {
    condition     = var.nos_maximos <= 4
    error_message = "Limite de segurança do Learner Lab: no máximo 4 nós."
  }
}

variable "disco_nos_gb" {
  description = "Tamanho do volume EBS de cada nó (máx. 100 GB no lab)."
  type        = number
  default     = 20

  validation {
    condition     = var.disco_nos_gb <= 100
    error_message = "O Learner Lab permite no máximo 100 GB de EBS por volume."
  }
}

variable "instalar_metrics_server" {
  description = <<-EOT
    Instala o addon metrics-server (gratuito). É pré-requisito do HPA:
    sem ele o HPA fica com `<unknown>` nas métricas e não escala.
  EOT
  type        = bool
  default     = true
}

# --------------------------------- RDS -------------------------------------

variable "criar_banco_rds" {
  description = <<-EOT
    Provisiona o PostgreSQL gerenciado (RDS). Coloque `false` para usar o
    PostgreSQL em cluster (manifests em ../k8s/persistencia) e economizar
    ~US$ 0,43/dia.
  EOT
  type        = bool
  default     = true
}

variable "versao_postgres" {
  description = "Versão major do PostgreSQL no RDS (a AWS escolhe o minor mais recente)."
  type        = string
  default     = "17"
}

variable "classe_instancia_banco" {
  description = "Classe da instância RDS. db.t3.micro = US$ 0,018/h (single-AZ)."
  type        = string
  default     = "db.t3.micro"
}

variable "armazenamento_banco_gb" {
  description = "Armazenamento alocado do RDS em GB (mínimo 20 para gp3)."
  type        = number
  default     = 20
}

variable "nome_banco" {
  description = <<-EOT
    Nome do database criado no RDS. Use minúsculas: o PostgreSQL dobra
    identificadores não citados para minúsculo e `GearUp` quebraria a
    connection string.
  EOT
  type        = string
  default     = "gearup"
}

variable "usuario_banco" {
  description = "Usuário master do RDS."
  type        = string
  default     = "gearup"
}

variable "senha_banco" {
  description = <<-EOT
    Senha do usuário master do RDS. Sem default de propósito — informe via
    variável de ambiente (`export TF_VAR_senha_banco=...`) ou em um
    terraform.tfvars NÃO versionado.
  EOT
  type        = string
  sensitive   = true

  validation {
    condition     = length(var.senha_banco) >= 12
    error_message = "Use ao menos 12 caracteres na senha do banco."
  }
}

variable "cidrs_admin_banco" {
  description = <<-EOT
    CIDRs extras liberados na porta 5432 do RDS (ex.: seu IP, para rodar
    migrations ou psql da máquina local). Vazio = somente o cluster acessa.
    Só funciona junto com `banco_publicamente_acessivel = true`.
  EOT
  type        = list(string)
  default     = []
}

variable "banco_publicamente_acessivel" {
  description = <<-EOT
    Expõe o RDS na internet. Mantenha `false`: o padrão é o banco viver em
    subnets privadas, acessível apenas de dentro da VPC.
  EOT
  type        = bool
  default     = false
}
