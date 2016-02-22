using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using BulletSharp;
using OpenTK;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

namespace UniRaider
{
    public enum FloorDataFunctions : byte
    {
        TR_FD_FUNC_PORTALSECTOR = 0x01,
        TR_FD_FUNC_FLOORSLANT = 0x02,
        TR_FD_FUNC_CEILINGSLANT = 0x03,
        TR_FD_FUNC_TRIGGER = 0x04,
        TR_FD_FUNC_DEATH = 0x05,
        TR_FD_FUNC_CLIMB = 0x06,
        TR_FD_FUNC_FLOORTRIANGLE_NW = 0x07, //  [_\_]
        TR_FD_FUNC_FLOORTRIANGLE_NE = 0x08, //  [_/_]
        TR_FD_FUNC_CEILINGTRIANGLE_NW = 0x09, //  [_/_]
        TR_FD_FUNC_CEILINGTRIANGLE_NE = 0x0A, //  [_\_]
        TR_FD_FUNC_FLOORTRIANGLE_NW_PORTAL_SW = 0x0B, //  [P\_]
        TR_FD_FUNC_FLOORTRIANGLE_NW_PORTAL_NE = 0x0C, //  [_\P]
        TR_FD_FUNC_FLOORTRIANGLE_NE_PORTAL_SE = 0x0D, //  [_/P]
        TR_FD_FUNC_FLOORTRIANGLE_NE_PORTAL_NW = 0x0E, //  [P/_]
        TR_FD_FUNC_CEILINGTRIANGLE_NW_PORTAL_SW = 0x0F, //  [P\_]
        TR_FD_FUNC_CEILINGTRIANGLE_NW_PORTAL_NE = 0x10, //  [_\P]
        TR_FD_FUNC_CEILINGTRIANGLE_NE_PORTAL_NW = 0x11, //  [P/_]
        TR_FD_FUNC_CEILINGTRIANGLE_NE_PORTAL_SE = 0x12, //  [_/P]
        TR_FD_FUNC_MONKEY = 0x13,
        TR_FD_FUNC_MINECART_LEFT = 0x14, // In TR3 only. Function changed in TR4+.
        TR_FD_FUNC_MINECART_RIGHT = 0x15 // In TR3 only. Function changed in TR4+.
    }

    public enum TriggerTypes : byte
    {
        TR_FD_TRIGTYPE_TRIGGER = 0x00, // If Lara is in sector, run (any case).
        TR_FD_TRIGTYPE_PAD = 0x01, // If Lara is in sector, run (land case).
        TR_FD_TRIGTYPE_SWITCH = 0x02, // If item is activated, run, else stop.
        TR_FD_TRIGTYPE_KEY = 0x03, // If item is activated, run.
        TR_FD_TRIGTYPE_PICKUP = 0x04, // If item is picked up, run.
        TR_FD_TRIGTYPE_HEAVY = 0x05, // If item is in sector, run, else stop.
        TR_FD_TRIGTYPE_ANTIPAD = 0x06, // If Lara is in sector, stop (land case).
        TR_FD_TRIGTYPE_COMBAT = 0x07, // If Lara is in combat state, run (any case).
        TR_FD_TRIGTYPE_DUMMY = 0x08, // If Lara is in sector, run (air case).
        TR_FD_TRIGTYPE_ANTITRIGGER = 0x09, // TR2-5 only: If Lara is in sector, stop (any case).
        TR_FD_TRIGTYPE_HEAVYSWITCH = 0x0A, // TR3-5 only: If item is activated by item, run.
        TR_FD_TRIGTYPE_HEAVYANTITRIGGER = 0x0B, // TR3-5 only: If item is activated by item, stop.
        TR_FD_TRIGTYPE_MONKEY = 0x0C, // TR3-5 only: If Lara is monkey-swinging, run.
        TR_FD_TRIGTYPE_SKELETON = 0x0D, // TR5 only: Activated by skeleton only?
        TR_FD_TRIGTYPE_TIGHTROPE = 0x0E, // TR5 only: If Lara is on tightrope, run.
        TR_FD_TRIGTYPE_CRAWLDUCK = 0x0F, // TR5 only: If Lara is crawling, run.
        TR_FD_TRIGTYPE_CLIMB = 0x10 // TR5 only: If Lara is climbing, run.
    }

