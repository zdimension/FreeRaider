using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BulletSharp;
using OpenTK;

namespace UniRaider
{
    public enum ENTITY_ANIM : short
    {
        None = 0x00,
        NewFrame = 0x01,
        NewAnim = 0x02
    }

    [Flags]
    public enum ENTITY_TYPE : ushort
    {
        /// <summary>
        /// Just an animating.
        /// </summary>
        Generic = 0x0000,
        /// <summary>
        /// Can respond to other entity's commands.
        /// </summary>
        Interactive = 0x0001,
        /// <summary>
        /// Can activate triggers.
        /// </summary>
        TriggerActivator = 0x0002,
        /// <summary>
        /// Can activate heavy triggers.
        /// </summary>
        HeavyTriggerActivator = 0x0004,
        /// <summary>
        /// Can be picked up.
        /// </summary>
        Pickable = 0x0008,
        /// <summary>
        /// Can be pushed/pulled.
        /// </summary>
        Traverse = 0x0010,
        /// <summary>
        /// Can be walked upon.
        /// </summary>
        TraverseFloor = 0x0020,
        /// <summary>
        /// Acts as a physical dynamic object.
        /// </summary>
        Dynamic = 0x0040,
        /// <summary>
        /// Is actor.
        /// </summary>
        Actor = 0x0080,
        /// <summary>
        /// Does collision checks for itself.
        /// </summary>
        CollCheck = 0x0100,
        /// <summary>
        /// Was spawned.
        /// </summary>
        Spawned = 0x8000
    }

    [Flags]
    public enum ENTITY_CALLBACK : uint
    {
        None = 0x00000000,
        Activate = 0x00000001,
        Deactivate = 0x00000002,
        Collision = 0x00000004,
        Stand = 0x00000008,
        Hit = 0x00000010,
        RoomCollision = 0x00000020,
    }

    public enum Substance
    {
        None,
        WaterShallow,
        WaterWade,
        WaterSwim,
        QuicksandShallow,
        QuicksandConsumed
    }

    public enum ENTITY_TLAYOUT : byte
    {
        /// <summary>
        /// Activation mask
        /// </summary>
        Mask = 0x1F,
        /// <summary>
        /// Last trigger event
        /// </summary>
        Event = 0x20,
        /// <summary>
        /// Activity lock
        /// </summary>
        Lock = 0x40,
        /// <summary>
        /// Sector status
        /// </summary>
        SectorStatus = 0x80
    }

    public struct EntityCollisionNode
    {
        public List<CollisionObject> Obj;
    }

    public struct BtEntityData
    {
        public bool NoFixAll;

        public uint NoFixBodyParts;

        /// <summary>
        /// Like Bullet character controller for penetration resolving.
        /// </summary>
        public List<PairCachingGhostObject> GhostObjects;

        /// <summary>
        /// Keep track of the contact manifolds
        /// </summary>
        public AlignedManifoldArray ManifoldArray;

        public List<CollisionShape> Shapes;

        public List<RigidBody> BtBody;

        /// <summary>
        /// Ragdoll joints
        /// </summary>
        public List<TypedConstraint> BtJoints;

        public List<EntityCollisionNode> LastCollisions;
    }

    /// <summary>
    /// Entity movement types
    /// </summary>
    public enum MoveType
    {
        StaticPos,
        Kinematic,
        OnFloor,
        Wade,
        Quicksand,
        OnWater,
        Underwater,
        FreeFalling,
        Climbing,
        Monkeyswing,
        WallsClimb,
        Dozy
    }

    public class Entity
    {
        /// <summary>
        /// Unique entity ID
        /// </summary>
        public uint ID { get; }

        /// <summary>
        /// Object code bit (since TR4)
        /// </summary>
        public int OCB;

        /// <summary>
        /// Mask + once + event + sector status flags
        /// </summary>
        public ENTITY_TLAYOUT TriggerLayout;

        /// <summary>
        /// Set by "timer" trigger field
        /// </summary>
        public float Timer;

        /// <summary>
        /// Information about script callbacks
        /// </summary>
        public ENTITY_CALLBACK CallbackFlags;

        public ENTITY_TYPE TypeFlags = ENTITY_TYPE.Generic;

        public bool Enabled = true;

        public bool Active = true;

        public bool Visible = true;

        /// <summary>
        /// Move direction
        /// </summary>
        public ENT_MOVE DirFlag = ENT_MOVE.Stay;

        /// <summary>
        /// On floor / free fall / swim / .....
        /// </summary>
        public MoveType MoveType = MoveType.StaticPos;

        /// <summary>
        /// Render once per frame trigger
        /// </summary>
        public bool WasRendered;

        /// <summary>
        /// Same for debug lines
        /// </summary>
        public bool WasRenderedLines;

        /// <summary>
        /// Current linear speed from animation info
        /// </summary>
        public float CurrentSpeed;

        /// <summary>
        /// Speed of the entity XYZ
        /// </summary>
        public Vector3 Speed;

        public float SpeedMult = Constants.TR_FRAME_RATE;

        /// <summary>
        /// Linear inertia
        /// </summary>
        public float InertiaLinear;

        /// <summary>
        /// Angular inertia - X and Y axes
        /// </summary>
        public Vector2 InertiaAngular;

        /// <summary>
        /// Current bone frame with full frame information
        /// </summary>
        public SSBoneFrame Bf;

        public BtEntityData Bt;

        public Vector3 Angles;

        /// <summary>
        /// GL transformation matrix
        /// </summary>
        public Transform Transform;

        public Vector3 Scaling = Vector3.One;

        /// <summary>
        /// Oriented bounding box
        /// </summary>
        public OBB OBB;
    }
}
