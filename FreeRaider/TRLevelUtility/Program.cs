using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using FreeRaider.Loader;

namespace TRLevelUtility
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

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("TRLevelUtility " + Assembly.GetExecutingAssembly().GetName().Version.ToString(2) +
                                  " - (c) zdimension 2016");
                Console.WriteLine("Many thanks to the TombRaiderForums guys (in particular b122251 who helped me a lot), and to the the TRosettaStone authors.");
                Console.WriteLine("Usage: tlc <file name> <command>");
                Console.WriteLine("Commands:");
                Console.WriteLine("info                              Shows information about a level.");
                Console.WriteLine("convert <format[d]> <file name>   Converts a level between TR games.");
                Console.WriteLine("     File extension: phd  tub  tr2 tr2 tr4 trc");
                Console.WriteLine("     Output format:  TR1 TR1UB TR2 TR3 TR4 TR5");
                Console.WriteLine("     Add 'd' at the end of the format if for the demo version");
                Console.WriteLine("     or for writing files containing only palette and textiles like TR3's 'VICT.TR2'");
                Console.WriteLine("dumptex                           Dumps all the textures of a level to PNG files.");
                Console.WriteLine(
                    "     A folder called '<file name> - Textures' containing three folders ('8-bit', '16-bit' and '32-bit') will be created.");
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

                switch (line[0])
                {
                    case "info":
                        if (Path.GetExtension(inf).ToUpper() == ".DAT")
                        {
                            var infn = Path.GetFileName(inf).ToUpper().Trim();
                            done = true;
                            if (infn == "TOMBPC.DAT" || infn == "TOMBPSX.DAT")
                            {
                                var sc = TOMBPCFile.ParseFile(inf, inf == "TOMBPSX.DAT");
                                Console.WriteLine($@"
Version: {sc.GameVersion:D} ({sc.GameVersion})
Description: {sc.CopyrightInfo}
FirstOption: {sc.FirstOption}
TitleReplace: {sc.TitleReplace}
OnDeathDemoMode: {sc.OnDeathDemoMode}
OnDeathInGame: {sc.OnDeathInGame}
DemoTime: {sc.DemoTime} game ticks ({sc.DemoTime / 30} seconds)
OnDemoInterrupt: {sc.OnDemoInterrupt}
OnDemoEnd: {sc.OnDemoEnd}
Levels: {sc.NumLevels}
Demo levels: {sc.NumDemoLevels}
Title sound ID: {sc.TitleSoundID}
SingleLevel: {sc.SingleLevel}
Flags: {sc.Flags:D} ({sc.Flags})
XOR key: 0x{sc.XORbyte:X} ({sc.XORbyte})
Language ID: {sc.Language:D} ({sc.Language})
Secret sound ID: {sc.SecretSoundID}
Level strings:
{string.Join("\n", sc.LevelDisplayNames)}
Chapter screens: {sc.NumChapterScreens}
{string.Join("\n", sc.ChapterScreens)}
Title strings: {sc.NumTitles}
{string.Join("\n", sc.TitleFileNames)}
FMV strings: {sc.NumFMVs}
{string.Join("\n", sc.FMVFileNames)}
Level path strings:
{string.Join("\n", sc.LevelFileNames)}
Cutscene path strings: {sc.NumCutscenes}
{string.Join("\n", sc.CutSceneFileNames)}
Demo level IDs: {(sc.DemoLevelIDs == null ? "Not present" : string.Join("; ", sc.DemoLevelIDs))}
Game strings: {sc.GameStrings1.Length}
{string.Join("\n", sc.GameStrings1)}
{(sc.IsPSX ? "PSX" : "PC")} strings: {sc.GameStrings2.Length}
{string.Join("\n", sc.GameStrings2)}
<puzzle and key strings not shown here>
");
                                sc.Write("tombpc2.dat");
                            }
                            else if (infn == "SCRIPT.DAT")
                            {

                            }
                            else if (infn.IsAnyOf("ENGLISH.DAT", "FRENCH.DAT", "GERMAN.DAT", "ITALIAN.DAT", "SPANISH.DAT",
                                "US.DAT"))
                            {
                                Die("Nothing to do for: " + inf, EX_USAGE);
                            }
                            else
                            {
                                done = false;
                            }
                        }
                        break;
                }

                if (!done)
                {
                    var lvl = OrDie(() => Level.FromFile(inf), e => $"Error while loading level '{inf}': {e.Message}", EX_DATAERR);

                    switch (line[0])
                    {
                        case "info":
                            Console.WriteLine($@"
Game version: {lvl.GameVersion}
Engine version: {lvl.EngineVersion}
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
Moveables: {lvl.Moveables.Length}
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
Items: {lvl.Items.Length}
AI objects (TR4-5): {lvl.AIObjects?.Length.ToString() ?? "Not present"}
Cinematic frames (TR1-2-3): {lvl.CinematicFrames?.Length.ToString() ?? "Not present"}
Demo data: {lvl.DemoData.Length}
Sound details: {lvl.SoundDetails.Length}
Sample indices: {lvl.SampleIndices.Length}
Lara type (TR5 only): {lvl.LaraType:D} ({lvl.LaraType})
Weather type (TR5 only): {lvl.WeatherType:D} ({lvl.WeatherType})
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

                            if (fmts.EndsWith("d"))
                            {
                                fmts = fmts.Substring(0, fmts.Length - 1).Trim();
                                lvl.WriteIsDemoOrUb = true;
                            }

                            var fmti = Array.IndexOf(new[] {"TR1", "TR1UB", "TR2", "TR3", "TR4", "TR5"}, fmts);
                            if (fmti == -1)
                            {
                                Die("Unknown format: " + fmts, EX_USAGE);
                            }
                            var fmt =
                                new[]
                                {TRGame.TR1, TRGame.TR1UnfinishedBusiness, TRGame.TR2, TRGame.TR3, TRGame.TR4, TRGame.TR5}
                                    [fmti];
                            if (fmt == TRGame.TR1UnfinishedBusiness) lvl.WriteIsDemoOrUb = true;

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

                            Console.WriteLine("Starting level conversion...");

                            OrDie(() =>
                            {
                                using (var fs = File.Open(outf, FileMode.Create))
                                {
                                    lvl.Write(fs, fmt);
                                }
                            }, e => "Error while converting level: " + e.Message, EX_CANTCREAT);

                            Console.WriteLine("Level conversion finished");
                            break;
                        case "dumptex":
                            var outd = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                                Path.GetFileName(inf) + " - Textures");
                            if (File.Exists(outd))
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
                            unsafe
                            {
                                Directory.CreateDirectory(outd);
                                var _8bit = Path.Combine(outd, "8-bit");
                                Directory.CreateDirectory(_8bit);
                                lvl.GenTexAndPalettesIfEmpty();
                                if (lvl.Texture8 == null)
                                {
                                    Console.WriteLine("Level doesn't contain 8-bit textures.");
                                }
                                else
                                {
                                    var strlen = lvl.Texture8.Length.ToString().Length;
                                    for (var bi = 0; bi < lvl.Texture8.Length; bi++)
                                    {
                                        var bt = lvl.Texture8[bi];
                                        var bmp = new Bitmap(256, 256, PixelFormat.Format32bppArgb);
                                        for (var y = 0; y < 256; y++)
                                        {
                                            for (var x = 0; x < 256; x++)
                                            {
                                                bmp.SetPixel(x, y, lvl.Palette.Colour[bt.Pixels[y][x]]);
                                            }
                                        }
                                        bmp.Save(Path.Combine(_8bit, "tex_" + bi.ToString().PadLeft(strlen, '0') + ".png"));
                                    }
                                }
                                var _16bit = Path.Combine(outd, "16-bit");
                                Directory.CreateDirectory(_16bit);
                                if (lvl.Texture16 == null)
                                {
                                    Console.WriteLine("Level doesn't contain 16-bit textures.");
                                }
                                else
                                {
                                    var strlen = lvl.Texture16.Length.ToString().Length;
                                    for (var bi = 0; bi < lvl.Texture16.Length; bi++)
                                    {
                                        var bt = lvl.Texture16[bi];
                                        var bmp = new Bitmap(256, 256, PixelFormat.Format32bppArgb);
                                        for (var y = 0; y < 256; y++)
                                        {
                                            for (var x = 0; x < 256; x++)
                                            {
                                                bmp.SetPixel(x, y, new ByteColor(bt.Pixels[y][x]));
                                            }
                                        }
                                        bmp.Save(Path.Combine(_16bit, "tex_" + bi.ToString().PadLeft(strlen, '0') + ".png"));
                                    }
                                }
                                var _32bit = Path.Combine(outd, "32-bit");
                                Directory.CreateDirectory(_32bit);
                                if (lvl.Textures == null)
                                {
                                    Console.WriteLine("Level doesn't contain 32-bit textures.");
                                }
                                else
                                {
                                    var strlen = lvl.Textures.Length.ToString().Length;
                                    for (var bi = 0; bi < lvl.Textures.Length; bi++)
                                    {
                                        var bt = lvl.Textures[bi];
                                        var bmp = new Bitmap(256, 256, PixelFormat.Format32bppArgb);
                                        var bmpData = bmp.LockBits(new Rectangle(0, 0, 256, 256), ImageLockMode.WriteOnly,
                                            PixelFormat.Format32bppArgb);
                                        var scan0 = (uint*) bmpData.Scan0;
                                        for (var y = 0; y < 256; y++)
                                        {
                                            var scanline = bt.Pixels[y];
                                            fixed (uint* ptr = scanline)
                                            {
                                                for (var i = 0; i < 256; i++)
                                                    scan0[y * 256 + i] = (ptr[i] & 0xff00ff00) |
                                                                         ((ptr[i] & 0x00ff0000) >> 16) |
                                                                         ((ptr[i] & 0x000000ff) << 16);
                                            }
                                        }
                                        bmp.Save(Path.Combine(_32bit, "tex_" + bi.ToString().PadLeft(strlen, '0') + ".png"));
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
            Console.ReadLine();
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
    }
}