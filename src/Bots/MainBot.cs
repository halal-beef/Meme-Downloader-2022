using Dottik.MemeDownloader.Downloader;
using Dottik.MemeDownloader.Logging;
using Dottik.MemeDownloader.Utilities;
using Newtonsoft.Json.Linq;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Dottik.MemeDownloader.Bots;

public static class BotEvents
{
    public static EventHandler<BotCrashArgs> OnBotCrash { get; set; }
    public static EventHandler<BotCreationArgs> OnBotCreate { get; set; }
    public static EventHandler<BotDownloadArgs> OnBotFinishDownload { get; set; }

    public class BotCrashArgs : EventArgs
    {
        public Exception exception { get; set; }
    }

    public class BotCreationArgs : EventArgs
    {
        public string BotName { get; set; }
    }

    public class BotDownloadArgs : EventArgs
    {
        public string BotName { get; set; }
        public string FileName { get; set; }
        public ulong FileSize { get; set; }
    }
}

public class BotMain
{
    public static readonly string DownloadPath = Environment.CurrentDirectory + "\\Downloaded Content\\";
    public static volatile BotMain Instance;

    public static async void TriggerWhenBotCrash(object sender, BotEvents.BotCrashArgs arguments)
    {
        await Logger.LOGE(
            $"An Exception occured in \'{sender?.GetType()}\'! Stack Trace:\r\n" +
            "--------BEGIN STACK TRACE\r\n" +
            $"{arguments.exception?.ToString()}\r\n" +
            "--------END STACK TRACE",
            "BotLogic");
    }

    public static async void TriggerWhenBotCreated(object sender, BotEvents.BotCreationArgs arguments)
        => await Logger.LOGI($"Class of type \'{sender?.GetType()}\' has started a new bot with name \'{arguments?.BotName}\'", "BotLogic -> BotEvents.OnCreate");

