﻿using System;
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
                        throw new ArgumentException("Light.Read: Found 0x" + separator1.ToString("X8") + ", Expected 0xCDCDCDCD", "separator1");
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
                        throw new ArgumentException("Light.Read: Found 0x" + sep.ToString("X2") + ", Expected 0xCD", "separator2");
                                                                                            
                    if ((sep = br.ReadByte()) != 0xCD)                                      
                        throw new ArgumentException("Light.Read: Found 0x" + sep.ToString("X2") + ", Expected 0xCD", "separator3");
                                                                                            
                    if ((sep = br.ReadByte()) != 0xCD)                                      
                        throw new ArgumentException("Light.Read: Found 0x" + sep.ToString("X2") + ", Expected 0xCD", "separator4");
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
                if (ver > TRVersion.TR1)
                {
                    ret.Attributes = (SpecialRenderingEffects) br.ReadUInt16();
                    ret.Lighting2 = ret.Lighting1;
                }
                else
                {
                    ret.Lighting2 = ver >= TRVersion.TR3 ? br.ReadInt16() : (short) ((8191 - br.ReadInt16()) << 2);
                }
                ret.Normal = Vertex.Zero;
                if (ver < TRVersion.TR3)
                    ret.Color = new FloatColor(
                        ret.Lighting1 / 32768.0f, 
                        ret.Lighting1 / 32768.0f,
                        ret.Lighting1 / 32768.0f);
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
        WindBlowPonytail = 0x0020
        // TR1 has only the water flag and the extra unknown flag 0x0100.
        // TR3 most likely has flags for "is raining", "is snowing", "water is cold", and "is filled by quicksand", among others.
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
        public RoomFlags Flags { get; set; }

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
        public static Room Read(BinaryReader br, TRVersion ver = TRVersion.Unknown)
        {
            var r = new Room();

            if(ver == TRVersion.TR5)
            {

            }
            else
            {
                r.Offset = new Vertex(br.ReadInt32(), 0, -br.ReadInt32());
                r.Y_Bottom = -br.ReadInt32();
                r.Y_Bottom = -br.ReadInt32();

                var numDataWords = br.ReadUInt32();
                var pos = br.BaseStream.Position;

                r.Vertices = br.ReadArray(br.ReadUInt16(), () => RoomVertex.Read(br, ver));
                r.Rectangles = br.ReadArray(br.ReadUInt16(), () => QuadFace.Read(br, TRVersion.TR1));
                r.Triangles = br.ReadArray(br.ReadUInt16(), () => Triangle.Read(br, TRVersion.TR1));
                r.Sprites = br.ReadArray(br.ReadUInt16(), () => Sprite.Read(br));

                br.BaseStream.Position = pos + numDataWords * 2;

                r.Portals = br.ReadArray(br.ReadUInt16(), () => Portal.Read(br));

                r.Num_Z_Sectors = br.ReadUInt16();
                r.Num_X_Sectors = br.ReadUInt16();

                r.Sectors = br.ReadArray(r.Num_Z_Sectors * r.Num_X_Sectors, () => Sector.Read(br));
            }

            return r;
        }
    }
}
