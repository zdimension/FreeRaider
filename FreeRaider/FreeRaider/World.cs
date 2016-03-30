using System;
using System.Collections.Generic;
using System.Linq;
using BulletSharp;
using FreeRaider.Loader;
using OpenTK;
using static FreeRaider.Constants;
using static FreeRaider.Global;
using static FreeRaider.StaticFuncs;

namespace FreeRaider
{
    public enum FloorDataFunctions : byte
    {
        PortalSector = 0x01,
        FloorSlant = 0x02,
        CeilingSlant = 0x03,
        Trigger = 0x04,
        Death = 0x05,
        Climb = 0x06,
        FloorTriangleNW = 0x07, //  [_\_]
        FloorTriangleNE = 0x08, //  [_/_]
        CeilingTriangleNW = 0x09, //  [_/_]
        CeilingTriangleNE = 0x0a, //  [_\_]
        FloorTriangleNWPortalSW = 0x0b, //  [P\_]
        FloorTriangleNWPortalNE = 0x0c, //  [_\P]
        FloorTriangleNEPortalSE = 0x0d, //  [_/P]
        FloorTriangleNEPortalNW = 0x0e, //  [P/_]
        CeilingTriangleNWPortalSW = 0x0f, //  [P\_]
        CeilingTriangleNWPortalNE = 0x10, //  [_\P]
        CeilingTriangleNEPortalNW = 0x11, //  [P/_]
        CeilingTriangleNEPortalSE = 0x12, //  [_/P]
        Monkey = 0x13,
        MinecartLeft = 0x14, // In TR3 only. Function changed in TR4+.
        MinecartRight = 0x15 // In TR3 only. Function changed in TR4+.
    }

    public enum TriggerTypes : byte
    {
        Trigger = 0x00, // If Lara is in sector, run (any case).
        Pad = 0x01, // If Lara is in sector, run (land case).
        Switch = 0x02, // If item is activated, run, else stop.
        Key = 0x03, // If item is activated, run.
        Pickup = 0x04, // If item is picked up, run.
        Heavy = 0x05, // If item is in sector, run, else stop.
        AntiPad = 0x06, // If Lara is in sector, stop (land case).
        Combat = 0x07, // If Lara is in combat state, run (any case).
        Dummy = 0x08, // If Lara is in sector, run (air case).
        AntiTrigger = 0x09, // TR2-5 only: If Lara is in sector, stop (any case).
        HeavySwitch = 0x0a, // TR3-5 only: If item is activated by item, run.
        HeavyAntiTrigger = 0x0b, // TR3-5 only: If item is activated by item, stop.
        Monkey = 0x0c, // TR3-5 only: If Lara is monkey-swinging, run.
        Skeleton = 0x0d, // TR5 only: Activated by skeleton only?
        Tightrope = 0x0e, // TR5 only: If Lara is on tightrope, run.
        CrawlDuck = 0x0f, // TR5 only: If Lara is crawling, run.
        Climb = 0x10 // TR5 only: If Lara is climbing, run.
    }

    public enum TriggerFunctionTypes : byte
    {
        Object = 0x00,
        CameraTarget = 0x01,
        UwCurrent = 0x02,
        FlipMap = 0x03,
        FlipOn = 0x04,
        FlipOff = 0x05,
        LookAt = 0x06,
        EndLevel = 0x07,
        PlayTrack = 0x08,
        FlipEffect = 0x09,
        Secret = 0x0a,
        ClearBodies = 0x0b, // Unused in TR4
        FlyBy = 0x0c,
        Cutscene = 0x0d
    }

    public enum ActionType : sbyte
    {
        Normal = 0,
        Anti = 1,
        Switch = 2,
        Bypass = -1
    }

    public enum Activator : byte
    {
        Normal = 0,
        Switch = 1,
        Key = 2,
        Pickup = 3
    }

    public enum ActivatorType : byte
    {
        Lara = 0,
        Misc = 1
    }

    [Flags]
    public enum RoomFlag : ushort
    {
        Water = 0x0001,
        Quicksand = 0x0002, // Moved from 0x0080 to avoid confusion with NL.
        Skybox = 0x0008,
        Unknown1 = 0x0010,
        Wind = 0x0020,
        Unknown2 = 0x0040,

        //@FIXME: Find what it means!!! Always set by Dxtre3d.
        NoLensflare = 0x0080, // In TR4-5. Was quicksand in TR3.
        Mist = 0x0100,

