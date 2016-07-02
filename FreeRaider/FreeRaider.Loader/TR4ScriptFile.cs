using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FreeRaider.Loader
{
    public enum TR4ScriptOptions : byte
    {
        /// <summary>
        /// Enables debug fly mode activated by typing DOZY ingame.
        /// </summary>
        FlyCheat = 0x01,
        /// <summary>
        /// When this bit is not set, load and save features are disabled. This option was used for demo versions.
        /// </summary>
        LoadSave = 0x02,
        /// <summary>
        /// Specifies if title screen should be displayed or not. If not set, game will start right away after user has launched an application.
        /// </summary>
        Title = 0x04,
        /// <summary>
        /// Gives an access to any level from the title screen.
        /// </summary>
        PlayAnyLevel = 0x08,
        /// <summary>
        /// Unknown feature, probably related to game versions deployed on promotional CDs.
        /// </summary>
        DemoDisc = 0x80
    }

    public class TR4ScriptFile
    {
        #region Global header

        /// <summary>
        /// Global game settings
        /// </summary>
        public TR4ScriptOptions Options;

        /// <summary>
        /// seconds before demo mode * 600
        /// </summary>
        public uint InputTimeout;

        public byte Security;

        #endregion

        #region Level header

        public byte NumTotalLevels;

        public ushort NumUniqueLevelPaths;

        public ushort LevelpathStringLen;

        public ushort LevelBlockLen;

        public string PSXLevelString;

        public string PSXFMVString;

        public string PSXCutString;

        public string PCLevelString;

        public string PCFMVString;

        public string PCCutString;

        #endregion

        #region Level listing

        public string[] LevelPaths;

        #endregion


    }

    public class TR4LanguageFile
    {
        public ushort NumGenericStrings;

        public ushort NumPSXStrings;

        public ushort NumPCStrings;

        public string[] StringTable;

        public string[] GenericStrings
        {
            get
            {
                var ret = new string[NumGenericStrings];
                Array.Copy(StringTable, 0, ret, 0, NumGenericStrings);
                return ret;
            }
        }

        public string[] PSXStrings
        {
            get
            {
                var ret = new string[NumPSXStrings];
                Array.Copy(StringTable, NumGenericStrings, ret, 0, NumPSXStrings);
                return ret;
            }
        }

        public string[] PCStrings
        {
            get
            {
                var ret = new string[NumPCStrings];
                Array.Copy(StringTable, NumGenericStrings + NumPSXStrings, ret, 0, NumPCStrings);
                return ret;
            }
        }

        public byte XORbyte => 0xA5; // Hard-coded

        public static TR4LanguageFile Read(string fname)
        {
            using (var fs = File.OpenRead(fname))
            {
                return Read(fs);
            }
        }

        public static TR4LanguageFile Read(Stream s)
        {
            using (var br = new BinaryReader(s))
            {
                return Read(br);
            }
        }

        public static TR4LanguageFile Read(BinaryReader br)
        {
            var ret = new TR4LanguageFile();

            ret.NumGenericStrings = br.ReadUInt16();
            ret.NumPSXStrings = br.ReadUInt16();
            ret.NumPCStrings = br.ReadUInt16();

            var genericStringsLen = br.ReadUInt16();
            var psxStringsLen = br.ReadUInt16();
            var pcStringsLen = br.ReadUInt16();

            var stringOffsetTable = br.ReadUInt16Array(ret.NumGenericStrings + ret.NumPSXStrings + ret.NumPCStrings);
            ret.StringTable = br.ReadStringArray(stringOffsetTable, ret.XORbyte);

            if (br.BaseStream.Position < br.BaseStream.Length)
            {
                Cerr.Write("Warning: " + (br.BaseStream.Length - br.BaseStream.Position) +
                           " bytes of data after end of language file");
            }

            return ret;
        }

        public void Write(string fname)
        {
            if (File.Exists(fname))
            {
                File.Delete(fname);
            }
            using (var fs = File.OpenWrite(fname))
            {
                Write(fs);
            }
        }

        public void Write(Stream s)
        {
            using (var bw = new BinaryWriter(s))
            {
                Write(bw);
            }
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(NumGenericStrings);
            bw.Write(NumPSXStrings);
            bw.Write(NumPCStrings);

            bw.Write(GenericStrings.Sum(x => Encoding.ASCII.GetByteCount(x) + 1));
            bw.Write(PSXStrings.Sum(x => Encoding.ASCII.GetByteCount(x) + 1));
            bw.Write(PCStrings.Sum(x => Encoding.ASCII.GetByteCount(x) + 1));

            var stringOTpos = bw.BaseStream.Position;
            var stringOffsetTable = new ushort[StringTable.Length];

            var stringStart = bw.BaseStream.Position + StringTable.Length * sizeof (ushort);
            bw.BaseStream.Position = stringStart;

            for (var i = 0; i < StringTable.Length; i++)
            {
                stringOffsetTable[i] = (ushort) (bw.BaseStream.Position - stringStart);
                bw.WriteStringASCII(StringTable[i]);
                bw.Write((byte)0x00); // null-terminator
            }

            bw.BaseStream.Position = stringOTpos;
            bw.WriteUInt16Array(stringOffsetTable);
        }
    }
}
