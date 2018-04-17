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
        //Get video duration from path
        public static float GetVideoDuration(string path)
        {
            string arguments = String.Format("-i \"{0}\" -show_entries format=duration -v quiet -of csv=\"p = 0\"", path);
            string result = FFMPEGExecuter.ExecuteFFProbe(arguments);
            return float.Parse(result);
        }

        public static void GetAndSaveVideoSection(string inPath, string outPath, float start, float duration)
        {
            string cmd = String.Format("-y -i \"{0}\" -ss {1} -t {2} -c:v libx264 -acodec copy \"{3}.mp4\"", inPath, start, duration, outPath);
            FFMPEGExecuter.ExecuteFFMPEG(cmd);
        }

        public static void GetAndSaveVideoSectionReEncode(string inPath, string outPath, float start, float duration)
        {
            string cmd = String.Format("-y -i \"{0}\" -ss {1} -t {2} -c:v libx264 -acodec copy \"{3}.mp4\"", inPath, start, duration, outPath);
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
                .Filter(Filterchain.FilterTo<VideoStream>(new Hudl.FFmpeg.Filters.Concat()));

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

            for (int i = 0; i < parts.Length; i++)
            {
                string outP = DirectoryHelper.GetParentFolderDir() + "\\ts\\clip" + i + ".ts";
                string cmd1 = String.Format("-y -i \"{0}\" -c copy -bsf:v h264_mp4toannexb -f mpegts \"{1}\"", parts[i], outP);
                FFMPEGExecuter.ExecuteFFMPEG(cmd1);

                cmd2 += outP;
                if (i < parts.Length - 1)
                {
                    cmd2 += "|";
                }
            }

            //concatenate the TS files and convert the output back to mp4
            string options = "-bsf:a aac_adtstoasc -movflags faststart";
            cmd2 += String.Format("\" -c copy -video_track_timescale 25 {0} -f mp4 -threads 1 \"{1}\"", options, outPath);
            FFMPEGExecuter.ExecuteFFMPEG(cmd2);
        }

        public static void AddTransition(string video1, float v1Duration, string video2, float v2Duration, string outputPath)
        {
          
            //Construct ffmpeg command
            //Read in the input and create a filter complex
            string s1 = String.Format("-y -i \"{0}\" -i \"{1}\" -filter_complex \"", video1, video2);

            //Break the two videos into 2 parts each (so 4 parts total). 1st video is split into (content+fade-out), 2nd video is split into (fadeIn+content)

            //Trim first clip to start at 0 and end at video length
            string s2 = String.Format("[0:v]trim=start=0:end=\"{0}\",setpts=PTS-STARTPTS[firstclip];", v1Duration - 1);

            //Trim second clip to start at 1
            string s3 = String.Format("[1:v]trim=start=1,setpts=PTS-STARTPTS[secondclip];");

            //trim the final second of the video to use as the fade out. This subsection would normally keep its timestamps, but we set it to 0 using setpts=PTS-STARTPTS
            string s4 = String.Format("[0:v]trim=start=\"{0}\":end=\"{1}\",setpts=PTS-STARTPTS[fadeoutsrc];", v1Duration - 1, v1Duration);

            //trim the first second of the second video to fade out. This subsection would normally keep its timestamps, but we set it to 0 using setpts=PTS-STARTPTS
            string s5 = String.Format("[1:v]trim=start=0:end=1,setpts=PTS-STARTPTS[fadeinsrc];");

            //Now to actually fade in/out we need to specify a pixel format that has an alpha channel, so we use yuva420p.
            string s6 = String.Format("[fadeinsrc]format=pix_fmts=yuva420p, fade=t=in:st=0:d=1:alpha=1[fadein];[fadeoutsrc]format=pix_fmts=yuva420p,fade=t=out:st=0:d=1:alpha=1[fadeout];");

            //Ensure there is buffer space available in the buffer complex. It is actually an ffmpeg bug that this is not done by default
            string s7 = String.Format("[fadein]fifo[fadeinfifo];[fadeout]fifo[fadeoutfifo];");

            //Overlay the fade in and fade out to create the crossfade effect
            string s8 = String.Format("[fadeoutfifo][fadeinfifo]overlay[crossfade];");

            //Put the sections together
            string s9 = String.Format("[firstclip][crossfade][secondclip]concat=n=3[output];");

            //Add back in the audio
            string s10 = String.Format("[0:a][1:a]acrossfade=d=1[audio]\" -map \"[output]\" -map \"[audio]\" \"{0}\"", outputPath);

            //Create the argument list
            string cmd = s1 + s2 + s3 + s4 + s5 + s6 + s7 + s8 + s9 + s10;
            FFMPEGExecuter.ExecuteFFMPEG(cmd);
            
        }

        public static string OverlayAudio(string video, string audio, string outPath, float volume)
        {
            //NOTE - If audio ends before the video it will cut the video short. Can fix this with padding video to length of audio at some point
            return String.Format("-y -i \"{0}\" -i \"{1}\" -c copy -shortest -map 0:0 -map 1:0 \"{2}\"", video, audio, outPath);

        }
    }
}
