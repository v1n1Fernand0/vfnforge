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

### Instalacao 1-liner
Scripts de bootstrap fazem o download do repo, instalam o template e registram o global tool automaticamente.

**Windows (PowerShell 7+)**
```powershell
irm https://raw.githubusercontent.com/v1n1Fernand0/vfnforge/main/scripts/bootstrap/install.ps1 | iex
```
Opcoes: `-Branch main`, `-Repository <url>`, `-NoCli`.

**Linux/macOS (Bash)**
```bash
curl -sSL https://raw.githubusercontent.com/v1n1Fernand0/vfnforge/main/scripts/bootstrap/install.sh | bash
```
Opcoes: `--branch main`, `--repo <url>`, `--no-cli`.

Depois desse comando voce ja consegue rodar `vfnforge api MinhaApp` (ou apenas `vfnforge api` para usar o assistente) em qualquer terminal.

### Instalacao direta via GitHub (dotnet new install)
Tambem e possivel usar apenas o `dotnet new` para consumir o repo:
- `dotnet new install https://github.com/v1n1Fernand0/vfnforge`
- `dotnet new install https://github.com/v1n1Fernand0/vfnforge::main` (branch especifica)
- `dotnet new uninstall https://github.com/v1n1Fernand0/vfnforge`
## CLI `vfnforge api`
Instalar o template tambem traz o Global Tool `vfnforge`, que foi pensado para deixar o fluxo o mais simples possivel:
```bash
vfnforge api MinhaApp              # cria ./MinhaApp com tudo renomeado
vfnforge api                       # abre um assistente interativo e pergunta o nome
vfnforge api --in-place -n MinhaApp  # usa a pasta atual sem criar subdiretorio
vfnforge api MinhaApp --force      # sobrescreve uma pasta existente (cuidado)
vfnforge api -- --dry-run          # argumentos extras sao repassados ao dotnet new
```
Voce pode simplesmente informar o nome como primeiro argumento (ex.: `vfnforge api MinhaApp`) e o CLI reutiliza o mesmo valor como pasta de saida. Caso nenhum nome seja passado, ele abre um assistente perguntando o nome/diretorio. Para gerar dentro de uma pasta existente, utilize `--in-place` e, se ela ja contiver arquivos, confirme com `--force` (ou responda `y` no modo interativo). O comando `vfnforge api` continua sendo apenas um alias amigavel para `dotnet new vfnforge`, entao todos os parametros da CLI oficial permanecem disponiveis.

## Autenticacao JWT + multi-tenant por configuracao
O projeto gerado ja inclui:
- Autenticacao JWT configurada em `appsettings.*` (secao `Jwt`). Basta alterar `Issuer`, `Audience` e principalmente `SigningKey` (ou apontar `Authority` para seu Identity Provider) e publicar.
- Middleware de resolucao de tenant (`X-Tenant-ID` por padrao) e filtro aplicado aos endpoints `/api`. A lista de tenants fica em `Tenancy:Tenants`.
- Um fluxo completo de dominio → aplicacao → infraestrutura usando a entidade `Tenant`. As mensagens de erro de dominio ficam centralizadas em `src/VFNForge.SaaS.Domain/Abstractions/DomainErrors.cs` e voce pode expandir conforme suas regras.
- Endpoint `/api/tenants` para consultar os tenants configurados e ver os DTOs/servicos em acao.
- `ConnectionStrings:SqlServer` ja esta pronto para apontar para o seu SQL Server; basta ajustar no `appsettings.*` e o `VFNForgeDbContext` sera configurado automaticamente via `AddInfrastructureData`.
- Controller `TenantsController` expõe `GET/POST/PUT` e as operacoes de `activate/deactivate`, tudo protegido por JWT e validando tenant.
- Os tenants definidos em `Tenancy:Tenants` sao semeados automaticamente no banco na primeira execucao, garantindo que o tenant padrao exista.

