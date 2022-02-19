using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Dottik.MemeDownloader;

public class EnvironmentConfig
{
    private static bool ffmpegBad = false;
    public static async Task<bool> CheckDependencyState()
    {
#nullable enable
        bool[]? passes = new bool[1];
        string? localPath = Environment.CurrentDirectory;
        string? expected_ffmpegHash = "";
        SHA256? sha256Gen = SHA256.Create();
#nullable restore

        // Compute FFMPEG Exe hash, and compare it to the expected one.
        if (File.Exists(localPath + "\\Dependencies\\ffmpeg.exe")) {
            Stream ffmpegStream = File.OpenRead(localPath + "\\Dependencies\\ffmpeg.exe");
            string ffmpegHash = HashToString(await sha256Gen.ComputeHashAsync(ffmpegStream));
            await ffmpegStream.DisposeAsync();
            ffmpegStream.Close();
            passes[0] = ffmpegHash.Equals(expected_ffmpegHash);
        }
        else { passes[0] = false; }

        // Set vars
        ffmpegBad = passes[0];
        // Dispose Elements.
        sha256Gen.Dispose();
        passes = null;
        localPath = null;
        expected_ffmpegHash = null;
        sha256Gen = null;
        GC.Collect();

        return true;
    }
    public static async Task RestoreDependencies() {
        // TODO: Restore Specific package if it's invalid now.
        if (ffmpegBad) {
            Memory<byte> tmpBuffer = new();
            string tempDLPath = Path.GetTempPath();
            FileStream tmpPth = File.OpenWrite(tempDLPath);
            
            Stream temp = await ProgramData.client.GetStreamAsync("");
                
            // Flush, Read and Dispose the stream.
            await temp.FlushAsync();
            await temp.ReadAsync(tmpBuffer);
            await temp.DisposeAsync();
                
            // Write to our temporal path, Flush and Dispose!
            await tmpPth.WriteAsync(tmpBuffer);
            await tmpPth.FlushAsync();
            await tmpPth.DisposeAsync();
            tmpBuffer = null;
            GC.Collect();
        }
    }
    private static string HashToString(byte[] hash) => BitConverter.ToString(hash);
}