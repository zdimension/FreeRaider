using System;
using System.Collections.Generic;
using BulletSharp;
using OpenTK;

namespace FreeRaider
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
        RoomCollision = 0x00000020
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

    public class EntityCollisionNode
    {
        public List<CollisionObject> Obj;
    }

    public class BtEntityData
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

        public RoomSector CurrentSector;

        public RoomSector LastSector;

        public EngineContainer Self;

        /// <summary>
        /// Where we can activate object (dx, dy, dz)
        /// </summary>
        public Vector3 ActivationOffset = new Vector3(0, 256, 0);

        public float ActivationRadius = 128;

        public Entity(uint id);

        ~Entity();

        public void CreateGhosts()
        {
            
        }

        public void Enable();

        public void Disable();

        public void EnableCollision();

        public void DisableCollision();

        public void GenRigidBody();

        public void GhostUpdate();

        public void UpdateCurrentCollisions();

        public int GetPenetrationFixVector(Vector3 reaction, bool hasMove);

        public void CheckCollisionCallback();

        public bool WasCollisionBodyParts(uint partsFlags);

        public void CleanCollisionAllBodyParts();

        public void CleanCollisionBodyParts(uint partsFlags);

        public CollisionObject GetRemoveCollisionBodyParts(uint partsFlags, uint currFlag);

        public void UpdateRoomPos();

        public void UpdateRigidBody(bool force);

        public void RebuildBV();

        public int GetAnimDispatchCase(uint id);

        public static void GetNextFrame(SSBoneFrame bf, float time, StateChange stc, out short frame, out short anim,
            ushort animFlags);

        /// <summary>
        /// Process frame + trying to change state
        /// </summary>
        public int Frame(float time);

        public virtual void UpdateTransform();

        public void UpdateCurrentSpeed(bool zeroVz = false);

        public void AddOverrideAnim(int modelID);

        public void CheckActivators();

        public virtual Substance GetSubstanceState()
        {
            return Substance.None;
        }

        public static void UpdateCurrentBoneFrame(SSBoneFrame bf, Transform etr);

        public void DoAnimCommands(SSAnimation ssAnim, int changing);

        public void ProcessSector();

        public void SetAnimation(int animation, int frame = 0, int anotherModel = -1);

        public void MoveForward(float dist);

        public void MoveStrafe(float dist);

        public void MoveVertical(float dist);

        public float FindDistance(Entity entity2);

        /// <summary>
        /// Constantly updates some specific parameterd to keep hair aligned to entity
        /// </summary>
        public virtual void UpdateHair()
        {
        }

        public bool CreateRagdoll(RDSetup setup);

        public bool DeleteRagdoll();

        public virtual void FixPenetrations(Vector3 move);

        public virtual Vector3 GetRoomPos()
        {
            return Transform * ((Bf.BBMin + Bf.BBMax) / 2);
        }

        public virtual void TransferToRoom(Room room);

        public virtual void FrameImpl(float time, short frame, int state)
        {
            Bf.Animations.CurrentFrame = frame;
        }

        public virtual void ProcessSectorImpl()
        {
        }

        public virtual void Jump(float vert, float hor)
        {
        }

        public virtual void Kill()
        {
        }

        public virtual void UpdateGhostRigidBody()
        {
        }

        public virtual BtEngineClosestConvexResultCallback CallbackForCamera();

        public virtual Vector3 CamPosForFollowing(float dz)
        {
            var camPos = Transform / Bf.BoneTags[0].FullTransform.Origin;
            camPos[2] += dz;
            return camPos;
        }
        
        public virtual void UpdatePlatformPreStep()
        {
        }

        public Vector3 ApplyGravity(float time);

        private void doAnimMove(short anim, short frame);

        private static float GetInnerBBRadius(Vector3 bbMin, Vector3 bbMax)
        {
            var d = bbMax - bbMin;
            return Math.Min(d.X, Math.Min(d.Y, d.Z));
        }
    }

    public partial class StaticFuncs
    {
        public static int GhostGetPenetrationFixVector(PairCachingGhostObject ghost, AlignedManifoldArray manifoldArray,
           Vector3 correction);

        public static StateChange Anim_FindStateChangeByAnim(AnimationFrame anim, int stateChangeAnim);

        public static StateChange Anim_FindStateChangeByID(AnimationFrame anim, uint id);
    }
}
