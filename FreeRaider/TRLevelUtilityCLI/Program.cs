using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Runtime.InteropServices;
using FreeRaider;
using FreeRaider.Loader;

namespace TRLevelUtilityCLI
{
    class Program
    {
        // Exit codes from sysexits.h

        /// <summary>
        ///     The command was used incorrectly, e.g., with the wrong number of arguments, a bad flag,
        ///     a bad syntax in a parameter, or whatever.
        /// </summary>
        public const int EX_USAGE = 64;

        /// <summary>
        ///     The input data was incorrect in some way. This should only be used for user's data and
        ///     not system files.
        /// </summary>
        public const int EX_DATAERR = 65;

        /// <summary>
        ///     An input file (not a system file) did not exist or was not readable.
        /// </summary>
        public const int EX_NOINPUT = 66;

        /// <summary>
        ///     A (user specified) output file cannot be created.
        /// </summary>
        public const int EX_CANTCREAT = 73;

        unsafe static void Main(string[] args)
        {
#if false
            var b = new[] {"GYM", "LEVEL1", "LEVEL2", "LEVEL3A", "LEVEL3B", "LEVEL4", "LEVEL5", "LEVEL6", "LEVEL7A", "LEVEL7B", "LEVEL8A", "LEVEL8B", "LEVEL8C", "LEVEL10A", "LEVEL10B", "LEVEL10C", "CUT1", "CUT2", "CUT3", "CUT4", "TITLE"};
            var sp = @"F:\Jeux\Tomb Raider Collection\Tomb Raider 1\Test bin cue\Nouveau dossier\DATA";
            using (var fp = File.Create(@"D:\Documents\GitHub\FreeRaider\FreeRaider\FreeRaider.Loader\entity_tr1.db"))
            using (var bw = new BinaryWriter(fp))
            {
                bw.Write(b.Length);
                foreach (var lv in b)
                {
                    var l = Level.FromFile(Path.Combine(sp, lv + ".PHD"), TRGame.TR1);
                    bw.Write(l.Entities.Length);
                    bw.WriteArray(l.Entities, x => x.Write(bw, Engine.TR1));
                    /*fixed (Entity* tmp = l.Entities)
                    {
                        var ptr = (byte*)tmp;
                        var length = l.Entities.Length * Marshal.SizeOf(typeof(Entity));
                        for(var i = 0; i < length; i++)
                            bw.Write(ptr[i]);
                    }*/
                    bw.Write(l.Models.Length);
                    bw.WriteArray(l.Models, x => x.Write(bw, Engine.TR1));
                    /*fixed (Model* tmp = l.Models)
                    {
                        var ptr = (byte*)tmp;
                        var length = l.Models.Length * Marshal.SizeOf(typeof(Model));
                        for (var i = 0; i < length; i++)
                            bw.Write(ptr[i]);
                    }*/

                    //Console.WriteLine("new [] { " + l.Entities.Length + ", " + string.Join(", ", l.Entities.Select(x => x.Flags)) + " },");
                }

            }

            return;
#endif
            if (args.Length == 0)
            {
                Console.WriteLine(
$@"TRLevelUtilityCLI {Assembly.GetExecutingAssembly().GetName().Version.ToString(2)} - (c) zdimension 2016
Many thanks to the TombRaiderForums guys, in particular b122251 who helped me a
lot, and to the the TRosettaStone authors.

Usage: tlu <file name> <command>
Commands:
info                              Shows information about a file.
info <2|3> <pc|psx> [strings]     Shows information about an uncompiled TXT
                                  script file.
     If the file is an uncompiled TXT script file, you must specify the game 
     version (either 2 or 3 for TR2 or TR3 respectively).
     If you want to specify a strings file other than the one specified in 
     the TXT script file, append its file name to the parameters.
info <version>                    Shows information about a savegame file.
     The input file name must contain the word ""savegame"" (case
     insensitive) to be detected as a savegame file.
     Version can be: TR1 TR1UB TR2 TR2GM TR3 TR3LA TR4 TR5
compile <2|3> <pc|psx> [strings]  Compiles a TXT script file into a 
                                  TOMBPC.DAT or TOMBPSX.DAT file.
     Same parameters as the command above.                                                                                     
convert <format[d]> <file name>   Converts a file (see list below).
     File extension: phd/tub tr2 tr2 tr4 trc    prj     pak bin png
     Output format:    TR1   TR2 TR3 TR4 TR5 PRJ (TRLE)   Picture
     Add 'd' at the end of the format if for the demo version,
     for TR1 Unfinished Business or for levels containing only palette and 
     textiles like TR3's 'VICT.TR2' 
dumptex [bits]...                 Dumps all the textures of a level to PNG 
                                  files.
Example: 
     dumptex       dumps 8-bit, 16-bit and 32-bit textures
     dumptex 8 32  dumps 8-bit and 32-bit textures
     A folder called '<file name> - Textures' containing three folders 
     ('8-bit', '16-bit' and '32-bit') will be created.
pak <output filename>             Compresses the file in a .PAK file.     
unpak <output filename>           Uncompresses a .PAK file.

Supported files:
- Level files
    TR1 up to TR5, including betas and demos
    (only PC)
- Script files
    TR2-3 TOMBPC.DAT / TOMBPSX.DAT
    TR4-5 SCRIPT.DAT - <LANGUAGE>.DAT
- TRLE Project files
    *.PRJ
- Compressed package files (TR4-5)
    *.PAK
- Pictures (TR4-5)
    TR4 *.PAK
    TR5 *.BIN");
                Environment.Exit(EX_USAGE);
            }

            if (args.Length > 1)
            {
                var line = args.Skip(1).ToArray();
                line[0] = line[0].ToLower();

                var inf = args[0].Trim();

                if (!File.Exists(inf))
                {
                    Die("The input file '" + inf + "' doesn't exist.", EX_NOINPUT);
                }
                var done = false;
                var ext = Path.GetExtension(inf).ToUpper();
                switch (line[0])
                {
                    case "info":
                        if (Path.GetFileName(inf).ToUpper().Contains("SAVEGAME"))
                        {
                            if (line.Length >= 2)
                            {
                                var ver = line[1].ToUpper().Trim();

                                switch (ver)
                                {
                                    case "TR1":
                                        var save = TR1SavegameFile.Read(inf);
                                        foreach (var field in save.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public))
                                        {
                                            Console.WriteLine(field.Name + " = " + field.GetValue(save));
                                        }
                                        foreach (var prop in save.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
                                        {
                                            Console.WriteLine(prop.Name + " => " + prop.GetValue(save));
                                        }
                                        break;
                                    default:
                                        Die("Unknown format: " + ver, EX_USAGE);
                                        return;
                                }
                            }
                            else Die("Not enough parameters", EX_USAGE);
                            done = true;
                        }
                        if (ext == ".DAT" || ext == ".TXT")
                        {
                            var infn = Path.GetFileName(inf).ToUpper().Trim();
                            done = true;
                            if (infn == "TOMBPC.DAT" || infn == "TOMBPSX.DAT" || ext == ".TXT")
                            {
                                TOMBPCFile sc = null;
                                if (ext == ".TXT")
                                {
                                    if (line.Length >= 3)
                                    {
                                        int ver;
                                        bool psx = line[2].ToUpper() == "PSX";
                                        string strs = null;
                                        if (line.Length > 3)
                                            strs = line[3];

                                        if (!int.TryParse(line[1], out ver))
                                        {
                                            Die("Wrong version number: " + line[1], EX_USAGE);
                                        }

                                        sc = OrDie(() => TOMBPCFile.ParseTXT(inf, (TOMBPCGameVersion) ver, psx, strs == null ? (Func<string>) null : () => strs), e => $"Error while loading uncompiled script '{inf}': {e.Message}", EX_DATAERR);
                                    }
                                    else Die("Not enough parameters", EX_USAGE);
                                }
                                else sc = OrDie(() => TOMBPCFile.ParseDAT(inf, infn == "TOMBPSX.DAT"), e => $"Error while loading script '{inf}': {e.Message}", EX_DATAERR);
                                Console.WriteLine($@"
Version: {sc.Script_Version} (always 3)
Game version: {sc.Game_Version:D} ({sc.Game_Version})
Type: {(sc.IsPSX ? "PSX" : "PC")}
Description: {sc.Description}
FirstOption: {sc.FirstOption}
TitleReplace: {sc.Title_Replace}
OnDeathDemoMode: {sc.OnDeath_Demo_Mode}
OnDeathInGame: {sc.OnDeath_InGame}
DemoTime: {sc.NoInput_Time} game ticks ({sc.NoInput_Time / 30} seconds)
OnDemoInterrupt: {sc.On_Demo_Interrupt}
OnDemoEnd: {sc.On_Demo_End}
Levels: {sc.Level_Names.Length}
Demo levels: {sc.DemoLevelIDs.Length}
Title sound ID: {sc.Title_Track}
SingleLevel: {sc.SingleLevel}
Flags: 0x{sc.Flags:X} ({sc.Flags})
XOR key: 0x{sc.Cypher_Code:X} ({sc.Cypher_Code})
Language ID: {sc.Language:D} ({sc.Language})
Secret sound ID: {sc.Secret_Track}
Level strings:
{string.Join("\n", sc.Level_Names)}
Chapter screens: {sc.Picture_Filenames.Length}
{string.Join("\n", sc.Picture_Filenames)}
Title strings: {sc.Title_Filenames.Length}
{string.Join("\n", sc.Title_Filenames)}
FMV strings: {sc.FMV_Filenames.Length}
{string.Join("\n", sc.FMV_Filenames)}
Level path strings:
{string.Join("\n", sc.Level_Filenames)}
Cutscene path strings: {sc.Cutscene_Filenames.Length}
{string.Join("\n", sc.Cutscene_Filenames)}
Demo level IDs: {(sc.DemoLevelIDs == null ? "Not present" : string.Join("; ", sc.DemoLevelIDs))}
Game strings: {sc.Game_Strings.Length}
{string.Join("\n", sc.Game_Strings)}
PC strings: {sc.PC_Strings.Length}
{string.Join("\n", sc.PC_Strings)}
PSX strings: {sc.PSX_Strings.Length}
{string.Join("\n", sc.PSX_Strings)}
<puzzle and key strings not shown here>
");
                            }
                            else if (infn == "SCRIPT.DAT")
                            {
                                var sc = OrDie(() => TR4ScriptFile.Read(inf), e => $"Error while loading script file '{inf}': {e.Message}", EX_DATAERR);
                                Console.WriteLine($@"
Options: 0x{sc.Options:X} ({sc.Options})
Input timeout: {sc.InputTimeout} ({sc.InputTimeout / 600} seconds)
XOR key: 0x{sc.Security:X} ({sc.Security})
Levels (including title): {sc.NumTotalLevels}
Unique level paths: {sc.NumUniqueLevelPaths}
{string.Join("\n", sc.LevelPaths)}
PSX file extensions: {sc.PSXLevelString} {sc.PSXFMVString} {sc.PSXCutString} {sc.PSXFiller}
PC file extensions: {sc.PCLevelString} {sc.PCFMVString} {sc.PCCutString} {sc.PCFiller}
");
                            }
                            else if (infn.IsAnyOf("ENGLISH.DAT", "FRENCH.DAT", "GERMAN.DAT", "ITALIAN.DAT", "SPANISH.DAT", "JAPAN.DAT",
                                "US.DAT", "DUTCH.DAT"))
                            {
                                var lng = OrDie(() => TR4LanguageFile.Read(inf), e => $"Error while loading language file '{inf}': {e.Message}", EX_DATAERR);
                                Console.WriteLine($@"
Generic strings: {lng.GenericStrings.Length}
{string.Join("\n", lng.GenericStrings)}
PSX strings: {lng.PSXStrings.Length}
{string.Join("\n", lng.PSXStrings)}
PC strings: {lng.PCStrings.Length}
{string.Join("\n", lng.PCStrings)}
");
                            }
                            else
                            {
                                done = false;
                            }
                        }
                        else if (ext == ".PRJ")
                        {
                            var prj = OrDie(() => PRJFile.Read(inf), e => $"Error while loading TRLE project '{inf}': {e.Message}", EX_DATAERR);
                            Console.WriteLine($@"
Header: {string.Concat(prj.Header.Select(x => (char) x))}
Version: {prj.Version}
Room list size: {prj.RoomListSize}
Number of objects: {prj.NumObjects}
Max. number of objects: {prj.MaxObjects}
Number of lights: {prj.NumLights}
Number of triggers: {prj.NumTriggers}
Texture file: {prj.TextureFile}
Number of texture info: {prj.TextInfo.Length}
Object file: {prj.ObjectFile}
Number of object data: {prj.ObjectData.Length}
Number of animated textures: {prj.NumAnimTextures}
");
                            done = true;
                        }
                        else if (ext == ".PAK")
                        {
                            var pakLength = OrDie(() => PAKFile.GetLength(inf), e => $"Error while loading PAK file '{inf}': {e.Message}", EX_DATAERR);
                            Console.WriteLine($@"
Uncompressed data size: {pakLength} (0x{pakLength:X4})
");
                            done = true;
                        }
                        break;
                    case "compile":
                        if (ext == ".TXT")
                        {
                            if (line.Length > 2)
                            {
                                int ver;
                                bool psx = line[2].ToUpper() == "PSX";
                                string strs = null;
                                if (line.Length > 3)
                                    strs = line[3];

                                if (!int.TryParse(line[1], out ver))
                                {
                                    Die("Wrong version number: " + line[1], EX_USAGE);
                                }
                                done = true;
                                var sc = TOMBPCFile.ParseTXT(inf, (TOMBPCGameVersion) ver, psx, strs == null ? (Func<string>) null : () => strs);
                                sc.WriteDAT(Path.Combine(Path.GetDirectoryName(inf), "TOMB" + line[2].ToUpper() + ".DAT"));
                            }
                            else Die("Not enough parameters", EX_USAGE);
                        }
                        break;
                    case "pak":
                    {
                        if (line.Length >= 2)
                        {
                            var input = OrDie(() => File.OpenRead(inf), e => $"Error while loading input file '{inf}': {e.Message}", EX_DATAERR);
                            var output = OrDie(() => File.Create(line[1]), e => $"Error while creating PAK file '{inf}': {e.Message}", EX_CANTCREAT);
                            PAKFile.Write(output, input);
                            done = true;
                        }
                       else Die("Not enough parameters", EX_USAGE);
                        }
                        break;
                    case "unpak":
                    {
                            if (line.Length >= 2)
                            {
                                var input = OrDie(() => File.OpenRead(inf), e => $"Error while loading PAK file '{inf}': {e.Message}", EX_DATAERR);
                                var output = OrDie(() => File.Create(line[1]), e => $"Error while creating output file '{inf}': {e.Message}", EX_CANTCREAT);
                                PAKFile.Read(input, output);
                                input.Close();
                                output.Close();
                                done = true;
                            }
                            else Die("Not enough parameters", EX_USAGE);
                        }
                        break;
                    case "convert":
                        if (line.Length != 3)
                        {
                            Die($"Invalid parameter count for command 'convert': {line.Length}, expected 3", EX_USAGE);
                            return;
                        }
                        var fmts = line[1].ToUpper().Trim();
                        var outf = line[2].Trim();
                        if (line.Length >= 2)
                        {
                            if (new[] {".BIN", ".PAK", ".PNG"}.Contains(ext))
                            {
                                if(ext == Path.GetExtension(line[1]).ToUpper())
                                    Die("The file is already in " + ext + " format.", EX_USAGE);

                            }
                        }
                        else Die("Not enough parameters", EX_USAGE);
                        break;
                }

                // level-only commands
                if (!done)
                {
                    var lvl = OrDie(() => Level.FromFile(inf), e => $"Error while loading level '{inf}': {e.Message}", EX_DATAERR);

                    switch (line[0])
                    {
                        case "info":
                            Console.WriteLine($@"
Game version: {lvl.Format.Game}
Engine version: {lvl.Format.Engine}
8-bit palette (TR1-2-3): {(Equals(lvl.Palette, default(Palette)) ? "Not present" : "Present")}
16-bit palette (TR2-3): {(Equals(lvl.Palette16, default(Palette)) ? "Not present" : "Present")}
8-bit (palettized) textures (TR1-2-3): {lvl.Texture8?.Length.ToString() ?? "Not present"}
16-bit (ARGB) textures (TR2-3-4-5): {lvl.Texture16?.Length.ToString() ?? "Not present"}
32-bit textures: {lvl.Textures?.Length.ToString() ?? "Not present"}
Rooms: {lvl.Rooms.Length}
Floor data: {lvl.FloorData.Length}
Meshes: {lvl.Meshes.Length}
Animations: {lvl.Animations.Length}
State changes: {lvl.StateChanges.Length}
Animation dispatches: {lvl.AnimDispatches.Length}
Animation commands: {lvl.AnimCommands.Length}
Mesh trees: {lvl.MeshTreeData.Length}
Frames: {lvl.FrameData.Length}
Moveables: {lvl.Models.Length}
Static meshes: {lvl.StaticMeshes.Length}
Sprite textures: {lvl.SpriteTextures.Length}
Sprite sequences: {lvl.SpriteSequences.Length}
Cameras: {lvl.Cameras.Length}
Flyby cameras (TR4-5): {lvl.FlybyCameras?.Length.ToString() ?? "Not present"}
Sound sources: {lvl.SoundSources.Length}
Boxes: {lvl.Boxes.Length}
Overlaps: {lvl.Overlaps.Length}
Zones: {lvl.Zones.Length}
Animated textures: {lvl.AnimatedTextures.Length}
Object textures: {lvl.ObjectTextures.Length}
Items: {lvl.Entities.Length}
AI objects (TR4-5): {lvl.AIObjects?.Length.ToString() ?? "Not present"}
Cinematic frames (TR1-2-3): {lvl.CinematicFrames?.Length.ToString() ?? "Not present"}
Demo data: {lvl.DemoData.Length}
Sound details: {lvl.SoundDetails.Length}
Sample indices: {lvl.SampleIndices.Length}");
                            if(lvl.Format.Game == TRGame.TR5) Console.WriteLine($@"Lara type: {lvl.LaraType:D} ({lvl.LaraType})
Weather type: {lvl.WeatherType:D} ({lvl.WeatherType})
");
                            break;
                        case "convert":
                            if (line.Length != 3)
                            {
                                Die($"Invalid parameter count for command 'convert': {line.Length}, expected 3", EX_USAGE);
                                return;
                            }
                            var fmts = line[1].ToUpper().Trim();
                            var outf = line[2].Trim();
                            var toTrle = fmts == "PRJ";
                            var fmt = TRGame.Unknown;

                            if (!toTrle)
                            {
                                if (fmts.EndsWith("d"))
                                {
                                    fmts = fmts.Substring(0, fmts.Length - 1).Trim();
                                    lvl.WriteFormat = lvl.WriteFormat.SetDemo(true);
                                }

                                var fmti = Array.IndexOf(new[] {"TR1", "TR2", "TR3", "TR4", "TR5"}, fmts);
                                if (fmti == -1)
                                {
                                    Die("Unknown format: " + fmts, EX_USAGE);
                                }
                                fmt =
                                    new[]
                                    {TRGame.TR1, TRGame.TR2, TRGame.TR3, TRGame.TR4, TRGame.TR5}
                                        [fmti];
                            }

                            OutputFileExists(outf);

                            Console.WriteLine("Starting level conversion...");

                            OrDie(() =>
                            {
                                using (var fs = File.Open(outf, FileMode.Create))
                                {
                                    if (toTrle)
                                    {
                                        PRJFile.LevelToProject(lvl).Write(fs);
                                    }
                                    else
                                    {
                                        lvl.Write(fs, fmt);
                                    }
                                }
                            }, e => "Error while converting level: " + e.Message, EX_CANTCREAT);

                            Console.WriteLine("Level conversion finished");
                            break;
                        case "dumptex":

                            var dump = line.Length > 1 ? new[] {line.Contains("8"), line.Contains("16"), line.Contains("32")} : new[] { true, true, true };
                            var outd = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                                Path.GetFileName(inf) + " - Textures");
                            OutputDirExists(outd);
                            unsafe
                            {
                                Directory.CreateDirectory(outd);
                                Console.WriteLine("Generating missing textures if needed...");
                                OrDie(() => lvl.GenTexAndPalettesIfEmpty(dump[0], dump[1], dump[2]), e => $"Error while dumping textures: {e.Message}", EX_DATAERR);
                                if (dump[0])
                                {
                                    var _8bit = Path.Combine(outd, "8-bit");
                                    Directory.CreateDirectory(_8bit);                                  
                                    if (lvl.Texture8 == null)
                                    {
                                        Console.WriteLine("Level doesn't contain 8-bit textures.");
                                    }
                                    else
                                    {
                                        var dg = lvl.Texture8.Length.ToString().Length;
                                        Console.Write("Dumping 8-bit textures... ");
                                        var cl = Console.CursorLeft;
                                        Console.Write(0.ToString().PadLeft(dg, ' ') + "/" + lvl.Texture8.Length);
                                        Console.CursorLeft = cl;
                                        for (var bi = 0; bi < lvl.Texture8.Length; bi++)
                                        {
                                            var bt = lvl.Texture8[bi];
                                            var bmp = new Bitmap(256, 256, PixelFormat.Format32bppArgb);
                                            var data = bmp.LockBits(new Rectangle(0, 0, 256, 256), ImageLockMode.WriteOnly, bmp.PixelFormat);
                                            var scan0 = (byte*) data.Scan0;
                                            fixed(ByteColor* bptr = lvl.Palette.Colour)
                                            fixed (byte* ptr = bt.Pixels)
                                            {
                                                ByteColor* curb;
                                                for (int p = 0, p2 = 0; p < 65536; p++, p2 += 4)
                                                {
                                                    curb = bptr + ptr[p];
                                                    scan0[p2 + 0] = (byte)(curb->B << 2);
                                                    scan0[p2 + 1] = (byte)(curb->G << 2);
                                                    scan0[p2 + 2] = (byte)(curb->R << 2);
                                                    scan0[p2 + 3] = curb->A;
                                                }
                                            }
                                            bmp.UnlockBits(data);
                                            bmp.Save(Path.Combine(_8bit, "tex_" + bi.ToString().PadLeft(dg, '0') + ".png"));
                                            Console.CursorLeft = cl;
                                            Console.Write((bi + 1).ToString().PadLeft(dg, ' '));
                                        }

                                        Console.CursorLeft += dg + 1;
                                        Console.WriteLine();
                                    }
                                }
                                if (dump[1])
                                {
                                    var _16bit = Path.Combine(outd, "16-bit");
                                    Directory.CreateDirectory(_16bit);
                                    if (lvl.Texture16 == null)
                                    {
                                        Console.WriteLine("Level doesn't contain 16-bit textures.");
                                    }
                                    else
                                    {
                                        var dg = lvl.Texture16.Length.ToString().Length;
                                        Console.Write("Dumping 16-bit textures... ");
                                        var cl = Console.CursorLeft;
                                        Console.Write(0.ToString().PadLeft(dg, ' ') + "/" + lvl.Texture16.Length);

                                        for (var bi = 0; bi < lvl.Texture16.Length; bi++)
                                        {
                                            var bt = lvl.Texture16[bi];
                                            var bmp = new Bitmap(256, 256, PixelFormat.Format32bppArgb);
                                            var data = bmp.LockBits(new Rectangle(0, 0, 256, 256), ImageLockMode.WriteOnly, bmp.PixelFormat);
                                            var scan0 = (byte*)data.Scan0;
                                            fixed (ByteColor* bptr = lvl.Palette.Colour)
                                            fixed (ushort* ptr = bt.Pixels)
                                            {
                                                ByteColor curb;
                                                for (int p = 0, p2 = 0; p < 65536; p++, p2 += 4)
                                                {
                                                    curb = new ByteColor(ptr[p]);
                                                    scan0[p2 + 0] = curb.B;
                                                    scan0[p2 + 1] = curb.G;
                                                    scan0[p2 + 2] = curb.R;
                                                    scan0[p2 + 3] = curb.A;
                                                }
                                            }
                                            bmp.UnlockBits(data);
                                            bmp.Save(Path.Combine(_16bit, "tex_" + bi.ToString().PadLeft(dg, '0') + ".png"));
                                            Console.CursorLeft = cl;
                                            Console.Write((bi + 1).ToString().PadLeft(dg, ' '));
                                        }

                                        Console.CursorLeft += dg + 1;
                                        Console.WriteLine();
                                    }
                                }
                                if (dump[2])
                                {
                                    var _32bit = Path.Combine(outd, "32-bit");
                                    Directory.CreateDirectory(_32bit);
                                    if (lvl.Textures == null)
                                    {
                                        Console.WriteLine("Level doesn't contain 32-bit textures.");
                                    }
                                    else
                                    {
                                        var dg = lvl.Texture16.Length.ToString().Length;
                                        Console.Write("Dumping 32-bit textures... ");
                                        var cl = Console.CursorLeft;
                                        Console.Write(0.ToString().PadLeft(dg, ' ') + "/" + lvl.Texture16.Length);

                                        for (var bi = 0; bi < lvl.Textures.Length; bi++)
                                        {
                                            var bt = lvl.Textures[bi];
                                            var bmp = new Bitmap(256, 256, PixelFormat.Format32bppArgb);
                                            var bmpData = bmp.LockBits(new Rectangle(0, 0, 256, 256), ImageLockMode.WriteOnly,
                                                PixelFormat.Format32bppArgb);
                                            var scan0 = (uint*) bmpData.Scan0;
                                            fixed (uint* ptr = bt.Pixels)
                                                for (var i = 0; i < 65536; i++)
                                                {
                                                    scan0[i] = (ptr[i] & 0xff00ff00) |
                                                               ((ptr[i] & 0x00ff0000) >> 16) |
                                                               ((ptr[i] & 0x000000ff) << 16);
                                                }
                                            bmp.UnlockBits(bmpData);
                                            bmp.Save(Path.Combine(_32bit, "tex_" + bi.ToString().PadLeft(dg, '0') + ".png"));
                                            Console.CursorLeft = cl;
                                            Console.Write((bi + 1).ToString().PadLeft(dg, ' '));
                                        }

                                        Console.CursorLeft += dg + 1;
                                        Console.WriteLine();
                                    }
                                }
                            }
                            Console.WriteLine("Dump finished");
                            break;
                        default:
                            Die("Unknown command: " + line[0], EX_USAGE);
                            return;
                    }
                }
            }
            else
            {
                Die("Expected command", EX_USAGE);
            }
#if DEBUG
            Console.ReadLine();
#endif
        }

        public static T OrDie<T>(Func<T> what, Func<Exception, string> message, int exitCode)
        {
            try
            {
                return what();
            }
            catch (Exception e)
            {
                Die(message(e), exitCode);
                throw e;
            }
        }

        public static T OrDie<T>(Func<T> what, string message, int exitCode)
        {
            return OrDie(what, e => message, exitCode);
        }

        public static void OrDie(Action what, Func<Exception, string> message, int exitCode)
        {
            try
            {
                what();
            }
            catch (Exception e)
            {
                Die(message(e), exitCode);
                throw e;
            }
        }

        public static void OrDie(Action what, string message, int exitCode)
        {
            OrDie(what, e => message, exitCode);
        }

        public static void Die(string message, int exitCode)
        {
            Console.Error.WriteLine(message);
            Environment.Exit(exitCode);
        }

        private static void OutputFileExists(string outf)
        {
            if (File.Exists(outf))
            {
                Console.WriteLine("Warning: the output file '{0}' already exists.", outf);
                Console.Write("Do you want to delete it? (y/n) ");
                if (Console.ReadLine().ToLower().Trim() == "y")
                {
                    File.Delete(outf);
                    Console.WriteLine("File deleted.");
                }
                else
                {
                    Die("File not deleted, operation aborted, exiting.", EX_CANTCREAT);
                }
            }
        }

        private static void OutputDirExists(string outd)
        {
            if (Directory.Exists(outd))
            {
                Console.WriteLine("Warning: the output directory '{0}' already exists.",
                    Path.GetFileName(outd));
                Console.Write("Do you want to delete it? (y/n) ");
                if (Console.ReadLine().ToLower().Trim() == "y")
                {
                    Directory.Delete(outd, true);
                    Console.WriteLine("Directory deleted.");
                }
                else
                {
                    Die("Directory not deleted, operation aborted, exiting.", EX_CANTCREAT);
                }
            }
        }
    }
}