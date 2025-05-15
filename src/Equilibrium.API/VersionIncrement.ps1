param (
    [string]$projectFile
)

# Verifica se o arquivo existe
if (-not (Test-Path $projectFile)) {
    Write-Error "Arquivo de projeto não encontrado: $projectFile"
    exit 1
}

try {
    # Lê o arquivo do projeto
    $content = Get-Content $projectFile -Raw

    # Verifica se a tag Version existe
    if ($content -notmatch '<Version>([\d\.]+)</Version>') {
        Write-Error "Tag <Version> não encontrada no arquivo do projeto. Adicione a tag <Version>1.0.0</Version> à PropertyGroup."
        exit 1
    }

    # Obtém a versão atual
    $currentVersion = $matches[1]
    Write-Host "Versão atual: $currentVersion"

    # Divide a versão em suas partes
    $versionParts = $currentVersion -split '\.'
    
    # Verifica se tem pelo menos 3 partes (major.minor.build)
    if ($versionParts.Count -lt 3) {
        Write-Host "Formato de versão incompleto, adicionando 0 para build"
        $versionParts += "0"
    }
    
    $major = [int]$versionParts[0]
    $minor = [int]$versionParts[1]
    $build = [int]$versionParts[2]

    # Incrementa a versão de build
    $build++

    # Cria a nova string de versão
    $newVersion = "$major.$minor.$build"
    Write-Host "Nova versão: $newVersion"

    # Substitui a versão no conteúdo
    $newContent = $content -replace '<Version>[\d\.]+</Version>', "<Version>$newVersion</Version>"
    
    # Verifica se existe tag FileVersion
    if ($content -match '<FileVersion>') {
        $newContent = $newContent -replace '<FileVersion>[\d\.]+</FileVersion>', "<FileVersion>$newVersion.0</FileVersion>"
    }
    
    # Verifica se existe tag AssemblyVersion
    if ($content -match '<AssemblyVersion>') {
        $newContent = $newContent -replace '<AssemblyVersion>[\d\.]+\.\*</AssemblyVersion>', "<AssemblyVersion>$newVersion.0</AssemblyVersion>"
        $newContent = $newContent -replace '<AssemblyVersion>[\d\.]+</AssemblyVersion>', "<AssemblyVersion>$newVersion.0</AssemblyVersion>"
    }

    # Escreve o conteúdo de volta no arquivo
    $newContent | Set-Content $projectFile -Force

    Write-Host "Versão incrementada com sucesso para $newVersion"
    exit 0
}
catch {
    Write-Error "Erro ao processar o arquivo: $_"
    exit 1
}