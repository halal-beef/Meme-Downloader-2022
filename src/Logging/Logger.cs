using System;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace Dottik.MemeDownloader.Logging;

public class Logger
{
    private static readonly int currentPID = Process.GetCurrentProcess().Id;
    /// <summary>
    /// Log INFO to the log file.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public static async Task LOGI(string message)
    {
        string trueMessage = $"{DateTime.Now.TimeOfDay.ToString().Split('.')[0]} - [{Thread.CurrentThread.Name}/Logger] {currentPID} I: {message}";
        await WriteLog(trueMessage);
    }
    /// <summary>
    /// Log WARN to the log file.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public static async Task LOGW(string message)
    {
        string trueMessage = $"{DateTime.Now.TimeOfDay.ToString().Split('.')[0]} - [{Thread.CurrentThread.Name}/Logger] {currentPID} W: {message}";
        await WriteLog(trueMessage);
    }
    /// <summary>
    /// Log ERROR to the log file.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public static async Task LOGE(string message)
    {
        string trueMessage = $"{DateTime.Now.TimeOfDay.ToString().Split('.')[0]} - [{Thread.CurrentThread.Name}/Logger] {currentPID} E: {message}";
        await WriteLog(trueMessage);
    }


    private static async Task WriteLog(string msg)
    {
        string targetFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + $"\\Dottik\\MD2022\\Logs\\",
            logFileName = $"{DateTime.Now.Date.ToString().Replace('/', '.').Split(' ')[0]}.log";
        if (!File.Exists(targetFolder + logFileName)) {
            await File.CreateText(targetFolder + logFileName).DisposeAsync();
        }
        await File.AppendAllTextAsync(targetFolder + logFileName, msg + "\r\n");
    }
}