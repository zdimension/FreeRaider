using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BulletSharp;
using FreeRaider.Script;
using NLua;
using NLua.Exceptions;
using OpenTK;

namespace FreeRaider
{
    public partial class Constants
    {
        /// <summary>
        /// Original (canonical) TR frame rate.
        /// Needed for animation speed calculations.
        /// </summary>
        public const float TR_FRAME_RATE = 30.0f;

        /// <summary>
        /// This is the global game logic refresh interval.
        /// All game logic should be refreshed at this rate, including enemy AI, values processing and audio update.
        /// </summary>
        public const float GAME_LOGIC_REFRESH_INTERVAL = 1.0f / 60.0f;
    }

    public partial class Global
    {
        public static Vector3 CamAngles;
    }

    public class Game
    {
        public static void InitGlobals()
        {
            Global.ControlStates.FreeLookSpeed = 3000.0f;
            Global.ControlStates.MouseLook = true;
            Global.ControlStates.FreeLook = false;
            Global.ControlStates.NoClip = false;
            Global.ControlStates.CamDistance = 800.0f;
        }

        public static void RegisterLUAFunctions(ScriptEngine state)
        {
            state.RegisterFunction("debuginfo", typeof(luaFuncs).GetMethod("lua_debuginfo"));
            state.RegisterFunction("mlook", typeof(luaFuncs).GetMethod("lua_mlook"));
            state.RegisterFunction("freelook", typeof(luaFuncs).GetMethod("lua_freelook"));
            state.RegisterFunction("noclip", typeof(luaFuncs).GetMethod("lua_noclip"));
            state.RegisterFunction("cam_distance", typeof(luaFuncs).GetMethod("lua_cam_distance"));
            state.RegisterFunction("timescale", typeof(luaFuncs).GetMethod("lua_timescale"));
        }

        public static bool Load(string name)
        {
            if (name.Contains("\\") || name.Contains("/")) name = "save/" + Path.GetFileName(name);

            try
            {
                Global.EngineLua.ClearTasks();
                Global.EngineLua.DoFile(name);
            }
            catch(LuaException ex)
            {
                Sys.DebugLog(Constants.LUA_LOG_FILENAME, ex.Message);
            }
            catch(Exception ex)
            {
                Sys.extWarn("Cannot read file \"{0}\": {1}", name, ex.Message);
                return false;
            }

            return true;
        }

        private static void Save_EntityTree(TextWriter f, Dictionary<uint, Entity> map)
        {
            foreach (var kvp in map)
            {
                Save_Entity(f, kvp.Value);
            }
        }

