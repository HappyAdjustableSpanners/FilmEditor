using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmergenceGuardian.FFmpeg;
using Hudl.FFmpeg.Sugar;
using Hudl.FFmpeg.Command;
using Hudl.FFmpeg.Resources.BaseTypes;
using Hudl.FFmpeg.Filters.BaseTypes;
using Hudl.FFmpeg.Resources;
using Hudl.FFmpeg.Filters;
using Hudl.FFmpeg.Settings.BaseTypes;
using Hudl.FFmpeg.Settings;
using System.Reflection;

namespace DynamicEditVideo
{
    class FFMPEGCommands
    {
        public static void GetAndSaveVideoSection(string inPath, string outPath, float start, float duration)
        {
            string cmd = String.Format("-y -i \"{0}\" -ss {1} -t {2} -vcodec copy -acodec copy \"{3}.mp4\"", inPath, start, duration, outPath);
            FFMPEGExecuter.ExecuteFFMPEG(cmd);

            cmd = String.Format("-y -i \"{0}\" -ss {1} -t {2} -vcodec copy -acodec libmp3lame \"{3}.mp3\"", inPath, start, duration, outPath);
            FFMPEGExecuter.ExecuteFFMPEG(cmd);
        }

        public static void GetAndSaveVideo(string inPath, string outPath)
        {
            string cmd = String.Format("-y -i \"{0}\" -vcodec copy -acodec copy \"{1}.mp4\"", inPath, outPath);
            FFMPEGExecuter.ExecuteFFMPEG(cmd);

            cmd = String.Format("-y -i \"{0}\" -vcodec copy -acodec libmp3lame \"{1}.mp3\"", inPath, outPath);
            FFMPEGExecuter.ExecuteFFMPEG(cmd);
        }

        public static void MuxerConcat(IEnumerable<string> files, string outPath)
        {
            ProcessStartOptions options = new ProcessStartOptions();

            MediaMuxer.Concatenate(files, outPath, options);
        }

        public static void ConcatHudl(string[] vpaths, string[] apaths, string outPath)
        {
            var commandFactory = CommandFactory.Create();
            var outputSettings = SettingsCollection.ForOutput(
                new CodecVideo("libx264"),
                new BitRateVideo(3000),
                new Size(852, 480),
                new OverwriteOutput()
            );

            string dir3 = DirectoryHelper.GetParentFolderDir() + "\\output\\concat.mp4";

            var command = commandFactory.CreateOutputCommand();

            for (int i = 0; i < vpaths.Length - 1; i++)
            {
                command.AddInput(vpaths[i]);
            };
            for (int i = 0; i < apaths.Length - 1; i++)
            {
                command.AddInput(apaths[i]);
            };



            //select the first two video streams and run concat filter
            var videoConcat = command.Select(command.Take(0), command.Take(1), command.Take(2))
                .Filter(Filterchain.FilterTo<VideoStream>(new Hudl.FFmpeg.Filters.Concat));

            //select the first two audio streams and run concat filter
            var audioConcat = command.Select(command.Take(3), command.Take(4), command.Take(5))
                .Filter(Filterchain.FilterTo<VideoStream>(new Hudl.FFmpeg.Filters.Concat(1, 0)));

            command.Select(audioConcat, videoConcat)
                .MapTo<Mp4>(outPath, SettingsCollection.ForOutput(new OverwriteOutput()));


            commandFactory.Render();
        }

        public static void ConcatVideos(string[] parts, string outPath)
        {

            string streamsToCopy = "";
            string streamsToCopySetSar = "";
            string filesToInput = "";

            for(int i = 0; i < parts.Length; i++)
            {
                //streamsToCopySetSar += String.Format("[{0}:v:0]setsar={2}[v{0}];", i, 1);
                streamsToCopy += String.Format("[{0}:v:0] [{0}:a:0] ", i);
                filesToInput += String.Format(" -i \"{0}\"", parts[i]);
            }

            string cmd = String.Format("-y {0} -filter_complex \"{1} concat=n={2}:v=1:a=1 [v] [a]\" -map [v] -map [a] \"{3}\"",
                filesToInput.Trim(),
                streamsToCopy.Trim(),
                parts.Length,
                outPath);

            FFMPEGExecuter.ExecuteFFMPEG(cmd);
        }

        public static void AddTransition(string video1, string video2)
        {
         

        }

        public static void ConcatVideosDemux(string partsTxtPath, string outPath)
        {
            string cmd = String.Format("-y -f concat -safe 0 -i \"{0}\" -c:v libx264 \"{1}\"", partsTxtPath, outPath);
            FFMPEGExecuter.ExecuteFFMPEG(cmd);
        }

        public static void ConcatVideosNoReencode(string[] parts, string outPath)
        {
            //return String.Format("-y -f concat -safe 0 -i \"{0}\" -c:v libx264 \"{1}\"", partsTxtPath, outPath);

            //convert the .mp4 files to intermediate .TS
            string cmd2 = "-y -i \"concat:";

            for(int i = 0; i < parts.Length; i++)
            {
                string outP = DirectoryHelper.GetParentFolderDir() + "\\ts\\clip" + i + ".ts";
                string cmd1 = String.Format("-y -i \"{0}\" -c copy -bsf:v h264_mp4toannexb -f mpegts \"{1}\"", parts[i], outP);
                FFMPEGExecuter.ExecuteFFMPEG(cmd1);

                cmd2 += outP;
                if( i < parts.Length - 1)
                {
                    cmd2 += "|";
                }
            }

            //concatenate the TS files and convert the output back to mp4
            string options = "-bsf:a aac_adtstoasc -movflags faststart";
            cmd2 += String.Format("\" -c copy -video_track_timescale 25 {0} -f mp4 -threads 1 \"{1}\"", options, outPath);
            FFMPEGExecuter.ExecuteFFMPEG(cmd2);
        }

        public static string OverlayAudio(string video, string audio, string outPath)
        {
            //NOTE - If audio ends before the video it will cut the video short. Can fix this with padding video to length of audio at some point
            return String.Format("-y -i \"{0}\" -i \"{1}\" -c copy -shortest -map 0:0 -map 1:0 \"{2}\"", video, audio, outPath );
        }

    }
}
