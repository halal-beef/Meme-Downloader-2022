using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Dottik.MemeDownloader;

public static class EnvironmentConfig
{
#pragma warning disable CS1998
    private static readonly Stream stub;
    public static bool FFFMPEGBad { get; private set; }

    public static async Task<bool> CheckDependencyState()
    {
#nullable enable
        bool[]? passes = new bool[1];
        string? localPath = Environment.CurrentDirectory;
        string? expected_ffmpegHash;
        SHA256? sha256Gen = SHA256.Create();
#nullable restore

#if WINDOWS
        expected_ffmpegHash = "c54ab015637c845aa053e94a4ed769915610c0628490e3f33e98e4763f61a804";
        // Compute FFMPEG Exe hash, and compare it to the expected one.
        if (File.Exists(localPath + "\\Dependencies\\ffmpeg.exe"))
        {
            Stream ffmpegStream = File.OpenRead(localPath + "\\Dependencies\\ffmpeg.exe");
            string ffmpegHash = HashToString(await sha256Gen.ComputeHashAsync(ffmpegStream));
            await ffmpegStream.DisposeAsync();
            ffmpegStream.Close();
            passes[0] = ffmpegHash.ToLower().Equals(expected_ffmpegHash);
        }
        else { passes[0] = false; }
#elif LINUX
        expected_ffmpegHash = "B8ABA52A98315C8B23917CCCEFA86D11CD2D630C459009FECECE3752AD2155DC".ToLower();
        // Compute FFMPEG binary hash, and compare it to the expected one.
        if (File.Exists(localPath + "/Dependencies/ffmpeg"))
        {
            Stream ffmpegStream = File.OpenRead(localPath + "/Dependencies/ffmpeg");
            string ffmpegHash = HashToString(await sha256Gen.ComputeHashAsync(ffmpegStream));
            await ffmpegStream.DisposeAsync();
            ffmpegStream.Close();
            passes[0] = ffmpegHash.ToLower().Equals(expected_ffmpegHash);
        }
        else { passes[0] = false; }
#endif
        // Set vars
        FFFMPEGBad = !passes[0];
        // Dispose Elements.
        sha256Gen.Dispose();
        GC.Collect();

        return true;
    }

    public static async Task RestoreDependencies(bool overrideChecks = false)
    {
        if (overrideChecks || FFFMPEGBad)
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
            FileStream tmpPth = File.OpenWrite(tempDLPath);

            await ProgramData.Client.GetStreamAsync("https://github.com/usrDottik/Stuff/releases/download/fmpglin/ffmpeg").Result.CopyToAsync(tmpPth);

            // Flush and Dispose!
            await tmpPth.FlushAsync();
            await tmpPth.DisposeAsync();
            tmpPth.Close();
            File.Move(tempDLPath, Environment.CurrentDirectory + "/Dependencies/ffmpeg", true);
            Mono.Unix.UnixFileInfo ffmpegFInfo = new(Environment.CurrentDirectory + "/Dependencies/ffmpeg");
            ffmpegFInfo.FileAccessPermissions = Mono.Unix.FileAccessPermissions.UserReadWriteExecute; // Hacker level permission editing
            GC.Collect();

            #endregion Download ffmpeg Linux Build.

#endif
        }
    }

    private static string HashToString(byte[] hash) => BitConverter.ToString(hash);

#pragma warning restore CS1998
}