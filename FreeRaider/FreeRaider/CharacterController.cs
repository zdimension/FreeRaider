using System;
using System.Collections.Generic;
using BulletSharp;
using OpenTK;

namespace FreeRaider
{
    /*------ Lara's model-------
                 .=.
                | 14|
                 \ / \
             / |     | \
        11  / |   7   | \  8
           /   |     |   \
           |    =====    |
        12 |    =====    | 9
           |   /  0  \   |
        13 0  /_______\  0 10
              |  | |  |
              |1 | |4 |
              |  | |  |
              |__| |__|
              |  | |  |
              |2 | |5 |
              |  | |  |
              |__| |__|
           3  |__| |__|  6
    --------------------------*/

    public enum BODY_PART : uint
    {
        BodyLow = 0x00000001, // 0
        BodyUpper = 0x00000002, // 7
        Head = 0x00000004, // 14

        LeftHand1 = 0x00000008, // 11
        LeftHand2 = 0x00000010, // 12
        LeftHand3 = 0x00000020, // 13
        RightHand1 = 0x00000040, // 8
        RightHand2 = 0x00000080, // 9
        RightHand3 = 0x00000100, // 10

        LeftLeg1 = 0x00000200, // 1
        LeftLeg2 = 0x00000400, // 2
        LeftLeg3 = 0x00000800, // 3
        RightLeg1 = 0x00001000, // 4
        RightLeg2 = 0x00002000, // 5
        RightLeg3 = 0x00004000, // 6

        Legs1 = LeftLeg1 | RightLeg1,
        Legs2 = LeftLeg2 | RightLeg2,
        Legs3 = LeftLeg3 | RightLeg3,

        Hands1 = LeftHand1 | RightHand1,
        Hands2 = LeftHand2 | RightHand2,
        Hands3 = LeftHand3 | RightHand3,

        Hands = Hands1 | Hands2 | Hands3,
        Legs = Legs1 | Legs2 | Legs3
    }

    public partial class Constants
    {
        public const int LEFT_LEG = 3;

        public const int RIGHT_LEG = 6;

        public const int LEFT_HAND = 13;

        public const int RIGHT_HAND = 10;

        public const bool CHARACTER_USE_COMPLEX_COLLISION = true;

        // Speed limits
        public const float FREE_FALL_SPEED_1 = 2000.0f;
        public const float FREE_FALL_SPEED_2 = 4500.0f;
        public const float FREE_FALL_SPEED_MAXSAFE = 5500.0f;
        public const float FREE_FALL_SPEED_CRITICAL = 7500.0f;
        public const float FREE_FALL_SPEED_MAXIMUM = 7800.0f;

        public const float MAX_SPEED_UNDERWATER = 64.0f;
        public const float MAX_SPEED_ONWATER = 24.0f;
        public const float MAX_SPEED_QUICKSAND = 5.0f;

        public const float ROT_SPEED_UNDERWATER = 2.0f;
        public const float ROT_SPEED_ONWATER = 3.0f;
        public const float ROT_SPEED_LAND = 4.5f;
        public const float ROT_SPEED_FREEFALL = 0.5f;
        public const float ROT_SPEED_MONKEYSWING = 3.5f;

        public const float INERTIA_SPEED_UNDERWATER = 1.0f;
        public const float INERTIA_SPEED_ONWATER = 1.5f;

        // Lara's character behavior constants
        public const int DEFAULT_MAX_MOVE_ITERATIONS = 3;                              //!< @fixme magic
        public const float DEFAULT_MIN_STEP_UP_HEIGHT = 128.0f;                         //!< @fixme check original
        public const float DEFAULT_MAX_STEP_UP_HEIGHT = 256.0f + 32.0f;                 //!< @fixme check original
        public const float DEFAULT_FALL_DOWN_HEIGHT = 320.0f;                         //!< @fixme check original
        public const float DEFAULT_CLIMB_UP_HEIGHT = 1920.0f;                        //!< @fixme check original
        public const float DEFAULT_CRITICAL_SLANT_Z_COMPONENT = 0.810f;                         //!< @fixme cos(alpha = 30 deg)
        public const float DEFAULT_CRITICAL_WALL_COMPONENT = -0.707f;                        //!< @fixme cos(alpha = 45 deg)
        public const float DEFAULT_CHARACTER_SLIDE_SPEED_MULT = 75.0f;                          //!< @fixme magic - not like in original
        public const float DEFAULT_CHARACTER_CLIMB_R = 32.0f;
        public const float DEFAULT_CHARACTER_WADE_DEPTH = 256.0f;

