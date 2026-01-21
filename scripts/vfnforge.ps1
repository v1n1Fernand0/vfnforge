#!/usr/bin/env pwsh
[CmdletBinding()]
param(
    [ValidateSet("install","uninstall","test")]
    [string]$Command = "install",
    [string]$Name = "VFNForgeSample",
    [string]$Output = ""
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot ".." )).Path
$templatePath = Join-Path $repoRoot "templates/vfnforge"
if ([string]::IsNullOrWhiteSpace($Output)) {
    $smokeRoot = Join-Path $repoRoot "_out/smoke"
} else {
    if (-not (Test-Path $Output)) {
        $null = New-Item -ItemType Directory -Path $Output -Force
    }
    $smokeRoot = (Resolve-Path $Output).Path
}

function Install-Template {
    Write-Host "Installing template from $templatePath"
    dotnet new install $templatePath --force | Out-Host
}

function Uninstall-Template {
    Write-Host "Uninstalling template at $templatePath"
    dotnet new uninstall $templatePath | Out-Host
}

function Invoke-SmokeTest {
    Install-Template
    if (Test-Path $smokeRoot) {
        Write-Host "Cleaning existing smoke directory: $smokeRoot"
        Remove-Item -Recurse -Force $smokeRoot
    }
    $null = New-Item -ItemType Directory -Path $smokeRoot -Force
    $appPath = Join-Path $smokeRoot $Name
    Write-Host "Generating sample app $Name in $appPath"
    dotnet new vfnforge -n $Name -o $appPath | Out-Host
    $sln = Join-Path $appPath "$Name.slnx"
    Write-Host "Building $sln"
    dotnet build $sln | Out-Host
    Write-Host "Smoke test complete. Project located at $appPath"
}

switch ($Command) {
    "install" { Install-Template }
    "uninstall" { Uninstall-Template }
    "test" { Invoke-SmokeTest }
}
