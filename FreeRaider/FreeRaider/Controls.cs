using System;
using System.Runtime.InteropServices;
using BulletSharp;
using OpenTK;
using static FreeRaider.Constants;
using static FreeRaider.Global;
using static SDL2.SDL;
using static SDL2.SDL.SDL_Keycode;
using static SDL2.SDL.SDL_Keymod;

namespace FreeRaider
{
    public partial class Constants
    {
        public const int JOY_BUTTON_MASK = 1000;

        public const int JOY_HAT_MASK = 1100;

        public const int JOY_TRIGGER_MASK = 1200;

        public const int JOY_TRIGGER_DEADZONE = 10000;
    }

    public partial class Global
    {
        public static IntPtr sdl_joystick = IntPtr.Zero;

        public static IntPtr sdl_controller = IntPtr.Zero;

        public static IntPtr sdl_haptic = IntPtr.Zero;

        public static IntPtr sdl_window = IntPtr.Zero;

        public static IntPtr sdl_gl_context = IntPtr.Zero;

        public static IntPtr sdl_renderer = IntPtr.Zero;
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
        public ControlAction[] ActionMap = Helper.RepeatValue((int) ACTIONS.LastIndex, () => new ControlAction());
    }

    public class Controls
    {
        public static void PollSDLInput()
        {
            SDL_Event ev;

            while(SDL_PollEvent(out ev) != 0)
            {
                switch(ev.type)
                {
                    case SDL_EventType.SDL_MOUSEMOTION:
                        if(!ConsoleInfo.Instance.IsVisible && ControlStates.MouseLook)
                        {
                            ControlStates.LookAxisX = ev.motion.xrel * ControlMapper.MouseSensitivity *
                                                      ControlMapper.MouseScaleX;
                            ControlStates.LookAxisY = ev.motion.yrel * ControlMapper.MouseSensitivity *
                                                      ControlMapper.MouseScaleY;
                        }
                        break;

                    case SDL_EventType.SDL_MOUSEBUTTONDOWN:
                        if(ev.button.button == 1) // LM = 1, MM = 2, RM = 3
                        {
                            PrimaryMouseDown();
                        }
                        else if(ev.button.button == 3)
                        {
                            SecondaryMouseDown();
                        }
                        break;

                    // Controller events are only invoked when joystick is initialized as
                    // game controller, otherwise, generic joystick event will be used.
                    case SDL_EventType.SDL_CONTROLLERAXISMOTION:
                        WrapGameControllerAxis(ev.caxis.axis, ev.caxis.axisValue);
                        break;

                    case SDL_EventType.SDL_CONTROLLERBUTTONDOWN:
                    case SDL_EventType.SDL_CONTROLLERBUTTONUP:
                        WrapGameControllerKey(ev.cbutton.button, ev.cbutton.state == SDL_PRESSED);
                        break;

                    // Joystick events are still invoked, even if joystick is initialized as game
                    // controller - that's why we need sdl_joystick checking - to filter out
                    // duplicate event calls.
                    case SDL_EventType.SDL_JOYAXISMOTION:
                        if(sdl_joystick != IntPtr.Zero)
                            JoyAxis(ev.jaxis.axis, ev.jaxis.axisValue);
                        break;

                    case SDL_EventType.SDL_JOYHATMOTION:
                        if(sdl_joystick != IntPtr.Zero)
                            JoyHat(ev.jhat.hatValue);
                        break;

                    case SDL_EventType.SDL_JOYBUTTONDOWN:
                    case SDL_EventType.SDL_JOYBUTTONUP:
                        // NOTE: Joystick button numbers are passed with added JOY_BUTTON_MASK (1000).
                        if (sdl_joystick != IntPtr.Zero)
                            Key(ev.jbutton.button + JOY_BUTTON_MASK, ev.jbutton.state == SDL_PRESSED);
                        break;

                    case SDL_EventType.SDL_TEXTINPUT:
                    case SDL_EventType.SDL_TEXTEDITING:
                        if(ConsoleInfo.Instance.IsVisible && ev.key.state != 0)
                        {
                            unsafe
                            {
                                ConsoleInfo.Instance.Filter(Marshal.PtrToStringAnsi((IntPtr) ev.text.text)); // TODO: Ansi instead?
                            }
                            return;
                        }
                        break;

                    case SDL_EventType.SDL_KEYUP:
                    case SDL_EventType.SDL_KEYDOWN:
                        if(ev.key.keysym.sym == SDLK_PAGEDOWN)
                        {
                            ConsoleInfo.Instance.AddLine(Helper.Format("pos = {0}", EngineWorld.Character.Bf.Position), FontStyle.ConsoleNotify);
                            break;
                        }

                        if(ev.key.keysym.sym == SDLK_F4 &&
                            ev.key.state == SDL_PRESSED && 
                            ev.key.keysym.mod.HasFlagUns(KMOD_ALT))
                        {
                            Done = true;
                            break;
                        }

                        if(ConsoleInfo.Instance.IsVisible && ev.key.state != 0)
                        {
                            switch(ev.key.keysym.sym)
                            {
                                case SDLK_RETURN:
                                case SDLK_UP:
                                case SDLK_DOWN:
                                case SDLK_LEFT:
                                case SDLK_RIGHT:
                                case SDLK_HOME:
                                case SDLK_END:
                                case SDLK_BACKSPACE:
                                case SDLK_DELETE:
                                case SDLK_TAB:
                                case SDLK_v: // for Ctrl+V
                                    ConsoleInfo.Instance.Edit((int)ev.key.keysym.sym, (int)ev.key.keysym.mod);
                                    break;
                                default:
                                    break;
                            }
                            return;
                        }
                        else
                        {
                            Key((int)ev.key.keysym.sym, ev.key.state == SDL_PRESSED);
                            // DEBUG KEYBOARD COMMANDS
                            DebugKeys(ev.key.keysym.sym, ev.key.state);
                        }
                        break;

                    case SDL_EventType.SDL_QUIT:
                        Done = true;
                        break;

                    case SDL_EventType.SDL_WINDOWEVENT:
                        if(ev.window.windowEvent == SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED)
                        {
                            Engine.Resize(ev.window.data1, ev.window.data2, ev.window.data1, ev.window.data2);
                        }
                        break;

                    default:
                        break;
                }
            }
        }