        public const float DEFAULT_CHARACTER_SWIM_DEPTH = 100.0f;

        // CHARACTER PARAMETERS DEFAULTS
        public const float PARAM_ABSOLUTE_MAX = -1;

        public const float LARA_PARAM_HEALTH_MAX = 1000.0f;      //!< 1000 HP
        public const float LARA_PARAM_AIR_MAX = 3600.0f;      //!< 60 secs of air
        public const float LARA_PARAM_STAMINA_MAX = 120.0f;       //!< 4  secs of sprint
        public const float LARA_PARAM_WARMTH_MAX = 240.0f;       //!< 8  secs of freeze
        public const float LARA_PARAM_POISON_MAX = 5.0f;

        public const float CHARACTER_BOX_HALF_SIZE = 128.0f;
        public const float CHARACTER_BASE_RADIUS = 128.0f;
        public const float CHARACTER_BASE_HEIGHT = 512.0f;
    }

    public enum SlideType
    {
        None,
        Back,
        Front
    }

    public enum StepType
    {
        DownCanHang, // enough height to hang here
        DownDrop,    // big height, cannot walk next, drop only
        DownBig,     // enough height change, step down is needed
        DownLittle,  // too little height change, step down is not needed
        Horizontal,  // horizontal plane
        UpLittle,    // too little height change, step up is not needed
        UpBig,       // enough height change, step up is needed
        UpClimb,     // big height, cannot walk next, climb only
        UpImpossible // too big height, no one ways here, or phantom case
    }

    public enum LeanType
    {
        None,
        Left,
        Right
    }

    public static partial class Extensions
    {
        public static bool IsLittleStep(this StepType type)
        {
            return type >= StepType.DownLittle && type <= StepType.UpLittle;
        }

        /// <summary>
        /// Check if the step doesn't require a drop or a climb
        /// </summary>
        public static bool IsWalkableStep(this StepType type)
        {
            return type >= StepType.DownBig && type <= StepType.UpBig;
        }
    }

    public enum CharParameters
    {
        Health,
        Air,
        Stamina,
        Warmth,
        Poison,
        Extra1,
        Extra2,
        Extra3,
        Extra4,
        Sentinel
    }

    public enum ClimbType
    {
        None,
        HandsOnly,
        FullBody
    }

    public class ClimbInfo
    {
        public StepType HeightInfo = StepType.Horizontal;

        public bool CanHang = false;

        public Vector3 Point;

        public Vector3 N;

        public Vector3 Right;

        public Vector3 Up;

        public float FloorLimit;

        public float CeilingLimit;

        public float NextZSpace = 0;

        public ClimbType WallHit = ClimbType.None;

        public bool EdgeHit = false;

        public Vector3 EdgePoint;

        public Vector3 EdgeNormale;

        public Vector3 EdgeTanXY;

        public float EdgeZAngle;

        public CollisionObject EdgeObject = null;
    }

    public enum QuicksandPosition
    {
        None,
        Sinking,
        Drowning
    }

    public class HeightInfo
    {
        public BtEngineClosestRayResultCallback Cb;

        public ClosestConvexResultCallback Ccb;

        public ConvexShape Sp = new SphereShape(16.0f);

        public bool CeilingClimb = false;

        public bool WallsClimb = false;

        public sbyte WallsClimbDir = 0;

        public Vector3 FloorNormale = new Vector3(0, 0, 1);

        public Vector3 FloorPoint = Vector3.Zero;

        public bool FloorHit = false;

        public CollisionObject FloorObject = null;

        public Vector3 CeilingNormale = new Vector3(0, 0, -1);

