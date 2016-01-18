using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Policy;
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

    public struct FloatColor
    {
        /// <summary>
        /// Red component
        /// </summary>
        public float R { get; set; }

        /// <summary>
        /// Green component
        /// </summary>
        public float G { get; set; }

        /// <summary>
        /// Blue component
        /// </summary>
        public float B { get; set; }

        /// <summary>
        /// Alpha (transparency) component
        /// </summary>
        public float A { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FloatColor"/> class.
        /// </summary>
        /// <param name="r">Red component</param>
        /// <param name="g">Green component</param>
        /// <param name="b">Blue component</param>
        public FloatColor(float r, float g, float b) : this(r, g, b, 1.0f)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ByteColor"/> class.
        /// </summary>
        /// <param name="r">Red component</param>
        /// <param name="g">Green component</param>
        /// <param name="b">Blue component</param>
        /// <param name="a">Alpha (transparency) component</param>
        public FloatColor(float r, float g, float b, float a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        /// <summary>
        /// Reads a <see cref="FloatColor"/>
        /// </summary>
        /// <param name="br">The <see cref="BinaryReader"/> used to read the <see cref="FloatColor"/></param>
        /// <returns>The <see cref="FloatColor"/></returns>
        public static FloatColor Read(BinaryReader br, bool withAlpha = true)
        {
            return new FloatColor(
                br.ReadByte() / 255.0f,
                br.ReadByte() / 255.0f,
                br.ReadByte() / 255.0f,
                withAlpha ? br.ReadByte() / 255.0f : 1.0f
                );
        }

        /// <summary>
        /// Reads a <see cref="FloatColor"/>
        /// </summary>
        /// <param name="br">The <see cref="BinaryReader"/> used to read the <see cref="FloatColor"/></param>
        /// <returns>The <see cref="FloatColor"/></returns>
        public static FloatColor ReadF(BinaryReader br, bool withAlpha = true)
        {
            return new FloatColor(
                br.ReadSingle(),
                br.ReadSingle(),
                br.ReadSingle(),
                withAlpha ? br.ReadSingle() : 1.0f
                );
        }

        public override string ToString()
        {
            return $"FloatColor [R={R}; G={G}; B={B}; A={A}]";
        }
    }

    public struct Vertex
    {
        /// <summary>
        /// X component
        /// </summary>
        public float X { get; set; }

        /// <summary>
        /// Y component
        /// </summary>
        public float Y { get; set; }

        /// <summary>
        /// Z component
        /// </summary>
        public float Z { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vertex"/> class.
        /// </summary>
        /// <param name="x">X component</param>
        /// <param name="y">Y component</param>
        /// <param name="z">Z component</param>
        public Vertex(float x, float y, float z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
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
        public static Vertex Read32(BinaryReader br, bool negate = true)
        {
            if (!negate) return new Vertex(br.ReadInt32(), br.ReadInt32(), br.ReadInt32());
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
            return $"Vertex [{X} {Y} {Z}]";
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

        /// <summary>
        /// Reads a <see cref="Light"/>
        /// </summary>
        /// <param name="br">The <see cref="BinaryReader"/> used to read the <see cref="Light"/></param>
        /// <param name="ver">The game version</param>
        /// <returns>A <see cref="Light"/></returns>
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
                    var separator1 = br.ReadUInt32();
                    if(separator1 != 0xCDCDCDCD)
                        throw new ArgumentException("Light.Read[TR5]: Found 0x" + separator1.ToString("X8") + ", Expected 0xCDCDCDCD", "separator1");
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

                    var sep = br.ReadByte();
                    if (sep != 0xCD)
                        throw new ArgumentException("Light.Read[TR5]: Found 0x" + sep.ToString("X2") + ", Expected 0xCD", "separator2");
                                                                                            
                    if ((sep = br.ReadByte()) != 0xCD)                                      
                        throw new ArgumentException("Light.Read[TR5]: Found 0x" + sep.ToString("X2") + ", Expected 0xCD", "separator3");
                                                                                            
                    if ((sep = br.ReadByte()) != 0xCD)                                      
                        throw new ArgumentException("Light.Read[TR5]: Found 0x" + sep.ToString("X2") + ", Expected 0xCD", "separator4");
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
        /// Initializes a new instance of the <see cref="Sprite"/> class.
        /// </summary>
        /// <param name="vertex">Offset into <see cref="Vertex"/> list</param>
        /// <param name="texture">Offset into sprite texture list</param>
        public Sprite(short vertex, short texture)
        {
            Vertex = vertex;
            Texture = texture;
        }

        /// <summary>
        /// Offset into <see cref="Vertex"/> list
        /// </summary>
        public short Vertex { get; set; }

        /// <summary>
        /// Offset into sprite texture list TODO
        /// </summary>
        public short Texture { get; set; }

        /// <summary>
        /// Reads a <see cref="Sprite"/>
        /// </summary>
        /// <param name="br">The <see cref="BinaryReader"/> used to read the <see cref="Sprite"/></param>
        /// <returns>A <see cref="Sprite"/></returns>
        public static Sprite Read(BinaryReader br)
        {
            return new Sprite(br.ReadInt16(), br.ReadInt16());
        }
    }

    /// <summary>
    /// Room layer (TR5)
    /// </summary>
    public struct Layer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Layer"/> class.
        /// </summary>
        public Layer(ushort numVertices, ushort unknownL1, ushort unknownL2, ushort numRectangles, ushort numTriangles, ushort unknownL3, BoundingBox boundingBox, uint unknownL4, uint unknownL5, uint unknownL6)
        {
            NumVertices = numVertices;
            Unknown_l1 = unknownL1;
            Unknown_l2 = unknownL2;
            NumRectangles = numRectangles;
            NumTriangles = numTriangles;
            Unknown_l3 = unknownL3;
            BoundingBox = boundingBox;
            Unknown_l4 = unknownL4;
            Unknown_l5 = unknownL5;
            Unknown_l6 = unknownL6;
        }

        /// <summary>
        /// Number of vertices in this layer
        /// </summary>
        public ushort NumVertices { get; set; }
        public ushort Unknown_l1 { get; set; }
        public ushort Unknown_l2 { get; set; }
        /// <summary>
        /// Number of rectangles in this layer
        /// </summary>
        public ushort NumRectangles { get; set; }
        /// <summary>
        /// Number of triangles in this layer
        /// </summary>
        public ushort NumTriangles { get; set; }
        public ushort Unknown_l3 { get; set; }

        /// <summary>
        /// Bounding box of the layer
        /// </summary>
        public BoundingBox BoundingBox { get; set; }

        public uint Unknown_l4 { get; set; }
        public uint Unknown_l5 { get; set; }
        public uint Unknown_l6 { get; set; }

        /// <summary>
        /// Reads a <see cref="Layer"/>
        /// </summary>
        /// <param name="br">The <see cref="BinaryReader"/> used to read the <see cref="Layer"/></param>
        /// <returns>A <see cref="Layer"/></returns>
        public static Layer Read(BinaryReader br)
        {
            var ret = new Layer();
            ret.NumVertices = br.ReadUInt16();
            ret.Unknown_l1 = br.ReadUInt16();
            ret.Unknown_l2 = br.ReadUInt16();
            ret.NumRectangles = br.ReadUInt16();
            ret.NumTriangles = br.ReadUInt16();
            ret.Unknown_l3 = br.ReadUInt16();
            var filler = br.ReadUInt16();
            if (filler != 0)
                throw new ArgumentException("Layer.Read: Found " + filler + ", Expected 0", "filler");
            var filler2 = br.ReadUInt16();
            if(filler2 != 0)
                throw new ArgumentException("Layer.Read: Found " + filler2 + ", Expected 0", "filler2");
            ret.BoundingBox = BoundingBox.Read(br);
            var filler3 = br.ReadUInt32();
            if(filler3 != 0)
                throw new ArgumentException("Layer.Read: Found " + filler3.ToString("X8") + ", Expected 0", "filler3");
            ret.Unknown_l4 = br.ReadUInt32();
            ret.Unknown_l5 = br.ReadUInt32();
            ret.Unknown_l6 = br.ReadUInt32();
            return ret;
        }
    }

    public struct BoundingBox
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BoundingBox"/> class.
        /// </summary>
        public BoundingBox(Vertex point1, Vertex point2)
        {
            Point1 = point1;
            Point2 = point2;
        }

        /// <summary>
        /// First point
        /// </summary>
        public Vertex Point1 { get; set; }

        /// <summary>
        /// Second point
        /// </summary>
        public Vertex Point2 { get; set; }

        /// <summary>
        /// Reads a <see cref="BoundingBox"/>
        /// </summary>
        /// <param name="br">The <see cref="BinaryReader"/> used to read the <see cref="BoundingBox"/></param>
        /// <returns>A <see cref="BoundingBox"/></returns>
        public static BoundingBox Read(BinaryReader br)
        {
            return new BoundingBox(Vertex.ReadF(br), Vertex.ReadF(br));
        }
    }

    [Flags]
    public enum SpecialRenderingEffects : ushort
    {
        Normal = 0x0010,
        WaterSurfaceMovement = 0x2000,
        UnderWaterLightingModulation = 0x4000,
        WaterSurface = 0x8000
    }

    public struct RoomVertex
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RoomVertex"/> class.
        /// </summary>
        public RoomVertex(Vertex vertex, short lighting1, SpecialRenderingEffects attributes = SpecialRenderingEffects.Normal, short lighting2 = 0, Vertex normal = default(Vertex), FloatColor color = default(FloatColor))
        {
            Vertex = vertex;
            Lighting1 = lighting1;
            Attributes = attributes;
            Lighting2 = lighting2;
            Normal = normal;
            Color = color;
        }

        /// <summary>
        /// Where this <see cref="RoomVertex"/> lies (relative to tr2_room_info::x/z) TODO
        /// </summary>
        public Vertex Vertex { get; set; }

        public short Lighting1 { get; set; }

        /// <summary>
        /// A set of flags for special rendering effects [absent from TR1 data files]
        /// </summary>
        public SpecialRenderingEffects Attributes { get; set; }

        /// <summary>
        /// Almost always equal to <see cref="Lighting1"/> [absent from TR1 data files]
        /// </summary>
        public short Lighting2 { get; set; }

        // TR5 only
        public Vertex Normal { get; set; }
        public FloatColor Color { get; set; }

        /// <summary>
        /// Reads a <see cref="RoomVertex"/>
        /// </summary>
        /// <param name="br">The <see cref="BinaryReader"/> used to read the <see cref="RoomVertex"/></param>
        /// <param name="ver">The game version</param>
        /// <returns>A <see cref="RoomVertex"/></returns>
        public static RoomVertex Read(BinaryReader br, TRVersion ver = TRVersion.Unknown)
        {
            var ret = new RoomVertex();
            if (ver == TRVersion.TR5)
            {
                ret.Vertex = Vertex.ReadF(br);
                ret.Normal = Vertex.ReadF(br);
                ret.Color = FloatColor.Read(br);
            }
            else
            {
                ret.Vertex = Vertex.Read16(br);
                ret.Lighting1 = ver >= TRVersion.TR3 ? br.ReadInt16() : (short) ((8191 - br.ReadInt16()) << 2);
                if (ver >= TRVersion.TR2)
                {
                    ret.Attributes = (SpecialRenderingEffects) br.ReadUInt16();
                    ret.Lighting2 = ver >= TRVersion.TR3 ? br.ReadInt16() : (short)((8191 - br.ReadInt16()) << 2);  
                }
                else
                {
                    ret.Lighting2 = ret.Lighting1;
                }
                ret.Normal = Vertex.Zero;
                if (ver < TRVersion.TR2)
                    ret.Color = new FloatColor(
                        ret.Lighting1 / 32768.0f,
                        ret.Lighting1 / 32768.0f,
                        ret.Lighting1 / 32768.0f);
                else if (ver == TRVersion.TR2)
                    ret.Color = new FloatColor(
                        ret.Lighting2 / 32768.0f, 
                        ret.Lighting2 / 32768.0f,
                        ret.Lighting2 / 32768.0f);
                else if (ver == TRVersion.TR3)
                    ret.Color = new FloatColor(
                        ((ret.Lighting2 & 0x7C00) >> 10) / 62.0f,
                        ((ret.Lighting2 & 0x03E0) >> 5) / 62.0f, 
                        (ret.Lighting2 & 0x001F) / 62.0f);
                else if (ver == TRVersion.TR4)
                    ret.Color = new FloatColor(
                        ((ret.Lighting2 & 0x7C00) >> 10) / 31.0f,
                        ((ret.Lighting2 & 0x03E0) >> 5) / 31.0f,
                        (ret.Lighting2 & 0x001F) / 31.0f);
            }
            return ret;
        }
    }

    public struct RoomStaticMesh
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RoomStaticMesh"/> class.
        /// </summary>
        public RoomStaticMesh(Vertex position, float rotation, short intensity1, short intensity2, ushort objectId, FloatColor tint)
        {
            Position = position;
            Rotation = rotation;
            Intensity1 = intensity1;
            Intensity2 = intensity2;
            ObjectID = objectId;
            Tint = tint;
        }

        /// <summary>
        /// Position in world coordinates
        /// </summary>
        public Vertex Position { get; set; }

        /// <summary>
        /// High two bits (0xC000) indicate steps of 90 degrees (e.g. (Rotation >> 14) * 90)
        /// </summary>
        public float Rotation { get; set; }

        /// <summary>
        /// Constant lighting; -1 means use mesh lighting
        /// </summary>
        public short Intensity1 { get; set; }

        /// <summary>
        /// Like <see cref="Intensity1"/>, and almost always the same value [absent from TR1 data files]
        /// </summary>
        public short Intensity2 { get; set; }

        /// <summary>
        /// Determines which StaticMesh item to draw TODO
        /// </summary>
        public ushort ObjectID { get; set; }

        /// <summary>
        /// Tint, extracted from intensity
        /// </summary>
        public FloatColor Tint { get; set; }

        /// <summary>
        /// Reads a <see cref="RoomStaticMesh"/>
        /// </summary>
        /// <param name="br">The <see cref="BinaryReader"/> used to read the <see cref="RoomStaticMesh"/></param>
        /// <param name="ver">The game version</param>
        /// <returns>A <see cref="RoomStaticMesh"/></returns>
        public static RoomStaticMesh Parse(BinaryReader br, TRVersion ver = TRVersion.Unknown)
        {
            var rsm = new RoomStaticMesh();
            rsm.Position = Vertex.Read32(br);
            rsm.Rotation = br.ReadUInt16() / 16384.0f * -90;
            rsm.Intensity1 = br.ReadInt16();
            rsm.Intensity2 = ver < TRVersion.TR2 ? rsm.Intensity1 : br.ReadInt16();
            rsm.ObjectID = br.ReadUInt16();

            if (rsm.Intensity1 >= 0)
                rsm.Intensity1 = (short)((8191 - rsm.Intensity1) << 2);

            if (rsm.Intensity2 >= 0)
                rsm.Intensity2 = (short)((8191 - rsm.Intensity2) << 2);

            if(ver < TRVersion.TR3)
            {
                var c = rsm.Intensity2 / 16384.0f;
                rsm.Tint = new FloatColor(c, c, c);
            }
            else
            {
                rsm.Tint = new FloatColor(
                    (rsm.Intensity1 & 0x001F) / 62.0f,
                    ((rsm.Intensity1 & 0x03E0) >> 5) / 62.0f,
                    ((rsm.Intensity1 & 0x7C00) >> 10) / 62.0f);
            }

            return rsm;
        }
    }

    [Flags]
    public enum RoomFlags : ushort
    {
        None = 0,
        FilledWithWater = 0x0001,
        WindBlowPonytail = 0x0020,
        Quicksand = 0x0080
        // TR1 has only the water flag and the extra unknown flag 0x0100.
        // TR3 most likely has flags for "is raining", "is snowing", "water is cold", and "is filled by quicksand" (0x0080), among others.
    }

    public enum ReverbInfo : byte
    {
        Outside = 0,
        SmallRoom = 1,
        MediumRoom = 2,
        LargeRoom = 3,
        Pipe = 4
    }

    public struct Room
    {
        /// <summary>
        /// Offset of room (world coordinates)
        /// </summary>
        public Vertex Offset { get; set; }

        /// <summary>
        /// Lowest point in the room
        /// </summary>
        public float Y_Bottom { get; set; }

        /// <summary>
        /// Highest point in the room
        /// </summary>
        public float Y_Top { get; set; }

        /// <summary>
        /// List of layers (TR5)
        /// </summary>
        public Layer[] Layers { get; set; }

        /// <summary>
        /// List of vertices (relative coordinates)
        /// </summary>
        public RoomVertex[] Vertices { get; set; }

        /// <summary>
        /// List of textured rectangles
        /// </summary>
        public QuadFace[] Rectangles { get; set; }

        /// <summary>
        /// List of textured triangles
        /// </summary>
        public Triangle[] Triangles { get; set; }

        /// <summary>
        /// List of sprites
        /// </summary>
        public Sprite[] Sprites { get; set; }

        /// <summary>
        /// List of visibility portals
        /// </summary>
        public Portal[] Portals { get; set; }

        /// <summary>
        /// "Width" of sector list
        /// </summary>
        public ushort Num_Z_Sectors { get; set; }

        /// <summary>
        /// "Height" of sector list
        /// </summary>
        public ushort Num_X_Sectors { get; set; }

        /// <summary>
        /// List of sectors [<see cref="Num_Z_Sectors"/> * <see cref="Num_X_Sectors"/>]
        /// </summary>
        public Sector[] Sectors { get; set; }

        /// <summary>
        /// This and the next one only affect externally-lit objects
        /// </summary>
        public short Intensity1 { get; set; }

        /// <summary>
        /// Almost always the same value as <see cref="Intensity1"/> [absent from TR1 data files]
        /// </summary>
        public short Intensity2 { get; set; }

        /// <summary>
        /// 0 is normal; 1 is flickering (?); 2 and 3 are uncertain [present only in TR2]
        /// </summary>
        public short LightMode { get; set; }

        /// <summary>
        /// List of point lights
        /// </summary>
        public Light[] Lights { get; set; }

        /// <summary>
        /// List of static meshes
        /// </summary>
        public RoomStaticMesh[] StaticMeshes { get; set; }

        /// <summary>
        /// ID of the room that this room can alternate with
        /// </summary>
        public short AlternateRoom { get; set; }

        /// <summary>
        /// ID of the group which is used to switch alternate rooms
        /// </summary>
        public sbyte AlternateGroup { get; set; }

        /// <summary>
        /// Flags
        /// </summary>
        public ushort Flags { get; set; }

        /// <summary>
        /// Water scheme is used with various room options, for example, R and M room flags in TRLE. Also, it specifies lighting scheme, when 0x4000 vertex attribute is set.
        /// </summary>
        public byte WaterScheme { get; set; }

        /// <summary>
        /// Used in TR3-5 and specifies reverb type
        /// </summary>
        public ReverbInfo ReverbInfo { get; set; }

        /// <summary>
        /// Present in TR5 only
        /// </summary>
        public FloatColor LightColor { get; set; }

        public float Room_X { get; set; }
        public float Room_Z { get; set; }
        public float Room_Y_Bottom { get; set; }
        public float Room_Y_Top { get; set; }

        public uint Unknown_R1 { get; set; }
        public uint Unknown_R2 { get; set; }
        public uint Unknown_R3 { get; set; }
        public ushort Unknown_R4a { get; set; }
        public ushort Unknown_R4b { get; set; }
        public uint Unknown_R5 { get; set; }
        public uint Unknown_R6 { get; set; }

        /// <summary>
        /// Reads a <see cref="Room"/>
        /// </summary>
        /// <param name="br">The <see cref="BinaryReader"/> used to read the <see cref="Room"/></param>
        /// <param name="ver">The game version</param>
        /// <returns>A <see cref="Room"/></returns>
        [SuppressMessage("ReSharper", "NotResolvedInText")]
        public static Room Read(BinaryReader br, TRVersion ver = TRVersion.Unknown)
        {
            var r = new Room();

            if (ver == TRVersion.TR5)
            {
                if (br.ReadUInt32() != 0x414C4558)
                    throw new FormatException("Room.Read: TR5 'XELA' header not found");

                var roomDataSize = br.ReadUInt32();
                var pos = br.BaseStream.Position;
                var endPos = pos + roomDataSize;

                r.Intensity1 = 32767;
                r.Intensity2 = 32767;
                r.LightMode = 0;

                var separator1 = br.ReadUInt32();
                if (separator1 != 0xCDCDCDCD)
                    throw new ArgumentException(
                        "Room.Read[TR5]: Found " + separator1.ToString("X8") + ", Expected 0xCDCDCDCD", "separator1");

                var portalOffset = br.ReadInt32();
                var sectorDataOffset = br.ReadUInt32();

                var separator2 = br.ReadUInt32();
                if (separator2 != 0 && separator2 != 0xCDCDCDCD)
                    throw new ArgumentException(
                        "Room.Read[TR5]: Found " + separator2.ToString("X8") + ", Expected 0xCDCDCDCD", "separator2");

                var staticMeshesOffset = br.ReadUInt32();

                r.Offset = new Vertex(br.ReadInt32(), br.ReadUInt32(), -br.ReadInt32());
                r.Y_Bottom = -br.ReadInt32();
                r.Y_Top = -br.ReadInt32();

                r.Num_Z_Sectors = br.ReadUInt16();
                r.Num_X_Sectors = br.ReadUInt16();

                var lc = FloatColor.Read(br);
                r.LightColor = new FloatColor(lc.R, lc.G, lc.B, 1.0f);

                var numLights = br.ReadUInt16();
                if (numLights > 512)
                    throw new ArgumentOutOfRangeException("numLights", numLights, "Room.Read[TR5]: numLights > 512");

                var numStaticMeshes = br.ReadUInt16();
                if (numStaticMeshes > 512)
                    throw new ArgumentOutOfRangeException("numStaticMeshes", numStaticMeshes,
                        "Room.Read[TR5]: numStaticMeshes > 512");

                r.ReverbInfo = (ReverbInfo) br.ReadByte();
                r.AlternateGroup = br.ReadSByte();
                r.WaterScheme = (byte) br.ReadUInt16();

                var filler1 = br.ReadUInt32();
                if (filler1 != 0x00007FFF)
                    throw new ArgumentException("Room.Read[TR5]: Found " + filler1.ToString("X8") + ", Expected 0x00007FFF",
                        "filler1");

                var filler2 = br.ReadUInt32();
                if (filler2 != 0x00007FFF)
                    throw new ArgumentException("Room.Read[TR5]: Found " + filler2.ToString("X8") + ", Expected 0x00007FFF",
                        "filler2");

                var separator4 = br.ReadUInt32();
                if (separator4 != 0xCDCDCDCD)
                    throw new ArgumentException(
                        "Room.Read[TR5]: Found " + separator4.ToString("X8") + ", Expected 0xCDCDCDCD", "separator4");

                var separator5 = br.ReadUInt32();
                if (separator5 != 0xCDCDCDCD)
                    throw new ArgumentException(
                        "Room.Read[TR5]: Found " + separator5.ToString("X8") + ", Expected 0xCDCDCDCD", "separator5");

                var separator6 = br.ReadUInt32();
                if (separator6 != 0xCDCDCDCD)
                    throw new ArgumentException(
                        "Room.Read[TR5]: Found " + separator6.ToString("X8") + ", Expected 0xCDCDCDCD", "separator6");

                r.AlternateRoom = br.ReadInt16();

                r.Flags = br.ReadUInt16();

                r.Unknown_R1 = br.ReadUInt32();
                r.Unknown_R2 = br.ReadUInt32();
                r.Unknown_R3 = br.ReadUInt32();

                var separator7 = br.ReadUInt32();
                if(separator7 != 0 && separator7 != 0xCDCDCDCD)
                    throw new ArgumentException(
                        "Room.Read[TR5]: Found " + separator7.ToString("X8") + ", Expected 0xCDCDCDCD", "separator7");

                r.Unknown_R4a = br.ReadUInt16();
                r.Unknown_R4b = br.ReadUInt16();

                r.Room_X = br.ReadSingle();
                r.Unknown_R5 = br.ReadUInt32();
                r.Room_Z = -br.ReadSingle();

                var separator8 = br.ReadUInt32();
                if (separator8 != 0xCDCDCDCD)
                    throw new ArgumentException(
                        "Room.Read[TR5]: Found " + separator8.ToString("X8") + ", Expected 0xCDCDCDCD", "separator8");

                var separator9 = br.ReadUInt32();
                if (separator9 != 0xCDCDCDCD)
                    throw new ArgumentException(
                        "Room.Read[TR5]: Found " + separator9.ToString("X8") + ", Expected 0xCDCDCDCD", "separator9");

                var separator10 = br.ReadUInt32();
                if (separator10 != 0xCDCDCDCD)
                    throw new ArgumentException(
                        "Room.Read[TR5]: Found " + separator10.ToString("X8") + ", Expected 0xCDCDCDCD", "separator10");

                var separator11 = br.ReadUInt32();
                if (separator11 != 0xCDCDCDCD)
                    throw new ArgumentException(
                        "Room.Read[TR5]: Found " + separator11.ToString("X8") + ", Expected 0xCDCDCDCD", "separator11");

                var separator12 = br.ReadUInt32();
                if (separator12 != 0 && separator12 != 0xCDCDCDCD)
                    throw new ArgumentException(
                        "Room.Read[TR5]: Found " + separator12.ToString("X8") + ", Expected 0xCDCDCDCD", "separator12");

                var separator13 = br.ReadUInt32();
                if (separator13 != 0xCDCDCDCD)
                    throw new ArgumentException(
                        "Room.Read[TR5]: Found " + separator13.ToString("X8") + ", Expected 0xCDCDCDCD", "separator13");

                var numTriangles = br.ReadUInt32();
                if (numTriangles == 0xCDCDCDCD) numTriangles = 0;
                if(numTriangles > 512)
                    throw new ArgumentOutOfRangeException("numTriangles", numTriangles,
                        "Room.Read[TR5]: numTriangles > 512");

                var numRectangles = br.ReadUInt32();
                if (numRectangles == 0xCDCDCDCD) numRectangles = 0;
                if (numRectangles > 1024)
                    throw new ArgumentOutOfRangeException("numRectangles", numRectangles,
                        "Room.Read[TR5]: numRectangles > 1024");

                var separator14 = br.ReadUInt32();
                if (separator14 != 0)
                    throw new ArgumentException(
                        "Room.Read[TR5]: Found " + separator14.ToString("X8") + ", Expected 0", "separator14");

                var lightSize = br.ReadUInt32();
                var numLights2 = br.ReadUInt32();
                if(numLights2 != numLights)
                    throw new ArgumentException("Room.Read[TR5]: Room.numLights2 != Room.numLights", "numLights2");

                r.Unknown_R6 = br.ReadUInt32();
                r.Room_Y_Top = -br.ReadSingle();
                r.Room_Y_Bottom = -br.ReadSingle();

                var numLayers = br.ReadUInt32();

                var layerOffset = br.ReadUInt32();
                var verticesOffset = br.ReadUInt32();
                var polyOffset = br.ReadUInt32();
                var polyOffset2 = br.ReadUInt32();
                if(polyOffset != polyOffset2)
                    throw new ArgumentException("Room.Read[TR5]: Room.polyOffset2 != Room.polyOffset", "polyOffset2");

                var verticesSize = br.ReadUInt32();
                if(verticesSize % 28 != 0)
                    throw new ArgumentException(
                        "Room.Read[TR5]: verticesSize has wrong value: " + verticesSize, "verticesSize");

                var separator15 = br.ReadUInt32();
                if (separator15 != 0xCDCDCDCD)
                    throw new ArgumentException(
                        "Room.Read[TR5]: Found " + separator15.ToString("X8") + ", Expected 0xCDCDCDCD", "separator15");

                var separator16 = br.ReadUInt32();
                if (separator16 != 0xCDCDCDCD)
                    throw new ArgumentException(
                        "Room.Read[TR5]: Found " + separator16.ToString("X8") + ", Expected 0xCDCDCDCD", "separator16");

                var separator17 = br.ReadUInt32();
                if (separator17 != 0xCDCDCDCD)
                    throw new ArgumentException(
                        "Room.Read[TR5]: Found " + separator17.ToString("X8") + ", Expected 0xCDCDCDCD", "separator17");

                var separator18 = br.ReadUInt32();
                if (separator18 != 0xCDCDCDCD)
                    throw new ArgumentException(
                        "Room.Read[TR5]: Found " + separator18.ToString("X8") + ", Expected 0xCDCDCDCD", "separator18");

                r.Lights = br.ReadArray(numLights, () => Light.Read(br, TRVersion.TR5));

                br.BaseStream.Position = pos + 208 + sectorDataOffset;
            }
            else
            {
                r.Offset = new Vertex(br.ReadInt32(), 0, -br.ReadInt32());
                r.Y_Bottom = -br.ReadInt32();
                r.Y_Bottom = -br.ReadInt32();

                var numDataWords = br.ReadUInt32();
                var pos = br.BaseStream.Position;

                var numVertices = br.ReadUInt16();
                r.Vertices = br.ReadArray(numVertices, () => RoomVertex.Read(br, ver));
                var numRectangles = br.ReadUInt16();
                r.Rectangles = br.ReadArray(numRectangles, () => QuadFace.Read(br, TRVersion.TR1));
                var numTriangles = br.ReadUInt16();
                r.Triangles = br.ReadArray(numTriangles, () => Triangle.Read(br, TRVersion.TR1));
                var numSprites = br.ReadUInt16();
                r.Sprites = br.ReadArray(numSprites, () => Sprite.Read(br));

                br.BaseStream.Position = pos + numDataWords * 2;

                r.Portals = br.ReadArray(br.ReadUInt16(), () => Portal.Read(br));

                r.Num_Z_Sectors = br.ReadUInt16();
                r.Num_X_Sectors = br.ReadUInt16();

                r.Sectors = br.ReadArray(r.Num_Z_Sectors * r.Num_X_Sectors, () => Sector.Read(br));

                if (ver < TRVersion.TR3)
                {
                    r.Intensity1 = (short) ((8191 - br.ReadInt16()) << 2);
                    r.Intensity2 = ver < TRVersion.TR2 ? r.Intensity1 : (short) ((8191 - br.ReadInt16()) << 2);
                }
                if (ver == TRVersion.TR2)
                {
                    r.LightMode = br.ReadInt16();
                }

                r.Lights = br.ReadArray(br.ReadUInt16(), () => Light.Read(br, ver));

                r.StaticMeshes = br.ReadArray(br.ReadUInt16(), () => RoomStaticMesh.Parse(br, ver));

                r.AlternateRoom = br.ReadInt16();

                r.Flags = br.ReadUInt16();

                if (ver == TRVersion.TR1)
                {
                    r.ReverbInfo = ReverbInfo.MediumRoom;

                    var c = r.Intensity1 / 32767.0f;
                    r.LightColor = new FloatColor(c, c, c);
                }
                else if (ver == TRVersion.TR2)
                {
                    r.ReverbInfo =
                        ((RoomFlags) r.Flags).HasFlag(RoomFlags.WindBlowPonytail)
                            ? ReverbInfo.Outside
                            : ReverbInfo.MediumRoom;

                    var c = r.Intensity1 / 16384.0f;
                    r.LightColor = new FloatColor(c, c, c);
                }
                else if (ver == TRVersion.TR3)
                {
                    if (((RoomFlags) r.Flags).HasFlag(RoomFlags.Quicksand))
                    {
                        // Move quicksand flag to another bit to avoid confusion with NL flag.
                        r.Flags = (ushort) ((r.Flags | 0x0002) ^ 0x0080);
                    }
                    r.WaterScheme = br.ReadByte();
                    r.ReverbInfo = (ReverbInfo) br.ReadByte();
                    r.AlternateGroup = br.ReadSByte();

                    var c = r.Intensity1 / 65534.0f;
                    r.LightColor = new FloatColor(c, c, c);
                }
                else if (ver == TRVersion.TR4)
                {
                    r.WaterScheme = br.ReadByte();
                    r.ReverbInfo = (ReverbInfo) br.ReadByte();
                    r.AlternateGroup = br.ReadSByte();

                    r.LightColor = new FloatColor(
                        (r.Intensity2 & 0x00FF) / 255.0f,
                        ((r.Intensity1 & 0xFF00) >> 8) / 255.0f,
                        (r.Intensity1 & 0x00FF) / 255.0f,
                        ((r.Intensity2 & 0xFF00) >> 8) / 255.0f);
                }
            }

            return r;
        }
    }

    public struct Mesh
    {
        /// <summary>
        /// Usually close to mesh's centroid
        /// </summary>
        public Vertex Centre { get; set; }

        /// <summary>
        /// Radius of collisional sphere
        /// </summary>
        public int CollisionSize { get; set; }

        /// <summary>
        /// List of vertices
        /// </summary>
        public Vertex[] Vertices { get; set; }

        /// <summary>
        /// List of normals
        /// </summary>
        public Vertex[] Normals { get; set; }

        /// <summary>
        /// List of light values
        /// </summary>
        public short[] Lights { get; set; }

        /// <summary>
        /// List of textured rectangles
        /// </summary>
        public QuadFace[] TexturedRectangles { get; set; }

        /// <summary>
        /// List of textured triangles
        /// </summary>
        public Triangle[] TexturedTriangles { get; set; }

        /// <summary>
        /// List of coloured rectangles
        /// </summary>
        public QuadFace[] ColouredRectangles { get; set; }

        /// <summary>
        /// List of coloured triangles
        /// </summary>
        public Triangle[] ColouredTriangles { get; set; }
        
        /// <summary>
        /// Reads a <see cref="Mesh"/>
        /// </summary>
        /// <param name="br">The <see cref="BinaryReader"/> used to read the <see cref="Mesh"/></param>
        /// <param name="ver">The game version</param>
        /// <returns>A <see cref="Mesh"/></returns>
        public static Mesh Read(BinaryReader br, TRVersion ver = TRVersion.Unknown)
        {
            var ret = new Mesh();

            ret.Centre = Vertex.Read16(br);
            ret.CollisionSize = br.ReadInt32();

            ret.Vertices = br.ReadArray(br.ReadInt16(), () => Vertex.Read16(br));

            var numNormals = br.ReadInt16();
            if (numNormals >= 0)
                ret.Normals = br.ReadArray(numNormals, () => Vertex.Read16(br));
            else
                ret.Lights = br.ReadInt16Array(-numNormals);

            ret.TexturedRectangles = br.ReadArray(br.ReadInt16(), () => QuadFace.Read(br, ver));
            ret.TexturedTriangles = br.ReadArray(br.ReadInt16(), () => Triangle.Read(br, ver));

            if(ver < TRVersion.TR4)
            {
                ret.ColouredRectangles = br.ReadArray(br.ReadInt16(), () => QuadFace.Read(br, ver));
                ret.ColouredTriangles = br.ReadArray(br.ReadInt16(), () => Triangle.Read(br, ver));
            }

            return ret;
        }
    }

    public struct StaticMesh
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StaticMesh"/> class.
        /// </summary>
        public StaticMesh(uint objectId, ushort mesh, Vertex[] visibilityBox, Vertex[] collisionBox, ushort flags)
        {
            ObjectID = objectId;
            Mesh = mesh;
            VisibilityBox = visibilityBox;
            CollisionBox = collisionBox;
            Flags = flags;
        }

        /// <summary>
        /// Object Identifier (matched in Items[])
        /// </summary>
        public uint ObjectID { get; set; }

        /// <summary>
        /// Mesh (offset into MeshPointers[])
        /// </summary>
        public ushort Mesh { get; set; }

        public Vertex[] VisibilityBox { get; set; }

        public Vertex[] CollisionBox { get; set; }

        /// <summary>
        /// Meaning is uncertain, it's usually 2, and is 3 for objects Lara can travel through
        /// </summary>
        public ushort Flags { get; set; }

        /// <summary>
        /// Reads a <see cref="StaticMesh"/>
        /// </summary>
        /// <param name="br">The <see cref="BinaryReader"/> used to read the <see cref="StaticMesh"/></param>
        /// <returns>A <see cref="StaticMesh"/></returns>
        public static StaticMesh Read(BinaryReader br)
        {
            var ret = new StaticMesh();

            ret.ObjectID = br.ReadUInt32();
            ret.Mesh = br.ReadUInt16();

            ret.VisibilityBox = new[]
            {
                new Vertex(br.ReadInt16(), br.ReadInt16(), -br.ReadInt16()),
                new Vertex(-br.ReadInt16(), -br.ReadInt16(), -br.ReadInt16()) 
            };

            ret.CollisionBox = new[]
            {
                new Vertex(br.ReadInt16(), br.ReadInt16(), -br.ReadInt16()),
                new Vertex(-br.ReadInt16(), -br.ReadInt16(), -br.ReadInt16())
            };

            ret.Flags = br.ReadUInt16();

            return ret;
        }
    }

    public struct MeshTree
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MeshTree"/> class.
        /// </summary>
        public MeshTree(uint flags, Vertex offset)
        {
            Flags = flags;
            Offset = offset;
        }

        /// <summary>
        /// Bit 1 (0x0002) indicates "put the parent mesh on the mesh stack"<br/>
        /// Bit 0 (0x0001) indicates "take the top mesh off the mesh stack and use as the parent mesh"
        /// </summary>
        public uint Flags { get; set; }

        public Vertex Offset { get; set; }

        /// <summary>
        /// Reads a <see cref="MeshTree"/>
        /// </summary>
        /// <param name="br">The <see cref="BinaryReader"/> used to read the <see cref="MeshTree"/></param>
        /// <returns>A <see cref="MeshTree"/></returns>
        public static MeshTree Read(BinaryReader br)
        {
            return new MeshTree(br.ReadUInt32(), Vertex.Read32(br));
        }
    }

    public struct Moveable
    {
        /// <summary>
        /// Item Identifier (matched in Items[])
        /// </summary>
        public uint ObjectID { get; set; }

        /// <summary>
        /// Number of meshes in this object
        /// </summary>
        public ushort NumMeshes { get; set; }

        /// <summary>
        /// Starting mesh (offset into MeshPointers[])
        /// </summary>
        public ushort StartingMesh { get; set; }

        /// <summary>
        /// Offset into MeshTree[]
        /// </summary>
        public uint MeshTreeIndex { get; set; }

        /// <summary>
        /// Byte offset into Frames[] (divide by 2 for Frames[i])
        /// </summary>
        public uint FrameOffset { get; set; }

        public uint FrameIndex { get; set; }

        /// <summary>
        /// Offset into Animations[]
        /// </summary>
        public ushort AnimationIndex { get; set; }

        /// <summary>
        /// Reads a <see cref="Moveable"/>
        /// </summary>
        /// <param name="br">The <see cref="BinaryReader"/> used to read the <see cref="Moveable"/></param>
        /// <param name="ver">The game version</param>
        /// <returns>A <see cref="Moveable"/></returns>
        public static Moveable Read(BinaryReader br, TRVersion ver = TRVersion.Unknown)
        {
            var ret = new Moveable
            {
                ObjectID = br.ReadUInt32(),
                NumMeshes = br.ReadUInt16(),
                StartingMesh = br.ReadUInt16(),
                MeshTreeIndex = br.ReadUInt32(),
                FrameOffset = br.ReadUInt32(),
                AnimationIndex = br.ReadUInt16()
            };
            
            if(ver == TRVersion.TR5)
            {
                var filler = br.ReadUInt16();
                if(filler != 0xFFEF)
                    throw new ArgumentException("Moveable.Read[TR5]: Found " + filler.ToString("X4") + ", Expected 0xFFEF", "filler");
            }

            return ret;
        }
    }

    public struct Item
    {
        /// <summary>
        /// Object Identifier (matched in Moveables[] or SpriteSequences[] as appropriate)
        /// </summary>
        public short ObjectID { get; set; }

        /// <summary>
        /// Which room contains this item
        /// </summary>
        public short Room { get; set; }

        /// <summary>
        /// Position (in world coordinates)
        /// </summary>
        public Vertex Position { get; set; }

        /// <summary>
        /// Rotation -> ((0xc000 >> 14) * 90) degrees
        /// </summary>
        public float Rotation { get; set; }

        /// <summary>
        /// Constant lighting, -1 means use mesh lighting
        /// </summary>
        public short Intensity1 { get; set; }

        /// <summary>
        /// Like Intensity1, and almost always the same value [absent from TR1 data files]
        /// </summary>
        public short Intensity2 { get; set; }

        /// <summary>
        /// Object code bit - used for altering entity behavior. Used only in TR4-5
        /// </summary>
        public short ObjectCodeBit { get; set; }

        /// <summary>
        /// 0x0100 indicates "initially invisible"<br/>
        /// 0x3e00 indicates "open" or "activated"
        /// </summary>
        public ushort Flags { get; set; }

        public ushort ActivationMash => (ushort)((Flags & 0x3e00) >> 9);

        /// <summary>
        /// Determines if the <see cref="Item"/> is initially invisible
        /// </summary>
        public bool InitiallyInvisible => (Flags & 0x0100) != 0;

        /// <summary>
        /// Reads a <see cref="Item"/>
        /// </summary>
        /// <param name="br">The <see cref="BinaryReader"/> used to read the <see cref="Item"/></param>
        /// <param name="ver">The game version</param>
        /// <returns>An <see cref="Item"/></returns>
        public static Item Read(BinaryReader br, TRVersion ver = TRVersion.Unknown)
        {
            var ret = new Item();

            ret.ObjectID = br.ReadInt16();
            ret.Room = br.ReadInt16();
            ret.Position = Vertex.Read32(br);
            ret.Rotation = br.ReadUInt16() / 16384.0f * -90;
            ret.Intensity1 = (short)br.ReadUInt16();

            if (ver < TRVersion.TR3)
                if (ret.Intensity1 >= 0)
                    ret.Intensity1 = (short)((8191 - ret.Intensity1) << 2);


            if (ver == TRVersion.TR2 || ver == TRVersion.TR3)
                ret.Intensity2 = (short) br.ReadUInt16(); // TODO: Cast ushort to short? weird
            else
                ret.Intensity2 = ret.Intensity1; 

            if(ver == TRVersion.TR2)
                if (ret.Intensity2 >= 0)
                    ret.Intensity2 = (short)((8191 - ret.Intensity2) << 2);


            if (ver < TRVersion.TR4)
            {
                ret.ObjectCodeBit = 0;
            }
            else
            {
                ret.ObjectCodeBit = (short)br.ReadUInt16();
            }

            ret.Flags = br.ReadUInt16();

            return ret;
        }
    }

    public struct SpriteTexture
    {
        public ushort Tile { get; set; }

        public short X0 { get; set; }

        public short Y0 { get; set; }

        public short X1 { get; set; }

        public short Y1 { get; set; }

        public short LeftSide { get; set; }

        public short TopSide { get; set; }

        public short RightSide { get; set; }

        public short BottomSide { get; set; }

        /// <summary>
        /// Reads a <see cref="SpriteTexture"/>
        /// </summary>
        /// <param name="br">The <see cref="BinaryReader"/> used to read the <see cref="SpriteTexture"/></param>
        /// <param name="ver">The game version</param>
        /// <returns>An <see cref="SpriteTexture"/></returns>
        public static SpriteTexture Read(BinaryReader br, TRVersion ver = TRVersion.Unknown)
        {
            var ret = new SpriteTexture();

            ret.Tile = br.ReadUInt16();

            var tx = br.ReadByte();
            var ty = br.ReadByte();
            var tw = br.ReadUInt16();
            var th = br.ReadUInt16();
            var tleft = br.ReadInt16();
            var ttop = br.ReadInt16();
            var tright = br.ReadInt16();
            var tbottom = br.ReadInt16();

            if(ver < TRVersion.TR4)
            {
                ret.X0 = tx;
                ret.Y0 = ty;
                ret.X1 = (short)(tx + tw / 256.0f);
                ret.Y1 = (short)(ty + th / 256.0f);

                ret.LeftSide = tleft;
                ret.RightSide = tright;
                ret.TopSide = (short)-tbottom;
                ret.BottomSide = (short)-ttop;
            }
            else
            {
                ret.X0 = tleft;
                ret.X1 = tright;
                ret.Y0 = tbottom;
                ret.Y1 = ttop;

                ret.LeftSide = tx;
                ret.RightSide = (short)(tx + tw / 256.0f);
                ret.BottomSide = ty;
                ret.TopSide = (short) (ty + th / 256.0f);
            }

            return ret;
        }
    }

    public struct SpriteSequence
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpriteSequence"/> class.
        /// </summary>
        public SpriteSequence(int spriteId, short negativeLength, short offset)
        {
            SpriteID = spriteId;
            NegativeLength = negativeLength;
            Offset = offset;
        }

        /// <summary>
        /// Sprite identifier 
        /// </summary>
        public int SpriteID { get; set; }

        /// <summary>
        /// Negative of 'how many sprites are in this sequence'
        /// </summary>
        public short NegativeLength { get; set; }

        /// <summary>
        /// Where (in sprite texture list) this sequence starts
        /// </summary>
        public short Offset { get; set; }

        /// <summary>
        /// Reads a <see cref="SpriteSequence"/>
        /// </summary>
        /// <param name="br">The <see cref="BinaryReader"/> used to read the <see cref="SpriteSequence"/></param>
        /// <returns>A <see cref="SpriteSequence"/></returns>
        public static SpriteSequence Read(BinaryReader br)
        {
            return new SpriteSequence
            {
                SpriteID = br.ReadInt32(),
                NegativeLength = (short)-br.ReadInt16(),
                Offset = br.ReadInt16()
            };
        } 
    }

    public struct Animation
    {
        /// <summary>
        /// Byte offset into Frames[] (divide by 2 for Frames[i])
        /// </summary>
        public uint FrameOffset { get; set; }

        /// <summary>
        /// Engine ticks per frame
        /// </summary>
        public byte FrameRate { get; set; }

        /// <summary>
        /// Number of <see cref="short"/> in Frames[] used by this animation
        /// </summary>
        public byte FrameSize { get; set; }

        public ushort StateID { get; set; }

        public int Speed { get; set; }

        public int Acceleration { get; set; }

        public int SpeedLateral { get; set; }

        public int AccelerationLateral { get; set; }

        /// <summary>
        /// First frame in this animation
        /// </summary>
        public ushort FrameStart { get; set; }

        /// <summary>
        /// Last frame in this animation
        /// </summary>
        public ushort FrameEnd { get; set; }

        public ushort NextAnimation { get; set; }

        public ushort NextFrame { get; set; }

        public ushort NumStateChanges { get; set; }

        /// <summary>
        /// Offset into StateChanges[]
        /// </summary>
        public ushort StateChangeOffset { get; set; }

        /// <summary>
        /// How many of them to use
        /// </summary>
        public ushort NumAnimCommands { get; set; }

        /// <summary>
        /// Offset into AnimCommand[]
        /// </summary>
        public ushort AnimCommand { get; set; }

        /// <summary>
        /// Reads a <see cref="Animation"/>
        /// </summary>
        /// <param name="br">The <see cref="BinaryReader"/> used to read the <see cref="Animation"/></param>
        /// <param name="ver">The game version</param>
        /// <returns>An <see cref="Animation"/></returns>
        public static Animation Read(BinaryReader br, TRVersion ver = TRVersion.Unknown)
        {
            var ret = new Animation();

            ret.FrameOffset = br.ReadUInt32();
            ret.FrameRate = br.ReadByte();
            ret.FrameSize = br.ReadByte();
            ret.StateID = br.ReadUInt16();

            ret.Speed = br.ReadInt32();
            ret.Acceleration = br.ReadInt32();

            if(ver >= TRVersion.TR4)
            {
                ret.SpeedLateral = br.ReadInt32();
                ret.AccelerationLateral = br.ReadInt32();
            }

            ret.FrameStart = br.ReadUInt16();
            ret.FrameEnd = br.ReadUInt16();
            ret.NextAnimation = br.ReadUInt16();
            ret.NextFrame = br.ReadUInt16();

            ret.NumStateChanges = br.ReadUInt16();
            ret.StateChangeOffset = br.ReadUInt16();
            ret.NumAnimCommands = br.ReadUInt16();
            ret.AnimCommand = br.ReadUInt16();

            return ret;
        }
    }

    public struct StateChange
    {
        public ushort StateID { get; set; }

        /// <summary>
        /// Number of ranges (seems to always be 1..5)
        /// </summary>
        public ushort NumAnimDispatches { get; set; }

        /// <summary>
        /// Offset into AnimDispatches[]
        /// </summary>
        public ushort AnimDispatch { get; set; }

        /// <summary>
        /// Reads a <see cref="StateChange"/>
        /// </summary>
        /// <param name="br">The <see cref="BinaryReader"/> used to read the <see cref="StateChange"/></param>
        /// <returns>A <see cref="StateChange"/></returns>
        public static StateChange Read(BinaryReader br)
        {
            return new StateChange
            {
                StateID = br.ReadUInt16(),
                NumAnimDispatches = br.ReadUInt16(),
                AnimDispatch = br.ReadUInt16()
            };
        }
    }

    public struct AnimDispatch
    {
        /// <summary>
        /// Lowest frame that uses this range
        /// </summary>
        public short Low { get; set; }

        /// <summary>
        /// Highest frame that uses this range
        /// </summary>
        public short High { get; set; }

        /// <summary>
        /// Animation to dispatch to
        /// </summary>
        public short NextAnimation { get; set; }

        /// <summary>
        /// Frame offset to dispatch to
        /// </summary>
        public short NextFrame { get; set; }

        /// <summary>
        /// Reads a <see cref="AnimDispatch"/>
        /// </summary>
        /// <param name="br">The <see cref="BinaryReader"/> used to read the <see cref="AnimDispatch"/></param>
        /// <returns>A <see cref="AnimDispatch"/></returns>
        public static AnimDispatch Read(BinaryReader br)
        {
            return new AnimDispatch
            {
                Low = br.ReadInt16(),
                High = br.ReadInt16(),
                NextAnimation = br.ReadInt16(),
                NextFrame = br.ReadInt16()
            };
        }
    }

    public struct Box
    {
        /// <summary>
        /// Horizontal dimensions in global units
        /// </summary>
        public uint Zmin { get; set; }

        public uint Zmax { get; set; }

        public uint Xmin { get; set; }

        public uint Xmax { get; set; }

        /// <summary>
        /// Height value in global units
        /// </summary>
        public short TrueFloor { get; set; }

        /// <summary>
        /// Index into Overlaps[]<br/>
        /// High bit is sometimes set; this occurs in front of swinging doors and the like
        /// </summary>
        public short OverlapIndex { get; set; }

        /// <summary>
        /// Reads a <see cref="Box"/>
        /// </summary>
        /// <param name="br">The <see cref="BinaryReader"/> used to read the <see cref="Box"/></param>
        /// <param name="ver">The game version</param>
        /// <returns>A <see cref="Box"/></returns>
        public static Box Read(BinaryReader br, TRVersion ver = TRVersion.Unknown)
        {
            var ret = new Box();
            
            if(ver >= TRVersion.TR2)
            {
                ret.Zmin = (uint) (1024 * br.ReadByte()); // todo: opentomb multiplies by -1024
                ret.Zmax = (uint) (1024 * br.ReadByte());
                ret.Xmin = (uint) (1024 * br.ReadByte());
                ret.Xmax = (uint) (1024 * br.ReadByte());
            }
            else
            {
                ret.Zmin = br.ReadUInt32();
                ret.Zmax = br.ReadUInt32();
                ret.Xmin = br.ReadUInt32();
                ret.Xmax = br.ReadUInt32();
            }

            ret.TrueFloor = br.ReadInt16();
            ret.OverlapIndex = br.ReadInt16();

            return ret;
        }
    }

    public struct Zone
    {
        public ushort[] GroundZonesNormal { get; set; }

        public ushort FlyZoneNormal { get; set; }

        public ushort[] GroundZonesAlternate { get; set; }

        public ushort FlyZoneAlternate { get; set; }

        /// <summary>
        /// Reads a <see cref="Zone"/>
        /// </summary>
        /// <param name="br">The <see cref="BinaryReader"/> used to read the <see cref="Zone"/></param>
        /// <param name="ver">The game version</param>
        /// <returns>A <see cref="Zone"/></returns>
        public static Zone Read(BinaryReader br, TRVersion ver = TRVersion.Unknown)
        {
            var ret = new Zone();

            var arrSize = ver < TRVersion.TR2 ? 2 : 4;

            ret.GroundZonesNormal = br.ReadUInt16Array(arrSize); // todo: OpenTomb inverts the two fields
            ret.FlyZoneNormal = br.ReadUInt16();
            ret.GroundZonesAlternate = br.ReadUInt16Array(arrSize);
            ret.FlyZoneAlternate = br.ReadUInt16();

            return ret;
        }
    }

    public struct SoundSource
    {
        /// <summary>
        /// Absolute X position of sound source (world coordinates)
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Absolute Y position of sound source (world coordinates)
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// Absolute Z position of sound source (world coordinates)
        /// </summary>
        public int Z { get; set; }

        /// <summary>
        /// Internal sound index
        /// </summary>
        public ushort SoundID { get; set; }

        /// <summary>
        /// 0x40, 0x80 or 0xc0
        /// </summary>
        public ushort Flags { get; set; }

        /// <summary>
        /// Reads a <see cref="SoundSource"/>
        /// </summary>
        /// <param name="br">The <see cref="BinaryReader"/> used to read the <see cref="SoundSource"/></param>
        /// <returns>A <see cref="SoundSource"/></returns>
        public static SoundSource Read(BinaryReader br)
        {
            var ret = new SoundSource
            {
                X = br.ReadInt32(),
                Y = br.ReadInt32(),
                Z = br.ReadInt32(),
                SoundID = br.ReadUInt16(),
                Flags = br.ReadUInt16()
            };

            return ret;
        }
    }

    public enum LoopType
    {
        /// <summary>
        /// Normal playback (0)
        /// </summary>
        None,
        /// <summary>
        /// Looped (2 in TR1, 3 in other games), meaning the sound will be looped until strictly stopped by an engine event
        /// </summary>
        Forward,
        /// <summary>
        /// One-shot rewound (1 in TR1/TR2, 2 in other games), meaning the sound will be rewound if triggered again
        /// </summary>
        PingPong,
        /// <summary>
        /// One-shot wait mode (only present above TR2, value 1), meaning the same sound will be ignored until current one stops
        /// </summary>
        Wait
    }

    public struct SoundDetails
    {
        /// <summary>
        /// Index into SampleIndices[]
        /// </summary>
        public ushort Sample { get; set; }

        /// <summary>
        /// Global sample volume
        /// </summary>
        public ushort Volume { get; set; }

        /// <summary>
        /// Sound range
        /// </summary>
        public ushort SoundRange { get; set; }

        /// <summary>
        /// Chance to play
        /// </summary>
        public ushort Chance { get; set; }

        /// <summary>
        /// Pitch shift
        /// </summary>
        public short Pitch { get; set; }

        public ushort Characteristics { get; set; }

        public LoopType GetLoopType(TRVersion ver = TRVersion.Unknown)
        {
            switch(Characteristics & 3)
            {
                case 1:
                    return ver < TRVersion.TR3 ? LoopType.PingPong : LoopType.Wait;
                case 2:
                    return ver == TRVersion.TR1 ? LoopType.Forward : LoopType.PingPong;
                case 3:
                    if(ver >= TRVersion.TR3) return LoopType.Forward;
                    break;
            }
            return LoopType.None;
        }

        /// <summary>
        /// Number of sound samples in this group. If there are more than one samples, then engine will select one to play based on randomizer (for example, listen to Lara footstep sounds).
        /// </summary>
        public byte SampleCount => (byte)((Characteristics >> 2) & 0x0f);

        /// <summary>
        /// Randomize pitch. When this flag is set, sound pitch will be slightly varied with each playback event.
        /// </summary>
        public bool UseRandomPitch => ((Characteristics >> 8) & 0x20) == 0x20;

        /// <summary>
        /// Randomize gain. When this flag is set, sound volume (gain) will be slightly varied with each playback event.
        /// </summary>
        public bool UseRandomVolume => ((Characteristics >> 8) & 0x40) == 0x40;

        public const int DefaultRange = 8;
        public const short DefaultPitch = 1;

        /// <summary>
        /// Reads a <see cref="SoundDetails"/>
        /// </summary>
        /// <param name="br">The <see cref="BinaryReader"/> used to read the <see cref="SoundDetails"/></param>
        /// <param name="ver">The game version</param>
        /// <returns>A <see cref="SoundDetails"/></returns>
        public static SoundDetails Read(BinaryReader br, TRVersion ver = TRVersion.Unknown)
        {
            var ret = new SoundDetails();

            ret.Sample = br.ReadUInt16();
            
            if(ver < TRVersion.TR3)
            {
                ret.Volume = br.ReadUInt16();
                ret.Chance = br.ReadUInt16();
                ret.Characteristics = br.ReadUInt16();
                ret.SoundRange = DefaultRange;
                ret.Pitch = DefaultPitch;
            }
            else
            {
                ret.Volume = br.ReadByte();
                ret.SoundRange = br.ReadByte();
                ret.Chance = br.ReadByte();
                ret.Pitch = br.ReadByte();
                ret.Characteristics = br.ReadUInt16();
            }

            return ret;
        }
    }

    public struct ObjectTextureVertex
    {
        /// <summary>
        /// 1 if <see cref="Xpixel"/> is the low value, -1 if <see cref="Xpixel"/> is the high value in the object texture
        /// </summary>
        public sbyte Xcoordinate { get; set; }

        public byte Xpixel { get; set; }

        /// <summary>
        /// 1 if <see cref="Ypixel"/> is the low value, -1 if <see cref="Ypixel"/> is the high value in the object texture
        /// </summary>
        public sbyte Ycoordinate { get; set; }

        public byte Ypixel { get; set; }

        /// <summary>
        /// Reads a <see cref="ObjectTextureVertex"/>
        /// </summary>
        /// <param name="br">The <see cref="BinaryReader"/> used to read the <see cref="ObjectTextureVertex"/></param>
        /// <param name="ver">The game version</param>
        /// <returns>A <see cref="ObjectTextureVertex"/></returns>
        public static ObjectTextureVertex Read(BinaryReader br, TRVersion ver = TRVersion.Unknown)
        {
            var ret = new ObjectTextureVertex();

            ret.Xcoordinate = br.ReadSByte();
            ret.Xpixel = br.ReadByte();
            ret.Ycoordinate = br.ReadSByte();
            ret.Ypixel = br.ReadByte();

            if(ver >= TRVersion.TR4)
            {
                if (ret.Xcoordinate == 0)
                    ret.Xcoordinate = 1;

                if (ret.Ycoordinate == 0)
                    ret.Ycoordinate = 1;
            }

            return ret;
        }
    }

    public enum BlendingMode : ushort
    {
        /// <summary>
        /// Texture is all-opaque, and that transparency information is ignored.
        /// </summary>
        Opaque = 0,

        /// <summary>
        /// Texture uses alpha testing, i.e. it may contain opaque and completely transparent regions.<br/>
        /// In 8-bit colour, index 0 is the transparent colour, while in 16-bit colour, the top bit (0x8000) is the alpha channel (1 = opaque, 0 = transparent).<br/>
        /// In 32-bit textures, transparency is specified by full magenta colour value (RGB = 255,0,255) — i.e. pixel has to be magenta to be transparent.
        /// </summary>
        Transparent = 1,

        /// <summary>
        /// Texture uses alpha blending with additive operation.<br/>
        /// No depth sorting is done on alpha-blended textures.
        /// </summary>
        Multiply = 2,

        /// <summary>
        /// Not implemented properly in PC version, but on PlayStation this type produces alpha blending with inversion operation, thus converting all the bright zones to dark, and dark zones to bright.<br/>
        /// This blending mode was used for smooth textured shadows, footprints and black smoke sprites.<br/>
        /// There is a remnant of this blending mode in the form of entity type named smoke emitter black.
        /// </summary>
        SimpleShade = 3,

        /// <summary>
        /// Alpha-tested face without Z testing, i.e. depth information is ignored. Used for GUI elements (such as fonts) and skyboxes.
        /// </summary>
        TransparentIgnoreZ = 4,

        /// <summary>
        /// Unused.<br/>
        /// Possibly was used in PlayStation versions.
        /// </summary>
        InvertSrc = 5,

        /// <summary>
        /// Wireframe mode.<br/>
        /// Used for “line particles”, such as gun sparks, water drops and laser beams. Possibly was also used for debug purposes.
        /// </summary>
        Wireframe = 6,
        /// <summary>
        /// Forced alpha value.<br/>
        /// It’s ordinary alpha-tested face, but alpha value for this face is overridden with global variable.<br/>
        /// Used to “fade out” specific meshes, like vanishing enemy bodies or Semerkhet ghost in “Tomb of Semerkhet” level.
        /// </summary>
        TransparentAlpha = 7,
        InvertDst = 8,
        Screen = 9,
        Hide = 10,
        AnimatedTexture = 11
    }

    public struct ObjectTexture
    {
        /// <summary>
        /// Specifies transparency mode (i.e. blending mode) used for face with this texture applied.
        /// </summary>
        public BlendingMode TransparencyFlags { get; set; }

        /// <summary>
        /// Index into textile list
        /// </summary>
        public ushort TileAndFlag { get; set; }

        public ushort NewFlags { get; set; }

        /// <summary>
        /// The four corners of the texture
        /// </summary>
        public ObjectTextureVertex[] Vertices { get; set; }

        public uint OriginalU { get; set; }

        public uint OriginalV { get; set; }

        /// <summary>
        /// Actually Width-1
        /// </summary>
        public uint Width { get; set; }

        /// <summary>
        /// Actually Height-1
        /// </summary>
        public uint Height { get; set; }

        /// <summary>
        /// Reads a <see cref="ObjectTexture"/>
        /// </summary>
        /// <param name="br">The <see cref="BinaryReader"/> used to read the <see cref="ObjectTexture"/></param>
        /// <param name="ver">The game version</param>
        /// <returns>A <see cref="ObjectTexture"/></returns>
        public static ObjectTexture Read(BinaryReader br, TRVersion ver = TRVersion.Unknown)
        {
            var ret = new ObjectTexture();

            ret.TransparencyFlags = (BlendingMode) br.ReadUInt16();
            ret.TileAndFlag = br.ReadUInt16();

            if (ver >= TRVersion.TR4)
            {
                if ((ret.TileAndFlag & 0x7FFF) > 128)
                    throw new ArgumentOutOfRangeException("tileAndFlag", ret.TileAndFlag,
                        "ObjectTexture.Read[" + ver + "]: tileAndFlag > 128");

                ret.NewFlags = br.ReadUInt16();
            }
            else
            {
                if (ret.TileAndFlag > 64)
                    throw new ArgumentOutOfRangeException("tileAndFlag", ret.TileAndFlag,
                        "ObjectTexture.Read[" + ver + "]: tileAndFlag > 64");

                if((ret.TileAndFlag & (1 << 15)) != 0)
                    throw new ArgumentException("ObjectTexture.Read[" + ver + "]: tileAndFlag has top bit set", "tileAndFlag");
            }

            ret.Vertices = br.ReadArray(4, () => ObjectTextureVertex.Read(br, ver));

            if(ver >= TRVersion.TR4)
            {
                ret.OriginalU = br.ReadUInt32();
                ret.OriginalV = br.ReadUInt32();

                ret.Width = br.ReadUInt32();
                ret.Height = br.ReadUInt32();
            }

            if(ver == TRVersion.TR5)
            {
                var filler = br.ReadUInt16();

                if(filler != 0)
                    throw new ArgumentException(
                        "ObjectTexture.Read[TR5]: Found " + filler.ToString("X4") + ", Expected 0", "filler");
            }

            return ret;
        }
    }

    public struct AnimatedTexture
    {
        /// <summary>
        /// Offsets into ObjectTextures[], in animation order
        /// </summary>
        public short[] TextureIDs { get; set; }
    }

    public struct Camera
    {
        public Vertex Position { get; set; }

        public short Room { get; set; }
        
        public ushort Flag { get; set; }
        
        /// <summary>
        /// Reads a <see cref="Camera"/>
        /// </summary>
        /// <param name="br">The <see cref="BinaryReader"/> used to read the <see cref="Camera"/></param>
        /// <returns>A <see cref="Camera"/></returns>
        public static Camera Read(BinaryReader br)
        {
            var ret = new Camera();
            
            ret.Position = Vertex.Read32(br, false);
            
            ret.Room = br.ReadInt16();
            ret.Flag = br.ReadUInt16();
            
            return ret;
        }
    }
    
    public struct FlybyCamera
    {
        /// <summary>
        /// Camera position
        /// </summary>
        public Vertex Position { get; set; }
        
        /// <summary>
        /// Camera target
        /// </summary>
        public Vertex Target { get; set; }

        /// <summary>
        /// Number of flyby camera “chain” this particular camera belongs to. Maximum amount of flyby sequences in single level is 8
        /// </summary>
        public byte Sequence { get; set; }

        /// <summary>
        /// Order of the cameras in this particular sequence. Camera with index 0 will be first one in sequence, index 1 means camera will be second in sequence, and so on.
        /// </summary>
        public byte Index { get; set; }

        /// <summary>
        /// Specifies this camera's field of view.
        /// </summary>
        public ushort FieldOfView { get; set; }

        /// <summary>
        /// Specifies the roll factor of this camera. When this parameter is not zero, camera will rotate either left or right along roll axis, creating so-called “dutch angle”.
        /// </summary>
        public short Roll { get; set; } // TODO: OpenTomb uses ushort instead

        /// <summary>
        /// Mainly used to stop camera movement for a given time (in game frames). As this parameter is temporal, it won’t be interpolated between two cameras.
        /// </summary>
        public ushort Timer { get; set; }

        /// <summary>
        /// Specifies movement speed for this particular camera.
        /// </summary>
        public ushort Speed { get; set; }

        /// <summary>
        /// Array of bit flags specifying different camera options
        /// </summary>
        public ushort Flags { get; set; }

        /// <summary>
        /// Should be valid for a given flyby camera, so it will display properly, as well as have the ability to activate heavy triggers.
        /// </summary>
        public uint RoomID { get; set; }

        /// <summary>
        /// Reads a <see cref="FlybyCamera"/>
        /// </summary>
        /// <param name="br">The <see cref="BinaryReader"/> used to read the <see cref="FlybyCamera"/></param>
        /// <returns>A <see cref="FlybyCamera"/></returns>
        public static FlybyCamera Read(BinaryReader br)
        {
            var ret = new FlybyCamera();

            ret.Position = Vertex.Read32(br, false);
            ret.Target = Vertex.Read32(br, false);

            ret.Sequence = br.ReadByte();
            ret.Index = br.ReadByte();

            ret.FieldOfView = br.ReadUInt16();
            ret.Roll = br.ReadInt16();
            ret.Timer = br.ReadUInt16();
            ret.Speed = br.ReadUInt16();
            ret.Flags = br.ReadUInt16();

            ret.RoomID = br.ReadUInt32();

            return ret;
        }
    }

    public struct AIObject
    {
        /// <summary>
        /// Object type ID (same meaning as with tr4_entity)
        /// </summary>
        public ushort TypeID { get; set; }

        /// <summary>
        /// Room where AI object is placed
        /// </summary>
        public ushort Room { get; set; }

        /// <summary>
        /// Coordinates
        /// </summary>
        public Vertex Position { get; set; }

        /// <summary>
        /// Same meaning as with tr4_entity
        /// </summary>
        public short OCB { get; set; }

        /// <summary>
        /// Activation mask, bitwise-shifted left by 1
        /// </summary>
        public ushort Flags { get; set; }

        public int Angle { get; set; }

        /// <summary>
        /// Reads a <see cref="AIObject"/>
        /// </summary>
        /// <param name="br">The <see cref="BinaryReader"/> used to read the <see cref="AIObject"/></param>
        /// <returns>A <see cref="AIObject"/></returns>
        public static AIObject Read(BinaryReader br)
        {
            var ret = new AIObject();

            ret.TypeID = br.ReadUInt16();
            ret.Room = br.ReadUInt16();

            ret.Position = Vertex.Read32(br, false);

            ret.OCB = br.ReadInt16();
            ret.Flags = br.ReadUInt16();
            ret.Angle = br.ReadInt32();

            return ret;
        }
    }

    public struct CinematicFrame
    {
        /// <summary>
        /// Rotation about Y axis, +/- 32767 == +/- 180 degrees
        /// </summary>
        public short RotationY { get; set; }

        /// <summary>
        /// Rotation about Z axis, +/- 32767 == +/- 180 degrees
        /// </summary>
        public short RotationZ { get; set; }

        /// <summary>
        /// Seems to work a lot like rotZ; I haven't yet been able to differentiate them
        /// </summary>
        public short RotationZ2 { get; set; }

        /// <summary>
        /// camera position relative to something (target? Lara? room origin?) *NOT IN WORLD COORDINATES
        /// </summary>
        public Vertex Position { get; set; }

        /// <summary>
        /// Changing this can cause a runtime error
        /// </summary>
        public short Unknown { get; set; }

        /// <summary>
        /// Rotation about X axis, +/- 32767 == +/- 180 degrees
        /// </summary>
        public short RotationX { get; set; }

        /// <summary>
        /// Reads a <see cref="CinematicFrame"/>
        /// </summary>
        /// <param name="br">The <see cref="BinaryReader"/> used to read the <see cref="CinematicFrame"/></param>
        /// <returns>A <see cref="CinematicFrame"/></returns>
        public static CinematicFrame Read(BinaryReader br)
        {
            var ret = new CinematicFrame();

            ret.RotationY = br.ReadInt16();
            ret.RotationZ = br.ReadInt16();
            ret.RotationZ2 = br.ReadInt16();

            ret.Position = new Vertex
            {
                Z = br.ReadInt16(),
                Y = br.ReadInt16(),
                X = br.ReadInt16()
            };

            ret.Unknown = br.ReadInt16();
            ret.RotationX = br.ReadInt16();

            return ret;
        }
    }

    public struct LightMap
    {
        /// <summary>
        /// 32 * 256 (8192) byte array, which is apparently for applying light to 8-bit colour, in some documentation called ColourMap.
        /// </summary>
        public byte[] Map { get; set; }

        /// <summary>
        /// Reads a <see cref="LightMap"/>
        /// </summary>
        /// <param name="br">The <see cref="BinaryReader"/> used to read the <see cref="LightMap"/></param>
        /// <returns>A <see cref="LightMap"/></returns>
        public static LightMap Read(BinaryReader br)
        {
            return new LightMap
            {
                Map = br.ReadBytes(8192) // 32 * 256
            };
        }
    }

    public struct Palette
    {
        /// <summary>
        /// Palette entries
        /// </summary>
        public ByteColor[] Colour { get; set; }

        /// <summary>
        /// Reads a <see cref="Palette"/>
        /// </summary>
        /// <param name="br">The <see cref="BinaryReader"/> used to read the <see cref="Palette"/></param>
        /// <param name="ver">The game version</param>
        /// <returns>A <see cref="Palette"/></returns>
        public static Palette Read(BinaryReader br, TRVersion ver = TRVersion.Unknown)
        {
            return new Palette
            {
                Colour = br.ReadArray(256, () => ByteColor.Read(br, ver))
            };
        }
    }
}
