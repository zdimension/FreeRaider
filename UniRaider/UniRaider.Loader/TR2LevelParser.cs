using System.IO;

namespace UniRaider.Loader
{
    public class TR2LevelParser
    {
        public static TR2LevelVersion CurrentVersion;

        public static TR2Level ParseFile(string filePath)
        {
            var lvl = new TR2Level();

            using (var fs = new FileStream(filePath, FileMode.Open))
            {
                using (var br = new BinaryReader(fs))
                {
                    CurrentVersion = lvl.GameVersion = (TR2LevelVersion) br.ReadUInt32();
                    lvl.Palette = br.ReadArray<tr2_colour>(256);
                    lvl.Palette16 = br.ReadArray<tr2_colour4>(256);
                    
                }
            }
            return lvl;
        }
    }
}
