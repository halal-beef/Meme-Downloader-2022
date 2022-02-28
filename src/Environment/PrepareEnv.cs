using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Dottik.MemeDownloader;

public static class EnvironmentConfig
{
    private static Stream stub = Stream.Null;
    public static bool ffmpegBad { get; private set; } = false;

    public static async Task<bool> CheckDependencyState()
    {
#nullable enable
        bool[]? passes = new bool[1];
        string? localPath = Environment.CurrentDirectory;
        string? expected_ffmpegHash;
        SHA256? sha256Gen = SHA256.Create();
#nullable restore

#if WINDOWS
        expected_ffmpegHash = "1dc6d8a3760819c22baf6878513bc43a735f1e6c9e7c19b52ee2e1e77294d8a9";
        // Compute FFMPEG Exe hash, and compare it to the expected one.
        if (File.Exists(localPath + "\\Dependencies\\ffmpeg.exe"))
        {
            Stream ffmpegStream = File.OpenRead(localPath + "\\Dependencies\\ffmpeg.exe");
            string ffmpegHash = HashToString(await sha256Gen.ComputeHashAsync(ffmpegStream));
            await ffmpegStream.DisposeAsync();
            ffmpegStream.Close();
            passes[0] = ffmpegHash.ToLower().Equals(expected_ffmpegHash.ToLower());
        }
        else { passes[0] = false; }
#elif LINUX
        expected_ffmpegHash = "B8ABA52A98315C8B23917CCCEFA86D11CD2D630C459009FECECE3752AD2155DC";
        // Compute FFMPEG binary hash, and compare it to the expected one.
        if (File.Exists(localPath + "/Dependencies/ffmpeg"))
        {
            Stream ffmpegStream = File.OpenRead(localPath + "/Dependencies/ffmpeg");
            string ffmpegHash = HashToString(await sha256Gen.ComputeHashAsync(ffmpegStream));
            await ffmpegStream.DisposeAsync();
            ffmpegStream.Close();
            passes[0] = ffmpegHash.ToLower().Equals(expected_ffmpegHash.ToLower());
        }
        else { passes[0] = false; }
#endif
        // Set vars
        ffmpegBad = !passes[0];
        // Dispose Elements.
        sha256Gen.Dispose();
        GC.Collect();

        return true;
    }

    public static async Task RestoreDependencies(bool overrideChecks = false)
    {
        if (ffmpegBad || overrideChecks)
        {
#if WINDOWS

            #region Download ffmpeg Windows Build.

            string tempDLPath = Path.GetTempFileName();
            FileStream tmpPth = File.OpenWrite(tempDLPath);

            await ProgramData.Client.GetStreamAsync("https://github.com/usrDottik/Stuff/releases/download/fmpgwin/ffmpeg.exe").Result.CopyToAsync(tmpPth);

            // Flush and Dispose!
            await tmpPth.FlushAsync();
            await tmpPth.DisposeAsync();
            tmpPth.Close();
            File.Move(tempDLPath, Environment.CurrentDirectory + "\\Dependencies\\ffmpeg.exe", true);
            GC.Collect();

            #endregion Download ffmpeg Windows Build.

#elif LINUX

            #region Download ffmpeg Linux Build.

            string tempDLPath = Path.GetTempFileName();
            string finalPath = Environment.CurrentDirectory + "/Dependencies/ffmpeg";
            FileStream tmpPth = File.OpenWrite(tempDLPath);

            await ProgramData.Client.GetStreamAsync("https://github.com/usrDottik/Stuff/releases/download/fmpglin/ffmpeg").Result.CopyToAsync(tmpPth);

            // Flush and Dispose!
            await tmpPth.FlushAsync();
            await tmpPth.DisposeAsync();
            tmpPth.Close();
            File.Move(tempDLPath, finalPath, true);
            Mono.Unix.UnixFileInfo ffmpegFInfo = new(finalPath);
            ffmpegFInfo.FileAccessPermissions = Mono.Unix.FileAccessPermissions.UserReadWriteExecute; // Hacker level permission editing
            GC.Collect();

            #endregion Download ffmpeg Linux Build.

#endif
        }
    }

    private static string HashToString(byte[] hash) => BitConverter.ToString(hash);
}