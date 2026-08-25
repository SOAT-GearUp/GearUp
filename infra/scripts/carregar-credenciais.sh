#!/usr/bin/env bash
# ---------------------------------------------------------------------------
# Carrega as credenciais temporárias do AWS Academy Learner Lab a partir de
# infra/.env.aws (equivalente ao carregar-credenciais.ps1, para Linux/macOS).
#
# Precisa ser SOURCED para as variáveis persistirem no seu shell:
#   source ./scripts/carregar-credenciais.sh
#
# Aceita tanto AWS_ACCESS_KEY_ID=... quanto o bloco original do lab
# (aws_access_key_id=..., com ou sem a linha [default]).
# ---------------------------------------------------------------------------

_gearup_diretorio="$(cd "$(dirname "${BASH_SOURCE[0]:-$0}")" && pwd)"
_gearup_arquivo="${1:-$_gearup_diretorio/../.env.aws}"

if [ ! -f "$_gearup_arquivo" ]; then
  echo "Arquivo não encontrado: $_gearup_arquivo" >&2
  echo "Crie-o a partir do modelo: cp .env.aws.example .env.aws" >&2
  echo "Depois cole o bloco de AWS Details -> AWS CLI no lab." >&2
  return 1 2>/dev/null || exit 1
fi

_gearup_exportadas=""

while IFS= read -r _gearup_linha || [ -n "$_gearup_linha" ]; do
  # Remove espaços das pontas e ignora vazias, comentários e [default].
  _gearup_linha="$(printf '%s' "$_gearup_linha" | sed -e 's/^[[:space:]]*//' -e 's/[[:space:]]*$//')"
  case "$_gearup_linha" in
    ''|'#'*|'['*) continue ;;
  esac

  _gearup_chave="${_gearup_linha%%=*}"
  _gearup_valor="${_gearup_linha#*=}"
  [ "$_gearup_chave" = "$_gearup_linha" ] && continue

  # Tira espaços e aspas do valor.
  _gearup_valor="$(printf '%s' "$_gearup_valor" | sed -e 's/^[[:space:]]*//' -e 's/[[:space:]]*$//' -e 's/^"//' -e 's/"$//' -e "s/^'//" -e "s/'$//")"

  # Normaliza os nomes do bloco do lab.
  case "$_gearup_chave" in
    aws_access_key_id)     _gearup_chave="AWS_ACCESS_KEY_ID" ;;
    aws_secret_access_key) _gearup_chave="AWS_SECRET_ACCESS_KEY" ;;
    aws_session_token)     _gearup_chave="AWS_SESSION_TOKEN" ;;
    region)                _gearup_chave="AWS_DEFAULT_REGION" ;;
  esac

  # Ignora placeholders não substituídos.
  case "$_gearup_valor" in
    ''|SUBSTITUA*|ASIA_SUBSTITUA*) continue ;;
  esac

  export "$_gearup_chave=$_gearup_valor"
  _gearup_exportadas="$_gearup_exportadas $_gearup_chave"
done < "$_gearup_arquivo"

_gearup_faltando=""
for _gearup_obrigatoria in AWS_ACCESS_KEY_ID AWS_SECRET_ACCESS_KEY AWS_SESSION_TOKEN; do
  eval "_gearup_atual=\${$_gearup_obrigatoria:-}"
  [ -z "$_gearup_atual" ] && _gearup_faltando="$_gearup_faltando $_gearup_obrigatoria"
done

if [ -n "$_gearup_faltando" ]; then
  echo "Valores ausentes ou não substituídos em $_gearup_arquivo:$_gearup_faltando" >&2
  echo "Recopie o bloco de AWS Details -> AWS CLI (o lab precisa estar iniciado)." >&2
  return 1 2>/dev/null || exit 1
fi

: "${AWS_DEFAULT_REGION:=us-east-1}"
export AWS_DEFAULT_REGION

# Grava ~/.aws/credentials para funcionar também em outros terminais.
mkdir -p "$HOME/.aws"
umask 077
cat > "$HOME/.aws/credentials" <<EOF
[default]
aws_access_key_id=$AWS_ACCESS_KEY_ID
aws_secret_access_key=$AWS_SECRET_ACCESS_KEY
aws_session_token=$AWS_SESSION_TOKEN
EOF

cat > "$HOME/.aws/config" <<EOF
[default]
region = $AWS_DEFAULT_REGION
output = json
EOF

echo "Credenciais carregadas (região $AWS_DEFAULT_REGION)."

# Configuração da infra (senha_banco etc.) vem do terraform.tfvars, não daqui.
if [ ! -f "$_gearup_diretorio/../terraform.tfvars" ]; then
  echo "Lembrete: terraform.tfvars ainda não existe (copie de terraform.tfvars.example e defina senha_banco)."
fi

echo "Validando com aws sts get-caller-identity..."

if aws sts get-caller-identity; then
  echo "Sessão válida. Próximo passo: terraform init && terraform plan"
else
  echo "Falha na validação. Se aparecer ExpiredToken: Start Lab e recopie o bloco." >&2
fi

unset _gearup_diretorio _gearup_arquivo _gearup_linha _gearup_chave _gearup_valor
unset _gearup_faltando _gearup_obrigatoria _gearup_atual _gearup_exportadas