    public enum TriggerFunctionTypes : byte
    {
        TR_FD_TRIGFUNC_OBJECT = 0x00,
        TR_FD_TRIGFUNC_CAMERATARGET = 0x01,
        TR_FD_TRIGFUNC_UWCURRENT = 0x02,
        TR_FD_TRIGFUNC_FLIPMAP = 0x03,
        TR_FD_TRIGFUNC_FLIPON = 0x04,
        TR_FD_TRIGFUNC_FLIPOFF = 0x05,
        TR_FD_TRIGFUNC_LOOKAT = 0x06,
        TR_FD_TRIGFUNC_ENDLEVEL = 0x07,
        TR_FD_TRIGFUNC_PLAYTRACK = 0x08,
        TR_FD_TRIGFUNC_FLIPEFFECT = 0x09,
        TR_FD_TRIGFUNC_SECRET = 0x0A,
        TR_FD_TRIGFUNC_CLEARBODIES = 0x0B, // Unused in TR4
        TR_FD_TRIGFUNC_FLYBY = 0x0C,
        TR_FD_TRIGFUNC_CUTSCENE = 0x0D
    }

    public enum ActionType : sbyte
    {
        TR_ACTIONTYPE_NORMAL = 0,
        TR_ACTIONTYPE_ANTI = 1,
        TR_ACTIONTYPE_SWITCH = 2,
        TR_ACTIONBYTE_BYPASS = -1
    }

    public enum Activator : byte
    {
        TR_ACTIVATOR_NORMAL = 0,
        TR_ACTIVATOR_SWITCH = 1,
        TR_ACTIVATOR_KEY = 2,
        TR_ACTIVATOR_PICKUP = 3
    }

    public enum ActivatorType : byte
    {
        TR_ACTIVATORTYPE_LARA = 0,
        TR_ACTIVATORTYPE_MISC = 1
    }

    [Flags]
    public enum RoomFlag : ushort
    {
        TR_ROOM_FLAG_WATER = 0x0001,
        TR_ROOM_FLAG_QUICKSAND = 0x0002, // Moved from 0x0080 to avoid confusion with NL.
        TR_ROOM_FLAG_SKYBOX = 0x0008,
        TR_ROOM_FLAG_UNKNOWN1 = 0x0010,
        TR_ROOM_FLAG_WIND = 0x0020,
        TR_ROOM_FLAG_UNKNOWN2 = 0x0040,

        //@FIXME: Find what it means!!! Always set by Dxtre3d.
        TR_ROOM_FLAG_NO_LENSFLARE = 0x0080, // In TR4-5. Was quicksand in TR3.
        TR_ROOM_FLAG_MIST = 0x0100,

        //@FIXME: Unknown meaning in TR1!!!
        TR_ROOM_FLAG_CAUSTICS = 0x0200,
        TR_ROOM_FLAG_UNKNOWN3 = 0x0400,
        TR_ROOM_FLAG_DAMAGE = 0x0800,

        //@FIXME: Is it really damage (D)?
        TR_ROOM_FLAG_POISON = 0x1000 //@FIXME: Is it really poison (P)?
    }

    public enum RoomLightMode : byte
    {
        None = 0x0,
        TR_ROOM_LIGHTMODE_FLICKER = 0x1
    }

