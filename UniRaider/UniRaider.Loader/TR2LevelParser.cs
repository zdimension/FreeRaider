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

                    #region Palette

                    lvl.Palette = br.ReadArray<tr2_colour>(256);
                    lvl.Palette16 = br.ReadArray<tr2_colour4>(256);

                    #endregion

                    #region Textures

                    lvl.NumTextiles = br.ReadUInt32();
                    lvl.Textile8 = br.ReadArray<tr2_textile8>(lvl.NumTextiles);
                    lvl.Textile16 = br.ReadArray<tr2_textile16>(lvl.NumTextiles);

                    #endregion

                    br.ReadUInt32(); // 32-bit unused value (4 bytes)

                    #region Rooms

                    lvl.NumRooms = br.ReadUInt16();
                    lvl.Rooms = br.ReadArray<tr2_room>(lvl.NumRooms);

                    #endregion

                    #region Floor Data

                    lvl.NumFloorData = br.ReadUInt32();

                    #endregion
                }
            }
            return lvl;
        }
    }
}
