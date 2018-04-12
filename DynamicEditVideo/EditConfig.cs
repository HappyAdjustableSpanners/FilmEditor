using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicEditVideo
{
    class EditConfig
    {
        public EditingPatterns.EditingPreset editPreset;
        public List<Video> videos;
        public string backingMusicPath;
        public string introPath;
        public string outroPath;

        public EditConfig(EditingPatterns.EditingPreset _editPreset, List<Video> _videos, string _backingMusicPath = "none", string _introPath = "none", string _outroPath = "none")
        {
            editPreset = _editPreset;
            videos = _videos;
            backingMusicPath = _backingMusicPath;
            introPath = _introPath;
            outroPath = _outroPath;
        }
    }
}
