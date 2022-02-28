using System.Threading.Tasks;

namespace Dottik.MemeDownloader.Downloader;

public static class PostDownloder
{
    /// <summary>
    /// Get raw random post data in JSON format from a subreddit.
    /// </summary>
    /// <param name="subreddit">
    /// the subreddit to download from ONLY the name, not "/r/{subreddit}", but without '/r/'
    /// </param>
    /// <returns>Raw JSON containing data from a random reddit post from that subreddit.</returns>
    public static async Task<string> GetRandomPostJson(string subreddit)
        => await ProgramData.Client.GetStringAsync($"http://reddit.com/r/{subreddit}/random.json");
}