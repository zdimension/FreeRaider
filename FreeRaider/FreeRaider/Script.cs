using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using KeraLua;
using NLua;
using OpenTK;
using Lua = NLua.Lua;
using LuaCore = KeraLua.Lua;
using static FreeRaider.Global;
using static FreeRaider.Constants;

namespace FreeRaider
{
    public partial class Constants
    {
        public const int CVAR_NAME_SIZE = 32;

        public const string CVAR_LUA_TABLE_NAME = "cvars";

        public const int LUA_TNONE = -1;
        public const int LUA_TNIL = 0;
        public const int LUA_TBOOLEAN = 1;
        public const int LUA_TLIGHTUSERDATA = 2;
        public const int LUA_TNUMBER = 3;
        public const int LUA_TSTRING = 4;
        public const int LUA_TTABLE = 5;
        public const int LUA_TFUNCTION = 6;
        public const int LUA_TUSERDATA = 7;
        public const int LUA_TTHREAD = 8;
    }

    public partial class Global
    {
        public static Script.MainEngine EngineLua;
    }

    public partial class luaFuncs
    {
        public static void lua_DumpModel(uint id)
        {
            var sm = EngineWorld.GetModelByID(id);
            if (sm == null)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_WRONG_MODEL_ID, id);
                return;
            }

