﻿using System.IO;
using Spectre.Console;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Diagnostics;
using System.Text;
using System.Net.Http;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Runtime.InteropServices;
using System.Linq;
using System.Transactions;

namespace Dottik.MemeDownloader.Downloader;

public static class RedditVideo
{
    /// <summary>
    /// Gets all the information needed to download a reddit video.
    /// </summary>
    /// <param name="json">The raw JSON response from Reddit</param>
    /// <returns>a new <see cref="RedditVideoInformation"/> with all the infromation set.</returns>
    public static RedditVideoInformation GetRedditVideoInfo(string json)
    {
        string videoLink = GetVideoLink(json);
        string audioLink = GetAudioLink(json);
        bool isAudioValid = SendHeadRequest(audioLink);
        bool isGif = GetIfGif(json);

        return new(audioLink, videoLink, isAudioValid, isGif);
    }

    /// <summary>
    /// Download the video and the audio (if valid). Index 0 contains video. Index 1 contains audio (IF VALID)
    /// </summary>
    /// <param name="videoInformation">Data.</param>
    /// <returns>An array of <see cref="Stream"/> containing the Streams of the Reddit Video and Audio(IF VALID)</returns>
    public static async Task<Stream[]> GetRedditVideoAsync(RedditVideoInformation videoInformation)
    {
        Stream[] videoAndAudio;
        List<Task<Stream>> downloaderTasks = new();
        if (videoInformation.isAudioValid)
        {
            videoAndAudio = new Stream[2];
        }
        else
        {
            videoAndAudio = new Stream[1];
        }

        Task<Stream> vidDl = ProgramData.Client.GetStreamAsync(videoInformation.VideoLink);
        downloaderTasks.Add(vidDl);

        if (videoInformation.isAudioValid)
        {
            Task<Stream> audioDl = ProgramData.Client.GetStreamAsync(videoInformation.AudioLink);
            downloaderTasks.Add(audioDl);
        }

        while (downloaderTasks.Count > 0)
        {
            Task<Stream> endedTask = await Task.WhenAny(downloaderTasks);

            if (endedTask == vidDl)
            {
                if (!endedTask.IsFaulted)
                    videoAndAudio[0] = endedTask.Result;
            }
            else
            {
                if (!endedTask.IsFaulted)
                    videoAndAudio[1] = endedTask.Result;
            }

            await Task.Run(() => downloaderTasks.Remove(endedTask));
        }

        return videoAndAudio;
    }

    #region Lambdas

    private static string GetVideoLink(string rawJson) => JObject.Parse(JArray.Parse(rawJson)[0]["data"]["children"][0]["data"]["secure_media"].ToString())
        .GetValue("reddit_video")
        .Value<string>("fallback_url")
        .Split('?')[0];

    private static string GetAudioLink(string rawJson) => JObject.Parse(JArray.Parse(rawJson)[0]["data"]["children"][0]["data"].ToString())
        .Value<string>("url_overridden_by_dest") + "/DASH_audio.mp4";

    private static bool GetIfGif(string rawJson) => JObject.Parse(JArray.Parse(rawJson)[0]["data"]["children"][0]["data"]["secure_media"]["reddit_video"].ToString()).Value<bool>("is_gif");

    #endregion Lambdas

    /// <summary>
    /// Starts FFMPEG to merge video and audio.
    /// </summary>
    /// <param name="videoPath">Path to the video file.</param>
    /// <param name="audioPath">Path to the audio file.</param>
    /// <param name="finalName">Path to the end result.</param>
    /// <returns>A new Task representing when will FFMPEG close.</returns>
    public static Task MergeVideoAndAudioAsync(string videoPath, string audioPath, string finalName, CancellationToken cancelToken)
    {
        ProcessStartInfo ffmpegPSI = new()
        {
            Arguments = $"-i \"{videoPath}\" -i \"{audioPath}\" -vcodec copy -map 0:v -map 1:a \"{finalName}\"", // Set args to merge/remux vid.
            CreateNoWindow = true, // Make no Windows
            WindowStyle = ProcessWindowStyle.Hidden, // Make WindowStyle hidden
            UseShellExecute = false // Don't use the OS Shell.
        };
        Process ffmpegProc = new()
        {
            StartInfo = ffmpegPSI // Set Start info to be ffmpegPSI
        };
#if WINDOWS
        ffmpegPSI.FileName = $"{Environment.CurrentDirectory}\\Dependencies\\ffmpeg.exe";
#elif LINUX
        ffmpegPSI.FileName = $"{Environment.CurrentDirectory}/Dependencies/ffmpeg";
#endif

        ffmpegProc.Start();

        return ffmpegProc.WaitForExitAsync(cancelToken);
    }

    /// <summary>
    /// Sends a head request to <paramref name="url"/>
    /// </summary>
    /// <param name="url">The target URL.</param>
    /// <returns>true if the Http status code is success, else false.</returns>
    private static bool SendHeadRequest(string url)
    {
        // Send a HEAD request to the specified URL.
        HttpRequestMessage sendMsg = new() { RequestUri = new(url), Method = HttpMethod.Head };

        // Return if the code was a success.
        return ProgramData.Client.Send(sendMsg).IsSuccessStatusCode;
    }
}