    public async Task BotLogic(int listIdentifier)
    {
        BotEvents.BotCrashArgs crashArgs = new();
        try
        {
            while (true)
            {
                for (int iterator = 0; iterator < BotConfigurations.targetSubreddits?.Length; iterator++)
                {
                    string localDlPath = $"{DownloadPath}\\{BotConfigurations.targetSubreddits[iterator]}\\";

                    #region Get JSON

                    string _rand_postJson = await PostDownloder.GetRandomPostJson("shitposting");

                    #endregion Get JSON

                    #region Get Post Address

                    JObject _result = JObject.Parse(
                            JArray.Parse(_rand_postJson)[0]["data"]["children"][0]["data"].ToString()
                        );

                    #endregion Get Post Address

                    // TODO: Download Posts (DONE), Galleries (DONE), images (DONE) and videos.
                    FileInformation dlInfo = await MainDownloader.GetFileType(_rand_postJson);

                    #region Check if Gallery and Download.

                    if (dlInfo.IsGallery)
                    {
                        FormattedLinks galleryData = await GetRedditGallery.FormatLinks(_rand_postJson);
                        List<Stream> streams = await GetRedditGallery.GetGalleryAsync(galleryData);

                        for (int i = 0; i < streams.Count; i++)
                        {
                            // TODO: Download Galleries correctly, not just append .jpg to end.
                            FileStream newFile = File.Create($"{localDlPath}__GALLERY_{i}_{dlInfo.FileName + ".jpg"}");
                            await streams?[i].CopyToAsync(newFile);
                            await newFile.FlushAsync();
                            await newFile.DisposeAsync();
                            newFile.Close();
                        }
                        continue;
                    }

                    #endregion Check if Gallery and Download.

                    #region Check if Image and Download.

                    if (dlInfo.FileTypes == FileTypes.Image)
                    {
                        FileStream fs = File.Create($"{localDlPath}{dlInfo.FileName}");
                        await ProgramData.Client.GetStreamAsync(dlInfo.DownloadURL).Result.CopyToAsync(fs);
                        await fs.FlushAsync();
                        await fs.DisposeAsync();
                        fs.Close();
                    }

                    #endregion Check if Image and Download.

                    #region Check if Video and Download.

                    if (dlInfo.FileTypes is FileTypes.Video)
                    {
                        CancellationTokenSource cancellerOwner = new();
                        RedditVideoInformation info = RedditVideo.GetRedditVideoInfo(_rand_postJson);
                        Stream[] dataStreams = await RedditVideo.GetRedditVideoAsync(info);

                        // Save both streams to a temp file and merge using ffmpeg.
                        if (info.isAudioValid)
                        {
                            // Get two temp files.
                            string tempVidPath = Path.GetTempFileName();
                            string tempAudPath = Path.GetTempFileName();

                            // Open a filestream to both.
                            FileStream tempVidFStream = File.Open(tempVidPath, FileMode.OpenOrCreate);
                            FileStream tempAudFStream = File.Open(tempAudPath, FileMode.OpenOrCreate);

                            // Send the data from downloaded streams to the files.
                            await dataStreams[0].CopyToAsync(tempVidFStream);
                            await dataStreams[1].CopyToAsync(tempVidFStream);

                            // Flush, Dispose and Close streams.
                            EnvironmentUtilities.DestroyStreams(dataStreams);
                            EnvironmentUtilities.DestroyStream(tempVidFStream);
                            EnvironmentUtilities.DestroyStream(tempAudFStream);

                            // Start merging task
                            Task mergingTask = RedditVideo.MergeVideoAndAudioAsync(tempVidPath, tempAudPath, $"{DownloadPath}\\{dlInfo.FileName}", cancellerOwner.Token);

                            // Create timer and start it to measure the time of the merging.
                            Stopwatch timePassedSinceStart = new();
                            timePassedSinceStart.Start();

                            while (!mergingTask.IsCompleted)
                            {
                                await Task.Delay(5 * 1000);
                                // Wait 69 seconds and then cancel the task if it is not done.
                                if (timePassedSinceStart.ElapsedMilliseconds > 69 * 1000)
                                {
                                    // Cancel and break out of while block.
                                    cancellerOwner.Cancel();
                                    break;
                                }
                            }
                            // Stop it.
                            timePassedSinceStart.Stop();

                            // Delete temporal files
                            File.Delete(tempVidPath);
                            File.Delete(tempAudPath);

                            try
                            {
                                // If the final file is not valid, delete.
                                FileInfo finalInfo = new($"{DownloadPath}\\{dlInfo.FileName}");
                                if (finalInfo.Length is 0)
                                {
                                    finalInfo.Delete();
                                }
                            }
                            catch (Exception ex)
                            {
                                AnsiConsole.MarkupLine($"[yellow]WARNING[/]: [red]IO ERROR![/] Couldn't delete file in path [red]{DownloadPath}\\{dlInfo.FileName}[/]! \r\n[bold yellow]SysMsg: {ex.Message.RemoveMarkup()}[/]");
                                await Logger.LOGW($"Failed to delete file in path {DownloadPath}\\{dlInfo.FileName}! \r\n--------BEGIN STACK TRACE\r\n{ex}\r\n--------END STACK TRACE\r\n", "Downloader -> Video and Audio Merger");
                            }
                        }
                        else if (info.isAudioValid)
                        {
                            // Get a temp file.
                            string tempVidPath = Path.GetTempFileName();

                            // Open a filestream to it.
                            FileStream tempVidFStream = File.Open(tempVidPath, FileMode.OpenOrCreate);

                            // Send the data from downloaded stream to the file.
                            await dataStreams[0].CopyToAsync(tempVidFStream);

                            // Flush, Dispose and Close streams.
                            EnvironmentUtilities.DestroyStream(tempVidFStream);
                            EnvironmentUtilities.DestroyStreams(dataStreams);

                            // If the final file is not valid, delete.
                            FileInfo finalInfo = new(tempVidPath);
                            if (finalInfo.Length is 0)
                            {
                                // Delete if invalid.
                                finalInfo.Delete();
                            }
                            else
                            {
                                // Move to final place.
                                finalInfo.MoveTo($"{DownloadPath}\\Video-Only_{dlInfo.FileName}");
                            }
                        }
                    }

                    #endregion Check if Video and Download.

                    #region Check if unknown, and if so download it as an HTML file.

                    if (dlInfo.FileTypes == FileTypes.Unknown)
                    {
                        FileStream fs = File.Create($"{DownloadPath}\\{BotConfigurations.targetSubreddits[iterator]}\\{dlInfo.FileName}");
                        await ProgramData.Client.GetStreamAsync(dlInfo.DownloadURL).Result.CopyToAsync(fs);
                        await fs.FlushAsync();
                        await fs.DisposeAsync();
                        fs.Close();
                    }

                    #endregion Check if unknown, and if so download it as an HTML file.
                }
            }
        }
        catch (Exception ex)
        {
            try
            {
                BotConfigurations.bots[listIdentifier] = false;
            }
            catch
            {
                await Logger.LOGE("ERROR DISABLING BOT IN LIST!", "BotLogic -> Exception Handler");
            }

            AnsiConsole.MarkupLine($"{Thread.CurrentThread.Name} - Has ended it's task due to an [red]error." +
                "\r\n" +
                $"An Exception occured in \'{this.GetType()}\'! Stack Trace:\r\n" +
                "[yellow]--------BEGIN STACK TRACE[/]\r\n" +
                $"{ex}\r\n" +
                "[yellow]--------END STACK TRACE[/][/]\r\n");
            crashArgs.exception = ex;
            BotEvents.OnBotCrash?.Invoke(this, crashArgs);
        }
    }

