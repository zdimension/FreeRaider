using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeRaider.Loader
{
    public abstract class Level
    {
        public Engine Version { get; }

        protected BinaryReader reader;


        protected Level(BinaryReader br, Engine ver)
        {
            Version = ver;
            reader = br;
        }

        protected Level(BinaryReader br, Game ver)
            : this(br, Helper.GameToEngine(ver))
        {
        }

        public bool IsDemoOrUb { get; set; }

        /// <summary>
        /// Color palette
        /// </summary>
        public Palette Palette;

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

        protected void ReadMeshData()
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

                Meshes[i] = Mesh.Read(reader, Version);

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

        protected void ReadFrameMoveableData()
        {
            var numFrameData = reader.ReadUInt32();
            var frameDataPos = reader.BaseStream.Position;
            FrameData = reader.ReadUInt16Array(numFrameData);

            var numMoveables = reader.ReadUInt32();
            Moveables = new Moveable[numMoveables];
            for (uint i = 0; i < numMoveables; i++)
            {
                Moveables[i] = Moveable.Read(reader, Version);

                // Disable unused skybox polygons
                if (Version == Engine.TR3 && Moveables[i].ObjectID == 355)
                {
                    var arr = Meshes[MeshIndices[Moveables[i].StartingMesh]].ColouredRectangles;
                    Array.Resize(ref arr, 16);
                    Meshes[MeshIndices[Moveables[i].StartingMesh]].ColouredRectangles = arr;
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

        public static Level CreateLoader(string fileName)
        {
            using (var fs = new FileStream(fileName, FileMode.Open))
            {
                using (var br = new BinaryReader(fs))
                {
                    var ver = Helper.ParseVersion(br, Path.GetExtension(fileName));
                    br.BaseStream.Position = 0;
                    Level lvl = null;
                    switch(ver)
                    {
                        case Game.TR1:
                            lvl = new TR1Level(br, ver);
                            break;
                        case Game.TR1Demo:
                            case Game.TR1UnfinishedBusiness:
                            lvl = new TR1Level(br, ver);
                            lvl.IsDemoOrUb = true;
                            break;
                        case Game.TR2:
                            lvl = new TR2Level(br, ver);
                            break;
                        case Game.TR2Demo:
                            lvl = new TR2Level(br, ver);
                            lvl.IsDemoOrUb = true;
                            break;
                        case Game.TR3:
                            lvl = new TR3Level(br, ver);
                            break;
                        case Game.TR4:
                        case Game.TR4Demo:
                            lvl = new TR4Level(br, ver);
                            break;
                        case Game.TR5:
                            lvl = new TR5Level(br, ver);
                            break;
                    }
                    return lvl;
                }
            }
        }

        protected StaticMesh FindStaticMeshById(uint objectID)
        {
            return StaticMeshes.FirstOrDefault(x => x.ObjectID == objectID && MeshIndices[x.Mesh] != 0);
        }

        protected Item FindItemById(short objectID)
        {
            return Items.FirstOrDefault(x => x.ObjectID == objectID);
        }

        protected Moveable FindMoveableById(uint objectID)
        {
            return Moveables.FirstOrDefault(x => x.ObjectID == objectID);
        }

        protected static DWordTexture ConvertTexture(Palette p, ByteTexture t)
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

        protected static DWordTexture ConvertTexture(WordTexture t)
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
}
