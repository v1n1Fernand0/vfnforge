using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace VFNForge.Cli;

internal static class Program
{
    private const string TemplateName = "vfnforge";
    private const string DefaultProjectName = "VFNForgeApp";

    private static int Main(string[] args)
    {
        Console.CancelKeyPress += (_, eventArgs) =>
        {
            Console.WriteLine("\nOperacao cancelada pelo usuario.");
            eventArgs.Cancel = true;
            Environment.Exit(1);
        };

        if (args.Contains("--version"))
        {
            Console.WriteLine(Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0");
            return 0;
        }

        if (args.Length == 0)
        {
            return RunInteractiveWizard();
        }

        if (args[0] is "-h" or "--help")
        {
            PrintUsage();
            return 0;
        }

        var command = args[0].ToLowerInvariant();
        var rest = args.Skip(1).ToArray();
        return command switch
        {
            "new" or "api" => RunNew(rest),
            _ => UnknownCommand(command)
        };
    }

    private static int RunNew(string[] args)
    {
        string? name = null;
        string? output = null;
        var forceCurrentDirectory = false;
        var forceOverwrite = false;
        var passthrough = new List<string>();

        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            if (arg is "-n" or "--name")
            {
                if (i + 1 >= args.Length)
                {
                    Console.Error.WriteLine("Missing value for --name");
                    return 1;
                }

                name = args[++i];
            }
            else if (arg is "-o" or "--output")
            {
                if (i + 1 >= args.Length)
                {
                    Console.Error.WriteLine("Missing value for --output");
                    return 1;
                }

                output = args[++i];
            }
            else if (arg is "--in-place" or "--inplace")
            {
                forceCurrentDirectory = true;
            }
            else if (arg is "--force")
            {
                forceOverwrite = true;
            }
            else if (arg == "--")
            {
                passthrough.AddRange(args.Skip(i + 1));
                break;
            }
            else if (!arg.StartsWith("-", StringComparison.Ordinal) && name is null)
            {
                name = arg;
            }
            else
            {
                passthrough.Add(arg);
            }
        }

        name = EnsureProjectName(name);
        if (!forceCurrentDirectory && string.IsNullOrWhiteSpace(output))
        {
            output = name;
        }

        return ExecuteTemplate(name, output, passthrough, forceOverwrite);
    }

    private static string EnsureProjectName(string? current)
    {
        if (!string.IsNullOrWhiteSpace(current))
        {
            return current!.Trim();
        }

        if (Console.IsInputRedirected)
        {
            return DefaultProjectName;
        }

        return PromptForProjectName();
    }

    private static string PromptForProjectName()
    {
        while (true)
        {
            Console.Write($"Nome do projeto [{DefaultProjectName}]: ");
            var response = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(response))
            {
                return DefaultProjectName;
            }

            var trimmed = response.Trim();
            if (trimmed.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                Console.WriteLine("O nome informado possui caracteres invalidos. Tente novamente.");
                continue;
            }

            return trimmed;
        }
    }

    private static int ExecuteTemplate(string name, string? output, IEnumerable<string> passthrough, bool force)
    {
        var targetPath = ResolveTargetPath(output);
        if (!force && DirectoryHasContent(targetPath))
        {
            Console.Error.WriteLine($"O diretorio '{targetPath}' ja possui arquivos. Execute com --force para sobrescrever.");
            return 1;
        }

        if (!string.IsNullOrWhiteSpace(output) && !Directory.Exists(targetPath))
        {
            Directory.CreateDirectory(targetPath);
        }

        var location = string.IsNullOrWhiteSpace(output) ? "pasta atual" : targetPath;
        Console.WriteLine($"Criando projeto '{name}' em '{location}'.");

        var dotnetArgs = new List<string> { "new", TemplateName, "-n", name };
        if (!string.IsNullOrWhiteSpace(output))
        {
            dotnetArgs.AddRange(new[] { "-o", output! });
        }

        dotnetArgs.AddRange(passthrough);
        return RunDotNet(dotnetArgs);
    }

    private static int RunInteractiveWizard()
    {
        Console.WriteLine("vfnforge CLI - modo interativo");
        Console.WriteLine("Pressione ENTER para aceitar o padrao exibido entre colchetes.");
        var name = PromptForProjectName();

        string? output = null;
        while (true)
        {
            Console.Write($"Diretorio de saida [./{name}]: ");
            var response = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(response))
            {
                output = name;
                break;
            }

            var trimmed = response.Trim();
            if (trimmed is "." or "./" or ".\\")
            {
                output = null;
                break;
            }

            output = trimmed;
            break;
        }

        var targetPath = ResolveTargetPath(output);
        var force = false;

        if (DirectoryHasContent(targetPath))
        {
            if (!PromptForceOverwrite(targetPath))
            {
                Console.WriteLine("Operacao cancelada.");
                return 1;
            }

            force = true;
        }

        return ExecuteTemplate(name, output, Array.Empty<string>(), force);
    }

    private static int UnknownCommand(string command)
    {
        Console.Error.WriteLine($"Unknown command '{command}'.");
        PrintUsage();
        return 1;
    }

    private static int RunDotNet(IEnumerable<string> arguments)
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
        if (process == null)
        {
            Console.Error.WriteLine("Failed to start dotnet process.");
            return 1;
        }

        process.WaitForExit();
        return process.ExitCode;
    }

    private static string ResolveTargetPath(string? output)
    {
        return string.IsNullOrWhiteSpace(output)
            ? Directory.GetCurrentDirectory()
            : Path.GetFullPath(output);
    }

    private static bool DirectoryHasContent(string path)
    {
        if (!Directory.Exists(path))
        {
            return false;
        }

        using var enumerator = Directory.EnumerateFileSystemEntries(path).GetEnumerator();
        return enumerator.MoveNext();
    }

    private static bool PromptForceOverwrite(string path)
    {
        while (true)
        {
            Console.Write($"O diretorio '{path}' nao esta vazio. Sobrescrever? [y/N]: ");
            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            input = input.Trim().ToLowerInvariant();
            if (input is "y" or "yes" or "s" or "sim")
            {
                return true;
            }

            if (input is "n" or "no" or "nao")
            {
                return false;
            }

            Console.WriteLine("Resposta invalida. Digite 'y' ou 'n'.");
        }
    }

    private static void PrintUsage()
    {
        Console.WriteLine("vfnforge CLI");
        Console.WriteLine("Comandos:");
        Console.WriteLine("  vfnforge api MinhaApp             # cria ./MinhaApp com tudo configurado");
        Console.WriteLine("  vfnforge api                      # abre assistente interativo");
        Console.WriteLine("  vfnforge new ...                  # alias para 'api'");
        Console.WriteLine("Opcoes:");
        Console.WriteLine("  -n|--name NomeDoProjeto           # informa o nome explicitamente");
        Console.WriteLine("  -o|--output CaminhoDeSaida        # muda o destino (padrao: ./<nome>)");
        Console.WriteLine("  --in-place                        # usa a pasta atual (nao cria subpasta)");
        Console.WriteLine("  --force                           # permite gerar em diretorio nao vazio");
        Console.WriteLine("  --                                # repassa os argumentos restantes ao dotnet new");
        Console.WriteLine("  --version                         # exibe versao");
        Console.WriteLine("  -h|--help                         # mostra esta tela");
        Console.WriteLine("Exemplo:");
        Console.WriteLine("  vfnforge api MinhaApp -- --dry-run");
    }
}