        //@FIXME: Unknown meaning in TR1!!!
        Caustics = 0x0200,
        Unknown3 = 0x0400,
        Damage = 0x0800,

        //@FIXME: Is it really damage (D)?
        Poison = 0x1000 //@FIXME: Is it really poison (P)?
    }

    public enum RoomLightMode : byte
    {
        None = 0x0,
        Flicker = 0x1
    }

    [Flags]
    public enum SectorFlag : uint
    {
        ClimbNorth = 0x00000001, // subfunction 0x01
        ClimbEast = 0x00000002, // subfunction 0x02
        ClimbSouth = 0x00000004, // subfunction 0x04
        ClimbWest = 0x00000008, // subfunction 0x08
        ClimbCeiling = 0x00000010,
        MinecartLeft = 0x00000020,
        MinecartRight = 0x00000040,
        TriggererMark = 0x00000080,
        BeetleMark = 0x00000100,
        Death = 0x00000200
    }

    public enum SectorMaterial : byte
    {
        Mud = 0,
        Snow = 1,
        Sand = 2,
        Gravel = 3,
        Ice = 4,
        Water = 5,
        Stone = 6, // Classic one, TR1-2.
        Wood = 7,
        Metal = 8,
        Marble = 9,
        Grass = 10,
        Concrete = 11,
        OldWood = 12,
        OldMetal = 13
    }

    public partial class Constants
    {
        // Maximum number of flipmaps specifies how many flipmap indices to store. Usually,
        // TR1-3 doesn't contain flipmaps above 10, while in TR4-5 number of flipmaps could
        // be as much as 14-16. To make sure flipmap array will be suitable for all game
        // versions, it is set to 32.
        public const int FLIPMAX_MAX_NUMBER = 32;

        // Activation mask operation can be either XOR (for switch triggers) or OR (for any
        // other types of triggers).
        public const int AMASK_OP_OR = 0;
        public const int AMASK_OP_XOR = 1;
    }

    public partial class Global
    {
        public static EngineContainer LastContainer;
    }

    public class BaseItem
    {
        public uint ID { get; set; }

        public uint WorldModelId { get; set; }

        public MenuItemType Type { get; set; }

        public ushort Count { get; set; }

        public string Name { get; set; }

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

        public TR_SECTOR_DIAGONAL_TYPE CeilingDiagonalType { get; set; }

        public TR_PENETRATION_CONFIG CeilingPenetrationConfig { get; set; }

        public Vector3[] FloorCorners { get; set; }

        public TR_SECTOR_DIAGONAL_TYPE FloorDiagonalType { get; set; }

        public TR_PENETRATION_CONFIG FloorPenetrationConfig { get; set; }

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
                Room r = EngineWorld.Rooms[PortalToRoom];
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
                Room r = EngineWorld.Rooms[PortalToRoom];
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

            if (Ceiling != s2.Ceiling || CeilingPenetrationConfig == TR_PENETRATION_CONFIG.Wall
                || s2.CeilingPenetrationConfig == TR_PENETRATION_CONFIG.Wall
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

            if (Floor != s2.Floor || FloorPenetrationConfig ==  TR_PENETRATION_CONFIG.Wall
                || s2.FloorPenetrationConfig == TR_PENETRATION_CONFIG.Wall
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
        None = 0, // Degenerated vertical polygon.
        TriangleRight = 1, // Triangle pointing right (viewed front).
        TriangleLeft = 2, // Triangle pointing left (viewed front).
        Quad = 3, //
        TwoTriangles = 4 // it looks like a butterfly
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
                if (sm.BtBody != null)
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
                if (sm.BtBody != null)
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
                Renderer.CleanList();
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
                Renderer.CleanList();
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
            foreach (Room r in EngineWorld.Rooms)
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
            OverlappedRoomList = EngineWorld.Rooms.Where(IsOverlapped).ToList();
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
                foreach(var sm in StaticMesh)
                {
                    RigidBody body = null;
                    if((body = sm.BtBody) != null)
                    {
                        if(body.MotionState != null)
                        {
                            body.MotionState.Dispose();
                            body.MotionState = null;
                        }
                        body.CollisionShape = null;

                        BtEngineDynamicsWorld.RemoveRigidBody(body);
                        body.Dispose();
                        body = null;
                        sm.BtBody = null;
                    }

                    if(sm.Self != null)
                    {
                        sm.Self.Room = null;
                        sm.Self = null;
                    }
                }
                StaticMesh.Clear();
            }

            if(BtBody != null)
            {
                BtBody.UserObject = null;
                if(BtBody.MotionState != null)
                {
                    BtBody.MotionState.Dispose();
                    BtBody.MotionState = null;
                }
                if(BtBody.CollisionShape != null)
                {
                    BtBody.CollisionShape.Dispose();
                    BtBody.CollisionShape = null;
                }

                BtEngineDynamicsWorld.RemoveRigidBody(BtBody);
                BtBody = null;
            }

            Sectors.Clear();
            SectorsX = 0;
            SectorsY = 0;

            Sprites.Clear();

            Lights.Clear();

            Self = null;
        }

