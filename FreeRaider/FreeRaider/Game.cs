using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BulletSharp;
using FreeRaider.Script;
using NLua;
using NLua.Exceptions;
using OpenTK;
using static FreeRaider.Constants;
using static FreeRaider.Global;
using static FreeRaider.StaticFuncs;

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
            ControlStates.FreeLookSpeed = 3000.0f;
            ControlStates.MouseLook = true;
            ControlStates.FreeLook = false;
            ControlStates.NoClip = false;
            ControlStates.CamDistance = 800.0f;
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
                EngineLua.ClearTasks();
                EngineLua.DoFile(name);
            }
            catch(LuaException ex)
            {
                Sys.DebugLog(LUA_LOG_FILENAME, ex.Message);
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
                    ent.Transform.Origin.ToStringEx("{0}, {1}, {2}", 2),
                    ent.Angles.ToStringEx("{0}, {1}, {2}", 2), ent.Self.Room?.ID ?? 0xFFFFFFFF, ent.ID);
            }
            else
            {
                f.WriteLine("setEntityPos({0}, {1}, {2});", ent.ID,
                    ent.Transform.Origin.ToStringEx("{0}, {1}, {2}", 2),
                    ent.Angles.ToStringEx("{0}, {1}, {2}", 2));
            }

            f.WriteLine("setEntitySpeed({0}, {1});", ent.ID, ent.Speed.ToStringEx("{0}, {1}, {2}", 2));
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
                    f.WriteLine("loadMap(\"{0}\", {1}, {2});", GameflowManager.CurrentLevelPath,
                        GameflowManager.GameID, GameflowManager.LevelID);

                    // Save flipmap and flipped room states.

                    for (var i = 0; i < EngineWorld.FlipData.Count; i++)
                    {
                        f.WriteLine("setFlipMap({0}, 0x{1:X}, 0);", i, EngineWorld.FlipData[i].Map);
                        f.WriteLine("setFlipState({0}, {1});", i, (EngineWorld.FlipData[i].State != 0).Lcase());
                    }

                    Save_Entity(f, EngineWorld.Character); // Save Lara.

                    Save_EntityTree(f, EngineWorld.EntityTree);
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
            var t = (int) (gameLogicTime / GAME_LOGIC_REFRESH_INTERVAL);
            var dt = t * GAME_LOGIC_REFRESH_INTERVAL;
            gameLogicTime -= dt;
            return dt;
        }

        public static void Frame(float time)
        {
            var gameLogicTime = 0.0f;
            gameLogicTime += time;

            var isCharacter = EngineWorld.Character != null;

            // GUI and controls should be updated at all times!

            Controls.PollSDLInput();
            Gui.Update();

            // FIXME: I have no idea what's happening here! - Lwmte
            
            if(!ConsoleInfo.Instance.IsVisible && ControlStates.GuiInventory && MainInventoryManager != null)
            {
                if(isCharacter && MainInventoryManager.CurrentState == InventoryManager.InventoryState.Disabled)
                {
                    MainInventoryManager.SetInventory(EngineWorld.Character.Inventory);
                    MainInventoryManager.NextState = InventoryManager.InventoryState.Open;
                }
                if(MainInventoryManager.CurrentState == InventoryManager.InventoryState.Idle)
                {
                    MainInventoryManager.NextState = InventoryManager.InventoryState.Closed;
                }
            }

            // If console or inventory is active, only thing to update is audio.
            if(ConsoleInfo.Instance.IsVisible || MainInventoryManager.CurrentState != InventoryManager.InventoryState.Disabled)
            {
                if(gameLogicTime >= GAME_LOGIC_REFRESH_INTERVAL)
                {
                    Audio.Update();
                    Tick(ref gameLogicTime);
                }
                return;
            }

            // We're going to update main logic with a fixed step.
            // This allows to conserve CPU resources and keep everything in sync!

            if(gameLogicTime >= GAME_LOGIC_REFRESH_INTERVAL)
            {
                var dt = Tick(ref gameLogicTime);
                EngineLua.DoTasks(dt);
                UpdateAI();
                Audio.Update();

                if(isCharacter)
                {
                    EngineWorld.Character.ProcessSector();
                    EngineWorld.Character.UpdateParams();
                    EngineWorld.Character.CheckCollisionCallbacks();   // FIXME: Must do it for ALL interactive entities!
                }

                LoopEntities(EngineWorld.EntityTree);
            }

            // This must be called EVERY frame to max out smoothness.
            // Includes animations, camera movement, and so on.

            ApplyControls(EngineWorld.Character);

            if(isCharacter)
            {
                if(EngineWorld.Character.TypeFlags.HasFlag(ENTITY_TYPE.Dynamic))
                {
                    EngineWorld.Character.UpdateRigidBody(false);
                }
                if(!ControlStates.NoClip && !ControlStates.FreeLook)
                {
                    EngineWorld.Character.Frame(EngineFrameTime);
                    EngineWorld.Character.ApplyCommands();
                    EngineWorld.Character.Frame(0.0f);
                    Cam_FollowEntity(Renderer.Camera, EngineWorld.Character, 16.0f, 128.0f);
                }
            }

            UpdateCharacters();

            UpdateAllEntities(EngineWorld.EntityTree);

            BtEngineDynamicsWorld.StepSimulation(time / 2.0f, 0);
            BtEngineDynamicsWorld.StepSimulation(time / 2.0f, 0);

            Controls.RefreshStates();
            EngineWorld.UpdateAnimTextures();
        }

        public static void Prepare()
        {
            if(EngineWorld.Character != null)
            {
                // Set character values to default.

                EngineWorld.Character.SetParamMaximum(CharParameters.Health, LARA_PARAM_HEALTH_MAX);
                EngineWorld.Character.SetParam(CharParameters.Health, LARA_PARAM_HEALTH_MAX);
                EngineWorld.Character.SetParamMaximum(CharParameters.Air, LARA_PARAM_AIR_MAX);
                EngineWorld.Character.SetParam(CharParameters.Air, LARA_PARAM_AIR_MAX);
                EngineWorld.Character.SetParamMaximum(CharParameters.Stamina, LARA_PARAM_STAMINA_MAX);
                EngineWorld.Character.SetParam(CharParameters.Stamina, LARA_PARAM_STAMINA_MAX);
                EngineWorld.Character.SetParamMaximum(CharParameters.Warmth, LARA_PARAM_WARMTH_MAX);
                EngineWorld.Character.SetParam(CharParameters.Warmth, LARA_PARAM_WARMTH_MAX);
                EngineWorld.Character.SetParamMaximum(CharParameters.Poison, LARA_PARAM_POISON_MAX);
                EngineWorld.Character.SetParam(CharParameters.Poison, 0);

                // Set character statistics to default.

                EngineWorld.Character.Statistics.Distance = 0.0f;
                EngineWorld.Character.Statistics.AmmoUsed = 0;
                EngineWorld.Character.Statistics.Hits = 0;
                EngineWorld.Character.Statistics.Kills = 0;
                EngineWorld.Character.Statistics.MedipacksUsed = 0;
                EngineWorld.Character.Statistics.SavesUsed = 0;
                EngineWorld.Character.Statistics.SecretsGame = 0;
                EngineWorld.Character.Statistics.SecretsLevel = 0;
            }
            else if(EngineWorld.Rooms.Count > 0)
            {
                // If there is no character present, move default camera position to
                // the first room (useful for TR1-3 cutscene levels).

                EngineCamera.Position = EngineWorld.Rooms[0].BBMax;
            }

            // Set gameflow parameters to default.
            // Reset secret trigger map.

            GameflowManager.SecretsTriggerMap = new bool[GF_MAX_SECRETS + 1];
        }

        public static void LevelTransition(ushort levelIndex)
        {
            var filePath = EngineLua.GetLoadingScreen(levelIndex);
            Gui.FadeAssignPic(FaderType.LoadScreen, filePath);
            Gui.FadeStart(FaderType.LoadScreen, FaderDir.Out);

            Audio.EndStreams();
        }

        public static void ApplyControls(Entity ent)
        {
            // Keyboard move logic

            var moveLogic = new[]
            {
                ControlStates.MoveForward.Sub(ControlStates.MoveBackward),
                ControlStates.MoveRight.Sub(ControlStates.MoveLeft),
                ControlStates.MoveUp.Sub(ControlStates.MoveDown)
            };

            // Keyboard look logic

            var lookLogic = new Vector3
                (
                ControlStates.LookLeft.Sub(ControlStates.LookRight),
                ControlStates.LookDown.Sub(ControlStates.LookUp),
                ControlStates.LookRollRight.Sub(ControlStates.LookRollLeft)
                );

            // APPLY CONTROLS

            CamAngles += 2.2f * EngineFrameTime * lookLogic;

            // FIXME: Duplicate code - do we need cam control with no world??
            if (Renderer.World == null)
            {
                if (ControlMapper.UseJoy)
                {
                    // TODO: Useless check? (if it's not zero, do nothing, if it's zero, multiply it, but if it's zero, it won't substract so check for zero is useless)
                    if (ControlMapper.JoyLookX != 0)
                    {
                        CamAngles.X -= 0.015f * EngineFrameTime * ControlMapper.JoyLookX;
                    }
                    if (ControlMapper.JoyLookX != 0)
                    {
                        CamAngles.X -= 0.015f * EngineFrameTime * ControlMapper.JoyLookX;
                    }
                }

                if (ControlStates.MouseLook)
                {
                    CamAngles.X -= 0.015f * ControlStates.LookAxisX;
                    CamAngles.Y -= 0.015f * ControlStates.LookAxisY;
                    ControlStates.LookAxisX = ControlStates.LookAxisY = 0.0f;
                }

                Renderer.Camera.SetRotation(CamAngles);
                var dist = ControlStates.FreeLookSpeed * EngineFrameTime;
                if (ControlStates.StateWalk) dist *= 0.3f;
                Renderer.Camera.MoveAlong(dist * moveLogic[0]);
                Renderer.Camera.MoveStrafe(dist * moveLogic[1]);
                Renderer.Camera.MoveVertical(dist * moveLogic[2]);

                return;
            }

            if (ControlMapper.UseJoy)
            {
                // TODO: Useless check? (if it's not zero, do nothing, if it's zero, multiply it, but if it's zero, it won't substract so check for zero is useless)
                if (ControlMapper.JoyLookX != 0)
                {
                    CamAngles.X -= EngineFrameTime * ControlMapper.JoyLookX;
                }
                if (ControlMapper.JoyLookX != 0)
                {
                    CamAngles.X -= EngineFrameTime * ControlMapper.JoyLookX;
                }
            }

            if (ControlStates.MouseLook)
            {
                CamAngles.X -= 0.015f * ControlStates.LookAxisX;
                CamAngles.Y -= 0.015f * ControlStates.LookAxisY;
                ControlStates.LookAxisX = ControlStates.LookAxisY = 0.0f;
            }

            if(ControlStates.FreeLook || !(ent is Character))
            {
                var dist = ControlStates.FreeLookSpeed * EngineFrameTime;
                if (ControlStates.StateWalk) dist *= 0.3f;
                Renderer.Camera.SetRotation(CamAngles);
                Renderer.Camera.MoveAlong(dist * moveLogic[0]);
                Renderer.Camera.MoveStrafe(dist * moveLogic[1]);
                Renderer.Camera.MoveVertical(dist * moveLogic[2]);
                Renderer.Camera.CurrentRoom = Room.FindPosCogerrence(Renderer.Camera.Position,
                    Renderer.Camera.CurrentRoom);
            }
            else if(ControlStates.NoClip)
            {
                var dist = ControlStates.FreeLookSpeed * EngineFrameTime;
                if (ControlStates.StateWalk) dist *= 0.3f;
                Renderer.Camera.SetRotation(CamAngles);
                Renderer.Camera.MoveAlong(dist * moveLogic[0]);
                Renderer.Camera.MoveStrafe(dist * moveLogic[1]);
                Renderer.Camera.MoveVertical(dist * moveLogic[2]);
                Renderer.Camera.CurrentRoom = Room.FindPosCogerrence(Renderer.Camera.Position,
                    Renderer.Camera.CurrentRoom);

                ent.Angles.X = CamAngles.X * DegPerRad;
                var pos = Renderer.Camera.Position +
                          Renderer.Camera.ViewDirection * ControlStates.CamDistance;
                pos.Z -= 512.0f;
                ent.Transform.Origin = pos;
                ent.UpdateTransform();
            }
            else
            {
                var ch = (Character) ent;
                // Apply controls to Lara
                ch.Command.Action = ControlStates.StateAction;
                ch.Command.ReadyWeapon = ControlStates.DoDrawWeapon;
                ch.Command.Jump = ControlStates.DoJump;
                ch.Command.Shift = ControlStates.StateWalk;

                ch.Command.Roll = (ControlStates.MoveForward && ControlStates.MoveBackward) ||
                                  ControlStates.DoRoll;

                // New commands only for TR3 and above
                ch.Command.Sprint = ControlStates.StateSprint;
                ch.Command.Crouch = ControlStates.StateCrouch;
                
                if(ControlStates.UseSmallMedipack)
                {
                    if(ch.GetItemsCount((uint)ITEM.SmallMedipack) > 0 && ch.ChangeParam(CharParameters.Health, 250))
                    {
                        ch.SetParam(CharParameters.Poison, 0);
                        ch.RemoveItem((uint) ITEM.SmallMedipack, 1);
                        Audio.Send((uint) TR_AUDIO_SOUND.Medipack);
                    }

                    ControlStates.UseSmallMedipack = !ControlStates.UseSmallMedipack;
                }

                if (ControlStates.UseBigMedipack)
                {
                    if (ch.GetItemsCount((uint)ITEM.LargeMedipack) > 0 && ch.ChangeParam(CharParameters.Health, LARA_PARAM_HEALTH_MAX))
                    {
                        ch.SetParam(CharParameters.Poison, 0);
                        ch.RemoveItem((uint)ITEM.LargeMedipack, 1);
                        Audio.Send((uint)TR_AUDIO_SOUND.Medipack);
                    }

                    ControlStates.UseBigMedipack = !ControlStates.UseBigMedipack;
                }

                // TODO: Useless check for zero?
                if(ControlMapper.UseJoy && ControlMapper.JoyMoveX != 0)
                {
                    ch.Command.Rotation.X = -2 * DegPerRad * EngineFrameTime *
                                            ControlMapper.JoyMoveX;
                }
                else
                {
                    ch.Command.Rotation.X = -2 * DegPerRad * EngineFrameTime * moveLogic[1];
                }

                // TODO: Useless check for zero?
                if (ControlMapper.UseJoy && ControlMapper.JoyMoveY != 0)
                {
                    ch.Command.Rotation.Y = -2 * DegPerRad * EngineFrameTime *
                                            ControlMapper.JoyMoveY;
                }
                else
                {
                    ch.Command.Rotation.Y = -2 * DegPerRad * EngineFrameTime * moveLogic[0];
                }

                ch.Command.Move = moveLogic;
            }
        }

        public static void UpdateAllEntities(Dictionary<uint, Entity> entities)
        {
            foreach (var entity in entities.Values)
            {
                if(entity.TypeFlags.HasFlag(ENTITY_TYPE.Dynamic))
                {
                    entity.UpdateRigidBody(false);
                }
                else if (entity.Frame(EngineFrameTime) != ENTITY_ANIM.None)
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
                EngineLua.LoopEntity((int) entity.ID);

                if(entity.TypeFlags.HasFlag(ENTITY_TYPE.CollCheck))
                    entity.CheckCollisionCallbacks();
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
            var ent = EngineWorld.Character;

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

            UpdateCharactersTree(EngineWorld.EntityTree);
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
            var cameraSphere = new SphereShape(COLLISION_CAMERA_SPHERE_RADIUS)
            {
                Margin = COLLISION_MARGIN_DEFAULT
            };
            cb.ClosestHitFraction = 1.0f;
            cb.HitCollisionObject = null;
            BtEngineDynamicsWorld.ConvexSweepTest(cameraSphere, (Matrix4)cameraFrom, (Matrix4)cameraTo, cb);
            return cb.HasHit;
        }

        public static void Cam_FollowEntity(Camera cam, Entity ent, float dx, float dz)
        {
            var cameraFrom = new Transform();
            var cameraTo = new Transform();

            // Reset to initial
            cameraFrom.SetIdentity();
            cameraTo.SetIdentity();

            var cb = ent.CallbackForCamera();

            var camPos = cam.Position;

            // Basic camera override, completely placeholder until a system classic-like is created
            if (!ControlStates.MouseLook) // If mouse look is off
            {
                var currentAngle = CamAngles.X * RadPerDeg; // Current is the current cam angle
                var targetAngle = ent.Angles.X * RadPerDeg;
                    // Target is the target angle which is the entity's angle itself
                var rotSpeed = 2.0f; // Speed of rotation

                // TODO FIX
                // If Lara is in a specific state we want to rotate -75 deg or +75 deg depending on camera collision
                if(ent.Bf.Animations.LastState == TR_STATE.LaraReach)
                {
                    if(cam.TargetDir == TR_CAM_TARG.Back)
                    {
                        var camPos2 = camPos;
                        cameraFrom.Origin = camPos2;
                        camPos2.X += (float) (Math.Sin((ent.Angles.X - 90.0f) * RadPerDeg) *
                                              ControlStates.CamDistance);
                        camPos2.Y -= (float) (Math.Cos((ent.Angles.X - 90.0f) * RadPerDeg) *
                                              ControlStates.CamDistance);
                        cameraTo.Origin = camPos2;

                        // If collided we want to go right otherwise stay left
                        if(Cam_HasHit(cb, cameraFrom, cameraTo))
                        {
                            camPos2 = camPos;
                            cameraFrom.Origin = camPos2;
                            camPos2.X += (float)(Math.Sin((ent.Angles.X + 90.0f) * RadPerDeg) *
                                             ControlStates.CamDistance);
                            camPos2.Y -= (float)(Math.Cos((ent.Angles.X + 90.0f) * RadPerDeg) *
                                                  ControlStates.CamDistance);
                            cameraTo.Origin = camPos2;

                            // If collided we want to go to back else right
                            cam.TargetDir = Cam_HasHit(cb, cameraFrom, cameraTo) ? TR_CAM_TARG.Back : TR_CAM_TARG.Right;
                        }
                        else
                        {
                            cam.TargetDir = TR_CAM_TARG.Left;
                        }
                    }
                }
                else if(ent.Bf.Animations.LastState == TR_STATE.LaraJumpBack)
                {
                    cam.TargetDir = TR_CAM_TARG.Front;
                }
                // ReSharper disable once RedundantCheckBeforeAssignment
                else if(cam.TargetDir != TR_CAM_TARG.Back)
                {
                    cam.TargetDir = TR_CAM_TARG.Back; // Reset to back
                }

                // If target mis-matches current we need to update the camera's angle to reach target!
                if (currentAngle != targetAngle)
                {
                    switch (cam.TargetDir)
                    {
                        case TR_CAM_TARG.Back:
                            targetAngle = ent.Angles.X * RadPerDeg;
                            break;
                        case TR_CAM_TARG.Front:
                            targetAngle = (ent.Angles.X - 180.0f) * RadPerDeg;
                            break;
                        case TR_CAM_TARG.Left:
                            targetAngle = (ent.Angles.X - 75.0f) * RadPerDeg;
                            break;
                        case TR_CAM_TARG.Right:
                            targetAngle = (ent.Angles.X + 75.0f) * RadPerDeg;
                            break;
                        default:
                            targetAngle = ent.Angles.X * RadPerDeg; // Same as TR_CAM_TARG_BACK (default pos)
                            break;
                    }

                    var dAngle = CamAngles.X - targetAngle;
                    if (dAngle > Rad90)
                    {
                        dAngle -= 1 * RadPerDeg;
                    }
                    else
                    {
                        dAngle += 1 * RadPerDeg;
                    }
                    CamAngles.X =
                        (CamAngles.X +
                         Helper.Atan2((float) Math.Sin(currentAngle - dAngle), (float) Math.Cos(currentAngle + dAngle)) *
                         EngineFrameTime * rotSpeed) % Rad360; // Update camera's angle
                }
            }

            camPos = ent.CamPosForFollowing(dz);

            // Code to manage screen shaking effects
            if(Renderer.Camera.ShakeTime > 0.0f && Renderer.Camera.ShakeValue > 0.0f)
            {
                camPos = camPos.AddF((Helper.CPPRand() % Math.Abs(Renderer.Camera.ShakeValue) -
                                     Renderer.Camera.ShakeValue / 2.0f) * Renderer.Camera.ShakeTime);
                Renderer.Camera.ShakeTime = Renderer.Camera.ShakeTime < 0.0f
                    ? 0.0f
                    : Renderer.Camera.ShakeTime - EngineFrameTime;
            }

            cameraFrom.Origin = camPos;
            camPos.Z += dz;
            cameraTo.Origin = camPos;
            if(Cam_HasHit(cb, cameraFrom, cameraTo))
            {
                Helper.SetInterpolate3(out camPos, cameraFrom.Origin, cameraTo.Origin, cb.ClosestHitFraction);
                camPos += cb.HitNormalWorld * 2.0f;
            }

            if(dx != 0.0f)
            {
                cameraFrom.Origin = camPos;
                camPos += dx * cam.RightDirection;
                cameraTo.Origin = camPos;
                if (Cam_HasHit(cb, cameraFrom, cameraTo))
                {
                    Helper.SetInterpolate3(out camPos, cameraFrom.Origin, cameraTo.Origin, cb.ClosestHitFraction);
                    camPos += cb.HitNormalWorld * 2.0f;
                }

                cameraFrom.Origin = camPos;

                var cosAy = Math.Cos(CamAngles.Y);
                var camDx = Math.Sin(CamAngles.X) * cosAy;
                var camDy = -Math.Cos(CamAngles.X) * cosAy;
                var camDz = -Math.Sin(CamAngles.Y);
                camPos.X += (float) (camDx * ControlStates.CamDistance);
                camPos.Y += (float) (camDy * ControlStates.CamDistance);
                camPos.Z += (float) (camDz * ControlStates.CamDistance);

                cameraTo.Origin = camPos;
                if (Cam_HasHit(cb, cameraFrom, cameraTo))
                {
                    Helper.SetInterpolate3(out camPos, cameraFrom.Origin, cameraTo.Origin, cb.ClosestHitFraction);
                    camPos += cb.HitNormalWorld * 2.0f;
                }
            }

            // Update cam pos
            cam.Position = camPos;

            // Modify cam pos for quicksand rooms
            cam.CurrentRoom = Room.FindPosCogerrence(cam.Position - new Vector3(0, 0, 128), cam.CurrentRoom);
            if(cam.CurrentRoom != null && cam.CurrentRoom.Flags.HasFlagUns(RoomFlag.Quicksand))
            {
                var pos = cam.Position;
                pos.Z = cam.CurrentRoom.BBMax.Z + 2.0f * 64.0f;
                cam.Position = pos;
            }

            cam.SetRotation(CamAngles);
            cam.CurrentRoom = Room.FindPosCogerrence(cam.Position, cam.CurrentRoom);
        }
    }

    public partial class luaFuncs
    {
        public static void lua_mlook(bool? mlook = null)
        {
            ControlStates.MouseLook = mlook ?? !ControlStates.MouseLook;

            ConsoleInfo.Instance.Printf("mlook = {0}", ControlStates.MouseLook);
        }

        public static void lua_freelook(bool? free = null)
        {
            ControlStates.FreeLook = free ?? !ControlStates.FreeLook;

            ConsoleInfo.Instance.Printf("free_look = {0}", ControlStates.FreeLook);
        }

        public static void lua_cam_distance(float? camDistance = null)
        {
            if (camDistance != null)
            {
                ControlStates.CamDistance = (float)camDistance;
            }

            ConsoleInfo.Instance.Printf("cam_distance = {0}", ControlStates.CamDistance);
        }

        public static void lua_noclip(bool? noclip = null)
        {
            ControlStates.NoClip = noclip ?? !ControlStates.NoClip;

            ConsoleInfo.Instance.Printf("noclip = {0}", ControlStates.NoClip);
        }

        public static void lua_debuginfo(bool? show = null)
        {
            Global.ScreenInfo.ShowDebugInfo = show ?? !Global.ScreenInfo.ShowDebugInfo;

            ConsoleInfo.Instance.Printf("debug_info = {0}", Global.ScreenInfo.ShowDebugInfo);
        }

        public static void lua_timescale(float? scale = null)
        {
            Program.TimeScale = scale ?? (Program.TimeScale == 1.0f ? 0.033f : 1.0f);

            ConsoleInfo.Instance.Printf("time_scale = {0.000}", Program.TimeScale);
        }
    }
}
