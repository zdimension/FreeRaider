using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace FreeRaider.Loader
{
    // Source: http://www.xythobuz.de/tr_docs/TombPC_TR2.pdf
    public class TOMBPCFile
    {
        public TOMBPCFile()
        {
            FirstOption = (uint) Actions["EXIT_TO_TITLE"];
            Title_Replace = Actions["NONE"];
            OnDeath_Demo_Mode = (uint) Actions["EXIT_TO_TITLE"];
            OnDeath_InGame = (uint) Actions["LEVEL"];
            On_Demo_Interrupt = (uint) Actions["EXIT_TO_TITLE"];
            On_Demo_End = (uint) Actions["EXIT_TO_TITLE"];
        }


        // names according to IceBerg's doc file

        /// <summary>
        /// A valid Tomb Raider II DAT file has a value of 3 in this field.
        /// </summary>
        public uint Script_Version = 3;

        /// <summary>
        /// For internal purposes
        /// </summary>
        public TOMBPCGameVersion Game_Version = TOMBPCGameVersion.TR2;

        /// <summary>
        /// This is a fixed length field, usually containing a null-terminated string describing the game and showing Core Design' signature.
        /// </summary>
        public string Description = $"Tomb Raider II Script. Final Release Version 1.1 (c) Core Design Ltd 1997. Generated using TRLevelUtility {Assembly.GetExecutingAssembly().GetName().Version.ToString(2)} (c) zdimension 2016.";

        /// <summary>
        /// What to do when the game starts, past the LEGAL.PCX and the LOGO.RPL full motion video.
        /// </summary>
        public uint FirstOption;

        /// <summary>
        /// Defines what happens when an EXIT_TO_TITLE is requested.
        /// </summary>
        public int Title_Replace;

        /// <summary>
        /// What to do when Lara dies during the demo mode.
        /// </summary>
        public uint OnDeath_Demo_Mode;

        /// <summary>
        /// What to do to when Lara dies during the game.
        /// </summary>
        public uint OnDeath_InGame;

        /// <summary>
        /// Time to wait before starting the demo mode. This is the number of 1/30th of a second.
        /// </summary>
        public uint NoInput_Time = 900;

        /// <summary>
        /// What to do when the demo mode is interrupted
        /// </summary>
        public uint On_Demo_Interrupt;

        /// <summary>
        /// What to do when the demo mode ends.
        /// </summary>
        public uint On_Demo_End;

        /// <summary>
        /// ID of the title' soundtrack.
        /// </summary>
        public ushort Title_Track;

        /// <summary>
        /// The game plays only one single level, overriding the script.
        /// Which level? That depends on the value stored in this field.A value of 1 indicates the first level, a value of 2 indicates the second level, etc. 
        /// </summary>
        public ushort SingleLevel = ushort.MaxValue;

        /// <summary>
        /// Various flags enabling or disabling several options.
        /// </summary>
        public TOMBPCFlags Flags = TOMBPCFlags.Use_Security_Tag;

        /// <summary>
        /// Cypher byte used to encrypt the strings in the script file. 
        /// Core Design used the value 166 ($A6) to XOR the TR2 strings.
        /// </summary>
        public byte Cypher_Code = 0xA6;

        /// <summary>
        /// Language of the script
        /// </summary>
        public TOMBPCLanguage Language;

        /// <summary>
        /// ID of the "found a secret" soundtrack.
        /// </summary>
        public ushort Secret_Track;

        public string[] Level_Names = new string[0];
        public string[] Picture_Filenames = new string[0];
        public string[] Title_Filenames = new string[0];
        public string[] FMV_Filenames = new string[0];
        public string[] Level_Filenames = new string[0];
        public string[] Cutscene_Filenames = new string[0];
        public ushort[][] Script = new ushort[0][];

        /// <summary>
        /// PSX only
        /// </summary>
        public PSXFMVInfo[] PSXFMVInfo = new PSXFMVInfo[0];

        public ushort[] DemoLevelIDs = new ushort[0];
        public string[] Game_Strings = new string[0];

        /// <summary>
        /// Either PC or PSX strings
        /// </summary>
        public string[] PC_Strings = new string[0];

        public string[] PSX_Strings = new string[0];

        public string[][] Puzzles = {new string[0], new string[0], new string[0], new string[0]};
        public string[][] Pickups = {new string[0], new string[0]};
        public string[][] Keys = {new string[0], new string[0], new string[0], new string[0]};

        // For TR2 PSX Beta
        public string[][] Secrets = {new string[0], new string[0], new string[0], new string[0]};
        public string[][] Special = {new string[0], new string[0]};

        private static readonly bool[] OpcodeHasOperand =
        {
            true, true, true, true, true, true,
            false, true, true, false, true, false,
            true, false, false, false, true, true,
            true, true, true, false, false
        };

        public string stringsFilename = "";

        private static readonly Dictionary<string, int> Actions = new Dictionary<string, int>
        {
            {"NONE", -1},
            {"LEVEL", 0x00000000},
            {"SEQUENCE", 0x00000000},
            //{"SAVEDGAME", 0x000000FF},
            //{"STARTSAVEDGAME", 0x000000FF},
            //{"CINE", 0x00000200},
            //{"STARTCINE", 0x00000200},
            //{"FMV", 0x00000300},
            //{"STARTFMV", 0x00000300},
            {"DEMO", 0x00000400},
            //{"STARTDEMO", 0x00000400},
            {"EXIT_TO_TITLE", 0x00000500},
            {"EXITGAME", 0x00000700},
            //{"EXIT_TO_OPTION", 0x00000800},
            {"TITLE_DESELECT", 0x00000900},

            /*{"DISABLED", -1},
            {"ENABLED", -1},
            {"ON", 1},
            {"OFF", 1},*/
        };

        public static readonly Dictionary<string, ushort> Items2 = new Dictionary<string, ushort>
        {
            {"PISTOLS", 0},
            {"SHOTGUN", 1},
            {"AUTOPISTOLS", 2},
            {"UZIS", 3},
            {"HARPOON", 4},
            {"M16", 5},
            {"ROCKET", 6},
            {"GRENADE", 6},
            {"PISTOLS_AMMO", 7},
            {"SHOTGUN_AMMO", 8},
            {"AUTOPISTOLS_AMMO", 9},
            {"UZI_AMMO", 10},
            {"UZIS_AMMO", 10},
            {"HARPOON_AMMO", 11},
            {"M16_AMMO", 12},
            {"ROCKET_AMMO", 13},
            {"GRENADE_AMMO", 13},
            {"FLARES", 14},
            {"MEDI", 15},
            {"BIGMEDI", 16},
            {"PICKUP1", 17},
            {"PICKUP2", 18},
            {"PUZZLE1", 19},
            {"PUZZLE2", 20},
            {"PUZZLE3", 21},
            {"PUZZLE4", 22},
            {"KEY1", 23},
            {"KEY2", 24},
            {"KEY3", 25},
            {"KEY4", 26}
        };

        public static readonly Dictionary<string, ushort> Items3 = new Dictionary<string, ushort>
        {
            {"PISTOLS", 0},
            {"SHOTGUN", 1},
            {"DESERTEAGLE", 2},
            {"UZIS", 3},
            {"HARPOON", 4},
            {"MP5", 5},
            {"ROCKET", 6},
            {"GRENADE", 7},
            {"PISTOLS_AMMO", 8},
            {"SHOTGUN_AMMO", 9},
            {"DESERTEAGLE_AMMO", 10},
            {"UZI_AMMO", 11},
            {"UZIS_AMMO", 11},
            {"HARPOON_AMMO", 12},
            {"MP5_AMMO", 13},
            {"ROCKET_AMMO", 14},
            {"GRENADE_AMMO", 15},
            {"FLARES", 16},
            {"MEDI", 17},
            {"BIGMEDI", 18},
            {"PICKUP1", 19},
            {"PICKUP2", 20},
            {"PUZZLE1", 21},
            {"PUZZLE2", 22},
            {"PUZZLE3", 23},
            {"PUZZLE4", 24},
            {"KEY1", 25},
            {"KEY2", 26},
            {"KEY3", 27},
            {"KEY4", 28},
            {"CRYSTAL", 29}
        };

        public bool IsPSX;

        public static TOMBPCFile ParseDAT(BinaryReader br, bool psx = false, bool isTR2beta = false)
        {
            try
            {
                var lvl = new TOMBPCFile();
                lvl.IsPSX = psx;
                lvl.Script_Version = br.ReadUInt32();
                lvl.Description = br.ParseString(256, true);
                var gameflowSize = br.ReadUInt16();
                if (gameflowSize != 128)
                {
                    throw new ArgumentOutOfRangeException("gameflowSize [UInt16]", gameflowSize, "Should be 128");
                }

                #region First options

                lvl.FirstOption = br.ReadUInt32();
                lvl.Title_Replace = br.ReadInt32();
                lvl.OnDeath_Demo_Mode = br.ReadUInt32();
                lvl.OnDeath_InGame = br.ReadUInt32();
                lvl.NoInput_Time = br.ReadUInt32();
                lvl.On_Demo_Interrupt = br.ReadUInt32();
                lvl.On_Demo_End = br.ReadUInt32();
                br.ReadBytes(36); // Unknown2
                var num_Levels = br.ReadUInt16();
                var num_Pictures = br.ReadUInt16();
                var num_Titles = br.ReadUInt16();
                var num_FMVs = br.ReadUInt16();
                var num_Cutscenes = br.ReadUInt16();
                var num_Demos = br.ReadUInt16();
                lvl.Title_Track = br.ReadUInt16();
                lvl.SingleLevel = br.ReadUInt16();
                br.ReadBytes(32); // Unknown3
                lvl.Flags = (TOMBPCFlags) br.ReadUInt16();
                br.ReadBytes(6); // Unknown4
                lvl.Cypher_Code = br.ReadByte();
                lvl.Language = (TOMBPCLanguage) br.ReadByte();
                lvl.Secret_Track = br.ReadUInt16();
                br.ReadBytes(4); // Unknown5

                #endregion

                #region Strings

                var xor = lvl.Flags.HasFlag(TOMBPCFlags.Use_Security_Tag) ? lvl.Cypher_Code : (byte) 0;
                lvl.Level_Names = br.ReadStringArray(num_Levels, xor);
                lvl.Picture_Filenames = br.ReadStringArray(num_Pictures, xor);
                lvl.Title_Filenames = br.ReadStringArray(num_Titles, xor);
                lvl.FMV_Filenames = br.ReadStringArray(num_FMVs, xor);
                lvl.Level_Filenames = br.ReadStringArray(num_Levels, xor);
                lvl.Cutscene_Filenames = br.ReadStringArray(num_Cutscenes, xor);

                #endregion

                #region Script

#if false
    // This code is partially from Script.cpp of OpenRaider project
                var v = new List<List<ushort>>();
                var n = lvl.Num_Levels + 1;
                var offset = br.ReadUInt16Array(n);
                var numBytes = br.ReadUInt16();
                var list = new ushort[(numBytes + 6) / 2];
                for (ushort i = 0; i < numBytes / 2; i++)
                {
                    list[i] = br.ReadUInt16();
                }
                lvl.DemoLevelIDs = br.ReadUInt16Array(lvl.Num_Demos);
                br.BaseStream.Position -= lvl.Num_Demos * 2;
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
                    while (readingOp || (list[end] != (ushort) ScriptOpCode.END))
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
#else



                var n = num_Levels + 1; // levels + frontend
                var offset = br.ReadUInt16Array(n + 1); // + end
                //var numBytes = br.ReadUInt16();
                var st = br.BaseStream.Position;
                lvl.Script = new ushort[n][];
                for (var i = 0; i < n; i++)
                {
                    br.BaseStream.Position = st + offset[i];
                    lvl.Script[i] = br.ReadUInt16Array(offset[i + 1] / 2 - offset[i] / 2);
                }

                br.BaseStream.Position = st + offset[n];

                lvl.DemoLevelIDs = br.ReadUInt16Array(num_Demos);
#endif

                #endregion

                if (psx)
                {
                    lvl.PSXFMVInfo = br.ReadArray(num_FMVs, () => Loader.PSXFMVInfo.Read(br));
                }

                var numGameStrings = br.ReadUInt16();
                lvl.Game_Strings = br.ReadStringArray(numGameStrings, lvl.Cypher_Code);
                if (psx)
                    lvl.PSX_Strings = br.ReadStringArray(isTR2beta ? 79 : 80, lvl.Cypher_Code);
                else
                    lvl.PC_Strings = br.ReadStringArray(41, lvl.Cypher_Code);
                lvl.Puzzles = br.ReadMultiArray(() => br.ReadStringArray(num_Levels, lvl.Cypher_Code), 4);
                if (psx && isTR2beta)
                {
                    lvl.Secrets = br.ReadMultiArray(() => br.ReadStringArray(num_Levels, lvl.Cypher_Code), 4);
                    lvl.Special = br.ReadMultiArray(() => br.ReadStringArray(num_Levels, lvl.Cypher_Code), 2);
                }
                lvl.Pickups = br.ReadMultiArray(() => br.ReadStringArray(num_Levels, lvl.Cypher_Code), 2);
                lvl.Keys = br.ReadMultiArray(() => br.ReadStringArray(num_Levels, lvl.Cypher_Code), 4);


                return lvl;
            }
            catch (Exception e)
            {
                throw new ScriptParseException(e, br.BaseStream.Position);
            }
        }

        internal static void add_script_command(ref ushort[][] sc, int lvid, ScriptOpCode comm, ushort op = UInt16.MaxValue)
        {
            if (sc.Length < lvid + 1)
                Array.Resize(ref sc, lvid + 1);
            if (sc[lvid] == null)
                sc[lvid] = new ushort[0];
            Helper.Add(ref sc[lvid], (ushort) comm);
            if (op != ushort.MaxValue)
                Helper.Add(ref sc[lvid], op);
        }

        internal static string file_up(string f)
        {
            return f.Substring(0, f.LastIndexOf('.')) + Path.GetExtension(f).ToUpper();
        }

        public static TOMBPCFile ParseTXT(string filePath, TOMBPCGameVersion version, bool psx = false, Func<string> strings = null, bool stoverride = true)
        {
            var ret = new TOMBPCFile();
            ret.IsPSX = psx;
            ret.Game_Version = version;
            var lines = File.ReadAllLines(filePath);
            var curBlock = 0;
            var curBlockVal = "";
            string cmd, value;
            var tmpstrlist = new List<string>();
            int tmp1 = 0;
            uint tmpu1 = 0;
            uint tmpu2 = 0;
            bool tmpb1 = false;
            var unkc = false;
            var num_Levels = 0;
            var num_FMVs = 0;
            var num_Pictures = 0;
            var num_Titles = 0;
            var num_Cutscenes = 0;
            var num_Demos = 0;
            var itemlist = version == TOMBPCGameVersion.TR3 ? Items3 : Items2;
            var nms = new[] {"PUZZLE", "SECRET", "KEY", "SPECIAL", "PICKUP"};
            var nmsv = new[] {ret.Puzzles, ret.Secrets, ret.Keys, ret.Special, ret.Pickups};
            string curstrings = null;
            var lvlGameFound = false;
            for (var i = 0; i < lines.Length; i++)
            {
                GetCommand(lines[i], out cmd, out value);
                if (cmd == "") continue;

                if (cmd.StartsWith("PSX"))
                {
                    if (!psx) continue;
                    cmd = cmd.Substring(3);
                }
                if (cmd.StartsWith("PC"))
                {
                    if (psx) continue;
                    cmd = cmd.Substring(2);
                }

                switch (curBlock)
                {
                    case 0: // Global
                        switch (cmd)
                        {
                            case "DESCRIPTION":
                                if (value == "") HandleTXTParseError("empty description", i, true);
                                else ret.Description = value;
                                continue;
                            case "GAMESTRINGS":
                                if (strings == null || !stoverride)
                                {
                                    if (!File.Exists(value) && !File.Exists(Path.Combine(Path.GetDirectoryName(filePath), value)))
                                        HandleTXTParseError("Specified game strings file doesn't exist", i);
                                    else curstrings = value;
                                }
                                continue;
                            case "OPTIONS":
                                if (value != "")
                                    HandleTXTParseError("ignored value: " + value, i, true);
                                curBlock = 1;
                                break;
                            case "TITLE":
                                if (value != "")
                                    HandleTXTParseError("ignored value: " + value, i, true);
                                curBlock = 2;
                                break;
                            case "FRONTEND":
                                if (value != "")
                                    HandleTXTParseError("ignored value: " + value, i, true);
                                curBlock = 3;
                                break;
                            case "GYM":
                                if (value == "")
                                    HandleTXTParseError("Gym level name cannot be empty", i);
                                else curBlock = 4;
                                ret.Flags |= TOMBPCFlags.GymEnabled;
                                break;
                            case "LEVEL":
                                if (value == "")
                                    HandleTXTParseError("Level name cannot be empty", i);
                                else curBlock = 5;
                                break;
                            case "DEMOLEVEL":
                                if (version != TOMBPCGameVersion.TR3 || psx)
                                {
                                    if (value == "")
                                        HandleTXTParseError("Demo level name cannot be empty", i);
                                    else curBlock = 6;
                                }
                                else
                                {
                                    while (i < lines.Length)
                                    {
                                        GetCommand(lines[i], out cmd, out value);
                                        i++;
                                        if (cmd == "END")
                                            break;
                                    }
                                }
                                break;
                            default:
                                unkc = true;
                                break;
                        }

                        if (curBlock != 0)
                        {
                            curBlockVal = value;
                        }

                        if (new[] {4, 5, 6}.Contains(curBlock))
                        {
                            lvlGameFound = false;
                            tmp1 = 1;
                            if ( /*curBlock != 4*/ true)
                            {
                                num_Levels++;
                                tmp1 = num_Levels;
                            }
                            if (curBlock == 6)
                            {
                                num_Demos++;
                                Helper.Set(ref ret.DemoLevelIDs, num_Demos - 1, (ushort) (num_Levels - 1));
                            }
                            Helper.Set(ref ret.Level_Names, tmp1 - 1, curBlockVal);
                        }
                        break;
                    case 1: // Options
                        switch (cmd)
                        {
                            case "FIRSTOPTION":
                                ret.FirstOption = (uint) ParseAction(value, "FIRSTOPTION");
                                break;
                            case "NOINPUT_TIME":
                                ret.NoInput_Time = (uint) ParseAction(value, "NOINPUT_TIME", true);
                                break;
                            case "ON_DEMO_INTERRUPT":
                                ret.On_Demo_Interrupt = (uint) ParseAction(value, "ON_DEMO_INTERRUPT");
                                break;
                            case "ON_DEMO_END":
                                ret.On_Demo_End = (uint) ParseAction(value, "ON_DEMO_END");
                                break;
                            case "ONDEATH_DEMO_MODE":
                                ret.OnDeath_Demo_Mode = (uint) ParseAction(value, "ONDEATH_DEMO_MODE");
                                break;
                            case "ONDEATH_INGAME":
                                ret.OnDeath_InGame = (uint) ParseAction(value, "ONDEATH_INGAME");
                                break;
                            case "TITLE_REPLACE":
                                ret.Title_Replace = ParseAction(value, "TITLE_REPLACE");
                                break;
                            case "SINGLELEVEL":
                                ret.SingleLevel = (ushort) ParseAction(value, "SINGLELEVEL", true);
                                break;

                            // flags
                            case "DEMOVERSION":
                                ret.Flags |= TOMBPCFlags.DemoVersion;
                                break;
                            case "TITLE_DISABLED":
                                ret.Flags |= TOMBPCFlags.Title_Disabled;
                                break;
                            case "CHEATMODECHECK_DISABLED":
                                ret.Flags |= TOMBPCFlags.CheatModeCheck_Disabled;
                                break;
                            case "NOINPUT_TIMEOUT":
                                ret.Flags |= TOMBPCFlags.NoInput_Timeout;
                                break;
                            case "LOADSAVE_DISABLED":
                                ret.Flags |= TOMBPCFlags.LoadSave_Disabled;
                                break;
                            case "SCREENSIZING_DISABLED":
                                ret.Flags |= TOMBPCFlags.ScreenSizing_Disabled;
                                break;
                            case "LOCKOUT_OPTIONRING":
                                ret.Flags |= TOMBPCFlags.LockOut_OptionRing;
                                break;
                            case "DOZY_CHEAT_ENABLED":
                                ret.Flags |= TOMBPCFlags.DOZY_Cheat_Enabled;
                                break;
                            case "CYPHER_CODE":
                                ret.Flags |= TOMBPCFlags.Use_Security_Tag;
                                ret.Cypher_Code = (byte) ParseAction(value, "CYPHER_CODE", true, defIfEmpty: 0);
                                break;
                            case "SELECT_ANY_LEVEL":
                                ret.Flags |= TOMBPCFlags.Select_Any_Level;
                                break;
                            case "ENABLE_CHEAT_CODE":
                                ret.Flags |= TOMBPCFlags.Enable_Cheat_Key;
                                break;
                            case "LANGUAGE":
                                if (!Enum.TryParse(value, true, out ret.Language))
                                {
                                    HandleTXTParseError("Invalid value for LANGUAGE: " + value, i);
                                }
                                break;
                            case "SECRET_TRACK":
                                ret.Secret_Track = (byte) ParseAction(value, "SECRET_TRACK", true);
                                break;
                            default:
                                unkc = true;
                                break;
                        }
                        break;
                    case 2: // Title
                        switch (cmd)
                        {
                            case "GAME":
                                if (value == "") HandleTXTParseError("empty title filename", i);
                                else curBlockVal = value;
                                break;
                            case "FILE":
                                tmpstrlist.Add(value);
                                break;
                            case "TRACK":
                                ret.Title_Track = (ushort) ParseAction(value, "TRACK", true, i);
                                break;
                            case "END":
                                ret.Title_Filenames = new string[tmpstrlist.Count + 1];
                                ret.Title_Filenames[0] = file_up(curBlockVal);
                                tmpstrlist.CopyTo(ret.Title_Filenames, 1);
                                num_Titles = (ushort) ret.Title_Filenames.Length;
                                break;
                            default:
                                unkc = true;
                                break;
                        }
                        break;
                    case 3: // Frontend
                        switch (cmd)
                        {
                            case "FMV_START":
                                tmpu1 = (uint) ParseAction(value, "FMV_START", true, i);
                                break;
                            case "FMV_END":
                                tmpu2 = (uint) ParseAction(value, "FMV_END", true, i);
                                break;
                            case "FMV":
                                num_FMVs++;
                                if (psx)
                                {
                                    Array.Resize(ref ret.PSXFMVInfo, num_FMVs);
                                    ret.PSXFMVInfo[num_FMVs - 1] = new PSXFMVInfo(tmpu1, tmpu2);
                                    tmpu1 = tmpu2 = 0;
                                }
                                Helper.Add(ref ret.FMV_Filenames, value.ToUpper());
                                add_script_command(ref ret.Script, 0, ScriptOpCode.FMV, (ushort) (num_FMVs - 1));
                                break;
                            case "END":
                                add_script_command(ref ret.Script, 0, ScriptOpCode.END);
                                break;
                            default:
                                unkc = true;
                                break;
                        }
                        break;
                    case 4: // Gym
                    case 5: // Level
                    case 6: // Demo level
                        if (curBlock != 6 || (curBlock == 6 && (version != TOMBPCGameVersion.TR3 || psx)))
                        {
                            var ab = nms.FirstOrDefault(x => cmd.StartsWith(x));
                            if (ab != null && !cmd.Contains("SECRETS"))
                            {
                                var op = ParseAction(cmd.Substring(ab.Length), ab, true, i);
                                if (op > 0 && op <= (Array.IndexOf(nms, cmd) > 2 ? 2 : 4))
                                {
                                    Helper.Set(ref nmsv[Array.IndexOf(nms, ab)][op - 1], num_Levels - 1, value, ab[0].ToString() + op);
                                }
                            }
                            else
                            {
                                switch (cmd)
                                {
                                    case "FMV_START":
                                        tmpu1 = (uint) ParseAction(value, "FMV_START", true, i);
                                        break;
                                    case "FMV_END":
                                        tmpu2 = (uint) ParseAction(value, "FMV_END", true, i);
                                        break;
                                    case "FMV":
                                        num_FMVs++;
                                        if (psx)
                                        {
                                            Array.Resize(ref ret.PSXFMVInfo, num_FMVs);
                                            ret.PSXFMVInfo[num_FMVs - 1] = new PSXFMVInfo(tmpu1, tmpu2);
                                            tmpu1 = tmpu2 = 0;
                                        }
                                        Helper.Add(ref ret.FMV_Filenames, value.ToUpper());
                                        add_script_command(ref ret.Script, tmp1, ScriptOpCode.FMV, (ushort) (num_FMVs - 1));
                                        break;
                                    case "GAME":
                                        lvlGameFound = true;
                                        Helper.Set(ref ret.Level_Filenames, tmp1 - 1, file_up(value));
                                        add_script_command(ref ret.Script, tmp1, ScriptOpCode.GAME, (ushort) (num_Levels - 1));
                                        break;
                                    case "CUT":
                                        Helper.Set(ref ret.Cutscene_Filenames, num_Cutscenes++, file_up(value));
                                        add_script_command(ref ret.Script, tmp1, ScriptOpCode.CUT, (ushort) (num_Cutscenes - 1));
                                        break;
                                    case "COMPLETE":
                                        add_script_command(ref ret.Script, tmp1, ScriptOpCode.COMPLETE);
                                        break;
                                    case "DEMO":
                                        Helper.Set(ref ret.Level_Filenames, num_Levels - 1, value);
                                        add_script_command(ref ret.Script, tmp1, ScriptOpCode.DEMO, (ushort) (num_Levels - 1));
                                        break;
                                    /*case "END":
                                    goto lvl_end;*/
                                    case "TRACK":
                                        add_script_command(ref ret.Script, tmp1, ScriptOpCode.TRACK, (ushort) ParseAction(value, "TRACK", true, i));
                                        break;
                                    case "SUNSET":
                                        add_script_command(ref ret.Script, tmp1, ScriptOpCode.SUNSET);
                                        break;
                                    case "LOAD_PIC":
                                        if (psx || version == TOMBPCGameVersion.TR3)
                                        {
                                            Helper.Set(ref ret.Picture_Filenames, num_Pictures++, file_up(value));
                                            add_script_command(ref ret.Script, tmp1, ScriptOpCode.LOAD_PIC, (ushort) (num_Pictures - 1));
                                        }
                                        break;
                                    case "DEADLY_WATER":
                                        add_script_command(ref ret.Script, tmp1, ScriptOpCode.DEADLY_WATER);
                                        break;
                                    case "REMOVE_WEAPONS":
                                        add_script_command(ref ret.Script, tmp1, ScriptOpCode.REMOVE_WEAPONS);
                                        break;
                                    case "GAMECOMPLETE":
                                        add_script_command(ref ret.Script, tmp1, ScriptOpCode.GAMECOMPLETE);
                                        break;
                                    case "CUTANGLE":
                                        add_script_command(ref ret.Script, tmp1, ScriptOpCode.CUTANGLE, (ushort) ParseAction(value, "CUTANGLE", true, i));
                                        break;
                                    case "NOFLOOR":
                                        add_script_command(ref ret.Script, tmp1, ScriptOpCode.NOFLOOR, (ushort) ParseAction(value, "NOFLOOR", true, i));
                                        break;
                                    case "BONUS":
                                        if (itemlist.ContainsKey(value))
                                        {
                                            add_script_command(ref ret.Script, tmp1, ScriptOpCode.STARTINV, itemlist[value]);
                                        }
                                        break;
                                    case "STARTINV":
                                        if (itemlist.ContainsKey(value))
                                        {
                                            add_script_command(ref ret.Script, tmp1, ScriptOpCode.STARTINV, (ushort) (itemlist[value] + 1000));
                                        }
                                        break;
                                    case "STARTANIM":
                                        add_script_command(ref ret.Script, tmp1, ScriptOpCode.STARTANIM, (ushort) ParseAction(value, "STARTANIM", true, i));
                                        break;
                                    case "SECRETS":
                                        add_script_command(ref ret.Script, tmp1, ScriptOpCode.SECRETS, (ushort) ParseAction(value, "SECRETS", true, i));
                                        break;
                                    case "KILLTOCOMPLETE":
                                        add_script_command(ref ret.Script, tmp1, ScriptOpCode.KILLTOCOMPLETE);
                                        break;
                                    case "REMOVE_AMMO":
                                        add_script_command(ref ret.Script, tmp1, ScriptOpCode.REMOVE_AMMO);
                                        break;
                                    default:
                                        unkc = true;
                                        break;
                                }
                                /*goto nop;
                            lvl_end:
                            
                            nop:*/

                            }
                        }
                        break;
                }

                if (curBlock != 0 && cmd == "END")
                {
                    switch (curBlock)
                    {
                        case 2: // Title

                            break;
                        case 3: // Frontend
                            /*ret.FMV_Filenames = ret.FMV_Filenames.AddArray(tmpstrlist.ToArray());
                            ret.Num_FMVs = (ushort) ret.FMV_Filenames.Length;*/
                            break;

                        case 4:
                            if (!lvlGameFound)
                            {
                                Helper.Set(ref ret.Level_Filenames, tmp1 - 1, "data\\assault." + (psx ? "psx" : "tr2"));
                                //add_script_command(ref ret.Script, tmp1, ScriptOpCode.GAME, (ushort)(num_Levels - 1));
                            }
                            goto case 5;
                        case 5:
                        case 6:

                            add_script_command(ref ret.Script, tmp1, ScriptOpCode.END);
                            break;
                    }

                    tmpb1 = false;
                    tmp1 = 0;
                    tmpu1 = tmpu2 = 0;
                    curBlock = 0;
                    curBlockVal = "";
                    tmpstrlist.Clear();
                    unkc = false;
                    continue;
                }

                if (unkc)
                {
                    HandleTXTParseError("Unknown command: " + cmd, i);
                    unkc = false;
                }
            }

            for (var i = 0; i < nmsv.Length; i++)
            {
                var cur = nmsv[i];
                for (var j = 0; j < cur.Length; j++)
                {
                    cur[j] = cur[j].Resize(ret.Level_Names.Length, nms[i][0].ToString() + (j + 1));
                }
            }

            if (curstrings == null)
            {
                if (strings != null) curstrings = strings();
                if (curstrings == null)
                    curstrings = Path.Combine(Path.GetDirectoryName(filePath), "strings.txt");
            }

            if (!File.Exists(curstrings))
                curstrings = Path.Combine(Path.GetDirectoryName(filePath), curstrings);

            if (File.Exists(curstrings))
            {
                ret.stringsFilename = curstrings;
                // Game strings
                var gsl = File.ReadAllLines(curstrings);
                var gsb = 0;
                var gs1 = new List<string>();
                var pcs = new List<string>();
                var pss = new List<string>();
                foreach (var l in gsl)
                {
                    var l_ = l.RemoveComments().Trim();
                    if (string.IsNullOrWhiteSpace(l_)) continue;

                    if (l_.Contains(':'))
                    {
                        var m = l_.SplitAtFirst(':')[0].Trim().ToUpper();
                        if (m == "GAME_STRINGS") gsb = 1;
                        else if (m == "END") gsb = 0;
                        else if (m == "PC_STRINGS") gsb = 2;
                        else if (m == "PSX_STRINGS") gsb = 3;
                    }
                    else if (gsb != 0)
                    {
                        new[] {gs1, pcs, pss}[gsb - 1].Add(l_.ConvertFromTR2Accent());
                    }
                }
                ret.Game_Strings = gs1.ToArray();
                ret.PC_Strings = pcs.ToArray();
                ret.PSX_Strings = pss.ToArray();
            }
            else
            {
                throw new ScriptParseException(new FileNotFoundException("The strings file doesn't exist", curstrings));
            }

            if (ret.Level_Filenames.Length > 0 && string.IsNullOrWhiteSpace(ret.Level_Filenames[0]))
            {
                ret.Level_Filenames[0] = @"data\assault.tr2";
            }

            return ret;
        }

        private static Tuple<string, string> GetCommand(string line)
        {
            var t = line.Trim().SplitAtFirst(':');
            var cmd = removeComment(t[0]).Trim().ToUpper();
            var val = removeComment(t[1]).Trim();
            if (string.IsNullOrWhiteSpace(cmd)) cmd = "";
            if (string.IsNullOrWhiteSpace(val)) val = "";
            return new Tuple<string, string>(cmd, val);
        }

        private static string removeComment(string s)
        {
            var ret = s;
            var id = ret.IndexOf("//");
            if (id != -1)
            {
                ret = ret.Substring(0, id);
            }
            return ret;
        }

        private static void GetCommand(string line, out string cmd, out string val)
        {
            var t = GetCommand(line);
            cmd = t.Item1;
            val = t.Item2;
        }

        private static int ParseAction(string s, string pname, bool onlyNumber = false, int l = -1, int defIfEmpty = -1)
        {
            s = s.ToUpper().Trim();
            if (string.IsNullOrWhiteSpace(s)) return defIfEmpty;
            var result = -1;
            var f = false;

            if (!onlyNumber)
            {
                foreach (var kvp in Actions)
                {
                    if (s.StartsWith(kvp.Key))
                    {
                        var ss = s.Substring(kvp.Key.Length).Trim();
                        if (string.IsNullOrWhiteSpace(ss)) return defIfEmpty;
                        if (int.TryParse(ss, out result))
                        {
                            f = true;
                            break;
                        }
                        HandleTXTParseError("Invalid value for " + pname + ": " + s, l);
                    }
                }
            }

            if (!f)
            {
                if (!int.TryParse(s, out result))
                {
                    HandleTXTParseError("Invalid value for " + pname + ": " + s, l);
                }
            }

            return result;
        }

        private const bool ON_ERROR_RESUME_NEXT = true; // hehe

        private static void HandleTXTParseError(string message, int line = -1, bool warn = false)
        {
            var m = "Parse " + (warn ? "warning" : "error");
            if (line != -1) m += " at line " + line;
            m += ": " + message;
            if (ON_ERROR_RESUME_NEXT) // #ugliness #vb6
            {
                Console.Error.WriteLine(m);
            }
            else
            {
                throw new FormatException(m);
            }
        }

        public static TOMBPCFile ParseDAT(string filePath, bool psx = false, bool isTR2beta = false)
        {
            using (var fs = new FileStream(filePath, FileMode.Open))
            {
                using (var br = new BinaryReader(fs))
                {
                    return ParseDAT(br, psx, isTR2beta);
                }
            }
        }

        public void WriteDAT(string filename, bool psx = false, bool isTR2beta = false)
        {
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }
            using (var fs = File.OpenWrite(filename))
            {
                using (var bw = new BinaryWriter(fs))
                {
                    WriteDAT(bw, psx, isTR2beta);
                }
            }
        }

        public void WriteDAT(BinaryWriter bw, bool psx = false, bool isTR2beta = false)
        {
            bw.Write((uint) Script_Version);
            bw.WriteStringASCII(Description, 256);
            bw.Write((ushort) 128); // gameflowSize

            bw.Write(FirstOption);
            bw.Write(Title_Replace);
            bw.Write(OnDeath_Demo_Mode);
            bw.Write(OnDeath_InGame);
            bw.Write(NoInput_Time);
            bw.Write(On_Demo_Interrupt);
            bw.Write(On_Demo_End);
            bw.Write(new byte[36]); // Unknown2
            bw.Write((ushort) Level_Names.Length);
            bw.Write((ushort) Picture_Filenames.Length);
            bw.Write((ushort) Title_Filenames.Length);
            bw.Write((ushort) FMV_Filenames.Length);
            bw.Write((ushort) Cutscene_Filenames.Length);
            bw.Write((ushort) DemoLevelIDs.Length);
            bw.Write(Title_Track);
            bw.Write(SingleLevel);
            bw.Write(new byte[32]); // Unknown3
            bw.Write((ushort) Flags);
            bw.Write(new byte[6]); // Unknown4
            bw.Write(Cypher_Code);
            if(Game_Version == TOMBPCGameVersion.TR2 && (Language == TOMBPCLanguage.Italian || Language == TOMBPCLanguage.Spanish))
                bw.Write((byte)TOMBPCLanguage.English);
            else
                bw.Write((byte) Language);
            bw.Write(Secret_Track);
            bw.Write(new byte[4]); // Unknown5

            var xor = Flags.HasFlag(TOMBPCFlags.Use_Security_Tag) ? Cypher_Code : (byte) 0;
            bw.WriteStringArray(Level_Names, xor);
            bw.WriteStringArray(Picture_Filenames, xor);
            bw.WriteStringArray(Title_Filenames, xor);
            bw.WriteStringArray(FMV_Filenames, xor);
            bw.WriteStringArray(Level_Filenames, xor);
            bw.WriteStringArray(Cutscene_Filenames, xor);

            var scrOffsets = new ushort[Level_Names.Length + 1];
            ushort totalOffset = 0;
            for (var i = 0; i <= Level_Names.Length; i++)
            {
                scrOffsets[i] = totalOffset;
                totalOffset += (ushort) (Script[i].Length * 2);
            }
            bw.WriteUInt16Array(scrOffsets);

            bw.Write((ushort) 0);
            var nbPos = bw.BaseStream.Position;
            foreach (var a in Script.Resize(Level_Names.Length + 1))
                bw.WriteUInt16Array(a);
            var tp = bw.BaseStream.Position;
            bw.BaseStream.Position = nbPos - 2;
            bw.Write((ushort) (tp - nbPos));
            bw.BaseStream.Position = tp;
            bw.WriteUInt16Array(DemoLevelIDs);

            if (psx)
            {
                bw.WriteArray(PSXFMVInfo.Resize(FMV_Filenames.Length), x => x.Write(bw));
            }

            bw.Write((ushort) Game_Strings.Length);
            bw.WriteStringArray(Game_Strings, xor);
            if (psx)
                bw.WriteStringArray(PSX_Strings.Resize(isTR2beta ? 79 : 80, "spare"), xor);
            else
                bw.WriteStringArray(PC_Strings.Resize(41, "spare"), xor);
            Array.Resize(ref Puzzles, 4);
            Array.Resize(ref Secrets, 4);
            Array.Resize(ref Special, 2);
            Array.Resize(ref Pickups, 2);
            Array.Resize(ref Keys, 4);
            for (var i = 0; i < 4; i++)
                bw.WriteStringArray(Puzzles[i].Resize(Level_Names.Length, "P" + (i + 1)), xor);
            if (psx && isTR2beta)
            {
                for (var i = 0; i < 4; i++)
                    bw.WriteStringArray(Secrets[i].Resize(Level_Names.Length, "S" + (i + 1)), xor);
                for (var i = 0; i < 2; i++)
                    bw.WriteStringArray(Special[i].Resize(Level_Names.Length, "S" + (i + 1)), xor);
            }
            for (var i = 0; i < 2; i++)
                bw.WriteStringArray(Pickups[i].Resize(Level_Names.Length, "P" + (i + 1)), xor);
            for (var i = 0; i < 4; i++)
                bw.WriteStringArray(Keys[i].Resize(Level_Names.Length, "K" + (i + 1)), xor);
        }

        internal static string value_to_string(int a)
        {
            if (Actions.ContainsValue(a))
            {
                return Actions.First(x => x.Value == a).Key;
            }
            var b = Actions.Where(x => x.Key != "SEQUENCE").FirstOrDefault(x => x.Value <= a) as KeyValuePair<string, int>?;
            if (b != null) return b?.Key + " " + (a - b?.Value);
            return a.ToString();
        }

        public void WriteTXT(string script, string strings)
        {
            if (File.Exists(script))
            {
                File.Delete(script);
            }
            if (File.Exists(strings))
            {
                File.Delete(strings);
            }
            using (var sws = new StreamWriter(script))
            {
                using (var swt = new StreamWriter(strings))
                {
                    WriteTXT(sws, swt);
                }
            }
        }

        public void WriteTXT(StreamWriter sw, StreamWriter swt)
        {
            var platform = IsPSX ? "PSX" : "PC";

            sw.WriteLine("// File generated using FreeRaider.Loader " + Assembly.GetExecutingAssembly().GetName().Version.ToString(2) + " - (c) zdimension 2016");
            sw.WriteLine();
            sw.WriteLine("Description: " + Description);
            sw.WriteLine();
            sw.WriteLine("Options:");
            sw.WriteLine("\tLanguage: " + Language.ToString().ToUpper());
            sw.WriteLine("\tSecret_Track: " + Secret_Track);
            sw.WriteLine("\tCypher_Code: " + Cypher_Code);
            sw.WriteLine("\tFirstOption: " + value_to_string((int) FirstOption));
            sw.WriteLine("\tTitle_Replace: " + value_to_string(Title_Replace));
            sw.WriteLine("\tOnDeath_Demo_Mode: " + value_to_string((int) OnDeath_Demo_Mode));
            sw.WriteLine("\tOnDeath_InGame: " + value_to_string((int) OnDeath_InGame));
            sw.WriteLine("\tNoInput_Time: " + NoInput_Time);
            sw.WriteLine("\tOn_Demo_Interrupt: " + value_to_string((int) On_Demo_Interrupt));
            sw.WriteLine("\tOn_Demo_End: " + value_to_string((int) On_Demo_End));
            sw.WriteLine("\t// Flags");
            foreach (var f in Helper.GetSetFlags(Flags))
                if (f != TOMBPCFlags.GymEnabled)
                    sw.WriteLine("\t" + f + ":");
            sw.WriteLine("End:");
            sw.WriteLine();
            sw.WriteLine();
            sw.WriteLine("Title:");
            sw.WriteLine("\tGame: " + Title_Filenames[0]);
            foreach (var f in Title_Filenames.Skip(1))
                sw.WriteLine("\t" + platform + "file: " + f);
            sw.WriteLine("\tTrack: " + Title_Track);
            sw.WriteLine("End:");
            sw.WriteLine();
            sw.WriteLine();
            sw.WriteLine("Frontend:");

        }
    }

    public enum TOMBPCGameVersion
    {
        TR2 = 2,
        TR3 = 3
    }

    public enum TOMBPCLanguage : byte
    {
        English = 0,
        French = 1,
        German = 2,
        American = 3,
        Japanese = 4,
        // TR3 only
        Italian = 5,
        Spanish = 6
    }

    [Flags]
    public enum TOMBPCFlags : ushort
    {
        /// <summary>
        /// If set, it indicates that the game is a demo distribution.
        /// </summary>
        DemoVersion = 1 << 0,

        /// <summary>
        /// If set, it indicates that the game has no Title Screen.
        /// </summary>
        Title_Disabled = 1 << 1,

        /// <summary>
        /// If set, it indicates that the game does not look for the cheat sequence keystrokes and events. 
        /// This will disable the flare/step/step/rotate/jump sequence.
        /// </summary>
        CheatModeCheck_Disabled = 1 << 2,

        /// <summary>
        /// If set, it indicates that the game waits forever if there is no input. 
        /// Otherwise it returns to the Title Screen, or it enters a demo mode, after a certain period of time with no user input.
        /// </summary>
        NoInput_Timeout = 1 << 3,

        /// <summary>
        /// If set, it indicates that the game does not allow save games.
        /// </summary>
        LoadSave_Disabled = 1 << 4,

        /// <summary>
        /// If set, it indicates that the game does not allow screen resizing. 
        /// The F1 / F2 / F3 / F4 keys will not work, they will not change the screen resolution or the screen size. 
        /// F12 still toggles between full screen and windowed.
        /// </summary>
        ScreenSizing_Disabled = 1 << 5,

        /// <summary>
        /// If set, it indicates that the user has no access to the Option Ring while playing the game. 
        /// This means that the user has no access to the Passport, therefore cannot use save games or exit the game.
        /// </summary>
        LockOut_OptionRing = 1 << 6,

        /// <summary>
        /// If set, it indicates that the game has the DOZY cheat enabled, but what does that mean on a TR2 game? 
        /// The known flare/step/step/rotate/jump cheat sequence does not depend on this field. 
        /// Typing "DOZY" yields nothing.
        /// </summary>
        DOZY_Cheat_Enabled = 1 << 7,

        /// <summary>
        /// If set, it indicates that a cypher byte was used to encrypt the strings in the script file, and is stored in the Cypher_Code field.
        /// </summary>
        Use_Security_Tag = 1 << 8,

        /// <summary>
        /// Is Gym available on title screen.
        /// </summary>
        GymEnabled = 1 << 9,

        /// <summary>
        /// If set, it indicates that the names of the levels are all listed in the Passport. 
        /// It will be the player's choice which level is played. 
        /// Otherwise levels are not displayed and the order defined in the script is kept and followed in sequence.
        /// </summary>
        Select_Any_Level = 1 << 10,

        /// <summary>
        /// It apparently has no effect on the PC game.
        /// The known flare/step/step/rotate/jump cheat sequence does not depend on this bit.
        /// </summary>
        Enable_Cheat_Key = 1 << 11
    }

    public enum ScriptOpCode : ushort
    {
        /// <summary>Unused. Compiles but does not show in-game. Maybe PSX.</summary>
        PICTURE,

        /// <summary>Unused. Does not compile. Maybe PSX.</summary>
        PSX_TRACK,

        /// <summary>Unused. Does not compile. Maybe PSX.</summary>
        PSX_FMV,

        /// <summary>Display Full Motion Video.</summary>
        FMV,

        /// <summary>Start a playable level.</summary>
        GAME,

        /// <summary>Display cut scene sequence.</summary>
        CUT,

        /// <summary>Display level-completion statistics panel.</summary>
        COMPLETE,

        /// <summary>Display demo sequence.</summary>
        DEMO,

        /// <summary>Unused. Does not compile. Maybe PSX.</summary>
        PSX_DEMO,

        /// <summary>Closes script sequences, LEVEL, DEMOLEVEL, GYM, etc...</summary>
        END,

        /// <summary>Play Soundtrack (it precedes opcodes of associated levels).</summary>
        TRACK,

        /// <summary>Unknown. Nothing changes in-game. Maybe this is an ancestor of the TR4 LensFlare command, not actually implemented under TR2.</summary>
        SUNSET,

        /// <summary>Unused. Does not compile. Will be used under TR3.</summary>
        LOAD_PIC,

        /// <summary>Unknown. Nothing changes in-game. Maybe this is an ancestor of the TR3 Death_By_Drowning effect, not actually implemented under TR2.</summary>
        DEADLY_WATER,

        /// <summary>Lara starts the level with no weapons.</summary>
        REMOVE_WEAPONS,

        /// <summary>End of game, shows the final statistics and starts the credits sequence with music ID = 52.</summary>
        GAMECOMPLETE,

        /// <summary>Matches the North-South orientation of the Room Editor and the North-South orientation of the 3D animated characters from a CAD application.</summary>
        CUTANGLE,

        /// <summary>Death_By_Depth. Lara dies when her feet reach the given depth. If falling, 4 to 5 extra blocks are added to Depth.</summary>
        NOFLOOR,

        /// <summary>Item ID Items given to Lara at level-start or at all-secrets-found.</summary>
        STARTINV,

        /// <summary>Special Lara's animation when the level starts.</summary>
        STARTANIM,

        /// <summary>If zero, the level does not account for secrets.</summary>
        SECRETS,

        /// <summary>Kill all enemies to finish the level.</summary>
        KILLTOCOMPLETE,

        /// <summary>Lara starts the level without ammunition or medi packs.</summary>
        REMOVE_AMMO,
    }

    public enum ScriptItem : ushort
    {
        /// <summary>Standard pistols (2)</summary>
        PISTOLS,

        /// <summary>Shotgun (1)</summary>
        SHOTGUN,

        /// <summary>Automatic Pistols (2)</summary>
        AUTOPISTOLS,

        /// <summary>Uzis (2)</summary>
        UZIS,

        /// <summary>Harpoon gun (1)</summary>
        HARPOON,

        /// <summary>M16 (1)</summary>
        M16,

        /// <summary>Grenade launcher (1)</summary>
        ROCKET,

        /// <summary>Pistol clip (no effect, infinite by default)</summary>
        PISTOLS_AMMO,

        /// <summary>Shotgun-shell box (adds 2 shells)</summary>
        SHOTGUN_AMMO,

        /// <summary>Automatic Pistols clip (adds 2 shells)</summary>
        AUTOPISTOLS_AMMO,

        /// <summary>Uzi clip (adds 2 shells)</summary>
        UZI_AMMO,

        /// <summary>Harpoon bundle (adds 2 harpoons)</summary>
        HARPOON_AMMO,

        /// <summary>M16 clip (adds 2 shells)</summary>
        M16_AMMO,

        /// <summary>Grenade pack (adds 1 grenade)</summary>
        ROCKET_AMMO,

        /// <summary>Flare box (adds 1 flare)</summary>
        FLARES,

        /// <summary>Small medi pack (adds 1 pack)</summary>
        MEDI,

        /// <summary>Big medi pack (adds 1 pack)</summary>
        BIGMEDI,

        /// <summary>Pickup item 1</summary>
        PICKUP1,

        /// <summary>Pickup item 2</summary>
        PICKUP2,

        /// <summary>Puzzle item 1</summary>
        PUZZLE1,

        /// <summary>Puzzle item 2</summary>
        PUZZLE2,

        /// <summary>Puzzle item 3</summary>
        PUZZLE3,

        /// <summary>Puzzle item 4</summary>
        PUZZLE4,

        /// <summary>Key item 1</summary>
        KEY1,

        /// <summary>Key item 2</summary>
        KEY2,

        /// <summary>Key item 3</summary>
        KEY3,

        /// <summary>Key item 4</summary>
        KEY4
    }

    public struct PSXFMVInfo
    {
        /// <summary>
        /// Start frame
        /// </summary>
        public uint Start;

        /// <summary>
        /// End frame
        /// </summary>
        public uint End;

        public PSXFMVInfo(uint start, uint end)
        {
            Start = start;
            End = end;
        }

        public static PSXFMVInfo Read(BinaryReader br)
        {
            return new PSXFMVInfo(br.ReadUInt32(), br.ReadUInt32());
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(Start);
            bw.Write(End);
        }
    }

    /*public static class TOMBPCActions
    {
        public const int LEVEL = 0x00000000;
        public const int SEQUENCE = 0x00000000;
        public const int SAVEDGAME = 0x000000FF;
        public const int CINE = 0x00000200;
        public const int FMV = 0x00000300;
        public const int DEMO = 0x00000400;
        public const int EXIT_TO_TITLE = 0x00000500;
        public const int EXITGAME = 0x00000700;
        public const int NONE = -1;
    }*/
}