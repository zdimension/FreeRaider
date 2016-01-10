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

    /// <summary>
    /// Vertex structure - this is how vertices are specified, using relative coordinates.  They are generally formed into lists, such that other entities (such as quads or triangles) can refer to them by simply using their index in the list.
    /// </summary>
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

    /// <summary>
    /// Rectangular (quad) face definition. Four vertices (the values are indices into the appropriate vertex list) and a texture (an index into the object-texture list) or colour (index into 8-bit palette or 16-bit palette).  If the rectangle is a coloured polygon (not textured), the .Texture element contains two indices: the low byte (.Texture & 0xff) is an index into the 256-colour palette, while the high byte (.Texture >> 8) is in index into the 16-bit palette, when present. A textured rectangle will have its vertices mapped onto all 4 vertices of an object texture, in appropriate correspondence.
    /// </summary>
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

    /// <summary>
    /// Three vertices (the values are indices into the appropriate vertex list) and a texture (an index into the object-texture list) or colour (index into 8-bit palette or 16-bit palette).  If the triangle is a coloured polygon (not textured), the .Texture element contains two indices: the low byte (.Texture & 0xff) is an index into the 256-colour palette, while the high byte (.Texture >> 8) is in index into the 16-bit palette, when present. A textured triangle will have its vertices mapped onto the first 3 vertices of an object texture, in appropriate correspondence.
    /// </summary>
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

    public struct tr2_textile8 // each byte represents a pixel whose colour is in the 8-bit palette
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

    /// <summary>
    /// These portals, sometimes called "doors", define the view from a room into another room. This can be through a "real" door, a window, or even some open area that makes the rooms look like one big room. Note that "rooms" here are really just areas; they aren't necessarily enclosed.  The portal structure below defines visibility portals, not an actual moveable door mesh, texture, or action (if any).  And if the portal is not properly oriented, the camera cannot "see" through it.
    /// </summary>
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

    /// <summary>
    /// All the geometry specified here is collisional geometry. Sectors are 1024 * 1024 (world coordinates). Floor and Ceiling are signed numbers of 256 units of height (relative to 0), e.g. Floor 0x04 corresponds to Y = 1024 in world coordinates. Note: this implies that, while X and Z can be quite large, Y is constrained to -32768..32512. Floor/Ceiling value of 0x81 is used to indicate impenetrable walls around the sector. Floor values are used by the game engine to determine what objects Lara can traverse and how. Relative steps of 1 (-256) can be walked up; steps of 2..7 (-512..-1792) can/must be jumped up; steps larger than 7 (-2048..-32768) cannot be jumped up (too tall). RoomAbove and RoomBelow indicate what neighboring rooms are in these directions; if RoomAbove is not "none", then the ceiling is a collisional portal to that room, while if RoomBelow is not "none", then the floor is a collisional portal to that room.
    /// </summary>
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

    /// <summary>
    /// X/Y/Z are in world coordinates.  Intensity1/Intensity2 are almost always equal. This lighting only affects externally-lit objects. Tomb Raider 1 has only the first of the paired Intensity and Fade values.
    /// </summary>
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

    /// <summary>
    /// This defines the vertices within a room. Room lighting is internal vertex lighting, except for necessarily external sources like flares; room ambient lights and point sources are ignored. Tomb Raider 1 has only the first of the two light values and lacks the rendering attributes.
    /// </summary>
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
        None = 0x0000,

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

    /// <summary>
    /// This indicates a point in space (Room.Vertices[room_sprite.Vertex]) and a sprite to display there (.Texture is an index into the sprite texture list).
    /// </summary>
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

    /// <summary>
    /// This is the geometry of the "room", including walls, floors, rocks, water, etc. It does not include objects that Lara can interact with (keyboxes, moveable blocks, moveable doors, etc.) The surfaces specified here are rendered surfaces.
    /// </summary>
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

    /// <summary>
    /// Positions and IDs of static meshes (e.g. skeletons, spiderwebs, furniture, trees). This is comparable to the <see cref="tr2_item"/> structure, except that static meshes typically have no animations and are confined to a single room.
    /// </summary>
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
        None = 0x000,
        FilledWithWater = 0x0001,
        PonytailBlownByTheWind = 0x0020,
        Unknown = 0x0100
    }

    /// <summary>
    /// Here's where all the room data come together. As it's stored in the file, the <see cref="tr2_room_info"/> structure comes first, followed by a <see cref="uint"/> NumDataWords, which specifies the number of 16-bit words to follow.  Those data words must be parsed in order to interpret and construct the variable-length arrays of vertices, meshes, doors, and sectors.
    /// </summary>
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

    /// <summary>
    /// The sign of the number of normals specifies which sort of lighting to use. If the sign is positive, then external vertex lighting is used, with the lighting calculated from the room's ambient and point-source lighting values. The latter appears to use a simple Lambert law for directionality: intensity is proportional to max((normal direction).(direction to source), 0). If the sign is negative, then internal vertex lighting is used, using the data included with the mesh.
    /// </summary>
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
        None = 0,
        Normal = 2,
        CanTravelThrough = 3
    }

    /// <summary>
    /// StaticMesh structure. This defines meshes that don't move (e.g. skeletons lying on the floor, spiderwebs, trees, statues, etc.). StaticMeshes have two bounding boxes; it is not clear why they have more than one. One could be the visibililty box, and one could be the collisional box, for instance; the former being used for visibility testing, and the latter for collision testing. 
    /// </summary>
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

    /// <summary>
    /// Moveable structure. This defines a list of contiguous meshes that comprise one object. This structure also points to the hierarchy and offsets of the meshes (MeshTree), and also to the animations used (Animation); these will be described in detail below. If the Animation index is -1, that means that the entity's animations are all generated by the engine; an example is Lara's ponytail. Some movables are really stationary, such as locks and the sky, and some are not rendered, such as "look at me" points to aim the camera at. 
    /// </summary>
    public struct tr2_moveable
    {
        public uint ObjectID; // Item Identifier (matched in Items[])
        public ushort NumMeshes; // number of meshes in this object
        public ushort StartingMesh; // starting mesh (offset into MeshPointers[])
        public uint MeshTree; // offset into MeshTree[]
        public uint FrameOffset; // byte offset into Frames[] (divide by 2 fro Frames[i])
        public ushort Animation; // offset into Animations[]

        public static tr2_moveable Parse(BinaryReader br)
        {
            return new tr2_moveable
            {
                ObjectID = br.ReadUInt32(),
                NumMeshes = br.ReadUInt16(),
                StartingMesh = br.ReadUInt16(),
                MeshTree = br.ReadUInt32(),
                FrameOffset = br.ReadUInt32(),
                Animation = br.ReadUInt16()
            };
        }
    }

    public enum tr2_item_Flags : ushort
    {
        None = 0,
        InitiallyInvisible = 0x0100,
        /// <summary>
        /// "open" or "activated"
        /// </summary>
        ActivationMask = 0x3e00
    }

    /// <summary>
    /// Items are instances of objects, which can be sprite sequences or movables. For an object to appear in a level, it must be referenced in the Items[] array. Multiple instances are possible (e.g. two identical tigers in different rooms are represented using two entries in Items[], one for each). The object ID is used to locate the appropriate sprite sequence or movable for the item.
    /// </summary>
    public struct tr2_item
    {
        public short ObjectID; // Object Identifier (matched in Moveables[], or SpriteSequences[], as appropriate)
        public short Room; // which room contains this item
        public int x; // item position in world coordinates
        public int y;
        public int z;
        public short Angle; // ((0xc000 >> 14) * 90) degrees
        public short Intensity1; // (constant lighting; -1 means use mesh lighting)
        public short Intensity2; // Like Intensity1, and almost always with the same value. [absent from TR1 data files] 
        public tr2_item_Flags Flags;

        public static tr2_item Parse(BinaryReader br, TR2LevelVersion ver = TR2LevelVersion.AutoDetect)
        {
            if (ver == TR2LevelVersion.AutoDetect) ver = TR2LevelParser.CurrentVersion;
            var ret = new tr2_item
            {
                ObjectID = br.ReadInt16(),
                Room = br.ReadInt16(),
                x = br.ReadInt32(),
                y = br.ReadInt32(),
                z = br.ReadInt32(),
                Angle = br.ReadInt16(),
                Intensity1 = br.ReadInt16()
            };
            if (ver != TR2LevelVersion.TR1) ret.Intensity2 = br.ReadInt16();
            ret.Flags = (tr2_item_Flags) br.ReadUInt16();
            return ret;
        }
    }

    /// <summary>
    /// These are "billboard" objects that are always rendered perpendicular to the view direction. These are used for text and explosion effects and similar things; they are also used for some scenery objects and pickup items, though this use gets less as one goes from TR1 to TR3. The various "Sides" below are the positions of the sprite sides relative to the sprite's overall position, measured in TR's world-coordinate units.
    /// </summary>
    public struct tr2_sprite_texture
    {
        public ushort Tile;
        public byte x;
        public byte y;
        public ushort Width; // actually (Width * 256) + 255 
        public ushort Height; // actually (Height * 256) + 255 
        public short LeftSide;
        public short TopSide;
        public short RightSide;
        public short BottomSide;

        public static tr2_sprite_texture Parse(BinaryReader br)
        {
            return new tr2_sprite_texture
            {
                Tile = br.ReadUInt16(),
                x = br.ReadByte(),
                y = br.ReadByte(),
                Width = br.ReadUInt16(),
                Height = br.ReadUInt16(),
                LeftSide = br.ReadInt16(),
                TopSide = br.ReadInt16(),
                RightSide = br.ReadInt16(),
                BottomSide = br.ReadInt16()
            };
        }
    }

    /// <summary>
    /// These are collections of sprites that are referred to as a group. The members of this group can be cycled through (animated sprites such as flames) or selected in other ways (text). Some sequences have only one member; this is done so as to access all the sprites in the same way.
    /// </summary>
    public struct tr2_sprite_sequence
    {
        public int ObjectID; // Item identifier (matched in Items[])
        public short NegativeLength; // negative of "how many sprites are in this sequence"
        public short Offset; // where (in sprite texture list) this sequence starts

        public static tr2_sprite_sequence Parse(BinaryReader br)
        {
            return new tr2_sprite_sequence
            {
                ObjectID = br.ReadInt32(),
                NegativeLength = br.ReadInt16(),
                Offset = br.ReadInt16()
            };
        }
    }

    /* 
     * MeshTree structure 
     * 
     * MeshTree[] is actually groups of four Int32s. The first one is a 
     * "flags" word; 
     *    bit 1 (0x0002) indicates "put the parent mesh on the mesh stack"; 
     *    bit 0 (0x0001) indicates "take the top mesh off of the mesh stack and use as the parent mesh" 
     * when set, otherwise "use the previous mesh are the parent mesh". 
     * When both are present, the bit-0 operation is always done before the bit-1 operation; in effect, read the stack but do not change it. 
     * The next three Int32s are X, Y, Z offsets of the mesh's origin from the parent mesh's origin. 
     */
    public struct tr2_meshtree
    {
        public int Coord;

        public static tr2_meshtree Parse(BinaryReader br)
        {
            return new tr2_meshtree
            {
                Coord = br.ReadInt32()
            };
        }
    }

    /* 
     * Animation structure. 
     * This describes each individual animation; these may be looped by specifying 
     * the next animation to be itself. In TR2 and TR3, one must be careful when 
     * parsing frames using the FrameSize value as the size of each frame, since 
     * an animation's frame range may extend into the next animation's frame range, 
     * and that may have a different FrameSize value. 
     */
    public struct tr2_animation
    {
        public uint FrameOffset; // byte offset into Frames[] (divide by 2 for Frames[i])
        public byte FrameRate; // Engine ticks per frame
        public byte FrameSize; // Number of Int16 in Frames[] used by this animation
        public ushort StateID;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] public byte[] Unknown2;
        public ushort FrameStart; // first frame in this animation
        public ushort FrameEnd; // last frame in this animation (numframes = (End - Start) + 1)
        public ushort NextAnimation;
        public ushort NextFrame;
        public ushort NumStateChanges;
        public ushort StateChangeOffset; // offset into StateChanges[]
        public ushort NumAnimCommands; // How many of them to use
        public ushort AnimCommand; // offset into AnimCommands[]

        public static tr2_animation Parse(BinaryReader br)
        {
            return new tr2_animation
            {
                FrameOffset = br.ReadUInt32(),
                FrameRate = br.ReadByte(),
                FrameSize = br.ReadByte(),
                StateID = br.ReadUInt16(),
                Unknown2 = br.ReadByteArray(8),
                FrameStart = br.ReadUInt16(),
                FrameEnd = br.ReadUInt16(),
                NextAnimation = br.ReadUInt16(),
                NextFrame = br.ReadUInt16(),
                NumStateChanges = br.ReadUInt16(),
                StateChangeOffset = br.ReadUInt16(),
                NumAnimCommands = br.ReadUInt16(),
                AnimCommand = br.ReadUInt16()
            };
        }
    }

    /* 
     * State Change structure 
     * Each one contains the state to change to and which animation dispatches 
     * to use; there may be more than one, with each separate one covering a different 
     * range of frames. 
     */
    public struct tr2_state_change
    {
        public ushort StateID;
        public ushort NumAnimDispatches; // number of ranges (seems to always be 1..5)
        public ushort AnimDispatch; // Offset into AnimDispatches[]

        public static tr2_state_change Parse(BinaryReader br)
        {
            return new tr2_state_change
            {
                StateID = br.ReadUInt16(),
                NumAnimDispatches = br.ReadUInt16(),
                AnimDispatch = br.ReadUInt16()
            };
        }
    }

    /* 
     * Animation Dispatch structure 
     * This specifies the next animation and frame to use; these are associated 
     * with some range of frames. This makes possible such specificity as one 
     * animation for left foot forward and another animation for right foot forward. 
     */
    public struct tr2_anim_dispatch
    {
        public short Low; // Lowest frame that uses this range
        public short High; // Highest frame (+1?) that uses this range
        public short NextAnimation; // Animation to dispatch to
        public short NextFrame; // Frame offset to dispatch to

        public static tr2_anim_dispatch Parse(BinaryReader br)
        {
            return new tr2_anim_dispatch
            {
                Low = br.ReadInt16(),
                High = br.ReadInt16(),
                NextAnimation = br.ReadInt16(),
                NextFrame = br.ReadInt16()
            };
        }
    }

    /* 
     * AnimCommand structure 
     * These are various commands associated with each animation; they are 
     * called "Bone1" in some documentation. They are varying numbers of Int16's 
     * packed into an array; the first of each set is the opcode, which determines 
     * how operand Int16's follow it. Some of them refer to the whole animation 
     * (jump and grab points, etc.), while others of them are associated with 
     * specific frames (sound, bubbles, etc.). 
     */
    public struct tr2_anim_command
    {
        public short Value;

        public static tr2_anim_command Parse(BinaryReader br)
        {
            return new tr2_anim_command
            {
                Value = br.ReadInt16()
            };
        }
    }
    /*
    Here are all the AnimCommand opcodes and their operands: 
    // 1: 3 operands. Position reference: (x,y,z); found in grab and block-move animations 
    // 2: 2 operands. Position reference on surface for jumping: (x,z) for horizontal and (y,z) for vertical surfaces(?) 
    // 3: No operands. Not clear; occurs in animations that are "slaved" to other animations, such as Lara throwing switches or moving blocks. 
    // 4: No operands. Not clear; occurs in some death and settling-down animations, but not all. 
    // 5: 2 operands. The first one is a frame number, and the second one is the ID of the sound to play at that frame (internal sound index). 
    In TR2 and TR3, one of the sound indices two highest bits may be set; when they are, their meanings are
    
    0x4000 -- play this sound when on dry land (example: footsteps) 
    0x8000 -- play this sound when in water (example: running through shallow water)
    
    // 6: 2 operands. The first one is a frame number, and the second one is some miscellaneous action. 
    //     0: Occurs in flipping-over animations; freeze camera at current position until end of animation? 
    //     3: Make bubble 
    //     12: Temporarily stop responding to controls? 
    //     etc. 
    14 and 15: Some kind of camera control? 
    18: ? 
    19: ? 
    20: Lara changing clothes (using a different Lara model) 
    21: ? 
    22: ? 
    23: Hide object 
    24: Show object 
    26: Some kind of camera control? 
    TR3 has additional ones, such as 
    -32736 = 0x8000 + 32 
    32 
    16416 = 0x4000 + 32 
    */

    /* 
     * Frame structure. 
     * 
     * Frames indicate how composite meshes are positioned and rotated. They work 
     * in conjunction with Animations[] and MeshTree[]. A given frame has the following 
     * format: 
     * Int16 BB1x, BB1y, BB1z // bounding box (low) 
     * Int16 BB2x, BB2y, BB2z // bounding box (high) 
     * Int16 OffsetX, OffsetY, OffsetZ // starting offset for this moveable 
     * (TR1 ONLY: Int16 NumValues // number of angle sets to follow; these start with the first mesh, and meshes without angles get zero angles.) 
     * (TR2/3: NumValues is implicitly NumMeshes (from moveable)) 
     * What follows next is a list of angle sets. In TR2/3, an angle set can 
     * specify either one or three axes of rotation. If either of the high two 
     * bits (0xc000) of the first angle UInt16 are set, it's one axis: only one 
     * UInt16, low 10 bits (0x03ff), scale is 0x100 ::= 90 degrees; the high two 
     * bits are interpreted as follows: 0x4000 ::= X only, 0x8000 ::= Y only, 
     * 0xC000 ::= Z only. 
     * If neither of the high bits are set, it's a three-axis rotation. The next 
     * 10 bits (0x3ff0) are the X rotation, the next 10 (including the following 
     * UInt16) (0x000f, 0xfc00) are the Y rotation, the next 10 (0x03ff) are the 
     * Z rotation, same scale as before (0x100 ::= 90 degrees). 
     * Rotations are performed in Y, X, Z order. 
     * TR1 ONLY: All angle sets are two words and interpreted like the two-word 
     * sets in TR2/3, EXCEPT that the word order is reversed. 
     */

    public struct tr2_box_TR1
    {
        public int Zmin;
        public int Zmax;
        public int Xmin;
        public int Xmax;
        public short TrueFloor; // Y value (no scaling) 
        public short OverlapIndex; // index into Overlaps[]. The high bit is sometimes set; this occurs in front of swinging doors and the like. 

        public static tr2_box_TR1 Parse(BinaryReader br)
        {
            return new tr2_box_TR1
            {
                Zmin = br.ReadInt32(),
                Zmax = br.ReadInt32(),
                Xmin = br.ReadInt32(),
                Xmax = br.ReadInt32(),
                TrueFloor = br.ReadInt16(),
                OverlapIndex = br.ReadInt16()
            };
        }
    }

    public struct tr2_box_AboveTR1
    {
        public byte Zmin;
        public byte Zmax;
        public byte Xmin;
        public byte Xmax;
        public short TrueFloor; // Y value (no scaling) 
        public short OverlapIndex; // index into Overlaps[]. The high bit is sometimes set; this occurs in front of swinging doors and the like. 

        public static tr2_box_AboveTR1 Parse(BinaryReader br)
        {
            return new tr2_box_AboveTR1
            {
                Zmin = br.ReadByte(),
                Zmax = br.ReadByte(),
                Xmin = br.ReadByte(),
                Xmax = br.ReadByte(),
                TrueFloor = br.ReadInt16(),
                OverlapIndex = br.ReadInt16()
            };
        }
    }

    public struct tr2_sound_source
    {
        public int x; // absolute X position of sound source (world coordinates) 
        public int y; // absolute Y position of sound source (world coordinates) 
        public int z; // absolute Z position of sound source (world coordinates) 
        public ushort SoundID; // internal sound index 
        public ushort Flags; // 0x40, 0x80, or 0xc0 

        public static tr2_sound_source Parse(BinaryReader br)
        {
            return new tr2_sound_source
            {
                x = br.ReadInt32(),
                y = br.ReadInt32(),
                z = br.ReadInt32(),
                SoundID = br.ReadUInt16(),
                Flags = br.ReadUInt16()
            };
        }
    }

    public struct tr2_sound_details
    {
        public short Sample; // (index into SampleIndices)
        public short Volume;
        public short Unknown1; // sound range? (distance at which this sound can be heard?)
        public short Unknown2; // Bits 8-15: priority?, Bits 2-7: number of sound samples in this group, Bits 0-1: channel number?

        public static tr2_sound_details Parse(BinaryReader br)
        {
            return new tr2_sound_details
            {
                Sample = br.ReadInt16(),
                Volume = br.ReadInt16(),
                Unknown1 = br.ReadInt16(),
                Unknown2 = br.ReadInt16()
            };
        }
    }

    /* 
     * Object-texture vertex structure. It specifies a vertex location in textile coordinates. 
     * The Xpixel and Ypixel are the actual coordinates of the vertex's pixel. 
     * The Xcoordinate and Ycoordinate values depend on where the other vertices 
     * are in the object texture. And if the object texture is used to specify 
     * a triangle, then the fourth vertex's values will all be zero. 
     */
    public struct tr2_object_texture_vert
    {
        public byte Xcoordinate; // 1 if Xpixel is the low value, 255 if Xpixel is the high value in the object texture 
        public byte Xpixel;
        public byte Ycoordinate; // 1 if Ypixel is the low value, 255 if Ypixel is the high value in the object texture 
        public byte Ypixel;

        public static tr2_object_texture_vert Parse(BinaryReader br)
        {
            return new tr2_object_texture_vert
            {
                Xcoordinate = br.ReadByte(),
                Xpixel = br.ReadByte(),
                Ycoordinate = br.ReadByte(),
                Ypixel = br.ReadByte()
            };
        }
    }

    /* 
     * Object texture structure. 
     * These, thee contents of ObjectTextures[], are used for specifying texture 
     * mapping for the world geometry and for mesh objects. 
     */
    public struct tr2_object_texture
    {
        public ushort Attribute;
        public ushort Tile; // index into textile list
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public tr2_object_texture_vert[] Vertices;
        
        public tr2_object_texture Parse(BinaryReader br)
        {
            return new tr2_object_texture
            {
                Attribute = br.ReadUInt16(),
                Tile = br.ReadUInt16(),
                Vertices = br.ReadArray<tr2_object_texture_vert>(4)
            };
        }
    }

    /// <summary>
    /// These are positions to switch the camera to; the camera gets switched to one of these as specified in the floordata, which also specify what to look at, how long to switch, and whether to do so only once.
    /// </summary>
    public struct tr2_camera
    {
        public int x;
        public int y;
        public int z;
        public short Room;
        public ushort Unknown1; // correlates to Boxes[]? Zones[]?

        public static tr2_camera Parse(BinaryReader br)
        {
            return new tr2_camera
            {
                x = br.ReadInt32(),
                y = br.ReadInt32(),
                z = br.ReadInt32(),
                Room = br.ReadInt16(),
                Unknown1 = br.ReadUInt16()
            };
        }
    }

    /// <summary>
    /// These are camera positionings for cutscenes. All the entity animations are specified separately, and it is not clear where there is any syncing between these frames and any of the animations.
    /// </summary>
    public struct tr2_cinematic_frame
    {
        public short rotY; // rotation about Y axis, +/- 32767 == +/- 180 degrees 
        public short rotZ; // rotation about Z axis, +/- 32767 == +/- 180 degrees 
        public short rotZ2; // seems to work a lot like rotZ; I haven't yet been able to differentiate them 
        public short posZ; // camera position relative to something (target? Lara? room origin?).  pos* are _not_ in world coordinates. 
        public short posY; // camera position relative to something (see posZ) 
        public short posX; // camera position relative to something (see posZ) 
        public short Unknown; // changing this can cause a runtime error 
        public short rotX; // rotation about X axis, +/- 32767 == +/- 180 degrees 

        public static tr2_cinematic_frame Parse(BinaryReader br)
        {
            return new tr2_cinematic_frame
            {
                rotY = br.ReadInt16(),
                rotZ = br.ReadInt16(),
                rotZ2 = br.ReadInt16(),
                posZ = br.ReadInt16(),
                posY = br.ReadInt16(),
                posX = br.ReadInt16(),
                Unknown = br.ReadInt16(),
                rotX = br.ReadInt16()
            };
        }
    }
}
