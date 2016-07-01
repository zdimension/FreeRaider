using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Ionic.Zlib;

namespace FreeRaider.Loader
{
    public partial class Level
    {
        public TRGame GameVersion { get; private set; }

        public Engine EngineVersion { get; private set; }

        private BinaryReader reader;


        public Level(BinaryReader br, TRGame ver)
        {
            setGameVer(ver);
            reader = br;
            Load();
        }

        private void setGameVer(TRGame ver)
        {
            GameVersion = ver;
            EngineVersion = Helper.GameToEngine(ver, out IsDemoOrUb);
        }

        private void setGameVerWrite(TRGame ver)
        {
            WriteGameVersion = ver;
            WriteEngineVersion = Helper.GameToEngine(ver, out WriteIsDemoOrUb);
        }


        public bool IsDemoOrUb;
        public bool WriteIsDemoOrUb;

        public TRGame WriteGameVersion { get; private set; }

        public Engine WriteEngineVersion { get; private set; }

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

        public Moveable[] Moveables;

        public StaticMesh[] StaticMeshes;

        public ObjectTexture[] ObjectTextures;

        public ushort[] AnimatedTextures;

        public uint AnimatedTexturesUVCount;

        public SpriteTexture[] SpriteTextures;

        public SpriteSequence[] SpriteSequences;

        public Camera[] Cameras;

        public FlybyCamera[] FlybyCameras;

        public SoundSource[] SoundSources;

        public Box[] Boxes;

        public ushort[] Overlaps;

        public Zone[] Zones;

        public Item[] Items;

        public LightMap LightMap;

        public AIObject[] AIObjects;

        public CinematicFrame[] CinematicFrames;

        public byte[] DemoData;

        public short[] SoundMap;

        public SoundDetails[] SoundDetails;

        public int SamplesCount;

        public byte[] SamplesData;

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

                Meshes[i] = Mesh.Read(reader, EngineVersion);

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
                    Meshes[MeshIndices[i]].Write(writer, WriteEngineVersion);

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
            Moveables = new Moveable[numMoveables];
            for (uint i = 0; i < numMoveables; i++)
            {
                Moveables[i] = Moveable.Read(reader, EngineVersion);

                // Disable unused skybox polygons
                if (EngineVersion == Engine.TR3 && Moveables[i].ObjectID == 355)
                {
                    Array.Resize(ref Meshes[MeshIndices[Moveables[i].StartingMesh]].ColouredRectangles, 16);
                }
            }

            var end = reader.BaseStream.Position;

            uint pos = 0;

            for (uint i = 0; i < numFrameData; i++)
            {
                for (uint j = 0; j < numMoveables; j++)
                {
                    if(Moveables[j].FrameOffset == pos)
                    {
                        Moveables[j].FrameIndex = i;
                        Moveables[j].FrameOffset = 0;
                    }
                }

                reader.BaseStream.Position = frameDataPos + pos;

                pos = 0;

                for (uint j = 0; j < numMoveables; j++)
                {
                    if (Moveables[j].FrameOffset > pos)
                    {
                        pos = Moveables[j].FrameOffset;
                        break;
                    }
                }
            }

            reader.BaseStream.Position = end;
        }

        private void WriteFrameMoveableData()
        {
            writer.Write((uint)FrameData.Length);

            writer.WriteUInt16Array(FrameData);

            writer.Write((uint)Moveables.Length);

            // todo maybe loss of data since unused skybox polys are disabled

            // todo also some stuff may or may not work since some things are modified

            writer.WriteArray(Moveables, x => x.Write(writer, WriteEngineVersion));
        }

        public static TRGame ParseVersion(BinaryReader br, string fext)
        {
            fext = fext.ToUpper();
            var check = br.ReadBytes(4);
            var ver = check[0] | (uint)(check[1] << 8) | (uint)(check[2] << 16) | (uint)(check[3] << 24);
            switch (fext)
            {
                case ".PHD":
                    if (ver == 0x00000020)
                        return TRGame.TR1;
                    break;
                case ".TUB":
                    if (ver == 0x00000020)
                        return TRGame.TR1UnfinishedBusiness;
                    break;
                case ".TR2":
                    if (ver == 0x0000002D)
                        return TRGame.TR2;
                    else if (ver.IsAnyOf(0xFF080038, 0xFF180038, 0xFF180034))
                        return TRGame.TR3;
                    break;
                case ".TR4":
                    if (ver.IsAnyOf(0x00345254, 0x63345254, 0xFFFFFFF0))
                        return TRGame.TR4;
                    break;
                case ".TRC":
                    if (ver == 0x00345254)
                        return TRGame.TR5;
                    break;
            }
            return TRGame.Unknown;
        }

        public static Level FromFile(string fileName)
        {
            var br = new BinaryReader(new FileStream(fileName, FileMode.Open));
            var ver = ParseVersion(br, Path.GetExtension(fileName));
            if (ver == TRGame.Unknown) return null;
            return new Level(br, ver);
        }

        private void Load()
        {
            reader.BaseStream.Position = 0;
            switch (GameVersion)
            {
                case TRGame.TR1:
                case TRGame.TR1Demo:
                case TRGame.TR1UnfinishedBusiness:
                    Load_TR1();
                    break;
                case TRGame.TR2:
                case TRGame.TR2Gold:
                case TRGame.TR2Demo:
                    Load_TR2();
                    break;
                case TRGame.TR3:
                case TRGame.TR3Gold:
                    Load_TR3();
                    break;
                case TRGame.TR4:
                case TRGame.TR4Demo:
                    Load_TR4();
                    break;
                case TRGame.TR5:
                    Load_TR5();
                    break;
                case TRGame.Unknown:
                    throw new ArgumentException("Unable to detect level format.");
            }
            if (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                Cerr.Write("Error: " + (reader.BaseStream.Length - reader.BaseStream.Position) +
                           " bytes of data after end of level");
            }
        }

