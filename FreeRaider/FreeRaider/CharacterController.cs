using System;
using System.Collections.Generic;
using System.Linq;
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

    [Flags]
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

        public const int TraverseNone = 0x00;

        public const int TraverseForward = 0x01;

        public const int TraverseBackward = 0x02;
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

        public Character(uint id)
            : base(id)
        {
            Sphere.Margin = Constants.COLLISION_MARGIN_DEFAULT;

            ClimbSensor = new SphereShape(ClimbR);
            ClimbSensor.Margin = Constants.COLLISION_MARGIN_DEFAULT;

            RayCb = new BtEngineClosestRayResultCallback(Self, true);
            RayCb.CollisionFilterMask = CollisionFilterGroups.StaticFilter | CollisionFilterGroups.KinematicFilter;
            HeightInfo.Cb = RayCb;

            ConvexCb = new BtEngineClosestConvexResultCallback(Self, true);
            ConvexCb.CollisionFilterMask = CollisionFilterGroups.StaticFilter | CollisionFilterGroups.KinematicFilter;
            HeightInfo.Ccb = ConvexCb;

            DirFlag = ENT_MOVE.Stay;
        }

        ~Character()
        {
            if(Self.Room != null && this != Global.EngineWorld.Character)
            {
                Self.Room.RemoveEntity(this);
            }
        }

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
        public int AddItem(uint itemID, int count)
        {
            Gui.NotifierStart(itemID);

            var item = Global.EngineWorld.GetBaseItemByID(itemID);
            if (item == null)
                return 0;

            count = count < 0 ? item.Count : count;

            var it = Inventory.FirstOrDefault(x => x.ID == itemID);
            if(it != null)
            {
                it.Count += count;
                return it.Count;
            }

            var i = new InventoryNode();
            i.ID = itemID;
            i.Count = count;
            Inventory.Add(i);

            return count;
        }

        /// <returns>Item count after the function's end</returns>
        public int RemoveItem(uint itemID, int count)
        {
            if (Inventory.Count == 0)
                return 0;

            foreach (var it in Inventory.Where(it => it.ID == itemID).ToList())
            {
                if(it.Count > count)
                {
                    it.Count -= count;
                    return it.Count;
                }
                else if(it.Count == count)
                {
                    Inventory.Remove(it);
                }
                else
                {
                    return it.Count - count;
                }
            }

            return -count;
        }


        public int RemoveAllItems()
        {
            var ret = Inventory.Count;
            Inventory.Clear();
            return ret;
        }

        /// <returns>Item count</returns>
        public int GetItemsCount(uint itemID)
        {
            return Inventory.FirstOrDefault(item => item.ID == itemID)?.Count ?? 0;
        }

        /// <summary>
        /// Start position are taken from transform
        /// </summary>
        public static void GetHeightInfo(Vector3 pos, HeightInfo fc, float vOffset = 0.0f)
        {
            var cb = fc.Cb;
            var r = cb.Container?.Room;
            RoomSector rs;

            fc.FloorHit = false;
            fc.CeilingHit = false;
            fc.Water = false;
            fc.Quicksand = QuicksandPosition.None;
            fc.TransitionLevel = 32512.0f;

            r = Room.FindPosCogerrence(pos, r)?.CheckFlip();
            if(r != null)
            {
                rs = r.GetSectorXYZ(pos);
                if(r.Flags.HasFlagUns(RoomFlag.Water))
                {
                    while(rs.SectorAbove != null)
                    {
                        Assert.That(rs.SectorAbove != null);
                        rs = rs.SectorAbove.CheckFlip();
                        Assert.That(rs?.OwnerRoom != null);
                        if((rs.OwnerRoom.Flags & (uint)RoomFlag.Water) == 0x00)
                        {
                            fc.TransitionLevel = rs.Floor;
                            fc.Water = true;
                            break;
                        }
                    }
                }
                else if (r.Flags.HasFlagUns(RoomFlag.Quicksand))
                {
                    while (rs.SectorAbove != null)
                    {
                        Assert.That(rs.SectorAbove != null);
                        rs = rs.SectorAbove.CheckFlip();
                        Assert.That(rs?.OwnerRoom != null);
                        if ((rs.OwnerRoom.Flags & (uint)RoomFlag.Quicksand) == 0x00)
                        {
                            fc.TransitionLevel = rs.Floor;
                            fc.Quicksand = fc.TransitionLevel - fc.FloorPoint.Z > vOffset
                                ? QuicksandPosition.Drowning
                                : QuicksandPosition.Sinking;
                            break;
                        }
                    }
                }
                else
                {
                    while (rs.SectorBelow != null)
                    {
                        Assert.That(rs.SectorBelow != null);
                        rs = rs.SectorBelow.CheckFlip();
                        Assert.That(rs?.OwnerRoom != null);
                        if ((rs.OwnerRoom.Flags & (uint)RoomFlag.Water) != 0x00)
                        {
                            fc.TransitionLevel = rs.Ceiling;
                            fc.Water = true;
                            break;
                        }
                        else if ((rs.OwnerRoom.Flags & (uint)RoomFlag.Quicksand) != 0x00)
                        {
                            fc.TransitionLevel = rs.Ceiling;
                            fc.Quicksand = fc.TransitionLevel - fc.FloorPoint.Z > vOffset
                                ? QuicksandPosition.Drowning
                                : QuicksandPosition.Sinking;
                            break;
                        }
                    }
                }
            }

            // get heights
            var from = pos;
            var to = from;
            to.Z -= 4096.0f;
            cb.ClosestHitFraction = 1.0f;
            cb.CollisionObject = null;
            Global.BtEngineDynamicsWorld.RayTest(from, to, cb);
            fc.FloorHit = cb.HasHit;
            if(fc.FloorHit)
            {
                fc.FloorNormale = cb.HitNormalWorld;
                Helper.SetInterpolate3(ref fc.FloorPoint, from, to, cb.ClosestHitFraction);
                fc.FloorObject = cb.CollisionObject;
            }

            to = from;
            to.Z += 4096.0f;
            cb.ClosestHitFraction = 1.0f;
            cb.CollisionObject = null;
            Global.BtEngineDynamicsWorld.RayTest(from, to, cb);
            fc.CeilingHit = cb.HasHit;
            if (fc.CeilingHit)
            {
                fc.CeilingNormale = cb.HitNormalWorld;
                Helper.SetInterpolate3(ref fc.CeilingPoint, from, to, cb.ClosestHitFraction);
                fc.CeilingObject = cb.CollisionObject;
            }
        }

        /// <summary>
        /// Calculates next floor info + phantom filter + returns step info.
        /// Current height info must be calculated!
        /// </summary>
        public StepType CheckNextStep(Vector3 offset, HeightInfo nfc)
        {
            float delta;
            Vector3 from, to;
            StepType ret;

            var pos = Transform.Origin + offset;
            GetHeightInfo(pos, nfc);

            if (HeightInfo.FloorHit && nfc.FloorHit)
            {
                delta = nfc.FloorPoint.Z - HeightInfo.FloorPoint.Z;
                if (Math.Abs(delta) < Constants.SPLIT_EPSILON)
                {
                    from.Z = HeightInfo.FloorPoint.Z;
                    ret = StepType.Horizontal;
                }
                else if (delta < 0.0f)
                {
                    delta = -delta;
                    from.Z = HeightInfo.FloorPoint.Z;
                    if (delta <= MinStepUpHeight)
                    {
                        ret = StepType.DownLittle;
                    }
                    else if (delta <= MaxStepUpHeight)
                    {
                        ret = StepType.DownBig;
                    }
                    else if (delta <= Height)
                    {
                        ret = StepType.DownDrop;
                    }
                    else
                    {
                        ret = StepType.DownCanHang;
                    }
                }
                else
                {
                    from.Z = nfc.FloorPoint.Z;
                    if (delta <= MinStepUpHeight)
                    {
                        ret = StepType.UpLittle;
                    }
                    else if (delta <= MaxStepUpHeight)
                    {
                        ret = StepType.UpBig;
                    }
                    else if (delta <= Height)
                    {
                        ret = StepType.UpClimb;
                    }
                    else
                    {
                        ret = StepType.UpImpossible;
                    }
                }
            }
            else if (!HeightInfo.FloorHit && !nfc.FloorHit)
            {
                from.Z = pos.Z;
                ret = StepType.Horizontal;
            }
            else if (!HeightInfo.FloorHit && nfc.FloorHit)
            {
                from.Z = nfc.FloorPoint.Z;
                ret = StepType.Horizontal;
            }
            else
            {
                from.Z = HeightInfo.FloorPoint.Z;
                ret = StepType.DownCanHang;
            }

            from.Z += ClimbR;
            to.Z = from.Z;
            from.X = Transform.Origin.X;
            from.Y = Transform.Origin.Y;
            to.X = from.X;
            to.Y = from.Y;
            HeightInfo.Cb.ClosestHitFraction = 1.0f;
            HeightInfo.Cb.CollisionObject = null;
            Global.BtEngineDynamicsWorld.RayTest(from, to, HeightInfo.Cb);
            if(HeightInfo.Cb.HasHit)
            {
                ret = StepType.UpImpossible;
            }

            return ret;
        }

        public bool HasStopSlant(HeightInfo nextFc)
        {
            var pos = Transform.Origin;
            var forward = Transform.Basis.Column1;
            var floor = nextFc.FloorNormale;

            return nextFc.FloorPoint.Z > pos.Z
                   && nextFc.FloorNormale.Z < CriticalSlantZComponent
                   && forward.X * floor.X + forward.Y * floor.Y < 0.0f;
        }

        public ClimbInfo CheckClimbability(Vector3 offset, HeightInfo nfc, float testHeight)
        {
            Vector3 from, to;
            float d;
            var pos = Transform.Origin;
            var t1 = new Transform();
            var t2 = new Transform();
            byte upFounded;
            var castRay = new float[6];

            // init callbacks functions
            nfc.Cb = RayCb;
            nfc.Ccb = ConvexCb;
            var tmp = pos + offset;

            var ret = new ClimbInfo();
            ret.HeightInfo = CheckNextStep(offset + new Vector3(0, 0, 128), nfc);
            ret.CanHang = false;
            ret.EdgeHit = false;
            ret.EdgeObject = null;
            ret.FloorLimit = HeightInfo.FloorHit ? HeightInfo.FloorPoint.Z : -9e10f;
            ret.CeilingLimit = HeightInfo.CeilingHit ? HeightInfo.CeilingPoint.Z : 9e10f;
            if (nfc.CeilingHit && nfc.CeilingPoint.Z < ret.CeilingLimit)
            {
                ret.CeilingLimit = nfc.CeilingPoint.Z;
            }
            ret.Point = Climb.Point;

            // check max height
            if (HeightInfo.CeilingHit && tmp.Z > HeightInfo.CeilingPoint.Z - ClimbR - 1.0f)
            {
                tmp.Z = HeightInfo.CeilingPoint.Z - ClimbR - 1.0f;
            }

            // let's calculate edge
            from.X = pos.X - Transform.Basis.Column1.X * ClimbR * 2.0f;
            from.Y = pos.Y - Transform.Basis.Column1.Y * ClimbR * 2.0f;
            from.Z = pos.Z;
            to = tmp;

            t1.SetIdentity();
            t2.SetIdentity();
            upFounded = 0;
            testHeight = Math.Max(testHeight, MaxStepUpHeight);
            d = pos.Z + Bf.BBMax.Z - testHeight;
            to.CopyToArray(castRay, 0);
            to.CopyToArray(castRay, 3);
            castRay[5] -= d;
            var n0 = Vector3.Zero;
            var n1 = Vector3.Zero;
            var n0d = 0.0f;
            var n1d = 0.0f;
            do
            {
                t1.Origin = from;
                t2.Origin = to;
                nfc.Ccb.ClosestHitFraction = 1.0f;
                nfc.Ccb.HitCollisionObject = null;
                Global.BtEngineDynamicsWorld.ConvexSweepTest(ClimbSensor, (Matrix4) t1, (Matrix4) t2, nfc.Ccb);
                if (nfc.Ccb.HasHit)
                {
                    if (nfc.Ccb.HitNormalWorld.Z >= 0.1f)
                    {
                        upFounded = 1;
                        n0 = nfc.Ccb.HitNormalWorld;
                        n0d = -n0.Dot(nfc.Ccb.HitPointWorld);
                    }
                    if (upFounded != 0 && nfc.Ccb.HitNormalWorld.Z < 0.001f)
                    {
                        n1 = nfc.Ccb.HitNormalWorld;
                        n1d = -n1.Dot(nfc.Ccb.HitPointWorld);
                        Climb.EdgeObject = nfc.Ccb.HitCollisionObject;
                        upFounded = 2;
                        break;
                    }
                }
                else
                {
                    tmp.X = to.X;
                    tmp.Y = to.Y;
                    tmp.Z = d;
                    t1.Origin = to;
                    t2.Origin = tmp;
                    t1.Origin = from;
                    t2.Origin = to;
                    nfc.Ccb.ClosestHitFraction = 1.0f;
                    nfc.Ccb.HitCollisionObject = null;
                    Global.BtEngineDynamicsWorld.ConvexSweepTest(ClimbSensor, (Matrix4) t1, (Matrix4) t2, nfc.Ccb);
                    if (nfc.Ccb.HasHit)
                    {
                        upFounded = 1;
                        n0 = nfc.Ccb.HitNormalWorld;
                        n0d = -n0.Dot(nfc.Ccb.HitPointWorld);
                    }
                    else
                    {
                        return ret;
                    }
                }

                // mult 0.66 is magic, but it must be less than 1.0 and greater than 0.0;
                // close to 1.0 - bad precision, good speed;
                // close to 0.0 - bad speed, bad precision;
                // close to 0.5 - middle speed, good precision
                from.Z -= 0.66f * ClimbR;
                to.Z -= 0.66f * ClimbR;
            } while (to.Z >= d); // we can't climb under floor!

            if (upFounded != 2)
            {
                return ret;
            }

            // get the character plane equation
            var n2 = Transform.Basis.Column0;
            var n2d = -n2.Dot(pos);

            Assert.That(!n0.FuzzyZero());
            Assert.That(!n1.FuzzyZero());
            Assert.That(!n2.FuzzyZero());

            /*
             * Solve system of the linear equations by Kramer method!
             * I know - It may be slow, but it has a good precision!
             * The root is point of 3 planes intersection.
             */
            d = -n0[0] * (n1[1] * n2[2] - n1[2] * n2[1]) +
                n1[0] * (n0[1] * n2[2] - n0[2] * n2[1]) -
                n2[0] * (n0[1] * n1[2] - n0[2] * n1[1]);

            if (Math.Abs(d) < 0.005f)
            {
                return ret;
            }

            ret.EdgePoint[0] = n0d * (n1[1] * n2[2] - n1[2] * n2[1]) -
                               n1d * (n0[1] * n2[2] - n0[2] * n2[1]) +
                               n2d * (n0[1] * n1[2] - n0[2] * n1[1]);
            ret.EdgePoint[0] /= d;

            ret.EdgePoint[1] = n0[0] * (n1d * n2[2] - n1[2] * n2d) -
                               n1[0] * (n0d * n2[2] - n0[2] * n2d) +
                               n2[0] * (n0d * n1[2] - n0[2] * n1d);
            ret.EdgePoint[1] /= d;

            ret.EdgePoint[2] = n0[0] * (n1[1] * n2d - n1d * n2[1]) -
                               n1[0] * (n0[1] * n2d - n0d * n2[1]) +
                               n2[0] * (n0[1] * n1d - n0d * n1[1]);
            ret.EdgePoint[2] /= d;
            ret.Point = ret.EdgePoint;
            ret.Point.CopyToArray(castRay, 3);
            /*
             * unclimbable edge slant %)
             */
            n2 = n0.Cross(n1);
            d = CriticalSlantZComponent;
            d *= d * (n2[0] * n2[0] + n2[1] * n2[1] + n2[2] * n2[2]);
            if (n2[2] * n2[2] > d)
            {
                return ret;
            }

            /*
             * Now, let us calculate z_angle
             */
            ret.EdgeHit = true;

            n2.Z = n2.X;
            n2.X = n2.Y;
            n2.Y = -n2.Z;
            n2.Z = 0.0f;
            if (n2.X * Transform.Basis.Column1.X + n2.Y * Transform.Basis.Column1.Y > 0) // direction fixing
            {
                n2.X = -n2.X;
                n2.Y = -n2.Y;
            }

            ret.N = n2;
            ret.Up.X = 0.0f;
            ret.Up.Y = 0.0f;
            ret.Up.Z = 1.0f;
            ret.EdgeZAngle = (float)Math.Atan2(n2.X, -n2.Y) * Constants.DegPerRad;
            ret.EdgeTanXY.X = -n2.Y;
            ret.EdgeTanXY.Y = n2.X;
            ret.EdgeTanXY.Z = 0.0f;
            ret.EdgeTanXY /= (float) Math.Sqrt(n2.X * n2.X + n2.Y * n2.Y);
            ret.Right = ret.EdgeTanXY;

            if(!HeightInfo.FloorHit || ret.EdgePoint.Z - HeightInfo.FloorPoint.Z >= Height)
            {
                ret.CanHang = true;
            }

            ret.NextZSpace = 2.0f * Height;
            if(nfc.FloorHit && nfc.CeilingHit)
            {
                ret.NextZSpace = nfc.CeilingPoint.Z - nfc.FloorPoint.Z;
            }

            return ret;
        }

        public ClimbInfo CheckWallsClimbability()
        {
            var ret = new ClimbInfo();
            ret.CanHang = false;
            ret.WallHit = ClimbType.None;
            ret.EdgeHit = false;
            ret.EdgeObject = null;
            ret.FloorLimit = HeightInfo.FloorHit ? HeightInfo.FloorPoint.Z : -9e10f;
            ret.CeilingLimit = HeightInfo.CeilingHit ? HeightInfo.CeilingPoint.Z : 9e10f;
            ret.Point = Climb.Point;

            if(!HeightInfo.WallsClimb)
            {
                return ret;
            }

            ret.Up = Vector3.UnitZ;

            var pos = Transform.Origin;
            var from = pos + Transform.Basis.Column2 * Bf.BBMax.Z - Transform.Basis.Column1 * ClimbR;
            var to = from;
            var t = ForwardSize + Bf.BBMax.Y;
            to += Transform.Basis.Column1 * t;

            var ccb = ConvexCb;
            ccb.ClosestHitFraction = 1.0f;
            ccb.HitCollisionObject = null;

            var tr1 = new Transform();
            tr1.SetIdentity();
            tr1.Origin = from;

            var tr2 = new Transform();
            tr2.SetIdentity();
            tr2.Origin = to;

            Global.BtEngineDynamicsWorld.ConvexSweepTest(ClimbSensor, (Matrix4) tr1, (Matrix4) tr2, ccb);
            if(!ccb.HasHit)
            {
                return ret;
            }

            ret.Point = ccb.HitPointWorld;
            ret.N = ccb.HitNormalWorld;
            var wn2 = new[] {ret.N.X, ret.N.Y};
            t = (float) Math.Sqrt(wn2[0] * wn2[0] + wn2[1] * wn2[1]);
            wn2[0] /= t;
            wn2[1] /= t;

            ret.Right.X = -wn2[1];
            ret.Right.Y = wn2[0];
            ret.Right.Z = 0.0f;

            // now we have wall normale in XOY plane. Let us check all flags
            if(HeightInfo.WallsClimbDir.HasFlagSig(SectorFlag.ClimbNorth) && wn2[1] < -0.7f
                || HeightInfo.WallsClimbDir.HasFlagSig(SectorFlag.ClimbEast) && wn2[0] < -0.7f
                || HeightInfo.WallsClimbDir.HasFlagSig(SectorFlag.ClimbSouth) && wn2[1] > 0.7f
                || HeightInfo.WallsClimbDir.HasFlagSig(SectorFlag.ClimbWest) && wn2[0] > 0.7f)
            {
                ret.WallHit = ClimbType.HandsOnly;
            }

            if(ret.WallHit != ClimbType.None)
            {
                t = 0.67f * Height;
                from -= Transform.Basis.Column2 * t;
                to = from;
                t = ForwardSize + Bf.BBMax.Y;
                to += Transform.Basis.Column1 * t;

                ccb.ClosestHitFraction = 1.0f;
                ccb.HitCollisionObject = null;
                tr1.SetIdentity();
                tr1.Origin = from;
                tr2.SetIdentity();
                tr2.Origin = to;
                Global.BtEngineDynamicsWorld.ConvexSweepTest(ClimbSensor, (Matrix4)tr1, (Matrix4)tr2, ccb);
                if (ccb.HasHit)
                {
                    ret.WallHit = ClimbType.FullBody;
                }
            }

            return ret;
        }

        /// <summary>
        /// Calculates next height info and information about next step
        /// </summary>
        public void UpdateCurrentHeight()
        {
            GetHeightInfo(Transform * new Vector3(0.0f, 0.0f, Bf.BoneTags[0].Transform.Origin.Z), HeightInfo, Height);
        }

        public override void UpdatePlatformPreStep()
        {
#if false
            if (character->platform)
            {
                EngineContainer* cont = (EngineContainer*)character->platform->getUserPointer();
                if (cont && (cont->object_type == OBJECT_ENTITY/* || cont->object_type == OBJECT_BULLET_MISC*/))
                {
                    btScalar trpl[16];
                    character->platform->getWorldTransform().getOpenGLMatrix(trpl);
#if false
                    new_tr = trpl * character->local_platform;
                    vec3_copy(transform.getOrigin(), new_tr + 12);
#else
                    ///make something with platform rotation
                    transform = trpl * character->local_platform;
#endif
                }
            }
#endif
        }

        /// <summary>
        /// Get local character transform relative platfom
        /// </summary>
        public void UpdatePlatformPostStep()
        {
#if false
            switch(move_type)
            {
                case MoveType::OnFloor:
                    if(character->height_info.floor_hit)
                    {
                        character->platform = character->height_info.floor_obj;
                    }
                    break;
                case MoveType::Climbing:
                    if(character->climb.edge_hit)
                    {
                        character->platform = character->climb.edge_obj;
                    }
                    break;
                default:
                    character->platform = nullptr;
                    break;
            };
            if(character->platform)
            {
                EngineContainer* cont = (EngineContainer*)character->platform->getUserPointer();
                if(cont && (cont->object_type == OBJECT_ENTITY/* || cont->object_type == OBJECT_BULLET_MISC*/))
                {
                    btScalar trpl[16];
                    character->platform->getWorldTransform().getOpenGLMatrix(trpl);
                    /* local_platform = (global_platform ^ -1) x (global_entity); */
                    character->local_platform = trpl.inverse() * transform;
                }
                else
                {
                    character->platform = nullptr;
                }
            }
#endif
        }

        public void Lean(CharacterCommand cmd, float maxLean)
        {
            var negLean = 360.0f - maxLean;
            var leanCoeff = maxLean == 0.0f ? 48.0f : maxLean * 3;

            // Continously lean character, according to current left/right direction.

            if (cmd.Move[1] == 0 || maxLean == 0.0f) // No direction - restore straight vertical position!
            {
                if(Angles.Z != 0.0f)
                {
                    if (Angles.Z < 180.0f)
                    {
                        Angles.Z = Math.Max(0.0f,
                            Angles.Z - (Math.Abs(Angles.Z) + leanCoeff) / 2 * Global.EngineFrameTime);
                    }
                    else
                    {
                        Angles.Z += (360 - Math.Abs(Angles.Z) + leanCoeff) / 2 * Global.EngineFrameTime;
                        if (Angles.Z < 180.0f) Angles.Z = 0.0f;
                    }
                }
            }
            else if(cmd.Move[1] == 1) // Right direction
            {
                if (Angles.Z != maxLean)
                {
                    if (Angles.Z < maxLean) // Approaching from center
                    {
                        Angles.Z = Math.Min(maxLean,
                            Angles.Z + (Math.Abs(Angles.Z) + leanCoeff) / 2 * Global.EngineFrameTime);
                    }
                    else if(Angles.Z > 180.0f) // Approaching from left
                    {
                        Angles.Z += (360 - Math.Abs(Angles.Z) + leanCoeff * 2) / 2 * Global.EngineFrameTime;
                        if (Angles.Z < 180.0f) Angles.Z = 0.0f;
                    }
                    else // Reduce previous lean
                    {
                        Angles.Z = Math.Max(0.0f,
                             Angles.Z - (Math.Abs(Angles.Z) + leanCoeff) / 2 * Global.EngineFrameTime);
                    }
                }
            }
            else if (cmd.Move[1] == -1) // Left direction
            {
                if (Angles.Z != negLean)
                {
                    if (Angles.Z > negLean) // Reduce previous lean
                    {
                        Angles.Z = Math.Max(negLean,
                           Angles.Z - (360.0f - Math.Abs(Angles.Z) + leanCoeff) / 2 * Global.EngineFrameTime);
                    }
                    else if (Angles.Z > 180.0f) // Approaching from right
                    {
                        Angles.Z -= (Math.Abs(Angles.Z) + leanCoeff * 2) / 2 * Global.EngineFrameTime;
                        if (Angles.Z < 0.0f) Angles.Z += 360.0f;
                    }
                    else // Approaching from center
                    {
                        Angles.Z += (360.0f - Math.Abs(Angles.Z) + leanCoeff) / 2 * Global.EngineFrameTime;
                        if (Angles.Z > 360.0f) Angles.Z -= 360.0f;
                    }
                }
            }
        }

        /// <summary>
        /// Linear inertia is absolutely needed for in-water states, and also it gives
        /// more organic feel to land animations.
        /// </summary>
        public float GetInertiaLinear(float maxSpeed, float accel, bool command)
        {
            if (accel == 0.0f || accel >= maxSpeed)
            {
                InertiaLinear = command ? maxSpeed : 0.0f;
            }
            else
            {
                if (command)
                {
                    if (InertiaLinear < maxSpeed)
                    {
                        InertiaLinear = Math.Min(maxSpeed, InertiaLinear + maxSpeed * accel * Global.EngineFrameTime);
                    }
                }
                else
                {
                    if (InertiaLinear > 0.0f)
                    {
                        InertiaLinear = Math.Max(0.0f, InertiaLinear - maxSpeed * accel * Global.EngineFrameTime);
                    }
                }

            }

            return InertiaLinear * SpeedMult;
        }

        /// <summary>
        /// Angular inertia is used on keyboard-driven (non-analog) rotational controls.
        /// </summary>
        public float GetInertiaAngular(float maxAngle, float accel, byte axis)
        {
            if (axis > 1) return 0.0f;

            var currRotDir = 0;
            if(Command.Rotation[axis] < 0.0f)
            {
                currRotDir = 1;
            }
            else if(Command.Rotation[axis] > 0.0f)
            {
                currRotDir = 2;
            }

            if(currRotDir == 0 || maxAngle == 0.0f || accel == 0.0f)
            {
                InertiaAngular[axis] = 0.0f;
            }
            else
            {
                if(InertiaAngular[axis] != maxAngle)
                {
                    if(currRotDir == 2)
                    {
                        if (InertiaAngular[axis] < 0.0f)
                        {
                            InertiaAngular[axis] = 0.0f;
                        }
                        else
                        {
                            InertiaAngular[axis] = Math.Min(maxAngle, InertiaAngular[axis] + maxAngle * accel * Global.EngineFrameTime);
                        }
                    }
                    else
                    {
                        if (InertiaAngular[axis] > 0.0f)
                        {
                            InertiaAngular[axis] = 0.0f;
                        }
                        else
                        {
                            InertiaAngular[axis] = Math.Max(-maxAngle, InertiaAngular[axis] - maxAngle * accel * Global.EngineFrameTime);
                        }
                    }
                }
            }

            return Math.Abs(InertiaAngular[axis]) * Command.Rotation[axis];
        }

        public int MoveOnFloor();

        public int FreeFalling();

        public int MonkeyClimbing();

        public int WallsClimbing();

        public int Climbing();

        public int MoveUnderwater();

        public int MoveOnWater();

        public int FindTraverse();

        public int CheckTraverse(Entity obj);

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



