# VFNForge

VFNForge é um template SaaS opinativo para .NET 10. Ele entrega uma solução completa (Api, Application, Domain, Infrastructure, Contracts) com JWT + multi-tenant configurados, EF Core pronto para SQL Server, scripts cross-platform e um CLI (`vfnforge`) para gerar projetos em poucos segundos.

## Comece agora

### Windows (PowerShell 7+)
```powershell
irm https://raw.githubusercontent.com/v1n1Fernand0/vfnforge/main/scripts/bootstrap/install.ps1 | iex
vfnforge api MinhaEmpresa
```

### Linux/macOS (Bash)
```bash
curl -sSL https://raw.githubusercontent.com/v1n1Fernand0/vfnforge/main/scripts/bootstrap/install.sh | bash
vfnforge api MinhaEmpresa
```

Pronto! O comando `vfnforge api` pergunta o nome se você não passar argumento e cria uma pasta já com solution `.slnx`, projetos, JWT/Tenant configurados e todo o pipeline pronto.

## Pré-requisitos
- .NET SDK 10.0.100 (há um `global.json` no template fixando esta versão).
- PowerShell 7+ ou Bash (se preferir rodar os scripts; todos os comandos funcionam manualmente).

## Instalação local (repo clonado)
Se você já clonou este repositório:

### PowerShell
```powershell
pwsh ./scripts/vfnforge.ps1 install      # registra o template
pwsh ./scripts/vfnforge.ps1 test MinhaApp # gera, compila e apaga um smoke test
pwsh ./scripts/vfnforge.ps1 uninstall    # remove o template local
```

### Bash
```bash
bash ./scripts/vfnforge.sh install
bash ./scripts/vfnforge.sh test MinhaApp
bash ./scripts/vfnforge.sh uninstall
```

Ou rode manualmente:
```powershell
dotnet new install .\templates\vfnforge
vfnforge api MinhaApp
```

## CLI `vfnforge`
Instalar o template também instala o Global Tool `vfnforge`.

```bash
vfnforge api MinhaApp               # usa o nome como diretório por padrão
vfnforge api                        # modo interativo (pergunta nome e pasta)
vfnforge api --in-place -n MinhaApp # gera na pasta atual
vfnforge api MinhaApp --force       # sobrescreve diretório existente
vfnforge api -- --dry-run           # passa argumentos direto para o dotnet new
```

Se você preferir não instalar o tool, basta chamar `dotnet new vfnforge` com os mesmos parâmetros.

## Outras formas de instalação
- **dotnet new direto do GitHub**  
  `dotnet new install https://github.com/v1n1Fernand0/vfnforge`  
  (use `::main` para fixar a branch ou `dotnet new uninstall ...` para remover)

- **Instalador Windows (.exe)**  
  Gere com:
  ```powershell
  dotnet publish installers/Windows/VFNForge.Installer/VFNForge.Installer.csproj `
      -c Release -r win-x64 --self-contained true `
      /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true
  ```
  Distribua o `VFNForge.Installer.exe` gerado. Ele baixa este repo, instala o template, registra o CLI e mostra `vfnforge api MinhaApp` como próximo passo. Argumentos: `--branch <nome>`, `--no-cli`, `--keep-temp`.

- **Fluxo NuGet**  
  1. `dotnet pack ./pack/VFNForge.Templates.csproj -c Release -o ./_out/nuget`  
  2. `dotnet nuget push _out/nuget/VFNForge.Templates.<versao>.nupkg --api-key <TOKEN> --source https://api.nuget.org/v3/index.json`  
  3. Usuários finais instalam com `dotnet new install VFNForge.Templates` e geram com `vfnforge api MinhaApp`.

## O que vem pronto no template
- **JWT** configurado (Issuer/Audience/SigningKey em `appsettings.*`). Basta trocar as credenciais ou apontar `Authority`.
- **Resolução multi-tenant** via header `X-Tenant-ID` ou claim `tenant_id`, com middleware + endpoint filter.
- **Entidade Tenant** atravessando Domain → Application → Infrastructure, erros centralizados em `Domain/Common/DomainErrors.cs`.
- **Controllers prontos** (`/api/tenants` com GET/POST/PUT/activate/deactivate) protegidos por JWT.
- **EF Core + SQL Server** (`VFNForgeDbContext`, `TenantConfiguration`, seeding automático baseado em `Tenancy:Tenants` e `DefaultTenantId`).
- **Scripts cross-platform** (`scripts/vfnforge.(ps1|sh)` + bootstrap 1-liner).
- **Arquivos essenciais** (`.slnx`, `.gitignore`, `.editorconfig`, `global.json`, `Directory.Build.props` com `net10.0`).

## Estrutura do repositório
- `templates/vfnforge/` – conteúdo do template (solution, projetos, `.template.config`).
- `scripts/` – instalação/desinstalação/teste automáticos e bootstraps (curl/irm).
- `pack/VFNForge.Templates.csproj` – projeto usado para gerar o pacote NuGet.
- `tools/VFNForge.Cli/` – código do Global Tool (alias `vfnforge api`).
- `installers/Windows/` – instalador self-contained (.exe).
- `_out/` – pasta de artefatos temporários/smoke-tests (é ignorada por git/template).

## Publicar o template pack
1. Atualize versão/metadados em `pack/VFNForge.Templates.csproj`.
2. `dotnet pack ./pack/VFNForge.Templates.csproj -c Release -o ./_out/nuget`.
3. Publique (`dotnet nuget push ...`) no feed desejado.
4. Divulgue: `dotnet new install VFNForge.Templates` + `vfnforge api MinhaEmpresa`.

## Ferramenta global (`vfnforge`)
`tools/VFNForge.Cli` contém o console app configurado como Global Tool. Para testar localmente:

```powershell
dotnet pack .\tools\VFNForge.Cli\VFNForge.Cli.csproj -c Release -o .\_out\tool
dotnet tool install --global VFNForge.Cli --add-source .\_out\tool
vfnforge api MinhaApp
vfnforge --version
```

## Checklist de validação
1. `dotnet new uninstall VFNForge.Templates` (garante ambiente limpo).
2. `dotnet new install ./templates/vfnforge`.
3. `vfnforge api SmokeTest -o _out/smoke/SmokeTest`.
4. `dotnet build _out/smoke/SmokeTest/SmokeTest.slnx`.
5. (Opcional) `bash|pwsh ./scripts/vfnforge.(sh|ps1) test`.
6. `dotnet pack pack/VFNForge.Templates.csproj -c Release`.
7. `dotnet test templates/vfnforge/tests/VFNForge.SaaS.Domain.Tests/VFNForge.SaaS.Domain.Tests.csproj`.