        /// <summary>
        /// Entity save function, based on engine lua scripts
        /// </summary>
        private static void Save_Entity(TextWriter f, Entity ent)
        {
            if (ent == null)
            {
                return;
            }

            if (ent.TypeFlags.HasFlag(ENTITY_TYPE.Spawned))
            {
                f.WriteLine("spawnEntity({0}, {1}, {2}, {3}, {4});", ent.Bf.Animations.Model.ID,
                    ent.Transform.Origin.ToString("{0:0.00}, {1:0.00}, {2:0.00}"),
                    ent.Angles.ToString("{0:0.00}, {1:0.00}, {2:0.00}"), ent.Self.Room?.ID ?? 0xFFFFFFFF, ent.ID);
            }
            else
            {
                f.WriteLine("setEntityPos({0}, {1}, {2});", ent.ID,
                    ent.Transform.Origin.ToString("{0:0.00}, {1:0.00}, {2:0.00}"),
                    ent.Angles.ToString("{0:0.00}, {1:0.00}, {2:0.00}"));
            }

            f.WriteLine("setEntitySpeed({0}, {1});", ent.ID, ent.Speed.ToString("{0:0.00}, {1:0.00}, {2:0.00}"));
            f.WriteLine("setEntityAnim({0}, {1:D}, {2});", ent.ID, ent.Bf.Animations.CurrentAnimation,
                ent.Bf.Animations.CurrentFrame);
            f.WriteLine("setEntityState({0}, {1:D}, {2:D});", ent.ID, ent.Bf.Animations.NextState,
                ent.Bf.Animations.LastState);
            f.WriteLine("setEntityCollisionFlags({0}, {1}, {2});", ent.ID, (long) ent.Self.CollisionType,
                (long) ent.Self.CollisionShape);

            f.WriteLine(ent.Enabled ? "enableEntity({0});" : "disableEntity({0});", ent.ID);

            f.WriteLine("setEntityFlags({0}, {1}, {2}, {3}, 0x{4:X}, 0x{5:X});", ent.ID, ent.Active.Lcase(),
                ent.Enabled.Lcase(), ent.Visible.Lcase(), ent.TypeFlags, ent.CallbackFlags);

            f.WriteLine("setEntityTriggerLayout({0}, 0x{1:X});", ent.ID, ent.TriggerLayout);

            f.WriteLine("setEntityRoomMove({0}, {1}, {2:D}, {3:D});", ent.ID, ent.Self.Room?.ID.ToString() ?? "nil",
                ent.MoveType, ent.DirFlag);

            if(ent is Character)
            {
                var ch = (Character) ent;
                f.WriteLine("removeAllItems({0});", ent.ID);
                foreach (var i in ch.Inventory)
                {
                    f.WriteLine("addItem({0}, {1}, {2});", ent.ID, i.ID, i.Count);
                }

                for(var i = 0; i < (int)CharParameters.Sentinel; i++)
                {
                    f.WriteLine("setCharacterParam({0}, {1}, {2}, {3});", ent.ID, i, ch.Parameters.Param[i], ch.Parameters.Maximum[i]);
                }
            }
        }

        public static bool Save(string name)
        {
            if (name.Contains("\\") || name.Contains("/")) name = "save/" + Path.GetFileName(name);

            try
            {
                using (var f = File.CreateText(name))
                {
                    f.WriteLine("loadMap(\"{0}\", {1}, {2});", Global.GameflowManager.CurrentLevelPath,
                        Global.GameflowManager.GameID, Global.GameflowManager.LevelID);

                    // Save flipmap and flipped room states.

                    for (var i = 0; i < Global.EngineWorld.FlipData.Count; i++)
                    {
                        f.WriteLine("setFlipMap({0}, 0x{1:X}, 0);", i, Global.EngineWorld.FlipData[i].Map);
                        f.WriteLine("setFlipState({0}, {1});", i, (Global.EngineWorld.FlipData[i].State != 0).Lcase());
                    }

                    Save_Entity(f, Global.EngineWorld.Character); // Save Lara.

                    Save_EntityTree(f, Global.EngineWorld.EntityTree);
                }

                return true;
            }
            catch (Exception ex)
            {
                Sys.extWarn("Cannot create file \"{0}\": {1}", name, ex.Message);
                return false;
            }
        }

        public static float Tick(ref float gameLogicTime)
        {
            var t = (int) (gameLogicTime / Constants.GAME_LOGIC_REFRESH_INTERVAL);
            var dt = t * Constants.GAME_LOGIC_REFRESH_INTERVAL;
            gameLogicTime -= dt;
            return dt;
        }

