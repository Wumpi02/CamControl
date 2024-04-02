using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using System;
using System.Diagnostics;
using System.IO;

namespace CamControl.Helpers
{
    public static class FFmpegHelper
    {
        public static bool IsFFmpegInstalled()
        {
            string pathToFFmpeg = FindFFmpegPath();
            if (File.Exists(pathToFFmpeg))
            {
                try
                {
                    using (Process process = new Process())
                    {
                        process.StartInfo.FileName = pathToFFmpeg;
                        process.StartInfo.RedirectStandardError = true;
                        process.StartInfo.RedirectStandardOutput = true;
                        process.StartInfo.UseShellExecute = false;
                        process.StartInfo.CreateNoWindow = true;

                        process.Start();
                        process.WaitForExit(); // Wait for the process to complete

                        int exitCode = process.ExitCode;
                        return exitCode == 0; // FFmpeg exited successfully
                    }
                }
                catch (Exception e)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        public static string FindFFmpegPath()
        {
            string[] paths = Environment.GetEnvironmentVariable("PATH").Split(Path.PathSeparator);
            string fullPath = null;
            PlatformID platform = Environment.OSVersion.Platform;

            foreach (string path in paths)
            {
                if (platform == PlatformID.Win32NT || platform == PlatformID.Win32Windows)
                {
                    fullPath = Path.Combine(path, "ffmpeg.exe"); // Windows
                }
                else if (platform == PlatformID.Unix)
                {
                    fullPath = Path.Combine(path, "ffmpeg"); // Linux 
                }

                if (File.Exists(fullPath))
                {
                    return fullPath;
                }
            }

            //Check for alternative 
            string[] searchDirectories =
        {
            @"C:\ffmpeg\bin",          
            "/usr/local/bin",          
            "/usr/bin",                
           @"C:\Program Files\ffmpeg\bin",
        };
           



            foreach (string directory in searchDirectories)
            {
                if (platform == PlatformID.Win32NT || platform == PlatformID.Win32Windows)
                {
                    fullPath = Path.Combine(directory, "ffmpeg.exe"); // Windows
                }
                else if (platform == PlatformID.Unix)
                {
                    fullPath = Path.Combine(directory, "ffmpeg"); // Linux
                }
                if (File.Exists(fullPath))
                {
                    return fullPath;
                }

            }
            return null; // FFmpeg executable not found
        }


    }


}
