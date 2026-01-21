[CmdletBinding()]
param(
    [string]$Branch = "main",
    [string]$Repository = "https://github.com/v1n1Fernand0/vfnforge",
    [switch]$NoCli
)

$ErrorActionPreference = "Stop"
$client = [System.Net.Http.HttpClient]::new()
$temp = Join-Path ([System.IO.Path]::GetTempPath()) ("vfnforge-" + [guid]::NewGuid())
New-Item -ItemType Directory -Path $temp | Out-Null
$zipPath = Join-Path $temp "repo.zip"

try {
    $zipUrl = "$Repository/archive/refs/heads/$Branch.zip"
    Write-Host "Baixando $zipUrl ..."
    $response = $client.GetAsync($zipUrl).GetAwaiter().GetResult()
    $response.EnsureSuccessStatusCode()
    $bytes = $response.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult()
    [System.IO.File]::WriteAllBytes($zipPath, $bytes)

    Expand-Archive -Path $zipPath -DestinationPath $temp -Force
    $repoRoot = Get-ChildItem -Path $temp -Directory | Where-Object { $_.Name -like "*vfnforge*" } | Select-Object -First 1
    if (-not $repoRoot) {
        throw "Nao encontrei o repo dentro do zip"
    }

    $templatePath = Join-Path $repoRoot.FullName "templates/vfnforge"
    Write-Host "Instalando template em $templatePath"
    dotnet new install $templatePath --force | Out-Host

    if (-not $NoCli.IsPresent) {
        $packDir = Join-Path $temp "cli-pack"
        New-Item -ItemType Directory -Path $packDir | Out-Null

        $cliProject = Join-Path $repoRoot.FullName "tools/VFNForge.Cli/VFNForge.Cli.csproj"
        if (Test-Path $cliProject) {
            Write-Host "Empacotando CLI..."
            dotnet pack $cliProject -c Release -o $packDir | Out-Host
            Write-Host "Instalando global tool 'vfnforge'"
            dotnet tool uninstall --global VFNForge.Cli 2>$null | Out-Null
            dotnet tool install --global --add-source $packDir VFNForge.Cli --ignore-failed-sources | Out-Host
        } else {
            Write-Warning "CLI nao encontrado, pulando instalacao do tool."
        }
    }

    Write-Host "Tudo pronto! Execute 'vfnforge api -n MinhaApp' para comecar."
}
finally {
    $client.Dispose()
    Remove-Item -Recurse -Force $temp -ErrorAction SilentlyContinue
}
