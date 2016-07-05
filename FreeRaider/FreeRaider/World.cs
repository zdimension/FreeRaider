using System;
using System.Collections.Generic;
using System.Linq;
using BulletSharp;
using FreeRaider.Loader;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using static FreeRaider.Constants;
using static FreeRaider.Loader.Constants;
using static FreeRaider.Global;
using static FreeRaider.StaticFuncs;

namespace FreeRaider
{
    public enum FD_FUNC : byte
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

    public enum FD_TRIGFUNC : byte
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

        // FIXME: Find what it means!!! Always set by Dxtre3d.
        NoLensflare = 0x0080, // In TR4-5. Was quicksand in TR3.
        Mist = 0x0100,

        // FIXME: Unknown meaning in TR1!!!
        Caustics = 0x0200,
        Unknown3 = 0x0400,
        Damage = 0x0800,

        // FIXME: Is it really damage (D)?
        Poison = 0x1000 // FIXME: Is it really poison (P)?
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

    public class BaseItem : IDisposable
    {
        public uint ID;

        public uint WorldModelId;

        public MenuItemType Type;

        public ushort Count;

        public string Name;

        public SSBoneFrame BoneFrame;

        public void Dispose()
        {
            BoneFrame.BoneTags.Clear();
        }
    }

    public class RoomBox
    {
        public int Xmin;

        public int Xmax;

        public int Ymin;

        public int Ymax;

        public int TrueFloor;

        public int OverlapIndex;
    }

    public class RoomSector
    {
        public uint TrigIndex;

        public int BoxIndex;

        public SectorFlag Flags;

        public SectorMaterial Material;

        public int Floor;

        public int Ceiling;

        public RoomSector SectorBelow;

        public RoomSector SectorAbove;

        public Room OwnerRoom;

        public short IndexX;

        public short IndexY;

        public Vector3 Position;

        public Vector3[] CeilingCorners = new Vector3[4];

        public TR_SECTOR_DIAGONAL_TYPE CeilingDiagonalType;

        public TR_PENETRATION_CONFIG CeilingPenetrationConfig;

        public Vector3[] FloorCorners = new Vector3[4];

        public TR_SECTOR_DIAGONAL_TYPE FloorDiagonalType;

        public TR_PENETRATION_CONFIG FloorPenetrationConfig;

        public int PortalToRoom;

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
                var r = EngineWorld.Rooms[PortalToRoom];
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
                var r = EngineWorld.Rooms[PortalToRoom];
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
        public Vector3[] FloorCorners = new Vector3[4];
        public SectorTweenType FloorTweenType = SectorTweenType.None;

        public Vector3[] CeilingCorners = new Vector3[4];

        public SectorTweenType CeilingTweenType = SectorTweenType.None;
    }

    public class RoomSprite
    {
        public Sprite Sprite;

        public Vector3 Position;

        public bool WasRendered;
    }

    public class Room
    {
        /// <summary>
        /// Room's ID
        /// </summary>
        public uint ID;

        /// <summary>
        /// Room's type + water, wind info
        /// </summary>
        public uint Flags;

        /// <summary>
        /// Present only in TR2: 0 is normal, 1 is flickering, 2 and 3 are uncertain
        /// </summary>
        public short LightMode;

        /// <summary>
        /// Room reverb type
        /// </summary>
        public byte ReverbInfo;

        public byte WaterScheme;

        public byte AlternateGroup;

        /// <summary>
        /// Flag: is active
        /// </summary>
        public bool Active;

        /// <summary>
        /// Do not render
        /// </summary>
        public bool Hide;

        /// <summary>
        /// Room's base mesh
        /// </summary>
        public BaseMesh Mesh;

        /// <summary>
        /// Render data for sprites
        /// </summary>
        public SpriteBuffer SpriteBuffer;

        public List<StaticMesh> StaticMesh = new List<StaticMesh>();

        public List<RoomSprite> Sprites = new List<RoomSprite>();

        public List<EngineContainer> Containers = new List<EngineContainer>();

        /// <summary>
        /// Room's bounding box
        /// </summary>
        public Vector3 BBMin;

        /// <summary>
        /// Room's bounding box
        /// </summary>
        public Vector3 BBMax;

        /// <summary>
        /// GL transformation matrix
        /// </summary>
        public Transform Transform;

