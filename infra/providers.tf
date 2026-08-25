# ---------------------------------------------------------------------------
# Provider AWS
#
# A região é fixa em us-east-1 (e não parametrizada) porque o Learner Lab só
# libera us-east-1 e us-west-2, e o key pair `vockey` existe apenas em
# us-east-1. As credenciais vêm de variáveis de ambiente exportadas a partir de
# "AWS Details -> AWS CLI" no lab (incluindo AWS_SESSION_TOKEN) ou de
# ~/.aws/credentials — nunca de arquivos .tf/.tfvars versionados.
# ---------------------------------------------------------------------------
provider "aws" {
  region = "us-east-1"

  # Tags aplicadas automaticamente a todo recurso que suporta tagging.
  # Facilitam achar recursos órfãos no Resource Groups & Tag Editor caso o
  # state seja perdido.
  default_tags {
    tags = var.tags_padrao
  }
}
