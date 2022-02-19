using System.Net;
using System.Net.Http;

namespace Dottik.MemeDownloader;

public struct ProgramData
{
    #region Static Methods & Variables
    public static CookieContainer container = new();
    private static HttpClientHandler httpHandler = new() {
        SslProtocols = System.Security.Authentication.SslProtocols.Tls12, 
        AutomaticDecompression = DecompressionMethods.All, 
        CookieContainer = container 
    };

    public static int versionCode = 01;
    public static string versionName = "v0.0.1-pre";
    public static HttpClient client { get; private set; }
    /// <summary>
    /// Initialize Web stuff used though out the program.
    /// </summary>
    /// <param name="customHandler"></param>
    public static void InitializeWebVariables(HttpClientHandler customHandler = null)
    {
        if (customHandler == null) {
            ProgramData.client = new(httpHandler);
        }
        else {
            ProgramData.client = new(customHandler);
        }
    }
    #endregion

    /// <summary>
    /// Constructor (Forced to use it thanks to .NET 6.0.1X).
    /// </summary>
    public ProgramData()
    {

    }
}