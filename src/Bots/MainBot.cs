using System;
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
    public async Task StartBots(int? _threadAmount = 0) {
        BotEvents.BotCreationArgs _args = new();
        AnsiConsole.MarkupLine("Starting Bots...");
        
        if (_threadAmount == 0)
            _threadAmount = Environment.ProcessorCount;

        // Declare an event

        BotEvents.OnBotCreate += TriggerWhenBotCreated;

        // TODO: Actually start the bots. | Done, well, Sort of.
        for (int i = 0; i < _threadAmount; i++) {
            _args.BotName = $"Bot {i}";
            Thread newBot = new(async () => await BotLogic());
            newBot.Name = _args.BotName;
            newBot.Start();
            BotEvents.OnBotCreate?.Invoke(this, _args);
        }
        _threadAmount = null;
    }
    public async Task BotLogic() {

        string _rand_postJson = await PostDownloder.GetRandomPostJson("shitposting");
        
        JObject _result = JObject.Parse(
                JArray.Parse(_rand_postJson)[0]["data"]["children"][0]["data"].ToString()
            );
        // TODO: Download Posts, images and videos.
    }
#nullable disable
    public static async void TriggerWhenBotCreated(object? sender, BotEvents.BotCreationArgs arguments)
    {
        await Logger.LOGI($"Class of type \'{sender.GetType()}\' has started a new bot.");
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
