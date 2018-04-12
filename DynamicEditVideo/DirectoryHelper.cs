using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicEditVideo
{
    public class DirectoryHelper
    {
        public static string GetParentFolderDir()
        {
            return Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
        }

        public static string RemoveExtension(string path)
        {
            return System.IO.Path.ChangeExtension(path, null);
        }

        public static string AddBeforeExtension(string filePath, string stringToAdd)
        {
            string extension = Path.GetExtension(filePath);
            string outputPath = filePath.Substring(0, filePath.Length - extension.Length) + stringToAdd + extension;
            return outputPath;
        }

        public static void ClearDir(string path)
        {
            System.IO.DirectoryInfo di = new DirectoryInfo(path);

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }
        }
    }
}
