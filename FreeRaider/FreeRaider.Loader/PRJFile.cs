using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace FreeRaider.Loader
{
    // Main source: aktrekker's PRJ format document. So many thanks to him.
    // Secondary source: http://www.trsearch.org/v5/wiki/Source:PRJ.h
    public unsafe class PRJFile
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct PRJ_TextInfo
        {
            /// <summary>
            /// The pixel offset of the left side of the texture within the TGA. This
            /// only permits the texture file to be 256 pixels wide at most. For full
            /// tiles, it must be a multiple of 64 (0, 64, 128, 192). For partial tiles,
            /// it may be any multiple of 16.
            /// </summary>
            public byte X;
            /// <summary>
            /// The pixel offset of the top of the texture within the TGA. The maximum
            /// height allowed for a TGA file is 4096 (for width of 256). Since 512 pixel
            /// wide TGA files are converted to 256, that makes 4096 the true maximum
            /// height.
            /// This offset can be at most 4032 for full tiles, or 4080 for partial tiles.
            /// For full tiles, it must be a multiple of 64. For partial tiles, it may be
            /// any multiple of 16.
            /// </summary>
            public byte Y;
            public ushort Tile;
            public byte FlipX;
            /// <summary>
            /// This is the pixel offset of the rightmost pixel within the tile. For full
            /// tiles, it must be 63. For partial tiles it must be 15, 31, 47, or 63. It
            /// is basically the pixel width of the texture minus 1. It cannot be set so
            /// that the texture would cross a tile boundary.
            /// </summary>
            public byte XSize;
            public byte FlipY;
            /// <summary>
            /// This is the pixel offset of the bottom pixel within the tile. For full
            /// tiles, it must be 63. For partial tiles it must be 15, 31, 47, or 63. It
            /// is basically the pixel height of the texture minus 1. It cannot be set so
            /// that the texture would cross a tile boundary.
            /// </summary>
            public byte YSize;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct PRJ_AnimText
        {
            /// <summary>
            /// This is ZERO if the animation range is not used. It is 1 if the animation
            /// range is being used.
            /// </summary>
            public uint Defined;
            /// <summary>This is the tile number of the first tile in the animation range.</summary>
            public uint FirstTile;
            /// <summary>This is the tile number of the last tile in the animation range.</summary>
            public uint LastTile;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct PRJ_Color4
        {
            public byte R;
            public byte G;
            public byte B;
            public byte A;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct PRJ_Camera
        {
            public short XPos;
            public short ZPos;
            public int Unknown1;
            public int YPos;
            public sbyte FOV;
            public sbyte CamID;
            public int Timer;
            public int WorldZPos;
            public int WorldYPos;
            public int WorldXPos;
            public short XRot;
            public short YRot;
            public short ZRot; // Roll
            public short Speed; // Speed*655
            public short Flags; // The buttons 0-15 in the camera settings. Each bit is a button.
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct PRJ_Sink
        {
            /// <summary>
            /// This is the BLOCK position of the sink in the X axis RELATIVE to the
            /// West side of the room. 1-18.
            /// </summary>
            public short XPos;
            /// <summary>
            /// This is the BLOCK position of the sink in the Z axis RELATIVE to the
            /// North side of the room. 1-18.
            /// </summary>
            public short ZPos;
            /// <summary>This is always 1 for sinks.</summary>
            public short XSize;
            /// <summary>This is always 1 for sinks.</summary>
            public short ZSize;
            /// <summary>
            /// This is how many clicks above the floor that the sink was placed. If you
            /// raise or lower the floor block, the sink keeps the same relative
            /// position to the floor. Objects can be positioned in 1/2 click increments
            /// (128 pixels). Always positive.
            /// </summary>
            public ushort YPos;
            /// <summary>The room number where this sink is.</summary>
            public ushort Room;
            /// <summary>NOT USED</summary>
            public ushort Slot;
            /// <summary>This is always ZERO for sinks.</summary>
            public ushort Timer;
            /// <summary>
            /// The orientation of the sink. This is when you right click on it and it
            /// moves from the center of the block to one of 4 positions around the
            /// outside edge. It is actually placed 1/2 click in from the edge of the
            /// block.
            /// </summary>
            public PRJObjectOrientation Orientation;
            /// <summary>
            /// The pixel coordinate of the sink on the Z axis, relative to the North
            /// side of the room. It is equal to <see cref="ZPos"/> * 1024, with adjustment
            /// made for the orientation.
            /// </summary>
            public int WorldZPos;
            /// <summary>
            /// The pixel coordinate of the sink on the Y axis. This is within the
            /// entire world space of 256 clicks. This is negative going UP.
            /// </summary>
            public int WorldYPos;
            /// <summary>
            /// The pixel coordinate of the sink on the X axis, relative to the West
            /// side of the room. It is equal to <see cref="XPos"/> * 1024, with adjustment made
            /// for the orientation.
            /// </summary>
            public int WorldXPos;

            public ushort Unknown;
            public PRJObjectFacing Facing;
            public short Roll;
            public ushort Speed;
            public PRJObjectOCB OCB;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct PRJ_Light
        {
            /*/// <summary>
            /// This is the BLOCK position of the light in the X axis RELATIVE to the
            /// West side of the room. 1-18.
            /// </summary>
            public short XPos;
            /// <summary>
            /// This is the BLOCK position of the object in the Z axis RELATIVE to the
            /// North side of the room. 1-18.
            /// </summary>
            public short ZPos;
            public short XSize;
            public short ZSize;
            /// <summary>
            /// This is how many clicks above the floor that the light was placed. If you
            /// raise or lower the floor block, the light keeps the same relative
            /// position to the floor. Lights can be positioned in 1/2 click increments
            /// (128 pixels). It is positive.
            /// </summary>
            public ushort YPos;
            /// <summary>The room number where this light is.</summary>
            public ushort Room;
            /// <summary>
            /// This is the light index number. Only 768 total lights are allowed, and
            /// this index keeps track of them. 0-767.
            /// </summary>
            public ushort Slot;
            public ushort Timer;
            /// <summary>
            /// The orientation of the light. This is when you right click on it and it
            /// moves from the center of the block to one of 4 positions around the
            /// outside edge. It is actually placed 1/2 click in from the edge of the
            /// block.
            /// </summary>
            public PRJObjectOrientation Orientation; // Where the light is. 0 is floor, 1 is ceiling and other all wall types (dark green, green and light green sectors).
            /// <summary>
            /// The pixel coordinate of the light on the Z axis, relative to the North
            /// side of the room. It is equal to <see cref="ZPos"/> * 1024, with adjustment
            /// made for the orientation.
            /// </summary>
            public int WorldZPos;
            /// <summary>
            /// The pixel coordinate of the light on the Y axis. This is within the
            /// entire world space of 256 clicks. This is negative going UP.
            /// </summary>
            public int WorldYPos;
            /// <summary>
            /// The pixel coordinate of the light on the X axis, relative to the West
            /// side of the room. It is equal to <see cref="XPos"/> * 1024, with adjustment made
            /// for the orientation.
            /// </summary>
            public int WorldXPos;
            public ushort Unknown;
            public PRJObjectFacing Facing;
            public short Roll;
            public ushort Speed;
            public PRJObjectOCB OCB;*/
            /// <summary>
            /// The intensity of the light. Used for ALL light types. This is a signed 
            /// 16-bit binary number, with an assumed decimal point after the 
            /// high-order 3 bits. The low-order 13 bits are decimal positions, in 
            /// standard binary notation. That is, .1 is 1/2, .01 is 1/4, .001 is 1/8, 
            /// etc. The low-order byte is rarely used, because the decimal places are so
            /// insignificant. The range of this value is basically -4 thru +3.999.
            /// Ranges vary by light type.
            /// <para/>
            /// Light       0 - 1 (0x0000 - 0x1fff)<para/>
            /// Shadow      -1 - 0 (0xe000 - 0xffff, 0x0000)<para/>
            /// Sun         0 - 1 (0x0000 - 0x1fff)<para/>
            /// Spot        0 - 1 (0x0000 - 0x1fff)<para/>
            /// Effect	    -4 - 3.99 (0x8000 - 0xffff, 0x0000 - 0x7fff)<para/>
            /// Fog Bulb	0 - 1 (0x0000 - 0x1fff)<para/>
            /// </summary>
            public short Intensity;
            /// <summary>
            /// This is indeed a floating-point value. But it is not quite the value
            /// displayed by the LE. When setting this value, it increments by 1/32 at a
            /// time (decimal .03125). A value of 32 "clicks" represents the integer
            /// 1.00. This value is then multiplied by 32. To convert this value to what
            /// is displayed by the LE, use the formula (<see cref="In"/> / 32) * .03125.
            /// The range is 0 - 32, but it must be less than <see cref="Out"/>.
            /// The exception to this is for Spot lights. In this case the value is in
            /// the range 0 - 360, and it is the exact value (no conversion required).
            /// This is not used for Sun and Effect lights.
            /// </summary>
            public float In;
            /// <summary>
            /// This is also a floating-point value. It is the same format as <see cref="In"/>. The
            /// range is 0 - 32, but it must be greater than <see cref="In"/>.
            /// The exception to this is for Spot lights. In this case the value is in
            /// the range 0 - 360, and it is the exact value (no conversion required).
            /// This is not used for Sun and Effect lights.
            /// </summary>
            public float Out;
            /// <summary>
            /// This is a floating-point integer value in the range 0 - 360. It is only
            /// used for Sun and Spot lights.
            /// </summary>
            public float X;
            /// <summary>
            /// This is a floating-point integer value in the range 0 - 360. It is only
            /// used for Sun and Spot lights.
            /// </summary>
            public float Y;
            /// <summary>
            /// This is a floating-point value in the range 0 - 32. It is only used for
            /// Spot lights.
            /// </summary>
            public float Len;
            /// <summary>
            /// This is a floating-point value in the range 0 - 64. It is only used for
            /// Spot lights.
            /// </summary>
            public float Cut;
            /// <summary>The red part of the light color.</summary>
            public byte R;
            /// <summary>The green part of the light color.</summary>
            public byte G;
            /// <summary>The Blue part of the light color.</summary>
            public byte B;
            /// <summary>This tells if the light is on or off. ZERO is OFF, 1 is ON.</summary>
            public byte On;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct PRJ_Sound
        {
            /// <summary>
            /// This is the BLOCK position of the sound in the X axis RELATIVE to the
            /// West side of the room. 1-18.
            /// </summary>
            public short XPos;
            /// <summary>
            /// This is the BLOCK position of the sound in the Z axis RELATIVE to the
            /// North side of the room. 1-18.
            /// </summary>
            public short ZPos;
            /// <summary>This is always 1 for sounds.</summary>
            public short XSize;
            /// <summary>This is always 1 for sounds.</summary>
            public short ZSize;
            /// <summary>
            /// This is how many clicks above the floor that the sound was placed. If you
            /// raise or lower the floor block, the sound keeps the same relative
            /// position to the floor. Sounds can be positioned in 1/2 click increments
            /// (128 pixels). Always positive.
            /// </summary>
            public ushort YPos;
            /// <summary>The room number where this sound is.</summary>
            public ushort Room;
            /// <summary>
            /// This is the sound slot number identifying which sound to play. It is one
            /// of the 370 sound effect numbers.
            /// </summary>
            public ushort Slot;
            /// <summary>This is always ZERO for sounds.</summary>
            public ushort Timer;
            /// <summary>
            /// The orientation of the sound. This is when you right click on it and it
            /// moves from the center of the block to one of 4 positions around the
            /// outside edge. It is actually placed 1/2 click in from the edge of the
            /// block.
            /// </summary>
            public PRJObjectOrientation Orientation;
            /// <summary>
            /// The pixel coordinate of the object on the Z axis, relative to the North
            /// side of the room. It is equal to <see cref="ZPos"/> * 1024, with adjustment
            /// made for the orientation.
            /// </summary>
            public int WorldZPos;
            /// <summary>
            /// The pixel coordinate of the object on the Y axis. This is within the
            /// entire world space of 256 clicks. This is negative going UP.
            /// </summary>
            public int WorldYPos;
            /// <summary>
            /// The pixel coordinate of the object on the X axis, relative to the West
            /// side of the room. It is equal to <see cref="XPos"/> * 1024, with adjustment made
            /// for the orientation.
            /// </summary>
            public int WorldXPos;
            public ushort Unknown;
            public PRJObjectFacing Facing;
            public short Roll;
            public ushort Speed;
            public PRJObjectOCB OCB;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct PRJ_Object2
        {
            public PRJObjectType ObjectCode;
            // Check the object code (see table in this document) and read the needed data:
            public PRJ_Object2Data ObjectData;
            public PRJ_Light LightData; // If it's a light (all types, including Fog Bulbs)
            /*public PRJ_Camera CameraData; // If it's a camera (all types)
            public PRJ_Sink SinkData; // If it's a sink object
            public PRJ_Sound SoundData;*/
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct PRJ_Object2Data
        {
            /// <summary>
            /// This is the BLOCK position of the object in the X axis RELATIVE to the
            /// West side of the room. 1-18.
            /// </summary>
            public short XPos;
            /// <summary>
            /// This is the BLOCK position of the object in the Z axis RELATIVE to the
            /// North side of the room. 1-18.
            /// </summary>
            public short ZPos;
            public short XSize;
            public short ZSize;
            /// <summary>
            /// This is how many clicks above the floor that the object was placed. If you
            /// raise or lower the floor block, the object keeps the same relative
            /// position to the floor. Lights can be positioned in 1/2 click increments
            /// (128 pixels). It is positive.
            /// </summary>
            public ushort YPos;
            /// <summary>The room number where this object is.</summary>
            public ushort Room;
            /// <summary>
            /// This is the object index number.
            /// Field not used for sinks.<para/>
            /// For cameras, it contains the following properties (with bit-masks):<para/>
            /// - CAMERA_VALUE_SEQ: 0xE000  1110000000000000<para/>
            /// - CAMERA_VALUE_NUM: 0x1F00  0001111100000000<para/>
            /// - CAMERA_VALUE_FOV: 0x00FF  0000000011111111<para/>
            /// </summary>
            public ushort Slot;
            /// <summary>
            /// This is always ZERO for sounds and sinks.
            /// This is the timer value for the camera.
            /// Not used for lights.
            /// </summary>
            public ushort Timer;
            /// <summary>
            /// The orientation of the object. This is when you right click on it and it
            /// moves from the center of the block to one of 4 positions around the
            /// outside edge. It is actually placed 1/2 click in from the edge of the
            /// block.
            /// </summary>
            public PRJObjectOrientation Orientation; // Where the light is. 0 is floor, 1 is ceiling and other all wall types (dark green, green and light green sectors).
            /// <summary>
            /// The pixel coordinate of the object on the Z axis, relative to the North
            /// side of the room. It is equal to <see cref="ZPos"/> * 1024, with adjustment
            /// made for the orientation.
            /// </summary>
            public int WorldZPos;
            /// <summary>
            /// The pixel coordinate of the object on the Y axis. This is within the
            /// entire world space of 256 clicks. This is negative going UP.
            /// </summary>
            public int WorldYPos;
            /// <summary>
            /// The pixel coordinate of the object on the X axis, relative to the West
            /// side of the room. It is equal to <see cref="XPos"/> * 1024, with adjustment made
            /// for the orientation.
            /// </summary>
            public int WorldXPos;
            public ushort Unknown;
            /// <summary>
            /// The angle the object is facing. You can rotate the object in 8 different
            /// angles. Values are below. This does not refer to the direction an object
            /// is LOOKING. When first placed in the map, the object is considered facing
            /// North, no matter which way it is looking.
            /// </summary>
            public PRJObjectFacing Facing;
            /// <summary>
            /// The roll value for the camera. No special coding, just straight binary in
            /// the range -180 thru +180 inclusive.
            /// Unused for others.
            /// </summary>
            public short Roll;
            /// <summary>The speed for the camera. Unused for others.</summary>
            public ushort Speed;
            /// <summary>The OCB switches for the object.</summary>
            public PRJObjectOCB OCB;
        }

        public enum PRJSectorTextureType : ushort
        {
            /// <summary>No texture or color has been applied yet.</summary>
            None = 0x0000,
            /// <summary>A color is applied, using the palette under the 3D display.</summary>
            Color = 0x0003,
            /// <summary>A texture is applied from the TGA.</summary>
            Tile = 0x0007
        }
        [Flags]
        public enum PRJSectorTextureFlags : byte
        {
            /// <summary>This flag is set for a flipped texture.</summary>
            Flipped = 0x80,
            /// <summary>This flag is set for a transparent texture.</summary>
            Transparent = 0x08,
            /// <summary>This flag is set for a double sided texture.</summary>
            DoubleSided = 0x04,
            /// <summary>
            /// These 2 low-order bits are the high-order bits of the texture index.
            /// Multiply the value in these bits by 256 and add <see cref="PRJ_Sector_Texture.Index"/> to get the full
            /// texture number. This allows up to 1024 textures. The first 256 are
            /// reserved for the tiles in the TGA file. The rest are for partial-tile
            /// textures.
            /// </summary>
            PartialTile = 0x03
        }

        public enum PRJSectorTextureRotation : byte
        {
            None = 0,
            Rot90 = 1,
            Rot180 = 2,
            Rot270 = 3
        }

        public enum PRJSectorTextureTriangle : byte
        {
            NorthWest = 0,
            NorthEast = 1,
            SouthEast = 2,
            SouthWest = 3
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct PRJ_Sector_Texture
        {
            /// <summary>This describes the type of texture applied to the surface.</summary>
            public PRJSectorTextureType Type;
            /// <summary>
            /// If a color is applied, this is the index of the color in the TR color palette.
            /// If a texture is applied, this is the low 8-bits of the texture number.
            /// </summary>
            public byte Index;
            /// <summary>Some flags about the texture.</summary>
            public PRJSectorTextureFlags Flags;
            /// <summary>
            /// This indicates the amount of rotation for the texture. I think they
            /// indicate clockwise rotation, but I haven’t checked.
            /// </summary>
            public PRJSectorTextureRotation Rotation;
            /// <summary>
            /// This indicates which triangular part of the texture is to be used for
            /// triangular surfaces (split floor or ceiling faces). This is the part
            /// outlined in green in the texture display.
            /// </summary>
            public PRJSectorTextureTriangle Triangle;
            public ushort Unknown;
        }

        public enum PRJSectorType : ushort
        {
            /// <summary>
            /// This is a standard floor block. It has a ceiling part and a floor part.
            /// Both parts are present.
            /// </summary>
            BlockFloor = 0x0001,
            /// <summary>This is a standard wall block. It extends from floor to ceiling.</summary>
            BlockWall = 0x000e,
            /// <summary>This is a floor block that has a door in both the ceiling and floor.</summary>
            BlockDoor = 0x0007,
            /// <summary>This is a floor block that has a door in the floor only.</summary>
            BlockFloorDoor = 0x0003,
            /// <summary>This is a floor block that has a door in the ceiling only.</summary>
            BlockCeilingDoor = 0x0005,
            /// <summary>This is a standard panel (gray square).</summary>
            PanelWall = 0x001e,
            /// <summary>This is a panel (gray square) that is a door.</summary>
            PanelDoor = 0x0006
        }

        [Flags]
        public enum PRJSectorFlags1 : ushort
        {
            /// <summary>This flag is set if this is a monkey block.</summary>
            Monkey = 0x4000,
            /// <summary>
            /// This flag is set if Toggle Opacity 2 is on. For this to work, the Toggle
            /// Opacity (<see cref="Opacity"/>) flag must ALSO be set.
            /// </summary>
            Opacity2 = 0x1000,
            /// <summary>
            /// Not sure exactly. But it is only set if this block is part of a floor
            /// door. One more thing I have to figure out.
            /// </summary>
            FloorDoor = 0x0800,
            /// <summary>This flag is set if climb is turned on for the north of the block.</summary>
            ClimbNorth = 0x0200,
            /// <summary>This flag is set if climb is turned on for the west of the block.</summary>
            ClimbWest = 0x0100,
            /// <summary>This flag is set if climb is turned on for the south of the block.</summary>
            ClimbSouth = 0x0080,
            /// <summary>This flag is set if climb is turned on for the east of the block.</summary>
            ClimbEast = 0x0040,
            /// <summary>This flag is turned on if this is a box block.</summary>
            Box = 0x0020,
            /// <summary>This flag is turned on if this is a death block.</summary>
            Death = 0x0010,
            /// <summary>This flag is turned on for Toggle Opacity AND for Toggle Opacity 2.</summary>
            Opacity = 0x0008,
            /// <summary>
            /// Not exactly sure. It is only set if the block is part of a floor door,
            /// and you are able to fall thru to the room below. One more thing I have to
            /// figure out.
            /// </summary>
            Fall = 0x0002,
            /// <summary>This flag is set if there is a trigger on this block.</summary>
            Trigger = 0x0001
        }
        public enum PRJSectorCorner
        {
            SouthWest = 0,
            NorthWest = 1,
            NorthEast = 2,
            SouthEast = 3
        }

        public enum PRJSectorTexture
        {
            Floor = 0,
            Ceiling = 1,
            North4 = 2,
            North1 = 3,
            North3 = 4,
            West4 = 5,
            West1 = 6,
            West3 = 7,
            FloorNENW = 8,
            CeilingNENW = 9,
            North5 = 10,
            North2 = 11,
            West5 = 12,
            West2 = 13,
        }

        [Flags]
        public enum PRJSectorFlags2 : ushort
        {
            /// <summary>This flag is set if you place the Clockwork Beetle on the block.</summary>
            Beetle = 0x0040,
            /// <summary>This flag is set if you place a Trigger Triggerer on the block.</summary>
            Triggerer = 0x0020,
            /// <summary>
            /// This flag is set if the NE or NW corner of the ceiling is marked as No
            /// Collision.
            /// </summary>
            NoCollision_Ceiling_NE_NW = 0x0010,
            /// <summary>
            /// This flag is set if the SE or SW corner of the ceiling is marked as No
            /// Collision.
            /// </summary>
            NoCollision_Ceiling_SE_SW = 0x0008,
            /// <summary>
            /// This flag is set if the NE or NW corner of the floor is marked as No
            /// Collision.
            /// </summary>
            NoCollision_Floor_NE_NW = 0x0004,
            /// <summary>
            /// This flag is set if the SE or SW corner of the floor is marked as No
            /// Collision.
            /// </summary>
            NoCollision_Floor_SE_SW = 0x0002,
        }

        [Flags]
        public enum PRJSectorFlags3 : ushort
        {
            /// <summary>
            /// This flag is set if you use ALT-click on a block split into triangles. It
            /// causes the block to split on the opposite diagonal, allowing you to make
            /// those neat pyramid corners.
            /// </summary>
            TriSplit = 0x0001
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct PRJ_Sector
        {
            public const int MAX_BLOCK_SURFACES = 14;

            /// <summary>This identifies what type of block this is.</summary>
            public PRJSectorType SectorType;
            /// <summary>Various flags describing the block.</summary>
            public PRJSectorFlags1 Flags1;
            /// <summary>
            /// The click level of the lowest corner of the floor part of the block. This
            /// refers to the lowest corner of the part raised using Floor +/-.
            /// The range is -126 thru 126. Negative is down.
            /// </summary>
            public short Floor;
            /// <summary>
            /// The click level of the highest corner of the ceiling part of the block. 
            /// This refers to the highest corner of the part lowered using Ceiling +/-. 
            /// The range is -126 thru 126. Negative is down.
            /// </summary>
            public short Ceiling;
            /// <summary>
            /// This is an array of 4 signed byte values. Each value represents one
            /// corner of the surface of the floor part of the block that is raised using
            /// Floor +/-. Values are always positive.
            /// This value is added to <see cref="Floor"/> to get the actual click height
            /// of this corner of the block.      
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public sbyte[] FloorCorner; // These are added to the floor height
            /// <summary>
            /// This is an array of 4 unsigned byte values. Each value represents one
            /// corner of the surface of the ceiling part of the block that is lowered
            /// using Ceiling +/-. Values are always negative.
            /// This value is added to <see cref="Ceiling"/> to get the actual click height
            /// of this corner of the block. 
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public sbyte[] CeilingCorner;
            /// <summary>
            /// This is an array of 4 signed byte values. Each value represents one
            /// corner of the surface of the floor part of the block that is raised using
            /// the E-D keys. It divides the walls into 2 parts for texturing. Values are
            /// always negative.
            /// This value is added to <see cref="Floor"/> to get the actual click height
            /// of this corner of the block.      
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public sbyte[] ED;
            /// <summary>
            /// This is an array of 4 signed byte values. Each value represents one
            /// corner of the surface of the ceiling part of the block that is lowered
            /// using the R-F keys. It divides the walls into 2 parts for texturing.
            /// Values are always positive.
            /// This value is added to <see cref="Ceiling"/> to get the actual click
            /// height of this corner of the block.           
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public sbyte[] RF;
            /// <summary>
            /// This is an array of structures describing the texture used on each
            /// possible surface of the block. The structure is described next. The order
            /// of the structures – the surface they represent – is below. The N/W is the
            /// direction the surface faces. The 1 – 5 represent the sections described
            /// earlier.
            /// If you only use the standard sections, then section 1, 3, and 4 are
            /// always used by the LE.It is important to note the use of section 1
            /// instead of 2, as this seems backwards. But it is the way they did it, so
            /// we have to live with it.           
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_BLOCK_SURFACES)]
            public PRJ_Sector_Texture[] Textures;
            /// <summary>A few more flags about the block.</summary>
            public PRJSectorFlags2 Flags2;
            /// <summary>And another flag, probably a late addition.</summary>
            public PRJSectorFlags3 Flags3;
        }

        public enum PRJDoorType : ushort
        {
            West = 0x0001,
            North = 0x0002,
            Floor = 0x0004,
            East = 0xFFFE,
            South = 0xFFFD,
            Ceiling = 0xFFFB
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct PRJ_Door
        {
            /// <summary>What type of door this is.</summary>
            public PRJDoorType Type;
            /// <summary>
            /// This is the BLOCK position of the door in the X axis RELATIVE to the West
            /// side of the room. If it is on the West wall, this is zero.
            /// </summary>
            public short XPos;
            /// <summary>
            /// This is the BLOCK position of the door in the Z axis RELATIVE to the
            /// North side of the room. If it is on the North wall, this is zero.
            /// </summary>
            public short ZPos;
            /// <summary>
            /// This is how many blocks are in the door along the X axis. If this door is
            /// on the West or East wall, this is always 1.
            /// </summary>
            public short XSize;
            /// <summary>
            /// This is how many blocks are in the door along the Z axis. If this door is
            /// on the North or South wall, this is always 1.
            /// </summary>
            public short ZSize;
            /// <summary>NOT USED</summary>
            public ushort YPos;
            /// <summary>The room number where this door is.</summary>
            public ushort Room;
            /// <summary>The Thing Index of the matching door in the other room.</summary>
            public ushort Slot;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 13)] public ushort[] Unknown3;
        }

        public enum PRJObjectType : ushort
        {
            Object = 0x0008,
            Trigger = 0x00010,

            Light = 0x4000,
            Shadow = 0x6000,
            Sun = 0x4200,
            Effect = 0x5000,
            Spot = 0x4100,
            Fog = 0x4020,

            Sound = 0x4C00,

            Sink = 0x4400,

            Camera = 0x4800,
            CameraFixed = 0x4080,
            CameraFlyBy = 0x4040
        }

        [Flags]
        public enum PRJObjectOCB : ushort
        {
            SW0 = 0x0001,
            SW1 = 0x0002,
            SW2 = 0x0004,
            SW3 = 0x0008,
            SW4 = 0x0010,
            SW5 = 0x0020,
            SW6 = 0x0040,
            SW7 = 0x0080,
            SW8 = 0x0100,
            SW9 = 0x0200,
            SW10 = 0x0400,
            SW11 = 0x0800,
            SW12 = 0x1000,
            SW13 = 0x2000,
            SW14 = 0x4000,
            SW15 = 0x8000,
            Invisible = 0x0001,
            ClearBody = 0x0080
        }

        public enum PRJObjectOrientation : ushort
        {
            Center = 0x0000,
            West = 0x0010,
            North = 0x0020,
            East = 0x0030,
            South = 0x0040
        }

        public enum PRJObjectFacing : ushort
        {
            N = 0x0000,
            NE = 0x2000,
            E = 0x4000,
            SE = 0x6000,
            S = 0x8000,
            SW = 0xA000,
            W = 0xC000,
            NW = 0xE000
        }

        public enum PRJObjectTint : ushort
        {
            Red = 0x001F,
            Green = 0x03E0,
            Blue = 0x7C00
        }

        public enum PRJTriggerType : ushort
        {
            Trigger = 0,
            Pad = 1,
            Switch = 2,
            Key = 3,
            Pickup = 4,
            Heavy = 5,
            Antipad = 6,
            Combat = 7,
            Dummy = 8,
            AntiTrigger = 9,
            HeavySwitch = 10,
            HeavyAnti = 11,
            Monkey = 12
        }

        [Flags]

        public enum PRJTriggerSwitches : ushort
        {
            SW5 = 0x0020,
            SW4 = 0x0010,
            SW3 = 0x0008,
            SW2 = 0x0004,
            SW1 = 0x0002,
            SWOneShot = 0x0001
        }

        public enum PRJTriggerItem : ushort
        {
            Object = 0,
            FlipMap = 3,
            FlipOn = 4,
            FlipOff = 5,
            Target = 6,
            Finish = 7,
            CD = 8,
            FlipEffect = 9,
            Secret = 10,
            BodyBag = 11,
            FlyBy = 12,
            CutScene = 13
        }
        [Flags]
        public enum PRJSinkTimer : ushort
        {
            SW1 = 0x0001,
            SW2 = 0x0002,
            SW4 = 0x0004,
            SW8 = 0x0008,
            SW16 = 0x0010
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct PRJ_Object
        {
            /// <summary>What type of door this is.</summary>
            public PRJObjectType ObjectCode;
            // Data for all types
            /// <summary>
            /// This is the BLOCK position of the object in the X axis RELATIVE to the
            /// West side of the room. 1-18.
            /// </summary>
            public short XPos;
            /// <summary>
            /// This is the BLOCK position of the object in the Z axis RELATIVE to the
            /// North side of the room. 1-18.
            /// </summary>
            public short ZPos;
            /// <summary>This is always 1 for objects.</summary>
            public short XSize;
            /// <summary>This is always 1 for objects.</summary>
            public short ZSize;
            /// <summary>
            /// This is how many clicks above the floor that the object was placed. If
            /// you raise or lower the floor block, the object keeps the same relative
            /// position to the floor. Objects can be positioned in 1/2 click increments
            /// (128 pixels).
            /// </summary>
            public ushort YPos;
            /// <summary>The room number where this object is.</summary>
            public ushort Room;

            /// <summary>This is the WAD slot number identifying which object this is.</summary>
            public ushort Slot;
            /// <summary>This is the OCB setting for this object.</summary>
            public PRJObjectOCB OCB;

            /// <summary>
            /// The orientation of the object. This is when you right click on it and it
            /// moves from the center of the block to one of 4 positions around the
            /// outside edge. It is actually placed 1/2 click in from the edge of the
            /// block.
            /// </summary>
            public PRJObjectOrientation Orientation;
            /// <summary>
            /// The pixel coordinate of the object on the Z axis, relative to the North
            /// side of the room. It is equal to <see cref="ZPos"/> * 1024, with adjustment
            /// made for the orientation. WAD object orientation is always center.
            /// </summary>
            public int WorldZPos;
            /// <summary>
            /// The pixel coordinate of the object on the Y axis. This is within the
            /// entire world space of 256 clicks. This is negative going UP.
            /// </summary>
            public int WorldYPos;
            /// <summary>
            /// The pixel coordinate of the object on the X axis, relative to the West
            /// side of the room. It is equal to <see cref="XPos"/> * 1024, with adjustment made
            /// for the orientation. WAD object orientation is always center.
            /// </summary>
            public int WorldXPos;
            public ushort Unknown3;
            /// <summary>
            /// The angle the object is facing. You can rotate the object in 8 different
            /// angles. Values are below. This does not refer to the direction an object
            /// is LOOKING. When first placed in the map, the object is considered facing
            /// North, no matter which way it is looking.
            /// </summary>
            public PRJObjectFacing YRot;
            /// <summary>NOT USED</summary>
            public short Roll;

            /// <summary>The tint of the object.</summary>
            public PRJObjectTint Tint;
            /// <summary>NOT USED / for triggers it's the Speed field</summary>
            public short Timer;

            // Following data only for triggers
            /// <summary>The type of trigger.</summary>
            public PRJTriggerType TriggerType; // TRIGGER, PAD, HEAVY
            /// <summary>The Thing Index of the object that is being triggered.</summary>
            public ushort TriggerItemNumber; // Number of object to trigger, CD track, camera number
            /// <summary>The trigger timer value.</summary>
            public short TriggerTimer;
            /// <summary>The trigger switches.</summary>
            public PRJTriggerSwitches TriggerSwitches; // One Shot, trigger flags.
            /// <summary>Identifies what is being triggered.</summary>
            public PRJTriggerItem TriggerItemType; // OBJECT, CD, FLIPMAP.
        }

        public enum PRJ_WADType : ushort
        {
            /// <summary>
            /// This means this slot is not included in the wad file. There is no name,
            /// and no structure, kind of like for rooms. The wSlotType field is immediately followed by another wSlotType field.
            /// </summary>
            Empty = 0x0000,
            /// <summary>
            /// This is for objects that are actually another file. The only wad objects
            /// I am aware of that use this code are SKY_GRAPHICS, DEFAULT_SPRITES, and
            /// MISC_SPRITES.
            /// </summary>
            File = 0x0008,
            /// <summary>
            /// This is for objects that require special attention by the game engine.
            /// This is any sort of animated object like Lara and baddies, as well as any
            /// type of pickup item, puzzles, keys, etc. Anything the game engine must
            /// handle in a special way.
            /// </summary>
            Special = 0x0010,
            /// <summary>
            /// This is for all static objects. It includes PLANT, FURNITURE, ROCK,
            /// ARCHITECTURE, DEBRIS, and SHATTER. If you are like me, you probably think
            /// the SHATTER objects require special attention. But they are lumped
            /// together with the static objects.
            /// </summary>
            Static = 0x0110
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct PRJ_ObjectData
        {
            public PRJ_WADType ObjectType; // 0 for empty (don't continue reading), 8 for sprites, 16 for moveables and 272 for static meshes.
            public string ObjectName; // String terminated by a space (0x20).
            public uint Slot;
            public ushort West;
            public ushort North;
            public ushort East;
            public ushort South;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5 * 5)] public ushort[] Collision;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5 * 5)] public ushort[] Mode;
        }

        public enum PRJRoomType : ushort
        {
            /// <summary>This is a room that actually exists.</summary>
            Defined = 0x0000,
            /// <summary>
            /// This is an Empty room.
            /// If the room is Empty, the rest of the room information is missing. 
            /// Even the rest of the header is not there.
            /// </summary>
            Undefined = 0x0001,
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct PRJ_Room
        {
            /// <summary>type of object</summary>
            public PRJRoomType Type;
            /// <summary>
            /// name of room
            /// It can be up to 79 characters long, leaving one for the null terminator.
            /// If there is room left, the LE adds a string looking like (n:x) where n is
            /// the room's actual slot number and x the logical position not counting
            /// empty slots.
            /// </summary>
            public string RoomName;
            /// <summary>
            /// The absolute PIXEL coordinate of the room on the Z axis. This refers to
            /// the North edge of the room (gray square). If the room is in the top-left
            /// corner of the 2D view, the coordinate will be zero.
            /// </summary>
            public uint WorldZPos;
            public uint WorldYPos;
            /// <summary>
            /// The absolute PIXEL coordinate of the room on the X axis. This refers to
            /// the West edge of the room (gray square). If the room is in the top-left
            /// corner of the 2D view, the coordinate will be zero.
            /// </summary>
            public uint WorldXPos;
            public short[] Unknown2;
            /// <summary>
            /// This is how many blocks to indent the room from the left side of the room
            /// grid. It is how the room is always displayed centered in the grid. It is
            /// only used in WinRoomEdit.
            /// (20 - <see cref="XSize"/>) / 2.
            /// </summary>
            public ushort DisplayXOffset;
            /// <summary>
            /// This is how many blocks to indent the room from the top of the room grid.
            /// It is how the room is always displayed centered in the grid. It is only
            /// used in WinRoomEdit.
            /// (20 - <see cref="ZSize"/>) / 2.
            /// </summary>
            public ushort DisplayZOffset;
            /// <summary>
            /// The number of blocks in the room on the X axis. This is the number of
            /// blocks plus 2 for the gray blocks around the room. It MUST be in the
            /// range 3 - 20 since you can’t have a room smaller than 1x1 or larger than
            /// 18x18.
            /// </summary>
            public short XSize;
            /// <summary>
            /// The number of blocks in the room on the Z axis. This is the number of
            /// blocks plus 2 for the gray blocks around the room. It MUST be in the
            /// range 3 - 20 since you can’t have a room smaller than 1x1 or larger than
            /// 18x18.
            /// </summary>
            public short ZSize;
            /// <summary>
            /// The BLOCK position of the room on the X axis within the world grid. It 
            /// refers to the Northwest corner gray block. Value 0-99.
            /// </summary>
            public short XPos;
            /// <summary>
            /// The BLOCK position of the room on the Z axis within the world grid. It
            /// refers to the Northwest corner gray block. Value 0-99.
            /// </summary>
            public short ZPos;
            /// <summary>
            /// Room number of a room that is connected to this room. This field forms a
            /// linked list of sorts, pointing to another room, not necessarily the next
            /// one (a room can have multiple doors). When you move a room on the 2D
            /// display, the LE also moves the room pointed to by this field. Then it
            /// looks at that room, and move the room it links to. And so on, until it
            /// finds a link that points back to the first room, then it stops. So the
            /// values in this field must form a circular list of room numbers. If the
            /// links are not circular, the LE will lockup.
            /// </summary>
            public ushort Link;
            /// <summary>Number of doors</summary>
            public ushort NumDoors;
            /// <summary>
            /// For each door (see <see cref="NumDoors"/>), an index within the
            /// 2000-entry thing table where the door belongs.
            /// </summary>
            public ushort[] DoorIndex;
            public PRJ_Door[] Doors;
            /// <summary>Number of objects</summary>
            public ushort NumObjects; // Moveables and static meshes
            /// <summary>
            /// For each object (see <see cref="NumObjects"/>), an index within the
            /// 2000-entry thing table where the object belongs.
            /// </summary>
            public ushort[] ObjectNumbers;
            public PRJ_Object[] Objects;
            public PRJ_Color4 RoomColor;
            public short NumObjects2; // Lights and cameras
            public short[] Objects2Index;
            public PRJ_Object2[] Objects2;

            /// <summary>
            /// This is the room number of the flipmap room. If this room does not have a
            /// flipmap, this contains -1.
            /// </summary>
            public short FlippedRoom; // Poshorter to the flipped room (-1 if none).
            /// <summary>These are several flag bits that affect the room.</summary>
            public ushort RoomFlags1;
            /// <summary>
            /// This is the water level minus 1. It is ignored unless the flag is set.
            /// Range is 0 - 3.
            /// </summary>
            public byte WaterInt;
            /// <summary>
            /// This is the mist level minus 1. It is ignored unless the flag is set.
            /// Range is 0 - 3.
            /// </summary>
            public byte MistInt;
            /// <summary>
            /// This is the reflection level minus 1. It is ignored unless the flag is set.
            /// Range is 0 - 3.
            /// </summary>
            public byte ReflectionInt;
            /// <summary>More flags, though not many are used.</summary>
            public ushort RoomFlags2; // Keeps this room on it's place in the map.
            public PRJ_Sector[] Sectors;
        }

        [Flags]
        public enum PRJRoomFlags1 : ushort
        {
            /// <summary>This flag is set if reflection is turned on.</summary>
            Reflection = 0x0200,
            /// <summary>This flag is set if mist is turned on.</summary>
            Mist = 0x0100,
            /// <summary>This flag is set if the NL button is clicked.</summary>
            NL = 0x0080,
            Bit6 = 0x0040,
            /// <summary>This flag is set if the room is outside.</summary>
            Outside = 0x0020,
            /// <summary>
            /// This flag is set if the room has the horizon color applied to any
            /// surface. That is the black color that is transparent in the game.
            /// </summary>
            Horizon = 0x0008,
            /// <summary>This flag is set if this room is a flipmap for another room.</summary>
            FlipRoom = 0x0002,
            /// <summary>This flag is set if water is turned on.</summary>
            Water = 0x0001
        }

        [Flags]
        public enum PRJRoomFlags2 : ushort
        {
            /// <summary>
            /// This flag is set when you click the lock button to lock the room into
            /// its current position in the world.
            /// </summary>
            Locked = 0x0100,
            /// <summary>
            /// This contains the 8-bit flipmap id you assign to your flipmap so you can
            /// uniquely identify it to a trigger.
            /// </summary>
            FlipMap = 0x00FF
        }

        public sbyte[] Header = new sbyte[8]; // A string 'PROJFILE'
        public int Version;
        public int RoomListSize;
        public PRJ_Room[] Rooms;

        public uint NumObjects;
        public uint MaxObjects;
        public uint[] UnusedObjects = new uint[2000];

        public uint NumLights;
        public uint[] UnusedLights = new uint[768];

        public uint NumTriggers;
        public uint[] UnusedTriggers = new uint[512];

        public string TextureFile; // The texture file (DOS Format, like C:\PROGRA~1\COREDE~1\TRLE\TEXTURES\VCI.TGA). Terminated by an space (0x20).
        //public uint NumTextInfo;
        public PRJ_TextInfo[] TextInfo;

        public string ObjectFile; // Same as TextureFile, terminated by 0x20. Name of SWD file.
        //public uint NumObjectData;
        public PRJ_ObjectData[] ObjectData;

        /// <summary>
        /// This contains the number of texture animation ranges that are defined. The
        /// maximum is 40 animation ranges.
        /// </summary>
        public uint NumAnimTextures;
        /// <summary>
        /// This is the table for unused texture animation range index numbers. It works
        /// just like the other unused index tables. It has 40 entries originally numbered 0 – 39.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_ANIMATIONS)] public uint[] UnusedAnimTextures;
        /// <summary>
        /// This is a table of 256 entries, one for each possible full tile in the TGA
        /// file. It is indexed by the full tile number, 0 – 255. Each entry has the
        /// number of the animation range that this tile belongs to. If the tile is not
        /// in any animation range, it contains 0xffffffff.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_TGATILES)] public uint[] AnimTextures;
        /// <summary>
        /// This is an array of 40 animation structures describing the animation ranges.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_ANIMATIONS)]
        public PRJ_AnimText[] AnimRanges;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_TGATILES)] public PRJTerrainType[] Terrain;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_TGATILES)] public PRJBumpLevel[] Bump;
        // Data from here is mostly 0xFF or 0x00 of 0x06, sometimes there is imformation about the animated textures like first texture and last texture.
        // Total size after here seems to be always 2176 bytes (until end of file).
        public const int MAX_ANIMATIONS = 40;
        public const int MAX_TGATILES = 256;

        public enum PRJTerrainType : byte
        {
            Mud = 0,
            Snow = 1,
            Sand = 2,
            Gravel = 3,
            Ice = 4,
            Water = 5,
            Stone = 6,
            Wood = 7,
            Metal = 8,
            Marble = 9,
            Grass = 10,
            Concrete = 11,
            OldWood = 12,
            OldMetal = 13
        }

        public enum PRJBumpLevel : byte
        {
            None = 0,
            Level1 = 1,
            Level2 = 2
        }

        public static PRJFile Read(string fname)
        {
            using (var fs = File.OpenRead(fname))
            {
                return Read(fs);
            }
        }

        public static PRJFile Read(Stream s)
        {
            using (var br = new BinaryReader(s, Encoding.ASCII))
            {
                return Read(br);
            }
        }

        public static PRJFile Read(BinaryReader br)
        {
            var ret = new PRJFile();

            ret.Header = br.ReadSByteArray(8);
            ret.Version = br.ReadInt32();
            ret.RoomListSize = br.ReadInt32();

            ret.Rooms = new PRJ_Room[ret.RoomListSize];

            for (var i = 0; i < ret.RoomListSize; i++)
            {
                var r = new PRJ_Room();

                r.Type = (PRJRoomType) br.ReadInt16();
                if (r.Type == PRJRoomType.Undefined)
                {
                    ret.Rooms[i] = r;
                    continue;
                }

                r.RoomName = br.ReadString(80, Encoding.ASCII);
                r.WorldZPos = br.ReadUInt32();
                r.WorldYPos = br.ReadUInt32();
                r.WorldXPos = br.ReadUInt32();

                r.Unknown2 = br.ReadInt16Array(3);
                r.DisplayXOffset = br.ReadUInt16();
                r.DisplayZOffset = br.ReadUInt16();

                r.XSize = br.ReadInt16();
                r.ZSize = br.ReadInt16();
                r.XPos = br.ReadInt16();
                r.ZPos = br.ReadInt16();
                r.Link = br.ReadUInt16();

                r.NumDoors = br.ReadUInt16();
                if (r.NumDoors > 0)
                {
                    r.DoorIndex = br.ReadUInt16Array(r.NumDoors);
                    r.Doors = br.ReadStructArray<PRJ_Door>(r.NumDoors);
                }

                r.NumObjects = br.ReadUInt16();
                if (r.NumObjects > 0)
                {
                    r.ObjectNumbers = br.ReadUInt16Array(r.NumObjects);
                    r.Objects = new PRJ_Object[r.NumObjects];
                    for (var j = 0; j < r.NumObjects; j++)
                    {
                        var o = new PRJ_Object();

                        o.ObjectCode = (PRJObjectType)br.ReadUInt16();
                        o.XPos = br.ReadInt16();
                        o.ZPos = br.ReadInt16();
                        o.XSize = br.ReadInt16();
                        o.ZSize = br.ReadInt16();
                        o.YPos = br.ReadUInt16();
                        o.Room = br.ReadUInt16();

                        o.Slot = br.ReadUInt16();
                        if (o.ObjectCode == PRJObjectType.Trigger)
                            o.Timer = br.ReadInt16();
                        else
                            o.OCB = (PRJObjectOCB)br.ReadUInt16();
                        o.Orientation = (PRJObjectOrientation) br.ReadUInt16();

                        o.WorldZPos = br.ReadInt32();
                        o.WorldYPos = br.ReadInt32();
                        o.WorldXPos = br.ReadInt32();

                        o.Unknown3 = br.ReadUInt16();
                        o.YRot = (PRJObjectFacing) br.ReadUInt16();
                        o.Roll = br.ReadInt16();

                        o.Tint = (PRJObjectTint) br.ReadUInt16();
                        if (o.ObjectCode == PRJObjectType.Trigger)
                            o.OCB = (PRJObjectOCB)br.ReadUInt16();
                        else
                            o.Timer = br.ReadInt16();

                        if (o.ObjectCode == PRJObjectType.Trigger)
                        {
                            o.TriggerType = (PRJTriggerType) br.ReadUInt16();
                            o.TriggerItemNumber = br.ReadUInt16();
                            o.TriggerTimer = br.ReadInt16();
                            o.TriggerSwitches = (PRJTriggerSwitches) br.ReadUInt16();
                            o.TriggerItemType = (PRJTriggerItem) br.ReadUInt16();
                        }

                        r.Objects[j] = o;
                    }
                }

                r.RoomColor = br.ReadStruct<PRJ_Color4>();

                r.NumObjects2 = br.ReadInt16();
                if (r.NumObjects2 > 0)
                {
                    r.Objects2Index = br.ReadInt16Array(r.NumObjects2);
                    r.Objects2 = new PRJ_Object2[r.NumObjects2];
                    for (var j = 0; j < r.NumObjects2; j++)
                    {
                        var o = new PRJ_Object2();
                        o.ObjectCode = (PRJObjectType) br.ReadInt16();
                        o.ObjectData = br.ReadStruct<PRJ_Object2Data>();
                        switch (o.ObjectCode)
                        {
                            case PRJObjectType.Light:
                            case PRJObjectType.Shadow:
                            case PRJObjectType.Sun:
                            case PRJObjectType.Effect:
                            case PRJObjectType.Spot:
                            case PRJObjectType.Fog:
                                o.LightData = br.ReadStruct<PRJ_Light>();
                                break;
                            /*case PRJObjectType.Sound:
                                o.SoundData = br.ReadStruct<PRJ_Sound>();
                                break;
                            case PRJObjectType.Sink:
                                o.SinkData = br.ReadStruct<PRJ_Sink>();
                                break;
                            case PRJObjectType.Camera:
                            case PRJObjectType.CameraFixed:
                            case PRJObjectType.CameraFlyBy:
                                o.CameraData = br.ReadStruct<PRJ_Camera>();
                                break;*/
                        }
                        r.Objects2[j] = o;
                    }
                }

                r.FlippedRoom = br.ReadInt16();
                r.RoomFlags1 = br.ReadUInt16();
                r.WaterInt = br.ReadByte();
                r.MistInt = br.ReadByte();
                r.ReflectionInt = br.ReadByte();
                r.RoomFlags2 = br.ReadUInt16();

                var NumSectors = r.XSize * r.ZSize;
                r.Sectors = br.ReadStructArray<PRJ_Sector>(NumSectors);

                ret.Rooms[i] = r;
            }

            ret.NumObjects = br.ReadUInt32();
            ret.MaxObjects = br.ReadUInt32();
            ret.UnusedObjects = br.ReadUInt32Array(2000);

            ret.NumLights = br.ReadUInt32();
            ret.UnusedLights = br.ReadUInt32Array(768);

            ret.NumTriggers = br.ReadUInt32();
            ret.UnusedTriggers = br.ReadUInt32Array(512);

            ret.TextureFile = br.ReadStringUntil(enc: Encoding.ASCII, ch: ' ');

            if (ret.TextureFile != "NA")
            {
                var NumTextInfo = br.ReadUInt32();
                ret.TextInfo = br.ReadStructArray<PRJ_TextInfo>(NumTextInfo);
            }

            ret.ObjectFile = br.ReadStringUntil(enc: Encoding.ASCII, ch: ' ');

            var NumObjectData = br.ReadUInt32();
            ret.ObjectData = new PRJ_ObjectData[NumObjectData];
            for (uint i = 0; i < NumObjectData; i++)
            {
                var o = new PRJ_ObjectData();

                o.ObjectType = (PRJ_WADType) br.ReadUInt16();
                if (o.ObjectType == 0)
                {
                    ret.ObjectData[i] = o;
                    continue;
                }
                o.ObjectName = br.ReadStringUntil(enc: Encoding.ASCII, ch: ' ');
                o.Slot = br.ReadUInt32();
                o.West = br.ReadUInt16();
                o.North = br.ReadUInt16();
                o.East = br.ReadUInt16();
                o.South = br.ReadUInt16();
                o.Collision = br.ReadUInt16Array(25);
                o.Mode = br.ReadUInt16Array(25);

                ret.ObjectData[i] = o;
            }

            ret.NumAnimTextures = br.ReadUInt32();
            ret.UnusedAnimTextures = br.ReadUInt32Array(40);
            ret.AnimTextures = br.ReadUInt32Array(256);
            ret.AnimRanges = br.ReadStructArray<PRJ_AnimText>(40);

            ret.Terrain = new PRJTerrainType[256];
            fixed(void* ptr = ret.Terrain) br.ReadToPtr(ptr, 256);
            ret.Bump = new PRJBumpLevel[256];
            fixed (void* ptr = ret.Bump) br.ReadToPtr(ptr, 256);

            return ret;
        }

        public void Write(string fname)
        {
            if (File.Exists(fname))
            {
                File.Delete(fname);
            }
            using (var fs = File.OpenWrite(fname))
            {
                Write(fs);
            }
        }

        public void Write(Stream s)
        {
            using (var bw = new BinaryWriter(s, Encoding.ASCII))
            {
                Write(bw);
            }
        }

        public void Write(BinaryWriter bw)
        {
            bw.WriteSByteArray(Header);
            bw.Write(Version);
            bw.Write(RoomListSize);

            for (var i = 0; i < RoomListSize; i++)
            {
                var r = Rooms[i];
                bw.Write((short)r.Type);
                if (r.Type == PRJRoomType.Undefined)
                    continue;

                bw.WriteString(r.RoomName, length: 80);
                bw.Write(r.WorldZPos);
                bw.Write(r.WorldYPos);
                bw.Write(r.WorldXPos);

                bw.WriteInt16Array(r.Unknown2);
                bw.Write(r.DisplayXOffset);
                bw.Write(r.DisplayZOffset);

                bw.Write(r.XSize);
                bw.Write(r.ZSize);
                bw.Write(r.XPos);
                bw.Write(r.ZPos);
                bw.Write(r.Link);

                bw.Write(r.NumDoors);
                if (r.NumDoors > 0)
                {
                    bw.WriteUInt16Array(r.DoorIndex);
                    bw.WriteStructArray(r.Doors);
                }

                bw.Write(r.NumObjects);
                if (r.NumObjects > 0)
                {
                    bw.WriteUInt16Array(r.ObjectNumbers);
                    for (var j = 0; j < r.NumObjects; j++)
                    {
                        var o = r.Objects[i];

                        bw.Write((ushort)o.ObjectCode);
                        bw.Write(o.XPos);
                        bw.Write(o.ZPos);
                        bw.Write(o.XSize);
                        bw.Write(o.ZSize);
                        bw.Write(o.YPos);
                        bw.Write(o.Room);

                        bw.Write(o.Slot);
                        if(o.ObjectCode == PRJObjectType.Trigger)
                            bw.Write(o.Timer);
                        else
                            bw.Write((ushort)o.OCB);
                        bw.Write((ushort)o.Orientation);

                        bw.Write(o.WorldZPos);
                        bw.Write(o.WorldYPos);
                        bw.Write(o.WorldXPos);

                        bw.Write(o.Unknown3);
                        bw.Write((ushort)o.YRot);
                        bw.Write(o.Roll);

                        bw.Write((ushort)o.Tint);
                        if (o.ObjectCode == PRJObjectType.Trigger)
                            bw.Write((ushort)o.OCB);
                        else
                            bw.Write(o.Timer);

                        if (o.ObjectCode == PRJObjectType.Trigger)
                        {
                            bw.Write((ushort)o.TriggerType);
                            bw.Write(o.TriggerItemNumber);
                            bw.Write(o.TriggerTimer);
                            bw.Write((ushort)o.TriggerSwitches);
                            bw.Write((ushort)o.TriggerItemType);
                        }
                    }
                }

                bw.WriteStruct(r.RoomColor);

                bw.Write(r.NumObjects2);
                if (r.NumObjects2 > 0)
                {
                    bw.WriteInt16Array(r.Objects2Index);
                    for (var j = 0; j < r.NumObjects2; j++)
                    {
                        var o = r.Objects2[i];

                        bw.Write((ushort)o.ObjectCode);
                        bw.WriteStruct(o.ObjectData);
                        switch (o.ObjectCode)
                        {
                            case PRJObjectType.Light:
                            case PRJObjectType.Shadow:
                            case PRJObjectType.Sun:
                            case PRJObjectType.Effect:
                            case PRJObjectType.Spot:
                            case PRJObjectType.Fog:
                                bw.WriteStruct(o.LightData);
                                break;
                            /*case PRJObjectType.Sound:
                                bw.WriteStruct(o.SoundData);
                                break;
                            case PRJObjectType.Sink:
                                bw.WriteStruct(o.SinkData);
                                break;
                            case PRJObjectType.Camera:
                            case PRJObjectType.CameraFixed:
                            case PRJObjectType.CameraFlyBy:
                                bw.WriteStruct(o.CameraData);
                                break;*/
                        }
                    }
                }

                bw.Write(r.FlippedRoom);
                bw.Write(r.RoomFlags1);
                bw.Write(r.WaterInt);
                bw.Write(r.MistInt);
                bw.Write(r.ReflectionInt);
                bw.Write(r.RoomFlags2);

                bw.WriteStructArray(r.Sectors, r.XSize * r.ZSize);
            }

            bw.Write(NumObjects);
            bw.Write(MaxObjects);
            bw.WriteUInt32Array(UnusedObjects);

            bw.Write(NumLights);
            bw.WriteUInt32Array(UnusedLights);

            bw.Write(NumTriggers);
            bw.WriteUInt32Array(UnusedTriggers);

            if (TextureFile[TextureFile.Length - 1] != ' ')
                TextureFile += ' ';
            bw.WriteString(TextureFile);

            bw.Write(TextInfo.Length);
            bw.WriteStructArray(TextInfo);

            if (ObjectFile[ObjectFile.Length - 1] != ' ')
                ObjectFile += ' ';
            bw.WriteString(ObjectFile);

            bw.Write(ObjectData.Length);
            for(uint i = 0; i < ObjectData.Length; i++)
            {
                var o = ObjectData[i];

                bw.Write((ushort) o.ObjectType);
                if (o.ObjectType == 0)
                    continue;
                if (o.ObjectName[o.ObjectName.Length - 1] != ' ')
                    o.ObjectName += ' ';
                bw.WriteString(o.ObjectName);
                bw.Write(o.Slot);
                bw.Write(o.West);
                bw.Write(o.North);
                bw.Write(o.East);
                bw.Write(o.South);
                bw.WriteUInt16Array(o.Collision);
                bw.WriteUInt16Array(o.Mode);
            }

            bw.Write(NumAnimTextures);
            bw.WriteUInt32Array(UnusedAnimTextures);
            bw.WriteUInt32Array(AnimTextures);
            bw.WriteStructArray(AnimRanges);

            var buf = new byte[MAX_TGATILES];
            fixed(void* ptr = Terrain)
                Marshal.Copy((IntPtr)ptr, buf, 0, MAX_TGATILES);
            bw.Write(buf);
            fixed (void* ptr = Bump)
                Marshal.Copy((IntPtr)ptr, buf, 0, MAX_TGATILES);
            bw.Write(buf);
        }

        public static PRJFile LevelToProject(Level l)
        {
            var ret = new PRJFile();
            var test = PRJFile.Read("trle\\test1.prj");
            ret.Header = new sbyte[] {80, 82, 79, 74, 70, 73, 76, 69}; // "PROJFILE"
            ret.Version = 1;

            ret.RoomListSize = l.Rooms.Length;
            ret.Rooms = new PRJ_Room[ret.RoomListSize];
            for (var i = 0; i < ret.RoomListSize; i++)
            {
                var r = new PRJ_Room();
                var lr = l.Rooms[i];

                r.Type = PRJRoomType.Defined;
                r.RoomName = string.Format("Room{0}\0({0}:{0})", i);
                r.WorldZPos = (uint) lr.Offset.X;
                r.WorldYPos = (uint) lr.Offset.Y;
                r.WorldXPos = (uint) -lr.Offset.Z;
                r.XSize = (short) lr.Num_X_Sectors;
                r.ZSize = (short) lr.Num_Z_Sectors;
                r.XPos = (short) (-lr.Offset.Z / 1024);
                r.ZPos = (short) (lr.Offset.X / 1024);
                r.NumDoors = (ushort) lr.Portals.Length;
                r.Doors = new PRJ_Door[r.NumDoors];
                for (var j = 0; j < r.Doors.Length; j++)
                {
                    var p = new PRJ_Door();
                    var lp = lr.Portals[i];
                    var map = new Dictionary<Vertex, PRJDoorType>
                    {
                        [new Vertex(0, 0, -1)] = PRJDoorType.West,
                        /*[new Vertex(0, 0, -1)] = PRJDoorType.North,
                        [new Vertex(0, 0, -1)] = PRJDoorType.Floor,*/
                        [new Vertex(0, 0, 1)] = PRJDoorType.East,
                        /*[new Vertex(0, 0, -1)] = PRJDoorType.South,
                        [new Vertex(0, 0, -1)] = PRJDoorType.Ceiling,*/
                    };
                    map.TryGetValue(lp.Normal, out p.Type);
                    p.XPos = (short)lp.Vertices[1].Y;
                    p.ZPos = (short) (lp.Vertices[0].X / 1024);
                    p.XSize = (short) ((lp.Vertices[0].Z - 1) / -1024);
                    p.ZSize = (short) ((lp.Vertices[3].X - lp.Vertices[0].X) / 1024);
                    p.YPos = 0;
                    p.Room = (ushort) i;
                    //p.Slot = (ushort)()
                }

                var obj = l.Entities.Where(x => x.Room == i).ToArray();

                r.NumObjects = (ushort) obj.Length;
                /*r.Objects = obj.Select(x =>
                {
                    var reto = new PRJ_Object();

                    
                }).ToArray();*/


                ret.Rooms[i] = r;
            }

            return ret;
        }
    }
}

