using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Dottik.MemeDownloader.Bots;
using Dottik.MemeDownloader.Logging;

namespace Dottik.MemeDownloader.Downloader;
public class PostDownloder
{
    /// <summary>
    /// Get raw random post data in JSON format from a subreddit.
    /// </summary>
    /// <param name="subreddit">the subreddit to download from ONLY the name, not "/r/{subreddit}", but without '/r/'</param>
    /// <returns>Raw JSON containing data from a random reddit post from that subreddit.</returns>
    public static async Task<string> GetRandomPostJson(string subreddit) => await ProgramData.client.GetStringAsync($"http://reddit.com/r/{subreddit}/random.json");
}