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
            return min / 2;
        }

        private static void CreateVideo(EditConfig config, int count)
        {
            // for each loop
            // get
            string[] folders = Directory.GetDirectories(DirectoryHelper.GetParentFolderDir() + "\\tmp");

            List<string> vparts = new List<string>();
            List<string> aparts = new List<string>();

            //Get min amount of videos in all the folders
            int min = GetMinVideos(folders);

            vparts.Add(DirectoryHelper.GetParentFolderDir() + "\\tmp\\intro.mp4");
            aparts.Add(DirectoryHelper.GetParentFolderDir() + "\\tmp\\intro.mp3");
            for (int j = 0; j < min; j++)
            {
                for (int i = 0; i < folders.Length; i++)
                {
                    vparts.Add(folders[i] + "\\" + j + ".mp4");
                    aparts.Add(folders[i] + "\\" + j + ".mp3");
                }
            }
            vparts.Add(DirectoryHelper.GetParentFolderDir() + "\\tmp\\outro.mp4");
            aparts.Add(DirectoryHelper.GetParentFolderDir() + "\\tmp\\outro.mp3");

            // Create a txt file, a list of all files to be concatenated
            string partsTxtPath = DirectoryHelper.GetParentFolderDir() + "\\tmp\\parts.txt";
            List<string> vpartTxts = new List<string>();
            List<string> apartTxts = new List<string>();
            if (config.introPath != "none")
            {
                string newPart = "file '" + config.introPath.PadLeft('5') + "'";
                //partTxts.Add(newPart);
            }
            foreach(string part in vparts)
            {
                string newPart = "file '" + part.PadLeft('5') + "'"; ;
                //partTxts.Add(newPart);
                vpartTxts.Add(part);
            }
            foreach(string part in aparts)
            {
                apartTxts.Add(part);
            }
            if (config.outroPath != "none")
            {
                //string newPart = "file '" + config.outroPath.PadLeft('5') + "'";
                //partTxts.Add(newPart);
                //vpartTxts.Add(config.outroPath);
            }
            //System.IO.File.WriteAllLines(partsTxtPath, partTxts.ToArray());

            DirectoryInfo concatVideoPath = Directory.CreateDirectory(DirectoryHelper.GetParentFolderDir() + "\\output\\" + count + "\\");
            string path = concatVideoPath.FullName + config.editPreset.ToString() + ".mp4";
            //FFMPEGCommands.ConcatVideos(partTxts.ToArray(), path );
            FFMPEGCommands.ConcatHudl(vpartTxts.ToArray(), apartTxts.ToArray(), path);
         
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
