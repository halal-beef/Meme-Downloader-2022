using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Dottik.MemeDownloader.Logging;

public static class Logger
{
    private static readonly int currentPID = Environment.ProcessId;
    private static readonly object locker = new();

    /// <summary>
    /// Log ERROR to the log file.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="sender">From where does this message come from</param>
    public static async Task LOGE(string message, string sender)
    {
        string trueMessage = $"{DateTime.Now.TimeOfDay.ToString().Split('.')[0]} - [{Thread.CurrentThread.Name}/{sender}] {currentPID} E: {message}";
        await WriteLog(trueMessage);
    }

    /// <summary>
    /// Log INFO to the log file.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="sender">From where does this message come from</param>
    public static async Task LOGI(string message, string sender)
    {
        string trueMessage = $"{DateTime.Now.TimeOfDay.ToString().Split('.')[0]} - [{Thread.CurrentThread.Name}/{sender}] {currentPID} I: {message}";
        await WriteLog(trueMessage);
    }

    /// <summary>
    /// Log WARN to the log file.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="sender">From where does this message come from</param>
    public static async Task LOGW(string message, string sender)
    {
        string trueMessage = $"{DateTime.Now.TimeOfDay.ToString().Split('.')[0]} - [{Thread.CurrentThread.Name}/{sender}] {currentPID} W: {message}";
        await WriteLog(trueMessage);
    }

    private static async Task WriteLog(string msg)
    {
        await Task.Run(() =>
        {
            lock (locker)
            {
                string targetFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Dottik\\MD2022\\Logs\\",
                    logFileName = $"{DateTime.Now.Date.ToString().Replace('/', '.').Split(' ')[0]}.log";
                if (!File.Exists(targetFolder + logFileName))
                {
                    File.CreateText(targetFolder + logFileName).Dispose();
                }
#if WINDOWS
                File.AppendAllText(targetFolder + logFileName, msg + "\r\n");
#elif LINUX
                File.AppendAllText(targetFolder + logFileName, msg + "\n");
#else
                File.AppendAllText(targetFolder + logFileName, msg + $"{Environment.NewLine}"); // If platform is NOT define, then try to use Environment.NewLine to get the NewLine of the OS.
#endif
            }
        });
    }
}