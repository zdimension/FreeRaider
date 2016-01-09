using System.IO;
using System.Runtime.InteropServices;

namespace UniRaider.Loader
{
    public struct TR2Level
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)] private tr2_colour[] _palette;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)] private tr2_colour4[] _palette16;

        public TR2LevelVersion GameVersion { get; set; }

        public tr2_colour[] Palette
        {
            get { return _palette; }
            set { _palette = value; }
        }

        public tr2_colour4[] Palette16
        {
            get { return _palette16; }
            set { _palette16 = value; }
        }

        public uint NumTextiles { get; set; }
        public tr2_textile8[] Textile8 { get; set; }
        public tr2_textile16[] Textile16 { get; set; }
        public uint Unused { get; set; }
        public ushort NumRooms { get; set; }
        public tr2_room[] Rooms { get; set; }
        public uint NumFloorData { get; set; }
        // TODO: Store Floor Data somewhere
           
    }

    public enum TR2LevelVersion : uint
    {
        AutoDetect = uint.MinValue, 
        TR1 = 0x00000020,
        TR2 = 0x0000002d,
        TR3 = 0xFF080038,
        TR3_Alt = 0xFF180038
    }

    public struct tr2_colour // most colours
    {
        public byte Red;
        public byte Green;
        public byte Blue;

        public override string ToString()
        {
            return "tr2_colour { " + Red + " " + Green + " " + Blue + " }";
        }

        public static tr2_colour Parse(BinaryReader br)
        {
            return new tr2_colour
            {
                Red = br.ReadByte(),
                Green = br.ReadByte(),
                Blue = br.ReadByte()
            };
        }
    }

    public struct tr2_colour4 // 16-bit palette uses this
    {
        public byte Red;
        public byte Green;
        public byte Blue;
        public byte Unused;

        public override string ToString()
        {
            return "tr2_colour4 { " + Red + " " + Green + " " + Blue + " Unused=" + Unused + " }";
        }

        public static tr2_colour4 Parse(BinaryReader br)
        {
            return new tr2_colour4
            {
                Red = br.ReadByte(),
                Green = br.ReadByte(),
                Blue = br.ReadByte(),
                Unused = br.ReadByte()
            };
        }
    }

    public struct tr2_vertex // this is how vertices are specified, using relative coordinates
    {
        public short x;
        public short y;
        public short z;

        public static tr2_vertex Parse(BinaryReader br)
        {
            return new tr2_vertex
            {
                x = br.ReadInt16(),
                y = br.ReadInt16(),
                z = br.ReadInt16()
            };
        }
    }

    public struct tr2_face4 // four vertices
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public ushort[] Vertices;
        public ushort Texture;

        public static tr2_face4 Parse(BinaryReader br)
        {
            return new tr2_face4
            {
                Vertices = br.ReadUInt16Array(4),
                Texture = br.ReadUInt16()
            };
        }
    }

    public struct tr2_face3 // three vertices
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public ushort[] Vertices;
        public ushort Texture;

        public static tr2_face3 Parse(BinaryReader br)
        {
            return new tr2_face3
            {
                Vertices = br.ReadUInt16Array(3),
                Texture = br.ReadUInt16()
            };
        }
    }

    public struct tr2_textile8 // eaxh byte represents a pixel whose colour is in the 8-bit palette
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256 * 256)] public byte[] Tile;

        public static tr2_textile8 Parse(BinaryReader br)
        {
            return new tr2_textile8
            {
                Tile = br.ReadByteArray(256 * 256)
            };
        }
    }

    public struct tr2_textile16 // each ushort represents a pixel whose colour is of the form ARGB
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256 * 256)] public ushort[] Tile;

        public static tr2_textile16 Parse(BinaryReader br)
        {
            return new tr2_textile16
            {
                Tile = br.ReadUInt16Array(256 * 256)
            };
        }
    }

    public struct tr2_room_info // x/z = base position in world coord and Y is always zero-relative
    {
        public int x; // X-offset
        public int z; // Z-offset
        public int yBottom; // lowest point
        public int yTop; // highest point

        public static tr2_room_info Parse(BinaryReader br)
        {
            return new tr2_room_info
            {
                x = br.ReadInt32(),
                z = br.ReadInt32(),
                yBottom = br.ReadInt32(),
                yTop = br.ReadInt32()
            };
        }
    }

    public struct tr2_room_portal
    {
        public ushort AdjoiningRoom; // which room this portal leads to
        public tr2_vertex Normal; // which way the portal faces
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public tr2_vertex[] Vertices; // corners of this portal

        public static tr2_room_portal Parse(BinaryReader br)
        {
            return new tr2_room_portal
            {
                AdjoiningRoom = br.ReadUInt16(),
                Normal = tr2_vertex.Parse(br),
                Vertices = br.ReadArray<tr2_vertex>(4)
            };
        }
    }

    public struct tr2_room_sector
    {
        public ushort FDindex; // Index into FloorData[]
        public ushort BoxIndex; // Index into Boxes[]/Zones[] (-1 if none)
        public byte RoomBelow; // The number of the room below this one (-1 or 255 if none)
        public sbyte Floor; // Absolute height of floor (multiply by 256 for world coordinates)
        public byte RoomAbove; // The number of the room above this one (-1 or 255 if none)
        public sbyte Ceiling; // Absolute height of ceiling (multiply by 256 for world coordinates)

        public static tr2_room_sector Parse(BinaryReader br)
        {
            return new tr2_room_sector
            {
                FDindex = br.ReadUInt16(),
                BoxIndex = br.ReadUInt16(),
                RoomBelow = br.ReadByte(),
                Floor = br.ReadSByte(),
                RoomAbove = br.ReadByte(),
                Ceiling = br.ReadSByte()
            };
        }
    }

    public struct tr2_room_light
    {
        // 24 bytes [TR1: 18 bytes] 
        public int x; // X-position of light, in world coordinates 
        public int y; // Y-position of light, in world coordinates 
        public int z; // Z-position of light, in world coordinates 
        public ushort Intensity1; // Light intensity 
        public ushort Intensity2; // Almost always equal to Intensity1 [absent from TR1 data files] 
        public uint Fade1; // Falloff value 1 
        public uint Fade2; // Falloff value 2 [absent from TR1 data files] 

        public static tr2_room_light Parse(BinaryReader br, TR2LevelVersion ver = TR2LevelVersion.AutoDetect)
        {
            if(ver == TR2LevelVersion.AutoDetect) ver = TR2LevelParser.CurrentVersion;
            var ret = new tr2_room_light
            {
                x = br.ReadInt32(),
                y = br.ReadInt32(),
                z = br.ReadInt32(),
                
                
                Fade1 = br.ReadUInt32()
                
            };
            ret.Intensity1 = br.ReadUInt16();
            if (ver != TR2LevelVersion.TR1) ret.Intensity2 = br.ReadUInt16();
            ret.Fade1 = br.ReadUInt32();
            if (ver != TR2LevelVersion.TR1) ret.Fade2 = br.ReadUInt32();
            return ret;
        }
    }

    public struct tr2_vertex_room
    {
        // 12 bytes [TR1: 8 bytes] 
        public tr2_vertex Vertex; // where this vertex lies (relative to tr2_room_info::x/z) 
        public short Lighting1;

        public tr2_vertex_room_Attributes Attributes; // A set of flags for special rendering effects [absent from TR1 data files] 

        public short Lighting2; // Almost always equal to Lighting1 [absent from TR1 data files] 

        public static tr2_vertex_room Parse(BinaryReader br, TR2LevelVersion ver = TR2LevelVersion.AutoDetect)
        {
            if (ver == TR2LevelVersion.AutoDetect) ver = TR2LevelParser.CurrentVersion;
            var ret = new tr2_vertex_room
            {
                Vertex = tr2_vertex.Parse(br),
                Lighting1 = br.ReadInt16()
            };
            if (ver != TR2LevelVersion.TR1)
            {
                ret.Attributes = (tr2_vertex_room_Attributes) br.ReadInt16();
                ret.Lighting2 = br.ReadInt16();
            }
            return ret;
        }
    }

    public enum tr2_vertex_room_Attributes : ushort
    {
        Normal = 0x0010,

        /// <summary>
        /// water/quicksand surface movement
        /// </summary>
        Water_Quicksand_Surface_Movement = 0x2000,

        /// <summary>
        /// under water lighting modulation and movement if viewed from above water surface
        /// </summary>
        WaterLightingModulation = 0x4000,

        /// <summary>
        /// something to do with water surface 
        /// </summary>
        WaterSurface = 0x8000
    }

    public struct tr2_room_sprite
    {
        public short Vertex; // offset into vertex list
        public short Texture; // offset into sprite texture list

        public static tr2_room_sprite Parse(BinaryReader br)
        {
            return new tr2_room_sprite
            {
                Vertex = br.ReadInt16(),
                Texture = br.ReadInt16()
            };
        }
    }

    public struct tr2_room_data
    {
        public short NumVertices; // number of vertices in the following list
        public tr2_vertex_room[] Vertices; // list of vertices (relative coordinates)
        public short NumRectangles; // number of textured rectangles
        public tr2_face4[] Rectangles; // list of textured rectangles
        public short NumTriangles; // number of textured triangles
        public tr2_face3[] Triangles; // list of textures triangles
        public short NumSprites; // number of sprites;
        public tr2_room_sprite[] Sprites; // list of sprites

        public static tr2_room_data Parse(BinaryReader br)
        {
            var ret = new tr2_room_data();
            ret.NumVertices = br.ReadInt16();
            ret.Vertices = br.ReadArray<tr2_vertex_room>(ret.NumVertices);
            ret.NumRectangles = br.ReadInt16();
            ret.Rectangles = br.ReadArray<tr2_face4>(ret.NumRectangles);
            ret.NumTriangles = br.ReadInt16();
            ret.Triangles = br.ReadArray<tr2_face3>(ret.NumTriangles);
            ret.NumSprites = br.ReadInt16();
            ret.Sprites = br.ReadArray<tr2_room_sprite>(ret.NumSprites);
            return ret;
        }
    }

    public struct tr2_room_staticmesh
    {
        public uint x; // absolute X position in world coordinates 
        public uint y; // absolute Y position in world coordinates 
        public uint z; // absolute Z position in world coordinates 
        public ushort Rotation; // high two bits (0xC000) indicate steps of 90 degrees (e.g. (Rotation >> 14) * 90)
        public ushort Intensity1; // Constant lighting; -1 means use mesh lighting
        public ushort Intensity2; // Like Intensity1, and almost always the same value [absent from TR1 data files]
        public ushort ObjectID; // which StaticMesh item to draw

        public static tr2_room_staticmesh Parse(BinaryReader br, TR2LevelVersion ver = TR2LevelVersion.AutoDetect)
        {
            if (ver == TR2LevelVersion.AutoDetect) ver = TR2LevelParser.CurrentVersion;
            var ret = new tr2_room_staticmesh
            {
                x = br.ReadUInt32(),
                y = br.ReadUInt32(),
                z = br.ReadUInt32(),
                Rotation = br.ReadUInt16(),
                Intensity1 = br.ReadUInt16()
            };
            if (ver != TR2LevelVersion.TR1) ret.Intensity2 = br.ReadUInt16();
            ret.ObjectID = br.ReadUInt16();
            return ret;
        }
    }

    public enum tr2_room_LightMode : short
    {
        Normal = 0,
        Flickering = 1,
        Unknown1 = 2,
        Unknown2 = 3
    }

    public enum tr2_room_Flags : short
    {
        FilledWithWater = 0x0001,
        PonytailBlownByTheWind = 0x0020,
        Unknown = 0x0100
    }

    public struct tr2_room
    {
        public tr2_room_info info; // where the room exists, in world coordinates 
        /*public uint NumDataWords; // number of data words
        public ushort[] Data; // the raw data from which the rest of this is derived*/
        public tr2_room_data RoomData; // the room mesh
        public ushort NumPortals; // number of visibility portals to other rooms
        public tr2_room_portal[] Portals; // list of visibility portals
        public ushort NumZsectors; // "width" of sector list
        public ushort NumXsectors; // "height" of sector list
        public tr2_room_sector[] SectorList; // list of sectors in this room
        public short AmbientIntensity1; // this and the next one only affect externally-lit objects

        public short AmbientIntensity2;
            // almost always the same value as AmbientIntensity1 [absent from TR1 data files]

        public tr2_room_LightMode LightMode;
        public ushort NumLights; // number of point lights in this room
        public tr2_room_light[] Lights; // list of point lights
        public ushort NumStaticMeshes; // number of static meshes
        public tr2_room_staticmesh[] StaticMeshes; // list of static meshes
        public short AlternateRoom; // number of the room that this room can alternate with
        public tr2_room_Flags Flags;
        public tr2_colour RoomLightColour; // Present in TR3 only; absent from TR1/TR2

        public static tr2_room Parse(BinaryReader br, TR2LevelVersion ver = TR2LevelVersion.AutoDetect)
        {
            if (ver == TR2LevelVersion.AutoDetect) ver = TR2LevelParser.CurrentVersion;
            var ret = new tr2_room();
            ret.info = tr2_room_info.Parse(br);
            /*ret.NumDataWords = br.ReadUInt32();
            ret.Data = br.ReadUInt16Array(ret.NumDataWords);*/
            ret.RoomData = tr2_room_data.Parse(br);
            ret.NumPortals = br.ReadUInt16();
            ret.Portals = br.ReadArray<tr2_room_portal>(ret.NumPortals);
            ret.NumZsectors = br.ReadUInt16();
            ret.NumXsectors = br.ReadUInt16();
            ret.SectorList = br.ReadArray<tr2_room_sector>(ret.NumZsectors * ret.NumXsectors);
            ret.AmbientIntensity1 = br.ReadInt16();
            if (ver != TR2LevelVersion.TR1)
            {
                ret.AmbientIntensity2 = br.ReadInt16();
                ret.LightMode = (tr2_room_LightMode) br.ReadInt16();
            }
            ret.NumLights = br.ReadUInt16();
            ret.Lights = br.ReadArray<tr2_room_light>(ret.NumLights);
            ret.NumStaticMeshes = br.ReadUInt16();
            ret.StaticMeshes = br.ReadArray<tr2_room_staticmesh>(ret.NumStaticMeshes);
            ret.AlternateRoom = br.ReadInt16();
            ret.Flags = (tr2_room_Flags) br.ReadInt16();
            if(ver > TR2LevelVersion.TR2) ret.RoomLightColour = tr2_colour.Parse(br);
            return ret;
        }
    }

    public struct tr2_mesh
    {
        public tr2_vertex Centre; // This is usually close to the mesh's centroid, and appears to be the center of a sphere used for collision testing. 
        public int CollisionSize; // This appears to be the radius of that aforementioned collisional sphere. 
        public short NumVertices; // number of vertices in this mesh 
        public tr2_vertex[] Vertices; // list of vertices (relative coordinates) 
        public short NumNormals; // If positive, number of normals in this mesh. If negative, number of vertex lighting elements (* (-1)) 
        public tr2_vertex[] Normals; // list of normals (if NumNormals is positive) 
        public short[] Lights; // list of light values (if NumNormals is negative) 
        public short NumTexturedRectangles; // number of textured rectangles in this mesh 
        public tr2_face4[] TexturedRectangles; // list of textured rectangles 
        public short NumTexturedTriangles; // number of textured triangles in this mesh 
        public tr2_face3[] TexturedTriangles; // list of textured triangles 
        public short NumColouredRectangles; // number of coloured rectangles in this mesh 
        public tr2_face4[] ColouredRectangles; // list of coloured rectangles 
        public short NumColouredTriangles; // number of coloured triangles in this mesh 
        public tr2_face3[] ColouredTriangles; // list of coloured triangles 

        public static tr2_mesh Parse(BinaryReader br)
        {
            var ret = new tr2_mesh();
            ret.Centre = tr2_vertex.Parse(br);
            ret.CollisionSize = br.ReadInt32();
            ret.NumVertices = br.ReadInt16();
            ret.Vertices = br.ReadArray<tr2_vertex>(ret.NumVertices);
            ret.NumNormals = br.ReadInt16();
            if(ret.NumNormals >= 0)
            {
                ret.Normals = br.ReadArray<tr2_vertex>(ret.NumNormals);
            }
            else
            {
                ret.Lights = br.ReadArray<short>(-ret.NumNormals);
            }
            ret.NumTexturedRectangles = br.ReadInt16();
            ret.TexturedRectangles = br.ReadArray<tr2_face4>(ret.NumTexturedRectangles);
            ret.NumTexturedTriangles = br.ReadInt16();
            ret.TexturedTriangles = br.ReadArray<tr2_face3>(ret.NumTexturedTriangles);
            ret.NumColouredRectangles = br.ReadInt16();
            ret.ColouredRectangles = br.ReadArray<tr2_face4>(ret.NumColouredRectangles);
            ret.NumColouredTriangles = br.ReadInt16();
            ret.ColouredTriangles = br.ReadArray<tr2_face3>(ret.NumColouredTriangles);
            return ret;
        }
    }

    public enum tr2_staticmesh_Flags : ushort
    {
        Normal = 2,
        CanTravelThrough = 3
    }

    public struct tr2_staticmesh
    {
        public uint ObjectID; // Object Identifier (matched in Items[])
        public ushort Mesh; // mesh (offset into MeshPointers[])
        public tr2_vertex[][] BoundingBox; // First index is which one; second index is opposite corners 
        public tr2_staticmesh_Flags Flags;

        public static tr2_staticmesh Parse(BinaryReader br)
        {
            var ret = new tr2_staticmesh();
            ret.ObjectID = br.ReadUInt32();
            ret.Mesh = br.ReadUInt16();
            ret.BoundingBox = new[]
            {
                new[] {tr2_vertex.Parse(br), tr2_vertex.Parse(br)},
                new[] {tr2_vertex.Parse(br), tr2_vertex.Parse(br)}
            };
            ret.Flags = (tr2_staticmesh_Flags) br.ReadUInt16();
            return ret;
        }
    }
}
