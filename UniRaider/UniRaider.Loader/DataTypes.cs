using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace UniRaider.Loader
{
    public enum TR2LevelVersion : uint
    {
        AutoDetect = uint.MinValue,
        TR1,
        TR1UnfinishedBusiness,
        TR2,
        TR3,
        TR4,
        TR5,
        Unknown = uint.MaxValue
        /*TR1 = 0x00000020,
        TR2 = 0x0000002d,
        TR3 = 0xFF080038,
        TR3_Alt = 0xFF180038,
        TR4 = 0x00345254,*/
    }

    public static class GameVersionHelper
    {
        public static TR2LevelVersion ParseVersion(BinaryReader br, string fext)
        {
            fext = fext.ToUpper();
            var check = br.ReadBytes(4);
            var ver = check[0] | (uint)(check[1] << 8) | (uint)(check[2] << 16) | (uint)(check[3] << 24);
            switch (fext)
            {
                case ".PHD":
                    if (ver == 0x00000020)
                        return TR2LevelVersion.TR1;
                    break;
                case ".TUB":
                    if (ver == 0x00000020)
                        return TR2LevelVersion.TR1UnfinishedBusiness;
                    break;
                case ".TR2":
                    if (ver == 0x0000002D)
                        return TR2LevelVersion.TR2;
                    else if ((check[0] == 0x38 || check[0] == 0x34) &&
                             (check[1] == 0x00) &&
                             (check[2] == 0x18 || check[2] == 0x08) &&
                             (check[3] == 0xFF))
                        return TR2LevelVersion.TR3;
                    break;
                case ".TR4":
                    if (ver == 0x00345254 || ver == 0x63345254 || ver == 0xFFFFFFF0)
                        return TR2LevelVersion.TR4;
                    break;
                case ".TRC":
                    if (ver == 0x00345254)
                        return TR2LevelVersion.TR5;
                    break;
            }
            return TR2LevelVersion.Unknown;
        }
    }

    #region Structures
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
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public ushort[] Vertices;
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
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public ushort[] Vertices;
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
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256 * 256)]
        public byte[] Tile;

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
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256 * 256)]
        public ushort[] Tile;

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
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public tr2_vertex[] Vertices; // corners of this portal

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

        public static tr2_room_staticmesh Parse(BinaryReader br)
        {
            return new tr2_room_staticmesh
            {
                x = br.ReadUInt32(),
                y = br.ReadUInt32(),
                z = br.ReadUInt32(),
                Rotation = br.ReadUInt16(),
                Intensity1 = br.ReadUInt16(),
                Intensity2 = br.ReadUInt16(),
                ObjectID = br.ReadUInt16()
            };
        }
    }

    #endregion

    public interface ILevel
    {
        TR2LevelVersion GameVersion { get; set; }
    }
}
