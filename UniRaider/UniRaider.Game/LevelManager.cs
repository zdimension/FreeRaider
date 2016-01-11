using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics;
using UniRaider.Loader;

namespace UniRaider.Game
{
    public class LevelManager
    {
        public static Color4[] Palette16 { get; set; }
        public static Color4[] Palette8 { get; set; }






        public static void SetLevel(string filename)
        {
            var l = TombLevelParser.ParseFile(filename);
            if(l.GameVersion == TR2LevelVersion.TR1 || l.GameVersion == TR2LevelVersion.TR1UnfinishedBusiness)
            {
                loadTR1((TR1Level)l);
            }
            else if(l.GameVersion == TR2LevelVersion.TR2)
            {
                loadTR2((TR2Level)l);
            }
            else if(l.GameVersion == TR2LevelVersion.TR3)
            {
                loadTR3((TR3Level)l);
            }
        }
        
        private static void loadTR1(TR1Level lvl)
        {
            Palette8 = lvl.Palette.Select(x => new Color4(x.Red, x.Green, x.Blue, 0xFF)).ToArray();

        }

        private static void loadTR2(TR2Level lvl)
        {
            Palette8 = lvl.Palette.Select(x => new Color4(x.Red, x.Green, x.Blue, 0xFF)).ToArray();
            Palette16 = lvl.Palette16.Select(x => new Color4(x.Red, x.Green, x.Blue, x.Unused)).ToArray();
        }

        private static void loadTR3(TR3Level lvl)
        {
            Palette8 = lvl.Palette.Select(x => new Color4(x.Red, x.Green, x.Blue, 0xFF)).ToArray();
            Palette16 = lvl.Palette16.Select(x => new Color4(x.Red, x.Green, x.Blue, x.Unused)).ToArray();
        }
    }
}