            for (var i = 0; i < sm.MeshCount; i++)
            {
                ConsoleInfo.Instance.Printf("mesh[{0}] = {1}", i, sm.MeshTree[i].MeshBase.ID);
            }
        }

        public static void lua_DumpRoom(int? id = null)
        {
            if (id == null)
            {
                Engine.DumpRoom(EngineCamera.CurrentRoom);
                return;
            }
            if (id >= EngineWorld.Rooms.Count)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_WRONG_ROOM, (int) id);
                return;
            }
            Engine.DumpRoom(EngineWorld.Rooms[(int) id]);
        }

        public static void lua_SetRoomEnabled(int id, bool value)
        {
            if (!id.IsBetween(0, EngineWorld.Rooms.Count, IB.aIbE))
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_WRONG_ROOM, id);
                return;
            }

            if (value)
            {
                EngineWorld.Rooms[id].Enable();
            }
            else
            {
                EngineWorld.Rooms[id].Disable();
            }
        }

        #region Base engine functions

        public static void lua_SetModelCollisionMapSize(uint id, int size)
        {
            var sm = EngineWorld.GetModelByID(id);
            if (sm == null)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_MODELID_OVERFLOW, id);
                return;
            }

            if (size.IsBetween(0, sm.MeshCount, IB.aIbE))
            {
                sm.CollisionMap.Resize(size);
            }
        }

        public static void lua_SetModelCollisionMap(uint id, int arg, ushort val)
        {
            var sm = EngineWorld.GetModelByID(id);
            if (sm == null)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_MODELID_OVERFLOW, id);
                return;
            }

            if (arg.IsBetween(0, sm.CollisionMap.Count, IB.aIbE) && val.IsBetween(0, sm.MeshCount, IB.aIbE))
            {
                sm.CollisionMap[arg] = val;
            }
        }

        public static void lua_EnableEntity(uint id)
        {
            EngineWorld.GetEntityByID(id)?.Enable();
        }

        public static void lua_DisableEntity(uint id, bool val)
        {
            var ent = EngineWorld.GetEntityByID(id);
            if (ent != null)
            {
                if (val)
                    ent.EnableCollision();
                else
                    ent.DisableCollision();
            }
        }

        public static void lua_SetEntityCollisionFlags(uint id, int? ctype = null, int? cshape = null)
        {
            var ent = EngineWorld.GetEntityByID(id);
            if (ent != null)
            {
                if (ctype != null) ent.Self.CollisionType = (COLLISION_TYPE) ctype;
                if (cshape != null) ent.Self.CollisionShape = (COLLISION_SHAPE) cshape;

                if (ent.Self.CollisionType.HasFlag(COLLISION_TYPE.Static))
                {
                    ent.EnableCollision();
                }
                else
                {
                    ent.DisableCollision();
                }
            }
        }

        public static uint lua_GetEntitySectorFlags(uint id)
        {
            return EngineWorld.GetEntityByID(id)?.CurrentSector?.Flags ?? 0;
        }

        public static uint lua_GetEntitySectorIndex(uint id)
        {
            return EngineWorld.GetEntityByID(id)?.CurrentSector?.TrigIndex ?? 0;
        }

        public static uint lua_GetEntitySectorMaterial(uint id)
        {
            return EngineWorld.GetEntityByID(id)?.CurrentSector?.Material ?? 0;
        }

        public static uint lua_GetEntitySubstanceState(uint id)
        {
            return EngineWorld.GetEntityByID(id)?.SubstanceStateUInt32 ?? 0;
        }

        public static bool lua_SameRoom(uint id1, uint id2)
        {
            var ent1 = EngineWorld.GetEntityByID(id1);
            var ent2 = EngineWorld.GetEntityByID(id2);

            return ent1 != null && ent2 != null && ent1.Self.Room == ent2.Self.Room;
        }

        public static bool lua_SameSector(uint id1, uint id2)
        {
            var ent1 = EngineWorld.GetEntityByID(id1);
            var ent2 = EngineWorld.GetEntityByID(id2);

            return ent1?.CurrentSector != null && ent2?.CurrentSector != null &&
                   ent1.CurrentSector.TrigIndex == ent2.CurrentSector.TrigIndex;
        }

        public static bool lua_NewSector(uint id)
        {
            var ent = EngineWorld.GetEntityByID(id);

            return ent != null && ent.CurrentSector == ent.LastSector;
        }

        public static float[] lua_GetGravity()
        {
            return BtEngineDynamicsWorld.Gravity.ToArray();
        }

        public static void lua_SetGravity(float x, float y = 0, float z = 0)
        {
            var g = new Vector3(x, y, z);
            BtEngineDynamicsWorld.Gravity = g;
            ConsoleInfo.Instance.Printf("gravity = {0}", g.ToStringEx(round: 3));
        }

        public static bool lua_DropEntity(uint id, float time, bool onlyRoom = false)
        {
            var ent = EngineWorld.GetEntityByID(id);
            if (ent == null)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_ENTITY, id);
                return false;
            }

            var move = ent.ApplyGravity(time);

            var cb = new BtEngineClosestRayResultCallback(ent.Self);
            var from = ent.Transform * ent.Bf.Centre;
            from.Z = ent.Transform.Origin.Z;
            var to = from + move;
            //to.Z -= ent.Bf.BBMax.Z - ent.Bf.BBMin.Z;
            BtEngineDynamicsWorld.RayTest(from, to, cb);

            if (cb.HasHit)
            {
                var cont = (EngineContainer) cb.CollisionObject.UserObject;

                if (!onlyRoom || (onlyRoom && cont.ObjectType == OBJECT_TYPE.RoomBase))
                {
                    Helper.SetInterpolate3(out move, from, to, cb.ClosestHitFraction);
                    ent.Transform.Origin.Z = move.Z;

                    ent.UpdateRigidBody(true);
                    return true;
                }
                else
                {
                    ent.UpdateRigidBody(true);
                    return false;
                }
            }
            else
            {
                ent.Transform.Origin += move;
                ent.UpdateRigidBody(true);
                return false;
            }
        }

        public static object lua_GetEntityModelID(uint id)
        {
            var ent = EngineWorld.GetEntityByID(id);

            if (ent?.Bf.Animations == null)
                return -1;
            return ent.Bf.Animations.Model.ID;
        }

        public static float[] lua_GetEntityActivationOffset(uint id)
        {
            var ent = EngineWorld.GetEntityByID(id);
            return ent == null ? new float[0] : ent.ActivationOffset.ToArray().AddArray(ent.ActivationRadius);
        }

        public static void lua_SetEntityActivationOffset(uint id, float x, float y, float z, float? r = null)
        {
            var ent = EngineWorld.GetEntityByID(id);
            if (ent == null)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_ENTITY, id);
                return;
            }

            ent.ActivationOffset = new Vector3(x, y, z);
            if (r != null)
                ent.ActivationRadius = (float) r;
        }

        public static float lua_GetCharacterParam(uint id, int parameter)
        {
            var ent = EngineWorld.GetCharacterByID(id);

            if (parameter >= (int) CharParameters.Sentinel)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_WRONG_OPTION_INDEX, (int) CharParameters.Sentinel);
                // TODO: Should put parameter instead?
                return -1;
            }

            if (ent != null)
            {
                return ent.GetParam((CharParameters) parameter);
            }
            else
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_CHARACTER, id);
                return -1;
            }
        }

        public static void lua_SetCharacterParam(uint id, int parameter, float value, float? maxValue = null)
        {
            var ent = EngineWorld.GetCharacterByID(id);

            if (parameter >= (int) CharParameters.Sentinel)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_WRONG_OPTION_INDEX, (int) CharParameters.Sentinel);
                // TODO: Should put parameter instead?
                return;
            }

            if (ent == null)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_CHARACTER, id);
                return;
            }
            else if (maxValue == null)
            {
                ent.SetParam((CharParameters) parameter, value);
            }
            else
            {
                ent.Parameters.Param[parameter] = value;
                ent.Parameters.Maximum[parameter] = (float) maxValue;
            }
        }

        public static int lua_GetCharacterCombatMode(uint id)
        {
            var ent = EngineWorld.GetCharacterByID(id);

            if (ent != null)
            {
                return (int) ent.WeaponCurrentState;
            }

            return -1;
        }

        public static void lua_ChangeCharacterParam(uint id, int parameter, float? value = null)
        {
            var ent = EngineWorld.GetCharacterByID(id);

            if (parameter >= (int) CharParameters.Sentinel)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_WRONG_OPTION_INDEX, (int) CharParameters.Sentinel);
                // TODO: Should put parameter instead?
                return;
            }

            if (ent != null && value != null)
            {
                ent.ChangeParam((CharParameters) parameter, (float) value);
            }
            else
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_CHARACTER, id);
            }
        }

        public static void lua_AddCharacterHair(uint entID, uint setupIndex)
        {
            var ent = EngineWorld.GetCharacterByID(entID);

            if (ent != null)
            {
                var hairSetup = new HairSetup();

                hairSetup.GetSetup(setupIndex);
                ent.Hairs.Add(new Hair());

                if (!ent.Hairs.Last().Create(hairSetup, ent))
                {
                    ConsoleInfo.Instance.Warning(Strings.SYSWARN_CANT_CREATE_HAIR, entID);
                    ent.Hairs.RemoveAt(ent.Hairs.Count - 1);
                }
            }
            else
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_CHARACTER, entID);
            }
        }

        public static void lua_ResetCharacterHair(uint entID)
        {
            var ent = EngineWorld.GetCharacterByID(entID);

            if (ent != null)
            {
                if (ent.Hairs.Count == 0)
                {
                    ConsoleInfo.Instance.Warning(Strings.SYSWARN_CANT_RESET_HAIR, entID);
                }
                else
                {
                    ent.Hairs.Clear();
                }
            }
            else
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_CHARACTER, entID);
            }
        }

        public static void lua_AddEntityRagdoll(uint entID, int setupIndex)
        {
            var ent = EngineWorld.GetEntityByID(entID);

            if (ent != null)
            {
                var ragdollSetup = new RDSetup();

                if (!ragdollSetup.GetSetup(setupIndex))
                {
                    ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_RAGDOLL_SETUP, setupIndex);
                }
                else
                {
                    if (!ent.CreateRagdoll(ragdollSetup))
                    {
                        ConsoleInfo.Instance.Warning(Strings.SYSWARN_CANT_CREATE_RAGDOLL, entID);
                    }
                }
            }
            else
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_ENTITY, entID);
            }
        }

        public static void lua_RemoveEntityRagdoll(uint entID)
        {
            var ent = EngineWorld.GetEntityByID(entID);

            if (ent != null)
            {
                if (ent.Bt.BtJoints.Count == 0)
                {
                    ConsoleInfo.Instance.Warning(Strings.SYSWARN_CANT_REMOVE_RAGDOLL, entID);
                }
                else
                {
                    ent.DeleteRagdoll();
                }
            }
            else
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_ENTITY, entID);
            }
        }

        public static bool lua_GetSecretStatus(int secretNumber)
        {
            if (!secretNumber.IsBetween(0, GF_MAX_SECRETS))
                return false; // No such secret - return TODO: Add Warning to console

            return GameflowManager.SecretsTriggerMap[secretNumber];
        }

        // TODO: If no 'status' provided, maybe toggle it?
        public static void lua_SetSecretStatus(int secretNumber, bool status)
        {
            if (!secretNumber.IsBetween(0, GF_MAX_SECRETS))
                return; // No such secret - return TODO: Add Warning to console

            GameflowManager.SecretsTriggerMap[secretNumber] = status;
        }

        public static bool lua_GetActionState(int act)
        {
            if (act.IsBetween(0, (int) ACTIONS.LastIndex, IB.aIbE))
            {
                return ControlMapper.ActionMap[act].State;
            }
            else
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_WRONG_ACTION_NUMBER);
                return false;
            }
        }

        public static bool lua_GetActionChange(int act)
        {
            if (act.IsBetween(0, (int) ACTIONS.LastIndex, IB.aIbE))
            {
                return ControlMapper.ActionMap[act].AlreadyPressed;
            }
            else
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_WRONG_ACTION_NUMBER);
                return false;
            }
        }

        public static int lua_GetEngineVersion()
        {
            return (int) EngineWorld.EngineVersion;
        }

        public static void lua_AddFont(int index, string path, uint size)
        {
            if (!Global.FontManager.AddFont((FontType) index, size, path))
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_CANT_CREATE_FONT, Global.FontManager.FontCount, MaxFonts);
            }
        }

        public static void lua_AddFontStyle(int style_index,
            float color_R, float color_G, float color_B, float color_A,
            bool shadowed, bool fading, bool rect, float rect_border,
            float rect_R, float rect_G, float rect_B, float rect_A,
            bool hide)
        {
            if (!Global.FontManager.AddFontStyle((FontStyle) style_index, 
                new[] {color_R, color_G, color_B, color_A},
                shadowed, fading,
                rect, rect_border, new[] {rect_R, rect_G, rect_B, rect_A},
                hide))
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_CANT_CREATE_STYLE, Global.FontManager.FontStyleCount, (int)FontStyle.Sentinel);
            }
        }

        public static void lua_DeleteFont(int fontindex)
        {
            if(!Global.FontManager.RemoveFont((FontType)fontindex))
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_CANT_REMOVE_FONT);
            }
        }

        public static void lua_DeleteFontStyle(int styleindex)
        {
            if (!Global.FontManager.RemoveFontStyle((FontStyle)styleindex))
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_CANT_REMOVE_STYLE);
            }
        }

        public static int lua_AddItem(uint entID, uint itemID, int count = -1)
        {
            var ent = EngineWorld.GetCharacterByID(entID);

            if (ent != null)
            {
                return ent.AddItem(itemID, count);
            }
            else
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_ENTITY, entID);
                return -1;
            }
        }

        public static int lua_RemoveItem(uint entID, uint itemID, int count = -1)
        {
            var ent = EngineWorld.GetCharacterByID(entID);

            if (ent != null)
            {
                return ent.RemoveItem(itemID, count);
            }
            else
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_ENTITY, entID);
                return -1;
            }
        }

        public static void lua_RemoveAllItems(uint entID) // TODO: Made this return an int (from RemoveAllItems)
        {
            var ent = EngineWorld.GetCharacterByID(entID);

            if (ent != null)
            {
                ent.RemoveAllItems(); 
            }
            else
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_ENTITY, entID);
            }
        }

        public static int lua_GetItemsCount(uint entID, uint itemID)
        {
            var ent = EngineWorld.GetCharacterByID(entID);

            if (ent != null)
            {
                return ent.GetItemsCount(itemID);
            }
            else
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_ENTITY, entID);
                return -1;
            }
        }

        public static void lua_CreateBaseItem(uint item_id, uint model_id, uint world_model_id, int type, ushort count,
            string name)
        {
            EngineWorld.CreateItem(item_id, model_id, world_model_id, (MenuItemType) type, count, name);
        }

        public static void lua_DeleteBaseItem(uint id) // TODO: Make this return int
        {
            EngineWorld.DeleteItem(id);
        }

        public static void lua_PrintItems(uint entityID)
        {
            var ent = EngineWorld.GetCharacterByID(entityID);
            if(ent == null)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_ENTITY, entityID);
                return;
            }

            foreach (var i in ent.Inventory)
            {
                ConsoleInfo.Instance.Printf("item[id = {0}]: count = {1}", i.ID, i.Count);
            }
        }

        public static void lua_SetStateChangeRange(uint id, int anim, short state, int dispatch, ushort frame_low, ushort frame_high, int? next_anim = null, ushort? next_frame = null)
        {
            var model = EngineWorld.GetModelByID(id);

            if(model == null)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_SKELETAL_MODEL, id);
                return;
            }

            if(!anim.IsBetween(0, model.Animations.Count, IB.aIbE))
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_WRONG_ANIM_NUMBER);
                return;
            }

            foreach (var sc in model.Animations[anim].StateChange)
            {
                if((short)sc.ID == state)
                {
                    if(dispatch.IsBetween(0, sc.AnimDispatch.Count, IB.aIbE))
                    {
                        sc.AnimDispatch[dispatch].FrameLow = frame_low;
                        sc.AnimDispatch[dispatch].FrameHigh = frame_high;
                        if(next_anim != null && next_frame != null)
                        {
                            sc.AnimDispatch[dispatch].NextAnim = (TR_ANIMATION) next_anim;
                            sc.AnimDispatch[dispatch].NextFrame = (ushort)next_frame;
                        }
                    }
                    else
                    {
                        ConsoleInfo.Instance.Warning(Strings.SYSWARN_WRONG_DISPATCH_NUMBER, dispatch);
                    }
                    break;
                }
            }
        }

        public static object[] lua_GetAnimCommandTransform(uint id, int anim, int frame)
        {
            var model = EngineWorld.GetModelByID(id);

            if (model == null)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_SKELETAL_MODEL, id);
                return new object[0];
            }

            if (!anim.IsBetween(0, model.Animations.Count, IB.aIbE))
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_WRONG_ANIM_NUMBER);
                return new object[0];
            }

            var an = model.Animations[anim];

            if (frame < 0) // it is convenient to use -1 as a last frame number
            {
                frame = an.Frames.Count + frame;
            }

            if (!frame.IsBetween(0, an.Frames.Count, IB.aIbE))
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_WRONG_FRAME_NUMBER);
                return new object[0];
            }

            var fr = an.Frames[frame];

            return new object[] {fr.Command, fr.Move.X, fr.Move.Y, fr.Move.Z};
        }

        public static void lua_SetAnimCommandTransform(uint id, int anim, int frame, ushort flag, float? dx = null, float? dy = null, float? dz = null)
        {
            var model = EngineWorld.GetModelByID(id);

            if (model == null)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_SKELETAL_MODEL, id);
                return;
            }

            if (!anim.IsBetween(0, model.Animations.Count, IB.aIbE))
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_WRONG_ANIM_NUMBER);
                return;
            }

            var an = model.Animations[anim];

            if (frame < 0) // it is convenient to use -1 as a last frame number
            {
                frame = an.Frames.Count + frame;
            }

            if (!frame.IsBetween(0, an.Frames.Count, IB.aIbE))
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_WRONG_FRAME_NUMBER);
                return;
            }

            an.Frames[frame].Command = flag;

            if (dx != null && dy != null && dz != null)
                an.Frames[frame].Move = new Vector3((float) dx, (float) dy, (float) dz);
        }

        public static void lua_SetAnimVerticalSpeed(uint id, int anim, int frame, float speed)
        {
            var model = EngineWorld.GetModelByID(id);

            if (model == null)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_SKELETAL_MODEL, id);
                return;
            }

            if (!anim.IsBetween(0, model.Animations.Count, IB.aIbE))
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_WRONG_ANIM_NUMBER);
                return;
            }

            var an = model.Animations[anim];

            if (frame < 0) // it is convenient to use -1 as a last frame number
            {
                frame = an.Frames.Count + frame;
            }

            if (!frame.IsBetween(0, an.Frames.Count, IB.aIbE))
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_WRONG_FRAME_NUMBER);
                return;
            }
            
            an.Frames[frame].V_Vertical = speed;
        }

        public static uint lua_SpawnEntity(uint model_id, float x, float y, float z, float ax, float ay, float az, uint room_id, int ov_id = -1)
        {
            return EngineWorld.SpawnEntity(model_id, room_id, new Vector3(x, y, z), new Vector3(ax, ay, az), ov_id);
        }

        public static bool lua_DeleteEntity(uint id)
        {
            var ent = EngineWorld.GetEntityByID(id);

            if(ent == null)
            {
                return false;
            }
            else
            {
                ent.Self.Room?.RemoveEntity(ent);
                EngineWorld.DeleteEntity(id); // TODO: Return this instead of true
                return true;
            }
        }

        #endregion

        #region Moveable script control section

        public static float[] lua_GetEntityVector(uint id1, uint id2)
        {
            var e1 = EngineWorld.GetEntityByID(id1);
            if(e1 == null)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_ENTITY, id1);
                return new float[0];
            }
            var e2 = EngineWorld.GetEntityByID(id2);
            if (e2 == null)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_ENTITY, id2);
                return new float[0];
            }

            return (e2.Transform.Origin - e1.Transform.Origin).ToArray();
        }

        public static float lua_GetEntityDistance(uint id1, uint id2)
        {
            var e1 = EngineWorld.GetEntityByID(id1);
            if (e1 == null)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_ENTITY, id1);
                return float.MaxValue;
            }
            var e2 = EngineWorld.GetEntityByID(id2);
            if (e2 == null)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_ENTITY, id2);
                return float.MaxValue;
            }

            return e1.FindDistance(e2);
        }

        public static float lua_GetEntityDirDot(uint id1, uint id2)
        {
            var e1 = EngineWorld.GetEntityByID(id1);
            if (e1 == null)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_ENTITY, id1);
                return float.MaxValue;
            }
            var e2 = EngineWorld.GetEntityByID(id2);
            if (e2 == null)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_ENTITY, id2);
                return float.MaxValue;
            }

            return e1.Transform.Basis.Column1.Dot(e2.Transform.Basis.Column1);
        }

        public static bool lua_IsInRoom(uint id)
        {
            var ent = EngineWorld.GetEntityByID(id);

            return ent?.Self.Room != null && ent.CurrentSector != null;
        }

        public static object[] lua_GetEntityPosition(uint id)
        {
            var ent = EngineWorld.GetEntityByID(id);
            if(ent == null)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_ENTITY, id);
                return new object[0];
            }

            return
                ent.Transform.Origin.ToArray()
                    .AddArray(ent.Angles.ToArray())
                    .Cast<object>()
                    .AddItems(ent.Self.Room.ID)
                    .ToArray();
        }

        public static float[] lua_GetEntityAngles(uint id)
        {
            var ent = EngineWorld.GetEntityByID(id);
            if (ent == null)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_ENTITY, id);
                return new float[0];
            }

            return ent.Angles.ToArray();
        }

        public static float[] lua_GetEntityScaling(uint id)
        {
            var ent = EngineWorld.GetEntityByID(id);
            if (ent == null)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_ENTITY, id);
                return new float[0];
            }

            return ent.Scaling.ToArray();
        }

        public static bool lua_SimilarSector(uint id, float dx, float dy, float dz, bool ignoreDoors,
            bool ceiling = false)
        {
            var ent = EngineWorld.GetEntityByID(id);

            if (ent == null)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_ENTITY, id);
                return false;
            }

            var nextPos = ent.Transform.Origin +
                          (dx * ent.Transform.Basis.Column0 +
                           dy * ent.Transform.Basis.Column1 +
                           dz * ent.Transform.Basis.Column2);

            var currSector = ent.Self.Room.GetSectorRaw(ent.Transform.Origin);
            var nextSector = ent.Self.Room.GetSectorRaw(nextPos);

            currSector = currSector.CheckPortalPointer();
            nextSector = nextSector.CheckPortalPointer();

            return ceiling
                ? currSector.SimilarCeiling(nextSector, ignoreDoors)
                : currSector.SimilarFloor(nextSector, ignoreDoors);
        }

        public static float lua_GetSectorHeight(uint id, bool ceiling = false, float? dx = null, float? dy = null,
            float? dz = null)
        {
            var ent = EngineWorld.GetEntityByID(id);

            if (ent == null)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_ENTITY, id);
                return 0;
            }

            var pos = ent.Transform.Origin;

            if (dx != null && dy != null && dz != null)
                pos += (float) dx * ent.Transform.Basis.Column0 +
                       (float) dy * ent.Transform.Basis.Column1 +
                       (float) dz * ent.Transform.Basis.Column2;

            var currSector = ent.Self.Room.GetSectorRaw(pos);
            currSector = currSector.CheckPortalPointer();

            return (ceiling ? currSector.GetCeilingPoint() : currSector.GetFloorPoint()).Z;
        }

        public static void lua_SetEntityPosition(uint id, float x, float y, float z, float? ax = null, float? ay = null,
            float? az = null)
        {
            var ent = EngineWorld.GetEntityByID(id);
            if (ent == null)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_ENTITY, id);
                return;
            }
            ent.Transform.Origin = new Vector3(x, y, z);
            if (ax != null && ay != null && az != null)
                ent.Angles = new Vector3((float) ax, (float) ay, (float) az);
            ent.UpdateTransform();
            ent.UpdatePlatformPreStep();
        }

        public static void lua_SetEntityAngles(uint id, float x, float? y = null, float? z = null)
        {
            var ent = EngineWorld.GetEntityByID(id);
            if (ent == null)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_ENTITY, id);
            }
            else
            {
                if (y == null || z == null)
                    ent.Angles.X = x;
                else
                    ent.Angles = new Vector3(x, (float) y, (float) z);
                ent.UpdateTransform();
            }
        }

        public static void lua_SetEntityScaling(uint id, float x, float y, float z)
        {
            var ent = EngineWorld.GetEntityByID(id);
            if (ent == null)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_ENTITY, id);
            }
            else
            {
                ent.Scaling = new Vector3(x, y, z);
                
                if(ent.Bf.BoneTags.Count > 0 && ent.Bt.BtBody.Count > 0)
                {
                    for (var i = 0; i < ent.Bf.BoneTags.Count; i++)
                    {
                        if(ent.Bt.BtBody[i] != null)
                        {
                            BtEngineDynamicsWorld.RemoveRigidBody(ent.Bt.BtBody[i]);
                            ent.Bt.BtBody[i].CollisionShape.LocalScaling = ent.Scaling;
                            BtEngineDynamicsWorld.AddRigidBody(ent.Bt.BtBody[i]);

                            ent.Bt.BtBody[i].Activate();
                        }
                    }
                }

                ent.UpdateRigidBody(true);
            }
        }

        public static void lua_MoveEntityGlobal(uint id, float x, float y, float z)
        {
            var ent = EngineWorld.GetEntityByID(id);
            if (ent == null)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_ENTITY, id);
                return;
            }
            ent.Transform.Origin += new Vector3(x, y, z);
            
            ent.UpdateRigidBody(true);
            ent.GhostUpdate();
        }

        public static void lua_MoveEntitLocal(uint id, float dx, float dy, float dz)
        {
            var ent = EngineWorld.GetEntityByID(id);
            if (ent == null)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_ENTITY, id);
                return;
            }

            ent.Transform.Origin += ent.Transform.Basis.MultiplyByVector(new Vector3(dx, dy, dz));

            ent.UpdateRigidBody(true);
            ent.GhostUpdate();
        }

        #endregion
    }
}

