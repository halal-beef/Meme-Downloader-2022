using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace Dottik.MemeDownloader;

public struct ProgramData
{
    #region Static Methods & Variables

    public static CookieContainer container = new();

    public static int versionCode = 01;

    // Used just for logs and maybe other stuff
    public static string versionName = "v0.0.1-dev";

    private static readonly HttpClientHandler httpHandler = new()
    {
        SslProtocols = System.Security.Authentication.SslProtocols.Tls12,
        AutomaticDecompression = DecompressionMethods.All,
        CookieContainer = container,
        UseCookies = true
    };

    // Used just for logs and maybe other stuff.
    public static HttpClient Client { get; private set; }

    /// <summary>
    /// Initialize Web stuff used though out the program.
    /// </summary>
    /// <param name="customHandler">A custom HttpClientHandler if a different setting is needed</param>
    public static void InitializeWebVariables(HttpClientHandler customHandler = null)
    {
        if (customHandler == null)
        {
            Client = new(httpHandler);
        }
        else
        {
            Client = new(customHandler);
        }
    }

    #endregion Static Methods & Variables

    /// <summary>
    /// Constructor (Forced to use it thanks to .NET 6.0.1X).
    /// </summary>
    public ProgramData()
    {
    }
}

public struct JSONData
{
    /// <summary>
    /// Should the program download NSFW?
    /// </summary>
    [JsonProperty("Allow NSFW")]
    public bool allowNSFW = false;

    /// <summary>
    /// Should the program run in more than one thread?
    /// </summary>
    [JsonProperty("MultiThreaded")]
    public bool multiThreaded = true;

    /// <summary>
    /// The array of the target subreddits.
    /// </summary>
    [JsonProperty("Target Sub Reddits")]
    public List<string> targetSubReddits;

    /// <summary>
    /// The amount of threads that should be used, IGNORED IF multiThreaded is false
    /// </summary>
    [JsonProperty("Thread Count")]
    public int threads;

    /// <summary>
    /// Initialize JSON Data.
    /// </summary>
    /// <param name="allowNSFW">Should the program download posts categorized as NSFW? if <see langword="true"/> it will download them.</param>
    /// <param name="multiThreaded">Should the program run in a multi threaded way? if <see langword="true"/> it will run with the amount of Threads specified</param>
    /// <param name="targetSubReddits"></param>
    public JSONData(bool allowNSFW, bool multiThreaded, List<string> targetSubReddits, int threads)
    {
        this.allowNSFW = allowNSFW;
        this.multiThreaded = multiThreaded;
        this.targetSubReddits = targetSubReddits;
        this.threads = threads;
    }
}