using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Configuration;
using System.Net.Mime;
using System.Security.Cryptography;
using Ionic.Zlib;
using static FreeRaider.Loader.Constants;

namespace FreeRaider.Loader
{
    public partial class Level
    {
        public LFormat Format;

        private BinaryReader reader;


        public Level(BinaryReader br, LFormat ver, string sfxpath = "MAIN.SFX", bool psx = false)
        {
            Format = ver;
            reader = br;
            SfxPath = sfxpath;
            Load();
        }

        public LFormat WriteFormat;

        public ByteTexture[] Texture8;
        public WordTexture[] Texture16;

        /// <summary>
        /// Color palette
        /// </summary>
        public Palette Palette;
        public Palette Palette16;

        public DWordTexture[] Textures;

        public Room[] Rooms;

        public ushort[] FloorData;

        public Mesh[] Meshes;

        public uint[] MeshIndices;

        public Animation[] Animations;

        public StateChange[] StateChanges;

        public AnimDispatch[] AnimDispatches;

        public short[] AnimCommands;

        public Model[] Models;

        public StaticMesh[] StaticMeshes;

        public ObjectTexture[] ObjectTextures;

        public ushort[] AnimatedTextures;

        public uint AnimatedTexturesUVCount;

        public SpriteTexture[] SpriteTextures;

        public SpriteSequence[] SpriteSequences;

        public Camera[] Cameras;

        public FlybyCamera[] FlybyCameras = new FlybyCamera[0];

        public SoundSource[] SoundSources;

        public Box[] Boxes;

        public ushort[] Overlaps;

        public Zone[] Zones;

        public Entity[] Entities;

        public LightMap LightMap;

        public AIObject[] AIObjects = new AIObject[0];

        public CinematicFrame[] CinematicFrames = new CinematicFrame[0];

        public byte[] DemoData;

        public short[] SoundMap;

        public SoundDetails[] SoundDetails;

        public int SamplesCount;

        public byte[] SamplesData = new byte[0];

        public uint[] SampleIndices;

        public ushort[] FrameData;

        public uint[] MeshTreeData;

        public string SfxPath = "MAIN.SFX";

        public LaraType LaraType = LaraType.Normal;

        public WeatherType WeatherType = WeatherType.Normal;

        private void ReadMeshData()
        {
            var dataWords = reader.ReadUInt32();
            var pos = reader.BaseStream.Position;

            var dataSize = dataWords * 2;
            reader.BaseStream.Seek(dataSize, SeekOrigin.Current);

            var numMeshIndices = reader.ReadUInt32();
            MeshIndices = reader.ReadUInt32Array(numMeshIndices);

            var end = reader.BaseStream.Position;

            uint dataPos = 0;
            Meshes = new Mesh[numMeshIndices];

            for (uint i = 0; i < numMeshIndices; i++)
            {
                MeshIndices.Replace(dataPos, i);

                reader.BaseStream.Position = pos + dataPos;

                Meshes[i] = Mesh.Read(reader, Format);

                for (var j = 0; j < numMeshIndices; j++)
                {
                    if (MeshIndices[j] > dataPos)
                    {
                        dataPos = MeshIndices[j];
                        break;
                    }
                }
            }

            reader.BaseStream.Position = end;
        }

        private void WriteMeshData()
        {
            // todo test this

            writer.Write((uint)0); // we'll write that later

            var tmpMeshIndices = new uint[Meshes.Length];

            var pos = writer.BaseStream.Position;

            for (var i = 0; i < MeshIndices.Length; i++)
            {
                // if the mesh index is 0 for an item other than the first, then we don't need to write again
                if (MeshIndices[i] != 0 || i == 0) 
                {
                    tmpMeshIndices[i] = (uint) (writer.BaseStream.Position - pos);
                    Meshes[MeshIndices[i]].Write(writer, WriteFormat);

                    while ((writer.BaseStream.Position - pos) % 4 != 0)
                        writer.BaseStream.Position++; // TR engine wants the data to be 4-byte aligned
                }
            }

            var numDataWords = (uint)(writer.BaseStream.Position - pos) / 2;
            var tmpPos = writer.BaseStream.Position;
            writer.BaseStream.Position = pos - 4;
            writer.Write(numDataWords);
            writer.BaseStream.Position = tmpPos;

            writer.Write((uint)Meshes.Length);

            writer.WriteUInt32Array(tmpMeshIndices);
        }

