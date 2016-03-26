using BulletSharp;
using OpenTK;
using static FreeRaider.Constants;
using static FreeRaider.Global;

namespace FreeRaider
{
    public partial class Constants
    {
        public const int JOY_BUTTON_MASK = 1000;

        public const int JOY_HAT_MASK = 1100;

        public const int JOY_TRIGGER_MASK = 1200;

        public const int JOY_TRIGGER_DEADZONE = 10000;
    }

    public enum ACTIONS
    {
        // Movement directions
        Up, // 0
        Down, // 1
        Left, // 2
        Right, // 3

        // Functional keys
        Action, // 4
        Jump, // 5
        Roll, // 6
        DrawWeapon, // 7
        Look, // 8
        Walk, // 9
        Sprint, // 10
        Crouch, // 11
        StepLeft, // 12
        StepRight, // 13

        // Free look keys
        LookUp, // 14
        LookDown, // 15
        LookLeft, // 16
        LookRight, // 17

        // Weapon scroller
        NextWeapon, // 18
        PrevWeapon, // 19

        // Item hotkeys
        Flare, // 20
        BigMedi, // 21
        SmallMedi, // 22
        Weapon1, // 23
        Weapon2, // 24
        Weapon3, // 25
        Weapon4, // 26
        Weapon5, // 27
        Weapon6, // 28
        Weapon7, // 29
        Weapon8, // 30
        Weapon9, // 31
        Weapon10, // 32
        Binoculars, // 33
        Pls, // 34 not in original, reserved for future

        // Interface keys
        Pause, // 35
        Inventory, // 36
        Diary, // 37 not in original, reserved for future
        Map, // 38 not in original, reserved for future
        LoadGame, // 39
        SaveGame, // 40

        // Service keys
        Console, // 41
        Screenshot, // 42

        // Last action index. This should ALWAYS remain last entry!
        LastIndex // 43
    }

    public enum AXES
    {
        LookX,        // Look axes
        LookY,
        MoveX,        // Move axes
        MoveY,
        // Last axis index. This should ALWAYS remain last entry!
        LastIndex
    }

    public class ControlAction
    {
        public int Primary = 0;

        public int Secondary = 0;

        public bool State = false;

        public bool AlreadyPressed = false;
    }

    public class ControlSettings
    {
        public float MouseSensitivity = 0;

        public float MouseScaleX = 0.01f;

        public float MouseScaleY = 0.01f;

        #region Joystick settings

        public bool UseJoy = false;

        public int JoyNumber = 0;

        public bool JoyRumble = false;

        #endregion

        #region Look axis settings

        public float JoyLookX = 0;

        public float JoyLookY = 0;

        public bool JoyLookInvertX = false;

        public bool JoyLookInvertY = false;

        public float JoyLookSensitivity = 0;

        public short JoyLookDeadzone = 0;

        #endregion

        #region Move axis settings

        public float JoyMoveX = 0;

        public float JoyMoveY = 0;

        public bool JoyMoveInvertX = false;

        public bool JoyMoveInvertY = false;

        public float JoyMoveSensitivity = 0;

        public short JoyMoveDeadzone = 0;

        #endregion

        /// <summary>
        /// Axis array for action mapper.
        /// </summary>
        public int[] JoyAxisMap = new int[(int)AXES.LastIndex];

        /// <summary>
        /// Actions array for action mapper.
        /// </summary>
        public ControlAction[] ActionMap = new ControlAction[(int)ACTIONS.LastIndex];
    }

    public class Controls
    {
        public static void PollSDLInput();

        public static void DebugKeys(int button, int state);

        public static void PrimaryMouseDown()
        {
            var cont = new EngineContainer();
            var dbgR = 128.0f;
            var v = EngineCamera.Position;
            var dir = EngineCamera.ViewDirection;
            var localInertia = Vector3.Zero;

            var cshape = new SphereShape(dbgR);
            cshape.Margin = COLLISION_MARGIN_DEFAULT;

            var startTransform = new Transform();
            startTransform.SetIdentity();
            var newPos = v;
            startTransform.Origin = newPos;
            cshape.CalculateLocalInertia(12.0f, out localInertia);
            var motionState = new DefaultMotionState((Matrix4)startTransform);
            var body = new RigidBody(new RigidBodyConstructionInfo(12.0f, motionState, cshape, localInertia));
            BtEngineDynamicsWorld.AddRigidBody(body);
            body.LinearVelocity = dir * 6000;
            cont.Room = Room.FindPosCogerrence(newPos, EngineCamera.CurrentRoom);
            cont.ObjectType = OBJECT_TYPE.BulletMisc; // bullet have to destroy this user pointer
            body.UserObject = cont;
            body.CcdMotionThreshold = dbgR; // disable tunneling effect
            body.CcdSweptSphereRadius = dbgR;
        }

        public static void SecondaryMouseDown()
        {
            var from = EngineCamera.Position;
            var to = from + EngineCamera.ViewDirection * 32768.0f;

            var camCont = new EngineContainer {Room = EngineCamera.CurrentRoom};

            var cbc = new BtEngineClosestRayResultCallback(camCont);
            //cbc.CollisionFilterMask = CollisionFilterGroups.StaticFilter | CollisionFilterGroups.KinematicFilter;
            BtEngineDynamicsWorld.RayTest(from, to, cbc);
            if(cbc.HasHit)
            {
                var castRay = new float[6];

                Vector3 place;
                Helper.SetInterpolate3(out place, from, to, cbc.ClosestHitFraction);
                place.CopyToArray(castRay, 0);
                (place + 100.0f * cbc.HitNormalWorld).CopyToArray(castRay, 3);

                var c0 = (EngineContainer) cbc.CollisionObject.UserObject;
                if(c0 != null)
                {
                    if(c0.ObjectType == OBJECT_TYPE.BulletMisc)
                    {
                        var obj = cbc.CollisionObject;
                        var body = RigidBody.Upcast(obj);
                        body?.MotionState?.Dispose();
                        body?.CollisionShape?.Dispose();

                        if(body != null)
                        {
                            body.UserObject = null;
                        }
                        c0.Room = null;
                        c0 = null;

                        BtEngineDynamicsWorld.RemoveCollisionObject(obj);
                        obj.Dispose();
                    }
                    else
                    {
                        LastContainer = c0;
                    }
                }
            }
        }

        public static void Key(int button, bool state);

        public static void WrapGameControllerKey(int button, bool state);

        public static void WrapGameControllerAxis(int axis, short value);

        public static void JoyAxis(int axis, short value);

        public static void JoyHat(int value);

        public static void JoyRumble(float power, int time);

        public static void RefreshStates()
        {
            for(var i = 0; i < (int)ACTIONS.LastIndex; i++)
            {
                ControlMapper.ActionMap[i].AlreadyPressed = ControlMapper.ActionMap[i].State;
            }
        }

        public static void InitGlobals();
    }
}
