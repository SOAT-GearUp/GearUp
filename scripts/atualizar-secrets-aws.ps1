<#
.SYNOPSIS
    Copia as credenciais da sessao atual do AWS Academy Learner Lab para os
    secrets dos 4 repositorios do GearUp (Windows PowerShell 5.1).

.DESCRIPTION
    As credenciais do lab (STS) expiram a cada sessao (max. 4h). Antes de
    rodar qualquer pipeline de deploy, execute este script para atualizar
    AWS_ACCESS_KEY_ID, AWS_SECRET_ACCESS_KEY e AWS_SESSION_TOKEN em:
      SOAT-GearUp/GearUp, gearup-infra-k8s, gearup-infra-db, gearup-lambda-auth

    As credenciais sao lidas de ~/.aws/credentials (perfil default) - cole ali
    o bloco de "AWS Details -> AWS CLI" do lab. Nada e gravado em arquivo
    versionado. Requer o GitHub CLI autenticado (gh auth login) com permissao
    de admin nos repositorios.

.EXAMPLE
    .\scripts\atualizar-secrets-aws.ps1

.EXAMPLE
    # Tambem grava as chaves do Datadog nos repositorios que as usam
    .\scripts\atualizar-secrets-aws.ps1 -DatadogApiKey xxx -DatadogAppKey yyy
#>
param(
    [string]$Organizacao = 'SOAT-GearUp',
    [string]$ArquivoCredenciais = (Join-Path $env:USERPROFILE '.aws\credentials'),
    [string]$DatadogApiKey = '',
    [string]$DatadogAppKey = ''
)

$repositorios = @('GearUp', 'gearup-infra-k8s', 'gearup-infra-db', 'gearup-lambda-auth')

if (-not (Test-Path $ArquivoCredenciais)) {
    Write-Host "Arquivo nao encontrado: $ArquivoCredenciais" -ForegroundColor Red
    Write-Host 'Cole o bloco de AWS Details -> AWS CLI do lab nesse arquivo.' -ForegroundColor Yellow
    exit 1
}

$valores = @{}
$noPerfilDefault = $false
foreach ($linha in Get-Content -LiteralPath $ArquivoCredenciais) {
    $texto = $linha.Trim()
    if ($texto.StartsWith('[')) { $noPerfilDefault = ($texto -eq '[default]'); continue }
    if (-not $noPerfilDefault -or $texto -eq '' -or $texto.StartsWith('#')) { continue }
    $i = $texto.IndexOf('=')
    if ($i -gt 0) { $valores[$texto.Substring(0, $i).Trim().ToLowerInvariant()] = $texto.Substring($i + 1).Trim() }
}

$mapa = [ordered]@{
    'AWS_ACCESS_KEY_ID'     = $valores['aws_access_key_id']
    'AWS_SECRET_ACCESS_KEY' = $valores['aws_secret_access_key']
    'AWS_SESSION_TOKEN'     = $valores['aws_session_token']
}

foreach ($nome in $mapa.Keys) {
    if (-not $mapa[$nome]) {
        Write-Host "Valor ausente no perfil [default]: $nome" -ForegroundColor Red
        exit 1
    }
}

Write-Host 'Validando credenciais com aws sts get-caller-identity...' -ForegroundColor Cyan
aws sts get-caller-identity --query Arn --output text
if ($LASTEXITCODE -ne 0) {
    Write-Host 'Credenciais invalidas ou expiradas. Inicie o lab (Start Lab) e recopie o bloco.' -ForegroundColor Red
    exit 1
}

foreach ($repo in $repositorios) {
    Write-Host "-> $Organizacao/$repo" -ForegroundColor Cyan
    foreach ($nome in $mapa.Keys) {
        # --body evita que o valor passe pelo pipeline/arquivo temporario.
        gh secret set $nome --repo "$Organizacao/$repo" --body $mapa[$nome]
        if ($LASTEXITCODE -ne 0) { Write-Host "   falha ao gravar $nome" -ForegroundColor Red; exit 1 }
    }
    if ($repo -eq 'gearup-infra-k8s') {
        if ($DatadogApiKey) { gh secret set DATADOG_API_KEY --repo "$Organizacao/$repo" --body $DatadogApiKey }
        if ($DatadogAppKey) { gh secret set DATADOG_APP_KEY --repo "$Organizacao/$repo" --body $DatadogAppKey }
    }
}

Write-Host 'Secrets atualizados nos 4 repositorios. Elas expiram junto com a sessao do lab.' -ForegroundColor Green
