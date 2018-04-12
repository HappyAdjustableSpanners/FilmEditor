using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicEditVideo
{
    public static class FFMPEGExecuter
    {
        private static string _ffprobePath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName + "\\ffmpeg\\ffprobe.exe";
        private static string _ffmpegPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName + "\\ffmpeg\\ffmpeg.exe";

        public static string ExecuteFFProbe(string arguments)
        {
            using (Process ffProbe = new Process())
            {
                ffProbe.StartInfo.UseShellExecute = false;
                ffProbe.StartInfo.RedirectStandardOutput = true;
                ffProbe.StartInfo.FileName = _ffprobePath;

                ffProbe.StartInfo.Arguments = arguments;
                ffProbe.Start();

                string result = ffProbe.StandardOutput.ReadToEnd();
                ffProbe.WaitForExit();
                return result;
            }
        }

        public static void ExecuteFFMPEG(string arguments)
        {
            using (Process ffmpeg = new Process())
            {
                ffmpeg.StartInfo.UseShellExecute = false;
                ffmpeg.StartInfo.RedirectStandardOutput = true;
                ffmpeg.StartInfo.FileName = _ffmpegPath;

                ffmpeg.StartInfo.Arguments = arguments;
                ffmpeg.Start();

                ffmpeg.WaitForExit();
            }
        }

    }
}
