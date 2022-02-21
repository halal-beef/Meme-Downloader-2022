using Dottik.MemeDownloader.Logging;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Dottik.MemeDownloader.Downloader;

public static class MainDownloader
{
    /// <summary>
    /// Analyze the whole requestJson string and return the file type identified + it's extension.
    /// </summary>
    /// <param name="requestJson"></param>
    /// <returns></returns>
    public static async Task<FileInformation> GetFileType(string requestJson)
    {
        FileInformation fileInfo = new();

        JObject Result = JObject.Parse(
            JArray.Parse(requestJson)[0]["data"]["children"][0]["data"].ToString()
        );
        string contentUrl = (string)Result["url"];
        string contentDomain = (string)Result["domain"];
        fileInfo.isNSFW = (bool)Result["over_18"];
        fileInfo.PostTitle = Result["title"].ToString().Trim(Path.GetInvalidFileNameChars()).Trim(new char[] { '\"', '\'' });

        if (contentDomain == "i.redd.it") {
            await Logger.LOGI($"Bot {Thread.CurrentThread.Name} has found an Image!", "Downloader");
            fileInfo.FileExtension = "." + contentUrl.Split('.').Last();
            fileInfo.DownloadURL = (string)Result["url_overridden_by_dest"];
        }
        else if ((bool)Result["is_video"] || contentUrl.Contains("mp4") || contentUrl.Contains("mov") || contentUrl.Contains("mkv"))
        {
            await Logger.LOGI($"Bot {Thread.CurrentThread.Name} has found a Video!", "Downloader");
            fileInfo.FileTypes = FileTypes.Video;
            fileInfo.FileExtension = "." + contentUrl.Split('.').Last();
        }
        else if (contentUrl.Contains("gallery") && contentDomain == "reddit.com")
        {
            await Logger.LOGI($"Bot {Thread.CurrentThread.Name} has found an Image Gallery!", "Downloader");
            fileInfo.isGallery = true;
        }
        else if (contentUrl.Contains("gif"))
        {
            await Logger.LOGI($"Bot {Thread.CurrentThread.Name} has found a Gif!", "Downloader");
            fileInfo.FileExtension = ".gif";
        }
        else
        {
            await Logger.LOGI($"Bot {Thread.CurrentThread.Name} has found an unknown media type...", "Downloader");
            fileInfo.FileTypes = FileTypes.Unknown;
            fileInfo.FileExtension = ".htm";
        }
        fileInfo.FileName = fileInfo.PostTitle + fileInfo.FileExtension;
        return fileInfo;
    }
}