using Spectre.Console;
using System;
using System.IO;
using System.Linq;
namespace Dottik.MemeDownloader;

public class MainActivity
{
    /// <summary>
    /// The program's main entry point.
    /// </summary>
    /// <param name="args"></param>
    static async void Main(string[] args)
    {
        if (args is not null && args.Length >= 1) {
            switch (ParseArguments(args)) {
                // Print help & exit
                case ProgramModes.HELP:
                    Utils.PrintHelp();
                    Environment.Exit(0);
                    break;
                // Run initial setup & exit
                case ProgramModes.SETUP:
                    Environment.Exit(0);
                    break;
            }
        } else if (!File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + $"\\Dottik\\MD2022\\{ProgramData.versionCode}.setup\\") && !ProgramData.versionName.Contains("-dev")) {

            bool dependencyStatus = await EnvironmentConfig.CheckDependencyState();

            if (!dependencyStatus) {
                // If corrupted we need to start our HttpClient!
                ProgramData.InitializeWebVariables();
                await EnvironmentConfig.RestoreDependencies();
                Environment.Exit(0);
            }
        }
    }

    static ProgramModes ParseArguments(string[] bootArgs)
    {
        for (int i = 0; i < bootArgs.Length; i++)
        {
            if (bootArgs.Contains("--setup") || bootArgs.Contains("-setup")) {
                return ProgramModes.SETUP;
            } else if (bootArgs.Contains("--help") || bootArgs.Contains("-help")) { 
                return ProgramModes.HELP;
            } else if (bootArgs.Contains("--ci") || bootArgs.Contains("-ci") || bootArgs.Contains("--test") || bootArgs.Contains("-test")) {
                return ProgramModes.TESTING;
            }
        }
        return ProgramModes.NORMAL;
    }
}
public class Utils
{
    public static void PrintHelp()
    {
        AnsiConsole.MarkupLine(
            "||||----------------------||||\r\n" +
            "|||| [red]Meme Downloader 2022[/] ||||\r\n" +
            "||||----------------------||||\r\n" +
            "\r\n" +
            " [green]-setup[/] = Run initial program setup.\r\n" +
            "\r\n" +
            " [green]-help[/] = Print this message.\r\n" +
            "\r\n" +
            " [yellow]-ci[/] = Mode to test the program (Used in Continuous Integration)\r\n"
            );
    }
}