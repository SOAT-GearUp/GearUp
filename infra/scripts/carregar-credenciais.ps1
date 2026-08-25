<#
.SYNOPSIS
    Carrega as credenciais temporárias do AWS Academy Learner Lab a partir de
    infra/.env.aws.

.DESCRIPTION
    Lê o arquivo, exporta as variáveis na sessão atual do PowerShell, grava
    ~/.aws/credentials (para que kubectl, aws e terraform funcionem em qualquer
    terminal) e valida com `aws sts get-caller-identity`.

    Aceita tanto o formato MAIÚSCULO (AWS_ACCESS_KEY_ID=...) quanto o bloco
    original copiado do lab (aws_access_key_id=..., com ou sem [default]).

.EXAMPLE
    # Precisa ser DOT-SOURCED para as variáveis persistirem no seu terminal:
    . .\scripts\carregar-credenciais.ps1

.EXAMPLE
    . .\scripts\carregar-credenciais.ps1 -Arquivo C:\caminho\outro.env
#>
param(
    [string]$Arquivo = (Join-Path $PSScriptRoot '..\.env.aws')
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path $Arquivo)) {
    Write-Host "Arquivo nao encontrado: $Arquivo" -ForegroundColor Red
    Write-Host "Crie-o a partir do modelo:" -ForegroundColor Yellow
    Write-Host "  Copy-Item .env.aws.example .env.aws" -ForegroundColor Yellow
    Write-Host "Depois cole o bloco de AWS Details -> AWS CLI no lab." -ForegroundColor Yellow
    return
}

# Nomes usados pelo bloco do lab -> variaveis de ambiente equivalentes.
$equivalencias = @{
    'aws_access_key_id'     = 'AWS_ACCESS_KEY_ID'
    'aws_secret_access_key' = 'AWS_SECRET_ACCESS_KEY'
    'aws_session_token'     = 'AWS_SESSION_TOKEN'
    'region'                = 'AWS_DEFAULT_REGION'
}

$valores = @{}

foreach ($linha in Get-Content -LiteralPath $Arquivo) {
    $texto = $linha.Trim()

    # Ignora vazias, comentarios e o cabecalho de perfil ([default]).
    if ($texto -eq '' -or $texto.StartsWith('#') -or $texto.StartsWith('[')) { continue }

    $separador = $texto.IndexOf('=')
    if ($separador -lt 1) { continue }

    $chave = $texto.Substring(0, $separador).Trim()
    $valor = $texto.Substring($separador + 1).Trim().Trim('"').Trim("'")

    if ($equivalencias.ContainsKey($chave.ToLowerInvariant())) {
        $chave = $equivalencias[$chave.ToLowerInvariant()]
    }

    if ($valor -eq '' -or $valor -like 'SUBSTITUA*' -or $valor -like 'ASIA_SUBSTITUA*') { continue }

    $valores[$chave] = $valor
}

$obrigatorias = @('AWS_ACCESS_KEY_ID', 'AWS_SECRET_ACCESS_KEY', 'AWS_SESSION_TOKEN')
$faltando = $obrigatorias | Where-Object { -not $valores.ContainsKey($_) }

if ($faltando.Count -gt 0) {
    Write-Host "Valores ausentes ou nao substituidos em $Arquivo :" -ForegroundColor Red
    $faltando | ForEach-Object { Write-Host "  - $_" -ForegroundColor Red }
    Write-Host "Recopie o bloco de AWS Details -> AWS CLI (o lab precisa estar iniciado)." -ForegroundColor Yellow
    return
}

if (-not $valores.ContainsKey('AWS_DEFAULT_REGION')) {
    $valores['AWS_DEFAULT_REGION'] = 'us-east-1'
}

# 1) Exporta na sessao atual (AWS_*, TF_VAR_* e afins).
foreach ($chave in $valores.Keys) {
    Set-Item -Path "env:$chave" -Value $valores[$chave]
}

# 2) Grava ~/.aws/credentials para funcionar tambem em outros terminais e no
#    kubectl/aws CLI sem depender das variaveis de ambiente.
$pastaAws = Join-Path $env:USERPROFILE '.aws'
if (-not (Test-Path $pastaAws)) {
    New-Item -ItemType Directory -Path $pastaAws | Out-Null
}

$conteudoCredenciais = @"
[default]
aws_access_key_id=$($valores['AWS_ACCESS_KEY_ID'])
aws_secret_access_key=$($valores['AWS_SECRET_ACCESS_KEY'])
aws_session_token=$($valores['AWS_SESSION_TOKEN'])
"@

$conteudoConfig = @"
[default]
region = $($valores['AWS_DEFAULT_REGION'])
output = json
"@

Set-Content -Path (Join-Path $pastaAws 'credentials') -Value $conteudoCredenciais -Encoding ascii
Set-Content -Path (Join-Path $pastaAws 'config') -Value $conteudoConfig -Encoding ascii

Write-Host "Credenciais carregadas (regiao $($valores['AWS_DEFAULT_REGION']))." -ForegroundColor Green

# Configuracao da infra (senha_banco etc.) vem do terraform.tfvars, nao daqui.
if (-not (Test-Path (Join-Path $PSScriptRoot '..\terraform.tfvars'))) {
    Write-Host "Lembrete: terraform.tfvars ainda nao existe (copie de terraform.tfvars.example e defina senha_banco)." -ForegroundColor Yellow
}

# 3) Valida de fato contra a AWS.
Write-Host "Validando com aws sts get-caller-identity..." -ForegroundColor Cyan
$identidade = aws sts get-caller-identity 2>&1

if ($LASTEXITCODE -eq 0) {
    Write-Host $identidade
    Write-Host "Sessao valida. Proximo passo: terraform init; terraform plan" -ForegroundColor Green
} else {
    Write-Host $identidade -ForegroundColor Red
    Write-Host "Falha na validacao. Se aparecer ExpiredToken, o lab precisa ser reiniciado (Start Lab) e o bloco recopiado." -ForegroundColor Yellow
}
