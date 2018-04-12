using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicEditVideo
{
    class Video
    {
        public string path;
        public float length;

        public Video(string _path)
        {
            path = _path;
            length = EditingHelper.GetVideoDuration(path);
        }

     
    }
}