    [Flags]
    public enum SectorFlag : uint
    {
        SECTOR_FLAG_CLIMB_NORTH = 0x00000001, // subfunction 0x01
        SECTOR_FLAG_CLIMB_EAST = 0x00000002, // subfunction 0x02
        SECTOR_FLAG_CLIMB_SOUTH = 0x00000004, // subfunction 0x04
        SECTOR_FLAG_CLIMB_WEST = 0x00000008, // subfunction 0x08
        SECTOR_FLAG_CLIMB_CEILING = 0x00000010,
        SECTOR_FLAG_MINECART_LEFT = 0x00000020,
        SECTOR_FLAG_MINECART_RIGHT = 0x00000040,
        SECTOR_FLAG_TRIGGERER_MARK = 0x00000080,
        SECTOR_FLAG_BEETLE_MARK = 0x00000100,
        SECTOR_FLAG_DEATH = 0x00000200
    }

    public enum SectorMaterial : byte
    {
        SECTOR_MATERIAL_MUD = 0,
        SECTOR_MATERIAL_SNOW = 1,
        SECTOR_MATERIAL_SAND = 2,
        SECTOR_MATERIAL_GRAVEL = 3,
        SECTOR_MATERIAL_ICE = 4,
        SECTOR_MATERIAL_WATER = 5,
        SECTOR_MATERIAL_STONE = 6, // Classic one, TR1-2.
        SECTOR_MATERIAL_WOOD = 7,
        SECTOR_MATERIAL_METAL = 8,
        SECTOR_MATERIAL_MARBLE = 9,
        SECTOR_MATERIAL_GRASS = 10,
        SECTOR_MATERIAL_CONCRETE = 11,
        SECTOR_MATERIAL_OLDWOOD = 12,
        SECTOR_MATERIAL_OLDMETAL = 13
    }

    public partial class Constants
    {
        public const int FlipmapMax = 32;
        public const int ActivationMask_OR = 0;
        public const int ActivationMask_XOR = 1;
    }

    public partial class Global
    {
        public static EngineContainer LastContainer { get; set; }
    }

    public class BaseItem
    {
        public uint ID { get; set; }

        public uint WorldModelId { get; set; }

        public MenuItemType Type { get; set; }

        public ushort Count { get; set; }

        public char[] Name { get; set; }

        public SSBoneFrame BoneFrame { get; set; }
    }

    public class RoomBox
    {
        public int Xmin { get; set; }

        public int Xmax { get; set; }

        public int Ymin { get; set; }

        public int Ymax { get; set; }

        public int TrueFloor { get; set; }

        public int OverlapIndex { get; set; }
    }

    public class RoomSector
    {
        public uint TrigIndex { get; set; }

        public int BoxIndex { get; set; }

        public uint Flags { get; set; }

        public uint Material { get; set; }

        public int Floor { get; set; }

        public int Ceiling { get; set; }

        public RoomSector SectorBelow { get; set; }

        public RoomSector SectorAbove { get; set; }

        public Room OwnerRoom { get; set; }

        public short IndexX { get; set; }

        public short IndexY { get; set; }

        public Vector3 Position { get; set; }

        public Vector3[] CeilingCorners { get; set; }

        public byte CeilingDiagonalType { get; set; }

        public byte CeilingPenetrationConfig { get; set; }

        public Vector3[] FloorCorners { get; set; }

        public byte FloorDiagonalType { get; set; }

        public byte FloorPenetrationConfig { get; set; }

        public int PortalToRoom { get; set; }

        public RoomSector GetLowestSector()
        {
            var lowest = this;

            RoomSector below = null;
            while ((below = lowest.SectorBelow) != null)
            {
                var flipped = below.CheckFlip();
                if (flipped == null) break;
                lowest = flipped;
            }

            return lowest.CheckFlip();
        }

        public RoomSector GetHighestSector()
        {
            var highest = this;

            RoomSector above = null;
            while ((above = highest.SectorAbove) != null)
            {
                var flipped = above.CheckFlip();
                if (flipped == null) break;
                highest = flipped;
            }

            return highest;
        }

