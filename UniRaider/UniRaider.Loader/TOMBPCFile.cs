using System;

namespace UniRaider.Loader
{
    public class TOMBPCFile
    {
        /// <summary>
        /// seems to be 3 for TR2
        /// </summary>
        public TOMBPCGameVersion GameVersion { get; set; }
        /// <summary>
        /// null-terminated string describing this game, copyright info, etc.
        /// </summary>
        public string CopyrightInfo { get; set; }
        /// <summary>
        /// Level to go to when that happens (0x500 is exit-to-title) ??? when WHAT happens?
        /// </summary>
        public int FirstOption { get; set; }
        /// <summary>
        /// Level to go to when that happens (-1 is NONE) ??? when WHAT happens?
        /// </summary>
        public int TitleReplace { get; set; }
        /// <summary>
        /// Level to go to when Lara dies during demo mode (0x500 is exit-to-title)
        /// </summary>
        public int OnDeathDemoMode { get; set; }
        /// <summary>
        /// Level to go to when Lara dies during the game (0 is exit-to-title) 
        /// </summary>
        public int OnDeathInGame { get; set; }
        /// <summary>
        /// time in game ticks (1/30th of a second?) to wait before starting a demo 
        /// </summary>
        public int DemoTime { get; set; }
        /// <summary>
        /// Level to go to when demo mode is interrupted (0x500 is exit-to-title) 
        /// </summary>
        public int OnDemoInterrupt { get; set; }
        /// <summary>
        /// Level to go to when the demo ends (0x500 is exit-to-title)
        /// </summary>
        public int OnDemoEnd { get; set; }
        /// <summary>
        /// number of levels in the game (some level files are used more than once for some reason) 
        /// </summary>
        public short NumLevels { get; set; }
        /// <summary>
        /// chapter screens (Present in TR2, first used in TR3)
        /// </summary>
        public short NumChapterScreens { get; set; }
        /// <summary>
        /// only one, TITLE.TR2 
        /// </summary>
        public short NumTitles { get; set; }
        /// <summary>
        /// number of FMV cutscenes (*.RPL) 
        /// </summary>
        public short NumRPLs { get; set; }
        /// <summary>
        /// number of in-game (engine-rendered) cutscenes (CUT*.TR2) 
        /// </summary>
        public short NumCutScenes { get; set; }
        /// <summary>
        /// Number of demo levels 
        /// </summary>
        public short NumDemoLevels { get; set; }
        /// <summary>
        /// ID of title soundtrack
        /// </summary>
        public short TitleSoundID { get; set; }
        /// <summary>
        /// If doing only a single level 
        /// </summary>
        public short SingleLevel { get; set; }
        /// <summary>
        /// Various flags
        /// </summary>
        public TOMBPCFlags Flags { get; set; }
        /// <summary>
        /// For encryption ("cipher code")
        /// </summary>
        public byte XORbyte { get; set; }
        /// <summary>
        /// ID of "found a secret" soundtrack
        /// </summary>
        public short SecretSoundID { get; set; }

        public string[] LevelDisplayNames { get; set; } 
    }

    public enum TOMBPCGameVersion : uint
    {
        TR2_3 = 3
    }

    [Flags]
    public enum TOMBPCFlags : ushort
    {
        /// <summary>
        /// 0 -> normal game<br/> 1 -> demo
        /// </summary>
        DemoVersion = 1,
        /// <summary>
        /// 0 -> normal title screen<br/> 1 -> no title screen
        /// </summary>
        Title_Disabled = 2,
        /// <summary>
        /// 0 -> cheat mode enabled<br/> 1 -> no cheat mode
        /// </summary>
        CheatModeCheck_Disabled = 4,
        /// <summary>
        /// 0 -> enter demo mode if no input timeout<br/> 1 -> wait forever if no input
        /// </summary>
        NoInputTimeout = 8,
        /// <summary>
        /// 0 -> load/save game enabled<br/> 1 -> load/save game disabled
        /// </summary>
        LoadSave_Disabled = 16,
        /// <summary>
        /// 0 -> screen re-sizing allowed<br/> 1 -> no screen re-sizing allowed
        /// </summary>
        ScreenSizing_Disabled = 32,
        /// <summary>
        /// 0 -> normal option ring<br/> 1 -> ???
        /// </summary>
        LockOutOptionRing = 64,
        /// <summary>
        /// ???
        /// </summary>
        DozyCheat_Enabled = 128,
        /// <summary>
        /// 0 -> leave StringData as-is<br/> 1 -> XOR all StringData with XORbyte
        /// </summary> 
        Use_Encryption = 256,
        /// <summary>
        /// 0 -> no level selection<br/> 1 -> allow player to select any level
        /// </summary>
        SelectAnyLevel = 512
    }
}