        public void Write(string fileName, TRGame version)
        {
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
            using (var fs = File.OpenWrite(fileName))
            {
                Write(fs, version);
            }
        }

        public void Write(Stream s, TRGame version)
        {
            using (var bw = new BinaryWriter(s))
            {
                Write(bw, version);
            }
        }

        public void Write(BinaryWriter bw, TRGame version)
        {
            setGameVerWrite(version);
            writer = bw;
            internalWriteBefore();
        }

        private void internalWriteBefore()
        {
            try
            {
                if (writer == null || !writer.BaseStream.CanWrite) throw new NullReferenceException("'writer' is null");

                switch (WriteGameVersion)
                {
                    case TRGame.TR1:
                    case TRGame.TR1Demo:
                    case TRGame.TR1UnfinishedBusiness:
                        Write_TR1();
                        break;
                    case TRGame.TR2:
                    case TRGame.TR2Gold:
                    case TRGame.TR2Demo:
                        Write_TR2();
                        break;
                    case TRGame.TR3:
                    case TRGame.TR3Gold:
                        Write_TR3();
                        break;
                    case TRGame.TR4:
                    case TRGame.TR4Demo:
                        Write_TR4();
                        break;
                    case TRGame.TR5:
                        Write_TR5();
                        break;
                    case TRGame.Unknown:
                        throw new ArgumentException("Unknown level format: " + (int)WriteGameVersion);
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

        public Item FindItemById(short objectID)
        {
            return Items.FirstOrDefault(x => x.ObjectID == objectID);
        }

        public Moveable FindMoveableById(uint objectID)
        {
            return Moveables.FirstOrDefault(x => x.ObjectID == objectID);
        }

        public void GenTexAndPalettesIfEmpty()
        {
            if (EngineVersion == Engine.TR2) return; // TR2 already has 8-bit, 16-bit and 32-bit tex and palette

            if ((Texture16 ?? new WordTexture[0]).Length == 0)
            {
                // Generate 16-bit textures
                Texture16 = new WordTexture[Textures.Length];
                for (var i = 0; i < Texture16.Length; i++)
                {
                    Texture16[i] = new WordTexture(new ushort[256][]);
                    for (var y = 0; y < 256; y++)
                    {
                        Texture16[i].Pixels[y] = new ushort[256];

                        for (var x = 0; x < 256; x++)
                        {
                            Texture16[i].Pixels[y][x] = new ByteColor(Textures[i].Pixels[y][x]).ToUInt16();
                        }
                    }
                }
            }

            if (Equals(Palette, default(Palette)))
            {
                Gen8bitPaletteFrom32bitTex(Textures);
            }

            if ((Texture8 ?? new ByteTexture[0]).Length == 0)
            {
                // Generate 8-bit textures
                Texture8 = new ByteTexture[Textures.Length];
                for (var i = 0; i < Textures.Length; i++)
                {
                    var tex = Textures[i];
                    Texture8[i] = new ByteTexture(new byte[256][]);
                    for (var y = 0; y < 256; y++)
                    {
                        Texture8[i].Pixels[y] = new byte[256];

                        for (var x = 0; x < 256; x++)
                        {
                            Texture8[i].Pixels[y][x] = (byte) Helper.closestColor1(Palette.Colour, new ByteColor(tex.Pixels[y][x]));
                        }
                    }
                }
            }
            if (Equals(Palette16, default(Palette)))
            {
                Palette16 = new Palette {Colour = Palette.Colour.Select(x => new ByteColor(x.R, x.G, x.B, 0xFF)).ToArray()};
            }
        }

        private static uint[] GetTopColors(DWordTexture[] texs, int n = 256)
        {
            var res = new Dictionary<uint, int>();
            foreach (var tex in texs)
            {
                for (var y = 0; y < 256; y++)
                {
                    for (var x = 0; x < 256; x++)
                    {
                        res[tex.Pixels[y][x]]++;
                    }
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

        private void Gen8bitPaletteFrom32bitTex(DWordTexture[] texs)
        {
            var top = GetTopColors(texs).Select(x => new ByteColor(x)).ToArray();
            Palette = new Palette() { Colour = top };
        }

        private static DWordTexture ConvertTexture(Palette p, ByteTexture t)
        {
            var ret = new DWordTexture(new uint[256][]);
            for (var y = 0; y < 256; y++)
            {
                ret.Pixels[y] = new uint[256];

                for (var x = 0; x < 256; x++)
                {
                    var color = t.Pixels[y][x];
                    uint val = 0;
                    if(color > 0)
                    {
                        var palc = p.Colour[color];
                        val = (uint)(palc.R | (palc.G << 8) | (palc.B << 16) | (0xff << 24));
                    }
                    ret.Pixels[y][x] = val;
                }
            }
            return ret;
        }

        private static DWordTexture ConvertTexture(WordTexture t)
        {
            var ret = new DWordTexture(new uint[256][]);
            for (var y = 0; y < 256; y++)
            {
                ret.Pixels[y] = new uint[256];

                for (var x = 0; x < 256; x++)
                {
                    var color = t.Pixels[y][x];
                    uint val = 0;
                    if ((color & 0x8000) != 0)
                    {
                        val = (uint)(
                            ((color & 0x00007c00) >> 7) 
                            | (((color & 0x000003e0) >> 2) << 8) 
                            | ((color & 0x0000001f) << 3 << 16) 
                            | 0xff000000);
                    }
                    ret.Pixels[y][x] = val;
                }
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
}