        public RoomSector CheckFlip()
        {
            if (OwnerRoom.Active) return this;

            Room r = null;

            if (OwnerRoom.BaseRoom != null && OwnerRoom.BaseRoom.Active)
            {
                r = OwnerRoom.BaseRoom;
            }
            else if (OwnerRoom.AlternateRoom != null && OwnerRoom.AlternateRoom.Active)
            {
                r = OwnerRoom.AlternateRoom;
            }

            return r == null ? this : r.Sectors[IndexX * r.SectorsY + IndexY];
        }

        public RoomSector CheckBaseRoom()
        {
            if (OwnerRoom.BaseRoom != null)
            {
                var r = OwnerRoom.BaseRoom;
                var ind_x = (int) ((Position.X - r.Transform.Origin.X) / TR_METERING_SECTORSIZE);
                var ind_y = (int) ((Position.Y - r.Transform.Origin.Y) / TR_METERING_SECTORSIZE);
                if (ind_x.IsBetween(0, r.SectorsX - 1) && ind_y.IsBetween(0, r.SectorsY - 1))
                {
                    return r.Sectors[ind_x * r.SectorsY + ind_y];
                }
            }
            return this;
        }

        public RoomSector CheckAlternateRoom()
        {
            if (OwnerRoom.AlternateRoom != null)
            {
                var r = OwnerRoom.AlternateRoom;
                var ind_x = (int) ((Position.X - r.Transform.Origin.X) / TR_METERING_SECTORSIZE);
                var ind_y = (int) ((Position.Y - r.Transform.Origin.Y) / TR_METERING_SECTORSIZE);
                if (ind_x.IsBetween(0, r.SectorsX - 1) && ind_y.IsBetween(0, r.SectorsY - 1))
                {
                    return r.Sectors[ind_x * r.SectorsY + ind_y];
                }
            }
            return this;
        }

        public RoomSector CheckPortalPointerRaw()
        {
            if (PortalToRoom >= 0)
            {
                Room r = Global.EngineWorld.Rooms[PortalToRoom];
                var ind_x = (int) ((Position.X - r.Transform.Origin.X) / TR_METERING_SECTORSIZE);
                var ind_y = (int) ((Position.Y - r.Transform.Origin.Y) / TR_METERING_SECTORSIZE);
                if (ind_x.IsBetween(0, r.SectorsX - 1) && ind_y.IsBetween(0, r.SectorsY - 1))
                {
                    return r.Sectors[ind_x * r.SectorsY + ind_y];
                }
            }
            return this;
        }

        public RoomSector CheckPortalPointer()
        {
            if (PortalToRoom >= 0)
            {
                Room r = Global.EngineWorld.Rooms[PortalToRoom];
                if (OwnerRoom.BaseRoom != null && OwnerRoom.AlternateRoom != null)
                {
                    r = r.AlternateRoom;
                }
                else if (OwnerRoom.AlternateRoom != null && r.BaseRoom != null)
                {
                    r = r.BaseRoom;
                }
                var ind_x = (int) ((Position.X - r.Transform.Origin.X) / TR_METERING_SECTORSIZE);
                var ind_y = (int) ((Position.Y - r.Transform.Origin.Y) / TR_METERING_SECTORSIZE);
                if (ind_x.IsBetween(0, r.SectorsX - 1) && ind_y.IsBetween(0, r.SectorsY - 1))
                {
                    return r.Sectors[ind_x * r.SectorsY + ind_y];
                }
            }
            return this;
        }

        public bool IsTwoSidePortal(RoomSector s2)
        {
            var s1 = CheckPortalPointer();
            s2 = s2.CheckPortalPointer();

            if (OwnerRoom == s2.OwnerRoom) return false;

            var s1p = s2.OwnerRoom.GetSectorRaw(Position);
            var s2p = s1.OwnerRoom.GetSectorRaw(s2.Position);

            if (s1p.PortalToRoom < 0)
            {
                s1p = s1p.CheckAlternateRoom();
                if (s1p.PortalToRoom < 0) return false;
            }

            if (s2p.PortalToRoom < 0)
            {
                s2p = s2p.CheckAlternateRoom();
                if (s2p.PortalToRoom < 0) return false;
            }

            return s1p.CheckPortalPointer() == s1.CheckBaseRoom() &&
                   s2p.CheckPortalPointer() == s2.CheckBaseRoom()
                   ||
                   s1p.CheckPortalPointer() == s1.CheckAlternateRoom() &&
                   s2p.CheckPortalPointer() == s2.CheckAlternateRoom();
        }

