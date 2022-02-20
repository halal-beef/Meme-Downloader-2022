using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using Spectre.Console;
using System.Linq;
using Dottik.MemeDownloader.Logging;

namespace Dottik.MemeDownloader;
public struct FormattedLinks
{
    /// <summary>
    /// The Links in order 0 -> 1 -> 2 -> 3 -> 4...
    /// </summary>
    public List<string> Links = new();
    /// <summary>
    /// The Extensions of the media to downloaded in order to the links.
    /// </summary>
    public List<string> Extensions = new();
    public FormattedLinks() {

    }
}
public class GetRedditGallery
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

        StringBuilder extensionTemp = new("");

        int _mediaIdCount;

        JObject _mediaIds, _mediaExtension;
        try
        {
            _mediaIds = GetMediaIds(Json);
            _mediaExtension = GetMediaExtension(Json);
            _mediaIdCount = GetJTokenChildrenLength(_mediaIds);

            for (int i = 0; i < _mediaIdCount; i++)
            {
                _linksId.Add(_mediaIds["items"][i].ToString());
            }
            for (int i = 0; i < _linksId.Count; i++)
            {
                extensionTemp.Append(_mediaExtension[_linksId[i]]["m"].ToString());
                string extmp = extensionTemp.ToString();
                string exfinal = "";
                // Get Extension.
                if (extmp.Contains("jpg")) {
                    _galleryData.Extensions.Add(".jpg");
                    exfinal = ".jpg";
                }
                else if (extmp.Contains("png")) {
                    _galleryData.Extensions.Add(".png");
                    exfinal = ".png";
                }
                else if (extmp.Contains("jpeg")) {
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
    public static async Task GetGallery(string PathToResult, string sourceLink, FormattedLinks _formattedLinkData)
    {
        if (_formattedLinkData.Links.Count > 0 && _formattedLinkData.Extensions.Count > 0)
        {
            for (int i = 0; i < _formattedLinkData.Links.Count; i++)
            {
                if (!File.Exists(PathToResult + $"_GALLERY_IMAGE_{i}{_formattedLinkData.Extensions[i]}")) {
                    using FileStream fs0 = File.Create(PathToResult + $"_GALLERY_IMAGE_{i}{_formattedLinkData.Extensions[i]}");

                    await ProgramData.client.GetStreamAsync(_formattedLinkData.Links[i]).Result.CopyToAsync(fs0);

                    await fs0.FlushAsync();
                    await fs0.DisposeAsync();
                    fs0.Close();
                } else {
                    AnsiConsole.MarkupLine($"{Thread.CurrentThread.Name} - Has downloaded an already existing gallery.");
                }
            }
            AnsiConsole.MarkupLine($"{Thread.CurrentThread.Name} - Downloaded a Gallery from {sourceLink.RemoveMarkup()}");
        } else
        {
            await Logger.LOGE("Failed to get gallery! Invalid Gallery Data was presented.", "Downloader -> Gallery");
            throw new Exception("Invalid Gallery Data.");
        }
    }
    private static JObject GetMediaExtension(string json) => JObject.Parse(JArray.Parse(json)[0]["data"]["children"][0]["data"]["media_metadata"].ToString());
    private static JObject GetMediaIds(string json) => JObject.Parse(JArray.Parse(json)[0]["data"]["children"][0]["data"]["gallery_data"].ToString());
    private static int GetJTokenChildrenLength(JObject token) => token.Children().ToArray().Length;
}