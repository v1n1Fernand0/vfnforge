# VFNForge

VFNForge e um template SaaS opinativo construido sobre .NET 10, com arquitetura em camadas (Api, Application, Domain, Infrastructure, Contracts) e pensado para ser instalado tanto via repositorio local quanto como pacote NuGet, funcionando igualmente em Windows, Linux e macOS.

## Estrutura do repositorio
- `templates/vfnforge/` - payload do template (solution, projetos e `.template.config`).
- `pack/VFNForge.Templates.csproj` - projeto de empacotamento NuGet do template.
- `scripts/` - automacoes cross-platform para instalar/desinstalar/testar o template localmente.
- `tools/VFNForge.Cli/` - ferramenta global opcional (`vfnforge`) que delega para `dotnet new vfnforge`.
- `_out/` - area de artefatos temporarios/smoke tests (ignorada).

## Pre-requisitos
- .NET SDK 10.0.100 (fixado em `templates/vfnforge/global.json`).
- PowerShell 7+ ou Bash para executar os scripts (todos os comandos podem ser rodados manualmente se preferir).

## Fluxo local do template
### Windows (PowerShell)
```powershell
# instalar a partir do repositorio clonado
pwsh ./scripts/vfnforge.ps1 install

# gerar e compilar um app de exemplo
pwsh ./scripts/vfnforge.ps1 test MinhaApp

# desinstalar quando terminar
pwsh ./scripts/vfnforge.ps1 uninstall
```
Ou rode os comandos manualmente:
```powershell
dotnet new install .\templates\vfnforge
dotnet new vfnforge -n MinhaApp
Get-ChildItem MinhaApp
```

### Linux/macOS (Bash)
```bash
# instalar
bash ./scripts/vfnforge.sh install

# smoke test
bash ./scripts/vfnforge.sh test MinhaApp

# desinstalar
bash ./scripts/vfnforge.sh uninstall
```
Comandos equivalentes:
```bash
dotnet new install ./templates/vfnforge
dotnet new vfnforge -n MinhaApp
dotnet build MinhaApp/MinhaApp.slnx
```

### Instalacao direta via GitHub (sem clonar)
Desde o .NET 8, `dotnet new install` consegue consumir um repo Git diretamente. Se preferir um comando pronto (sem clonar manualmente), use:
\- `dotnet new install https://github.com/vinic/vfnforge`

Isso baixa a ultima versao do template direto do GitHub e ja deixa o `vfnforge` disponivel.

Se quiser controlar branch/versao especifica (ex: `main`):
\- `dotnet new install https://github.com/vinic/vfnforge::main`

Quando precisar remover:
\- `dotnet new uninstall https://github.com/vinic/vfnforge`

Se estiver em um ambiente sem suporte ao fluxo acima, ha tambem um comando one-liner que clona para uma pasta temporaria, instala o template e limpa o cache:

**PowerShell**
```powershell
pwsh -Command "$tmp = Join-Path $env:TEMP ('vfnforge-' + [guid]::NewGuid()); git clone https://github.com/vinic/vfnforge $tmp --depth 1; dotnet new install (Join-Path $tmp 'templates/vfnforge'); Remove-Item $tmp -Recurse -Force"
```

**Bash**
```bash
tmp=$(mktemp -d); git clone https://github.com/vinic/vfnforge "$tmp" --depth 1 && dotnet new install "$tmp/templates/vfnforge" && rm -rf "$tmp"
```

Com isso, qualquer terminal consegue instalar o template diretamente sem precisar clonar o repo manualmente; depois basta executar `dotnet new vfnforge -n MinhaApp`.

## Empacotamento e publicacao (NuGet)
1. Gerar o pacote `VFNForge.Templates`:
   - Windows: `dotnet pack .\pack\VFNForge.Templates.csproj -c Release -o .\_out\nuget`
   - Linux/macOS: `dotnet pack ./pack/VFNForge.Templates.csproj -c Release -o ./_out/nuget`
2. Publicar no NuGet.org (ou feed privado):
   - `dotnet nuget push _out/nuget/VFNForge.Templates.<versao>.nupkg --api-key <TOKEN> --source https://api.nuget.org/v3/index.json`
3. Consumir: `dotnet new install VFNForge.Templates` e `dotnet new vfnforge -n MinhaApp`.

O template gera uma `.slnx`, aplica `net10.0` via `Directory.Build.props`, inclui `.gitignore`, `.editorconfig` e `global.json`, e ja ignora bin/obj/.vs/_out etc. durante a instalacao (`template.json`).

## Ferramenta global opcional (`vfnforge`)
O diretorio `tools/VFNForge.Cli` contem um console app configurado como .NET Global Tool. Ele apenas repassa `vfnforge new -n MinhaApp` para `dotnet new vfnforge`, mas melhora a descoberta.

Para testar localmente:
```powershell
# empacotar o tool
dotnet pack .\tools\VFNForge.Cli\VFNForge.Cli.csproj -c Release -o .\_out\tool
# instalar via source local
dotnet tool install --global VFNForge.Cli --add-source .\_out\tool
vfnforge new -n MinhaApp
vfnforge --version
```
Publicar o tool e opcional; o template ja entrega o fluxo principal.

## Checklist de validacao
1. `dotnet new uninstall VFNForge.Templates` (caso ja exista).
2. `dotnet new install ./templates/vfnforge`.
3. `dotnet new vfnforge -n SmokeTest -o _out/smoke/SmokeTest`.
4. `dotnet build _out/smoke/SmokeTest/SmokeTest.slnx`.
5. (Opcional) `bash|pwsh ./scripts/vfnforge.(sh|ps1) test` automatiza tudo.
6. `dotnet pack pack/VFNForge.Templates.csproj -c Release` para garantir o pacote NuGet.