        public bool SimilarCeiling(RoomSector s2, bool ignoreDoors)
        {
            if (s2 == null) return false;
            if (this == s2) return true;

            if (Ceiling != s2.Ceiling || CeilingPenetrationConfig == TR_PENETRATION_CONFIG_WALL
                || s2.CeilingPenetrationConfig == TR_PENETRATION_CONFIG_WALL
                || !ignoreDoors && (SectorAbove != null || s2.SectorAbove != null))
                return false;

            for (var i = 0; i < 4; i++)
            {
                if (CeilingCorners[2] != s2.CeilingCorners[2]) return false;
            }

            return true;
        }

        public bool SimilarFloor(RoomSector s2, bool ignoreDoors)
        {
            if (s2 == null) return false;
            if (this == s2) return true;

            if (Floor != s2.Floor || FloorPenetrationConfig == TR_PENETRATION_CONFIG_WALL
                || s2.FloorPenetrationConfig == TR_PENETRATION_CONFIG_WALL
                || !ignoreDoors && (SectorBelow != null || s2.SectorBelow != null))
                return false;

            for (var i = 0; i < 4; i++)
            {
                if (FloorCorners[2] != s2.FloorCorners[2]) return false;
            }

            return true;
        }

        public Vector3 GetFloorPoint()
        {
            return HighestFloorCorner(GetLowestSector());
        }

        public Vector3 GetCeilingPoint()
        {
            return LowestCeilingCorner(GetHighestSector());
        }

        public static Vector3 HighestFloorCorner(RoomSector rs)
        {
            return rs.FloorCorners.MaxBy(x => x.Z);
        }

        public static Vector3 LowestCeilingCorner(RoomSector vs)
        {
            return vs.CeilingCorners.MinBy(x => x.Z);
        }
    }

    public enum SectorTweenType
    {
        TR_SECTOR_TWEEN_TYPE_NONE = 0, // Degenerated vertical polygon.
        TR_SECTOR_TWEEN_TYPE_TRIANGLE_RIGHT = 1, // Triangle pointing right (viewed front).
        TR_SECTOR_TWEEN_TYPE_TRIANGLE_LEFT = 2, // Triangle pointing left (viewed front).
        TR_SECTOR_TWEEN_TYPE_QUAD = 3, //
        TR_SECTOR_TWEEN_TYPE_2TRIANGLES = 4 // it looks like a butterfly
    }

    public class SectorTween
    {
        public Vector3[] FloorCorners { get; set; }
        public SectorTweenType FloorTweenType { get; set; }

        public Vector3[] CeilingCorners { get; set; }

        public SectorTweenType CeilingTweenType { get; set; }
    }

    public class RoomSprite
    {
        public Sprite Sprite { get; set; }

        public Vector3 Position { get; set; }

        public bool WasRendered { get; set; }
    }

    public class Room
    {
        /// <summary>
        /// Room's ID
        /// </summary>
        public uint ID { get; set; }

        /// <summary>
        /// Room's type + water, wind info
        /// </summary>
        public uint Flags { get; set; }

        /// <summary>
        /// Present only in TR2: 0 is normal, 1 is flickering, 2 and 3 are uncertain
        /// </summary>
        public short LightMode { get; set; }

        /// <summary>
        /// Room reverb type
        /// </summary>
        public byte ReverbInfo { get; set; }

        public byte WaterScheme { get; set; }

        public byte AlternateGroup { get; set; }

        /// <summary>
        /// Flag: is active
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// Do not render
        /// </summary>
        public bool Hide { get; set; }

