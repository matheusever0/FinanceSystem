# Script para baixar e configurar as bibliotecas JavaScript
# Para executar, abra o PowerShell como Administrador na pasta raiz do projeto

# Configuração de variáveis
$projectRoot = Get-Location
$wwwrootPath = Join-Path -Path $projectRoot -ChildPath "src\FinanceSystem.Web\wwwroot"
$libPath = Join-Path -Path $wwwrootPath -ChildPath "lib"
$tempPath = Join-Path -Path $projectRoot -ChildPath "temp"

Write-Host "Configurando bibliotecas para o projeto FinanceSystem" -ForegroundColor Cyan
Write-Host "Diretório wwwroot: $wwwrootPath" -ForegroundColor Yellow

# Criar diretórios necessários
if (-not (Test-Path $tempPath)) {
    New-Item -Path $tempPath -ItemType Directory | Out-Null
}

if (-not (Test-Path $libPath)) {
    New-Item -Path $libPath -ItemType Directory | Out-Null
}

# 1. Bootstrap 5.3.0
Write-Host "Baixando Bootstrap 5.3.0..." -ForegroundColor Green
$bootstrapUrl = "https://github.com/twbs/bootstrap/releases/download/v5.3.0/bootstrap-5.3.0-dist.zip"
$bootstrapZip = Join-Path -Path $tempPath -ChildPath "bootstrap.zip"

Invoke-WebRequest -Uri $bootstrapUrl -OutFile $bootstrapZip

$bootstrapDir = Join-Path -Path $libPath -ChildPath "bootstrap"
if (-not (Test-Path $bootstrapDir)) {
    New-Item -Path $bootstrapDir -ItemType Directory | Out-Null
    New-Item -Path "$bootstrapDir\css" -ItemType Directory | Out-Null
    New-Item -Path "$bootstrapDir\js" -ItemType Directory | Out-Null
}

Expand-Archive -Path $bootstrapZip -DestinationPath $tempPath -Force
Copy-Item -Path "$tempPath\bootstrap-5.3.0-dist\css\bootstrap.min.css" -Destination "$bootstrapDir\css\" -Force
Copy-Item -Path "$tempPath\bootstrap-5.3.0-dist\js\bootstrap.bundle.min.js" -Destination "$bootstrapDir\js\" -Force

# 2. Font Awesome 6.4.0
Write-Host "Baixando Font Awesome 6.4.0..." -ForegroundColor Green
$fontAwesomeUrl = "https://use.fontawesome.com/releases/v6.4.0/fontawesome-free-6.4.0-web.zip"
$fontAwesomeZip = Join-Path -Path $tempPath -ChildPath "fontawesome.zip"

Invoke-WebRequest -Uri $fontAwesomeUrl -OutFile $fontAwesomeZip

$fontAwesomeDir = Join-Path -Path $libPath -ChildPath "font-awesome"
if (-not (Test-Path $fontAwesomeDir)) {
    New-Item -Path $fontAwesomeDir -ItemType Directory | Out-Null
    New-Item -Path "$fontAwesomeDir\css" -ItemType Directory | Out-Null
    New-Item -Path "$fontAwesomeDir\webfonts" -ItemType Directory | Out-Null
}

Expand-Archive -Path $fontAwesomeZip -DestinationPath $tempPath -Force
Copy-Item -Path "$tempPath\fontawesome-free-6.4.0-web\css\all.min.css" -Destination "$fontAwesomeDir\css\" -Force
Copy-Item -Path "$tempPath\fontawesome-free-6.4.0-web\webfonts\*" -Destination "$fontAwesomeDir\webfonts\" -Force

# 3. jQuery 3.6.0
Write-Host "Baixando jQuery 3.6.0..." -ForegroundColor Green
$jqueryUrl = "https://code.jquery.com/jquery-3.6.0.min.js"
$jqueryDir = Join-Path -Path $libPath -ChildPath "jquery"

if (-not (Test-Path $jqueryDir)) {
    New-Item -Path $jqueryDir -ItemType Directory | Out-Null
}

Invoke-WebRequest -Uri $jqueryUrl -OutFile "$jqueryDir\jquery.min.js"

# 4. jQuery Validate 1.19.3 e jQuery Validation Unobtrusive 3.2.12
Write-Host "Baixando jQuery Validation..." -ForegroundColor Green
$jqueryValidateUrl = "https://cdnjs.cloudflare.com/ajax/libs/jquery-validate/1.19.3/jquery.validate.min.js"
$jqueryUnobtrusiveUrl = "https://cdnjs.cloudflare.com/ajax/libs/jquery-validation-unobtrusive/3.2.12/jquery.validate.unobtrusive.min.js"
$jqueryValidationDir = Join-Path -Path $libPath -ChildPath "jquery-validation"

if (-not (Test-Path $jqueryValidationDir)) {
    New-Item -Path $jqueryValidationDir -ItemType Directory | Out-Null
}

Invoke-WebRequest -Uri $jqueryValidateUrl -OutFile "$jqueryValidationDir\jquery.validate.min.js"
Invoke-WebRequest -Uri $jqueryUnobtrusiveUrl -OutFile "$jqueryValidationDir\jquery.validate.unobtrusive.min.js"

# Limpeza
Write-Host "Limpando arquivos temporários..." -ForegroundColor Green
Remove-Item -Path $tempPath -Recurse -Force

Write-Host "Todas as bibliotecas foram baixadas e configuradas com sucesso!" -ForegroundColor Cyan
Write-Host "Por favor, atualize as referências nos arquivos .cshtml conforme o guia fornecido." -ForegroundColor Yellow