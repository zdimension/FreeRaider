using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace UniRaider.Loader
{
    public enum TRVersion
    {
        Unknown,
        TR1,
        TR1UnfinishedBusiness,
        TR2,
        TR3,
        TR4,
        TR5
    }

    public class LevelParseException : Exception
    {
        public long Position { get; set; }

        public LevelParseException(Exception e, long pos = -1) : base("At position: " + (pos == -1 ? "Unknown" : pos.ToString()), e)
        {
            Position = pos;
        }
    }

    public class ScriptParseException : Exception
    {
        public long Position { get; set; }

        public ScriptParseException(Exception e, long pos = -1) : base("At position " + (pos == -1 ? "Unknown" : pos.ToString()) + ": " + e.Message, e)
        {
            Position = pos;
        }
    }

    public struct ByteColor
    {
        /// <summary>
        /// Red component
        /// </summary>
        public byte R { get; set; }

        /// <summary>
        /// Green component
        /// </summary>
        public byte G { get; set; }

        /// <summary>
        /// Blue component
        /// </summary>
        public byte B { get; set; }

        /// <summary>
        /// Alpha (transparency) component
        /// </summary>
        public byte A { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ByteColor"/> class.
        /// </summary>
        /// <param name="r">Red component</param>
        /// <param name="g">Green component</param>
        /// <param name="b">Blue component</param>
        public ByteColor(byte r, byte g, byte b) : this(r, g, b, 0xFF)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ByteColor"/> class.
        /// </summary>
        /// <param name="r">Red component</param>
        /// <param name="g">Green component</param>
        /// <param name="b">Blue component</param>
        /// <param name="a">Alpha (transparency) component</param>
        public ByteColor(byte r, byte g, byte b, byte a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        /// <summary>
        /// Reads a <see cref="ByteColor"/>
        /// </summary>
        /// <param name="br">The <see cref="BinaryReader"/> used to read the <see cref="ByteColor"/></param>
        /// <param name="ver">The game version</param>
        /// <returns>The <see cref="ByteColor"/></returns>
        public static ByteColor Read(BinaryReader br, TRVersion ver = TRVersion.Unknown)
        {
            return new ByteColor(
                br.ReadByte(),
                br.ReadByte(),
                br.ReadByte(),
                ver >= TRVersion.TR2
                    ? br.ReadByte()
                    : (byte) 0xFF);
        }

        /// <summary>
        /// Reads a <see cref="ByteColor"/>
        /// </summary>
        /// <param name="br">The <see cref="BinaryReader"/> used to read the <see cref="ByteColor"/></param>
        /// <param name="noAlpha">If true, won't parse Alpha value</param>
        /// <returns>The <see cref="ByteColor"/></returns>
        public static ByteColor ReadF(BinaryReader br, bool noAlpha = false)
        {
            return new ByteColor(
                (byte) (br.ReadSingle() * 255),
                (byte) (br.ReadSingle() * 255),
                (byte) (br.ReadSingle() * 255),
                (byte) (noAlpha ? 255 : br.ReadSingle() * 255));
        }

        public override string ToString()
        {
            return $"ByteColor [R={R}; G={G}; B={B}; A={A}]";
        }

        public static explicit operator ByteColor(Color c)
        {
            return new ByteColor(c.R, c.G, c.B, c.A);
        }

        public static implicit operator Color(ByteColor c)
        {
            return Color.FromArgb(c.A, c.R, c.G, c.B);
        }
    }

    public struct Vertex
    {
        /// <summary>
        /// X component
        /// </summary>
        public float x { get; set; }

        /// <summary>
        /// Y component
        /// </summary>
        public float y { get; set; }

        /// <summary>
        /// Z component
        /// </summary>
        public float z { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vertex"/> class.
        /// </summary>
        /// <param name="x">X component</param>
        /// <param name="y">Y component</param>
        /// <param name="z">Z component</param>
        public Vertex(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        /// <summary>
        /// Reads a 16-bit integer <see cref="Vertex"/>
        /// </summary>
        /// <param name="br">The <see cref="BinaryReader"/> used to read the <see cref="Vertex"/></param>
        /// <returns>A 16-bit integer <see cref="Vertex"/></returns>
        public static Vertex Read16(BinaryReader br)
        {
            return new Vertex(br.ReadInt16(), -br.ReadInt16(), -br.ReadInt16());
        }

        /// <summary>
        /// Reads a 32-bit integer <see cref="Vertex"/>
        /// </summary>
        /// <param name="br">The <see cref="BinaryReader"/> used to read the <see cref="Vertex"/></param>
        /// <returns>A 32-bit integer <see cref="Vertex"/></returns>
        public static Vertex Read32(BinaryReader br)
        {
            return new Vertex(br.ReadInt32(), -br.ReadInt32(), -br.ReadInt32());
        }

        /// <summary>
        /// Reads a 32-bit floating-point <see cref="Vertex"/>
        /// </summary>
        /// <param name="br">The <see cref="BinaryReader"/> used to read the <see cref="Vertex"/></param>
        /// <returns>A 32-bit floating-point <see cref="Vertex"/></returns>
        public static Vertex ReadF(BinaryReader br)
        {
            return new Vertex(br.ReadSingle(), -br.ReadSingle(), -br.ReadSingle());
        }

        public override string ToString()
        {
            return $"Vertex [{x} {y} {z}]";
        }

        public static readonly Vertex Zero = new Vertex(0, 0, 0);
    }

    public struct Triangle
    {
        /// <summary>
        /// Index into Vertices[]
        /// </summary>
        public ushort[] Vertices { get; set; }

        /// <summary>
        /// Object-texture index or color index
        /// </summary>
        public ushort Texture { get; set; }

        /// <summary>
        /// Transparency flag and strength of the highlight (TR4-5)<br/>
        /// bit0 if set then alpha channel = intensity<br/>
        /// bit1-7 is the strength of the highlight
        /// </summary>
        public ushort Lighting { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Triangle"/> class.
        /// </summary>
        /// <param name="vert">Triangle vertices</param>
        /// <param name="tex">Texture index</param>
        /// <param name="light">Lighting flag</param>
        public Triangle(IEnumerable<ushort> vert, ushort tex, ushort light = 0)
        {
            Vertices = vert.ToArray();
            Texture = tex;
            Lighting = light;
        }

        /// <summary>
        /// Reads a <see cref="Triangle"/>
        /// </summary>
        /// <param name="br">The <see cref="BinaryReader"/> used to read the <see cref="Triangle"/></param>
        /// <param name="ver">The game version</param>
        /// <returns>A <see cref="Triangle"/></returns>
        public static Triangle Read(BinaryReader br, TRVersion ver = TRVersion.Unknown)
        {
            var ret = new Triangle
            {
                Vertices = br.ReadUInt16Array(3),
                Texture = br.ReadUInt16()
            };
            if (ver >= TRVersion.TR4) ret.Lighting = br.ReadUInt16();
            return ret;
        }
    }

    public struct QuadFace
    {
        /// <summary>
        /// Index into Vertices[]
        /// </summary>
        public ushort[] Vertices { get; set; }

        /// <summary>
        /// Object-texture index or color index
        /// </summary>
        public ushort Texture { get; set; }

        /// <summary>
        /// Transparency flag and strength of the highlight (TR4-5)<br/>
        /// bit0 if set then alpha channel = intensity<br/>
        /// bit1-7 is the strength of the highlight
        /// </summary>
        public ushort Lighting { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="QuadFace"/> class.
        /// </summary>
        /// <param name="vert">Rectangle vertices</param>
        /// <param name="tex">Texture index</param>
        /// <param name="light">Lighting flag</param>
        public QuadFace(IEnumerable<ushort> vert, ushort tex, ushort light = 0)
        {
            Vertices = vert.ToArray();
            Texture = tex;
            Lighting = light;
        }

        /// <summary>
        /// Reads a <see cref="QuadFace"/>
        /// </summary>
        /// <param name="br">The <see cref="BinaryReader"/> used to read the <see cref="QuadFace"/></param>
        /// <param name="ver">The game version</param>
        /// <returns>A <see cref="QuadFace"/></returns>
        public static QuadFace Read(BinaryReader br, TRVersion ver = TRVersion.Unknown)
        {
            var ret = new QuadFace()
            {
                Vertices = br.ReadUInt16Array(4),
                Texture = br.ReadUInt16()
            };
            if (ver >= TRVersion.TR4) ret.Lighting = br.ReadUInt16();
            return ret;
        }
    }

    public struct ByteTexture
    {
        /// <summary>
        /// The pixels
        /// </summary>
        public byte[][] Pixels { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ByteTexture"/> class.
        /// </summary>
        /// <param name="pix">The image pixels</param>
        public ByteTexture(byte[][] pix)
        {
            Pixels = pix;
        }

        /// <summary>
        /// Reads a <see cref="ByteTexture"/>
        /// </summary>
        /// <param name="br">The <see cref="BinaryReader"/> used to read the <see cref="ByteTexture"/></param>
        /// <returns>A <see cref="ByteTexture"/></returns>
        public static ByteTexture Read(BinaryReader br)
        {
            var ret = new ByteTexture
            {
                Pixels = new byte[256][]
            };
            for (var i = 0; i < 256; i++)
            {
                ret.Pixels[i] = br.ReadBytes(256);
            }
            return ret;
        }
    }

    public struct WordTexture
    {
        /// <summary>
        /// The pixels
        /// </summary>
        public ushort[][] Pixels { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WordTexture"/> class.
        /// </summary>
        /// <param name="pix">The image pixels</param>
        public WordTexture(ushort[][] pix)
        {
            Pixels = pix;
        }

        /// <summary>
        /// Reads a <see cref="WordTexture"/>
        /// </summary>
        /// <param name="br">The <see cref="BinaryReader"/> used to read the <see cref="WordTexture"/></param>
        /// <returns>A <see cref="WordTexture"/></returns>
        public static WordTexture Read(BinaryReader br)
        {
            var ret = new WordTexture()
            {
                Pixels = new ushort[256][]
            };
            for (var i = 0; i < 256; i++)
            {
                ret.Pixels[i] = br.ReadUInt16Array(256);
            }
            return ret;
        }
    }

    public struct DWordTexture
    {
        /// <summary>
        /// The pixels
        /// </summary>
        public uint[][] Pixels { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DWordTexture"/> class.
        /// </summary>
        /// <param name="pix">The image pixels</param>
        public DWordTexture(uint[][] pix)
        {
            Pixels = pix;
        }

        /// <summary>
        /// Reads a <see cref="DWordTexture"/>
        /// </summary>
        /// <param name="br">The <see cref="BinaryReader"/> used to read the <see cref="DWordTexture"/></param>
        /// <returns>A <see cref="DWordTexture"/></returns>
        public static DWordTexture Read(BinaryReader br)
        {
            var ret = new DWordTexture()
            {
                Pixels = new uint[256][]
            };
            for (var i = 0; i < 256; i++)
            {
                for (var j = 0; j < 256; j++)
                {
                    var tmp = br.ReadUInt32();
                    ret.Pixels[i][j] = (tmp & 0xff00ff00) | ((tmp & 0x00ff0000) >> 16) | ((tmp & 0x000000ff) << 16);
                }
            }
            return ret;
        }
    }

    public struct Portal
    {
        /// <summary>
        /// The <see cref="Room"/> this <see cref="Portal"/> leads to
        /// </summary>
        public ushort AdjoiningRoom { get; set; }

        /// <summary>
        /// Which way the <see cref="Portal"/> faces
        /// </summary>
        public Vertex Normal { get; set; }

        /// <summary>
        /// The 4 corners of this <see cref="Portal"/>
        /// </summary>
        public Vertex[] Vertices { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Portal"/> class.
        /// </summary>
        /// <param name="adjRoom">The <see cref="Room"/> this <see cref="Portal"/> leads to</param>
        /// <param name="norm">Which way the <see cref="Portal"/> faces</param>
        /// <param name="vert">The 4 corners of the <see cref="Portal"/></param>
        public Portal(ushort adjRoom, Vertex norm, IEnumerable<Vertex> vert)
        {
            AdjoiningRoom = adjRoom;
            Normal = norm;
            Vertices = vert.ToArray();
        }

        /// <summary>
        /// Reads a <see cref="Portal"/>
        /// </summary>
        /// <param name="br">The <see cref="BinaryReader"/> used to read the <see cref="Portal"/></param>
        /// <returns>A <see cref="Portal"/></returns>
        public static Portal Read(BinaryReader br)
        {
            return new Portal(br.ReadUInt16(), Vertex.Read16(br), br.ReadArray(4, () => Vertex.Read16(br)));
        }
    }

    public struct Sector
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Sector"/> class.
        /// </summary>
        /// <param name="fdIndex">Index into FloorData[]</param>
        /// <param name="boxIndex">Index into Boxes[]/Zones[] (-1 if none)</param>
        /// <param name="roomBelow">The number of the room below this one (-1 or 255 if none)</param>
        /// <param name="floor">Absolute height of the floor (multiply by 256 for world coordinates)</param>
        /// <param name="roomAbove">The number of the room above this one (-1 or 255 if none)</param>
        /// <param name="ceiling">Absolute height of the ceiling (multiply by 256 for world coordinates)</param>
        public Sector(ushort fdIndex, ushort boxIndex, byte roomBelow, sbyte floor, byte roomAbove, sbyte ceiling)
        {
            FD_Index = fdIndex;
            Box_Index = boxIndex;
            RoomBelow = roomBelow;
            Floor = floor;
            RoomAbove = roomAbove;
            Ceiling = ceiling;
        }

        /// <summary>
        /// Index into FloorData[]
        /// </summary>
        public ushort FD_Index { get; set; }

        /// <summary>
        /// Index into Boxes[]/Zones[] (-1 if none)
        /// </summary>
        public ushort Box_Index { get; set; }

        /// <summary>
        /// The number of the room below this one (-1 or 255 if none)
        /// </summary>
        public byte RoomBelow { get; set; }

        /// <summary>
        /// Absolute height of the floor (multiply by 256 for world coordinates)
        /// </summary>
        public sbyte Floor { get; set; }

        /// <summary>
        /// The number of the room above this one (-1 or 255 if none)
        /// </summary>
        public byte RoomAbove { get; set; }

        /// <summary>
        /// Absolute height of the ceiling (multiply by 256 for world coordinates)
        /// </summary>
        public sbyte Ceiling { get; set; }

        /// <summary>
        /// Reads a <see cref="Sector"/>
        /// </summary>
        /// <param name="br">The <see cref="BinaryReader"/> used to read the <see cref="Sector"/></param>
        /// <returns>A <see cref="Sector"/></returns>
        public static Sector Read(BinaryReader br)
        {
            return new Sector(br.ReadUInt16(), br.ReadUInt16(), br.ReadByte(), br.ReadSByte(), br.ReadByte(),
                br.ReadSByte());
        }
    }

    public enum LightType : byte
    {
        Null,
        Point,
        Spotlight,
        Sun,
        Shadow
    }

    public struct Light
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Light"/> class.
        /// </summary>
        public Light(byte lightType, Vertex position, ByteColor color, float intensity = 0.0f, ushort intensity1 = 0,
            ushort intensity2 = 0, uint fade1 = 0, uint fade2 = 0, byte unknown = 255, float hotspot = 0.0f, float falloff= 0.0f, float length= 0.0f,
            float cutoff = 0.0f, Vertex direction = default(Vertex), Vertex position2 = default(Vertex), Vertex direction2 = default(Vertex), float radInput = 0.0f, float radOutput = 0.0f, float range = 0.0f)
        {
            this.lightType = lightType;
            Position = position;
            Color = color;
            Intensity = intensity;
            Intensity1 = intensity1;
            Intensity2 = intensity2;
            Fade1 = fade1;
            Fade2 = fade2;
            Unknown = unknown;
            Hotspot = hotspot;
            Falloff = falloff;
            Length = length;
            Cutoff = cutoff;
            Direction = direction;
            Position2 = position2;
            Direction2 = direction2;
            RadianInput = radInput;
            RadianOutput = radOutput;
            Range = range;
        }

        /// <summary>
        /// Position of the light (in world coordinates)
        /// </summary>
        public Vertex Position { get; set; }

        /// <summary>
        /// Color of the light
        /// </summary>
        public ByteColor Color { get; set; }

        /// <summary>
        /// The calculated intensity
        /// </summary>
        public float Intensity { get; set; }

        /// <summary>
        /// Light intensity
        /// </summary>
        public ushort Intensity1 { get; set; }

        /// <summary>
        /// Almost always equal to <see cref="Intensity1"/> [absent from TR1 data files]
        /// </summary>
        public ushort Intensity2 { get; set; }

        /// <summary>
        /// Falloff value 1
        /// </summary>
        public uint Fade1 { get; set; }

        /// <summary>
        /// Falloff value 2 [absent from TR1 data files]
        /// </summary>
        public uint Fade2 { get; set; }

        private byte lightType;

        /// <summary>
        /// Same as Direct3D (i.e. 2 is for spotlight)
        /// </summary>
        public LightType LightType
        {
            get { return (LightType) lightType; }
            set { lightType = (byte) value; }
        }

        /// <summary>
        /// No known effect, seems to always be 255
        /// </summary>
        public byte Unknown { get; set; }

        /// <summary>
        /// Used for Spot, Light and Shadow; Hotspot angle cosine or TR units
        /// </summary>
        public float Hotspot { get; set; }

        /// <summary>
        /// Used for Spot, Light and Shadow; Falloff angle cosine or TR units
        /// </summary>
        public float Falloff { get; set; }

        /// <summary>
        /// Length
        /// </summary>
        public float Length { get; set; }

        /// <summary>
        /// Cutoff
        /// </summary>
        public float Cutoff { get; set; }

        /// <summary>
        /// Used by spot light
        /// </summary>
        public float RadianInput { get; set; }

        /// <summary>
        /// Used by spot light
        /// </summary>
        public float RadianOutput { get; set; }

        /// <summary>
        /// Used by spot light
        /// </summary>
        public float Range { get; set; }

        /// <summary>
        /// Direction
        /// </summary>
        public Vertex Direction { get; set; }

        /// <summary>
        /// Position of the light (in world coordinates)
        /// </summary>
        public Vertex Position2 { get; set; }

        /// <summary>
        /// Direction
        /// </summary>
        public Vertex Direction2 { get; set; }

        public static Light Read(BinaryReader br, TRVersion ver = TRVersion.Unknown)
        {
            var ret = new Light();
            var t1 = ver < TRVersion.TR2;
            ret.Position = ver == TRVersion.TR5 ? Vertex.ReadF(br) : Vertex.Read32(br);
            if (ver < TRVersion.TR3)
            {
                ret.Intensity1 = t1
                    ? (ushort) ((8191 - br.ReadUInt16()) << 2)
                    : br.ReadUInt16();
                ret.Intensity2 = t1
                    ? ret.Intensity1
                    : br.ReadUInt16();
                ret.Fade1 = br.ReadUInt32();
                ret.Fade2 = t1
                    ? ret.Fade2
                    : br.ReadUInt32();

                ret.Intensity = Math.Min(ret.Intensity1 / 4096.0f, 1);

                ret.Falloff = ret.Fade1;
                ret.Hotspot = (float) ret.Fade1 / 2;

                ret.LightType = LightType.Point;

                ret.Color = new ByteColor(255, 255, 255);
            }
            else if (ver > TRVersion.TR3)
            {
                if(ver == TRVersion.TR4)
                {
                    ret.Color = ByteColor.Read(br, TRVersion.TR1);
                    ret.lightType = br.ReadByte();
                    ret.Unknown = br.ReadByte();
                    ret.Intensity1 = br.ReadByte();
                    ret.Intensity = (float) ret.Intensity1 / 32;
                    ret.Hotspot = br.ReadSingle();
                    ret.Falloff = br.ReadSingle();
                    ret.Length = br.ReadSingle();
                    ret.Cutoff = br.ReadSingle();
                    ret.Direction = Vertex.ReadF(br);
                }
                else
                {
                    ret.Color = ByteColor.ReadF(br, true);
                    if(br.ReadUInt32() != 0xCDCDCDCD)
                        throw new ArgumentException("Wrong value, should be 0xCDCDCDCD", "separator1");
                    ret.Intensity = 1;
                    ret.Hotspot = br.ReadSingle();
                    ret.Falloff = br.ReadSingle();
                    ret.RadianInput = br.ReadSingle();
                    ret.RadianOutput = br.ReadSingle();
                    ret.Range = br.ReadSingle();
                    ret.Direction = Vertex.ReadF(br);
                    ret.Position2 = Vertex.Read32(br);
                    ret.Direction2 = Vertex.Read32(br);
                    ret.lightType = br.ReadByte();

                    if (br.ReadByte() != 0xCD)
                        throw new ArgumentException("Wrong value, should be 0xCD", "separator2");

                    if (br.ReadByte() != 0xCD)
                        throw new ArgumentException("Wrong value, should be 0xCD", "separator3");

                    if (br.ReadByte() != 0xCD)
                        throw new ArgumentException("Wrong value, should be 0xCD", "separator4");
                }
            }
            else
            {
                ret.Color = ByteColor.Read(br, ver);

                ret.Fade1 = br.ReadUInt32();
                ret.Fade2 = br.ReadUInt32();

                ret.Intensity = 1;

                ret.Falloff = ret.Fade1;
                ret.Hotspot = (float) ret.Fade1 / 2;

                ret.LightType = LightType.Point;
            }

            return ret;
        }
    }

    public struct Sprite
    {
        /// <summary>
        /// Offset into vertex list
        /// </summary>
        public short Vertex { get; set; }

        /// <summary>
        /// Offset into sprite list
        /// </summary>
        public short Texture { get; set; }
    }

    public struct Room
    {
        Error here 
    }
}