        public Vector3 CeilingPoint = Vector3.Zero;

        public bool CeilingHit = false;

        public CollisionObject CeilingObject = null;

        public float TransitionLevel;

        public bool Water = false;

        public QuicksandPosition Quicksand = QuicksandPosition.None;

        public HeightInfo()
        {
            Sp.Margin = Constants.COLLISION_MARGIN_DEFAULT;
        }
    }

    public class CharacterCommand
    {
        public Vector3 Rotation = Vector3.Zero;

        public sbyte[] Move = {0, 0, 0};

        public bool Roll = false;

        public bool Jump = false;

        public bool Crouch = false;

        public bool Shift = false;

        public bool Action = false;

        public bool ReadyWeapon = false;

        public bool Sprint = false;
    }

    public class CharacterResponse
    {
        public bool Killed;

        public sbyte VerticalCollide = 0;

        public sbyte HorizontalCollide = 0;

        public SlideType Slide = SlideType.None;

        public LeanType Lean = LeanType.None;
    }

    public class CharacterParam
    {
        /// <summary>
        /// Length=(int) CharParameters.Sentinel
        /// </summary>
        public float[] Param;

        /// <summary>
        /// Length=(int) CharParameters.Sentinel
        /// </summary>
        public float[] Maximum;

        public CharacterParam()
        {
            Param = Helper.FillArray(0.0f, (int) CharParameters.Sentinel);
            Maximum = Helper.FillArray(0.0f, (int) CharParameters.Sentinel);
        }
    }

    public class CharacterStats
    {
        public float Distance;

        /// <summary>
        /// Level amount of secrets
        /// </summary>
        public uint SecretsLevel;

        /// <summary>
        /// Overall amount of secrets
        /// </summary>
        public uint SecretsGame;

        /// <summary>
        /// Number of ammunition used
        /// </summary>
        public uint AmmoUsed;

        public uint Hits;

        public uint Kills;

        /// <summary>
        /// Number of medipacks used<br/>
        /// Small = 0.5<br/>
        /// Big = 1
        /// </summary>
        public float MedipacksUsed;

        public uint SavesUsed;
    }

    public class InventoryNode
    {
        public uint ID;

        public int Count;

        public uint MaxCount;
    }

    public enum WeaponState
    {
        Hide,
        HideToReady,
        Idle,
        IdleToFire,
        Fire,
        FireToIdle,
        IdleToHide
    }

    public class Character : Entity
    {
        /// <summary>
        /// Character control commands
        /// </summary>
        public CharacterCommand Command;

        /// <summary>
        /// Character response info (collides, slide, next steps, drops, e.t.c.)
        /// </summary>
        public CharacterResponse Response;

        public List<InventoryNode> Inventory;

        public CharacterParam Parameters = null;

        public CharacterStats Statistics;

        public List<Hair> Hairs = new List<Hair>();

        public int CurrentWeapon = 0;

        public WeaponState WeaponCurrentState = WeaponState.Hide;

        public Func<Character, SSAnimation, int> StateFunc = (c, a) => 0;

        public sbyte CamFollowCenter = 0;

        public float MinStepUpHeight = Constants.DEFAULT_MIN_STEP_UP_HEIGHT;

        public float MaxStepUpHeight = Constants.DEFAULT_MAX_STEP_UP_HEIGHT;

        public float MaxClimbHeight = Constants.DEFAULT_CLIMB_UP_HEIGHT;

        public float FallDownHeight = Constants.DEFAULT_FALL_DOWN_HEIGHT;

        public float CriticalSlantZComponent = Constants.DEFAULT_CRITICAL_SLANT_Z_COMPONENT;

        public float CriticalWallComponent = Constants.DEFAULT_CRITICAL_WALL_COMPONENT;

        /// <summary>
        /// Climbing sensor radius
        /// </summary>
        public float ClimbR = Constants.DEFAULT_CHARACTER_CLIMB_R;

        /// <summary>
        /// Offset for climbing calculation
        /// </summary>
        public float ForwardSize = 48;

