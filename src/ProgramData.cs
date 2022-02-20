﻿using System.Net;
using System.Net.Http;
using Newtonsoft.Json;

namespace Dottik.MemeDownloader;

public struct ProgramData
{
    #region Static Methods & Variables
    public static CookieContainer container = new();
    private static HttpClientHandler httpHandler = new() {
        SslProtocols = System.Security.Authentication.SslProtocols.Tls12, 
        AutomaticDecompression = DecompressionMethods.All, 
        CookieContainer = container,
        UseCookies = true
    };

    public static int versionCode = 01; // Used just for logs and maybe other stuff
    public static string versionName = "v0.0.1-dev"; // Used just for logs and maybe other stuff.
    public static HttpClient client { get; private set; }
    /// <summary>
    /// Initialize Web stuff used though out the program.
    /// </summary>
    /// <param name="customHandler">A custom HttpClientHandler if a different setting is needed</param>
    public static void InitializeWebVariables(HttpClientHandler customHandler = null) {
        if (customHandler == null) {
            client = new(httpHandler);
        } else {
            client = new(customHandler);
        }
    }
    #endregion

    /// <summary>
    /// Constructor (Forced to use it thanks to .NET 6.0.1X).
    /// </summary>
    public ProgramData() {

    }
}

public struct JSONData {
    /// <summary>
    /// Should the program run in more than one thread?
    /// </summary>
    [JsonProperty("MultiThreaded")]
    public bool multiThreaded = true;
    /// <summary>
    /// The array of the target subreddits.
    /// </summary>
    [JsonProperty("Target Sub Reddits")]
    public string[] targetSubReddits;
    /// <summary>
    /// Should the program download NSFW?
    /// </summary>
    [JsonProperty("Allow NSFW")]
    public bool allowNSFW = false;
    /// <summary>
    /// The amount of threads that should be used, IGNORED IF multiThreaded is false
    /// </summary>
    [JsonProperty("Thread Count")]
    public byte threads;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="allowNSFW">Should the program download posts categorized as NSFW? if <see langword="true"/> it will download them.</param>
    /// <param name="multiThreaded">Should the program run in a multi threaded way? if <see langword="true"/> it will run with the amount of Threads specified</param>
    /// <param name="targetSubReddits"></param>
    public JSONData(bool allowNSFW, bool multiThreaded, string[] targetSubReddits, byte threads) {
        this.allowNSFW = allowNSFW;
        this.multiThreaded = multiThreaded;
        this.targetSubReddits = targetSubReddits;
        this.threads = threads;
    }
}