        private void ReadFrameMoveableData()
        {
            var numFrameData = reader.ReadUInt32();
            var frameDataPos = reader.BaseStream.Position;
            FrameData = reader.ReadUInt16Array(numFrameData);

            var numMoveables = reader.ReadUInt32();
            Models = new Model[numMoveables];
            for (uint i = 0; i < numMoveables; i++)
            {
                Models[i] = Loader.Model.Read(reader, Format);

                // Disable unused skybox polygons
                if (Format == Engine.TR3 && Models[i].ObjectID == 355)
                {
                    Array.Resize(ref Meshes[MeshIndices[Models[i].StartingMesh]].ColouredRectangles, 16);
                }
            }

            var end = reader.BaseStream.Position;

            uint pos = 0;

            for (uint i = 0; i < numFrameData; i++)
            {
                for (uint j = 0; j < numMoveables; j++)
                {
                    if(Models[j].FrameOffset == pos)
                    {
                        Models[j].FrameIndex = i;
                        // Moveables[j].FrameOffset = 0;
                    }
                }

                reader.BaseStream.Position = frameDataPos + pos;

                pos = 0;

                for (uint j = 0; j < numMoveables; j++)
                {
                    if (Models[j].FrameOffset > pos)
                    {
                        pos = Models[j].FrameOffset;
                        break;
                    }
                }
            }

            reader.BaseStream.Position = end;
        }

        public static int ConvertID(Engine from, Engine to, uint id)
        {
            var mod = Catalogue.Models.FirstOrDefault(x => x[(int) from] == id);
            if (mod == null) return -1;

            return mod[(int) to];
        }

        public static Model[] ConvertModelArray(Engine from, Engine to, Model[] arr)
        {
            if (from == to) return arr.ToArray();

            var ret = new List<Model>();

            foreach (var m in arr)
            {
                var newID = ConvertID(from, to, m.ObjectID);

                if (newID != -1)
                {
                    var clone = m;
                    clone.ObjectID = (uint)newID;
                    ret.Add(clone);
                }
            }

            return ret.ToArray();
        }

        public static Entity[] ConvertEntityArray(Engine from, Engine to, Entity[] arr)
        {
            if (from == to) return arr.ToArray();

            var ret = new List<Entity>();

            foreach (var m in arr)
            {
                var newID = ConvertID(from, to, (uint)m.ObjectID);

                if (newID != -1)
                {
                    var clone = m;
                    clone.ObjectID = (short)newID;
                    ret.Add(clone);
                }
            }

            return ret.ToArray();
        }

        private void WriteFrameMoveableData()
        {
            writer.Write((uint)FrameData.Length);

            writer.WriteUInt16Array(FrameData);

            var newMod = ConvertModelArray(Format.Engine, WriteFormat.Engine, Models);

            writer.Write((uint)newMod.Length);

            // todo maybe loss of data since unused skybox polys are disabled

            // todo also some stuff may or may not work since some things are modified

            writer.WriteArray(newMod, x => x.Write(writer, WriteFormat));
        }

        public static LFormat ParseVersion(string name)
        {
            var ret = LFormat.Unknown;

            try
            {
                using (var fs = File.OpenRead(name))
                using (var br = new BinaryReader(fs))
                {
                    ret = ParseVersion(br, Path.GetExtension(name));
                }
            }
            catch
            {
                
            }

            return ret;
        }

