using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hudl.FFmpeg;
using Hudl.FFmpeg.Command;

namespace DynamicEditVideo
{
    class Program
    {
        static void Main(string[] args)
        {
            SetupFFMPEGWrapper();

            // Clean up 
            DirectoryHelper.ClearDir(DirectoryHelper.GetParentFolderDir() + "\\output");       

            // for each folder
            string dir = DirectoryHelper.GetParentFolderDir() + "\\videos";
            string[] folders = Directory.GetDirectories(dir);
            for(int i = 0; i < folders.Length; i++)
            {
                // Read in videos into an array from a folder 
                List<Video> videos = GetVideos(folders[i]);

                // Specify edit params
                //EditConfig editConfig = new EditConfig(EditingPatterns.EditingPreset.Relaxed, videos, DirectoryHelper.GetParentFolderDir() + "\\resources\\audio\\music.mp3", DirectoryHelper.GetParentFolderDir() + "\\resources\\introoutro\\intro.mp4", DirectoryHelper.GetParentFolderDir() + "\\resources\\introoutro\\outro.mp4");
                //EditingHelper.Edit(editConfig, i);

                // Specify edit params
                EditConfig editConfig2 = new EditConfig(EditingPatterns.EditingPreset.Action, videos, DirectoryHelper.GetParentFolderDir() + "\\resources\\audio\\music.mp3", DirectoryHelper.GetParentFolderDir() + "\\resources\\introoutro\\intro.mp4", DirectoryHelper.GetParentFolderDir() + "\\resources\\introoutro\\outro.mp4");
                EditingHelper.Edit(editConfig2, i);
            }
        }

        static void SetupFFMPEGWrapper()
        {
            string FFmpegPath = Path.Combine(DirectoryHelper.GetParentFolderDir() , "ffmpeg\\ffmpeg.exe");
            string FFprobePath = Path.Combine(DirectoryHelper.GetParentFolderDir() , "ffmpeg\\ffprobe.exe");
            string outputPath = Path.Combine(DirectoryHelper.GetParentFolderDir() , "output\\");
            ResourceManagement.CommandConfiguration = CommandConfiguration.Create(outputPath, FFmpegPath, FFprobePath);
     
        }

        static List<Video> GetVideos(string dir)
        {
            List<Video> videos = new List<Video>();
           
            foreach (string s in Directory.GetFiles(dir, "*.mp4").Select(Path.GetFileName))
            {
                Video video = new Video(dir + "\\" + s);
                videos.Add(video);
            }
            return videos;
        }
    }
}
