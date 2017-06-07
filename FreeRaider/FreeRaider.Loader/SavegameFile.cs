using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace FreeRaider.Loader
{
    public class TR1SavegameFile
    {
        public string SaveName;
        public uint SaveNumber;
        public LevelInit[] LevelInitData;
        public uint ElapsedTime;
        public TimeSpan ElapsedTimeTS { get { return new TimeSpan((long) (ElapsedTime / 30.0 * TimeSpan.TicksPerSecond));}set { ElapsedTime = (uint) (value.Ticks * 30.0 / TimeSpan.TicksPerSecond); } }
        public uint Kills;
        public ushort SecretsFound;
        public ushort LevelNumber;
        public byte NumPickups;
        public bool UnlimitedAmmo;
        public byte HasItem141;
        public byte HasItem142;
        public byte[] Puzzle;
        public byte[] Keys;
        public byte Pickup;
        public byte LevelInitDataCRC;
        public byte RoomsAreSwapped;
        public byte[] FlipFlags;
        public ushort[] CameraFlagsZoneIndexes;
        public ITEM[] Entities;
        public MOVEABLE[] Moveables;
        public LaraStruct Lara;
        public int PostFxFunc;
        public int AnimFxTime;
        private static ITEM[][] _itemDb;
        private static MOVEABLE[][] _moveableDb;

        #region Structures

        public enum HandStatusType : byte
        {
            None = 0,
            Climbing = 1,
            SingleShot = 3,
            Combat = 4
        }

        public enum Weapons8 : byte
        {
            None = 0,
            Pistols = 1,
            Magnums = 2,
            Uzis = 3,
            Shotgun = 4
        }

        public enum Weapons16 : short
        {
            None = 0,
            Pistols = 1,
            Magnums = 2,
            Uzis = 3,
            Shotgun = 4
        }

        [Flags]
        public enum WeaponsFlag : byte
        {
            Unlocked = 1 << 0,
            Pistols = 1 << 1,
            Magnums = 1 << 2,
            Uzis = 1 << 3,
            Shotgun = 1 << 4,
            MidasHand = 1 << 5
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct LevelInit
        {
            public ushort Header;
            public ushort Ammo_Magnums;
            public ushort Ammo_Uzis;
            public ushort Ammo_Shotgun;
            public byte SmallMedipacks;
            public byte LargeMedipacks;
            public byte NumScionPieces;
            public HandStatusType HandStatus;
            public Weapons8 Weapon;
            public WeaponsFlag Weapons;
            public byte Unknown2;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct LaraStruct
        {
            public short ItemID;
            public short HandStatus;
            public Weapons16 CurrentWeapon;
            public short NextWeapon;
            public short ClimbFallSpeedOverride;
            public short UnderwaterState;
            public short Unknown1;
            public short CollisionFrame;
            public short CollisionAxis;
            public short Air;
            public short SwimToDiveKeypressDuration;
            public short DeadTime;
            public short UnderwaterCurrentStrength;
            public short Unknown2;
            public IntPtr WallPosition;
            public MeshTree LaraMeshTree;
            public IntPtr Enemy;
            public VERTEX2YX EnemyLookRot;
            public short YRotationSpeed;
            public short MovementAngle;
            public VERTEX2YXZ HeadRotation;
            public VERTEX2YXZ TorsoRotation;
            public AimInfo AimInfoLeft;
            public AimInfo AimInfoRight;
            public AMMO PistolAmmo;
            public AMMO MagnumAmmo;
            public AMMO UziAmmo;
            public AMMO ShotgunAmmo;
            public RoutePlanner AiInfo;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public unsafe struct MeshTree
        {
            public int ReplacedMeshesBits;
            public fixed int Meshes [15];
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct VERTEX2
        {
            public short X;
            public short Y;
            public short Z;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct VERTEX2YX
        {
            public short Y;
            public short X;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct VERTEX2YXZ
        {
            public short Y;
            public short X;
            public short Z;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct VERTEX4
        {
            public int X;
            public int Y;
            public int Z;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct AMMO
        {
            public int Ammo;
            public int Hits;
            public int Misses;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct AimInfo
        {
            public IntPtr WeaponAnimData;
            public short Frame;
            public short Aiming;
            public VERTEX2YXZ AimRotation;
            public short ShootTimeout;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct RoutePlanner
        {
            public IntPtr Node;
            public ushort Head;
            public ushort Tail;
            public ushort SearchNumber;
            public short BlockMask;
            public short StepHeight;
            public short DropHeight;
            public short FlyHeight;
            public short ZoneCount;
            public short DestinationBox;
            public ushort SearchOverride;
            public VERTEX4 SearchTarget;
        }

        [Flags]
        public enum ITEMFLAGS2 : byte
        {
            ITEMFLAGS2_ACTIVE = 0x1,
            ITEMFLAGS2_STATE_DISABLED = 0x0,
            ITEMFLAGS2_STATE_ENABLED = 0x2,
            ITEMFLAGS2_STATE_ACTIVE = 0x4,
            ITEMFLAGS2_STATE_LOCKED = 0x6,
            ITEMFLAGS2_GRAVITY_STATUS = 0x8,
            ITEMFLAGS2_HIT_STATUS = 0x10,
            ITEMFLAGS2_COLLIDABLE = 0x20,
            ITEMFLAGS2_LOOKED_AT = 0x40,
            ITEMFLAGS2_DYNAMIC_LIGHT = 0x80,
        }

        public enum ITEM_ID : short
        {
            ITEM_ID_LARA = 0,
            ITEM_ID_LARA_PISTOL = 1,
            ITEM_ID_LARA_SHOTGUN = 2,
            ITEM_ID_LARA_AUTOPISTOL = 3,
            ITEM_ID_LARA_UZI = 4,
            ITEM_ID_LARA_MIDAS_HOME_WOUNDED = 5,
            ITEM_ID_LARATWIN = 6,
            ITEM_ID_WOLF = 7,
            ITEM_ID_BEAR = 8,
            ITEM_ID_BAT = 9,
            ITEM_ID_CROC_IN_WATER = 11,
            ITEM_ID_GORILLA = 15,
            ITEM_ID_TYRANNOSAUR = 18,
            ITEM_ID_RAPTOR = 19,
            ITEM_ID_WINGED_MUTANT = 20,
            ITEM_ID_LARAHIPS1 = 21,
            ITEM_ID_LARAHIPS2 = 22,
            ITEM_ID_CENTAUR_MUTANT = 23,
            ITEM_ID_MUMMY = 24,
            ITEM_ID_26 = 26,
            ITEM_ID_LARSON = 27,
            ITEM_ID_PIERRE = 28,
            ITEM_ID_COLLAPSIBLE_FLOOR = 35,
            ITEM_ID_SWING_BLADE = 36,
            ITEM_ID_TRAPDOOR_UP = 41,
            ITEM_ID_SLAMMING_DOOR = 42,
            ITEM_ID_CUBICAL_BLOCK_1 = 48,
            ITEM_ID_CUBICAL_BLOCK_2 = 49,
            ITEM_ID_CUBICAL_BLOCK_3 = 50,
            ITEM_ID_CUBICAL_BLOCK_4 = 51,
            ITEM_ID_TALL_BLOCK = 52,
            ITEM_ID_SWITCH = 55,
            ITEM_ID_UNDERWATER_SWITCH = 56,
            ITEM_ID_TRAPDOOR_DOWN1 = 65,
            ITEM_ID_TRAPDOOR_DOWN2 = 66,
            ITEM_ID_FLAT_BRIDGE = 68,
            ITEM_ID_BRIDGE_SLOPE1 = 69,
            ITEM_ID_BRIDGE_SLOPE2 = 70,
            ITEM_ID_PASSPORT = 71,
            ITEM_ID_COMPASS = 72,
            ITEM_ID_LARA_HOME = 73,
            ITEM_ID_PISTOLS_SPRITE = 84,
            ITEM_ID_SHOTGUN_SPRITE = 85,
            ITEM_ID_MAGNUMS_SPRITE = 86,
            ITEM_ID_UZIS_SPRITE = 87,
            ITEM_ID_SHOTGUN_AMMO_SPRITE = 89,
            ITEM_ID_MAGNUM_AMMO_SPRITE = 90,
            ITEM_ID_UZI_AMMO_SPRITE = 91,
            ITEM_ID_92_SPRITE = 92,
            ITEM_ID_MEDIPACK_SMALL_SPRITE = 93,
            ITEM_ID_MEDIPACK_LARGE_SPRITE = 94,
            ITEM_ID_SUNGLASSES = 95,
            ITEM_ID_DIR_KEYS = 97,
            ITEM_ID_PISTOL_3D = 99,
            ITEM_ID_SHOTGUN_3D = 100,
            ITEM_ID_MAGNUM_3D = 101,
            ITEM_ID_UZIS_3D = 102,
            ITEM_ID_MENU_UZI = 102,
            ITEM_ID_SHOTGUN_AMMO_3D = 104,
            ITEM_ID_MAGNUM_AMMO_3D = 105,
            ITEM_ID_UZI_AMMO_3D = 106,
            ITEM_ID_92_3D = 107,
            ITEM_ID_MEDIPACK_SMALL_3D = 108,
            ITEM_ID_MEDIPACK_LARGE_3D = 109,
            ITEM_ID_PUZZLE1_SPRITE = 110,
            ITEM_ID_PUZZLE2_SPRITE = 111,
            ITEM_ID_PUZZLE3_SPRITE = 112,
            ITEM_ID_PUZZLE4_SPRITE = 113,
            ITEM_ID_PUZZLE1_3D = 114,
            ITEM_ID_PUZZLE2_3D = 115,
            ITEM_ID_PUZZLE3_3D = 116,
            ITEM_ID_PUZZLE4_3D = 117,
            ITEM_ID_PICKUP_SPRITE = 126,
            ITEM_ID_PICKUP_3D = 127,
            ITEM_ID_KEY1_SPRITE = 129,
            ITEM_ID_KEY2_SPRITE = 130,
            ITEM_ID_KEY3_SPRITE = 131,
            ITEM_ID_KEY4_SPRITE = 132,
            ITEM_ID_KEY1_3D = 133,
            ITEM_ID_KEY2_3D = 134,
            ITEM_ID_KEY3_3D = 135,
            ITEM_ID_KEY4_3D = 136,
            ITEM_ID_141_SPRITE = 141,
            ITEM_ID_142_SPRITE = 142,
            ITEM_ID_SCION_PIECE_SPRITE1 = 143,
            ITEM_ID_SCION_PIECE_SPRITE2 = 144,
            ITEM_ID_141_3D = 148,
            ITEM_ID_142_3D = 149,
            ITEM_ID_SCION_PIECE_3D = 150,
            ITEM_ID_WATER_SPLASH = 153,
            ITEM_ID_BUBBLES = 155,
            ITEM_ID_BLOOD_SPLATTER = 158,
            ITEM_ID_GUNFLARE = 166,
            ITEM_ID_MUTANT_BULLET = 172,
            ITEM_ID_CHOPPY_WATER_EMITTER = 177,
            ITEM_ID_FIRE = 178,
            ITEM_ID_FONT = 190,
        }

        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public unsafe struct ITEM
        {
            public int Floor;
            public int TouchBits;
            public int MeshBits;
            public ITEM_ID ObjectNumber;
            public short CurrentAnimState;
            public short GoalAnimState;
            public short RequiredAnimState;
            public short AnimNumber;
            public short FrameNumber;
            public short RoomNumber;
            public short NextItem;
            public short NextActive;
            public short Speed;
            public short Fallspeed;
            public short Health;
            public short BoxNumber;
            public short Timer;
            public short TriggerFlags;
            public short Shade;
            public void* Data;
            public CinematicInfo Transform;
            public short Flags;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct CinematicInfo
        {
            public VERTEX4 Position;
            public VERTEX2 Angle;
        }

        [Flags]
        public enum MOVEABLE_FLAG
        {
            MOVEABLE_FLAG_LOADED = 0x1,
            MOVEABLE_FLAG_INTELLIGENT = 0x2,
            MOVEABLE_FLAG_NON_LOT = 0x4,
            MOVEABLE_FLAG_SAVE_POSITION = 0x8,
            MOVEABLE_FLAG_SAVE_HITPOINTS = 0x10,
            MOVEABLE_FLAG_SAVE_FLAGS = 0x20,
            MOVEABLE_FLAG_SAVE_ANIM = 0x40,
            MOVEABLE_FLAG_SEMI_TRANSPARENT = 0x80,
            MOVEABLE_FLAG_WATER_CREATURE = 0x100,
            MOVEABLE_FLAG_ACTIVATION_MASK = 0x3E00,
        }

        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public unsafe struct MOVEABLE
        {
            public short NMeshes;
            public short FrameNumber;
            public int BoneIndex;
            public ANIM_FRAME* FrameBase;
            public void* Initialise;
            public void* Control;
            public void* Floor;
            public void* Ceiling;
            public void* DrawRoutine;
            public void* Collision;
            public short AnimIndex;
            public short HitPoints;
            public short PivotLength;
            public short Radius;
            public short AttackTargetUpdateProbability;
            public short ShadowScale;
            public byte Flags;
            public byte Field31;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ANIM_FRAME
        {
            public BBOX BBox;
            public VERTEX2 Pos;
            public short NumValues;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct BBOX
        {
            public short MinX;
            public short MaxX;
            public short MinY;
            public short MaxY;
            public short MinZ;
            public short MaxZ;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct Brain
        {
            public VERTEX2YXZ JointRotation;
            public short NumResurrectionsFromDeath;
            public short ItemNum;
            public byte Mood;
            public RoutePlanner Route;
            public VERTEX4 MoveTarget;
        }

        #endregion

        public static ITEM[][] ItemsDB { get { if(_itemDb == null) LoadDB(); return _itemDb; } }

        public static MOVEABLE[][] MoveableDB { get { if (_moveableDb == null) LoadDB(); return _moveableDb; } }

        private static unsafe void LoadDB()
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("FreeRaider.Loader.entity_tr1.db"))
            using (var br = new BinaryReader(stream))
            {
                var numLevels = br.ReadInt32();
                _itemDb = new ITEM[numLevels][];
                _moveableDb = new MOVEABLE[numLevels][];

                for (var i = 0; i < numLevels; i++)
                {
                    // Item DB
                    {
                        var len = br.ReadInt32();
                        _itemDb[i] = new ITEM[len];
                        for (var j = 0; j < len; j++)
                        {
                            _itemDb[i][j].ObjectNumber = (ITEM_ID) br.ReadInt16();
                            _itemDb[i][j].RoomNumber = br.ReadInt16();
                            _itemDb[i][j].Transform.Position.X = br.ReadInt32();
                            _itemDb[i][j].Transform.Position.Y = br.ReadInt32();
                            _itemDb[i][j].Transform.Position.Z = br.ReadInt32();
                            _itemDb[i][j].Transform.Angle.Y = br.ReadInt16();
                            _itemDb[i][j].Shade = br.ReadInt16();
                            _itemDb[i][j].TriggerFlags = br.ReadInt16();
                        }
                    }

                    // Moveable DB
                    {
                        var len = br.ReadInt32();
                        _moveableDb[i] = new MOVEABLE[191];
                        for (var j = 0; j < len; j++)
                        {
                            br.ReadUInt32();
                            _moveableDb[i][j].NMeshes = br.ReadInt16();
                            _moveableDb[i][j].FrameNumber = br.ReadInt16();
                            _moveableDb[i][j].BoneIndex = br.ReadInt32();
                            br.ReadUInt32();
                            _moveableDb[i][j].FrameBase = (ANIM_FRAME*) 0;
                            _moveableDb[i][j].AnimIndex = br.ReadInt16();

                            _moveableDb[i][j].Flags |= (byte) MOVEABLE_FLAG.MOVEABLE_FLAG_LOADED;
                        }

                        #region initAll
                        for (var j = 0; j < 191; j++)
                        {
                            _moveableDb[i][j].Radius = 10;
                            _moveableDb[i][j].HitPoints = -16384;
                            _moveableDb[i][j].Initialise = (void*)0;
                            _moveableDb[i][j].Collision = (void*)0;
                            _moveableDb[i][j].Control = (void*) 0;
                            _moveableDb[i][j].Ceiling = (void*) 0;
                            _moveableDb[i][j].Floor = (void*) 0;
                            _moveableDb[i][j].PivotLength = 0;
                            _moveableDb[i][j].ShadowScale = 0;
                            unchecked
                            {
                                _moveableDb[i][j].Flags &= (byte)~(byte)(MOVEABLE_FLAG.MOVEABLE_FLAG_SAVE_FLAGS | MOVEABLE_FLAG.MOVEABLE_FLAG_SAVE_HITPOINTS | MOVEABLE_FLAG.MOVEABLE_FLAG_SAVE_POSITION | MOVEABLE_FLAG.MOVEABLE_FLAG_NON_LOT | MOVEABLE_FLAG.MOVEABLE_FLAG_INTELLIGENT);
                            }
                        }
                        #endregion

                        #region initBaddies

                        _moveableDb[i][(int) ITEM_ID.ITEM_ID_LARA].Flags |= (byte) (ITEMFLAGS2.ITEMFLAGS2_COLLIDABLE | ITEMFLAGS2.ITEMFLAGS2_HIT_STATUS | ITEMFLAGS2.ITEMFLAGS2_GRAVITY_STATUS | ITEMFLAGS2.ITEMFLAGS2_STATE_ACTIVE);
                        _moveableDb[i][(int) ITEM_ID.ITEM_ID_LARATWIN].Flags |= (byte) (ITEMFLAGS2.ITEMFLAGS2_GRAVITY_STATUS | ITEMFLAGS2.ITEMFLAGS2_STATE_ACTIVE);
                        if ((_moveableDb[i][(int) ITEM_ID.ITEM_ID_WOLF].Flags & 1) != 0)
                        {
                            _moveableDb[i][(int)ITEM_ID.ITEM_ID_WOLF].Flags |= (byte)(ITEMFLAGS2.ITEMFLAGS2_COLLIDABLE | ITEMFLAGS2.ITEMFLAGS2_HIT_STATUS | ITEMFLAGS2.ITEMFLAGS2_GRAVITY_STATUS | ITEMFLAGS2.ITEMFLAGS2_STATE_LOCKED);
                        }
                        var tmpItems = new int[] 
                        {8, (int) ITEM_ID.ITEM_ID_BAT, 18, 19, 27, 28, 16, 17, 12, 13, 14, 10, 11, 15, 20, 23, 30, 31, 32, 34, 33};
                        foreach (var n in tmpItems)
                        {
                            if ((_moveableDb[i][n].Flags & 1) != 0)
                            {
                                _moveableDb[i][n].Flags |= 0x3E;
                            }
                        }
                        /*if ((_moveableDb[i][16].Flags & 1) != 0)
                        {
                            _moveableDb[i][16].Flags |= 0x3E;
                        }*/
                        if ((_moveableDb[i][(int)ITEM_ID.ITEM_ID_MUMMY].Flags & 1) != 0)
                        {
                            _moveableDb[i][(int)ITEM_ID.ITEM_ID_MUMMY].Flags |= (byte)(ITEMFLAGS2.ITEMFLAGS2_COLLIDABLE | ITEMFLAGS2.ITEMFLAGS2_HIT_STATUS | ITEMFLAGS2.ITEMFLAGS2_GRAVITY_STATUS);
                        }
                        if ((_moveableDb[i][163].Flags & 1) != 0)
                        {
                            _moveableDb[i][163].Flags |= (byte)(ITEMFLAGS2.ITEMFLAGS2_COLLIDABLE | ITEMFLAGS2.ITEMFLAGS2_HIT_STATUS);
                        }
                        if ((_moveableDb[i][181].Flags & 1) != 0)
                        {
                            _moveableDb[i][181].Flags |= (byte)(ITEMFLAGS2.ITEMFLAGS2_COLLIDABLE | ITEMFLAGS2.ITEMFLAGS2_HIT_STATUS);
                        }
                        if ((_moveableDb[i][161].Flags & 1) != 0)
                        {
                            _moveableDb[i][161].Flags |= (byte)(ITEMFLAGS2.ITEMFLAGS2_COLLIDABLE | ITEMFLAGS2.ITEMFLAGS2_HIT_STATUS);
                        }
                        // todo
                        #endregion

                        #region initInteractiveItems
                        // todo
                        #endregion

                        #region initAll3
                        _moveableDb[i][74].Flags |= 0x10;
                        _moveableDb[i][76].Flags |= 0x10;
                        _moveableDb[i][75].Flags |= 0x10;
                        _moveableDb[i][142].Flags |= 0x10;
                        _moveableDb[i][130].Flags |= 0x10;
                        _moveableDb[i][141].Flags |= 0x10;
                        _moveableDb[i][129].Flags |= 0x10;
                        _moveableDb[i][131].Flags |= 0x10;
                        _moveableDb[i][132].Flags |= 0x10;
                        _moveableDb[i][110].Flags |= 0x10;
                        _moveableDb[i][111].Flags |= 0x10;
                        _moveableDb[i][113].Flags |= 0x10;
                        _moveableDb[i][85].Flags |= 0x10;
                        _moveableDb[i][112].Flags |= 0x10;
                        _moveableDb[i][84].Flags |= 0x10;
                        _moveableDb[i][86].Flags |= 0x10;
                        _moveableDb[i][87].Flags |= 0x10;
                        _moveableDb[i][89].Flags |= 0x10;
                        _moveableDb[i][91].Flags |= 0x10;
                        _moveableDb[i][88].Flags |= 0x10;
                        _moveableDb[i][90].Flags |= 0x10;
                        _moveableDb[i][92].Flags |= 0x10;
                        _moveableDb[i][94].Flags |= 0x10;
                        _moveableDb[i][144].Flags |= 0x10;
                        _moveableDb[i][137].Flags |= 0x10;
                        _moveableDb[i][139].Flags |= 0x10;
                        _moveableDb[i][126].Flags |= 0x10;
                        _moveableDb[i][118].Flags |= 0x10;
                        _moveableDb[i][138].Flags |= 0x10;
                        _moveableDb[i][140].Flags |= 0x10;
                        _moveableDb[i][119].Flags |= 0x10;
                        _moveableDb[i][120].Flags |= 0x10;
                        _moveableDb[i][124].Flags |= 0x10;
                        _moveableDb[i][122].Flags |= 0x10;
                        _moveableDb[i][121].Flags |= 0x10;
                        _moveableDb[i][125].Flags |= 0x10;
                        _moveableDb[i][123].Flags |= 0x10;
                        _moveableDb[i][147].Flags |= 0x30;
                        _moveableDb[i][146].Flags |= 0x10;
                        _moveableDb[i][145].Flags |= 0x10;
                        _moveableDb[i][168].Flags |= 0x1;
                        _moveableDb[i][148].NMeshes = 0;
                        #endregion
                    }


                }
            }
        }

        public static TR1SavegameFile Read(string fname)
        {
            using (var fs = File.OpenRead(fname))
            {
                return Read(fs);
            }
        }

        public static TR1SavegameFile Read(Stream s)
        {
            using (var br = new BinaryReader(s))
            {
                return Read(br);
            }
        }

        public static unsafe TR1SavegameFile Read(BinaryReader br)
        {
            var ret = new TR1SavegameFile();

            ret.SaveName = br.ReadStringUntil(enc: Helper.CP437, length: 75).ConvertFromTR2Accent();
            ret.SaveNumber = unchecked((ushort)br.ReadUInt32()); // stored in 4 bytes, game clips it to 2
            br.ReadBytes(15);
            ret.LevelInitData = br.ReadStructArray<LevelInit>(21);
            ret.ElapsedTime = br.ReadUInt32();
            ret.Kills = br.ReadUInt32();
            ret.SecretsFound = br.ReadUInt16();
            ret.LevelNumber = br.ReadUInt16();
            ret.NumPickups = br.ReadByte();
            ret.UnlimitedAmmo = br.ReadBoolean();
            ret.HasItem141 = br.ReadByte();
            ret.HasItem142 = br.ReadByte();
            ret.Puzzle = br.ReadBytes(4);
            ret.Keys = br.ReadBytes(4);
            ret.Pickup = br.ReadByte();
            ret.LevelInitDataCRC = br.ReadByte();
            ret.RoomsAreSwapped = br.ReadByte();
            ret.FlipFlags = br.ReadBytes(10);
            ret.CameraFlagsZoneIndexes = br.ReadUInt16Array(
                new[] {1, 2, 6, 6, 10, 14, 10, 7, 7, 15, 3, 8, 12, 15, 6, 2, 1, 0, 0, 0, 0}
                [ret.LevelNumber]);


            br.ReadBytes(3);

            ret.Entities = ItemsDB[ret.LevelNumber].ToArray();
            ret.Moveables = MoveableDB[ret.LevelNumber].ToArray();

            for (var i = 0; i < ret.Entities.Length; i++)
            {
                var item = ret.Entities[i];
                var mov = ret.Moveables[(int)item.ObjectNumber];

                if (mov.Flags.HasFlagUns(MOVEABLE_FLAG.MOVEABLE_FLAG_NON_LOT))
                {
                    item.Transform = br.ReadStruct<CinematicInfo>();
                    item.RoomNumber = br.ReadInt16();
                    item.Speed = br.ReadInt16();
                    item.Fallspeed = br.ReadInt16();
                }
                if (mov.Flags.HasFlagUns(MOVEABLE_FLAG.MOVEABLE_FLAG_SAVE_FLAGS))
                {
                    item.CurrentAnimState = br.ReadInt16();
                    item.GoalAnimState = br.ReadInt16();
                    item.RequiredAnimState = br.ReadInt16();
                    item.AnimNumber = br.ReadInt16();
                    item.FrameNumber = br.ReadInt16();
                }
                if (mov.Flags.HasFlagUns(MOVEABLE_FLAG.MOVEABLE_FLAG_SAVE_POSITION))
                {
                    item.Health = br.ReadInt16();
                }
                if (mov.Flags.HasFlagUns(MOVEABLE_FLAG.MOVEABLE_FLAG_SAVE_HITPOINTS))
                {
                    item.TriggerFlags = br.ReadInt16();
                    item.Timer = br.ReadInt16();
                    if (mov.Flags.HasFlagUns(MOVEABLE_FLAG.MOVEABLE_FLAG_INTELLIGENT))
                    {
                        // todo
                        item.Data = (void*) br.ReadUInt16();
                        //var b = (Brain*) item.Data;
                        var tmp = new Brain();
                        var b = &tmp;
                        b->JointRotation.X = br.ReadInt16();
                        b->JointRotation.Z = br.ReadInt16();
                        b->NumResurrectionsFromDeath = br.ReadInt16();
                        b->Mood = br.ReadByte();
                    }
                }

                ret.Entities[i] = item;
            }



            /*fixed (ITEM* items = ret.Entities)
            fixed (MOVEABLE* moveables = ret.Moveables)
            {
                var v79 = &items->Transform.Position.X;
                var v80 = &items->Speed;
                var v81 = &items->Fallspeed;
                var v82 = &items->CurrentAnimState;
                var v83 = &items->GoalAnimState;
                var v84 = &items->RequiredAnimState;
                var v85 = &items->AnimNumber;
                var v86 = &items->FrameNumber;
                var v87 = &items->Health;
                var v88 = &items->TriggerFlags;
                var v89 = &items->Timer;

                for (var i = 0; i < ret.Entities.Length; i++)
                {
                    var v7 = ret.Entities[i];
                    var v8 = ret.Moveables[(short) v7.ObjectNumber];
                    if ((v8.Flags & 4) != 0)
                    {

                    }
                }
            }*/

            var cur = br.BaseStream.Position; // should be 1437
            /*br.BaseStream.Seek(-8, SeekOrigin.End);
            while (br.BaseStream.Position > cur)
            {
                var tmp = br.ReadUInt32();
                if (tmp == 0xFFFFFFFF)
                {
                    br.BaseStream.Position -= 240;
                    break;
                }
                br.BaseStream.Position -= 5;
            }*/

            //br.BaseStream.Position += 236;

            ret.Lara = br.ReadStruct<LaraStruct>();
            ret.PostFxFunc = br.ReadInt32();
            ret.AnimFxTime = br.ReadInt32();

            return ret;
        }
    }

    public class TR2SavegameFile
    {
        public string SaveName;
        public int SaveNumber;
        public ushort Ammo_AutoPistols;
        public ushort Ammo_Uzis;
        public ushort Ammo_Shotgun;
        public ushort Ammo_M16;
        public ushort Ammo_Grenade;
        public ushort Ammo_Harpoon;
        public byte SmallMedipacks;
        public byte LargeMedipacks;
        public byte Flares;
        public TR2Weapons Weapons = TR2Weapons._;
        public uint AmmoUsed;
        public uint Hits;
        public float Health;
        public float Air;

        public static TR2SavegameFile Read(string fname, Engine ver)
        {
            using (var fs = File.OpenRead(fname))
            {
                return Read(fs, ver);
            }
        }

        public static TR2SavegameFile Read(Stream s, Engine ver)
        {
            using (var br = new BinaryReader(s))
            {
                return Read(br, ver);
            }
        }

        public static TR2SavegameFile Read(BinaryReader br, Engine ver)
        {
            var ret = new TR2SavegameFile();

            ret.SaveName = br.ReadStringUntil(enc: Helper.CP437, length: 33).ConvertFromTR2Accent();
            br.ReadBytes(42);
            ret.SaveNumber = br.ReadInt32();
            br.ReadBytes(49);
            ret.Ammo_AutoPistols = br.ReadUInt16();
            ret.Ammo_Uzis = br.ReadUInt16();
            ret.Ammo_Shotgun = (ushort) (br.ReadUInt16() / 6);
            ret.Ammo_M16 = br.ReadUInt16();
            ret.Ammo_Grenade = br.ReadUInt16();
            ret.Ammo_Harpoon = br.ReadUInt16();
            ret.SmallMedipacks = br.ReadByte();
            ret.LargeMedipacks = br.ReadByte();
            br.ReadByte();
            ret.Flares = br.ReadByte();
            br.ReadBytes(2);
            ret.Weapons = (TR2Weapons) br.ReadByte();
            br.ReadBytes(995);
            ret.AmmoUsed = br.ReadUInt32();
            ret.Hits = br.ReadUInt32();

			return ret;
        }
    }

    public class TR45SaveFile
    {
        private static readonly Encoding CP437 = Encoding.GetEncoding(437);

        public string SaveName { get; set; }
        public int SaveNumber { get; set; }
        /// <summary>
        /// 04 = normal, 0C = burning Lara
        /// </summary>
        public byte BurningLara { get; set; }
        /// <summary>
        /// 00 = no, 09 = yes
        /// </summary>
        public byte HasPistols { get; set; }
        /// <summary>
        /// 00 = no, 01 = yes
        /// </summary>
        public byte HasUzis { get; set; }
        /// <summary>
        /// 00 = no, 09 = yes
        /// </summary>
        public byte HasShotgun { get; set; }
        /// <summary>
        /// 00 = no, 09 = yes, 0D = lasersight combined
        /// </summary>
        public byte HasCrossbow { get; set; }
        /// <summary>
        /// 00 = no, 09 = yes
        /// </summary>
        public byte HasGrenadeLauncher { get; set; }
        /// <summary>
        /// 00 = no, 01 = yes, 05 = lasersight combined
        /// </summary>
        public byte HasRevolver { get; set; }
        /// <summary>
        /// 00 = no, 01 = yes (separated)
        /// </summary>
        public byte HasLaserSight { get; set; }
        public byte GoldenSkulls { get; set; }
        public ushort SmallMedipacks { get; set; }
        public ushort LargeMedipacks { get; set; }
        public ushort Flares { get; set; }
        public ushort UziAmmo { get; set; }
        public ushort RevolverAmmo { get; set; }
        public ushort[] ShotgunAmmo { get; set; }
        public ushort[] GrenadeAmmo { get; set; }
        public ushort[] CrossbowAmmo { get; set; }
        public byte SecretsFound { get; set; }
        public byte EastWest { get; set; }
        public byte NorthSouth { get; set; }
        public ushort Orientation { get; set; }

        public double OrientationD { get { return Orientation * 360.0 / 65536; } set { Orientation = (ushort)(value * 65536 / 360); } }

        public static TR45SaveFile Read(string fname, bool tr5)
        {
            using (var fs = File.OpenRead(fname))
            {
                return Read(fs, tr5);
            }
        }

        public static TR45SaveFile Read(Stream s, bool tr5)
        {
            using (var br = new BinaryReader(s))
            {
                return Read(br, tr5);
            }
        }

        public static TR45SaveFile Read(BinaryReader br, bool tr5)
        {
            var ret = new TR45SaveFile();

            ret.SaveName = br.ReadString(40, CP437).StopAtNull();

            br.ReadBytes(35);

            ret.SaveNumber = br.ReadInt32();

            br.ReadBytes(76);

            ret.BurningLara = br.ReadByte();

            br.ReadBytes(205);

            ret.HasPistols = br.ReadByte();
            ret.HasUzis = br.ReadByte();
            ret.HasShotgun = br.ReadByte();
            ret.HasCrossbow = br.ReadByte();
            ret.HasGrenadeLauncher = br.ReadByte();
            ret.HasRevolver = br.ReadByte();
            ret.HasLaserSight = br.ReadByte();

            br.ReadBytes(8);

            ret.GoldenSkulls = br.ReadByte();

            br.ReadBytes(23);

            ret.SmallMedipacks = br.ReadUInt16();
            ret.LargeMedipacks = br.ReadUInt16();
            ret.Flares = br.ReadUInt16();

            ret.UziAmmo = br.ReadUInt16();
            ret.RevolverAmmo = br.ReadUInt16();
            ret.ShotgunAmmo = br.ReadUInt16Array(2);
            ret.GrenadeAmmo = br.ReadUInt16Array(3);
            ret.CrossbowAmmo = br.ReadUInt16Array(3);

            br.ReadBytes(79);

            ret.SecretsFound = br.ReadByte();

            return ret;
        }
    }

    [Flags]
    public enum TR1Weapons : byte
    {
        _ = 1,
        Pistols = 1 << 1,
        Shotgun = 1 << 4,
        Magnum = 1 << 2
    }

    [Flags]
    public enum TR2Weapons : byte
    {
        _ = 1,
        Pistols = 1 << 1,
        AutoPistols = 1 << 2,
        Uzis = 1 << 3,
        Shotgun = 1 << 4,
        M16 = 1 << 5,
        GrenadeLauncher = 1 << 6,
        HarpoonGun = 1 << 7
    }

    [Flags]
    public enum TR3Weapons : ushort
    {
        _ = 1,
        Pistols = 1 << 1,
        DesertEagle = 1 << 2,
        Uzis = 1 << 3,
        Shotgun = 1 << 4,
        MP5 = 1 << 5,
        RocketLauncher = 1 << 6,
        GrenadeLauncher = 1 << 7,
        HarpoonGun = 1 << 8
    }
}