        public static LFormat ParseVersion(BinaryReader br, string fext)
        {
            if (fext.Length >= 4)
            {
                fext = fext.ToUpper();
                /*var check = br.ReadBytes(4);
            var ver = check[0] | (uint)(check[1] << 8) | (uint)(check[2] << 16) | (uint)(check[3] << 24);*/
                var ver = br.ReadUInt32();
                switch (fext)
                {
                    case ".PHD":
                        if (ver == 0x00000020)
                            return TRGame.TR1;
                        break;
                    case ".TUB":
                        if (ver == 0x00000020)
                            return TRGame.TR1Demo;
                        break;
                    case ".TR2":
                        if (ver == 0x0000002D)
                            return TRGame.TR2;
                        else if (ver.IsAnyOf(0xFF080038, 0xFF180038, 0xFF180034))
                            return TRGame.TR3;
                        break;
                    case ".TR4":
                        if (ver.IsAnyOf((uint) 0x00345254, 0xFFFFFFF0))
                            return TRGame.TR4;
                        if (ver == 0x63345254)
                            return TRGame.TR4Demo;
                        break;
                    case ".TRC":
                        if (ver == 0x00345254)
                            return TRGame.TR5;
                        break;

                    case ".PSX":
                        br.ReadUInt32();
                        ver = br.ReadUInt32();
                        if(ver == 0x56414270)
                            return new LFormat(TRGame.TR1, LevelPlatform.PSX);
                        break;
                }
            }
            return LFormat.Unknown;
        }

        public static LevelPlatform GetLevelFormat(string name)
        {
            // PLACEHOLDER: Currently, only PC levels are supported.
            return LevelPlatform.PC;
        }

        public static Level FromFile(string fileName)
        {
            var br = new BinaryReader(File.OpenRead(fileName));
            var ver = ParseVersion(br, Path.GetExtension(fileName));
            if (ver == Engine.Unknown)
            {
                throw new ArgumentException("Couldn't detect level version", nameof(ver));
            }
            return new Level(br, ver, Path.Combine(Path.GetDirectoryName(Path.GetFullPath(fileName)), "MAIN.SFX"));
        }

        public static Level FromFile(string fileName, TRGame ver)
        {
            return new Level(new BinaryReader(File.OpenRead(fileName)), ver, Path.Combine(Path.GetDirectoryName(Path.GetFullPath(fileName)), "MAIN.SFX"));
        }

        private void Load()
        {
            reader.BaseStream.Position = 0;
            switch (Format.Engine)
            {
                case Engine.TR1:
                    Load_TR1();
                    break;
                case Engine.TR2:
                    Load_TR2();
                    break;
                case Engine.TR3:
                //case TRGame.TR3Gold:
                    Load_TR3();
                    break;
                case Engine.TR4:
                    Load_TR4();
                    break;
                case Engine.TR5:
                    Load_TR5();
                    break;
                case Engine.Unknown:
                    throw new ArgumentException("Unable to detect level format.");
            }
            if (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                Cerr.Write("Error: " + (reader.BaseStream.Length - reader.BaseStream.Position) +
                           " bytes of data after end of level");
            }
        }

        public void Write(string fileName, LFormat fmt)
        {
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
            using (var fs = File.OpenWrite(fileName))
            {
                Write(fs, fmt);
            }
        }

        public void Write(Stream s, LFormat fmt)
        {
            using (var bw = new BinaryWriter(s))
            {
                Write(bw, fmt);
            }
        }

        public void Write(BinaryWriter bw, LFormat fmt)
        {
            WriteFormat = fmt;
            writer = bw;
            internalWriteBefore();
        }

        private void internalWriteBefore()
        {
            try
            {
                if (writer == null || !writer.BaseStream.CanWrite) throw new NullReferenceException("'writer' is null");

                GenTexAndPalettesIfEmpty(true, true, true);

                switch (WriteFormat.Engine)
                {
                    case Engine.TR1:
                        Write_TR1();
                        break;
                    case Engine.TR2:
                        Write_TR2();
                        break;
                    case Engine.TR3:
                        Write_TR3();
                        break;
                    case Engine.TR4:
                        Write_TR4();
                        break;
                    case Engine.TR5:
                        Write_TR5();
                        break;
                    case Engine.Unknown:
                        throw new ArgumentException("Unknown level format: " + (int)WriteFormat.Game);
                }
            }
            finally
            {
                writer.Close();
            }
        }

