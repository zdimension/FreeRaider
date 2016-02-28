using System;
using BulletSharp;
using FreeRaider.Loader;

namespace FreeRaider
{
    public partial class Constants
    {
        public const int MAX_ENGINE_PATH = 1024;

        public const int COLLISION_NONE = 0x0000;

        public const int COLLISION_MASK_ALL = 0x7FFF; // bullet uses signed short int for these flags!

        public const float COLLISION_MARGIN_DEFAULT = 2.0f;

        public const float COLLISION_MARGIN_RIGIDBODY = 0.5f;

        public const float COLLISION_GHOST_VOLUME_COEFFICIENT = 0.4f;

        public const float COLLISION_CAMERA_SPHERE_RADIUS = 16.0f;

        public const float COLLISION_TRAVERSE_TEST_RADIUS = 0.48f;
    }

    public enum LEVEL_FORMAT
    {
        PC = 0,
        PSX = 1,
        DC = 2,
        OPENTOMB = 3 // Maybe some day...
    }

    public enum OBJECT_TYPE : ushort
    {
        None = 0,
        StaticMesh = 0x0001,
        RoomBase = 0x0002,
        Entity = 0x0003,
        Hair = 0x0004,
        BulletMisc = 0x7FFF
    }

    public enum COLLISION_SHAPE
    {
        None = 0,
        Box = 0x0001,
        BoxBase = 0x0002, // use single box collision
        Sphere = 0x0003,
        Trimesh = 0x0004, // for static objects and room's!
        TrimeshConvex = 0x0005 // for dynamic objects
    }

    public enum COLLISION_TYPE
    {
        None = 0x0000,
        Static = 0x0001, // static object - never moved
        Kinematic = 0x0003, // doors and other moveable statics
        Dynamic = 0x0005, // hellow full physics interaction
        Actor = 0x0007, // actor, enemies, npc, animals
        Vehicle = 0x0009, // car, moto, bike
        Ghost = 0x000B
        // no fix character position, but works in collision callbacks and interacts with dynamic objects
    }

    public enum COLLISION_GROUP
    {
        All = 0x7FFF,
        Static = 0x0001, // room mesh, statics
        Kinematic = 0x0002, // doors, blocks, static animated entityes
        Characters = 0x0004, // lara, enemies, friends, creatures
        Bullets = 0x0008, // bullets, rockets, grenades, arrows...
        Dynamics = 0x0010 // test balls, warious
    }

    public class EngineContainer
    {
        public OBJECT_TYPE ObjectType = OBJECT_TYPE.None;

        public COLLISION_TYPE CollisionType = COLLISION_TYPE.None;

        public COLLISION_SHAPE CollisionShape = COLLISION_SHAPE.None;

        public Object Object = null;

        public Room Room = null;
    }

    /// <summary>
    /// TODO: Use bools where appropriate
    /// </summary>
    public class EngineControlState
    {
        public bool FreeLook = false;

        public float FreeLookSpeed = 0;

        public bool MouseLook = false;

        public float CamDistance = 800;

        public bool NoClip = false;

        #region Unified look axis data

        public float LookAxisX = 0;

        public float LookAxisY = 0;

        #endregion

        #region Directional movement keys

        public bool MoveForward = false;

        public bool MoveBackward = false;

        public bool MoveLeft = false;

        public bool MoveRight = false;

        public bool MoveUp = false; // These are not typically used.

        public bool MoveDown = false;

        #endregion

        #region Look (camera) keys

        public bool LookUp = false;

        public bool LookDown = false;

        public bool LookLeft = false;

        public bool LookRight = false;

        public bool LookRollLeft = false;

        public bool LookRollRight = false;

        #endregion

        #region Eventual actions

        public bool DoJump = false;

        public bool DoDrawWeapon = false;

        public bool DoRoll = false;

        #endregion

        #region Conditional actions

        public bool StateAction = false;

        public bool StateWalk = false;

        public bool StateSprint = false;

        public bool StateCrouch = false;

        public bool UseBigMedipack = false;

        public bool UseSmallMedipack = false;

        public bool GuiInventory = false;

        #endregion
    }

    public partial class Global
    {
        public static EngineControlState ControlStates;

        public static ControlSettings ControlMapper;

        public static AudioSettings AudioSettings;

        public static float EngineFrameTime;

        public static Camera EngineCamera;

        public static World EngineWorld;

        public static DefaultCollisionConfiguration BtEngineCollisionConfiguration;

        public static CollisionDispatcher BtEngineDispatcher;

        public static BroadphaseInterface BtEngineOverlappingPairCache;

