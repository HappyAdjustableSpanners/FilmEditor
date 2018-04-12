using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmergenceGuardian.FFmpeg;

namespace DynamicEditVideo
{
    class EditingHelper
    {

        //Get video duration from path
        public static float GetVideoDuration(string path)
        {
            string arguments = String.Format("-i \"{0}\" -show_entries format=duration -v quiet -of csv=\"p = 0\"", path);
            string result = FFMPEGExecuter.ExecuteFFProbe(arguments);
            return float.Parse(result);
        }

        public static void Edit(EditConfig config, int count)
        {

            DirectoryHelper.ClearDir(DirectoryHelper.GetParentFolderDir() + "\\tmp");
            // Split each video 
            SplitAndSaveParts(config.editPreset, config.videos, config.introPath, config.outroPath);

            // Create video
            CreateVideo(config, count);
        }

       

        private static int GetMinVideos(string[] folders)
        {
            int min = Directory.GetFiles(folders[0]).Count();
            for (int i = 0; i < folders.Length; i++)
            {
                int numVideos = Directory.GetFiles(folders[i]).Count();
                if (numVideos < min)
                {
                    min = numVideos;
                }
            }
            return min;
        }

        private static void CreateVideo(EditConfig config, int count)
        {
            // for each loop
            // get
            string[] folders = Directory.GetDirectories(DirectoryHelper.GetParentFolderDir() + "\\tmp");

            List<string> parts = new List<string>();

            //Get min amount of videos in all the folders
            int min = GetMinVideos(folders);

            parts.Add(DirectoryHelper.GetParentFolderDir() + "\\tmp\\intro.mp4");
            for (int j = 0; j < min; j++)
            {
                for (int i = 0; i < folders.Length; i++)
                {
                    parts.Add(folders[i] + "\\" + j + ".mp4");
                }
            }
            parts.Add(DirectoryHelper.GetParentFolderDir() + "\\tmp\\outro.mp4");

            if (config.outroPath != "none")
            {
                //string newPart = "file '" + config.outroPath.PadLeft('5') + "'";
                //partTxts.Add(newPart);
                //vpartTxts.Add(config.outroPath);
            }

            DirectoryInfo concatVideoPath = Directory.CreateDirectory(DirectoryHelper.GetParentFolderDir() + "\\output\\" + count + "\\");
            string path = concatVideoPath.FullName + config.editPreset.ToString() + ".mp4";
            
            //for(int i = 0; i < parts.Count(); i++)
            //{
            //    FFMPEGCommands.AddTransition(
            //        parts[0], 
            //        GetVideoDuration(parts[0]), 
            //        parts[1], GetVideoDuration(parts[1]),
            //        DirectoryHelper.AddBeforeExtension(path, "_trans"));

            //}
            FFMPEGCommands.AddTransition(parts[0], GetVideoDuration(parts[0]), parts[1], GetVideoDuration(parts[1]), DirectoryHelper.AddBeforeExtension(path, "_trans"));
            FFMPEGCommands.ConcatVideos(parts.ToArray(), path );
           
            //FFMPEGCommands.ConcatHudl(vpartTxts.ToArray(), apartTxts.ToArray(), path);
         
            if (config.backingMusicPath != "none")
            {
                string concatAudioVideoPath = DirectoryHelper.RemoveExtension(path);
                string cmd = FFMPEGCommands.OverlayAudio(path, config.backingMusicPath, concatAudioVideoPath + "_audio.mp4");
                FFMPEGExecuter.ExecuteFFMPEG(cmd);
            }
        }

        private static int GetPatternLength(EditingPatterns.EditingPreset preset)
        {
            switch (preset)
            {
                case EditingPatterns.EditingPreset.Action:
                    return EditingPatterns.actionPattern.Length;
                case EditingPatterns.EditingPreset.Relaxed:
                    return EditingPatterns.relaxedPattern.Length;
            }
            return -1;
        }

        private static void SplitAndSaveParts(EditingPatterns.EditingPreset editPreset, List<Video> videos, string introPath, string outroPath)
        {
            for (int i = 0; i < videos.Count; i++)
            {
                
                string outFolder = Directory.CreateDirectory(DirectoryHelper.GetParentFolderDir() + "\\tmp\\" + i).FullName;
                float start = 0;

                int patternLength = GetPatternLength(editPreset);

                FFMPEGCommands.GetAndSaveVideo(introPath, DirectoryHelper.GetParentFolderDir() + "\\tmp\\" + "\\intro");
                FFMPEGCommands.GetAndSaveVideo(outroPath, DirectoryHelper.GetParentFolderDir() + "\\tmp\\" + "\\outro");
                for (int j = 0; j < patternLength; j++)
                {
                    // duration of this part
                    int duration = EditingPatterns.GetPatternStep(editPreset, j);
                    float end = start + duration;

                    if (end > videos[i].length)
                    {
                        break;
                    }

                    // call ffmpeg to get this part and save it at x location
                    string outPath = outFolder + "\\" + j;
                    FFMPEGCommands.GetAndSaveVideoSection(videos[i].path, outPath, start, duration);

                    start = end;
                }

            }
        }
    }
}