        public static void Frame(float time)
        {
            var gameLogicTime = 0.0f;
            gameLogicTime += time;

            var isCharacter = Global.EngineWorld.Character != null;

            // GUI and controls should be updated at all times!

            Controls.PollSDLInput();
            Gui.Update();

            // FIXME: I have no idea what's happening here! - Lwmte
            
            if(!ConsoleInfo.Instance.IsVisible && Global.ControlStates.GuiInventory && Global.MainInventoryManager != null)
            {
                if(isCharacter && Global.MainInventoryManager.CurrentState == InventoryManager.InventoryState.Disabled)
                {
                    Global.MainInventoryManager.SetInventory(Global.EngineWorld.Character.Inventory);
                    Global.MainInventoryManager.NextState = InventoryManager.InventoryState.Open;
                }
                if(Global.MainInventoryManager.CurrentState == InventoryManager.InventoryState.Idle)
                {
                    Global.MainInventoryManager.NextState = InventoryManager.InventoryState.Closed;
                }
            }

            // If console or inventory is active, only thing to update is audio.
            if(ConsoleInfo.Instance.IsVisible || Global.MainInventoryManager.CurrentState != InventoryManager.InventoryState.Disabled)
            {
                if(gameLogicTime >= Constants.GAME_LOGIC_REFRESH_INTERVAL)
                {
                    Audio.Update();
                    Tick(ref gameLogicTime);
                }
                return;
            }

            // We're going to update main logic with a fixed step.
            // This allows to conserve CPU resources and keep everything in sync!

            if(gameLogicTime >= Constants.GAME_LOGIC_REFRESH_INTERVAL)
            {
                var dt = Tick(ref gameLogicTime);
                Global.EngineLua.DoTasks(dt);
                UpdateAI();
                Audio.Update();

                if(isCharacter)
                {
                    Global.EngineWorld.Character.ProcessSector();
                    Global.EngineWorld.Character.UpdateParams();
                    Global.EngineWorld.Character.CheckCollisionCallback();   // FIXME: Must do it for ALL interactive entities!
                }

                LoopEntities(Global.EngineWorld.EntityTree);
            }

            // This must be called EVERY frame to max out smoothness.
            // Includes animations, camera movement, and so on.

            ApplyControls(Global.EngineWorld.Character);

            if(isCharacter)
            {
                if(Global.EngineWorld.Character.TypeFlags.HasFlag(ENTITY_TYPE.Dynamic))
                {
                    Global.EngineWorld.Character.UpdateRigidBody(false);
                }
                if(!Global.ControlStates.NoClip && !Global.ControlStates.FreeLook)
                {
                    Global.EngineWorld.Character.Frame(Global.EngineFrameTime);
                    Global.EngineWorld.Character.ApplyCommands();
                    Global.EngineWorld.Character.Frame(0.0f);
                    StaticFuncs.Cam_FollowEntity(Global.Renderer.Camera, Global.EngineWorld.Character, 16.0f, 128.0f);
                }
            }

            UpdateCharacters();

            UpdateAllEntities(Global.EngineWorld.EntityTree);

            Global.BtEngineDynamicsWorld.StepSimulation(time / 2.0f, 0);
            Global.BtEngineDynamicsWorld.StepSimulation(time / 2.0f, 0);

            Controls.RefreshStates();
            Global.EngineWorld.UpdateAnimTextures();
        }

        public static void Prepare();

        public static void LevelTransition(ushort levelIndex);

        public static void ApplyControls(Entity ent);

        public static void UpdateAllEntities(Dictionary<uint, Entity> entities)
        {
            foreach (var entity in entities.Values)
            {
                if(entity.TypeFlags.HasFlag(ENTITY_TYPE.Dynamic))
                {
                    entity.UpdateRigidBody(false);
                }
                else if (entity.Frame(Global.EngineFrameTime) != ENTITY_ANIM.None)
                {
                    entity.UpdateRigidBody(false);
                }
            }
        }

        public static void LoopEntities(Dictionary<uint, Entity> entities)
        {
            foreach (var entity in entities.Values.Where(entity => entity.Enabled))
            {
                entity.ProcessSector();
                Global.EngineLua.LoopEntity((int) entity.ID);

                if(entity.TypeFlags.HasFlag(ENTITY_TYPE.CollCheck))
                    entity.CheckCollisionCallback();
            }
        }

        public static void UpdateAI()
        {
            //for(ALL CHARACTERS, EXCEPT PLAYER)
            {
                // UPDATE AI commands
            }
        }