    public async Task BotRestarter()
    {
        BotEvents.BotCrashArgs crashArgs = new();
        try
        {
            while (true)
            {
                BotEvents.BotCreationArgs _args = new();
                Predicate<bool> quickDetect = new(a => !a);
                float x = 0;
                x = BotConfigurations.bots.FindAll(quickDetect).Count - BotConfigurations.bots.Count;

                if (x < BotConfigurations.bots.Count / 2)
                {
                    for (int i = 0; i < BotConfigurations.bots.Count; i++)
                    {
                        i = BotConfigurations.bots.FindIndex(i, quickDetect);

                        if (i is not -1 && i >= 0 && i < BotConfigurations.bots.Count)
                        {
                            // Re-Run bots.
                            _args.BotName = $"Bot {i}";
                            Thread newBot = new(async () => await BotLogic(i));
                            newBot.Name = _args.BotName;
                            newBot.Start();
                            BotEvents.OnBotCreate?.Invoke(this, _args);

                            BotConfigurations.bots[i] = true;
                        }
                        else
                        {
                            break;
                        }
                    }

                    await Task.Delay(420);
                }
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine("Bot Restarter Has ended it's task due to an [red]error[/].");
            crashArgs.exception = ex;
            BotEvents.OnBotCrash?.Invoke(this, crashArgs);
        }
    }

    public async Task StartBots(bool multiThreading, int _threadAmount = -1)
    {
        BotEvents.BotCreationArgs _args = new();
        AnsiConsole.MarkupLine("Starting Bots...");

        BotConfigurations.multiThreaded = multiThreading;

        if (_threadAmount == -1 && multiThreading)
            _threadAmount = Environment.ProcessorCount;
        else if (!multiThreading)
            _threadAmount = 1;

        #region Subscribing to some Events.

        BotEvents.OnBotCreate += TriggerWhenBotCreated;
        BotEvents.OnBotCrash += TriggerWhenBotCrash;

        #endregion Subscribing to some Events.

        // Done, sort of.

        #region Start the bots with a loop

        for (int i = 0; i < _threadAmount; i++)
        {
            _args.BotName = $"Bot {i}";
            BotConfigurations.bots.Add(true);
            Thread newBot = new(async () => await BotLogic(i));
            newBot.Name = _args.BotName;
            newBot.Start();
            BotEvents.OnBotCreate?.Invoke(this, _args);
            Thread.Sleep(100);
        }

        #endregion Start the bots with a loop

        BotConfigurations.threadAmount = _threadAmount;
        await BotRestarter();
        await Task.Delay(-1); // Avoid program's termination.
    }
}

public struct BotConfigurations
{
    /// <summary>
    /// The list of <see cref="bool"/> to keep track of the bots running.
    /// </summary>
    public static volatile List<bool> bots = new();

    /// <summary>
    /// Is Running in MultiThreading mode.
    /// </summary>
    public static volatile bool multiThreaded = false;

    /// <summary>
    /// The target subreddits.
    /// </summary>
    public static volatile string[] targetSubreddits;

    /// <summary>
    /// The amount of threads in which bots should exist.
    /// </summary>
    public static volatile int threadAmount;
}