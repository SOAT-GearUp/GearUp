# ---------------------------------------------------------------------------
# Versões e backend do state
# ---------------------------------------------------------------------------
terraform {
  required_version = ">= 1.6.0"

  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 6.0"
    }
  }

  # ---------------------------------------------------------------------------
  # BACKEND REMOTO (S3) — deixado comentado de propósito.
  #
  # O projeto roda num AWS Academy Learner Lab, onde o state local é a opção
  # mais segura e barata: o bucket é mais uma peça para dar errado e o
  # DynamoDB de lock cobra à parte. Mantendo local, o `terraform.tfstate` fica
  # em disco (e fora do Git, ver .gitignore).
  #
  # Para habilitar o backend remoto:
  #   1) Crie o bucket UMA VEZ, fora deste Terraform (nome global e único):
  #        aws s3api create-bucket --bucket gearup-tfstate-<seu-sufixo> --region us-east-1
  #        aws s3api put-bucket-versioning --bucket gearup-tfstate-<seu-sufixo> \
  #          --versioning-configuration Status=Enabled
  #   2) Descomente o bloco abaixo, ajustando o `bucket`.
  #   3) Rode `terraform init -migrate-state` para enviar o state atual ao S3.
  #
  # `use_lockfile = true` usa o lock nativo do S3 (provider AWS >= 6.x) e
  # dispensa a tabela do DynamoDB.
  # ---------------------------------------------------------------------------
  # backend "s3" {
  #   bucket       = "gearup-tfstate-SUBSTITUA"
  #   key          = "fase2/terraform.tfstate"
  #   region       = "us-east-1"
  #   encrypt      = true
  #   use_lockfile = true
  # }
}
