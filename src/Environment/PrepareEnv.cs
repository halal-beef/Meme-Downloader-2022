﻿using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Dottik.MemeDownloader;

public static class EnvironmentConfig
{
    private static bool ffmpegBad = false;

    public static async Task<bool> CheckDependencyState()
    {
#nullable enable
        bool[]? passes = new bool[1];
        string? localPath = Environment.CurrentDirectory;
        const string? expected_ffmpegHash = "";
        SHA256? sha256Gen = SHA256.Create();
#nullable restore

        // Compute FFMPEG Exe hash, and compare it to the expected one.
        if (File.Exists(localPath + "\\Dependencies\\ffmpeg.exe"))
        {
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
        GC.Collect();

        return true;
    }

    public static async Task RestoreDependencies()
    {
        // TODO: Restore Specific package if it's invalid now.
        // TODO V2: Restore FFMPEG for Linux.
        if (ffmpegBad && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            string tempDLPath = Path.GetTempFileName();
            FileStream tmpPth = File.OpenWrite(tempDLPath);

            await ProgramData.Client.GetStreamAsync("").Result.CopyToAsync(tmpPth);

            // Flush and Dispose!
            await tmpPth.FlushAsync();
            await tmpPth.DisposeAsync();
            tmpPth.Close();
            File.Move(tempDLPath, Environment.CurrentDirectory + "\\Dependencies\\ffmpeg.exe", true);
            GC.Collect();
        }
    }

    private static string HashToString(byte[] hash) => BitConverter.ToString(hash);
}