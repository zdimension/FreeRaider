using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FreeRaider.Loader
{
    [Flags]
    public enum TR4ScriptOptions : byte
    {
        /// <summary>
        /// Enables debug fly mode activated by typing DOZY ingame.
        /// </summary>
        FlyCheat = 1 << 0,
        /// <summary>
        /// When this bit is not set, load and save features are disabled. This option was used for demo versions.
        /// </summary>
        LoadSave = 1 << 1,
        /// <summary>
        /// Specifies if title screen should be displayed or not. If not set, game will start right away after user has launched an application.
        /// </summary>
        Title = 1 << 2,
        /// <summary>
        /// Gives an access to any level from the title screen.
        /// </summary>
        PlayAnyLevel = 1 << 3,
        /// <summary>
        /// Unknown feature, probably related to game versions deployed on promotional CDs.
        /// </summary>
        DemoDisc = 1 << 7
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

        public byte NumTotalLevels => (byte)LevelPaths.Length;

        public ushort NumUniqueLevelPaths;

        public string PSXLevelString;

        public string PSXFMVString;

        public string PSXCutString;

        public string PSXFiller;

        public string PCLevelString;

        public string PCFMVString;

        public string PCCutString;

        public string PCFiller;

        #endregion

        #region Level listing

        public string[] LevelPaths = new string[0];

        public byte[][] LevelData = new byte[0][];

        #endregion

        public string[] LanguageFiles = new string[0];

        public static TR4ScriptFile Read(string fname)
        {
            using (var fs = File.OpenRead(fname))
            {
                return Read(fs);
            }
        }

        public static TR4ScriptFile Read(Stream s)
        {
            using (var br = new BinaryReader(s))
            {
                return Read(br);
            }
        }

        public static TR4ScriptFile Read(BinaryReader br)
        {
            var ret = new TR4ScriptFile();

            ret.Options = (TR4ScriptOptions)br.ReadByte();
            br.ReadBytes(3); // filler
            ret.InputTimeout = br.ReadUInt32();
            ret.Security = br.ReadByte();

            var numTotalLevels = br.ReadByte();
            ret.NumUniqueLevelPaths = br.ReadUInt16();

            var levelpathStringLen = br.ReadUInt16();
            var levelBlockLen = br.ReadUInt16();

            ret.PSXLevelString = br.ReadString(5);
            ret.PSXFMVString = br.ReadString(5);
            ret.PSXCutString = br.ReadString(5);
            ret.PSXFiller = br.ReadString(5);

            ret.PCLevelString = br.ReadString(5);
            ret.PCFMVString = br.ReadString(5);
            ret.PCCutString = br.ReadString(5);
            ret.PCFiller = br.ReadString(5);

            var pathsOffsets = br.ReadUInt16Array(numTotalLevels);

            var st = br.BaseStream.Position;

            ret.LevelPaths = br.ReadStringArray(pathsOffsets);

            br.BaseStream.Position = st + levelpathStringLen;

            var offset = br.ReadUInt16Array(numTotalLevels);

            st = br.BaseStream.Position;
            ret.LevelData = new byte[numTotalLevels][];
            for (var i = 0; i < numTotalLevels; i++)
            {
                br.BaseStream.Position = st + offset[i];
                ret.LevelData[i] = br.ReadBytes((i == numTotalLevels - 1 ? levelBlockLen : offset[i + 1]) - offset[i]);
            }

            br.BaseStream.Position = st + levelBlockLen;

            var tmp = new List<string>();

            while (br.BaseStream.Position != br.BaseStream.Length)
            {
                tmp.Add(br.ReadStringUntil());
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
            
        }
    }

    public class TR4LanguageFile
    {
        public string[] GenericStrings;

        public string[] PSXStrings;

        public string[] PCStrings;

        public string[] StringTable
        {
            get
            {
                var ret = new string[GenericStrings.Length + PSXStrings.Length + PCStrings.Length];
                Array.Copy(GenericStrings, 0, ret, 0, GenericStrings.Length);
                Array.Copy(PSXStrings, 0, ret, GenericStrings.Length, PSXStrings.Length);
                Array.Copy(PCStrings, 0, ret, GenericStrings.Length + PSXStrings.Length, PCStrings.Length);
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
            using (var br = new BinaryReader(s, Helper.CP437))
            {
                return Read(br);
            }
        }

        public static TR4LanguageFile Read(BinaryReader br)
        {
            var ret = new TR4LanguageFile();

            var numGenericStrings = br.ReadUInt16();
            var numPSXStrings = br.ReadUInt16();
            var numPCStrings = br.ReadUInt16();

            var genericStringsLen = br.ReadUInt16();
            var psxStringsLen = br.ReadUInt16();
            var pcStringsLen = br.ReadUInt16();

            var stringOffsetTable = br.ReadUInt16Array(numGenericStrings + numPSXStrings + numPCStrings);
            var stringTable = br.ReadStringArray(stringOffsetTable, ret.XORbyte, Helper.CP437);

            ret.GenericStrings = new string[numGenericStrings];
            Array.Copy(stringTable, 0, ret.GenericStrings, 0, numGenericStrings);

            ret.PSXStrings = new string[numPSXStrings];
            Array.Copy(stringTable, numGenericStrings, ret.PSXStrings, 0, numPSXStrings);

            ret.PCStrings = new string[numPCStrings];
            Array.Copy(stringTable, numGenericStrings + numPSXStrings, ret.PCStrings, 0, numPCStrings);

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
            bw.Write((ushort)GenericStrings.Length);
            bw.Write((ushort)PSXStrings.Length);
            bw.Write((ushort)PCStrings.Length);

            bw.Write((ushort)GenericStrings.Sum(x => Helper.CP437.GetByteCount(x) + 1));
            bw.Write((ushort)PSXStrings.Sum(x => Helper.CP437.GetByteCount(x) + 1));
            bw.Write((ushort)PCStrings.Sum(x => Helper.CP437.GetByteCount(x) + 1));

            var st = StringTable;
            var stringOTpos = bw.BaseStream.Position;
            var stringOffsetTable = new ushort[st.Length];

            var stringStart = bw.BaseStream.Position + st.Length * sizeof(ushort);
            bw.BaseStream.Position = stringStart;


            for (var i = 0; i < st.Length; i++)
            {
                stringOffsetTable[i] = (ushort)(bw.BaseStream.Position - stringStart);
                bw.WriteString(st[i], XORbyte, Helper.CP437);
                bw.Write((byte)0x00); // null-terminator
            }

            bw.BaseStream.Position = stringOTpos;
            bw.WriteUInt16Array(stringOffsetTable);
        }
    }

    public enum TR4ScriptOpcodes : byte
    {
        FMV = 0x80,
        Level = 0x81,
        TitleLevel = 0x82,
        LevelDataEnd = 0x83,
        Cut = 0x84,
        ResidentCut1 = 0x85,
        ResidentCut2 = 0x86,
        ResidentCut3 = 0x87,
        ResidentCut4 = 0x88,
        Layer1 = 0x89,
        Layer2 = 0x8A,
        UVrotate = 0x8B,
        Legend = 0x8C,
        LensFlare = 0x8D,
        Mirror = 0x8E,
        Fog = 0x8F,
        AnimatingMIP = 0x90,
        LoadCamera = 0x91,
        ResetHUB = 0x92,
        KeyItem1 = 0x93,
        KeyItem2 = 0x94,
        KeyItem3 = 0x95,
        KeyItem4 = 0x96,
        KeyItem5 = 0x97,
        KeyItem6 = 0x98,
        KeyItem7 = 0x99,
        KeyItem8 = 0x9A,
        KeyItem9 = 0x9B,
        KeyItem10 = 0x9C,
        KeyItem11 = 0x9D,
        KeyItem12 = 0x9E,
        PuzzleItem1 = 0x9F,
        PuzzleItem2 = 0xA0,
        PuzzleItem3 = 0xA1,
        PuzzleItem4 = 0xA2,
        PuzzleItem5 = 0xA3,
        PuzzleItem6 = 0xA4,
        PuzzleItem7 = 0xA5,
        PuzzleItem8 = 0xA6,
        PuzzleItem9 = 0xA7,
        PuzzleItem10 = 0xA8,
        PuzzleItem11 = 0xA9,
        PuzzleItem12 = 0xAA,
        PickupItem1 = 0xAB,
        PickupItem2 = 0xAC,
        PickupItem3 = 0xAD,
        PickupItem4 = 0xAE,
        Examine1 = 0xAF,
        Examine2 = 0xB0,
        Examine3 = 0xB1,
        KeyItem1Combo1 = 0xB2,
        KeyItem1Combo2 = 0xB3,
        KeyItem2Combo1 = 0xB4,
        KeyItem2Combo2 = 0xB5,
        KeyItem3Combo1 = 0xB6,
        KeyItem3Combo2 = 0xB7,
        KeyItem4Combo1 = 0xB8,
        KeyItem4Combo2 = 0xB9,
        KeyItem5Combo1 = 0xBA,
        KeyItem5Combo2 = 0xBB,
        KeyItem6Combo1 = 0xBC,
        KeyItem6Combo2 = 0xBD,
        KeyItem7Combo1 = 0xBE,
        KeyItem7Combo2 = 0xBF,
        KeyItem8Combo1 = 0xC0,
        KeyItem8Combo2 = 0xC1,
        PuzzleItem1Combo1 = 0xC2,
        PuzzleItem1Combo2 = 0xC3,
        PuzzleItem2Combo1 = 0xC4,
        PuzzleItem2Combo2 = 0xC5,
        PuzzleItem3Combo1 = 0xC6,
        PuzzleItem3Combo2 = 0xC7,
        PuzzleItem4Combo1 = 0xC8,
        PuzzleItem4Combo2 = 0xC9,
        PuzzleItem5Combo1 = 0xCA,
        PuzzleItem5Combo2 = 0xCB,
        PuzzleItem6Combo1 = 0xCC,
        PuzzleItem6Combo2 = 0xCD,
        PuzzleItem7Combo1 = 0xCE,
        PuzzleItem7Combo2 = 0xCF,
        PuzzleItem8Combo1 = 0xD0,
        PuzzleItem8Combo2 = 0xD1,
        PickupItem1Combo1 = 0xD2,
        PickupItem1Combo2 = 0xD3,
        PickupItem2Combo1 = 0xD4,
        PickupItem2Combo2 = 0xD5,
        PickupItem3Combo1 = 0xD6,
        PickupItem3Combo2 = 0xD7,
        PickupItem4Combo1 = 0xD8,
        PickupItem4Combo2 = 0xD9
    }
}