        /// <summary>
        /// Room's base mesh
        /// </summary>
        public BaseMesh Mesh { get; set; }

        /// <summary>
        /// Render data for sprites
        /// </summary>
        public SpriteBuffer SpriteBuffer { get; set; }

        public List<StaticMesh> StaticMesh { get; set; }

        public List<RoomSprite> Sprites { get; set; }

        public List<EngineContainer> Containers { get; set; }

        /// <summary>
        /// Room's bounding box
        /// </summary>
        public Vector3 BBMin { get; set; }

        /// <summary>
        /// Room's bounding box
        /// </summary>
        public Vector3 BBMax { get; set; }

        /// <summary>
        /// GL transformation matrix TODO: it's a btTransform
        /// </summary>
        public Transform Transform { get; set; }

        public float[] AmbientLighting { get; set; }

        public List<Light> Lights { get; set; }

        /// <summary>
        /// Room portals array
        /// </summary>
        public List<Portal> Portals { get; set; }

        /// <summary>
        /// Alternative room pointer
        /// </summary>
        public Room AlternateRoom { get; set; }

        /// <summary>
        /// Base room = room->alternate_room->base_room
        /// </summary>
        public Room BaseRoom { get; set; }

        public ushort SectorsX { get; set; }

        public ushort SectorsY { get; set; }

        public List<RoomSector> Sectors { get; set; }

        public List<Frustum> Frustum { get; set; }

        /// <summary>
        /// Maximum number of portals in this room
        /// </summary>
        public int MaxPath { get; set; }

        public List<Room> NearRoomList { get; set; }

        public List<Room> OverlappedRoomList { get; set; }

        public RigidBody BtBody { get; set; }

        public EngineContainer Self { get; set; }

        public void Enable()
        {
            if (Active) return;

            if (BtBody != null)
            {
                // TODO: World.cpp, L815
            }

            foreach (var sm in StaticMesh)
            {
                if (sm.Body != null)
                {
                    // TODO: World.cpp, L822
                }
            }

            Active = true;
        }

        public void Disable()
        {
            if (!Active) return;

            if (BtBody != null)
            {
                // TODO: World.cpp, L850
            }

            foreach (var sm in StaticMesh)
            {
                if (sm.Body != null)
                {
                    // TODO: World.cpp, L857
                }
            }

            Active = false;
        }

        public void SwapToAlternate()
        {
            if (AlternateRoom != null && Active)
            {
                renderer.cleanList();
                Disable();
                AlternateRoom.Disable();
                SwapPortals(AlternateRoom);
                SwapItems(AlternateRoom);
                AlternateRoom.Enable();
            }
        }

        public void SwapToBase()
        {
            if (BaseRoom != null && Active)
            {
                renderer.cleanList();
                Disable();
                BaseRoom.Disable();
                SwapPortals(BaseRoom);
                SwapItems(BaseRoom);
                BaseRoom.Enable();
            }
        }

        public Room CheckFlip()
        {
            if (!Active)
            {
                if (BaseRoom != null && BaseRoom.Active) return BaseRoom;
                else if (AlternateRoom != null && AlternateRoom.Active) return AlternateRoom;
            }
            return this;
        }

        public void SwapPortals(Room dest)
        {
            foreach (Room r in Global.EngineWorld.rooms)
            {
                foreach (Portal p in r.Portals)
                {
                    if (p.DestRoom != null && p.DestRoom.ID == ID)
                        p.DestRoom = dest;
                }
                r.BuildNearRoomsList();
            }
        }

        public void SwapItems(Room dest)
        {
            foreach (var t in Containers)
            {
                t.Room = dest;
            }

            foreach (var t in dest.Containers)
            {
                t.Room = this;
            }

            var tmp = Containers;
            Containers = dest.Containers;
            dest.Containers = tmp;
        }