        public void AddEntity(Entity entity)
        {
            if (Containers.Any(x => x == entity.Self)) return;

            entity.Self.Room = this;
            Containers.Insert(0, entity.Self);
        }

        public bool RemoveEntity(Entity entity)
        {
            if (entity == null || Containers.Count == 0)
                return false;

            return Containers.Remove(entity.Self);
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

        public void GenMesh(World world, uint roomID, Level tr)
        {
            var texMask = world.EngineVersion == Loader.Engine.TR4 ? TextureIndexMaskTr4 : TextureIndexMask;

            var trRoom = tr.Rooms[roomID];

            if(trRoom.Triangles.Length == 0 && trRoom.Rectangles.Length == 0)
            {
                Mesh = null;
                return;
            }

            Mesh = new BaseMesh();
            Mesh.ID = roomID;
            Mesh.TexturePageCount = world.TextureAtlas.NumAtlasPages + 1;
            Mesh.UsesVertexColors = true; // This is implicitly true on room meshes

            Mesh.Vertices.Resize(trRoom.Vertices.Length);
            for (var i = 0; i < Mesh.Vertices.Count; i++)
            {
                Mesh.Vertices[i].Position = TR_vertex_to_arr(trRoom.Vertices[i].Vertex);
                Mesh.Vertices[i].Normal = Vector3.Zero; // paranoid
            }

            Mesh.FindBB();

            Mesh.Polygons.Resize(trRoom.Triangles.Length + trRoom.Rectangles.Length);
            var p = 0;

            // triangles
            for (var i = 0; i < trRoom.Triangles.Length; i++, p++)
            {
                tr_setupRoomVertices(world, tr, trRoom, Mesh, 3, trRoom.Triangles[i].Vertices,
                    (ushort)(trRoom.Triangles[i].Texture & texMask), Mesh.Polygons[p]);
                Mesh.Polygons[p].DoubleSide = Convert.ToBoolean(trRoom.Triangles[i].Texture & 0x8000);
            }

            // rectangles
            for (var i = 0; i < trRoom.Rectangles.Length; i++, p++)
            {
                tr_setupRoomVertices(world, tr, trRoom, Mesh, 4, trRoom.Rectangles[i].Vertices,
                    (ushort)(trRoom.Rectangles[i].Texture & texMask), Mesh.Polygons[p]);
                Mesh.Polygons[p].DoubleSide = Convert.ToBoolean(trRoom.Rectangles[i].Texture & 0x8000);
            }

            // let us normalise normals %)
            foreach (var v in Mesh.Vertices)
            {
                v.Normal = v.Normal.SafeNormalize();
            }

            p = 0;
            // triangles
            for (var i = 0; i < trRoom.Triangles.Length; i++, p++)
            {
                tr_copyNormals(Mesh.Polygons[p], Mesh, trRoom.Triangles[i].Vertices);
            }

            // rectangles
            for (var i = 0; i < trRoom.Rectangles.Length; i++, p++)
            {
                tr_copyNormals(Mesh.Polygons[p], Mesh, trRoom.Rectangles[i].Vertices);
            }

            Mesh.Vertices.Clear();
            Mesh.GenFaces();
            Mesh.PolySortInMesh();
        }

        public static Room FindPosCogerrence(Vector3 newPos, Room room)
        {
            if(room == null)
            {
                return EngineWorld.FindRoomByPosition(newPos);
            }


        }
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

        public Loader.Engine EngineVersion { get; set; }

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

        public unsafe void CalculateWaterTint(float[] tint, bool fixedColour)
        {
            fixed(float* ptr = tint)
                CalculateWaterTint(ptr, fixedColour);
        }

        public unsafe void CalculateWaterTint(float* tint, bool fixedColour);

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
            Character = new Character(0);

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
            LastContainer = null;

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
