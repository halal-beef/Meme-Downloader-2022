using Dottik.MemeDownloader.Downloader;
using Dottik.MemeDownloader.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Dottik.MemeDownloader.Bots;

public class BotMain
{
    public static BotMain Instance;

    public async Task StartBots(bool multiThreading, int? _threadAmount = 1)
    {
        BotEvents.BotCreationArgs _args = new();
        AnsiConsole.MarkupLine("Starting Bots...");

        BotConfigurations.multiThreaded = multiThreading;

        if (_threadAmount == 1 && multiThreading)
            _threadAmount = Environment.ProcessorCount;
        else if (multiThreading)
            _threadAmount = 1;

        // Declare an event

        BotEvents.OnBotCreate += TriggerWhenBotCreated;

        // TODO: Actually start the bots. | Done, well, Sort of.
        for (int i = 0; i < _threadAmount; i++)
        {
            _args.BotName = $"Bot {i}";
            Thread newBot = new(async () => await BotLogic(i));
            BotConfigurations.bots.Add(true);
            newBot.Name = _args.BotName;
            newBot.Start();
            BotEvents.OnBotCreate?.Invoke(this, _args);
        }
        BotConfigurations.threadAmount = _threadAmount;
        _threadAmount = null;
        await Task.Delay(15000);
        await BotRestarter();
    }

    public async Task BotLogic(int listIdentifier)
    {
        try
        {
            string _rand_postJson = await PostDownloder.GetRandomPostJson("shitposting");

            JObject _result = JObject.Parse(
                    JArray.Parse(_rand_postJson)[0]["data"]["children"][0]["data"].ToString()
                );
            // TODO: Download Posts, images and videos.
            FileInformation dlInfo = await MainDownloader.GetFileType(_rand_postJson);

            if (dlInfo.isGallery)
            {
            }
        }
        catch (Exception ex)
        {
            BotConfigurations.bots[listIdentifier] = false;
            AnsiConsole.MarkupLine($"{Thread.CurrentThread.Name} [gray]-[/] [red]Has suffered an exception. Logging to log file![/]");
            await Logger.LOGE(
                "An Exception occured! Stack Trace:\r\n" +
                "--------BEGIN STACK TRACE\r\n" +
                $"{JsonConvert.SerializeObject(ex.ToString(), Formatting.Indented)}\r\n" +
                "--------END STACK TRACE",
                "BotLogic");
        }
    }

    public async Task BotRestarter()
    {
        BotEvents.BotCreationArgs _args = new();
        Predicate<bool> quickDetect = new(a => !a);
        float x = 0;
        x = BotConfigurations.bots.FindAll(quickDetect).Count - BotConfigurations.bots.Count;

        if (x < BotConfigurations.bots.Count / 2)
        {
            for (int i = 0; i < BotConfigurations.bots.Count; i++)
            {
                i = BotConfigurations.bots.FindIndex(i, quickDetect);

                // Re-Run bots.
                _args.BotName = $"Bot {i}";
                Thread newBot = new(async () => await BotLogic(i));
                newBot.Name = _args.BotName;
                newBot.Start();
                BotEvents.OnBotCreate?.Invoke(this, _args);

                BotConfigurations.bots[i] = true;
            }
            await Task.Delay(420);
        }
    }

#nullable disable

    public static async void TriggerWhenBotCreated(object sender, BotEvents.BotCreationArgs arguments)
    {
        await Logger.LOGI($"Class of type \'{sender.GetType()}\' has started a new bot with name \'{arguments.BotName}\'", "BotLogic -> BotEvents.OnCreate");
    }

#nullable restore
}

public static class BotEvents
{
    public static EventHandler<BotCreationArgs> OnBotCreate;
    public static EventHandler<BotDownloadArgs> OnBotFinishDownload;

    public class BotCreationArgs : EventArgs
    {
        public string BotName { get; set; }
    }

    public class BotDownloadArgs : EventArgs
    {
        public string BotName { get; set; }
        public string FileName { get; set; }
        public ulong FileSize { get; set; }
    }
}

public struct BotConfigurations
{
    /// <summary>
    /// The target subreddits.
    /// </summary>
    public static string[] targetSubreddits;

    /// <summary>
    /// The amount of threads in which bots should exist.
    /// </summary>
    public static int? threadAmount;

    /// <summary>
    /// The list of <see cref="bool"/> to keep track of the bots running.
    /// </summary>
    public static List<bool> bots = new();

    /// <summary>
    /// Is Running in MultiThreading mode.
    /// </summary>
    public static bool multiThreaded = false;
}