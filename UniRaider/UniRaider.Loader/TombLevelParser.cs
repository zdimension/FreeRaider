﻿using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace UniRaider.Loader
{
    public class TombLevelParser
    {
        public static ILevel ParseFile(string filePath)
        {
            ILevel lvl = null;

            using (var fs = new FileStream(filePath, FileMode.Open))
            {
                using (var br = new BinaryReader(fs))
                {
                    var ver = GameVersionHelper.ParseVersion(br, Path.GetExtension(filePath));

                    switch (ver)
                    {
                        case TR2LevelVersion.TR1:
                        case TR2LevelVersion.TR1UnfinishedBusiness:
                            lvl = TR1Level.Parse(br);
                            break;
                        case TR2LevelVersion.TR2:
                            lvl = TR2Level.Parse(br);
                            break;
                        case TR2LevelVersion.TR3:
                            lvl = TR3Level.Parse(br);
                            break;
                    }

                    lvl.GameVersion = ver;
                }
            }
            return lvl;
        }
    }
}
