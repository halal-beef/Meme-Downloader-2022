using Dottik.MemeDownloader.Logging;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dottik.MemeDownloader.Utilities;

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
        string contentUrl = "";
        string contentDomain = "";
        bool isGif = false;
        await Task.Run(() =>
        {
            contentUrl =
            (string)Result["url_overridden_by_dest"] is "" or " " or null ? (string)Result["url"] : (string)Result["url_overridden_by_dest"];

            contentDomain = (string)Result["domain"];
            fileInfo.isNSFW = (bool)Result["over_18"];
            fileInfo.PostTitle = (string)Result["title"];

            isGif = (bool?)Result["secure_media"]["reddit_video"]["is_gif"] is not null && (bool)Result["secure_media"]["reddit_video"]["is_gif"];
        });
        await Task.Run(async () =>
        {
            if (contentDomain == "i.redd.it")
            {
                await Logger.LOGI($"Bot {Thread.CurrentThread.Name} has found an Image!", "Downloader");

                fileInfo.FileExtension = "." + contentUrl.Split('.').Last();
                fileInfo.DownloadURL = contentUrl;
            }
            else if ((bool)Result["is_video"] || contentUrl.Contains("mp4") || contentUrl.Contains("mov") || contentUrl.Contains("mkv") && isGif is not true)
            {
                await Logger.LOGI($"Bot {Thread.CurrentThread.Name} has found a Video!", "Downloader");

                fileInfo.FileTypes = FileTypes.Video;

                if (contentDomain != "v.redd.it")
                    fileInfo.FileExtension = "." + contentUrl.Split('.').Last();
                else
                    fileInfo.FileExtension = ".mp4";
            }
            else if (contentUrl.Contains("gallery") && contentDomain is "reddit.com")
            {
                await Logger.LOGI($"Bot {Thread.CurrentThread.Name} has found an Image Gallery!", "Downloader");

                fileInfo.IsGallery = true;
            }
            else if (contentUrl.Contains("gif"))
            {
                await Logger.LOGI($"Bot {Thread.CurrentThread.Name} has found a Gif!", "Downloader");

                fileInfo.DownloadURL = contentUrl;
                fileInfo.FileExtension = ".gif";
            }
            else
            {
                await Logger.LOGI($"Bot {Thread.CurrentThread.Name} has found an unknown media type...", "Downloader");

                fileInfo.DownloadURL = contentUrl;
                fileInfo.FileTypes = FileTypes.Unknown;
                fileInfo.FileExtension = ".htm";
            }
        });

        fileInfo.PostTitle = await EnvironmentUtilities.SanitizeString(fileInfo.PostTitle);

        fileInfo.FileName = fileInfo.PostTitle + fileInfo.FileExtension;

        return fileInfo;
    }
}