        private BinaryWriter writer;

        public StaticMesh FindStaticMeshById(uint objectID)
        {
            return StaticMeshes.FirstOrDefault(x => x.ObjectID == objectID && MeshIndices[x.Mesh] != 0);
        }

        public Entity FindItemById(short objectID)
        {
            return Entities.FirstOrDefault(x => x.ObjectID == objectID);
        }

        public Model FindMoveableById(uint objectID)
        {
            return Models.FirstOrDefault(x => x.ObjectID == objectID);
        }

        private unsafe void Gen16BitTexPalette()
        {
            if ((Texture16 ?? new WordTexture[0]).Length == 0)
            {
                // Generate 16-bit textures
                Texture16 = new WordTexture[Textures.Length];
                for (var i = 0; i < Texture16.Length; i++)
                {
                    Texture16[i] = new WordTexture(new ushort[TextureSize]);
                    fixed (uint* pix32 = Textures[i].Pixels)
                    fixed (ushort* pix16 = Texture16[i].Pixels)
                        for (var j = 0; j < TextureSize; j++)
                        {
                            pix16[j] = new ByteColor(pix32[j]).ToUInt16();
                        }
                }
            }
            if (/*WriteEngineVersion < Engine.TR4 &&*/ Equals(Palette16, default(Palette)))
            {
                Palette16 = new Palette { Colour = Palette.Colour.Select(x => new ByteColor(x.R, x.G, x.B, 0xFF)).ToArray() };
            }
        }

