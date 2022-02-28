using Dottik.MemeDownloader.Bots;
using Dottik.MemeDownloader.Logging;
using Dottik.MemeDownloader.Utilities;
using Newtonsoft.Json;
using Spectre.Console;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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

                case ProgramModes.TESTING:

                    // Make a new thread that kills the program one minute afterwards.
                    new Thread(() =>
                    {
                        Thread.Sleep(60 * 1000);
                        Environment.Exit(0);
                    }).Start();
                    goto NORMALEXEC;
            }
        }

        if (setupMode || !File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + $"\\Dottik\\MD2022\\{ProgramData.versionCode}.setup\\"))
        {
            AnsiConsole.MarkupLine("[yellow]Preparing Dependencies[/]...");
            // Init HttpClient, else no instance.
            ProgramData.InitializeWebVariables();

            if (!setupMode)
            {
                await EnvironmentConfig.RestoreDependencies(true);
                AnsiConsole.MarkupLine("[green]Dependencies Downloaded![/] Proceeding...");
            }
            else
            {
                await EnvironmentConfig.CheckDependencyState();
                if (EnvironmentConfig.ffmpegBad is true)
                {
                    AnsiConsole.MarkupLine("[green]Dependency ffmpeg failed the integrity test, redownloading...[/]");
                    await EnvironmentConfig.RestoreDependencies(false);
                }
            }
            File.Create(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + $"\\Dottik\\MD2022\\{ProgramData.versionCode}.setup");
            if (setupMode)
            {
                JSONData baseJDat = new()
                {
                    targetSubReddits = new()
                    { "shitposting", "memes" },
                    multiThreaded = true,
                    threads = Environment.ProcessorCount
                };
                string jsonBase = JsonConvert.SerializeObject(baseJDat, Formatting.Indented);
                File.WriteAllText(Environment.CurrentDirectory + "\\Configurations.json", jsonBase);
                AnsiConsole.MarkupLine("[green bold]Setup complete![/]");
                Environment.Exit(0);
            }
        }

    NORMALEXEC:

        #region Declare some variables

        JSONData progData = new();
        BotMain botMainInstance = new();
        ProgramData.InitializeWebVariables();

        BotMain.Instance = botMainInstance;

        #endregion Declare some variables

        #region Add to PATH Var (Process Only)

        Environment.SetEnvironmentVariable("PATH", Environment.GetEnvironmentVariable("PATH") + $";{Environment.CurrentDirectory}\\Dependencies\\", EnvironmentVariableTarget.Process);

        #endregion Add to PATH Var (Process Only)

        #region Check if configurations exist.

        if (File.Exists(Environment.CurrentDirectory + "\\Configurations.json"))
        {
            progData = await ReadConfigurations();
        }
        else
        {
            AnsiConsole.MarkupLine($"please run \'{Environment.ProcessPath} -setup\' to setup Meme Downloader 2022 {ProgramData.versionName}");
            Environment.Exit(-2);
        }

        #endregion Check if configurations exist.

        AnsiConsole.MarkupLine("Starting Meme Downloader 2022...");

        BotConfigurations.targetSubreddits = progData.targetSubReddits.ToArray();

        string threadAmount = "";

        if (progData.multiThreaded)
            threadAmount = $" - Thread Amount: {progData.threads}\r\n";

        AnsiConsole.MarkupLine(
             " - Program Settings:\r\n" +
            $" - Multi-Threading: {progData.multiThreaded}\r\n" +
            $" - Target Subreddits: [green]{ParseArrayToString(progData.targetSubReddits.ToArray()).Result.RemoveMarkup()}[/]\r\n" +
            $" - Allow NSFW: {progData.allowNSFW}\r\n" +
            $"{threadAmount}");

        AnsiConsole.MarkupLine(" - Creating Folders for Downloaded content...");
        for (int i = 0; i < progData.targetSubReddits.Count; i++)
        {
            Directory.CreateDirectory(BotMain.DownloadPath + $"{progData.targetSubReddits[i]}\\");
        }

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
                    newString.Append(content[i]).Append(", ");
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