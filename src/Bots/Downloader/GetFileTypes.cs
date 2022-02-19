using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dottik.MemeDownloader.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Dottik.MemeDownloader.Downloader;
public class MainDownloader
{
    /// <summary>
    /// Analyze the whole requestJson string and return the file type identified + it's extension.
    /// </summary>
    /// <param name="requestJson"></param>
    /// <returns></returns>
    public static async Task<FileInformation> GetFileType(string requestJson) {
        FileInformation fileInfo = new();

        JObject Result = JObject.Parse(
            JArray.Parse(requestJson)[0]["data"]["children"][0]["data"].ToString()
        );
        string contentUrl = Result["url"].ToString();

        if (contentUrl.Contains("png") || contentUrl.Contains("jpg") || contentUrl.Contains("jpeg") || contentUrl.Contains("webp")) {
            await Logger.LOGI($"Bot {Thread.CurrentThread.Name} has found an Image!");
            if (contentUrl.Contains("png")) {
                fileInfo.FileExtension = ".png";
            }
            else if (contentUrl.Contains("jpg")) {
                fileInfo.FileExtension = ".jpg";
            }
            else if (contentUrl.Contains("jpeg")) {
                fileInfo.FileExtension = ".jpeg";
            }
            else { 
                fileInfo.FileExtension = ".webp"; 
            }
                
        } else if (contentUrl.Contains("mp4") || contentUrl.Contains("mov") || contentUrl.Contains("mkv")) {
            await Logger.LOGI($"Bot {Thread.CurrentThread.Name} has found a Video!");
            fileInfo.FileTypes = FileTypes.Video;

            if (contentUrl.Contains("mp4")) {
                fileInfo.FileExtension = ".mp4";
            } else if (contentUrl.Contains("mov")) {
                fileInfo.FileExtension = ".mov";
            } else {
                fileInfo.FileExtension = ".mkv";
            }
        } else if (contentUrl.Contains("gallery")) {
            await Logger.LOGI($"Bot {Thread.CurrentThread.Name} has found an Image Gallery!");
            fileInfo.isGallery = true;
        } else if (contentUrl.Contains("gif")) {
            await Logger.LOGI($"Bot {Thread.CurrentThread.Name} has found a Gif!");
            fileInfo.FileExtension = ".gif";
        } else {
            await Logger.LOGI($"Bot {Thread.CurrentThread.Name} has found an unknown media type...");
            fileInfo.FileTypes = FileTypes.Unknown;
            fileInfo.FileExtension = ".htm";
        }
        return fileInfo;
    }
}
