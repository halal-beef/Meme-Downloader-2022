﻿using Dottik.MemeDownloader.Downloader;
using Dottik.MemeDownloader.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Spectre.Console;
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Dottik.MemeDownloader.Bots;

public class BotMain
{
    public static readonly string DownloadPath = Environment.CurrentDirectory + "\\Downloaded Content\\";
    public static BotMain Instance;

    public async Task StartBots(bool multiThreading, int _threadAmount = -1)
    {
        BotEvents.BotCreationArgs _args = new();
        AnsiConsole.MarkupLine("Starting Bots...");

        BotConfigurations.multiThreaded = multiThreading;

        if (_threadAmount == -1 && multiThreading)
            _threadAmount = Environment.ProcessorCount;
        else if (!multiThreading)
            _threadAmount = 1;

        // Declare an event

        BotEvents.OnBotCreate += TriggerWhenBotCreated;
        BotEvents.OnBotCrash += TriggerWhenBotCrash;

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
        await Task.Delay(15000);
        await BotRestarter();
    }

    public async Task BotLogic(int listIdentifier)
    {
        BotEvents.BotCrashArgs crashArgs = new();
        try
        {
            while (true)
            {
                for (int iterator = 0; iterator < BotConfigurations.targetSubreddits?.Length; iterator++)
                {
                    #region Get JSON
                    string _rand_postJson = await PostDownloder.GetRandomPostJson("shitposting");
                    #endregion

                    #region Get Post Address
                    JObject _result = JObject.Parse(
                            (string)JArray.Parse(_rand_postJson)[0]["data"]["children"][0]["data"]
                        );
                    #endregion

                    // TODO: Download Posts, Galleries (DONE), images and videos.
                    FileInformation dlInfo = await MainDownloader.GetFileType(_rand_postJson);



                    #region Check if Gallery and Download.
                    if (dlInfo.isGallery)
                    {
                        FormattedLinks galleryData = await GetRedditGallery.FormatLinks(_rand_postJson);
                        List<Stream> streams = await GetRedditGallery.GetGallery(galleryData);

                        for (int i = 0; i < streams.Count; i++)
                        {
                            FileStream newFile = File.Create(DownloadPath);
                            streams?[i].CopyToAsync(newFile);
                        }
                    }
                    #endregion
                    if (dlInfo.FileTypes == FileTypes.Image)
                    {
                        FileStream fs = File.Create(DownloadPath + $"\\{BotConfigurations.targetSubreddits[iterator]}\\{dlInfo.FileName}");
                        await ProgramData.Client.GetStreamAsync(dlInfo.DownloadURL).Result.CopyToAsync(fs);
                        await fs.FlushAsync();
                        await fs.DisposeAsync();
                        fs.Close();
                    }
                }
            }
        } catch (Exception ex) {
            BotConfigurations.bots[listIdentifier] = false;
            AnsiConsole.MarkupLine($"{Thread.CurrentThread.Name} - Has ended it's task due to an error.");
            crashArgs.exception = ex;
            BotEvents.OnBotCrash?.Invoke(this, crashArgs);
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
        => await Logger.LOGI($"Class of type \'{sender?.GetType()}\' has started a new bot with name \'{arguments?.BotName}\'", "BotLogic -> BotEvents.OnCreate");

    public static async void TriggerWhenBotCrash(object sender, BotEvents.BotCrashArgs arguments)
    {
        await Logger.LOGE(
            $"An Exception occured in \'{sender?.GetType()}\'! Stack Trace:\r\n" +
            "--------BEGIN STACK TRACE\r\n" +
            $"{JsonConvert.SerializeObject(arguments.exception?.ToString(), Formatting.Indented)}\r\n" +
            "--------END STACK TRACE",
            "BotLogic");
    }

#nullable restore
}

public static class BotEvents
{
    public static EventHandler<BotCreationArgs> OnBotCreate;
    public static EventHandler<BotDownloadArgs> OnBotFinishDownload;
    public static EventHandler<BotCrashArgs> OnBotCrash;
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
    public class BotCrashArgs : EventArgs
    {
        public Exception exception { get; set; }
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