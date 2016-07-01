using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using FreeRaider.Loader;

namespace TRLevelUtility
{
    class Program
    {
        // Exit codes from sysexits.h

        /// <summary>
        /// The command was used incorrectly, e.g., with the wrong number of arguments, a bad flag,
        /// a bad syntax in a parameter, or whatever.
        /// </summary>
        public const int EX_USAGE = 64;
        /// <summary>
        /// The input data was incorrect in some way. This should only be used for user's data and
        /// not system files.
        /// </summary>
        public const int EX_DATAERR = 65;
        /// <summary>
        /// An input file (not a system file) did not exist or was not readable.
        /// </summary>
        public const int EX_NOINPUT = 66;
        /// <summary>
        /// A (user specified) output file cannot be created.
        /// </summary>
        public const int EX_CANTCREAT = 73;

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("TRLevelUtility " + Assembly.GetExecutingAssembly().GetName().Version.ToString(2) + " - (c) zdimension 2016");
                Console.WriteLine("Many thanks to the TombRaiderForums guys, and to the the TRosettaStone authors.");
                Console.WriteLine("Usage: tlc <file name> <command>");
                Console.WriteLine("Commands:");
                Console.WriteLine("info                              Shows information about a level.");
                Console.WriteLine("convert <format[d]> <file name>   Converts a level between TR games.");
                Console.WriteLine("     File extension: phd  tub  tr2 tr2 tr4 trc");
                Console.WriteLine("     Output format:  TR1 TR1UB TR2 TR3 TR4 TR5");
                Console.WriteLine("     Add 'd' at the end of the format if for the demo version");
                Console.WriteLine("     or for writing files containing only palette and textiles like TR3's 'VICT.TR2'");
                Console.WriteLine("dumptex                           Dumps all the textures of a level to PNG files.");
                Console.WriteLine("     A folder called '<file name> - Textures' containing three folders ('8-bit', '16-bit' and '32-bit') will be created.");
                Environment.Exit(EX_USAGE);
            }

            var inf = args[0].Trim();

            if (!File.Exists(inf))
            {
                Console.WriteLine("The input file '" + inf + "' doesn't exist.");
                Environment.Exit(EX_NOINPUT);
            }

            Level lvl = null;
            try
            {
                lvl = Level.FromFile(inf);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Error while loading level '{0}': {1}", inf, e.Message);
                Environment.Exit(EX_DATAERR);
            }

            if (args.Length > 1)
            {
                var line = args.Skip(1).ToArray();
                line[0] = line[0].ToLower();

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
                            Console.Error.WriteLine("Invalid parameter count for command 'convert': " + line.Length + ", expected 3");
                            Environment.Exit(EX_USAGE);
                            return;
                        }
                        var fmts = line[1].ToUpper().Trim();
                        var outf = line[2].Trim();

                        if (fmts.EndsWith("d"))
                        {
                            fmts = fmts.Substring(0, fmts.Length - 1).Trim();
                            lvl.WriteIsDemoOrUb = true;
                        }

                        var fmti = Array.IndexOf(new[] { "TR1", "TR1UB", "TR2", "TR3", "TR4", "TR5" }, fmts);
                        if (fmti == -1)
                        {
                            Console.Error.WriteLine("Unknown format: " + fmts);
                            Environment.Exit(EX_USAGE);
                        }
                        var fmt =
                            new[] { TRGame.TR1, TRGame.TR1UnfinishedBusiness, TRGame.TR2, TRGame.TR3, TRGame.TR4, TRGame.TR5 }
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
                                Console.Error.WriteLine("File not deleted, operation aborted, exiting.");
                                Environment.Exit(EX_CANTCREAT);
                            }
                        }

                        Console.WriteLine("Starting level conversion...");

                        try
                        {
                            using (var fs = File.Open(outf, FileMode.Create))
                            {
                                lvl.Write(fs, fmt);
                            }
                        }
                        catch (Exception e)
                        {
                            Console.Error.WriteLine("Error while converting level: " + e.Message);
                            Environment.Exit(EX_CANTCREAT);
                        }

                        Console.WriteLine("Level conversion finished");
                        Console.ReadLine();
                        break;
                    case "dumptex":
                        var outd = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                            Path.GetFileName(inf) + " - Textures");
                        if (File.Exists(outd))
                        {
                            Console.WriteLine("Warning: the output directory '{0}' already exists.", Path.GetFileName(outd));
                            Console.Write("Do you want to delete it? (y/n) ");
                            if (Console.ReadLine().ToLower().Trim() == "y")
                            {
                                Directory.Delete(outd, true);
                                Console.WriteLine("Directory deleted.");
                            }
                            else
                            {
                                Console.Error.WriteLine("Directory not deleted, operation aborted, exiting.");
                                Environment.Exit(EX_CANTCREAT);
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
                        Console.ReadLine();
                        break;
                    default:
                        Console.Error.WriteLine("Unknown command: " + line[0]);
                        Environment.Exit(EX_USAGE);
                        return;
                }
            }
        }
    }
}
