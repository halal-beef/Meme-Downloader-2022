﻿using Dottik.MemeDownloader.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Dottik.MemeDownloader.Downloader;

public struct FormattedLinks
{
    /// <summary>
    /// The Extensions of the media to downloaded in order to the links.
    /// </summary>
    public List<string> Extensions = new();

    /// <summary>
    /// The Links in order 0 -> 1 -> 2 -> 3 -> 4...
    /// </summary>
    public List<string> Links = new();

    public FormattedLinks()
    {
        // Filler code!
    }
}

public static class GetRedditGallery
{
    /// <summary>
    /// Give in the string of the JSON and it lets out the links and extensions to each image in the Reddit Gallery!
    /// </summary>
    /// <param name="Json">String of all the data in the JSON</param>
    /// <returns>A FormattedLinks struct with ordered data with links and extensions.</returns>
    public static async Task<FormattedLinks> FormatLinks(string Json)
    {
        FormattedLinks _galleryData = new();

        List<string> _linksId = new();

        StringBuilder extensionTemp = new();

        int _mediaIdCount;

        JObject _mediaIds, _mediaExtension;
        try
        {
            _mediaIds = GetGalleryData(Json);
            _mediaExtension = GetGalleryMetadata(Json);
            _mediaIdCount = GetJTokenChildrenLength(_mediaIds);
            await File.WriteAllTextAsync(Environment.CurrentDirectory + "\\galleryJson.json", Json);
            for (int i = 0; i < _mediaIds["items"].Count(); i++)
            {
                _linksId.Add(_mediaIds["items"][i]["media_id"].ToString());
            }

            for (int i = 0; i < _linksId.Count; i++)
            {
                extensionTemp?.Append(_mediaExtension?[_linksId[i]]?["m"]?.ToString());
                string extmp = extensionTemp.ToString();
                string exfinal = "";
                // Get Extension.
                if (extmp.Contains("jpg"))
                {
                    _galleryData.Extensions.Add(".jpg");
                    exfinal = ".jpg";
                }
                else if (extmp.Contains("png"))
                {
                    _galleryData.Extensions.Add(".png");
                    exfinal = ".png";
                }
                else if (extmp.Contains("jpeg"))
                {
                    _galleryData.Extensions.Add(".jpeg");
                    exfinal = ".jpeg";
                }

                _galleryData.Links.Add($"https://i.redd.it/{_linksId[i]}{exfinal}");
            }

            return _galleryData;
        }
        catch (Exception ex)
        {
            await Logger.LOGE($"Error processing Gallery! \r\nStack Trace: {JsonConvert.SerializeObject(ex.ToString(), Formatting.Indented)}", "Downloader -> Gallery");
        }

        return _galleryData;
    }

    /// <summary>
    /// Download images as a List of Streams.
    /// </summary>
    /// <param name="_formattedLinkData"></param>
    /// <Exception_ cref="Exception">Thrown if the Gallery data is invalid.</Exception_>
    /// <returns>A List of streams containing the data of images.</returns>
    public static async Task<List<Stream>> GetGalleryAsync(FormattedLinks _formattedLinkData)
    {
        List<Stream> finalStream = new();
        List<Task<Stream>> taskStreams = new();
        if (_formattedLinkData.Links.Count > 0 && _formattedLinkData.Extensions.Count > 0)
        {
            for (int i = 0; i < _formattedLinkData.Links.Count; i++)
            {
                taskStreams.Add(ProgramData.Client.GetStreamAsync(_formattedLinkData.Links[i]));
            }

            while (taskStreams.Count > 0)
            {
                Task<Stream> terminatedTask = await Task.WhenAny(taskStreams);

                // Remove the task that is already done.
                for (int i = 0; i < taskStreams.Count; i++)
                {
                    try
                    {
                        if (terminatedTask == taskStreams[i])
                        {
                            finalStream.Add(terminatedTask.Result);
                        }
                    }
                    catch (Exception ex)
                    {
                        await Logger.LOGE($"Bot {Thread.CurrentThread.Name} Could not get image from gallery with error \'{ex.Message}\'. " +
                            "Full Stack Trace -> \r\n" +
                            "--------START STACK TRACE\r\n" +
                            $"{ex}\r\n" +
                            "--------END STACK TRACE\r\n",
                            "Downloader -> GetGalleryAsync()");
                    }
                    finally { taskStreams.RemoveAt(i); }
                }
            }
        }
        else
        {
            await Logger.LOGE("Failed to get gallery! Invalid Gallery Data was presented.", "Downloader -> Gallery");
            throw new Exception("Download Error -> Invalid Gallery Data.");
        }

        return finalStream;
    }

    private static int GetJTokenChildrenLength(JObject token) => token.Children().ToArray().Length;

    private static JObject GetGalleryMetadata(string json) => JObject.Parse(JArray.Parse(json)[0]["data"]["children"][0]["data"]["media_metadata"].ToString());

    private static JObject GetGalleryData(string json) => JObject.Parse(JArray.Parse(json)[0]["data"]["children"][0]["data"]["gallery_data"].ToString());
}