        /// <summary>
        /// Base character height
        /// </summary>
        public float Height = Constants.CHARACTER_BASE_HEIGHT;

        /// <summary>
        /// Water depth that enable wade walk
        /// </summary>
        public float WadeDepth = Constants.DEFAULT_CHARACTER_WADE_DEPTH;

        /// <summary>
        /// Depth offset for starting to swim
        /// </summary>
        public float SwimDepth = Constants.DEFAULT_CHARACTER_SWIM_DEPTH;

        public SphereShape Sphere = new SphereShape(Constants.CHARACTER_BASE_RADIUS);

        public SphereShape ClimbSensor;

        public HeightInfo HeightInfo = null;

        public ClimbInfo Climb = null;

        public Entity TraversedObject = null;

        public BtEngineClosestRayResultCallback RayCb;

        public BtEngineClosestConvexResultCallback ConvexCb;

        public Character(uint id);

        ~Character();

        public int CheckNextPenetration(Vector3 move);

        public void DoWeaponFrame(float time);

        public override void FixPenetrations(Vector3 move);

        public override Vector3 GetRoomPos()
        {
            var pos = Transform * Bf.BoneTags[0].FullTransform.Origin;
            pos.X = Transform.Origin.X;
            pos.Y = Transform.Origin.Y;
            return pos;
        }

        public override void TransferToRoom(Room room)
        {
        }

        /// <summary>
        /// Constantly updates some specific parameterd to keep hair aligned to entity
        /// </summary>
        public override void UpdateHair();

        public override void FrameImpl(float time, short frame, int state);

        public override void ProcessSectorImpl();

        public override void Jump(float vert, float hor);

        public override void Kill()
        {
            Response.Killed = true;
        }

        public override Substance GetSubstanceState();

        public override void UpdateTransform()
        {
            GhostUpdate();
            base.UpdateTransform();
        }

        public override void UpdateGhostRigidBody();

        public override BtEngineClosestConvexResultCallback CallbackForCamera()
        {
            return ConvexCb;
        }

        public override Vector3 CamPosForFollowing(float dz);

        /// <returns>Item count after the function's end</returns>
        public int AddItem(uint itemID, int count);

        /// <returns>Item count after the function's end</returns>
        public int RemoveItem(uint itemID, int count);


        public int RemoveAllItems();

        /// <returns>Item count</returns>
        public int GetItemsCount(uint itemID);

        public static void GetHeightInfo(Vector3 pos, HeightInfo fc, float vOffset = 0.0f);

        public StepType CheckNextStep(Vector3 offset, HeightInfo nfc);

        public bool HasStopSlant(HeightInfo nextFc);

        public ClimbInfo CheckClimbability(Vector3 offset, HeightInfo nfc, float testHeight);

        public ClimbInfo CheckWallsClimbability();

        public void UpdateCurrentHeight();

        public override void UpdatePlatformPreStep();

        public void UpdatePlatformPostStep();

        public void Lean(CharacterCommand cmd, float maxLen);

        public float GetInertiaLinear(float maxSpeed, float accel, bool command);

        public float GetInertiaAngular(float maxAngle, float accel, byte axis);

        public int MoveOnFloor();

        public int FreeFalling();

        public int MonkeyClimbing();

        public int WallsClimbing();

        public int Climbing();

        public int MoveUnderwater();

        public int MoveOnWater();

        public int FindTraverse();

        public int CheckTraverse(Entity obj);

        public const int TraverseNone = 0x00;

        public const int TraverseForward = 0x01;

        public const int TraverseBackward = 0x02;

        public void ApplyCommands();

        public void UpdateParams();

        public float GetParam(CharParameters parameter);

        public int SetParam(CharParameters parameter, float value);

        public int ChangeParam(CharParameters parameter, float value);

        public int SetParamMaximum(CharParameters parameter, float maxValue);

        public int SetWeaponModel(CharParameters weaponModel, int armed);
    }

    public partial class StaticFuncs
    {
        public static bool IsCharacter(Entity ent);

        public static int Sector_AllowTraverse(RoomSector rs, float floor, EngineContainer cont);
    }
}

