using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http;
using System.Linq;

namespace VFNForge.Installer;

internal sealed record InstallOptions(string Branch, bool InstallCli, bool KeepTemp)
{
    public static InstallOptions Parse(string[] args)
    {
        var branch = "main";
        var installCli = true;
        var keepTemp = false;

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--branch":
                case "-b":
                    if (i + 1 >= args.Length)
                    {
                        throw new ArgumentException("Missing value for --branch");
                    }

                    branch = args[++i];
                    break;
                case "--no-cli":
                    installCli = false;
                    break;
                case "--keep-temp":
                    keepTemp = true;
                    break;
                case "--help":
                case "-h":
                    PrintUsage();
                    Environment.Exit(0);
                    break;
                default:
                    throw new ArgumentException($"Argumento desconhecido: {args[i]}");
            }
        }

        return new InstallOptions(branch, installCli, keepTemp);
    }

    private static void PrintUsage()
    {
        Console.WriteLine("Instalador VFNForge");
        Console.WriteLine("Argumentos opcionais:");
        Console.WriteLine("  --branch|-b <nome>   => branch/tag do repo (default: main)");
        Console.WriteLine("  --no-cli             => nao instala o global tool (apenas template)");
        Console.WriteLine("  --keep-temp          => mantem arquivos temporarios para debug");
    }
}

internal static class Program
{
    private const string RepoOwner = "v1n1Fernand0";
    private const string RepoName = "vfnforge";

    private static readonly HttpClient Http = new();

    private static async Task<int> Main(string[] args)
    {
        InstallOptions options;
        try
        {
            options = InstallOptions.Parse(args);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }

        var tempRoot = Path.Combine(Path.GetTempPath(), $"VFNForgeInstaller-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempRoot);

        try
        {
            var zipUrl = $"https://github.com/{RepoOwner}/{RepoName}/archive/refs/heads/{options.Branch}.zip";
            var zipPath = Path.Combine(tempRoot, "repo.zip");

            Console.WriteLine($"Baixando {zipUrl}...");
            await DownloadAsync(zipUrl, zipPath);
            Console.WriteLine("Extrair arquivos...");
            var repoPath = ExtractRepository(tempRoot, zipPath);

            await InstallTemplateAsync(repoPath);

            if (options.InstallCli)
            {
                await InstallCliAsync(repoPath, tempRoot);
            }

            Console.WriteLine();
            Console.WriteLine("VFNForge instalado com sucesso!");
            Console.WriteLine("Use: vfnforge api -n MinhaApp");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Erro durante instalacao: {ex.Message}");
            return 1;
        }
        finally
        {
            if (!options.KeepTemp)
            {
                TryDelete(tempRoot);
            }
        }
    }

    private static async Task InstallTemplateAsync(string repoPath)
    {
        var templatePath = Path.Combine(repoPath, "templates", "vfnforge");
        if (!Directory.Exists(templatePath))
        {
            throw new DirectoryNotFoundException($"Nao encontrei o template em {templatePath}");
        }

        Console.WriteLine("Instalando template (dotnet new install)...");
        await RunDotNetAsync(new[] { "new", "install", templatePath, "--force" });
    }

    private static async Task InstallCliAsync(string repoPath, string tempRoot)
    {
        var cliProject = Path.Combine(repoPath, "tools", "VFNForge.Cli", "VFNForge.Cli.csproj");
        if (!File.Exists(cliProject))
        {
            Console.WriteLine("CLI nao encontrado, pulando instalacao do tool.");
            return;
        }

        var packDir = Path.Combine(tempRoot, "cli-pack");
        Directory.CreateDirectory(packDir);

        Console.WriteLine("Empacotando CLI...");
        await RunDotNetAsync(new[]
        {
            "pack", cliProject, "-c", "Release", "-o", packDir
        });

        var package = Directory.GetFiles(packDir, "VFNForge.Cli*.nupkg")
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .FirstOrDefault();

        if (package is null)
        {
            throw new InvalidOperationException("Nao achei o pacote do CLI depois do pack.");
        }

        Console.WriteLine("Instalando global tool 'vfnforge'...");
        await RunDotNetAsync(new[] { "tool", "uninstall", "--global", "VFNForge.Cli" }, ignoreErrors: true);
        await RunDotNetAsync(new[]
        {
            "tool", "install", "--global", "--add-source", packDir, "VFNForge.Cli", "--ignore-failed-sources"
        });
    }

    private static async Task DownloadAsync(string url, string destination)
    {
        using var response = await Http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync();
        await using var output = File.Create(destination);
        await stream.CopyToAsync(output);
    }

    private static string ExtractRepository(string tempRoot, string zipPath)
    {
        ZipFile.ExtractToDirectory(zipPath, tempRoot);
        var directories = Directory.GetDirectories(tempRoot);
        var repoDir = directories.FirstOrDefault(d =>
            Path.GetFileName(d).StartsWith(RepoName, StringComparison.OrdinalIgnoreCase));

        return repoDir ?? directories.First();
    }

    private static async Task RunDotNetAsync(IEnumerable<string> arguments, bool ignoreErrors = false)
    {
        var startInfo = new ProcessStartInfo("dotnet")
        {
            RedirectStandardOutput = false,
            RedirectStandardError = false,
            UseShellExecute = false
        };

        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = Process.Start(startInfo);
        if (process is null)
        {
            throw new InvalidOperationException("Nao foi possivel iniciar o processo dotnet.");
        }

        await process.WaitForExitAsync();
        if (process.ExitCode != 0 && !ignoreErrors)
        {
            var joined = string.Join(' ', startInfo.ArgumentList);
            throw new InvalidOperationException($"Comando 'dotnet {joined}' retornou codigo {process.ExitCode}");
        }
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }
        }
        catch
        {
            // ignore cleanup errors
        }
    }
}
