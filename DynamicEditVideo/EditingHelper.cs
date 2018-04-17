using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace DynamicEditVideo
{
    class EditingHelper
    {
        public static void Edit(EditConfig config, int count)
        {
            Thread.Sleep(5000);
            DirectoryHelper.ClearDir(DirectoryHelper.GetParentFolderDir() + "\\tmp");

            // Split each video 
            SplitAndSaveParts(config.editPreset, config.videos, config.introPath, config.outroPath);

            // Create video
            CreateVideo(config, count);
        }

        private static void SplitAndSaveParts(EditingPatterns.EditingPreset editPreset, List<Video> videos, string introPath, string outroPath)
        {
            // Split up all vids from videos directory into parts and into their relevant directory
            for (int i = 0; i < videos.Count; i++)
            {
                string outFolder = Directory.CreateDirectory(DirectoryHelper.GetParentFolderDir() + "\\tmp\\" + i).FullName;

                SaveParts(i, videos, editPreset, introPath, outroPath, outFolder);
            }
        }

        private static void SaveParts(
            int index,
            List<Video> videos,
            EditingPatterns.EditingPreset editPreset,
            string introPath,
            string outroPath,
            string outFolder)
        {
            float start = 0;

            int patternLength = EditingPatterns.GetPatternLength(editPreset);

            FFMPEGCommands.GetAndSaveVideo(introPath, DirectoryHelper.GetParentFolderDir() + "\\tmp\\" + "\\intro");
            FFMPEGCommands.GetAndSaveVideo(outroPath, DirectoryHelper.GetParentFolderDir() + "\\tmp\\" + "\\outro");
            for (int j = 0; j < patternLength; j++)
            {
                // duration of this part
                int duration = EditingPatterns.GetPatternStep(editPreset, j);
                float end = start + duration;

                if (end > videos[index].length)
                {
                    break;
                }

                // call ffmpeg to get this part and save it at x location
                string outPath = outFolder + "\\" + j;
                FFMPEGCommands.GetAndSaveVideoSection(videos[index].path, outPath, start, duration);
                start = end;
            }
        }

        private static void CreateVideo(EditConfig config, int count)
        {
            // Populate our video parts from folder directory
            string[] folders = Directory.GetDirectories(DirectoryHelper.GetParentFolderDir() + "\\tmp");
            List<string> parts = PopulateParts(folders);

            // Get the output path for this video
            string path = DirectoryHelper.GetOutputVideoPath(config.editPreset, count);

            // Build concatenated video
            SaveVideo(config, parts, path);
        }

        private static List<string> PopulateParts(string[] folders)
        {
            // Populate out parts for editing from tmp directory
            List<string> parts = new List<string>();
            int min = DirectoryHelper.GetMinFiles(folders);
            parts.Add(DirectoryHelper.GetParentFolderDir() + "\\tmp\\intro.mp4");
            for (int j = 0; j < min; j++)
            {
                for (int i = 0; i < folders.Length; i++)
                {
                    parts.Add(folders[i] + "\\" + j + ".mp4");
                }
            }
            parts.Add(DirectoryHelper.GetParentFolderDir() + "\\tmp\\outro.mp4");
            return parts;
        }

        private static void SaveVideo(EditConfig config, List<string> parts, string path)
        {
            string outPath = "";
            for (int i = 0; i < parts.Count - 1; i++)
            {
                if (outPath != "")
                {
                    string tempOutPath = "";
                    if (i == parts.Count - 2)
                    {
                        tempOutPath = path;
                    }
                    else
                    {
                        tempOutPath = DirectoryHelper.AddBeforeExtension(path, "_trans" + i);
                    }

                    if(i == 8)
                    {
                        Console.WriteLine("Test");
                    }
                    FFMPEGCommands.AddTransition(outPath, FFMPEGCommands.GetVideoDuration(outPath), parts[i + 1], FFMPEGCommands.GetVideoDuration(parts[i + 1]), tempOutPath);

                    // Clean up last video
                    File.Delete(outPath);
                    outPath = tempOutPath;
                }
                else
                {
                    outPath = DirectoryHelper.AddBeforeExtension(path, "_trans" + i);
                    FFMPEGCommands.AddTransition(parts[i], FFMPEGCommands.GetVideoDuration(parts[i]), parts[i + 1], FFMPEGCommands.GetVideoDuration(parts[i + 1]), outPath);
                }
            }

            // Add backing music if provided
            if (config.backingMusicPath != "none")
            {
                string concatAudioVideoPath = DirectoryHelper.RemoveExtension(path);
                string cmd = FFMPEGCommands.OverlayAudio(path, config.backingMusicPath, concatAudioVideoPath + "_audio.mp4", 1);
                FFMPEGExecuter.ExecuteFFMPEG(cmd);
            }
        }
    }
}