        public static SequentialImpulseConstraintSolver BtEngineSolver;

        public static DiscreteDynamicsWorld BtEngineDynamicsWorld;
    }

    public class BtEngineClosestRayResultCallback : ClosestRayResultCallback
    {
        public BtEngineClosestRayResultCallback(EngineContainer cont, bool skipGhost = false)
            : base(ref Helper.ZeroW, ref Helper.ZeroW)
        {
            Container = cont;
            SkipGhost = skipGhost;
        }

        public override float AddSingleResult(LocalRayResult rayResult, bool normalInWorldSpace)
        {
            var c1 = rayResult.CollisionObject.UserObject as EngineContainer;

            if ((c1 == null && c1 == Container) || (SkipGhost && c1.CollisionType == COLLISION_TYPE.Ghost))
            {
                return 1.0f;
            }

            var r0 = Container?.Room;
            var r1 = c1?.Room;

            if (r0 == null || r1 == null)
            {
                return base.AddSingleResult(rayResult, normalInWorldSpace);
            }

            if (r0 != null && r1 != null)
            {
                if (r0.IsInNearRoomsList(r1))
                {
                    return base.AddSingleResult(rayResult, normalInWorldSpace);
                }
            }

            return 1.0f;
        }

        public EngineContainer Container;

        public bool SkipGhost;
    }

    public class BtEngineClosestConvexResultCallback : ClosestConvexResultCallback
    {
        public BtEngineClosestConvexResultCallback(EngineContainer cont, bool skipGhost = false)
            : base(ref Helper.ZeroW, ref Helper.ZeroW)
        {
            Container = cont;
            SkipGhost = skipGhost;
        }

        public override float AddSingleResult(LocalConvexResult convexResult, bool normalInWorldSpace)
        {
            var r0 = Container?.Room;
            var c1 = convexResult.HitCollisionObject.UserObject as EngineContainer;
            var r1 = c1?.Room;

            if ((c1 == null && c1 == Container) || (SkipGhost && c1.CollisionType == COLLISION_TYPE.Ghost))
            {
                return 1.0f;
            }

            if (r0 == null || r1 == null)
            {
                return base.AddSingleResult(convexResult, normalInWorldSpace);
            }

            if (r0 != null && r1 != null)
            {
                if (r0.IsInNearRoomsList(r1))
                {
                    return base.AddSingleResult(convexResult, normalInWorldSpace);
                }
            }

            return 1.0f;
        }

        public EngineContainer Container;

        public bool SkipGhost;
    }

    public class Engine
    {
        #region Starter and destructor

        public static void Start();

        public static void Destroy();

        public static void Shutdown(int val);

        #endregion

        #region Initializers

        /// <summary>
        /// Initial init
        /// </summary>
        public static void InitPre();

        /// <summary>
        /// Finalizing init
        /// </summary>
        public static void InitPost();

        public static void InitDefaultGlobals();

        public static void InitGL();

        public static void InitSDLControls();

        public static void InitSDLVideo();

        public static void InitSDLImage();

        public static void InitAL();

        public static void InitBullet();

        #endregion

        #region Config parser

        public static void InitConfig(string filename);

        public static void SaveConfig();

        #endregion

        #region Core system routines - display and tick

        public static void Display();

        public static void Frame(float time);

        #endregion

        #region Resize event

        // Nominal values are used e.g. to set the size for the console.
        // pixel values are used for glViewport. Both will be the same on
        // normal displays, but on retina displays or similar, pixels will be twice nominal (or more).
        public static void Resize(int nominalW, int nominalH, int pixelsW, int pixelsH);

        #endregion

        #region Debug functions

        public static void PrimaryMouseDown();

        public static void SecondaryMouseDown();

        public static void ShowDebugInfo();

        public static void DumpRoom(Room r);

        #endregion

        #region PC-specific level loader routines

        public static bool LoadPCLevel(string name);

        #endregion

        #region General level loading routines

        public static bool FileFound(string name, bool write = false);

        public static int GetLevelFormat(string name);

        public static int LoadMap(string name);

        #endregion

        #region String getters

        public static string GetLevelName(string path);

        public static string GetAutoexecName(Game gameVersion, string postfix = "");

        #endregion

        #region Console command parser

        public static int ExecCmd(string ch);

        #endregion

        #region Bullet global methods

        public static void RoomNearCallback(BroadphasePair collisionPair, CollisionDispatcher dispatcher,
            DispatcherInfo dispatchInfo);

        public static void InternalTickCallback(DynamicsWorld world, float timeStep);

        #endregion
    }
}
