using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UniRaider.Loader
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
        public short NumRPLs { get; set; }

        /// <summary>
        ///     number of in-game (engine-rendered) cutscenes (CUT*.TR2)
        /// </summary>
        public short NumCutScenes { get; set; }

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
        public string[] RPLFileNames { get; set; }
        public string[] LevelFileNames { get; set; }
        public string[] CutSceneFileNames { get; set; }
        public ushort[][] Script { get; set; }
        public ushort[] DemoLevelList { get; set; }
        public string[] GameStrings1 { get; set; }
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

        public static TOMBPCFile Parse(BinaryReader br)
        {
            try
            {
                var lvl = new TOMBPCFile();
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
                br.ReadByteArray(36);
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
                lvl.Language = (TOMBPCLanguage) br.ReadByte();
                lvl.SecretSoundID = br.ReadInt16();
                br.ReadByteArray(4);

                #endregion

                #region Strings

                lvl.LevelDisplayNames = br.ReadStringArray(lvl.NumLevels, lvl.XORbyte);
                lvl.ChapterScreens = br.ReadStringArray(lvl.NumChapterScreens, lvl.XORbyte);
                lvl.TitleFileNames = br.ReadStringArray(lvl.NumTitles, lvl.XORbyte);
                lvl.RPLFileNames = br.ReadStringArray(lvl.NumRPLs, lvl.XORbyte);
                lvl.LevelFileNames = br.ReadStringArray(lvl.NumLevels, lvl.XORbyte);
                lvl.CutSceneFileNames = br.ReadStringArray(lvl.NumCutScenes, lvl.XORbyte);

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
                    while (readingOp || (list[end] != (ushort) ScriptOpCode.OP_END))
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

        public static TOMBPCFile ParseFile(string filePath)
        {
            using (var fs = new FileStream(filePath, FileMode.Open))
            {
                using (var br = new BinaryReader(fs))
                {
                    return Parse(br);
                }
            }
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
        ///     0 -> normal game<br /> 1 -> demo
        /// </summary>
        DemoVersion = 1,

        /// <summary>
        ///     0 -> normal title screen<br /> 1 -> no title screen
        /// </summary>
        TitleDisabled = 2,

        /// <summary>
        ///     0 -> cheat mode enabled<br /> 1 -> no cheat mode
        /// </summary>
        CheatModeCheckDisabled = 4,

        /// <summary>
        ///     0 -> enter demo mode if no input timeout<br /> 1 -> wait forever if no input
        /// </summary>
        NoInputTimeout = 8,

        /// <summary>
        ///     0 -> load/save game enabled<br /> 1 -> load/save game disabled
        /// </summary>
        LoadSaveDisabled = 16,

        /// <summary>
        ///     0 -> screen re-sizing allowed<br /> 1 -> no screen re-sizing allowed
        /// </summary>
        ScreenSizingDisabled = 32,

        /// <summary>
        ///     0 -> normal option ring<br /> 1 -> ???
        /// </summary>
        LockOutOptionRing = 64,

        /// <summary>
        ///     ???
        /// </summary>
        DozyCheatEnabled = 128,

        /// <summary>
        ///     0 -> leave StringData as-is<br /> 1 -> XOR all StringData with XORbyte
        /// </summary>
        UseEncryption = 256,

        /// <summary>
        /// Usually set, no known effect
        /// </summary>
        Unknown = 512,

        /// <summary>
        ///     0 -> no level selection<br /> 1 -> allow player to select any level
        /// </summary>
        SelectAnyLevel = 1024,

        /// <summary>
        /// No known effect
        /// </summary>
        EnableCheatCode = 2048
    }

    public enum ScriptOpCode : ushort
    {
        OP_PICTURE = 0, // Unused in TR2. Or PSX? Used in TR3.
        OP_PSX_TRACK = 1, // Does not compile. PSX?
        OP_PSX_FMV = 2, // Does not compile. PSX?
        OP_FMV = 3, // Display FMV
        OP_GAME = 4, // Start a playable level
        OP_CUT = 5, // Display a cutscene
        OP_COMPLETE = 6, // Display level-completion stats
        OP_DEMO = 7, // Display demo sequence
        OP_PSX_DEMO = 8, // Does not compile. PSX?
        OP_END = 9, // Closes script sequence
        OP_TRACK = 10, // Play soundtrack (precedes level opcode)
        OP_SUNSET = 11, // Unknown, nothing changes in TR2. Start in Motorboat?
        OP_LOAD_PIC = 12, // Does not compile. PSX? Used in TR3.
        OP_DEADLY_WATER = 13, // Unknown, nothing changes in TR2.
        OP_REMOVE_WEAPONS = 14, // Start level without weapons
        OP_GAMECOMPLETE = 15, // End of game. Show stats, start credits sequence, music ID 52 in TR2.
        OP_CUTANGLE = 16, // Match N-S orientation of Room and animated characters.
        OP_NOFLOOR = 17, // Lara dies when her feet reach given depth.
        OP_STARTINV = 18, // Items given to Lara at level start (+1000), or at all secrets found (+0)
        OP_STARTANIM = 19, // Special animation of Lara when level starts
        OP_SECRETS = 20, // If zero, level does not account for secrets
        OP_KILLTOCOMPLETE = 21, // Kill all enemies to finish the level
        OP_REMOVE_AMMO = 22, // Lara starts level without ammo or medi packs
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
}