        public static void DebugKeys(SDL_Keycode button, int state)
        {
            if(state != 0)
            {
                if (MainInventoryManager != null)
                {
                    switch (button)
                    {
                        case SDLK_RETURN:
                            MainInventoryManager.NextState = InventoryManager.InventoryState.Activate;
                            break;

                        case SDLK_UP:
                            MainInventoryManager.NextState = InventoryManager.InventoryState.Up;
                            break;

                        case SDLK_DOWN:
                            MainInventoryManager.NextState = InventoryManager.InventoryState.Down;
                            break;

                        case SDLK_LEFT:
                            MainInventoryManager.NextState = InventoryManager.InventoryState.RLeft;
                            break;

                        case SDLK_RIGHT:
                            MainInventoryManager.NextState = InventoryManager.InventoryState.RRight;
                            break;
                    }
                }
            }
        }

        public static void PrimaryMouseDown()
        {
            var cont = new EngineContainer();
            var dbgR = 128.0f;
            var v = EngineCamera.Position;
            var dir = EngineCamera.ViewDirection;
            var localInertia = BulletSharp.Math.Vector3.Zero;

            var cshape = new SphereShape(dbgR);
            cshape.Margin = COLLISION_MARGIN_DEFAULT;

            var startTransform = new Transform();
            startTransform.SetIdentity();
            var newPos = v;
            startTransform.Origin = newPos;
            cshape.CalculateLocalInertia(12.0f, out localInertia);
            var motionState = new DefaultMotionState(((Matrix4)startTransform).ToBullet());
            var body = new RigidBody(new RigidBodyConstructionInfo(12.0f, motionState, cshape, localInertia));
            BtEngineDynamicsWorld.AddRigidBody(body);
            body.LinearVelocity = (dir * 6000).ToBullet();
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
            BtEngineDynamicsWorld.RayTest(from.ToBullet(), to.ToBullet(), cbc);
            if(cbc.HasHit)
            {
                var castRay = new float[6];

                Vector3 place;
                Helper.SetInterpolate3(out place, from, to, cbc.ClosestHitFraction);
                place.CopyToArray(castRay, 0);
                (place + 100.0f * cbc.HitNormalWorld.ToOpenTK()).CopyToArray(castRay, 3);

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

        public static void Key(int button, bool state)
        {
            // Fill script-driven debug keyboard input.

            EngineLua.AddKey(button, state);

            // Compare ALL mapped buttons.

            for(var i = 0; i < (int)ACTIONS.LastIndex; i++)
            {
                if (button.IsAnyOf(ControlMapper.ActionMap[i].Primary, // If button = mapped action...
                    ControlMapper.ActionMap[i].Secondary))
                {
                    switch((ACTIONS)i) // ...Choose corresponding action.
                    {
                        case ACTIONS.Up:
                            ControlStates.MoveForward = state;
                            break;

                        case ACTIONS.Down:
                            ControlStates.MoveBackward = state;
                            break;

                        case ACTIONS.Left:
                            ControlStates.MoveLeft = state;
                            break;

                        case ACTIONS.Right:
                            ControlStates.MoveRight = state;
                            break;

                        case ACTIONS.DrawWeapon:
                            ControlStates.DoDrawWeapon = state;
                            break;

                        case ACTIONS.Action:
                            ControlStates.StateAction = state;
                            break;

                        case ACTIONS.Jump:
                            ControlStates.MoveUp = ControlStates.DoJump = state;
                            break;

                        case ACTIONS.Roll:
                            ControlStates.DoRoll = state;
                            break;

                        case ACTIONS.Walk:
                            ControlStates.StateWalk = state;
                            break;

                        case ACTIONS.Sprint:
                            ControlStates.StateSprint = state;
                            break;

                        case ACTIONS.Crouch:
                            ControlStates.MoveDown = ControlStates.StateCrouch = state;
                            break;

                        case ACTIONS.LookUp:
                            ControlStates.LookUp = state;
                            break;

                        case ACTIONS.LookDown:
                            ControlStates.LookDown = state;
                            break;

                        case ACTIONS.LookLeft:
                            ControlStates.LookLeft = state;
                            break;

                        case ACTIONS.LookRight:
                            ControlStates.LookRight = state;
                            break;

                        case ACTIONS.BigMedi:
                            if(!ControlMapper.ActionMap[i].AlreadyPressed)
                            {
                                ControlStates.UseBigMedipack = state;
                            }
                            break;

                        case ACTIONS.SmallMedi:
                            if(!ControlMapper.ActionMap[i].AlreadyPressed)
                            {
                                ControlStates.UseSmallMedipack = state;
                            }
                            break;

                        case ACTIONS.Console:
                            if(!state)
                            {
                                ConsoleInfo.Instance.ToggleVisibility();

                                if(ConsoleInfo.Instance.IsVisible)
                                {
                                    SDL_SetRelativeMouseMode(SDL_bool.SDL_FALSE);
                                    SDL_StartTextInput();
                                }
                                else
                                {
                                    SDL_SetRelativeMouseMode(SDL_bool.SDL_TRUE);
                                    SDL_StopTextInput();
                                }
                            }
                            break;

                        case ACTIONS.Screenshot:
                            if(!state)
                            {
                                Common.TakeScreenshot();
                            }
                            break;

                        case ACTIONS.Inventory:
                            ControlStates.GuiInventory = state;
                            break;

                        case ACTIONS.SaveGame:
                            if(!state)
                            {
                                Game.Save("qsave.lua");
                            }
                            break;

                        case ACTIONS.LoadGame:
                            if(!state)
                            {
                                Game.Load("qsave.lua");
                            }
                            break;

                        default:
                            // ControlStates.MoveForward = state;
                            return;
                    }

                    ControlMapper.ActionMap[i].State = state;
                }
            }
        }

        public static void WrapGameControllerKey(int button, bool state)
        {
            // SDL2 Game Controller interface doesn't operate with HAT directions,
            // instead it treats them as button pushes. So, HAT doesn't return
            // hat motion event on any HAT direction release - instead, each HAT
            // direction generates its own press and release event. That's why
            // game controller's HAT (DPAD) events are directly translated to
            // Controls.Key function.

            switch ((SDL_GameControllerButton)button)
            {
                case SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_UP:
                    Key(JOY_HAT_MASK + SDL_HAT_UP, state);
                    break;
                case SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_DOWN:
                    Key(JOY_HAT_MASK + SDL_HAT_DOWN, state);
                    break;
                case SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_LEFT:
                    Key(JOY_HAT_MASK + SDL_HAT_LEFT, state);
                    break;
                case SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_RIGHT:
                    Key(JOY_HAT_MASK + SDL_HAT_RIGHT, state);
                    break;
                default:
                    Key(JOY_BUTTON_MASK + button, state);
                    break;
            }
        }

        public static void WrapGameControllerAxis(int axis, short value)
        {
            // Since left/right triggers on X360-like controllers are actually axes,
            // and we still need them as buttons, we remap these axes to button events.
            // Button event is invoked only if trigger is pressed more than 1/3 of its range.
            // Triggers are coded as native SDL2 enum number + JOY_TRIGGER_MASK (1200).

            if (axis.IsAnyOf((int) SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_TRIGGERLEFT,
                (int) SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_TRIGGERRIGHT))
            {
                Key(axis + JOY_TRIGGER_MASK, value >= JOY_TRIGGER_DEADZONE);
            }
            else
            {
                JoyAxis(axis, value);
            }
        }

        public static void JoyAxis(int axis, short value)
        {
            for (var i = 0; i < (int) AXES.LastIndex; i++) // Compare with ALL mapped axes.
            {
                if (axis == ControlMapper.JoyAxisMap[i]) // If mapped = current...
                {
                    switch ((AXES) i) // ...Choose corresponding action.
                    {
                        case AXES.LookX:
                            if (value.IsBetween(-ControlMapper.JoyLookDeadzone, ControlMapper.JoyLookDeadzone))
                            {
                                ControlMapper.JoyLookX = 0;
                            }
                            else
                            {
                                ControlMapper.JoyLookX = value * ControlMapper.JoyLookSensitivity / 32767;
                                if (ControlMapper.JoyLookInvertX) ControlMapper.JoyLookX = -ControlMapper.JoyLookX;
                            }
                            return;

                        case AXES.LookY:
                            if (value.IsBetween(-ControlMapper.JoyLookDeadzone, ControlMapper.JoyLookDeadzone))
                            {
                                ControlMapper.JoyLookY = 0;
                            }
                            else
                            {
                                ControlMapper.JoyLookY = value * ControlMapper.JoyLookSensitivity / 32767;
                                if (ControlMapper.JoyLookInvertY) ControlMapper.JoyLookY = -ControlMapper.JoyLookY;
                            }
                            return;

                        case AXES.MoveX:
                            if (value.IsBetween(-ControlMapper.JoyMoveDeadzone, ControlMapper.JoyMoveDeadzone))
                            {
                                ControlStates.MoveLeft = ControlStates.MoveRight = false;
                                ControlMapper.JoyMoveX = 0;
                            }
                            else
                            {
                                if (ControlMapper.JoyMoveInvertX)
                                {
                                    ControlMapper.JoyMoveX = -(value * ControlMapper.JoyMoveSensitivity / 32767);

                                    if (value > ControlMapper.JoyMoveDeadzone)
                                    {
                                        ControlStates.MoveLeft = true;
                                        ControlStates.MoveRight = false;
                                    }
                                    else
                                    {
                                        ControlStates.MoveLeft = false;
                                        ControlStates.MoveRight = true;
                                    }
                                }
                                else
                                {
                                    ControlMapper.JoyMoveX = value * ControlMapper.JoyMoveSensitivity / 32767;

                                    if (value > ControlMapper.JoyMoveDeadzone)
                                    {
                                        ControlStates.MoveLeft = false;
                                        ControlStates.MoveRight = true;
                                    }
                                    else
                                    {
                                        ControlStates.MoveLeft = true;
                                        ControlStates.MoveRight = false;
                                    }
                                }
                            }
                            return;


                        case AXES.MoveY:
                            if (value.IsBetween(-ControlMapper.JoyMoveDeadzone, ControlMapper.JoyMoveDeadzone))
                            {
                                ControlStates.MoveForward = ControlStates.MoveBackward = false;
                                ControlMapper.JoyMoveY = 0;
                            }
                            else
                            {
                                if (ControlMapper.JoyMoveInvertY)
                                {
                                    ControlMapper.JoyMoveY = -(value * ControlMapper.JoyMoveSensitivity / 32767);

                                    if (value > ControlMapper.JoyMoveDeadzone)
                                    {
                                        ControlStates.MoveForward = true;
                                        ControlStates.MoveBackward = false;
                                    }
                                    else
                                    {
                                        ControlStates.MoveForward = false;
                                        ControlStates.MoveBackward = true;
                                    }
                                }
                                else
                                {
                                    ControlMapper.JoyMoveY = value * ControlMapper.JoyMoveSensitivity / 32767;

                                    if (value > ControlMapper.JoyMoveDeadzone)
                                    {
                                        ControlStates.MoveForward = false;
                                        ControlStates.MoveBackward = true;
                                    }
                                    else
                                    {
                                        ControlStates.MoveForward = true;
                                        ControlStates.MoveBackward = false;
                                    }
                                }
                            }
                            return;

                        default:
                            return;
                    } // end switch((AXES)i)
                } // end if(axis == ControlMapper.JoyAxisMap[i])
            } // end for (var i = 0; i < (int) AXES.LASTINDEX; i++)
        }

        public static void JoyHat(int value)
        {
            // NOTE: Hat movements emulate keypresses
            // with HAT direction + JOY_HAT_MASK (1100) index.

            Key(JOY_HAT_MASK + SDL_HAT_UP, false); // Reset all directions.
            Key(JOY_HAT_MASK + SDL_HAT_DOWN, false);
            Key(JOY_HAT_MASK + SDL_HAT_LEFT, false);
            Key(JOY_HAT_MASK + SDL_HAT_RIGHT, false);

            if (value.HasFlagSig(SDL_HAT_UP))
                Key(JOY_HAT_MASK + SDL_HAT_UP, true);
            if (value.HasFlagSig(SDL_HAT_DOWN))
                Key(JOY_HAT_MASK + SDL_HAT_DOWN, true);
            if (value.HasFlagSig(SDL_HAT_LEFT))
                Key(JOY_HAT_MASK + SDL_HAT_LEFT, true);
            if (value.HasFlagSig(SDL_HAT_RIGHT))
                Key(JOY_HAT_MASK + SDL_HAT_RIGHT, true);
        }

        public static void JoyRumble(float power, uint time)
        {
            // JoyRumble is a simple wrapper for SDL's haptic rumble play.

            if (sdl_haptic != IntPtr.Zero)
                SDL_HapticRumblePlay(sdl_haptic, power, time);
        }

        public static void RefreshStates()
        {
            for(var i = 0; i < (int)ACTIONS.LastIndex; i++)
            {
                ControlMapper.ActionMap[i].AlreadyPressed = ControlMapper.ActionMap[i].State;
            }
        }

        public static void InitGlobals()
        {
            ControlMapper.MouseSensitivity = 25.0f;
            ControlMapper.UseJoy = false;

            ControlMapper.JoyNumber = 0;                // FIXME: Replace with joystick scanner default value when done.
            ControlMapper.JoyRumble = false;            // FIXME: Make it according to GetCaps of default joystick.

            ControlMapper.JoyAxisMap[(int)AXES.MoveX] = 0;
            ControlMapper.JoyAxisMap[(int)AXES.MoveY] = 1;
            ControlMapper.JoyAxisMap[(int)AXES.LookX] = 2;
            ControlMapper.JoyAxisMap[(int)AXES.LookY] = 3;

            ControlMapper.JoyLookInvertX = false;
            ControlMapper.JoyLookInvertY = false;
            ControlMapper.JoyMoveInvertX = false;
            ControlMapper.JoyMoveInvertY = false;

            ControlMapper.JoyLookDeadzone = 1500;
            ControlMapper.JoyMoveDeadzone = 1500;

            ControlMapper.JoyLookSensitivity = 1.5f;
            ControlMapper.JoyMoveSensitivity = 1.5f;

            ControlMapper.ActionMap[(int) ACTIONS.Jump].Primary = (int) SDLK_SPACE;
            ControlMapper.ActionMap[(int)ACTIONS.Action].Primary = (int) SDLK_LCTRL;
            ControlMapper.ActionMap[(int)ACTIONS.Roll].Primary = (int) SDLK_x;
            ControlMapper.ActionMap[(int)ACTIONS.Sprint].Primary = (int) SDLK_CAPSLOCK;
            ControlMapper.ActionMap[(int)ACTIONS.Crouch].Primary = (int) SDLK_c;
            ControlMapper.ActionMap[(int)ACTIONS.Walk].Primary = (int) SDLK_LSHIFT;

            ControlMapper.ActionMap[(int)ACTIONS.Up].Primary = (int) SDLK_w;
            ControlMapper.ActionMap[(int)ACTIONS.Down].Primary = (int) SDLK_s;
            ControlMapper.ActionMap[(int)ACTIONS.Left].Primary = (int) SDLK_a;
            ControlMapper.ActionMap[(int)ACTIONS.Right].Primary = (int) SDLK_d;

            ControlMapper.ActionMap[(int)ACTIONS.StepLeft].Primary = (int) SDLK_h;
            ControlMapper.ActionMap[(int)ACTIONS.StepRight].Primary = (int) SDLK_j;

            ControlMapper.ActionMap[(int)ACTIONS.LookUp].Primary = (int) SDLK_UP;
            ControlMapper.ActionMap[(int)ACTIONS.LookDown].Primary = (int) SDLK_DOWN;
            ControlMapper.ActionMap[(int)ACTIONS.LookLeft].Primary = (int) SDLK_LEFT;
            ControlMapper.ActionMap[(int)ACTIONS.LookRight].Primary = (int) SDLK_RIGHT;

            ControlMapper.ActionMap[(int)ACTIONS.Screenshot].Primary = (int) SDLK_PRINTSCREEN;
            ControlMapper.ActionMap[(int)ACTIONS.Console].Primary = (int) SDLK_F12;
            ControlMapper.ActionMap[(int)ACTIONS.SaveGame].Primary = (int) SDLK_F5;
            ControlMapper.ActionMap[(int)ACTIONS.LoadGame].Primary = (int) SDLK_F6;
        }
    }
}
