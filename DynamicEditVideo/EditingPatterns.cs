using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicEditVideo
{
    public class EditingPatterns
    {
        public static int[] actionPattern  = { 2, 1, 2, 2, 2 };
        public static int[] relaxedPattern = { 4, 3, 3, 4, 3 };
        public enum EditingPreset { Action, Relaxed }

        public static int GetPatternStep(EditingPreset editPreset, int index)
        {
            switch (editPreset)
            {
                case EditingPreset.Action:
                    if (index > actionPattern.Length)
                    {
                        index = 0;
                    }
                    return actionPattern[index];
                case EditingPreset.Relaxed:
                    if(index > relaxedPattern.Length)
                    {
                        index = 0;
                    }
                    return relaxedPattern[index];
               default:
                    return -1;
            }
        }

        public static int GetPatternLength(EditingPreset preset)
        {
            switch (preset)
            {
                case EditingPreset.Action:
                    return actionPattern.Length;
                case EditingPreset.Relaxed:
                    return relaxedPattern.Length;
            }
            return -1;
        }
    }
}