        public void BuildNearRoomsList()
        {
            NearRoomList.Clear();

            Portals.ForEach(x => AddToNearRoomsList(x.DestRoom));

            var nrl = NearRoomList;
            nrl.ForEach(x => x?.Portals.ForEach(y => AddToNearRoomsList(y.DestRoom)));
        }

        public void BuildOverlappedRoomsList()
        {
            OverlappedRoomList = Global.EngineWorld.rooms.Where(x => IsOverlapped(x));
        }

        public bool IsJoined(Room r2)
        {
            return Portals.Any(x => x.DestRoom != null && x.DestRoom.ID == r2.ID)
                   || r2.Portals.Any(x => x.DestRoom != null && x.DestRoom.ID == ID);
        }

        public bool IsOverlapped(Room r1)
        {
            if (this == r1 || this == r1.AlternateRoom || AlternateRoom == r1) return false;

            if (BBMin.HigherThan(r1.BBMax, true) || BBMax.LowerThan(r1.BBMin, true))
            {
                return false;
            }

            return !IsJoined(r1);
        }

        public bool HasSector(int x, int y)
        {
            return x < SectorsX && y < SectorsY;
        }

        public void Empty()
        {
            Containers.Clear();
            NearRoomList.Clear();
            Portals.Clear();
            Frustum.Clear();
            Mesh = null;

            if (StaticMesh.Count > 0)
            {
                /*foreach(var sm in StaticMesh)
                {
                    RigidBody body = null;
                    if((body = sm.Body) != null)
                    {
                        if(body.MotionState.)
                    }
                }*/
                StaticMesh.Clear();
            }

            Sectors.Clear();
            SectorsX = 0;
            SectorsY = 0;

            Sprites.Clear();

            Lights.Clear();
        }

        public void AddEntity(Entity entity)
        {
            if (Containers.Any(x => x == entity.m_self)) return;

            entity.m_self.room = this;
            Containers.Insert(0, entity.m_self);
        }

        public bool RemoveEntity(Entity entity)
        {
            if (entity == null || Containers.Count == 0)
                return false;

            return Containers.Remove(entity.m_self);
        }

        public void AddToNearRoomsList(Room r)
        {
            if (r != null && !IsInNearRoomsList(r) && ID != r.ID && !IsOverlapped(r))
            {
                NearRoomList.Add(r);
            }
        }

        public bool IsInNearRoomsList(Room r)
        {
            return r.ID == ID || NearRoomList.Any(x => x.ID == r.ID) || r.NearRoomList.Any(x => x.ID == ID);
        }

        public bool IsPointIn(Vector3 dot)
        {
            return dot.IsBetween(BBMin, BBMax);
        }

        public RoomSector GetSectorRaw(Vector3 pos)
        {
            if (!Active) return null;

            var x = (int) (pos.X - Transform.Origin.X) / 1024;
            var y = (int) (pos.Y - Transform.Origin.Y) / 1024;

            if (!x.IsBetween(0, SectorsX - 1) || !y.IsBetween(0, SectorsY - 1)) return null;

            return Sectors[x * SectorsY + y];
        }

        public RoomSector GetSectorXYZ(Vector3 pos)
        {
            var room = CheckFlip();

            if (!room.Active) return null;

            var x = (int) (pos.X - room.Transform.Origin.X) / 1024;
            var y = (int) (pos.Y - room.Transform.Origin.Y) / 1024;

            if (!x.IsBetween(0, room.SectorsX - 1) || !y.IsBetween(0, room.SectorsY - 1)) return null;

            var ret = Sectors[x * SectorsY + y];

            if (ret.SectorBelow != null && ret.SectorBelow.Ceiling >= pos.Z)
                return ret.SectorBelow.CheckFlip();

            if (ret.SectorAbove != null && ret.SectorAbove.Floor <= pos.Z)
                return ret.SectorAbove.CheckFlip();

            return ret;
        }

        public void GenMesh(World world, uint roomID, Loader.Level tr);
    }

    public class FlipInfo
    {
        public byte Map { get; set; }

        public byte State { get; set; }
    }

    public class World
    {
        public string Name { get; set; }