namespace FreeRaider.Script
{
    public class ScriptEngine
    {
        public ScriptEngine()
        {
            ExposeConstants();
            RegisterFunction("print", print);
            //lua_atpanic(m_state.getState(), &ScriptEngine::panic); NOT NEEDE ANYMORE, NLua handles exceptions by itself
        }

        public void DoFile(string filename)
        {
            state.DoFile(filename);
        }

        public void DoString(string script)
        {
            state.DoString(script);
        }

        public object this[string key] => Get(key);

        public object Get(string key)
        {
            return state[key];
        }

        public ScriptEngine Set(string key, object value)
        {
            state[key] = value;
            return this;
        }

        public object[] Call(string funcName, params object[] args)
        {
            return (Get(funcName) as LuaFunction).Call(args);
        }

        public void RegisterC(string funcName, MethodInfo m)
        {
            state.RegisterFunction(funcName, m);
            state.RegisterFunction(funcName.ToLower(), m);
            state.RegisterFunction(funcName.ToUpper(), m);
        }

        public void RegisterC(string funcName, Func<Lua, int> func)
        {
            RegisterC(funcName, func.Method);
        }

        public void RegisterFunction(string funcName, Func<Lua, int> func)
        {
            state.RegisterFunction(funcName, func.Method);
        }

