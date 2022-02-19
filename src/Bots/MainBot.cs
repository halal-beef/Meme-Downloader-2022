using System;
using System.Collections.Generic;
using System.Linq;
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
    public static async Task StartBots(int? threadAmount = 0) {
        AnsiConsole.MarkupLine("Starting Bots...");
        
        if (threadAmount == 0)
            threadAmount = Environment.ProcessorCount;

        // TODO: Actually start the bots.
        for (int i = 0; i > threadAmount; i++) {
            await Logger.LOGI($"Starting Bot {i}...");
        }
        threadAmount = null;
    }
    public static async Task BotLogic() {

        string rand_postJson = await PostDownloder.GetRandomPostJson("shitposting");
        
        JObject Result = JObject.Parse(
                JArray.Parse(rand_postJson)[0]["data"]["children"][0]["data"].ToString()
            );
        // TODO: Download Posts, images and videos.
    }
}

