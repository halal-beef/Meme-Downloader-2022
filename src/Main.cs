using Dottik.MemeDownloader.Bots;
using Dottik.MemeDownloader.Logging;
using Dottik.MemeDownloader.Utilities;
using Newtonsoft.Json;
using Spectre.Console;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dottik.MemeDownloader;

public static class MainActivity
{
    /// <summary>
    /// The program's main entry point.
    /// </summary>
    /// <param name="args">The program startup arguments!</param>
    public static async Task Main(string[] args)
    {
        #region Obligatory Folder Creation.

        string[] foldersToCreate = new string[] {
            $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\Dottik\\MD2022\\",
            $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\Dottik\\MD2022\\Logs",
            $"{Environment.CurrentDirectory}\\Dependencies\\",
            $"{Environment.CurrentDirectory}\\Temp\\",
            $"{Environment.CurrentDirectory}\\Downloaded Content\\"};

        EnvironmentUtilities.BatchCreateFolders(foldersToCreate);

        #endregion Obligatory Folder Creation.

        bool setupMode = false;

        Console.Title = "Meme Downloader 2022 - Reddit Post Downloader";
        await Logger.LOGI($"Meme Downloader 2022 {ProgramData.versionName} ({ProgramData.versionCode}) has been started | Executed by user {Environment.UserName}\\\\{Environment.MachineName}", "Main");
        if (args?.Length >= 1)
        {
            switch (ParseArguments(args))
            {
                case ProgramModes.NORMAL:
                    goto NORMALEXEC;
                // Print help & exit
                case ProgramModes.HELP:
                    PrintHelp();
                    Environment.Exit(0);
                    break;
                // Run initial setup & exit
                case ProgramModes.SETUP:
                    setupMode = true;
                    break;
            }
        }
        if (setupMode || !File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + $"\\Dottik\\MD2022\\{ProgramData.versionCode}.setup\\"))
        {
            AnsiConsole.MarkupLine("[yellow]Preparing Dependencies[/]...");
            await EnvironmentConfig.RestoreDependencies();
            if (!setupMode)
                AnsiConsole.MarkupLine("[green]Dependencies Downloaded![/] Proceeding...");

            File.Create(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + $"\\Dottik\\MD2022\\{ProgramData.versionCode}.setup");
            if (setupMode)
            {
                string jsonBase = JsonConvert.SerializeObject(new JSONData());
                File.WriteAllText(Environment.CurrentDirectory + "\\Configurations.json", jsonBase);
                AnsiConsole.MarkupLine("[green bold]Setup complete![/]");
                Environment.Exit(0);
            }
        }

    NORMALEXEC:
        AnsiConsole.MarkupLine("Starting Meme Downloader 2022...");

        #region Declare some variables

        JSONData progData = new();
        BotMain botMainInstance = new();
        ProgramData.InitializeWebVariables();

        BotMain.Instance = botMainInstance;

        #endregion Declare some variables

        #region Check if configurations exist.

        if (File.Exists(Environment.CurrentDirectory + "\\Configurations.json"))
        {
            progData = await ReadConfigurations();
        }
        else
        {
            AnsiConsole.MarkupLine($"run \'{Environment.ProcessPath} -setup\'");
            Environment.Exit(0);
        }

        #endregion Check if configurations exist.

        string threadAmount = "";

        if (progData.multiThreaded)
            threadAmount = $" - Thread Amount: {progData.threads}\r\n";

        AnsiConsole.MarkupLine(
             " - Program Settings:\r\n" +
            $" - Multi-Threading: {progData.multiThreaded}\r\n" +
            $" - Target Subreddits: [green]{ParseArrayToString(progData.targetSubReddits)}[/]\r\n" +
            $" - Allow NSFW: {progData.allowNSFW}\r\n" +
            $"{threadAmount}");
        await BotMain.Instance.StartBots(progData.multiThreaded, progData.threads);
    }

    private static ProgramModes ParseArguments(string[] bootArgs)
    {
        for (int i = 0; i < bootArgs.Length; i++)
        {
            if (bootArgs.Contains("--setup") || bootArgs.Contains("-setup"))
            {
                return ProgramModes.SETUP;
            }
            else if (bootArgs.Contains("--help") || bootArgs.Contains("-help"))
            {
                return ProgramModes.HELP;
            }
            else if (bootArgs.Contains("--ci") || bootArgs.Contains("-ci") || bootArgs.Contains("--test") || bootArgs.Contains("-test"))
            {
                return ProgramModes.TESTING;
            }
        }
        return ProgramModes.NORMAL;
    }

    private static async Task<string> ParseArrayToString(string[] content)
    {
        StringBuilder newString = new();
        await Task.Run(() =>
        {
            for (int i = 0; i < content.Length; i++)
            {
                if (i != content.Length - 1)
                    newString.Append(content[i] + ", ");
                else
                    newString.Append(content[i]);
            }
        });
        return newString.ToString();
    }

    private static void PrintHelp()
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

    private static async Task<JSONData> ReadConfigurations() => JsonConvert.DeserializeObject<JSONData>(await File.ReadAllTextAsync(Environment.CurrentDirectory + "\\Configurations.json"));
}