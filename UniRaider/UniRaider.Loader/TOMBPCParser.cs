using System.IO;

namespace UniRaider.Loader
{
    public class TOMBPCParser
    {
        public static TOMBPCFile ParseFile(string filePath)
        {
            var lvl = new TOMBPCFile();
            using (var fs = new FileStream(filePath, FileMode.Open))
            {
                using (var br = new BinaryReader(fs))
                {
                    lvl.GameVersion = (TOMBPCGameVersion) br.ReadUInt32();
                    lvl.CopyrightInfo = br.ParseString(256);
                    lvl.FirstOption = br.ReadInt32();
                    lvl.TitleReplace = br.ReadInt32();
                    lvl.OnDeathDemoMode = br.ReadInt32();
                    lvl.OnDeathInGame = br.ReadInt32();
                    lvl.DemoTime = br.ReadInt32();
                    lvl.OnDemoInterrupt = br.ReadInt32();
                    lvl.OnDemoEnd = br.ReadInt32();
                    br.ReadByteArray(38); // filler FIX: Length is 38 instead of 36 (on TRosettaStone)
                    lvl.NumLevels = br.ReadInt16();
                    lvl.NumChapterScreens = br.ReadInt16();
                    lvl.NumTitles = br.ReadInt16();
                    lvl.NumRPLs = br.ReadInt16();
                    lvl.NumCutScenes = br.ReadInt16();
                    lvl.NumDemoLevels = br.ReadInt16();
                    lvl.TitleSoundID = br.ReadInt16();
                    lvl.SingleLevel = br.ReadInt16();
                    br.ReadByteArray(32); // filler
                    lvl.Flags = (TOMBPCFlags) br.ReadUInt16();
                    br.ReadByteArray(6); // filler
                    lvl.XORbyte = br.ReadByte();
                    lvl.SecretSoundID = br.ReadInt16();
                    br.ReadByteArray(4);
                    lvl.LevelDisplayNames = br.ReadStringArray(lvl.NumLevels);
                    if(lvl.Flags.HasFlag(TOMBPCFlags.Use_Encryption) || true)
                    {
                        lvl.LevelDisplayNames = lvl.LevelDisplayNames.XORArray((int) lvl.XORbyte);
                    }
                }
            }
            return lvl;
        }
    }
}
