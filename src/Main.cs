using Spectre.Console;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dottik.MemeDownloader.Logging;
using Dottik.MemeDownloader.Utilities;
using Dottik.MemeDownloader.Bots;

namespace Dottik.MemeDownloader;

public class MainActivity
{
    /// <summary>
    /// The program's main entry point.
    /// </summary>
    /// <param name="args">The program startup arguments!</param>
    public static async Task Main(string[] args) {
        #region Obligatory Folder Creation.
        string[] foldersToCreate = new string[] {
            $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\Dottik\\MD2022\\",
            $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\Dottik\\MD2022\\Logs",
            $"{Environment.CurrentDirectory}\\Dependencies\\",
            $"{Environment.CurrentDirectory}\\Temp\\",
            $"{Environment.CurrentDirectory}\\Downloaded Content\\"};

        EnvironmentUtilities.BatchCreateFolders(foldersToCreate);
        #endregion

        Console.Title = "Meme Downloader 2022 - Reddit Post Downloader";
        await Logger.LOGI($"Meme Downloader 2022 {ProgramData.versionName} ({ProgramData.versionCode}) has been started | Executed by user {Environment.UserName}\\\\{Environment.MachineName}");
        if (args is not null && args.Length >= 1) {
            switch (ParseArguments(args)) {
                // Print help & exit
                case ProgramModes.HELP:
                    PrintHelp();
                    Environment.Exit(0);
                    break;
                // Run initial setup & exit
                case ProgramModes.SETUP:
                    Environment.Exit(0);
                    break;
            }
        } 
        else if (!File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + $"\\Dottik\\MD2022\\{ProgramData.versionCode}.setup\\")) {

            AnsiConsole.MarkupLine("[yellow]Preparing Dependencies[/]...");
            await EnvironmentConfig.RestoreDependencies();
            AnsiConsole.MarkupLine("[green]Dependencies Downloaded![/] Proceeding...");
            File.Create(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + $"\\Dottik\\MD2022\\{ProgramData.versionCode}.setup");
        }
    }

    static ProgramModes ParseArguments(string[] bootArgs) {
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
    private static void PrintHelp() {
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