        public void RegisterFunction(string funcName, MethodBase func)
        {
            state.RegisterFunction(funcName, func);
        }

        public void RegisterFunction(string funcName, Action func)
        {
            state.RegisterFunction(funcName, func.Method);
        }


        public void ExposeConstants();

        public List<string> GetGlobals();

        public void ParseScreen(ScreenInfo sc);

        public void ParseRender(RenderSettings rs);

        public void ParseAudio(AudioSettings au);

        public void ParseConsole(ConsoleInfo cn);

        public void ParseControls(ControlSettings cs);

        public void ParseSystem(SystemSettings ss);

        protected void CheckStack()
        {
            ConsoleInfo.Instance.Notify(Strings.SYSNOTE_LUA_STACK_INDEX, LuaCore.LuaGetTop(state.GetLuaState()));
        }

        public NLua.Lua state;

        private static int print(NLua.Lua state)
        {
            var top = LuaCore.LuaGetTop(state.GetLuaState());

            if(top == 0)
            {
                ConsoleInfo.Instance.AddLine("nil", FontStyle.ConsoleEvent);
                return 0;
            }

            for(var i = 1; i <= top; i++)
            {
                string str;
                switch(LuaCore.LuaType(state.GetLuaState(), i))
                {
                    case LUA_TNONE:
                        str = "<none>";
                        break;
                    case LUA_TNIL:
                        str = "nil";
                        break;
                    case LUA_TBOOLEAN:
                        str = LuaCore.LuaToBoolean(state.GetLuaState(), i) == 1 ? "true" : "false";
                        break;
                    case LUA_TLIGHTUSERDATA:
                        str = "<userdata>";
                        break;
                    case LUA_TNUMBER:
                    case LUA_TSTRING:
                        uint tmp;
                        str = LuaCore.LuaToLString(state.GetLuaState(), i, out tmp).ToString(); // TODO: Maybe won't work
                        break;
                    case LUA_TTABLE:
                        str = "<table>";
                        break;
                    case LUA_TFUNCTION:
                        str = "<function>";
                        break;
                    case LUA_TUSERDATA:
                        str = "<userdata>";
                        break;
                    case LUA_TTHREAD:
                        str = "<thread>";
                        break;
                    default:
                        str = "<invalid>";
                        break;
                }

                ConsoleInfo.Instance.AddLine(str, FontStyle.ConsoleEvent);
            }
            return 0;
        }

