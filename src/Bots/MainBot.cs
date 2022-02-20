﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Text;
using System.Threading.Tasks;
using Dottik.MemeDownloader.Downloader;
using Dottik.MemeDownloader.Logging;
using Dottik.MemeDownloader.Utilities;
using Spectre.Console;
using Newtonsoft.Json.Linq;

namespace Dottik.MemeDownloader.Bots;
public class BotMain
{
    public static BotMain Instance;
    public async Task StartBots(bool multiThreading, int? _threadAmount = 1) {
        BotEvents.BotCreationArgs _args = new();
        AnsiConsole.MarkupLine("Starting Bots...");
        
        
        if (_threadAmount == 1 && multiThreading)
            _threadAmount = Environment.ProcessorCount;
        else if (multiThreading) {
            BotConfigurations.multiThreaded = multiThreading;
        }
        else
            _threadAmount = 1;
        // Declare an event

        BotEvents.OnBotCreate += TriggerWhenBotCreated;

        // TODO: Actually start the bots. | Done, well, Sort of.
        for (int i = 0; i < _threadAmount; i++) {
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
    public async Task BotLogic(int listIdentifier) {

        string _rand_postJson = await PostDownloder.GetRandomPostJson("shitposting");

        JObject _result = JObject.Parse(
                JArray.Parse(_rand_postJson)[0]["data"]["children"][0]["data"].ToString()
            );
        // TODO: Download Posts, images and videos.
        BotConfigurations.bots[listIdentifier] = false;

    }
    public async Task BotRestarter()
    {
        BotEvents.BotCreationArgs _args = new();
        Predicate<bool> quickDetect = new(a => { if (a == false) return true; else return false; });  
        float x = 0;
        x = BotConfigurations.bots.FindAll(quickDetect).Count - BotConfigurations.bots.Count;

        if (x < BotConfigurations.bots.Count / 2) {

            for (int i = 0; i < BotConfigurations.bots.Count; i++)
            {
                i = BotConfigurations.bots.FindIndex(i, quickDetect);
                
                // Create bots.
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
        await Logger.LOGI($"Class of type \'{sender.GetType()}\' has started a new bot with name \'{arguments.BotName}\'");
    }
#nullable restore
}
public class BotEvents
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