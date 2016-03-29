using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using BulletSharp;
using FreeRaider.Loader;
using FreeRaider.Script;
using NLua.Exceptions;
using OpenTK;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using OpenTK.Graphics.OpenGL;
using static FreeRaider.Constants;
using static FreeRaider.Global;
using static FreeRaider.Strings;
using static FreeRaider.StaticFuncs;

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
        public static EngineControlState ControlStates = new EngineControlState();

        public static ControlSettings ControlMapper = new ControlSettings();

        public static AudioSettings AudioSettings = new AudioSettings();

        public static float EngineFrameTime = 0.0f;

        public static Camera EngineCamera;

        public static World EngineWorld;

        public static List<float> FrameVertexBuffer = new List<float>();

        public static int FrameVertexBufferSizeLeft = 0; 

        public static DefaultCollisionConfiguration BtEngineCollisionConfiguration = null;

        public static CollisionDispatcher BtEngineDispatcher = null;

        public static GhostPairCallback BtEngineGhostPairCallback = null;

        public static BroadphaseInterface BtEngineOverlappingPairCache = null;

        public static SequentialImpulseConstraintSolver BtEngineSolver = null;

        public static DiscreteDynamicsWorld BtEngineDynamicsWorld = null;

        public static OverlapFilterCallback BtEngineFilterCallback = null;

        public static IntPtr ALDevice;

        public static ContextHandle ALContext;

        // Debug globals

        public static Vector3 LightPosition = new Vector3(255.0f, 255.0f, 8.0f);

        public static float[] CastRay = {0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f};

        public static int FPSCycles = 0;

        public static float FPSTime = 0.0f;
    }

    public partial class StaticFuncs
    {
        public static void FPSCycle(float time)
        {
            if(FPSCycles < 20)
            {
                FPSCycles++;
                FPSTime += time;
            }
            else
            {
                Global.ScreenInfo.Fps = 20.0f / FPSTime;
                SystemFps.Text = Math.Round(Global.ScreenInfo.Fps, 1).ToString();
                FPSCycles = 0;
                FPSTime = 0.0f;
            }
        }
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

        public static void Start()
        {
            // Set defaults parameters and load config file.
            InitConfig("config.lua");

            // Primary initialization.
            InitPre();

            // Init generic SDL interfaces.
            InitSDLControls();
            InitSDLVideo();

            // Additional OpenGL initialization.
            InitGL();
            Renderer.DoShaders();

            // Secondary (deferred) initialization.
            InitPost();

            // Initial window resize.
            Resize(Global.ScreenInfo.W, Global.ScreenInfo.H, Global.ScreenInfo.W, Global.ScreenInfo.H);

            // OpenAL initialization.
            InitAL();

            ConsoleInfo.Instance.Notify(SYSNOTE_ENGINE_INITED);

            // Clearing up memory for initial level loading.
            EngineWorld.Prepare();

            // SDL_SetRelativeMouseMode(SDL_TRUE); TODO

            // Make splash screen.
            Gui.FadeAssignPic(FaderType.LoadScreen, "resource/graphics/legal.png");
            Gui.FadeStart(FaderType.LoadScreen, FaderDir.Out);

            EngineLua.DoFile("autoexec.lua");
        }

        public static void Destroy()
        {
            Renderer.Empty();
            //ConsoleInfo.Instance.Destroy();
            Common.Destroy();
            Sys.Destroy();

            // delete dynamics world
            BtEngineDynamicsWorld.Dispose();
            BtEngineDynamicsWorld = null;

            // delete solver
            BtEngineSolver.Dispose();
            BtEngineSolver = null;

            // delete broadphase
            BtEngineOverlappingPairCache.Dispose();
            BtEngineOverlappingPairCache = null;

            // delete disptacher
            BtEngineDispatcher.Dispose();
            BtEngineDispatcher = null;

            BtEngineCollisionConfiguration.Dispose();
            BtEngineCollisionConfiguration = null;

            BtEngineGhostPairCallback.Dispose();
            BtEngineGhostPairCallback = null;

            Gui.Destroy();
        }

        public static void Shutdown(int val)
        {
            EngineLua.ClearTasks();
            Renderer.Empty();
            EngineWorld.Empty();
            Destroy();

            // TODO Joystick SDL stuff etc L792 Engine.cpp

            if(ALContext != ContextHandle.Zero)
            {
                Alc.MakeContextCurrent(ContextHandle.Zero);
                Alc.DestroyContext(ALContext);
            }

            if(ALDevice != IntPtr.Zero)
            {
                Alc.CloseDevice(ALDevice);
            }

            // free temporary memory
            FrameVertexBuffer.Clear();
            FrameVertexBufferSizeLeft = 0;

            Environment.Exit(val);
        }

        #endregion

        #region Initializers

        /// <summary>
        /// Initial init
        /// </summary>
        public static void InitPre()
        {
            Gui.InitFontManager();
            ConsoleInfo.Instance.Init();

            EngineLua.Call("loadscript_pre");

            GameflowManager.Init();

            FrameVertexBuffer.Resize(INIT_FRAME_VERTEX_BUFFER_SIZE);
            FrameVertexBufferSizeLeft = FrameVertexBuffer.Count;

            ConsoleInfo.Instance.CompletionItems = EngineLua.GetGlobals();

            Common.Init();
            Renderer.Init();
            Renderer.Camera = EngineCamera;

            InitBullet();
        }

        /// <summary>
        /// Finalizing init
        /// </summary>
        public static void InitPost()
        {
            EngineLua.Call("loadscript_post");

            ConsoleInfo.Instance.InitFonts();

            Gui.Init();
            Sys.Init();
        }

        public static void InitDefaultGlobals()
        {
            ConsoleInfo.Instance.InitGlobals();
            Controls.InitGlobals();
            Game.InitGlobals();
            Renderer.InitGlobals();
            Audio.InitGlobals();
        }

        public static void InitGL()
        {
            GL.GetError();

            GL.ClearColor(Color.Black);

            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);

            if(Renderer.Settings.Antialias)
            {
                GL.Enable(EnableCap.Multisample);
            }
            else
            {
                GL.Disable(EnableCap.Multisample);
            }
        }

        public static void InitSDLControls();

        public static void InitSDLVideo();

        public static void InitSDLImage();

        public static void InitAL()
        {
            if(!NO_AUDIO)
            {
                var paramList = new int[]
                {
                    (int)AlcContextAttributes.StereoSources, TR_AUDIO_STREAM_NUMSOURCES,
                    (int)AlcContextAttributes.MonoSources,
                    TR_AUDIO_MAX_CHANNELS - TR_AUDIO_STREAM_NUMSOURCES,
                    (int)AlcContextAttributes.Frequency, 44100, 0
                };

                Sys.DebugLog(LOG_FILENAME, "Probing OpenAL devices...");

                var devlist = Alc.GetString(IntPtr.Zero, AlcGetStringList.DeviceSpecifier);

                if(devlist.Count == 0)
                {
                    Sys.DebugLog(LOG_FILENAME, "InitAL: No AL audio devices!");
                    return;
                }

                foreach (var s in devlist)
                {
                    Sys.DebugLog(LOG_FILENAME, " Device: {0}", s);
                    var dev = Alc.OpenDevice(s);

                    if(Global.AudioSettings.UseEffects)
                    {
                        if(Alc.IsExtensionPresent(dev, "ALC_EXT_EFX"))
                        {
                            Sys.DebugLog(LOG_FILENAME, " EFX supported!");
                            ALDevice = dev;
                            ALContext = Alc.CreateContext(dev, paramList);
                            // fails e.g. with Rapture3D, where EFX is supported
                            if(ALContext != ContextHandle.Zero)
                            {
                                break;
                            }
                        }
                        Alc.CloseDevice(dev);
                    }
                    else
                    {
                        ALDevice = dev;
                        ALContext = Alc.CreateContext(dev, paramList);
                        break;
                    }
                }

                if(ALContext == ContextHandle.Zero)
                {
                    Sys.DebugLog(LOG_FILENAME, " Failed to create OpenAL context.");
                    Alc.CloseDevice(ALDevice);
                    ALDevice = IntPtr.Zero;
                    return;
                }

                Alc.MakeContextCurrent(ALContext);

                Audio.LoadALExtFunctions(ALDevice);

                var driver = "OpenAL library: " + Alc.GetString(ALDevice, AlcGetString.DeviceSpecifier);
                ConsoleInfo.Instance.AddLine(driver, FontStyle.ConsoleInfo);

                AL.SpeedOfSound(330.0f * 512.0f);
                AL.DopplerVelocity(330.0f * 510.0f);
                AL.DistanceModel(ALDistanceModel.LinearDistanceClamped);
            }
        }

        public static void InitBullet()
        {
            // collision configuration contains default setup for memory, collision setup. Advanced users can create their own configuration.
            BtEngineCollisionConfiguration = new DefaultCollisionConfiguration();

            // use the default collision dispatcher. For parallel processing you can use a diffent dispatcher (see Extras/BulletMultiThreaded)
            BtEngineDispatcher = new CollisionDispatcher(BtEngineCollisionConfiguration);
            BtEngineDispatcher.NearCallback = RoomNearCallback;

            // btDbvtBroadphase is a good general purpose broadphase. You can also try out btAxis3Sweep.
            BtEngineOverlappingPairCache = new DbvtBroadphase();
            BtEngineGhostPairCallback = new GhostPairCallback();
            BtEngineOverlappingPairCache.OverlappingPairCache.SetInternalGhostPairCallback(BtEngineGhostPairCallback);

            // the default constraint solver. For parallel processing you can use a different solver (see Extras/BulletMultiThreaded)
            BtEngineSolver = new SequentialImpulseConstraintSolver();

            BtEngineDynamicsWorld = new DiscreteDynamicsWorld(BtEngineDispatcher,
                BtEngineOverlappingPairCache, BtEngineSolver, BtEngineCollisionConfiguration);
            BtEngineDynamicsWorld.SetInternalTickCallback(InternalTickCallback);
            BtEngineDynamicsWorld.Gravity = new Vector3(0, 0, -4500.0f);

            DebugDrawer.DebugMode = DebugDrawModes.DrawWireframe | DebugDrawModes.DrawConstraints;
            BtEngineDynamicsWorld.DebugDrawer = DebugDrawer;
            //Global.BtEngineDynamicsWorld.PairCache.SetInternalGhostPairCallback(Global.BtEngineFilterCallback);
        }

        #endregion

        #region Config parser

        public static void InitConfig(string filename)
        {
            InitDefaultGlobals();

            if(!string.IsNullOrWhiteSpace(filename) && FileFound(filename))
            {
                var state = new ScriptEngine();
                state.RegisterC("bind", ((Action<int, int, object>) MainEngine.BindKey).Method); // get and set key bindings
                try
                {
                    state.DoFile(filename);
                }
                catch(Exception e)
                {
                    Sys.DebugLog(LUA_LOG_FILENAME, "{0}", e.Message);
                    return;
                }

                state.ParseScreen(Global.ScreenInfo);
                state.ParseRender(Renderer.Settings);
                state.ParseAudio(Global.AudioSettings);
                state.ParseConsole(ConsoleInfo.Instance);
                state.ParseControls(ControlMapper);
                state.ParseSystem(Global.SystemSettings);
            }
            else
            {
                Sys.Warn("Could not find \"{0}\"", filename);
            }
        }

        //public static void SaveConfig();

        #endregion

        #region Core system routines - display and tick

        public static void Display()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            EngineCamera.Apply();
            EngineCamera.RecalcClipPlanes();

            if(Global.ScreenInfo.ShowDebugInfo)
            {
                ShowDebugInfo();
            }

            GL.FrontFace(FrontFaceDirection.Cw);

            Renderer.GenWorldList();
            Renderer.DrawList();

            Gui.SwitchGLMode(true);
            {
                Gui.DrawNotifier();
                if(EngineWorld.Character != null && MainInventoryManager != null)
                {
                    Gui.DrawInventory();
                }
            }

            Gui.Render();
            Gui.SwitchGLMode(false);

            Renderer.DrawListDebugLines();

            // SDL_GL_SwapWindow(sdl_window); TODO
        }

        public static void Frame(float time)
        {
            if(time > 0.1f)
            {
                time = 0.1f;
            }

            EngineFrameTime = time;
            FPSCycle(time);

            Game.Frame(time);
            GameflowManager.Do();
        }

        #endregion

        #region Resize event

        // Nominal values are used e.g. to set the size for the console.
        // pixel values are used for glViewport. Both will be the same on
        // normal displays, but on retina displays or similar, pixels will be twice nominal (or more).
        public static void Resize(int nominalW, int nominalH, int pixelsW, int pixelsH)
        {
            Global.ScreenInfo.W = (short) nominalW;
            Global.ScreenInfo.H = (short) nominalH;

            Global.ScreenInfo.Wunit = nominalW / ScreenMeteringResolution;
            Global.ScreenInfo.Hunit = nominalH / ScreenMeteringResolution;
            Global.ScreenInfo.ScaleFactor = Global.ScreenInfo.W < Global.ScreenInfo.H
                ? Global.ScreenInfo.Hunit
                : Global.ScreenInfo.Wunit;

            Gui.Resize();

            EngineCamera.SetFovAspect(Global.ScreenInfo.Fov, (float)nominalW / nominalH);
            EngineCamera.RecalcClipPlanes();

            GL.Viewport(0, 0, pixelsW, pixelsH);
        }

        #endregion

        #region Debug functions

        public static void PrimaryMouseDown()
        {
            // No implementation in OpenTomb
        }

        public static void SecondaryMouseDown()
        {
            // No implementation in OpenTomb
        }

        public static void ShowDebugInfo()
        {
            var colorArray = new[] {1.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f};

            LightPosition = EngineCamera.Position;

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.LineWidth(2.0f);
            GL.VertexPointer(3, VertexPointerType.Float, 0, CastRay);
            GL.ColorPointer(3, ColorPointerType.Float, 0, colorArray);
            GL.DrawArrays(PrimitiveType.Lines, 0, 2);

            var ent = EngineWorld.Character;
            if(ent != null)
            {
                Gui.OutTextXY(30.0f, 30.0f,
                    "last_anim = {0} ({0:D}), curr_anim = {1} ({1:D}), next_anim = {2} ({2:D}), last_st = {3} ({3:D}), next_st = {4} ({4:D}), speed={5} frame={6}",
                    ent.Bf.Animations.LastAnimation,
                    ent.Bf.Animations.CurrentAnimation,
                    ent.Bf.Animations.NextAnimation,
                    ent.Bf.Animations.LastState,
                    ent.Bf.Animations.NextState,
                    ent.CurrentSpeed,
                    ent.Bf.Animations.CurrentFrame
                    );

                Gui.OutTextXY(20, 8, "pos = {0}", ent.Transform.Origin);
            }

            if(LastContainer != null)
            {
                switch(LastContainer.ObjectType)
                {
                    case OBJECT_TYPE.Entity:
                        Gui.OutTextXY(30.0f, 60.0f, "cont_entity: id = {0}, model = {1}",
                            ((Entity) LastContainer.Object).ID,
                            ((Entity) LastContainer.Object).Bf.Animations.Model.ID);
                        break;

                    case OBJECT_TYPE.StaticMesh:
                        Gui.OutTextXY(30.0f, 60.0f, "cont_static: id = {0}",
                            ((StaticMesh)LastContainer.Object).ObjectID);
                        break;

                    case OBJECT_TYPE.RoomBase:
                        Gui.OutTextXY(30.0f, 60.0f, "cont_room: id = {0}",
                            ((Room)LastContainer.Object).ID);
                        break;
                }
            }

            if(EngineCamera.CurrentRoom != null)
            {
                var rs = EngineCamera.CurrentRoom.GetSectorRaw(EngineCamera.Position);
                if(rs != null)
                {
                    Gui.OutTextXY(30.0f, 90.0f, "room = (id = {0}, sx = {1}, sy = {2})",
                        EngineCamera.CurrentRoom.ID, rs.IndexX, rs.IndexY);
                    Gui.OutTextXY(30.0f, 120.0f, "room_below = {0}, room_above = {1}",
                        rs.SectorBelow == null ? -1 : (long) rs.SectorBelow.OwnerRoom.ID,
                        rs.SectorAbove == null ? -1 : (long) rs.SectorAbove.OwnerRoom.ID);
                }
            }
            Gui.OutTextXY(30.0f, 150.0f, "cam_pos = {0}", EngineCamera.Position);
        }

        public static void DumpRoom(Room r)
        {
            if(r != null)
            {
                Sys.DebugLog("room_dump.txt", "ROOM = {0}, ({1} x {2}), bottom = {3}, top = {4}, pos({5})",
                    r.ID, r.SectorsX, r.SectorsY, r.BBMin.Z, r.BBMax.Z, r.Transform.Origin.ToString("{0}, {1}"));
                Sys.DebugLog("room_dump.txt", "flag = {0} ({0:X}), alt_room = {1}, base_room = {2}",
                    (RoomFlag) r.Flags, r.AlternateRoom == null ? -1 : (long) r.AlternateRoom.ID,
                    r.BaseRoom == null ? -1 : (long) r.BaseRoom.ID);
                foreach (var rs in r.Sectors)
                {
                    Sys.DebugLog("room_dump.txt", "({0}, {1})\tfloor = {2}, ceiling = {3}, portal = {4}", rs.IndexX,
                        rs.IndexY, rs.Floor, rs.Ceiling, rs.PortalToRoom);
                }
                foreach (var sm in r.StaticMesh)
                {
                    Sys.DebugLog("room_dump.txt", "static_mesh = {0}", sm.ObjectID);
                }
                foreach (
                    var ent in
                        from cont in r.Containers
                        where cont.ObjectType == OBJECT_TYPE.Entity
                        select (Entity) cont.Object)
                {
                    Sys.DebugLog("room_dump.txt", "entity: id = {0}, model = {1}", ent.ID,
                        ent.Bf.Animations.Model.ID);
                }
            }
        }

        #endregion

        #region PC-specific level loader routines

        public static bool LoadPCLevel(string name)
        {
            var loader = Level.CreateLoader(name);
            if (loader == null)
                return false;

            loader.Load();

            TR_GenWorld(EngineWorld, loader);

            ConsoleInfo.Instance.Notify(SYSNOTE_LOADED_PC_LEVEL);
            ConsoleInfo.Instance.Notify(SYSNOTE_ENGINE_VERSION, loader.GameVersion, GetLevelName(name));
            ConsoleInfo.Instance.Notify(SYSNOTE_NUM_ROOMS, EngineWorld.Rooms.Count);

            return true;
        }

        #endregion

        #region General level loading routines

        public static bool FileFound(string name, bool write = false)
        {
            return File.Exists(name);
        }

        public static LEVEL_FORMAT GetLevelFormat(string name)
        {
            return LEVEL_FORMAT.PC;
        }

        public static bool LoadMap(string name)
        {
            if(!FileFound(name))
            {
                ConsoleInfo.Instance.Warning(SYSWARN_FILE_NOT_FOUND, name);
                return false;
            }

            Gui.DrawLoadScreen(0);

            EngineCamera.CurrentRoom = null;

            Renderer.HideSkyBox();
            Renderer.ResetWorld();

            GameflowManager.CurrentLevelPath = name; // it is needed for "not in the game" levels or correct saves loading.

            Gui.DrawLoadScreen(50);

            EngineWorld.Empty();
            EngineWorld.Prepare();

            EngineLua.Clean();

            Audio.Init();

            Gui.DrawLoadScreen(100);

            // Here we can place different platform-specific level loading routines.

            switch (GetLevelFormat(name))
            {
                case LEVEL_FORMAT.PC:
                    if (!LoadPCLevel(name)) return false;
                    break;
                case LEVEL_FORMAT.PSX:
                    break;
                case LEVEL_FORMAT.DC:
                    break;
                case LEVEL_FORMAT.OPENTOMB:
                    break;
            }

            EngineWorld.ID = 0;
            EngineWorld.Name = null;
            EngineWorld.Type = 0;

            Game.Prepare();

            EngineLua.Prepare();

            Renderer.SetWorld(EngineWorld);

            Gui.DrawLoadScreen(1000);

            Gui.FadeStart(FaderType.LoadScreen, FaderDir.In);
            Gui.NotifierStop();

            return true;
        }

        #endregion

        #region String getters

        public static string GetLevelName(string path)
        {
            return Path.GetFileNameWithoutExtension(path);
        }

        public static string GetAutoexecName(TRGame gameVersion, string postfix = "")
        {
            var levelName = GetLevelName(GameflowManager.CurrentLevelPath).ToUpper();

            var name = "scripts/autoexec";

            if(gameVersion < TRGame.TR2)
            {
                name += "tr1/";
            }
            else if(gameVersion < TRGame.TR3)
            {
                name += "tr2/";
            }
            else if(gameVersion < TRGame.TR4)
            {
                name += "tr3/";
            }
            else if(gameVersion < TRGame.TR5)
            {
                name += "tr4/";
            }
            else
            {
                name += "tr5/";
            }

            name += levelName;
            name += postfix;
            name += ".lua";
            return name;
        }

        #endregion

        #region Console command parser

        public static bool ExecCmd(string cmd)
        {
            var token = "";
            var sect = new RoomSector();
            cmd = cmd.ToLower();

            var ch = 0;
            for(var i = 0; i < cmd.Length; i++)
            {
                ch = MainEngine.ParseToken(cmd, ch, out token);
                token = token.ToLower();
                
                switch (token)
                {
                    case "help":
                        for(var j = SYSNOTE_COMMAND_HELP1; j <= SYSNOTE_COMMAND_HELP15; j++)
                        {
                            ConsoleInfo.Instance.Notify(j);
                        }
                        break;
                    case "goto":
                    {
                        ControlStates.FreeLook = true;
                        var x = MainEngine.ParseFloat(cmd, ch);
                        var y = MainEngine.ParseFloat(cmd, ch);
                        var z = MainEngine.ParseFloat(cmd, ch);
                        Renderer.Camera.Position = new Vector3(x, y, z);
                        return true;
                    }
                    case "save":
                        ch = MainEngine.ParseToken(cmd, ch, out token);
                        if(ch != 0)
                        {
                            Game.Save(token);
                        }
                        return true;
                    case "load":
                        ch = MainEngine.ParseToken(cmd, ch, out token);
                        if (ch != 0)
                        {
                            Game.Load(token);
                        }
                        return true;
                    case "exit":
                        Shutdown(0);
                        return true;
                    case "cls":
                        ConsoleInfo.Instance.Clean();
                        return true;
                    case "spacing":
                        ch = MainEngine.ParseToken(cmd, ch, out token);
                        if(ch == 0)
                        {
                            ConsoleInfo.Instance.Notify(SYSNOTE_CONSOLE_SPACING, ConsoleInfo.Instance.Spacing);
                            return true;
                        }
                        ConsoleInfo.Instance.SetLineInterval(float.Parse(token));
                        return true;
                    case "showing_lines":
                        ch = MainEngine.ParseToken(cmd, ch, out token);
                        if (ch == 0)
                        {
                            ConsoleInfo.Instance.Notify(SYSNOTE_CONSOLE_LINECOUNT, ConsoleInfo.Instance.VisibleLines);
                            return true;
                        }
                        else
                        {
                            var val = int.Parse(token);
                            if(val >= 2 && val <= Global.ScreenInfo.H / ConsoleInfo.Instance.LineHeight)
                            {
                                ConsoleInfo.Instance.VisibleLines = val;
                                ConsoleInfo.Instance.CursorY = (short)(Global.ScreenInfo.H -
                                                                       ConsoleInfo.Instance.LineHeight *
                                                                       ConsoleInfo.Instance.VisibleLines);
                            }
                            else
                            {
                                ConsoleInfo.Instance.Warning(SYSWARN_INVALID_LINECOUNT);
                            }
                        }
                        return true;
                    case "r_wireframe":
                        Renderer.ToggleWireframe();
                        return true;
                    case "r_points":
                        Renderer.ToggleDrawPoints();
                        return true;
                    case "r_coll":
                        Renderer.ToggleDrawColl();
                        return true;
                    case "r_normals":
                        Renderer.ToggleDrawNormals();
                        return true;
                    case "r_portals":
                        Renderer.ToggleDrawPortals();
                        return true;
                    case "r_frustums":
                        Renderer.ToggleDrawFrustums();
                        return true;
                    case "r_room_boxes":
                        Renderer.ToggleDrawRoomBoxes();
                        return true;
                    case "r_boxes":
                        Renderer.ToggleDrawBoxes();
                        return true;
                    case "r_axis":
                        Renderer.ToggleDrawAxis();
                        return true;
                    case "r_allmodels":
                        Renderer.ToggleDrawAllModels();
                        return true;
                    case "r_dummy_statics":
                        Renderer.ToggleDrawDummyStatics();
                        return true;
                    case "r_skip_room":
                        Renderer.ToggleSkipRoom();
                        return true;
                    case "room_info":
                        var r = Renderer.Camera.CurrentRoom;
                        if (r != null)
                        {
                            sect = r.GetSectorXYZ(Renderer.Camera.Position);
                            ConsoleInfo.Instance.Printf("ID = {0}, x_sect = {1}, y_sect = {2}", r.ID, r.SectorsX, r.SectorsY);
                            if(sect != null)
                            {
                                ConsoleInfo.Instance.Printf("sect({0}, {1}), inpenetrable = {2}, r_up = {3}, r_down = {4}",
                                    sect.IndexX, sect.IndexY,
                                    TR_METERING_WALLHEIGHT.IsAnyOf(sect.Ceiling, sect.Floor),
                                    sect.SectorAbove != null, sect.SectorBelow != null);
                                for (var j = 0; j < sect.OwnerRoom.StaticMesh.Count; j++)
                                {
                                    ConsoleInfo.Instance.Printf("static[{0}].object_id = {1}", j, sect.OwnerRoom.StaticMesh[j].ObjectID);
                                }
                                foreach (
                                    var e in
                                        sect.OwnerRoom.Containers.Where(x => x.ObjectType == OBJECT_TYPE.Entity)
                                            .Select(cont => (Entity) cont.Object))
                                {
                                    ConsoleInfo.Instance.Printf("cont[entity]{0}.object_id = {1}", e.Transform.Origin, e.ID);
                                }
                            }
                        }
                        return true;
                    default:
                        if(token[0] != '\0')
                        {
                            ConsoleInfo.Instance.AddLine(cmd, FontStyle.ConsoleEvent);
                            try
                            {
                                EngineLua.DoString(cmd);
                            }
                            catch(Exception e)
                            {
                                ConsoleInfo.Instance.AddLine(e.Message, FontStyle.ConsoleWarning);
                            }
                            
                        }
                        return false;
                }
            }

            return false;
        }

        #endregion

        #region Bullet global methods

        public static void RoomNearCallback(BroadphasePair collisionPair, CollisionDispatcher dispatcher,
            DispatcherInfo dispatchInfo)
        {
            var c0 = (EngineContainer) ((CollisionObject) collisionPair.Proxy0.ClientObject).UserObject;
            var r0 = c0?.Room;
            var c1 = (EngineContainer) ((CollisionObject) collisionPair.Proxy1.ClientObject).UserObject;
            var r1 = c1?.Room;

            if (c1 != null && c1 == c0)
            {
                if (((CollisionObject) collisionPair.Proxy0.ClientObject).IsStaticOrKinematicObject
                    || ((CollisionObject) collisionPair.Proxy1.ClientObject).IsStaticOrKinematicObject)
                {
                    return; // No self interaction
                }
                dispatcher.NearCallback(collisionPair, dispatcher, dispatchInfo);
                return;
            }

            if(r0 == null && r1 == null)
            {
                dispatcher.NearCallback(collisionPair, dispatcher, dispatchInfo); // Both are out of rooms
                return;
            }

            if(r0 != null && r1 != null)
            {
                if(r0.IsInNearRoomsList(r1))
                {
                    dispatcher.NearCallback(collisionPair, dispatcher, dispatchInfo);
                }
            }
        }

        public static void InternalTickCallback(DynamicsWorld world, float timeStep)
        {
            for(var i = world.NumCollisionObjects - 1; i >= 0; i--)
            {
                Assert.That(i >= 0 && i < BtEngineDynamicsWorld.CollisionObjectArray.Count);
                var obj = BtEngineDynamicsWorld.CollisionObjectArray[i];
                var body = RigidBody.Upcast(obj);
                if(body != null && !body.IsStaticObject && body.MotionState != null)
                {
                    var trans = (Transform)body.MotionState.WorldTransform;
                    var cont = (EngineContainer) body.UserObject;
                    if(cont != null && cont.ObjectType == OBJECT_TYPE.BulletMisc)
                    {
                        cont.Room = Room.FindPosCogerrence(trans.Origin, cont.Room);
                    }
                }
            }
        }

        #endregion
    }
}