        private void Gen8BitTexPalette()
        {
            if ((Equals(Palette, default(Palette)) || (Texture8 ?? new ByteTexture[0]).Length == 0))
            {
                unsafe
                {
                    Texture8 = new ByteTexture[Textures.Length];

                    var bigBmp = new Bitmap(Textures.Length * 256, 256, PixelFormat.Format32bppArgb);
                    for (var bi = 0; bi < Textures.Length; bi++)
                    {
                        var bt = Textures[bi];
                        var bmp = new Bitmap(256, 256, PixelFormat.Format32bppArgb);
                        var bmpData = bmp.LockBits(new Rectangle(0, 0, 256, 256), ImageLockMode.WriteOnly,
                            PixelFormat.Format32bppArgb);
                        var scan0 = (uint*)bmpData.Scan0;
                        fixed (uint* ptr = bt.Pixels)
                        {
                            uint* pp = ptr;
                            for (var i = 0; i < 65536; i++, pp++)
                            {
                                if (*pp == 0)
                                    scan0[i] = 0xffff00ff;
                                else
                                    scan0[i] = (*pp & 0xff00ff00) |
                                               ((*pp & 0x00ff0000) >> 16) |
                                               ((*pp & 0x000000ff) << 16);
                            }
                        }
                        bmp.UnlockBits(bmpData);
                        using (var gr = Graphics.FromImage(bigBmp))
                            gr.DrawImage(bmp, new Rectangle(bi * 256, 0, 256, 256), 0, 0, 256, 256, GraphicsUnit.Pixel);
                    }
                    var bmp8 = bigBmp.Clone(new Rectangle(0, 0, bigBmp.Width, 256), PixelFormat.Format8bppIndexed);
                    var bmpData8 = bmp8.LockBits(new Rectangle(0, 0, bigBmp.Width, 256), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                    var scan08 = (uint*)bmpData8.Scan0;
                    Palette = new Palette() { Colour = bmp8.Palette.Entries.Select(x => (ByteColor)x).ToArray().Resize(256) };
                    for (var i = 0; i < 256; i++)
                    {
                        Palette.Colour[i].R >>= 2;
                        Palette.Colour[i].G >>= 2;
                        Palette.Colour[i].B >>= 2;
                    }
                    for (var i = 0; i < Textures.Length; i++)
                    {
                        Texture8[i] = new ByteTexture(new byte[TextureSize]);

                        fixed (byte* ptr = Texture8[i].Pixels)
                        {
                            for (var y = 0; y < 256; y++)
                                for (var x = 0; x < 256; x++)
                                {
                                    ptr[(y << 8) | x] = (byte)Array.IndexOf(bmp8.Palette.Entries, Color.FromArgb((int)scan08[(y * bigBmp.Width) + ((i << 8) + x)]));
                                }
                        }
                    }
                    bmp8.UnlockBits(bmpData8);



                    //Gen8bitPaletteFrom32bitTex(Textures);

                    // Generate 8-bit textures

                    /*for (var i = 0; i < Textures.Length; i++)
                    {
                        var tex = Textures[i];
                        Texture8[i] = new ByteTexture(new byte[256][]);
                        for (var y = 0; y < 256; y++)
                        {
                            Texture8[i].Pixels[y] = new byte[256];

                            for (var x = 0; x < 256; x++)
                            {
                                Texture8[i].Pixels[y][x] =
                                    (byte) Helper.closestColor1(Palette.Colour, new ByteColor(tex.Pixels[y][x]));
                            }
                        }
                    }*/
                }
            }
        }

        private void Gen32BitTexPalette()
        {
            var has8 = (Texture8 ?? new ByteTexture[0]).Length != 0;
            var has16 = (Texture16 ?? new WordTexture[0]).Length != 0;
            if (!has8 && !has16)
            {
                Gen16BitTexPalette();
            }
            if ((Texture16 ?? new WordTexture[0]).Length != 0)
            {
                Textures = new DWordTexture[Texture16.Length];
                for (uint i = 0; i < Texture16.Length; i++)
                {
                    Textures[i] = ConvertTexture(Texture16[i]);
                }
            }
            else
            {
                if (!has8) throw new OhShitException("Neither 8-bit nor 16-bit textures are present, so it's quite hard to generate 32-bit ones", true, double.MaxValue);

                Textures = new DWordTexture[Texture8.Length];
                for (uint i = 0; i < Texture8.Length; i++)
                {
                    Textures[i] = ConvertTexture(Palette, Texture8[i]);
                }
            }
        }

        public unsafe void GenTexAndPalettesIfEmpty(bool _8 = true, bool _16 = true, bool _32 = true)
        {
            //if (EngineVersion == Engine.TR2 || EngineVersion == Engine.TR3) return; // TR2 and TR3 already have 8-bit, 16-bit and 32-bit tex and palette

            if (_8) Gen8BitTexPalette();
            
            if(_16) Gen16BitTexPalette();
            
        }

        private static unsafe uint[] GetTopColors(DWordTexture[] texs, int n = 256)
        {
            var res = new Dictionary<uint, int>();
            for (int index = 0; index < texs.Length; index++)
            {
                var tex = texs[index];
                fixed(uint* ptr = tex.Pixels)
                for (var i = 0; i < TextureSize; i++)
                {
                        if (!res.ContainsKey(ptr[i])) res[ptr[i]] = 1;
                        else res[ptr[i]]++;
                    
                }
            }
            return res.OrderByDescending(x => x.Value).Distinct().Select(x => x.Key).Take(n).ToArray();
        }

        /*private static ByteTexture ConvertTexture(DWordTexture tex, out Palette p)
        {
            var top = GetTopColors(tex).Select(x => new ByteColor(x));
            p = new Palette() {Colour = top.ToArray()};
            var res = new ByteTexture(new byte[256][]);
            for (var y = 0; y < 256; y++)
            {
                res.Pixels[y] = new byte[256];

                for (var x = 0; x < 256; x++)
                {
                    res.Pixels[y][x] = (byte) closestColor1(top, new ByteColor(tex.Pixels[y][x]));
                }
            }
            return res;
        }*/

        private unsafe void Gen8bitPaletteFrom32bitTex(DWordTexture[] texs)
        {
            for (int index = 0; index < texs.Length; index++)
            {
                var bt = texs[index];
                var bmp = new Bitmap(256, 256, PixelFormat.Format32bppArgb);
                var bmpData = bmp.LockBits(new Rectangle(0, 0, 256, 256), ImageLockMode.WriteOnly,
                    PixelFormat.Format32bppArgb);
                var scan0 = (uint*) bmpData.Scan0;
                fixed(uint* ptr = bt.Pixels)
                for (var i = 0; i < 65536; i++)
                {
                    scan0[i] = (ptr[i] & 0xff00ff00) |
                                             ((ptr[i] & 0x00ff0000) >> 16) |
                                             ((ptr[i] & 0x000000ff) << 16);
                }
                bmp.UnlockBits(bmpData);
                /*for (var y = 0; y < 256; y++)
                {
                    var ptr = bt.Pixels + y * 256;
                    for (var i = 0; i < 256; i++)
                        scan0[y * 256 + i] = (ptr[i] & 0xff00ff00) |
                                             ((ptr[i] & 0x00ff0000) >> 16) |
                                             ((ptr[i] & 0x000000ff) << 16);
                }*/
                var bmp8 = bmp.Clone(new Rectangle(0, 0, 256, 256), PixelFormat.Format8bppIndexed);
                Palette = new Palette() {Colour = bmp8.Palette.Entries.Select(x => (ByteColor) x).ToArray()};
            }
            /*var top = GetTopColors(texs).Select(x => new ByteColor(x)).ToArray();
            Palette = new Palette() { Colour = top };*/
        }

        private static unsafe DWordTexture ConvertTexture(Palette p, ByteTexture t)
        {
            var ret = new DWordTexture();
            ret.Pixels = new uint[TextureSize];
            fixed(byte* ptr = t.Pixels)
            fixed(uint* ptr32 = ret.Pixels)
            for (var i = 0; i < TextureSize; i++)
            {
                var color = ptr[i];
                uint val = 0;
                if (color > 0)
                {
                    var palc = p.Colour[color];
                    val = (uint)(palc.R | (palc.G << 8) | (palc.B << 16) | (0xff << 24));
                }
                ptr32[i] = val;
            }
            return ret;
        }

        private static unsafe DWordTexture ConvertTexture(WordTexture t)
        {
            var ret = new DWordTexture();
            ret.Pixels = new uint[TextureSize];
            fixed (ushort* ptr = t.Pixels)
            fixed (uint* ptr32 = ret.Pixels)
                for (var i = 0; i < TextureSize; i++)
                {
                    var color = ptr[i];
                    uint val = 0;
                    if ((color & 0x8000) != 0)
                    {
                        val = (uint) (
                            ((color & 0x00007c00) >> 7)
                            | (((color & 0x000003e0) >> 2) << 8)
                            | ((color & 0x0000001f) << 3 << 16)
                            | 0xff000000);
                    }
                    ptr32[i] = val;
                }

            return ret;
        }
    }

    public enum LaraType : ushort
    {
        Normal = 0,
        Catsuit = 3,
        Divesuit = 4,
        Invisible = 6
    }

    public enum WeatherType : ushort
    {
        Normal = 0,
        Rain = 1,
        Snow = 2
    }

    /*public class LevelReader : IDisposable
    {
        public LevelReader(BinaryReader bw)
        {
            reader = bw;
        }

        public LevelReader(Stream s) : this(new BinaryReader(s))
        {
        }

        private BinaryReader reader;

        public Level Read(TRGame version)
        {
            
        }


        public void Dispose()
        {
            reader.Dispose();
        }
    }*/

    public class OhShitException : Exception
    {
        public OhShitException(string message, bool shitBrix = false, double shitLevel = 0)
        {

        }
    }
}
