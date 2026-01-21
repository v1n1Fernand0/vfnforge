using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace VFNForge.Cli;

internal static class Program
{
    private const string TemplateName = "vfnforge";

    private static int Main(string[] args)
    {
        if (args.Contains("--version"))
        {
            Console.WriteLine(Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0");
            return 0;
        }

        if (args.Length == 0 || args[0] is "-h" or "--help")
        {
            PrintUsage();
            return args.Length == 0 ? 1 : 0;
        }

        var command = args[0].ToLowerInvariant();
        var rest = args.Skip(1).ToArray();
        return command switch
        {
            "new" => RunNew(rest),
            _ => UnknownCommand(command)
        };
    }

    private static int RunNew(string[] args)
    {
        string? name = null;
        string? output = null;
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
            else if (arg == "--")
            {
                passthrough.AddRange(args.Skip(i + 1));
                break;
            }
            else
            {
                passthrough.Add(arg);
            }
        }

        name ??= "VFNForgeApp";
        var dotnetArgs = new List<string> { "new", TemplateName, "-n", name };
        if (!string.IsNullOrWhiteSpace(output))
        {
            dotnetArgs.AddRange(new[] { "-o", output! });
        }

        dotnetArgs.AddRange(passthrough);
        return RunDotNet(dotnetArgs);
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

    private static void PrintUsage()
    {
        Console.WriteLine("vfnforge CLI");
        Console.WriteLine("Usage:");
        Console.WriteLine("  vfnforge new -n MinhaApp [-o ./output] [-- additional dotnet new args]");
        Console.WriteLine("  vfnforge --version");
    }
}