        private static int panic(NLua.Lua state)
        {
            if(LuaCore.LuaGetTop(state.GetLuaState()) < 1)
            {
                Console.Error.WriteLine("Fatal lua error (no details provided).");
            }
            else
            {
                uint tmp;
                Console.Error.WriteLine("Fatal lua error: {0}", LuaCore.LuaToLString(state.GetLuaState(), 1, out tmp).ToString());
            }
            Console.Error.Flush();
            return 0;
        }
    }

    public class MainEngine : ScriptEngine
    {
        public MainEngine() : base()
        {
            registerMainFunctions();
            DoFile("scripts/loadscript.lua");
        }

        public void ClearTasks()
        {
            Call("clearTasks");
        }

        public void Prepare()
        {
            Call("fe_Prepare");
        }

        public void Clean()
        {
            Call("st_Clear");
            Call("tlist_Clear");
            Call("entfuncs_Clear");
            Call("fe_Clear");

            Call("clearAutoexec");
        }

        public void DoTasks(float time)
        {
            Set("FRAME_TIME", time);

            Call("doTasks");
            Call("clearKeys");
        }

        public void LoopEntity(int objectID);

        public void ExecEntity(int idCallback, int idObject, int idActivator = -1);

        public void ExecEffect(int id, int caller = -1, int operand = -1);

        public void AddKey(int keycode, bool state);

        public static void BindKey(int act, int primary, int? secondary = null)
        {
            if(!act.IsBetween(0, (int)ACTIONS.LastIndex, IB.aIbE))
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_WRONG_ACTION_NUMBER);
            }
            ControlMapper.ActionMap[act].Primary = primary;
            if (secondary != null)
                ControlMapper.ActionMap[act].Secondary = (int) secondary;
        }

        public bool GetOverridedSamplesInfo(out int numSamples, out int numSounds, out string samplesNameMask);

        public bool GetOverridedSample(int soundID, out int firstSampleNumber, out int samplesCount);

        public int GetGlobalSound(int globalSoundID);

        public int GetSecretTrackNumber();

        public int GetNumTracks();

        public bool GetSoundtrack(int trackIndex, string trackPath, TR_AUDIO_STREAM_METHOD loadMethod, TR_AUDIO_STREAM_TYPE streamType);

        public string GetLoadingScreen(int levelIndex);

        public string GetString(int stringID);

        public string GetSysNotify(int stringID);

        public static int ParseToken(string data, int index, out string token);

        public static float ParseFloat(string str, int index);

        public static int ParseInt(string str, int index);

        private void registerMainFunctions();
    }
}