        public uint ID { get; set; }

        public Loader.TRVersion EngineVersion { get; set; }

        public List<Room> Rooms { get; set; }

        public List<RoomBox> RoomBoxes { get; set; }

        public List<FlipInfo> FlipData { get; set; }

        public BorderedTextureAtlas TextureAtlas { get; set; }

        public List<uint> Textures { get; set; }

        public List<AnimSeq> AnimSequences { get; set; }

        public List<BaseMesh> Meshes { get; set; }

        public List<Sprite> Sprites { get; set; }

        public List<SkeletalModel> SkeletalModels { get; set; }

        public Character Character { get; set; }

        public SkeletalModel SkyBox { get; set; }

        /// <summary>
        /// Tree of world active objects
        /// </summary>
        public Dictionary<uint, Entity> EntityTree { get; set; }

        public uint NextEntityID { get; set; }

        /// <summary>
        /// Tree of world items
        /// </summary>
        public Dictionary<uint, BaseItem> ItemsTree { get; set; }

        public uint Type { get; set; }

        /// <summary>
        /// Cameras and sinks
        /// </summary>
        public List<StatCameraSink> CamerasSinks { get; set; }

        public List<short> AnimCommands { get; set; }

        /// <summary>
        /// Audio emitters
        /// </summary>
        public List<AudioEmitter> AudioEmitters { get; set; }
        
        /// <summary>
        /// Effect indexes
        /// </summary>
        public List<short> AudioMap { get; set; }  

        /// <summary>
        /// Effects and their parameters
        /// </summary>
        public List<AudioEffect> AudioEffects { get; set; } 

        /// <summary>
        /// Samples
        /// </summary>
        public List<uint> AudioBuffers { get; set; } 

        /// <summary>
        /// Channels
        /// </summary>
        public List<AudioSource> AudioSources { get; set; } 

        /// <summary>
        /// Stream tracks
        /// </summary>
        public List<StreamTrack> StreamTracks { get; set; } 

        /// <summary>
        /// Stream track flag map
        /// </summary>
        public List<byte> StreamTrackMap { get; set; }

        public void UpdateAnimTextures();

        public void CalculateWaterTint();

        public void AddEntity(Entity entity);

        public bool CreateItem(uint itemID, uint modelID, uint worldModelID, MenuItemType type, ushort count,
            string name);

        public int DeleteItem(uint itemID);

        public Sprite GetSpriteByID(uint spriteID);

        public SkeletalModel GetModelByID(uint modelID);

        public void Prepare()
        {
            ID = 0;
            Name = "";
            Type = 0;
            Meshes = new List<BaseMesh>();
            Sprites = new List<Sprite>();
            Rooms = new List<Room>();
            FlipData = new List<FlipInfo>();
            Textures = new List<uint>();
            EntityTree = new Dictionary<uint, Entity>();
            ItemsTree = new Dictionary<uint, BaseItem>();
            Character = new Character();

            AudioSources = new List<AudioSource>();
            AudioBuffers = new List<uint>();
            AudioEffects = new List<AudioEffect>();
            AnimSequences = new List<AnimSeq>();
            StreamTracks = new List<StreamTrack>();
            StreamTrackMap = new List<byte>();

            RoomBoxes = new List<RoomBox>();
            CamerasSinks = new List<StatCameraSink>();
            SkeletalModels = new List<SkeletalModel>();
            SkyBox = null;
            AnimCommands = new List<short>();
        }

        public void Empty()
        {
            Global.LastContainer = null;

        }

        public uint SpawnEntity(uint modelID, uint roomID, Vector3 pos, Vector3 angle, int ID);

        public bool DeleteEntity(uint ID);

        public Entity GetEntityByID(uint ID);

        public Character GetCharacterByID(uint ID);

        public BaseItem GetBaseItemByID(uint ID);

        public Room FindRoomByPosition(Vector3 pos);

        public Room GetRoomByID(uint ID);
    }
}