        public static void UpdateCharacters()
        {
            var ent = Global.EngineWorld.Character;

            if(ent != null)
            {
                if (ent.Command.Action && ent.TypeFlags.HasFlag(ENTITY_TYPE.TriggerActivator))
                {
                    ent.CheckActivators();
                }
                if (ent.GetParam(CharParameters.Health) <= 0.0f)
                {
                    ent.Response.Killed = true; // Kill, if no HP.
                }
                ent.UpdateHair();
            }

            UpdateCharactersTree(Global.EngineWorld.EntityTree);
        }

        private static void UpdateCharactersTree(Dictionary<uint, Entity> entities)
        {
            foreach (Character ent in entities.Values.Where(ent => ent != null))
            {
                if(ent.Command.Action && ent.TypeFlags.HasFlag(ENTITY_TYPE.TriggerActivator))
                {
                    ent.CheckActivators();
                }
                if(ent.GetParam(CharParameters.Health) <= 0.0f)
                {
                    ent.Response.Killed = true; // Kill, if no HP.
                }
                ent.ApplyCommands();
                ent.UpdateHair();
            }
        }
    }

    public partial class StaticFuncs
    {
        public static bool Cam_HasHit(BtEngineClosestConvexResultCallback cb, Transform cameraFrom, Transform cameraTo)
        {
            var cameraSphere = new SphereShape(Constants.COLLISION_CAMERA_SPHERE_RADIUS);
            cameraSphere.Margin = Constants.COLLISION_MARGIN_DEFAULT;
            cb.ClosestHitFraction = 1.0f;
            cb.HitCollisionObject = null;
            Global.BtEngineDynamicsWorld.ConvexSweepTest(cameraSphere, (Matrix4)cameraFrom, (Matrix4)cameraTo, cb);
            return cb.HasHit;
        }

        public static void Cam_FollowEntity(Camera cam, Entity ent, float dx, float dz);
    }

    public class luaFuncs
    {
        public static void lua_mlook(object mlook)
        {
            if (mlook is bool)
            {
                Global.ControlStates.MouseLook = (bool) mlook;
            }
            else
            {
                Global.ControlStates.MouseLook = !Global.ControlStates.MouseLook;
            }

            ConsoleInfo.Instance.Printf("mlook = {0}", Global.ControlStates.MouseLook);
        }

        public static void lua_freelook(object free)
        {
            if (free is bool)
            {
                Global.ControlStates.FreeLook = (bool)free;
            }
            else
            {
                Global.ControlStates.FreeLook = !Global.ControlStates.FreeLook;
            }

            ConsoleInfo.Instance.Printf("free_look = {0}", Global.ControlStates.FreeLook);
        }

        public static void lua_cam_distance(object mlook)
        {
            if (mlook is float)
            {
                Global.ControlStates.CamDistance = (float)mlook;
            }

            ConsoleInfo.Instance.Printf("cam_distance = {0}", Global.ControlStates.CamDistance);
        }

        public static void lua_noclip(object noclip)
        {
            if (noclip is bool)
            {
                Global.ControlStates.NoClip = (bool)noclip;
            }
            else
            {
                Global.ControlStates.NoClip = !Global.ControlStates.NoClip;
            }

            ConsoleInfo.Instance.Printf("noclip = {0}", Global.ControlStates.NoClip);
        }

        public static void lua_debuginfo(object show)
        {
            if (show is bool)
            {
                Global.ScreenInfo.ShowDebugInfo = (bool)show;
            }
            else
            {
                Global.ScreenInfo.ShowDebugInfo = !Global.ScreenInfo.ShowDebugInfo;
            }

            ConsoleInfo.Instance.Printf("debug_info = {0}", Global.ScreenInfo.ShowDebugInfo);
        }

        public static void lua_timescale(object scale)
        {
            if (scale is float)
            {
                Program.TimeScale = (float) scale;
            }
            else
            {
                Program.TimeScale = Program.TimeScale == 1.0f ? 0.033f : 1.0f;
            }

            ConsoleInfo.Instance.Printf("time_scale = {0}", Math.Round(Program.TimeScale, 3));
        }
    }
}