        public float[] AmbientLighting;

        public List<Light> Lights = new List<Light>();

        /// <summary>
        /// Room portals array
        /// </summary>
        public List<Portal> Portals = new List<Portal>();

        /// <summary>
        /// Alternative room pointer
        /// </summary>
        public Room AlternateRoom;

        /// <summary>
        /// Base room = room->alternate_room->base_room
        /// </summary>
        public Room BaseRoom;

        public ushort SectorsX;

        public ushort SectorsY;

        public List<RoomSector> Sectors;

        public List<Frustum> Frustum;

        /// <summary>
        /// Maximum number of portals in this room
        /// </summary>
        public int MaxPath;

        public List<Room> NearRoomList;

        public List<Room> OverlappedRoomList;

        public RigidBody BtBody;

        public EngineContainer Self;

        public void Enable()
        {
            if (Active) return;

            if (BtBody != null)
            {
                BtEngineDynamicsWorld.AddRigidBody(BtBody);
            }

            foreach (var sm in StaticMesh)
            {
                if (sm.BtBody != null)
                {
                    BtEngineDynamicsWorld.AddRigidBody(sm.BtBody);
                }
            }

            Active = true;
        }

        public void Disable()
        {
            if (!Active) return;

            if (BtBody != null)
            {
                BtEngineDynamicsWorld.RemoveRigidBody(BtBody);
            }

            foreach (var sm in StaticMesh)
            {
                if (sm.BtBody != null)
                {
                   BtEngineDynamicsWorld.RemoveRigidBody(sm.BtBody);
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
            foreach (var r in EngineWorld.Rooms)
            {
                foreach (var p in r.Portals)
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
                    //BtBody.CollisionShape = null;
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

            Mesh = new BaseMesh
            {
                ID = roomID,
                TexturePageCount = world.TextureAtlas.NumAtlasPages + 1,
                UsesVertexColors = true // This is implicitly true on room meshes
            };

            Mesh.Vertices.Resize(trRoom.Vertices.Length, () => new Vertex());
            for (var i = 0; i < Mesh.Vertices.Count; i++)
            {
                Mesh.Vertices[i].Position = trRoom.Vertices[i].Vertex.ToVector3();
                Mesh.Vertices[i].Normal = Vector3.Zero; // paranoid
            }

            Mesh.FindBB();

            Mesh.Polygons.Resize(trRoom.Triangles.Length + trRoom.Rectangles.Length, () => new Polygon());
            var p = 0;

            // triangles
            for (var i = 0; i < trRoom.Triangles.Length; i++, p++)
            {
                tr_setupRoomVertices(world, tr, trRoom, Mesh, 3, trRoom.Triangles[i].Vertices,
                    (ushort)(trRoom.Triangles[i].Texture & texMask), Mesh.Polygons[p]);
                Mesh.Polygons[p].DoubleSide = (trRoom.Triangles[i].Texture & 0x8000) != 0;
            }

            // rectangles
            for (var i = 0; i < trRoom.Rectangles.Length; i++, p++)
            {
                tr_setupRoomVertices(world, tr, trRoom, Mesh, 4, trRoom.Rectangles[i].Vertices,
                    (ushort)(trRoom.Rectangles[i].Texture & texMask), Mesh.Polygons[p]);
                Mesh.Polygons[p].DoubleSide = (trRoom.Rectangles[i].Texture & 0x8000) != 0;
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

            if (room.Active && newPos.X.IsBetween(room.BBMin.X, room.BBMax.X, IB.aIbE) &&
                newPos.Y.IsBetween(room.BBMin.Y, room.BBMax.Y, IB.aIbE))
            {
                if(newPos.Z.IsBetween(room.BBMin.Z, room.BBMax.Z, IB.aIbE))
                {
                    return room;
                }
                else if(newPos.Z >= room.BBMax.Z)
                {
                    var origSector = room.GetSectorRaw(newPos);
                    if(origSector.SectorAbove != null)
                    {
                        return origSector.SectorAbove.OwnerRoom.CheckFlip();
                    }
                }
                else if (newPos.Z < room.BBMin.Z)
                {
                    var origSector = room.GetSectorRaw(newPos);
                    if (origSector.SectorBelow != null)
                    {
                        return origSector.SectorBelow.OwnerRoom.CheckFlip();
                    }
                }
            }

            var newSector = room.GetSectorRaw(newPos);
            if(newSector != null && newSector.PortalToRoom >= 0)
            {
                return EngineWorld.Rooms[newSector.PortalToRoom].CheckFlip();
            }

            foreach (var r in room.NearRoomList)
            {
                if(r.Active && newPos.IsBetween(r.BBMin, r.BBMax, IB.aIbE))
                {
                    return r;
                }
            }

            return EngineWorld.FindRoomByPosition(newPos);
        }
    }

    public class FlipInfo
    {
        public byte Map;

        public byte State;
    }

    public class World
    {
        public string Name;

        public uint ID;

        public Loader.Engine EngineVersion;

        public List<Room> Rooms;

        public List<RoomBox> RoomBoxes;

        public List<FlipInfo> FlipData;

        public BorderedTextureAtlas TextureAtlas;

        public List<uint> Textures;

        public List<AnimSeq> AnimSequences;

        public List<BaseMesh> Meshes;

        public List<Sprite> Sprites;

        public List<SkeletalModel> SkeletalModels;

        public Character Character;

        public SkeletalModel SkyBox;

        /// <summary>
        /// Tree of world active objects
        /// </summary>
        public Dictionary<uint, Entity> EntityTree;

        public uint NextEntityID;

        /// <summary>
        /// Tree of world items
        /// </summary>
        public Dictionary<uint, BaseItem> ItemsTree;

        public uint Type;

        /// <summary>
        /// Cameras and sinks
        /// </summary>
        public List<StatCameraSink> CamerasSinks;

        public short[] AnimCommands;

#if !NO_AUDIO
        /// <summary>
        /// Audio emitters
        /// </summary>
        public List<AudioEmitter> AudioEmitters;
        
        /// <summary>
        /// Effect indexes
        /// </summary>
        public List<short> AudioMap;  

        /// <summary>
        /// Effects and their parameters
        /// </summary>
        public List<AudioEffect> AudioEffects; 

        /// <summary>
        /// Samples
        /// </summary>
        public uint[] AudioBuffers; 

        /// <summary>
        /// Channels
        /// </summary>
        public List<AudioSource> AudioSources; 

        /// <summary>
        /// Stream tracks
        /// </summary>
        public List<StreamTrack> StreamTracks; 

        /// <summary>
        /// Stream track flag map
        /// </summary>
        public List<byte> StreamTrackMap;
#endif

        /// <summary>
        /// This function is used for updating global animated texture frame
        /// </summary>
        public void UpdateAnimTextures()
        {
            foreach (var seq in AnimSequences)
            {
                if(seq.FrameLock)
                {
                    continue;
                }

                seq.FrameTime += EngineFrameTime;
                if(seq.FrameTime >= seq.FrameRate)
                {
                    var j = (int) (seq.FrameTime / seq.FrameRate);
                    seq.FrameTime -= j * seq.FrameRate;

                    switch(seq.AnimType)
                    {
                        case TR_ANIMTEXTURE.Reverse:
                            if(seq.ReverseDirection)
                            {
                                if(seq.CurrentFrame == 0)
                                {
                                    seq.CurrentFrame++;
                                    seq.ReverseDirection = false;
                                }
                                else if(seq.CurrentFrame > 0)
                                {
                                    seq.CurrentFrame--;
                                }
                            }
                            else
                            {
                                if (seq.CurrentFrame == seq.Frames.Count - 1)
                                {
                                    seq.CurrentFrame--;
                                    seq.ReverseDirection = true;
                                }
                                else if (seq.CurrentFrame < seq.Frames.Count - 1)
                                {
                                    seq.CurrentFrame++;
                                }
                                seq.CurrentFrame %= (ushort)seq.Frames.Count; // PARANOID
                            }
                            break;

                        case TR_ANIMTEXTURE.Forward: // inversed in polygon anim. texture frames
                        case TR_ANIMTEXTURE.Backward:
                            seq.CurrentFrame++;
                            seq.CurrentFrame %= (ushort)seq.Frames.Count;
                            break;
                    }
                }
            }
        }

        public unsafe void CalculateWaterTint(float[] tint, bool fixedColour)
        {
            fixed(float* ptr = tint)
                CalculateWaterTint(ptr, fixedColour);
        }

        public unsafe void CalculateWaterTint(float* tint, bool fixedColour)
        {
            if(EngineVersion < Loader.Engine.TR4) // If water room and level is TR1-3
            {
                if(EngineVersion < Loader.Engine.TR3)
                {
                    // Placeholder, color very similar to TR1 PSX ver.
                    if(fixedColour)
                    {
                        tint[0] = 0.585f;
                        tint[1] = 0.9f;
                        tint[2] = 0.9f;
                        tint[3] = 1.0f;
                    }
                    else
                    {
                        tint[0] *= 0.585f;
                        tint[1] *= 0.9f;
                        tint[2] *= 0.9f;
                    }
                }
                else
                {
                    // TOMB3 - closely matches TOMB3
                    if (fixedColour)
                    {
                        tint[0] = 0.275f;
                        tint[1] = 0.45f;
                        tint[2] = 0.5f;
                        tint[3] = 1.0f;
                    }
                    else
                    {
                        tint[0] *= 0.275f;
                        tint[1] *= 0.45f;
                        tint[2] *= 0.5f;
                    }
                }
            }
            else
            {
                if (fixedColour)
                {
                    tint[0] = 1.0f;
                    tint[1] = 1.0f;
                    tint[2] = 1.0f;
                    tint[3] = 1.0f;
                }
            }
        }

        public void AddEntity(Entity entity)
        {
            if (EntityTree.ContainsKey(entity.ID))
                return;
            EntityTree[entity.ID] = entity;
            if (entity.ID + 1 > NextEntityID)
                NextEntityID = entity.ID + 1;
        }

        public bool CreateItem(uint itemID, uint modelID, uint worldModelID, MenuItemType type, ushort count,
            string name)
        {
            var model = GetModelByID(modelID);
            if (model == null)
            {
                return false;
            }

            var bf = new SSBoneFrame();
            bf.FromModel(model);

            var item = new BaseItem
            {
                ID = itemID,
                WorldModelId = worldModelID,
                Type = type,
                Count = count,
                Name = (char) 0 + name, // TODO: Is it useful?
                BoneFrame = bf
            };

            ItemsTree[item.ID] = item;

            return true;
        }

        public int DeleteItem(uint itemID)
        {
            ItemsTree.Remove(itemID);
            return 1;
        }

        public Sprite GetSpriteByID(uint spriteID)
        {
            return Sprites.FirstOrDefault(sp => sp.ID == spriteID);
        }

        public SkeletalModel GetModelByID(uint modelID)
        {
            return SkeletalModels.FirstOrDefault(sm => sm.ID == modelID);
        }

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
            Character = null;

#if !NO_AUDIO
            AudioSources = new List<AudioSource>();
            AudioBuffers = new uint[0];
            AudioEffects = new List<AudioEffect>();
            AudioEmitters = new List<AudioEmitter>();
            AudioMap = new List<short>();
            StreamTracks = new List<StreamTrack>();
            StreamTrackMap = new List<byte>();
#endif
            AnimSequences = new List<AnimSeq>();

            RoomBoxes = new List<RoomBox>();
            CamerasSinks = new List<StatCameraSink>();
            SkeletalModels = new List<SkeletalModel>();
            SkyBox = null;
            AnimCommands = new short[0];
        }

        public void Empty()
        {
            LastContainer = null;
            EngineLua.ClearTasks();

#if !NO_AUDIO
            Audio.DeInit(); // De-initialize and destroy all audio objects.
#endif

            if(MainInventoryManager != null)
            {
                MainInventoryManager.SetInventory(null);
                MainInventoryManager.SetItemsType(MenuItemType.Supply); // see base items
            }

            if(Character != null)
            {
                Character.Self.Room = null;
                Character.CurrentSector = null;
            }

            EntityTree.Clear(); // Clearing up entities must happen before destroying rooms.

            // Destroy Bullet's MISC objects (debug spheres etc.)
            // FIXME: Hide it somewhere, it is nasty being here.

            if(BtEngineDynamicsWorld != null)
            {
                for(var i = BtEngineDynamicsWorld.NumCollisionObjects - 1; i >= 0; i--)
                {
                    var obj = BtEngineDynamicsWorld.CollisionObjectArray[i];
                    RigidBody body;
                    if((body = obj as RigidBody) != null)
                    {
                        var cont = (EngineContainer) body.UserObject;
                        body.UserObject = null;

                        if(cont != null && cont.ObjectType == OBJECT_TYPE.BulletMisc)
                        {
                            if(body.MotionState != null)
                            {
                                body.MotionState.Dispose();
                                body.MotionState = null;
                            }

                            //body.CollisionShape = null;

                            BtEngineDynamicsWorld.RemoveRigidBody(body);
                            cont.Room = null;
                            cont = null;
                            body.Dispose();
                            body = null;
                        }
                    }
                }
            }

            foreach(var room in Rooms)
            {
                room.Empty();
            }
            Rooms.Clear();

            FlipData.Clear();
            RoomBoxes.Clear();
            CamerasSinks.Clear();
            Sprites.Clear();
            ItemsTree.Clear();
            Character = null;
            SkeletalModels.Clear();
            Meshes.Clear();

            GL.DeleteTextures(Textures.Count, Textures.ToArray());
            Textures.Clear();

            TextureAtlas = null;
            AnimSequences.Clear();
        }

        public uint SpawnEntity(uint modelID, uint roomID, Vector3 pos, Vector3 angle, int id)
        {
            var model = GetModelByID(modelID);
            if(model != null)
            {
                var ent = GetEntityByID((uint)id);
                if(ent != null)
                {
                    if(pos != Vector3.Zero)
                    {
                        ent.Transform.Origin = pos;
                    }
                    if(angle != Vector3.Zero)
                    {
                        ent.Angles = angle;
                        ent.UpdateTransform();
                    }
                    if(roomID < Rooms.Count)
                    {
                        ent.Self.Room = Rooms[(int) roomID];
                        ent.CurrentSector = ent.Self.Room.GetSectorRaw(ent.Transform.Origin);
                    }
                    else
                    {
                        ent.Self.Room = null;
                    }

                    return ent.ID;
                }

                if(id < 0)
                {
                    ent = new Entity(NextEntityID);
                    EntityTree[NextEntityID] = ent;
                    NextEntityID++;
                }
                else
                {
                    ent = new Entity((uint)id);
                    if (id + 1 > NextEntityID)
                        NextEntityID = (uint)id + 1;
                }

                if (pos != Vector3.Zero)
                {
                    ent.Transform.Origin = pos;
                }
                if (angle != Vector3.Zero)
                {
                    ent.Angles = angle;
                    ent.UpdateTransform();
                }
                if (roomID < Rooms.Count)
                {
                    ent.Self.Room = Rooms[(int)roomID];
                    ent.CurrentSector = ent.Self.Room.GetSectorRaw(ent.Transform.Origin);
                }
                else
                {
                    ent.Self.Room = null;
                }

                ent.TypeFlags = ENTITY_TYPE.Spawned;
                ent.Active = ent.Enabled = true;
                ent.TriggerLayout = 0x00;
                ent.OCB = 0x00;
                ent.Timer = 0.0f;

                ent.MoveType = MoveType.StaticPos;
                ent.InertiaLinear = 0.0f;
                ent.InertiaAngular = Vector2.Zero;

                ent.Bf.FromModel(model);

                ent.SetAnimation(TR_ANIMATION.LaraRun, 0); // Set zero animation and zero frame

                Res_SetEntityProperties(ent);
                ent.RebuildBV();
                ent.GenRigidBody();

                ent.Self.Room?.AddEntity(ent);
                AddEntity(ent);
                Res_SetEntityFunction(ent);

                return ent.ID;
            }

            return 0xFFFFFFFF;
        }

        public bool DeleteEntity(uint id)
        {
            return Character.ID != id && EntityTree.Remove(id);
        }

        public Entity GetEntityByID(uint id)
        {
            return Character.ID == id ? Character : EntityTree.GetValueOrDefault(id);
        }

        public Character GetCharacterByID(uint id)
        {
            return GetEntityByID(id) as Character;
        }

        public BaseItem GetBaseItemByID(uint id)
        {
            return ItemsTree.GetValueOrDefault(id);
        }

        public Room FindRoomByPosition(Vector3 pos)
        {
            return Rooms.FirstOrDefault(r => r.Active && pos.IsBetween(r.BBMin, r.BBMax, IB.aIbE));
        }

        public Room GetByID(uint id)
        {
            return Rooms.FirstOrDefault(r => r.ID == id);
        }
    }
}