Fluxo padrao:
1. Gere/obtenha um token JWT contendo o audience configurado e (opcionalmente) o claim `tenant_id`.
2. Envie a requisicao com `Authorization: Bearer <token>` e o header `X-Tenant-ID` correspondente. Caso nao envie o header, o tenant sera resolvido pelo claim ou caira no `DefaultTenantId`.
3. Caso queira aceitar tenants dinamicos, adicione-os em `appsettings.json` (ou use feature flag para desabilitar `RequireKnownTenant`).

Todo middleware/filtro ja esta registrado; mudancas ficam centralizadas nas configuracoes.

## Instalador Windows (.exe)
No diretorio `installers/Windows/VFNForge.Installer/` existe um bootstrapper que baixa o repo oficial (`https://github.com/v1n1Fernand0/vfnforge`), instala o template e registra o global tool automaticamente.

### Como gerar o instalador
```powershell
dotnet publish installers/Windows/VFNForge.Installer/VFNForge.Installer.csproj `
    -c Release -r win-x64 --self-contained true `
    /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true
```
O executavel sai em `installers/Windows/VFNForge.Installer/bin/Release/net10.0/win-x64/publish/VFNForge.Installer.exe`. Distribua esse arquivo para os usuarios Windows ou disponibilize o `.exe` em uma release do GitHub para que o usuario apenas baixe e rode:

```powershell
Invoke-WebRequest https://github.com/v1n1Fernand0/vfnforge/releases/latest/download/VFNForge.Installer.exe -OutFile vfnforge-installer.exe
./vfnforge-installer.exe
```


### O que o instalador faz
- Baixa o zip da branch escolhida (default `main`).
- Executa `dotnet new install` apontando para `templates/vfnforge`.
- Executa `dotnet pack` + `dotnet tool install --global VFNForge.Cli` a partir da pasta `tools/VFNForge.Cli`.
- Exibe mensagem final com `vfnforge api MinhaApp`.

Argumentos disponiveis:
```
VFNForge.Installer.exe [opcoes]
  --branch|-b <nome>   branch/tag do Git (padrao: main)
  --no-cli             pula instalacao do global tool (so instala o template)
  --keep-temp          mantem a pasta temporaria para debug
```
O instalador exige que o .NET SDK 10 esteja presente (mesmo requisito do template).

## Empacotamento e publicacao (NuGet)
1. Gerar o pacote `VFNForge.Templates`:
   - Windows: `dotnet pack .\pack\VFNForge.Templates.csproj -c Release -o .\_out\nuget`
   - Linux/macOS: `dotnet pack ./pack/VFNForge.Templates.csproj -c Release -o ./_out/nuget`
2. Publicar no NuGet.org (ou feed privado):
   - `dotnet nuget push _out/nuget/VFNForge.Templates.<versao>.nupkg --api-key <TOKEN> --source https://api.nuget.org/v3/index.json`
3. Consumir: `dotnet new install VFNForge.Templates` e `dotnet new vfnforge -n MinhaApp`.

O template gera uma `.slnx`, aplica `net10.0` via `Directory.Build.props`, inclui `.gitignore`, `.editorconfig` e `global.json`, e ja ignora bin/obj/.vs/_out etc. durante a instalacao (`template.json`).

## Ferramenta global opcional (`vfnforge`)
O diretorio `tools/VFNForge.Cli` contem um console app configurado como .NET Global Tool. Ele fornece o comando amigavel `vfnforge api`, que internamente chama `dotnet new vfnforge` e cuida do nome e da pasta automaticamente.

Para testar localmente:
```powershell
# empacotar o tool
dotnet pack .\tools\VFNForge.Cli\VFNForge.Cli.csproj -c Release -o .\_out\tool
# instalar via source local
dotnet tool install --global VFNForge.Cli --add-source .\_out\tool
vfnforge api MinhaApp
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
7. `dotnet test tests/VFNForge.SaaS.Domain.Tests/VFNForge.SaaS.Domain.Tests.csproj` para ver o exemplo de teste unitario em cima da camada de dominio.
