using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FreeRaider.Loader
{
    public class TOMBPCFile
    {
        /// <summary>
        ///     seems to be 3 for TR2
        /// </summary>
        public TOMBPCGameVersion GameVersion { get; set; }

        /// <summary>
        ///     null-terminated string describing this game, copyright info, etc.
        /// </summary>
        public string CopyrightInfo { get; set; }

        /// <summary>
        ///     Level to go to when that happens (0x500 is exit-to-title) ??? when WHAT happens?
        /// </summary>
        public int FirstOption { get; set; }

        /// <summary>
        ///     Level to go to when that happens (-1 is NONE) ??? when WHAT happens?
        /// </summary>
        public int TitleReplace { get; set; }

        /// <summary>
        ///     Level to go to when Lara dies during demo mode (0x500 is exit-to-title)
        /// </summary>
        public int OnDeathDemoMode { get; set; }

        /// <summary>
        ///     Level to go to when Lara dies during the game (0 is exit-to-title)
        /// </summary>
        public int OnDeathInGame { get; set; }

        /// <summary>
        ///     time in game ticks (1/30th of a second?) to wait before starting a demo
        /// </summary>
        public int DemoTime { get; set; }

        /// <summary>
        ///     Level to go to when demo mode is interrupted (0x500 is exit-to-title)
        /// </summary>
        public int OnDemoInterrupt { get; set; }

        /// <summary>
        ///     Level to go to when the demo ends (0x500 is exit-to-title)
        /// </summary>
        public int OnDemoEnd { get; set; }

        /// <summary>
        ///     number of levels in the game (some level files are used more than once for some reason)
        /// </summary>
        public short NumLevels { get; set; }

        /// <summary>
        ///     chapter screens (Present in TR2, first used in TR3)
        /// </summary>
        public short NumChapterScreens { get; set; }

        /// <summary>
        ///     only one, TITLE.TR2
        /// </summary>
        public short NumTitles { get; set; }

        /// <summary>
        ///     number of FMV cutscenes (*.RPL)
        /// </summary>
        public short NumFMVs { get; set; }

        /// <summary>
        ///     number of in-game (engine-rendered) cutscenes (CUT*.TR2)
        /// </summary>
        public short NumCutscenes { get; set; }

        /// <summary>
        ///     Number of demo levels
        /// </summary>
        public short NumDemoLevels { get; set; }

        /// <summary>
        ///     ID of title soundtrack
        /// </summary>
        public short TitleSoundID { get; set; }

        /// <summary>
        ///     If doing only a single level
        /// </summary>
        public short SingleLevel { get; set; }

        /// <summary>
        ///     Various flags
        /// </summary>
        public TOMBPCFlags Flags { get; set; }


        /// <summary>
        /// Language of the script
        /// </summary>
        public TOMBPCLanguage Language { get; set; }

        /// <summary>
        ///     For encryption ("cipher code")
        /// </summary>
        public byte XORbyte { get; set; }

        /// <summary>
        ///     ID of "found a secret" soundtrack
        /// </summary>
        public short SecretSoundID { get; set; }

        public string[] LevelDisplayNames { get; set; }
        public string[] ChapterScreens { get; set; }
        public string[] TitleFileNames { get; set; }
        public string[] FMVFileNames { get; set; }
        public string[] LevelFileNames { get; set; }
        public string[] CutSceneFileNames { get; set; }
        public ushort[][] Script { get; set; }

        /// <summary>
        /// PSX only
        /// </summary>
        public PSXFMVInfo[] PSXFMVInfo { get; set; }

        public ushort[] DemoLevelIDs { get; set; }
        public string[] GameStrings1 { get; set; }
        /// <summary>
        /// Either PC or PSX strings
        /// </summary>
        public string[] GameStrings2 { get; set; }
        public string[][] Puzzles { get; set; }
        public string[][] Pickups { get; set; }
        public string[][] Keys { get; set; }

        private static bool[] OpcodeHasOperand { get; set; } = {
            true, true, true, true, true, true,
            false, true, true, false, true, false,
            true, false, false, false, true, true,
            true, true, true, false, false
        };

        public bool IsPSX { get; set; }

        public static TOMBPCFile Parse(BinaryReader br, bool psx = false)
        {
            try
            {
                var lvl = new TOMBPCFile();
                lvl.IsPSX = psx;
                lvl.GameVersion = (TOMBPCGameVersion) br.ReadUInt32();
                lvl.CopyrightInfo = br.ParseString(256);
                var gameflowSize = br.ReadUInt16();
                if (gameflowSize != 128)
                {
                    throw new ArgumentOutOfRangeException("gameflowSize [UInt16]", gameflowSize, "Should be 128");
                }

                #region First options

                lvl.FirstOption = br.ReadInt32();
                lvl.TitleReplace = br.ReadInt32();
                lvl.OnDeathDemoMode = br.ReadInt32();
                lvl.OnDeathInGame = br.ReadInt32();
                lvl.DemoTime = br.ReadInt32();
                lvl.OnDemoInterrupt = br.ReadInt32();
                lvl.OnDemoEnd = br.ReadInt32();
                br.ReadBytes(36); // Unknown2
                lvl.NumLevels = br.ReadInt16();
                lvl.NumChapterScreens = br.ReadInt16();
                lvl.NumTitles = br.ReadInt16();
                lvl.NumFMVs = br.ReadInt16();
                lvl.NumCutscenes = br.ReadInt16();
                lvl.NumDemoLevels = br.ReadInt16();
                lvl.TitleSoundID = br.ReadInt16();
                lvl.SingleLevel = br.ReadInt16();
                br.ReadBytes(32); // Unknown3
                lvl.Flags = (TOMBPCFlags) br.ReadUInt16();
                br.ReadBytes(6); // Unknown4
                lvl.XORbyte = br.ReadByte();
                lvl.Language = (TOMBPCLanguage) br.ReadByte();
                lvl.SecretSoundID = br.ReadInt16();
                br.ReadBytes(4); // Unknown5

                #endregion

                #region Strings

                var xor = lvl.Flags.HasFlag(TOMBPCFlags.UseEncryption) ? lvl.XORbyte : (byte)0;
                lvl.LevelDisplayNames = br.ReadStringArray(lvl.NumLevels, xor);
                lvl.ChapterScreens = br.ReadStringArray(lvl.NumChapterScreens, xor);
                lvl.TitleFileNames = br.ReadStringArray(lvl.NumTitles, xor);
                lvl.FMVFileNames = br.ReadStringArray(lvl.NumFMVs, xor);
                lvl.LevelFileNames = br.ReadStringArray(lvl.NumLevels, xor);
                lvl.CutSceneFileNames = br.ReadStringArray(lvl.NumCutscenes, xor);

                #endregion

                #region Script

                // This code is partially from Script.cpp of OpenRaider project
                var v = new List<List<ushort>>();
                var n = lvl.NumLevels + 1;
                var offset = br.ReadUInt16Array(n);
                var numBytes = br.ReadUInt16();
                var list = new ushort[(numBytes + 6) / 2];
                for (ushort i = 0; i < numBytes / 2; i++)
                {
                    list[i] = br.ReadUInt16();
                }
                lvl.DemoLevelIDs = br.ReadUInt16Array(lvl.NumDemoLevels);
                br.BaseStream.Position -= lvl.NumDemoLevels * 2;
                var hack = br.ReadUInt16Array(3);
                if (hack[0] == 19 && hack[1] == 20 && hack[2] == 21
                    || hack[0] == 21 && hack[1] == 22 && hack[2] == 23)
                {
                    list[numBytes / 2] = hack[0];
                    list[numBytes / 2 + 1] = hack[1];
                    list[numBytes / 2 + 2] = hack[2];
                }
                else
                {
                    br.BaseStream.Position -= 6;
                }

                var numPCStrings = 0;
                hack = br.ReadUInt16Array(3);
                if ((hack[0] == 1) && (hack[1] == 0) && (hack[2] == 864))
                {
                    br.BaseStream.Position += 58;
                    numPCStrings = 80; // TR2 has 80 PSX Strings
                }
                else if ((hack[0] == 1) && (hack[1] == 0) && (hack[2] == 817))
                {
                    br.BaseStream.Position += 34;
                    numPCStrings = 80; // TR3 also has 80 PSX Strings
                }
                else
                {
                    br.BaseStream.Position -= 6;
                    numPCStrings = 41;
                }

                for (uint i = 0; i < n; i++)
                {
                    uint end = (uint) (offset[i] / 2);

                    var readingOp = false;
                    while (readingOp || (list[end] != (ushort) ScriptOpCode.OP_ENDSEQUENCE))
                    {
                        if (readingOp)
                        {
                            readingOp = false;
                            end++;
                        }
                        else
                        {
                            if (OpcodeHasOperand[list[end]])
                                readingOp = true;

                            end++;
                        }
                    }

                    end++;

                    var tmp = new List<ushort>();
                    for (uint a = (uint) (offset[i] / 2); a < end; a++)
                    {
                        tmp.Add(list[a]);
                    }

                    v.Add(tmp);
                }

                lvl.Script = v.Select(x => x.ToArray()).ToArray();

                #endregion

                if (psx)
                {
                    lvl.PSXFMVInfo = br.ReadArray(lvl.NumFMVs, () => Loader.PSXFMVInfo.Read(br));
                }

                var numGameStrings = br.ReadUInt16();
                lvl.GameStrings1 = br.ReadStringArray(numGameStrings, lvl.XORbyte);
                lvl.GameStrings2 = br.ReadStringArray(numPCStrings, lvl.XORbyte);
                lvl.Puzzles = new[]
                {
                    br.ReadStringArray(lvl.NumLevels, lvl.XORbyte),
                    br.ReadStringArray(lvl.NumLevels, lvl.XORbyte),
                    br.ReadStringArray(lvl.NumLevels, lvl.XORbyte),
                    br.ReadStringArray(lvl.NumLevels, lvl.XORbyte)
                };
                lvl.Pickups = new[]
                {
                    br.ReadStringArray(lvl.NumLevels, lvl.XORbyte),
                    br.ReadStringArray(lvl.NumLevels, lvl.XORbyte)
                };
                lvl.Keys = new[]
                {
                    br.ReadStringArray(lvl.NumLevels, lvl.XORbyte),
                    br.ReadStringArray(lvl.NumLevels, lvl.XORbyte),
                    br.ReadStringArray(lvl.NumLevels, lvl.XORbyte),
                    br.ReadStringArray(lvl.NumLevels, lvl.XORbyte)
                };


                return lvl;
            }
            catch (Exception e)
            {
                throw new ScriptParseException(e, br.BaseStream.Position);
            }
        }

        public static TOMBPCFile ParseFile(string filePath, bool psx = false)
        {
            using (var fs = new FileStream(filePath, FileMode.Open))
            {
                using (var br = new BinaryReader(fs))
                {
                    return Parse(br, psx);
                }
            }
        }

        public void Write(string filename, bool psx = false)
        {
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }
            using (var fs = File.OpenWrite(filename))
            {
                using (var bw = new BinaryWriter(fs))
                {
                    Write(bw, psx);
                }
            }
        }

        public void Write(BinaryWriter bw, bool psx = false)
        {
            bw.Write((uint) GameVersion);
            bw.WriteStringASCII(CopyrightInfo, 256);
            bw.Write((ushort) 128); // gameflowSize

            bw.Write(FirstOption);
            bw.Write(TitleReplace);
            bw.Write(OnDeathDemoMode);
            bw.Write(OnDeathInGame);
            bw.Write(DemoTime);
            bw.Write(OnDemoInterrupt);
            bw.Write(OnDemoEnd);
            bw.Write(new byte[36]); // Unknown2
            bw.Write(NumLevels);
            bw.Write(NumChapterScreens);
            bw.Write(NumTitles);
            bw.Write(NumFMVs);
            bw.Write(NumCutscenes);
            bw.Write(NumDemoLevels);
            bw.Write(TitleSoundID);
            bw.Write(SingleLevel);
            bw.Write(new byte[32]); // Unknown3
            bw.Write((ushort) Flags);
            bw.Write(new byte[6]); // Unknown4
            bw.Write(XORbyte);
            bw.Write((byte) Language);
            bw.Write(SecretSoundID);
            bw.Write(new byte[4]); // Unknown5

            var xor = Flags.HasFlag(TOMBPCFlags.UseEncryption) ? XORbyte : (byte)0;
            bw.WriteStringArray(LevelDisplayNames, xor);
            bw.WriteStringArray(ChapterScreens, xor);
            bw.WriteStringArray(TitleFileNames, xor);
            bw.WriteStringArray(FMVFileNames, xor);
            bw.WriteStringArray(LevelFileNames, xor);
            bw.WriteStringArray(CutSceneFileNames, xor);

            var scrOffsets = new ushort[NumLevels + 1];
            ushort totalOffset = 0;
            for (var i = 0; i <= NumLevels; i++)
            {
                scrOffsets[i] = totalOffset;
                totalOffset += (ushort)(Script[i].Length * 2);
            }
            bw.WriteUInt16Array(scrOffsets);

            bw.Write((ushort)0);
            var nbPos = bw.BaseStream.Position;
            foreach (var a in Script.Resize(NumLevels + 1))
                bw.WriteUInt16Array(a);
            var tp = bw.BaseStream.Position;
            bw.BaseStream.Position = nbPos - 2;
            bw.Write((ushort)(tp - nbPos));
            bw.BaseStream.Position = tp;
            bw.WriteUInt16Array(DemoLevelIDs);

            if (psx)
            {
                bw.WriteArray(PSXFMVInfo.Resize(NumFMVs), x => x.Write(bw));
            }

            bw.Write((ushort) GameStrings1.Length);
            bw.WriteStringArray(GameStrings1, xor);
            bw.WriteStringArray(GameStrings2, xor);
            foreach (var puz in Puzzles.Resize(4))
                bw.WriteStringArray(puz.Resize(NumLevels), xor);
            foreach (var puz in Pickups.Resize(2))
                bw.WriteStringArray(puz.Resize(NumLevels), xor);
            foreach (var puz in Keys.Resize(4))
                bw.WriteStringArray(puz.Resize(NumLevels), xor);
        }
    }

    public enum TOMBPCGameVersion : uint
    {
        TR2_3 = 3
    }

    public enum TOMBPCLanguage : byte
    {
        English = 0,
        French = 1,
        German = 2,
        American = 3,
        Japanese = 4
    }

    [Flags]
    public enum TOMBPCFlags : ushort
    {
        /// <summary>
        /// Is this a demo version of the game
        /// </summary>
        DemoVersion = 1 << 0,

        /// <summary>
        /// Disables Title Screen
        /// </summary>
        TitleDisabled = 1 << 1,

        /// <summary>
        /// Related to in-game cheats?
        /// </summary>
        CheatModeCheckDisabled = 1 << 2,

        /// <summary>
        /// Disables input timeout for demo mode
        /// </summary>
        NoInputTimeout = 1 << 3,

        /// <summary>
        /// Disables loading/saving
        /// </summary>
        LoadSaveDisabled = 1 << 4,

        /// <summary>
        /// Disables screen resizing PC only?
        /// </summary>
        ScreenSizingDisabled = 1 << 5,

        /// <summary>
        /// Stops options ring from being selected
        /// </summary>
        LockoutOptionRing = 1 << 6,

        /// <summary>
        /// Enable DOZY (sometimes they removed the internal code)
        /// </summary>
        DozyCheatEnabled = 1 << 7,

        /// <summary>
        /// If true all string chars (except null termination) must be xor-ed by XorKey.
        /// </summary>
        UseEncryption = 1 << 8,

        /// <summary>
        /// Is Gym available on title screen.
        /// </summary>
        GymEnabled = 1 << 9,

        /// <summary>
        /// Unlocks all levels
        /// </summary>
        SelectAnyLevel = 1 << 10,

        /// <summary>
        /// Related to in-game cheats?
        /// </summary>
        EnableCheatCode = 1 << 11
    }

    public enum ScriptOpCode : ushort
    {
        OP_PICTURE = 0,            // ?
        OP_LISTSTART = 1,          // ?
        OP_LISTEND = 2,            // ?
        OP_STARTFMV = 3,           // Play FMV, operand is FMV ID
        OP_STARTLEVEL = 4,         // Play (interactive) level, operand is level ID
        OP_STARTCINE = 5,          // Play Cutscene, operand is cutscene ID
        OP_LEVELCOMPLETE = 6,      // Do level-completion display (no operands)
        OP_STARTDEMO = 7,          // Play demo level: operand is level ID
        OP_JUMPTOSEQUENCE = 8,     // Jumps to specified sequence?
        OP_ENDSEQUENCE = 9,        // End of sequence (no operands)
        OP_SETTRACK = 10,          // Play soundtrack: operand is soundtrack ID (it precedes opcodes of associated levels)
        OP_SUNSETENABLED = 11,     // Bartoli's Hideous
        OP_LOADINGPIC = 12,        // Chapter screen: operand is chapter ID
        OP_DEADLYWATER = 13,       // Temple of Xian? Kills Lara when she touches water?
        OP_REMOVEWEAPONS = 14,     // Lose your weapons (no operands)
        OP_GAMECOMPLETE = 15,      // End of game (no operands)
        OP_CUTANGLE = 16,          // Match N-S orientation of Room and animated characters. (one operand?)
        OP_NOFLOOR = 17,           // Lara dies when her feet reach given depth (for example Floating Islands or Thames Wharf) (one operand)
        OP_ADDTOINVENTORY = 18,    // Give item; operand is item type (see below)
        OP_LARASTARTANIMATION = 19,// Item-type 12 state to start level in: operand is state number
        OP_NUMSECRETS = 20,        // Number of secrets (overrides engine's hardcoded count of them?): operand is that number
        OP_KILLTOCOMPLETE = 21,    // Kill all enemies to finish the level (no operands)
        OP_REMOVEAMMO = 22,         // Lara starts level without ammo or medi packs
        OP_UNKNOWN = 23
    }

    public enum ScriptItem : ushort
    {
        OP_WEAPON_PISTOLS = 0, // Add standard pistols (2)
        OP_WEAPON_SHOTGUN = 1, // Add shotgun (1)
        OP_WEAPON_AUTOPISTOLS = 2, // Add automatic pistols (2)
        OP_WEAPON_UZIS = 3, // Add uzis (2)
        OP_WEAPON_HARPOON = 4, // Add harpoon gun (1)
        OP_WEAPON_M16 = 5, // Add M16 (1)
        OP_WEAPON_ROCKET = 6, // Add grenade launcher (1)
        OP_AMMO_PISTOLS = 7, // No effect, infinite ammo
        OP_AMMO_SHOTGUN = 8, // Add 2 shells
        OP_AMMO_AUTOPISTOLS = 9, // Add 2 shells
        OP_AMMO_UZIS = 10, // Add 2 shells
        OP_AMMO_HARPOON = 11, // Add 2 harpoons
        OP_AMMO_M16 = 12, // Add 2 shells
        OP_AMMO_ROCKET = 13, // Add 1 grenade
        OP_ITEM_FLARE = 14, // Add 1 flare
        OP_ITEM_MEDI = 15, // Add 1 small MediPack
        OP_ITEM_BIGMEDI = 16, // Add 1 big MediPack
        OP_ITEM_PICKUP1 = 17, // Add Pickup Item 1
        OP_ITEM_PICKUP2 = 18, // Add Pickup Item 2
        OP_ITEM_PUZZLE1 = 19, // Add Puzzle Item 1
        OP_ITEM_PUZZLE2 = 20, // Add Puzzle Item 2
        OP_ITEM_PUZZLE3 = 21, // Add Puzzle Item 3
        OP_ITEM_PUZZLE4 = 22, // Add Puzzle Item 4
        OP_ITEM_KEY1 = 23, // Add Key Item 1
        OP_ITEM_KEY2 = 24, // Add Key Item 2
        OP_ITEM_KEY3 = 25, // Add Key Item 3
        OP_ITEM_KEY4 = 26, // Add Key Item 4
        OP_ITEM_UNKNOWN = 27
    }

    public struct PSXFMVInfo
    {
        /// <summary>
        /// ? First FMV usually has this set as 1
        /// </summary>
        public uint Flags;
        /// <summary>
        /// Possibly: size, lba or length can't remember which
        /// </summary>
        public uint Unknown;

        public PSXFMVInfo(uint flags, uint unknown)
        {
            Flags = flags;
            Unknown = unknown;
        }

        public static PSXFMVInfo Read(BinaryReader br)
        {
            return new PSXFMVInfo(br.ReadUInt32(), br.ReadUInt32());
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(Flags);
            bw.Write(Unknown);
        }
    }
}