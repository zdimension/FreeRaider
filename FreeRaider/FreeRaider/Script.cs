using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using BulletSharp;
using FreeRaider.Loader;
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
            if (!id.IsBetween(0, EngineWorld.Rooms.Count - 1))
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

            if (size.IsBetween(0, sm.MeshCount - 1))
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

            if (arg.IsBetween(0, sm.CollisionMap.Count - 1) && val.IsBetween(0, sm.MeshCount - 1))
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
            if (act.IsBetween(0, (int) ACTIONS.LastIndex - 1))
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
            if (act.IsBetween(0, (int) ACTIONS.LastIndex - 1))
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
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_CANT_CREATE_STYLE, Global.FontManager.FontStyleCount,
                    (int) FontStyle.Sentinel);
            }
        }

        public static void lua_DeleteFont(int fontindex)
        {
            if (!Global.FontManager.RemoveFont((FontType) fontindex))
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_CANT_REMOVE_FONT);
            }
        }

        public static void lua_DeleteFontStyle(int styleindex)
        {
            if (!Global.FontManager.RemoveFontStyle((FontStyle) styleindex))
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
            if (ent == null)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_ENTITY, entityID);
                return;
            }

            foreach (var i in ent.Inventory)
            {
                ConsoleInfo.Instance.Printf("item[id = {0}]: count = {1}", i.ID, i.Count);
            }
        }

        public static void lua_SetStateChangeRange(uint id, int anim, short state, int dispatch, ushort frame_low,
            ushort frame_high, int? next_anim = null, ushort? next_frame = null)
        {
            var model = EngineWorld.GetModelByID(id);

            if (model == null)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_SKELETAL_MODEL, id);
                return;
            }

            if (!anim.IsBetween(0, model.Animations.Count - 1))
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_WRONG_ANIM_NUMBER);
                return;
            }

            foreach (var sc in model.Animations[anim].StateChange)
            {
                if ((short) sc.ID == state)
                {
                    if (dispatch.IsBetween(0, sc.AnimDispatch.Count - 1))
                    {
                        sc.AnimDispatch[dispatch].FrameLow = frame_low;
                        sc.AnimDispatch[dispatch].FrameHigh = frame_high;
                        if (next_anim != null && next_frame != null)
                        {
                            sc.AnimDispatch[dispatch].NextAnim = (TR_ANIMATION) next_anim;
                            sc.AnimDispatch[dispatch].NextFrame = (ushort) next_frame;
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

            if (!anim.IsBetween(0, model.Animations.Count - 1))
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_WRONG_ANIM_NUMBER);
                return new object[0];
            }

            var an = model.Animations[anim];

            if (frame < 0) // it is convenient to use -1 as a last frame number
            {
                frame = an.Frames.Count + frame;
            }

            if (!frame.IsBetween(0, an.Frames.Count - 1))
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_WRONG_FRAME_NUMBER);
                return new object[0];
            }

            var fr = an.Frames[frame];

            return new object[] {fr.Command, fr.Move.X, fr.Move.Y, fr.Move.Z};
        }

        public static void lua_SetAnimCommandTransform(uint id, int anim, int frame, ushort flag, float? dx = null,
            float? dy = null, float? dz = null)
        {
            var model = EngineWorld.GetModelByID(id);

            if (model == null)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_SKELETAL_MODEL, id);
                return;
            }

            if (!anim.IsBetween(0, model.Animations.Count - 1))
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_WRONG_ANIM_NUMBER);
                return;
            }

            var an = model.Animations[anim];

            if (frame < 0) // it is convenient to use -1 as a last frame number
            {
                frame = an.Frames.Count + frame;
            }

            if (!frame.IsBetween(0, an.Frames.Count - 1))
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

            if (!anim.IsBetween(0, model.Animations.Count - 1))
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_WRONG_ANIM_NUMBER);
                return;
            }

            var an = model.Animations[anim];

            if (frame < 0) // it is convenient to use -1 as a last frame number
            {
                frame = an.Frames.Count + frame;
            }

            if (!frame.IsBetween(0, an.Frames.Count - 1))
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_WRONG_FRAME_NUMBER);
                return;
            }

            an.Frames[frame].V_Vertical = speed;
        }

        public static uint lua_SpawnEntity(uint model_id, float x, float y, float z, float ax, float ay, float az,
            uint room_id, int ov_id = -1)
        {
            return EngineWorld.SpawnEntity(model_id, room_id, new Vector3(x, y, z), new Vector3(ax, ay, az), ov_id);
        }

        public static bool lua_DeleteEntity(uint id)
        {
            var ent = EngineWorld.GetEntityByID(id);

            if (ent == null)
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
            if (e1 == null)
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
            if (ent == null)
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

                if (ent.Bf.BoneTags.Count > 0 && ent.Bt.BtBody.Count > 0)
                {
                    for (var i = 0; i < ent.Bf.BoneTags.Count; i++)
                    {
                        if (ent.Bt.BtBody[i] != null)
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

        public static void lua_MoveEntityLocal(uint id, float dx, float dy, float dz)
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

        public static void lua_MoveEntityToSink(uint id, int sinkIndex)
        {
            var ent = EngineWorld.GetEntityByID(id); // TODO: Add check

            if (!sinkIndex.IsBetween(0, EngineWorld.CamerasSinks.Count - 1))
                return;
            var sink = EngineWorld.CamerasSinks[sinkIndex];

            var entPos = ent.Transform.Origin;

            var sinkPos = new Vector3(sink.X, sink.Y, sink.Z + 256.0f);

            Assert.That(ent.CurrentSector != null);
            var ls = ent.CurrentSector.GetLowestSector();
            Assert.That(ls != null);
            var hs = ent.CurrentSector.GetHighestSector();
            Assert.That(hs != null);
            if (sinkPos.Z > hs.Ceiling || sinkPos.Z < ls.Floor)
            {
                sinkPos.Z = entPos.Z;
            }

            var dist = entPos.Distance(sinkPos);
            if (dist == 0.0f) dist = 1.0f; // Prevents division by zero.

            var speed = (sinkPos - entPos) / dist * (sink.RoomOrStrength * 1.5f);

            ent.Transform.Origin.X += speed.X;
            ent.Transform.Origin.Y += speed.Y;
            ent.Transform.Origin.Z += speed.Z * 16.0f;

            ent.UpdateRigidBody(true);
            ent.GhostUpdate();
        }

        public static void lua_MoveEntityToEntity(uint id1, uint id2, float speedMult, bool ignoreZ = false)
        {
            var ent1 = EngineWorld.GetEntityByID(id1);
            var ent2 = EngineWorld.GetEntityByID(id2);

            var ent1pos = ent1.Transform.Origin;
            var ent2pos = ent2.Transform.Origin;

            var dist = ent1pos.Distance(ent2pos);
            if (dist == 0.0f) dist = 1.0f; // Prevents division by zero

            var speed = (ent2pos - ent1pos) / dist * speedMult; // FIXME!

            ent1.Transform.Origin.X += speed.X;
            ent1.Transform.Origin.Y += speed.Y;
            if (!ignoreZ)
                ent1.Transform.Origin.Z += speed.Z;
            ent1.UpdatePlatformPreStep();
            ent1.UpdateRigidBody(true);
        }

        public static void lua_RotateEntity(uint id, float rx, float ry = 0, float rz = 0)
        {
            var ent = EngineWorld.GetEntityByID(id);
            if (ent == null)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_ENTITY, id);
            }
            else
            {
                ent.Angles += new Vector3(rx, ry, rz);
                // TODO: In OpenTomb, it doesn't work if ANY of ry / rz is null. Here, it's default-ed to 0

                ent.UpdateTransform();
                ent.UpdateRigidBody(true);
            }
        }

        public static void lua_RotateEntity(uint id1, uint id2, int axis, float? speed = null, float? smooth = null,
            float? addAngle = null)
        {
            var ent1 = EngineWorld.GetEntityByID(id1);
            var ent2 = EngineWorld.GetEntityByID(id2);

            if (ent1 == null || ent2 == null)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_ENTITY, ent1?.ID ?? ent2.ID);
            }
            else if (axis < 0 || axis > 2)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_WRONG_AXIS, axis);
                // TODO: OpenTomb uses entity's ID here? wtf?
            }
            else
            {
                var ent1pos = ent1.Transform.Origin;
                var ent2pos = ent2.Transform.Origin;
                var facing = ent1pos - ent2pos;

                int targAngleId = -1;
                float theta = -1;

                switch (axis)
                {
                    case 0:
                        targAngleId = 0;
                        theta = Helper.Atan2(-facing.X, facing.Y);
                        break;
                    case 1:
                        targAngleId = 1;
                        theta = Helper.Atan2(facing.Z, facing.Y);
                        break;
                    case 2:
                        targAngleId = 2;
                        theta = Helper.Atan2(facing.X, facing.Z);
                        break;
                }

                theta = theta.Degrees();
                if (addAngle != null) theta += (float) addAngle;

                var delta = ent1.Angles[targAngleId] - theta;

                if (Math.Ceiling(delta) != 180.0)
                {
                    if (speed != null)
                    {
                        var spd = (float) speed;

                        if (Math.Abs(delta) > spd)
                        {
                            // Solve ~0-360 rotation cases

                            if (Math.Abs(delta) > 180.0f)
                            {
                                delta = -(360.0f - ent1.Angles[targAngleId] + theta);
                            }
                            else
                            {
                                delta = 360.0f - theta + ent1.Angles[targAngleId];
                            }

                            if (delta > 180.0f)
                            {
                                ent1.Angles[targAngleId] = theta + 180.0f;
                            }
                            else if (delta.IsBetween(0.0f, 180.0f, IB.aIbE))
                            {
                                ent1.Angles[targAngleId] += spd;
                            }
                            else
                            {
                                ent1.Angles[targAngleId] -= spd;
                            }
                        }

                        if (Math.Abs(delta) + speed >= 180.0f)
                            ent1.Angles[targAngleId] = (float) (Math.Floor(theta) + 180.0);
                    }
                    else
                    {
                        ent1.Angles[targAngleId] = theta + 180.0f;
                    }
                }

                ent1.UpdateTransform();
                ent1.UpdateRigidBody(true);
            }
        }

        public static float lua_GetEntityOrientation(uint id1, uint id2, int axis, float? addAngle = null)
        {
            var ent1 = EngineWorld.GetEntityByID(id1);
            var ent2 = EngineWorld.GetEntityByID(id2);

            if (ent1 == null || ent2 == null)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_ENTITY, ent1?.ID ?? ent2.ID);
                return 0;
            }
            else
            {
                var ent1pos = ent1.Transform.Origin;
                var ent2pos = ent2.Transform.Origin;
                var facing = ent1pos - ent2pos;

                var theta = Helper.Atan2(-facing.X, facing.Y);
                if (addAngle != null) theta += (float) addAngle;

                return (ent2.Angles.X - theta).WrapAngle();
            }
        }

        public static float[] lua_GetEntitySpeed(uint id)
        {
            var ent = EngineWorld.GetEntityByID(id);
            if (ent == null)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_ENTITY, id);
                return new float[0];
            }

            return ent.Speed.ToArray();
        }

        public static float lua_GetEntitySoeedLinear(uint id)
        {
            var ent = EngineWorld.GetEntityByID(id);
            if (ent == null)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_ENTITY, id);
                return 0;
            }

            return ent.Angles.Length;
        }

        public static void lua_SetEntitySpeed(uint id, float vx, float? vy = null, float? vz = null)
        {
            var ent = EngineWorld.GetEntityByID(id);
            if (ent == null)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_ENTITY, id);
            }
            else
            {
                if (vy == null || vz == null)
                    ent.Angles.X = vx;
                else
                    ent.Angles = new Vector3(vx, (float) vy, (float) vz);
                ent.UpdateCurrentSpeed();
            }
        }

        public static void lua_SetEntityAnim(uint id, int anim, int frame = 0, int otherModel = -1)
        {
            var ent = EngineWorld.GetEntityByID(id);
            if (ent == null)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_ENTITY, id);
                return;
            }

            ent.SetAnimation((TR_ANIMATION) anim, frame, otherModel);
        }

        public static void lua_SetEntityAnimFlag(uint id, ushort animFlag)
        {
            var ent = EngineWorld.GetEntityByID(id);
            if (ent == null)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_ENTITY, id);
                return;
            }

            ent.Bf.Animations.AnimFlags = (AnimControlFlags) animFlag;
        }

        public static void lua_SetEntityBodyPartFlag(uint id, int boneID, uint bodyPartFlag)
        {
            var ent = EngineWorld.GetEntityByID(id);
            if (ent == null)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_ENTITY, id);
                return;
            }

            if (!boneID.IsBetween(0, ent.Bf.BoneTags.Count - 1))
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_WRONG_OPTION_INDEX, boneID);
                return;
            }

            ent.Bf.BoneTags[boneID].BodyPart = bodyPartFlag;
        }

        public static void lua_SetModelBodyPartFlag(uint id, int boneID, uint bodyPartFlag)
        {
            var model = EngineWorld.GetModelByID(id);
            if (model == null)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_SKELETAL_MODEL, id);
                return;
            }

            if (!boneID.IsBetween(0, model.MeshCount - 1))
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_WRONG_OPTION_INDEX, boneID);
                return;
            }

            model.MeshTree[boneID].BodyPart = bodyPartFlag;
        }

        public static object[] lua_GetEntityAnim(uint id)
        {
            var ent = EngineWorld.GetEntityByID(id);
            if (ent == null)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_ENTITY, id);
                return new object[0];
            }

            return new object[]
            {
                ent.Bf.Animations.CurrentAnimation,
                ent.Bf.Animations.CurrentFrame,
                ent.Bf.Animations.Model.Animations[(int) ent.Bf.Animations.CurrentAnimation].Frames.Count
            };
        }

        public static bool lua_CanTriggerEntity(uint id1, uint id2, float rv = -1, float? ofsX = null,
            float? ofsY = null, float? ofsZ = null)
        {
            var e1 = EngineWorld.GetCharacterByID(id1);
            if (e1 == null || !e1.Command.Action)
            {
                return false;
            }

            var e2 = EngineWorld.GetEntityByID(id2);
            if (e2 == null || e1 == e2)
            {
                return false;
            }

            var r = rv < 0 ? e2.ActivationRadius : rv;
            r *= r;

            var offset = new[] {ofsX, ofsY, ofsZ}.Contains(null)
                ? e2.ActivationOffset
                : new Vector3((float) ofsX, (float) ofsY, (float) ofsZ);

            var pos = e2.Transform * offset;
            return e1.Transform.Basis.Column1.Dot(e2.Transform.Basis.Column1) > 0.75f &&
                   (e1.Transform.Origin - pos).LengthSquared < r;
        }

        public static bool lua_GetEntityVisibility(uint id)
        {
            var ent = EngineWorld.GetEntityByID(id);

            if (ent == null)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_ENTITY, id);
                return false;
            }

            return ent.Visible;
        }

        public static void lua_SetEntityVisibility(uint id, bool value)
        {
            var ent = EngineWorld.GetEntityByID(id);

            if (ent == null)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_ENTITY, id);
                return;
            }

            ent.Visible = value;
        }

        public static bool lua_GetEntityEnability(uint id)
        {
            var ent = EngineWorld.GetEntityByID(id);

            if (ent == null)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_ENTITY, id);
                return false;
            }

            return ent.Enabled;
        }

        public static bool lua_GetEntityActivity(uint id)
        {
            var ent = EngineWorld.GetEntityByID(id);

            if (ent == null)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_ENTITY, id);
                return false;
            }

            return ent.Active;
        }

        public static void lua_SetEntityActivity(uint id, bool value)
        {
            var ent = EngineWorld.GetEntityByID(id);

            if (ent == null)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_ENTITY, id);
                return;
            }

            ent.Active = value;
        }

        public static object[] lua_GetEntityTriggerLayout(uint id)
        {
            var ent = EngineWorld.GetEntityByID(id);

            if (ent == null)
                return new object[0]; // No entity found - return. TODO: Add the warning?

            return new object[]
            {
                (int) (ent.TriggerLayout & ENTITY_TLAYOUT.Mask),
                (ent.TriggerLayout & ENTITY_TLAYOUT.Event) != 0,
                (ent.TriggerLayout & ENTITY_TLAYOUT.Lock) != 0
            };
        }

        public static void lua_SetEntityTriggerLayout(uint id, int mask, bool eventOrLayout = false, bool _lock = false)
        {
            var ent = EngineWorld.GetEntityByID(id);

            if (ent == null)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_ENTITY, id);
                return;
            }

            if (_lock)
            {
                var triggerLayout = (int) ent.TriggerLayout;
                triggerLayout &= ~(int) ENTITY_TLAYOUT.Mask;
                triggerLayout ^= mask;
                triggerLayout &= ~(int) ENTITY_TLAYOUT.Event;
                triggerLayout ^= (eventOrLayout ? 1 : 0) << 5;
                triggerLayout &= ~(int) ENTITY_TLAYOUT.Lock;
                triggerLayout ^= (_lock ? 1 : 0) << 6;
                ent.TriggerLayout = (ENTITY_TLAYOUT) triggerLayout;
            }
            else
            {
                ent.TriggerLayout = (ENTITY_TLAYOUT) (eventOrLayout ? 0 : 1);
            }
        }

        public static void lua_SetEntityLock(uint id, bool _lock)
        {
            var ent = EngineWorld.GetEntityByID(id);
            if (ent != null) // TODO: Add warning if null
            {
                var triggerLayout = (int) ent.TriggerLayout;
                triggerLayout &= ~(int) ENTITY_TLAYOUT.Lock;
                triggerLayout ^= (_lock ? 1 : 0) << 6; // lock - 01000000
                ent.TriggerLayout = (ENTITY_TLAYOUT) triggerLayout;
            }
        }

        public static bool lua_GetEntityLock(uint id)
        {
            var ent = EngineWorld.GetEntityByID(id);
            // TODO: Add warning if null
            return ent != null && (int) (ent.TriggerLayout & ENTITY_TLAYOUT.Lock) >> 6 != 0;
        }

        public static void lua_SetEntityEvent(uint id, bool _event)
        {
            var ent = EngineWorld.GetEntityByID(id);
            if (ent != null) // TODO: Add warning if null
            {
                var triggerLayout = (int)ent.TriggerLayout;
                triggerLayout &= ~(int)ENTITY_TLAYOUT.Event;
                triggerLayout ^= (_event ? 1 : 0) << 5; // event - 00100000
                ent.TriggerLayout = (ENTITY_TLAYOUT)triggerLayout;
            }
        }

        public static bool lua_GetEntityEvent(uint id)
        {
            var ent = EngineWorld.GetEntityByID(id);
            // TODO: Add warning if null
            return ent != null && (int)(ent.TriggerLayout & ENTITY_TLAYOUT.Event) >> 5 != 0;
        }

        public static int lua_GetEntityMask(uint id)
        {
            var ent = EngineWorld.GetEntityByID(id);
            // TODO: Add warning if null
            if(ent != null)
            {
                return (int) (ent.TriggerLayout & ENTITY_TLAYOUT.Mask); // mask
            }
            return -1;
        }

        public static void lua_SetEntityMask(uint id, int mask)
        {
            var ent = EngineWorld.GetEntityByID(id);
            if (ent != null) // TODO: Add warning if null
            {
                var triggerLayout = (int)ent.TriggerLayout;
                triggerLayout &= ~(int)ENTITY_TLAYOUT.Mask;
                triggerLayout ^= mask; // mask - 00011111
                ent.TriggerLayout = (ENTITY_TLAYOUT)triggerLayout;
            }
        }

        public static bool lua_GetEntitySectorStatus(uint id)
        {
            var ent = EngineWorld.GetEntityByID(id);
            // TODO: Add warning if null
            if (ent != null)
            {
                return (int) (ent.TriggerLayout & ENTITY_TLAYOUT.SectorStatus) >> 7 != 0;
            }
            return true;
        }

        public static void lua_SetEntitySectorStatus(uint id, bool status)
        {
            var ent = EngineWorld.GetEntityByID(id);
            if (ent != null) // TODO: Add warning if null
            {
                var triggerLayout = (int)ent.TriggerLayout;
                triggerLayout &= ~(int)ENTITY_TLAYOUT.SectorStatus;
                triggerLayout ^= (status ? 1 : 0) << 7; // sector_status - 10000000
                ent.TriggerLayout = (ENTITY_TLAYOUT)triggerLayout;
            }
        }

        public static int lua_GetEntityOCB(uint id)
        {
            var ent = EngineWorld.GetEntityByID(id);
            // TODO: Add warning if null
            return ent?.OCB ?? -1;
        }

        public static void lua_SetEntityOCB(uint id, int ocb)
        {
            var ent = EngineWorld.GetEntityByID(id);
            // TODO: Add warning if null
            if (ent == null)
                return; // No entity found - return.

            ent.OCB = ocb;
        }

        public static object[] lua_GetEntityFlags(uint id)
        {
            var ent = EngineWorld.GetEntityByID(id);

            if (ent == null)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_ENTITY, id);
                return new object[0];
            }

            return new object[]
            {
                ent.Active,
                ent.Enabled,
                ent.Visible,
                (ushort)ent.TypeFlags,
                (uint)ent.CallbackFlags
            };
        }

        public static void lua_SetEntityFlags(uint id, bool active, bool enabled, bool visible, ushort typeFlags, uint? cbFlags = null)
        {
            var ent = EngineWorld.GetEntityByID(id);

            if (ent == null)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_ENTITY, id);
                return;
            }

            ent.Active = active;
            ent.Enabled = enabled;
            ent.Visible = visible;
            ent.TypeFlags = (ENTITY_TYPE) typeFlags;
            if (cbFlags != null)
                ent.CallbackFlags = (ENTITY_CALLBACK) cbFlags;
        }

        public static uint lua_GetEntityTypeFlag(uint id, ushort? flag = null)
        {
            var ent = EngineWorld.GetEntityByID(id);

            if (ent == null)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_ENTITY, id);
                return uint.MaxValue;
            }

            if (flag != null)
                return (uint) ((ushort) ent.TypeFlags & flag);
            else
                return (uint)ent.TypeFlags;
        }

        public static void lua_SetEntityTypeFlag(uint id, ushort typeFlag, bool? value = null)
        {
            var ent = EngineWorld.GetEntityByID(id);

            if (ent == null)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_ENTITY, id);
                return;
            }
            if(value == null)
            {
                ent.TypeFlags ^= (ENTITY_TYPE) typeFlag;
                return;
            }

            if((bool)value)
            {
                ent.TypeFlags |= (ENTITY_TYPE) typeFlag;
            }
            else
            {
                ent.TypeFlags &= ~(ENTITY_TYPE)typeFlag;
            }
        }

        public static bool lua_GetEntityStateFlag(uint id, string whichCstr)
        {
            var ent = EngineWorld.GetEntityByID(id);

            if (ent == null)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_ENTITY, id);
                return false;
            }

            switch(whichCstr)
            {
                case "active":
                    return ent.Active;
                case "enabled":
                    return ent.Enabled;
                case "visible":
                    return ent.Visible;
                default:
                    return false;
            }
        }

        public static void lua_SetEntityStateFlag(uint id, string whichCstr, bool? value = null)
        {
            var ent = EngineWorld.GetEntityByID(id);

            if (ent == null)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_ENTITY, id);
                return;
            }

            var tmp = value == null;

            switch (whichCstr)
            {
                case "active":
                    ent.Active = tmp ? !ent.Active : (bool) value;
                    break;
                case "enabled":
                    ent.Enabled = tmp ? !ent.Enabled : (bool)value;
                    break;
                case "visible":
                    ent.Visible = tmp ? !ent.Visible : (bool)value;
                    break;
            }
        }

        public static uint lua_GetENTITY_CALLBACKFlag(uint id, ushort? flag = null)
        {
            var ent = EngineWorld.GetEntityByID(id);

            if (ent == null)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_ENTITY, id);
                return uint.MaxValue;
            }

            if (flag == null)
                return (uint) ent.CallbackFlags;
            else
                return (uint) ent.CallbackFlags & (ushort)flag;
        }

        public static void lua_SetENTITY_CALLBACKFlag(uint id, uint callbackFlag, bool? value = null)
        {
            var ent = EngineWorld.GetEntityByID(id);

            if (ent == null)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_ENTITY, id);
                return;
            }
            if (value == null)
            {
                ent.CallbackFlags ^= (ENTITY_CALLBACK)callbackFlag;
                return;
            }

            if ((bool)value)
            {
                ent.CallbackFlags |= (ENTITY_CALLBACK)callbackFlag;
            }
            else
            {
                ent.CallbackFlags &= ~(ENTITY_CALLBACK)callbackFlag;
            }
        }

        public static float lua_GetEntityTimer(uint id)
        {
            var ent = EngineWorld.GetEntityByID(id);
            // TODO: Add warning if null
            return ent?.Timer ?? float.MaxValue;
        }

        public static void lua_SetEntityTimer(uint id, float timer)
        {
            var ent = EngineWorld.GetEntityByID(id);
            // TODO: Add warning if null
            if (ent == null)
                return; // No entity found - return.

            ent.Timer = timer;
        }

        public static ushort lua_GetEntityMoveType(uint id, ushort? flag = null)
        {
            var ent = EngineWorld.GetEntityByID(id);

            if (ent == null)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_ENTITY, id);
                return ushort.MaxValue;
            }

            return (ushort) ent.MoveType;
        }

        public static void lua_SetENTITY_CALLBACKFlag(uint id, ushort type)
        {
            var ent = EngineWorld.GetEntityByID(id);
            // TODO: Add warning if null
            if (ent == null)
                return;
            ent.MoveType = (MoveType) type;
        }

        public static int lua_GetEntityResponse(uint id, int response)
        {
            var ent = EngineWorld.GetCharacterByID(id);

            if (ent != null)
            {
                switch (response)
                {
                    case 0: return ent.Response.Killed ? 1 : 0;
                    case 1: return ent.Response.VerticalCollide;
                    case 2: return ent.Response.HorizontalCollide;
                    case 3: return (int)ent.Response.Slide;
                    case 4: return (int)ent.Response.Lean;
                    default: return 0;
                }
            }
            else
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_ENTITY, id);
                return 0;
            }
        }

        public static void lua_SetEntityResponse(uint id, int response, int value)
        {
            var ent = EngineWorld.GetCharacterByID(id);

            if (ent != null)
            {
                switch (response)
                {
                    case 0:
                        ent.Response.Killed = value != 0;
                        break;
                    case 1:
                        ent.Response.VerticalCollide = (sbyte) value;
                        break;
                    case 2:
                        ent.Response.HorizontalCollide = (sbyte) value;
                        break;
                    case 3:
                        ent.Response.Slide = (SlideType) value;
                        break;
                    case 4:
                        ent.Response.Lean = (LeanType) value;
                        break;
                }
            }
            else
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_ENTITY, id);
            }
        }

        public static short lua_GetEntityState(uint id)
        {
            var ent = EngineWorld.GetEntityByID(id);

            if (ent == null)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_ENTITY, id);
                return -1;
            }

            return (short)ent.Bf.Animations.LastState;
        }

        public static uint lua_GetEntityModel(uint id)
        {
            var ent = EngineWorld.GetEntityByID(id);

            if (ent == null)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_ENTITY, id);
                return uint.MaxValue;
            }

            return ent.Bf.Animations.Model.ID;
        }

        public static void lua_SetEntityState(uint id, short value, short? next = null)
        {
            var ent = EngineWorld.GetEntityByID(id);

            if (ent == null)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_ENTITY, id);
                return;
            }

            ent.Bf.Animations.NextState = (TR_STATE) value;
            if (next != null)
                ent.Bf.Animations.LastState = (TR_STATE) next; // TODO: WTF?? It's inverted
        }

        public static void lua_SetEntityRommMove(uint id, int room, ushort moveType, int dirFlag)
        {
            var ent = EngineWorld.GetEntityByID(id);

            if (ent == null)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_ENTITY, id);
                return;
            }

            if(room < EngineWorld.Rooms.Count)
            {
                var r = EngineWorld.Rooms[room];
                if(ent == EngineWorld.Character)
                {
                    ent.Self.Room = r;
                }
                else if(ent.Self.Room != r)
                {
                    ent.Self.Room?.RemoveEntity(ent);
                    r.AddEntity(ent);
                }
            }
            ent.UpdateRoomPos();

            ent.MoveType = (MoveType) moveType;
            ent.DirFlag = (ENT_MOVE)dirFlag;
        }

        public static int lua_GetEntityMeshCount(uint id)
        {
            var ent = EngineWorld.GetEntityByID(id);

            if (ent == null)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_ENTITY, id);
                return 0;
            }

            return ent.Bf.BoneTags.Count;
        }

        public static void lua_SetEntityMeshswap(uint idDest, uint idSrc)
        {
            var entDest = EngineWorld.GetEntityByID(idDest); // TODO: Add warning if null
            var modelSrc = EngineWorld.GetModelByID(idSrc);

            var meshesToCopy = Math.Min(entDest.Bf.BoneTags.Count, modelSrc.MeshCount);

            for(var i = 0; i < meshesToCopy; i++)
            {
                entDest.Bf.BoneTags[i].MeshBase = modelSrc.MeshTree[i].MeshBase;
                entDest.Bf.BoneTags[i].MeshSkin = modelSrc.MeshTree[i].MeshSkin;
            }
        }

        public static void lua_SetModelMeshReplaceFlag(uint id, int bone, byte flag)
        {
            var sm = EngineWorld.GetModelByID(id);
            if(sm != null)
            {
                if(bone.IsBetween(0, sm.MeshCount - 1))
                {
                    sm.MeshTree[bone].ReplaceMesh = flag;
                }
                else
                {
                    ConsoleInfo.Instance.Warning(Strings.SYSWARN_WRONG_BONE_NUMBER, bone);
                }
            }
            else
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_WRONG_MODEL_ID, id);
            }
        }

        public static void lua_SetModelAnimReplaceFlag(uint id, int bone, byte flag)
        {
            var sm = EngineWorld.GetModelByID(id);
            if (sm != null)
            {
                if (bone.IsBetween(0, sm.MeshCount - 1))
                {
                    sm.MeshTree[bone].ReplaceAnim = flag;
                }
                else
                {
                    ConsoleInfo.Instance.Warning(Strings.SYSWARN_WRONG_BONE_NUMBER, bone);
                }
            }
            else
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_WRONG_MODEL_ID, id);
            }
        }

        public static void lua_CopyMeshFromModelToModel(uint id1, uint id2, int bone1, int bone2)
        {
            var sm1 = EngineWorld.GetModelByID(id1);
            if(sm1 == null)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_WRONG_MODEL_ID, id1);
                return;
            }

            var sm2 = EngineWorld.GetModelByID(id2);
            if (sm2 == null)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_WRONG_MODEL_ID, id2);
                return;
            }

            var tmp1 = bone1.IsBetween(0, sm1.MeshCount - 1);
            var tmp2 = bone2.IsBetween(0, sm2.MeshCount - 1);

            if (tmp1 && tmp2)
            {
                sm1.MeshTree[bone1].MeshBase = sm2.MeshTree[bone2].MeshBase;
                return;
            }
            if(!tmp1)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_WRONG_BONE_NUMBER, bone1);
            }
            if(!tmp2)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_WRONG_BONE_NUMBER, bone2);
            }
        }

        public static void lua_CreateEntityGhosts(uint id)
        {
            var ent = EngineWorld.GetEntityByID(id);
            // TODO: Add warning if null
            if(ent != null && ent.Bf.BoneTags.Count > 0)
            {
                ent.CreateGhosts();
            }
        }

        public static void lua_PushEntityBody(uint id, int bodyNumber, float hForce, float vForce, bool resetFlag)
        {
            var ent = EngineWorld.GetEntityByID(id);
            // TODO: Add warning if null
            // TODO: Check bodyNumber > 0
            if(ent != null && bodyNumber < ent.Bf.BoneTags.Count && ent.Bt.BtBody[bodyNumber] != null && ent.TypeFlags.HasFlag(ENTITY_TYPE.Dynamic))
            {
                var t = ent.Angles.X * RadPerDeg;

                var ang1 = (float)Math.Sin(t);
                var ang2 = (float)Math.Cos(t);

                var angle = new Vector3(-ang1 * hForce, ang2 * hForce, vForce);

                if(resetFlag)
                    ent.Bt.BtBody[bodyNumber].ClearForces();

                ent.Bt.BtBody[bodyNumber].LinearVelocity = angle;
                ent.Bt.BtBody[bodyNumber].AngularVelocity = angle / 1024.0f;
            }
            else
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_CANT_APPLY_FORCE, id);
            }
        }

        public static int lua_SetEntityBodyMass(uint id, int bodyNumber, params float[] masses)
        {
            if(masses.Length == 0)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_WRONG_ARGS, "[entity_id, body_number, (mass / each body mass)]");
                return 0;
            }

            var ent = EngineWorld.GetEntityByID(id);

            bodyNumber = Math.Max(1, bodyNumber);

            var dyn = false;

            if(ent != null && bodyNumber < ent.Bf.BoneTags.Count)
            {
                for(var i = 0; i < bodyNumber; i++)
                {
                    var mass = 0.0f;
                    if (i < masses.Length) mass = masses[i];

                    if(ent.Bt.BtBody[i] != null)
                    {
                        BtEngineDynamicsWorld.RemoveRigidBody(ent.Bt.BtBody[i]);

                        Vector3 inertia;
                        ent.Bt.BtBody[i].CollisionShape.CalculateLocalInertia(mass, out inertia);

                        ent.Bt.BtBody[i].SetMassProps(mass, inertia);

                        ent.Bt.BtBody[i].UpdateInertiaTensor();
                        ent.Bt.BtBody[i].ClearForces();

                        ent.Bt.BtBody[i].CollisionShape.LocalScaling = ent.Scaling;

                        var factor = mass > 0.0f ? Vector3.One : Vector3.Zero;
                        ent.Bt.BtBody[i].LinearFactor = factor;
                        ent.Bt.BtBody[i].AngularFactor = factor;

                        //ent.Bt.BtBody[i].ForceActivationState(ActivationState.DisableDeactivation);

                        //ent.Bt.BtBody[i].CcdMotionThreshold = 32.0f; // disable tunneling effect
                        //ent.Bt.BtBody[i].CcdSweptSphereRadius = 32.0f;

                        BtEngineDynamicsWorld.AddRigidBody(ent.Bt.BtBody[i]);

                        ent.Bt.BtBody[i].Activate();

                        //ent.Bt.BtBody[i].BroadphaseHandle.CollisionFilterGroup = CollisionFilterGroups.AllFilter;
                        //ent.Bt.BtBody[i].BroadphaseHandle.CollisionFilterMask = CollisionFilterGroups.AllFilter;

                        //ent.Self.ObjectType = OBJECT_TYPE.Entity;
                        //ent.Bt.BtBody[i].UserObject = ent.Self;

                        if (mass > 0.0f) dyn = true;
                    }
                }

                if(dyn)
                {
                    ent.TypeFlags |= ENTITY_TYPE.Dynamic;
                }
                else
                {
                    ent.TypeFlags &= ~ENTITY_TYPE.Dynamic;
                }

                ent.UpdateRigidBody(true);
            }
            else
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_WRONG_ENTITY_OR_BODY, id, bodyNumber); // TODO: Replace bodyNumber by ent.Bf.BoneTags.Count
            }

            return 0;
        }

        public static void lua_LockEntityBodyLinearFactor(uint id, int bodyNumber, float vFactor = 1.0f)
        {
            var ent = EngineWorld.GetEntityByID(id);
            // TODO: Add warning if null
            // TODO: Check bodyNumber > 0
            if (ent != null && bodyNumber < ent.Bf.BoneTags.Count && ent.Bt.BtBody[bodyNumber] != null && ent.TypeFlags.HasFlag(ENTITY_TYPE.Dynamic))
            {
                var t = ent.Angles.X * RadPerDeg;
                var ang1 = (float)Math.Sin(t);
                var ang2 = (float)Math.Cos(t);
                var ang3 = Math.Min(1.0f, Math.Abs(vFactor));

                ent.Bt.BtBody[bodyNumber].LinearFactor = new Vector3(Math.Abs(ang1), Math.Abs(ang2), ang3);
            }
            else
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_CANT_APPLY_FORCE, id);
            }
        }

        public static void lua_SetCharacterWeaponModel(uint id, int weaponModel, int state)
        {
            var ent = EngineWorld.GetCharacterByID(id);

            if(ent != null)
            {
                ent.SetWeaponModel(weaponModel, state);
            }
            else
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_ENTITY, id);
            }
        }

        public static int lua_GetCharacterCurrentWeapon(uint id)
        {
            var ent = EngineWorld.GetCharacterByID(id);

            if (ent != null)
            {
                return ent.CurrentWeapon;
            }
            else
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_ENTITY, id);
                return -1;
            }
        }

        public static void lua_SetCharacterCurrentWeapon(uint id, int weapon)
        {
            var ent = EngineWorld.GetCharacterByID(id);

            if (ent != null)
            {
                ent.CurrentWeapon = weapon;
            }
            else
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_NO_ENTITY, id);
            }
        }

        #endregion

        #region Camera functions

        public static void lua_CamShake(float power, float time, uint? id = null)
        {
            if(id != null)
            {
                var ent = EngineWorld.GetEntityByID((uint) id);

                var camPos = Renderer.Camera.Position;

                var dist = ent.Transform.Origin.Distance(camPos);
                dist = dist > TR_CAM_MAX_SHAKE_DISTANCE ? 0 : 1.0f - dist / TR_CAM_MAX_SHAKE_DISTANCE;

                power *= dist;
            }

            if(power > 0.0f)
                Renderer.Camera.Shake(power, time);
        }

        public static void lua_FlashSetup(byte alpha, byte R, byte G, byte B, ushort fadeinSpeed, ushort fadeoutSpeed)
        {
            Gui.FadeSetup(FaderType.Effect, alpha, R, G, B, BlendingMode.Multiply, fadeinSpeed, fadeoutSpeed);
        }

        public static void lua_FlashStart()
        {
            Gui.FadeStart(FaderType.Effect, FaderDir.Timed);
        }

        public static void lua_FadeOut()
        {
            Gui.FadeStart(FaderType.Effect, FaderDir.Out);
        }

        public static void lua_FadeIn()
        {
            Gui.FadeStart(FaderType.Effect, FaderDir.In);
        }

        public static bool lua_FadeCheck()
        {
            return Gui.FadeCheck(FaderType.Black) != FaderStatus.Idle;
        }

        #endregion
        
        #region General gameplay functions

        public static void lua_PlayStream(uint id, byte mask = 0)
        {
            /*if (id < 0)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_WRONG_STREAM_ID);
                return;
            }*/

            Audio.StreamPlay(id, mask); // TODO: return value?
        }

        public static void lua_StopStreams()
        {
            Audio.StopStreams();
        }

        public static void lua_PlaySound(uint id, uint? entID = null)
        {
            //if (id < 0) return;

            if(id > EngineWorld.AudioMap.Count)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_WRONG_SOUND_ID, EngineWorld.AudioMap.Count);
                return;
            }

            var eid = -1;
            if (entID != null)
                eid = (int) entID;
            if (eid < 0 || EngineWorld.GetEntityByID((uint) eid) == null)
                eid = -1;

            TR_AUDIO_SEND result;

            result = eid >= 0 ? Audio.Send(id, TR_AUDIO_EMITTER.Entity, eid) : Audio.Send(id, TR_AUDIO_EMITTER.Global);

            if(result < 0)
            {
                switch(result)
                {
                    case TR_AUDIO_SEND.NoChannel:
                        ConsoleInfo.Instance.Warning(Strings.SYSWARN_AS_NOCHANNEL);
                        break;

                    case TR_AUDIO_SEND.NoSample:
                        ConsoleInfo.Instance.Warning(Strings.SYSWARN_AS_NOSAMPLE);
                        break;
                }
            }
        }

        public static void lua_StopSound(uint id, uint? entID = null)
        {
            if (id > EngineWorld.AudioMap.Count)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_WRONG_SOUND_ID, EngineWorld.AudioMap.Count);
                return;
            }

            var eid = -1;
            if (entID != null)
                eid = (int)entID;
            if (eid < 0 || EngineWorld.GetEntityByID((uint)eid) == null)
                eid = -1;

            var result = eid == -1 ? Audio.Send(id, TR_AUDIO_EMITTER.Global) : Audio.Send(id, TR_AUDIO_EMITTER.Entity, eid);

            if (result < 0)
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_AK_NOTPLAYED, id);
        }

        public static uint lua_GetLevel()
        {
            return GameflowManager.LevelID;
        }

        public static void lua_SetLevel(uint id)
        {
            ConsoleInfo.Instance.Notify(Strings.SYSNOTE_CHANGING_LEVEL, id);

            Game.LevelTransition((ushort)id);
            GameflowManager.Send(GF_OP.LevelComplete, (int) id); // Next level
        }

        public static void lua_SetGame(byte gameID, uint? levelID = null)
        {
            GameflowManager.GameID = gameID;
            if (levelID != null)
                GameflowManager.LevelID = (uint) levelID;

            var str = EngineLua.Call("getTitleScreen", gameID)[0].ToString();
            Gui.FadeAssignPic(FaderType.LoadScreen, str);
            Gui.FadeStart(FaderType.LoadScreen, FaderDir.Out);

            ConsoleInfo.Instance.Notify(Strings.SYSNOTE_CHANGING_GAME, gameID);
            Game.LevelTransition((ushort)GameflowManager.LevelID);
            GameflowManager.Send(GF_OP.LevelComplete, (int) GameflowManager.LevelID);
        }

        public static void lua_LoadMap(string mapName, byte? gameID = null, uint? mapID = null)
        {
            ConsoleInfo.Instance.Notify(Strings.SYSNOTE_LOADING_MAP, mapName);

            if(!string.IsNullOrWhiteSpace(mapName) && mapName != GameflowManager.CurrentLevelPath)
            {
                if(gameID != null)
                {
                    GameflowManager.GameID = (byte) gameID;
                }
                if (mapID != null)
                {
                    GameflowManager.LevelID = (uint) mapID;
                }
                var filePath = EngineLua.GetLoadingScreen(GameflowManager.LevelID);
                Gui.FadeAssignPic(FaderType.LoadScreen, filePath);
                Gui.FadeStart(FaderType.LoadScreen, FaderDir.In);
                Engine.LoadMap(mapName); // TODO: return value?
            }
        }

        #endregion

        #region Flipped (alternate) room functions

        public static void lua_SetFlipState(int group, bool state)
        {
            if(group >= EngineWorld.FlipData.Count)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_WRONG_FLIPMAP_INDEX, group);
                return;
            }

            if(EngineWorld.FlipData[group].Map == 0x1F) // Check flipmap state.
            {
                if(EngineWorld.EngineVersion > Loader.Engine.TR3)
                {
                    foreach (var currentRoom in EngineWorld.Rooms)
                    {
                        if(currentRoom.AlternateGroup == group) // Check if group is valid.
                        {
                            if(state)
                                currentRoom.SwapToAlternate();
                            else
                                currentRoom.SwapToBase();
                        }
                    }
                    
                    EngineWorld.FlipData[group].State = (byte)(state ? 1 : 0);
                }
                else
                {
                    foreach (var currentRoom in EngineWorld.Rooms)
                    {
                        if (state)
                            currentRoom.SwapToAlternate();
                        else
                            currentRoom.SwapToBase();
                    }

                    EngineWorld.FlipData[0].State = (byte)(state ? 1 : 0); // In TR1-3, state is always global.
                }
            }
        }

        public static void lua_SetFlipMap(int group, byte mask, int _op)
        {
            var op = mask > AMASK_OP_XOR ? AMASK_OP_XOR : AMASK_OP_OR;

            if(group >= EngineWorld.FlipData.Count)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_WRONG_FLIPMAP_INDEX, group);
                return;
            }

            if(op == AMASK_OP_XOR)
            {
                EngineWorld.FlipData[group].Map ^= mask;
            }
            else
            {
                EngineWorld.FlipData[group].Map |= mask;
            }
        }

        public static byte lua_GetFlipMap(int group)
        {
            if (group >= EngineWorld.FlipData.Count)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_WRONG_FLIPMAP_INDEX, group);
                return 0;
            }

            return EngineWorld.FlipData[group].Map;
        }

        public static byte lua_GetFlipState(int group)
        {
            if (group >= EngineWorld.FlipData.Count)
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_WRONG_FLIPMAP_INDEX, group);
                return 0;
            }

            return EngineWorld.FlipData[group].State;
        }

        #endregion

        #region Generate UV rotate animations

        public static void lua_genUVRotateAnimations(uint id)
        {
            var model = EngineWorld.GetModelByID(id);

            if (model == null)
                return; // TODO: Add warning if null

            if (model.MeshTree[0].MeshBase.TransparencyPolygons.Count == 0)
                return;
            var firstPolygon = model.MeshTree[0].MeshBase.TransparencyPolygons[0];
            if (firstPolygon.AnimID != 0)
                return;

            var seq = new AnimSeq();

            // Fill up new sequence with frame list

            seq.AnimType = TR_ANIMTEXTURE.Forward;
            seq.FrameLock = false; // by default anim is playing
            seq.UVRotate = true;
            seq.Frames.Resize(16);
            seq.FrameList.Resize(16);
            seq.ReverseDirection = false; // Neede for proper reverse-type start-up.
            seq.FrameRate = 0.025f; // Should be passed as 1 / FPS
            seq.FrameTime = 0.0f; // Reset frame time to initial state
            seq.CurrentFrame = 0; // Reset current frame to zero.
            seq.FrameList[0] = 0;

            var vMin = firstPolygon.Vertices.Min(x => x.TexCoord[1]);
            var vMax = firstPolygon.Vertices.Max(x => x.TexCoord[1]);

            seq.UVRotateMax = 0.5f * (vMax - vMin);
            seq.UVRotateSpeed = seq.UVRotateMax / seq.Frames.Count;

            for (var j = 0; j < seq.Frames.Count; j++)
            {
                seq.Frames[j].TextureIndex = firstPolygon.TexIndex;
                seq.Frames[j].Mat[0] = 1.0f;
                seq.Frames[j].Mat[1] = 0.0f;
                seq.Frames[j].Mat[2] = 0.0f;
                seq.Frames[j].Mat[3] = 1.0f;
                seq.Frames[j].Move[0] = 0.0f;
                seq.Frames[j].Move[1] = -(j * seq.UVRotateSpeed);
            }

            EngineWorld.AnimSequences.Add(seq);

            foreach (var p in model.MeshTree[0].MeshBase.TransparencyPolygons)
            {
                p.AnimID = (ushort) EngineWorld.AnimSequences.Count;
                foreach (var v in p.Vertices)
                {
                    v.TexCoord[1] = vMin + 0.5f * (v.TexCoord[1] - vMin) + seq.UVRotateMax;
                }
            }
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
            LuaLib.LuaAtPanic(state.GetLuaState(), panic);
            // MAYBE NOT NEEDED ANYMORE, NLua handles exceptions by itself
        }

        public void DoFile(string filename)
        {
            state.DoFile(filename);
        }

        public void DoString(string script)
        {
            state.DoString(script);
        }

        public dynamic this[string key] => Get(key);

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

        public void RegisterC(string funcName, object target, MethodInfo m)
        {
            state.RegisterFunction(funcName, target, m);
            state.RegisterFunction(funcName.ToLower(), target, m);
            state.RegisterFunction(funcName.ToUpper(), target, m);
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


        public void ExposeConstants()
        {
            state["MOVE_STATIC_POS"] = (int) MoveType.StaticPos;
            state["MOVE_KINEMATIC"] = (int) MoveType.Kinematic;
            state["MOVE_ON_FLOOR"] = (int) MoveType.OnFloor;
            state["MOVE_WADE"] = (int) MoveType.Wade;
            state["MOVE_QUICKSAND"] = (int) MoveType.Quicksand;
            state["MOVE_ON_WATER"] = (int) MoveType.OnWater;
            state["MOVE_UNDERWATER"] = (int) MoveType.Underwater;
            state["MOVE_FREE_FALLING"] = (int) MoveType.FreeFalling;
            state["MOVE_CLIMBING"] = (int) MoveType.Climbing;
            state["MOVE_MONKEYSWING"] = (int) MoveType.Monkeyswing;
            state["MOVE_WALLS_CLIMB"] = (int) MoveType.WallsClimb;
            state["MOVE_DOZY"] = (int) MoveType.Dozy;

            state["Game"] = new Dictionary<string, int>
            {
                {"I", (int) TRGame.TR1},
                {"I_DEMO", (int) TRGame.TR1Demo},
                {"I_GOLD", (int) TRGame.TR1UnfinishedBusiness},
                {"II", (int) TRGame.TR2},
                {"II_DEMO", (int) TRGame.TR2Demo},
                {"II_GOLD", (int) TRGame.TR2Gold},
                {"III", (int) TRGame.TR3},
                {"III_GOLD", (int) TRGame.TR3Gold},
                {"IV", (int) TRGame.TR4},
                {"IV_DEMO", (int) TRGame.TR4Demo},
                {"V", (int) TRGame.TR5},
                {"Unknown", (int) TRGame.Unknown},
            };

            state["Engine"] = new Dictionary<string, int>
            {
                {"I", (int) Loader.Engine.TR1},
                {"II", (int) Loader.Engine.TR2},
                {"III", (int) Loader.Engine.TR3},
                {"IV", (int) Loader.Engine.TR4},
                {"V", (int) Loader.Engine.TR5},
                {"Unknown", (int) Loader.Engine.Unknown},
            };

            state["ENTITY_TYPE_GENERIC"] = (int) ENTITY_TYPE.Generic;
            state["ENTITY_TYPE_INTERACTIVE"] = (int) ENTITY_TYPE.Interactive;
            state["ENTITY_TYPE_TRIGGER_ACTIVATOR"] = (int) ENTITY_TYPE.TriggerActivator;
            state["ENTITY_TYPE_HEAVYTRIGGER_ACTIVATOR"] = (int) ENTITY_TYPE.HeavyTriggerActivator;
            state["ENTITY_TYPE_PICKABLE"] = (int) ENTITY_TYPE.Pickable;
            state["ENTITY_TYPE_TRAVERSE"] = (int) ENTITY_TYPE.Traverse;
            state["ENTITY_TYPE_TRAVERSE_FLOOR"] = (int) ENTITY_TYPE.TraverseFloor;
            state["ENTITY_TYPE_DYNAMIC"] = (int) ENTITY_TYPE.Dynamic;
            state["ENTITY_TYPE_ACTOR"] = (int) ENTITY_TYPE.Actor;
            state["ENTITY_TYPE_COLLCHECK"] = (int) ENTITY_TYPE.CollCheck;

            state["ENTITY_CALLBACK_NONE"] = (int) ENTITY_CALLBACK.None;
            state["ENTITY_CALLBACK_ACTIVATE"] = (int) ENTITY_CALLBACK.Activate;
            state["ENTITY_CALLBACK_DEACTIVATE"] = (int) ENTITY_CALLBACK.Deactivate;
            state["ENTITY_CALLBACK_COLLISION"] = (int) ENTITY_CALLBACK.Collision;
            state["ENTITY_CALLBACK_STAND"] = (int) ENTITY_CALLBACK.Stand;
            state["ENTITY_CALLBACK_HIT"] = (int) ENTITY_CALLBACK.Hit;
            state["ENTITY_CALLBACK_ROOMCOLLISION"] = (int) ENTITY_CALLBACK.RoomCollision;

            state["COLLISION_TYPE_NONE"] = (int) COLLISION_TYPE.None;
            state["COLLISION_TYPE_STATIC"] = (int) COLLISION_TYPE.Static;
            state["COLLISION_TYPE_KINEMATIC"] = (int) COLLISION_TYPE.Kinematic;
            state["COLLISION_TYPE_DYNAMIC"] = (int) COLLISION_TYPE.Dynamic;
            state["COLLISION_TYPE_ACTOR"] = (int) COLLISION_TYPE.Actor;
            state["COLLISION_TYPE_VEHICLE"] = (int) COLLISION_TYPE.Vehicle;
            state["COLLISION_TYPE_GHOST"] = (int) COLLISION_TYPE.Ghost;

            state["COLLISION_SHAPE_BOX"] = (int) COLLISION_SHAPE.Box;
            state["COLLISION_SHAPE_BOX_BASE"] = (int) COLLISION_SHAPE.BoxBase;
            state["COLLISION_SHAPE_SPHERE"] = (int) COLLISION_SHAPE.Sphere;
            state["COLLISION_SHAPE_TRIMESH"] = (int) COLLISION_SHAPE.Trimesh;
            state["COLLISION_SHAPE_TRIMESH_CONVEX"] = (int) COLLISION_SHAPE.TrimeshConvex;

            state["SECTOR_MATERIAL_MUD"] = (int) SectorMaterial.Mud;
            state["SECTOR_MATERIAL_SNOW"] = (int) SectorMaterial.Snow;
            state["SECTOR_MATERIAL_SAND"] = (int) SectorMaterial.Sand;
            state["SECTOR_MATERIAL_GRAVEL"] = (int) SectorMaterial.Gravel;
            state["SECTOR_MATERIAL_ICE"] = (int) SectorMaterial.Ice;
            state["SECTOR_MATERIAL_WATER"] = (int) SectorMaterial.Water;
            state["SECTOR_MATERIAL_STONE"] = (int) SectorMaterial.Stone;
            state["SECTOR_MATERIAL_WOOD"] = (int) SectorMaterial.Wood;
            state["SECTOR_MATERIAL_METAL"] = (int) SectorMaterial.Metal;
            state["SECTOR_MATERIAL_MARBLE"] = (int) SectorMaterial.Marble;
            state["SECTOR_MATERIAL_GRASS"] = (int) SectorMaterial.Grass;
            state["SECTOR_MATERIAL_CONCRETE"] = (int) SectorMaterial.Concrete;
            state["SECTOR_MATERIAL_OLDWOOD"] = (int) SectorMaterial.OldWood;
            state["SECTOR_MATERIAL_OLDMETAL"] = (int) SectorMaterial.OldMetal;

            state["ANIM_NORMAL_CONTROL"] = (int) AnimControlFlags.NormalControl;
            state["ANIM_LOOP_LAST_FRAME"] = (int) AnimControlFlags.LoopLastFrame;
            state["ANIM_LOCK"] = (int) AnimControlFlags.Lock;

            state["ACT_ACTION"] = (int) ACTIONS.Action;

            TODO
            SDL KEYS;

            state["PARAM_HEALTH"] = (int) CharParameters.Health;
            state["PARAM_AIR"] = (int) CharParameters.Air;
            state["PARAM_STAMINA"] = (int) CharParameters.Stamina;
            state["PARAM_WARMTH"] = (int) CharParameters.Warmth;
            state["PARAM_POISON"] = (int) CharParameters.Poison;
            state["PARAM_EXTRA1"] = (int) CharParameters.Extra1;
            state["PARAM_EXTRA2"] = (int) CharParameters.Extra2;
            state["PARAM_EXTRA3"] = (int) CharParameters.Extra3;
            state["PARAM_EXTRA4"] = (int) CharParameters.Extra4;

            state["PARAM_ABSOLUTE_MAX"] = PARAM_ABSOLUTE_MAX;

            state["BODY_PART_BODY_LOW"] = (int) BODY_PART.BodyLow;
            state["BODY_PART_BODY_UPPER"] = (int) BODY_PART.BodyUpper;
            state["BODY_PART_HEAD"] = (int) BODY_PART.Head;

            state["BODY_PART_LEFT_HAND_1"] = (int) BODY_PART.LeftHand1;
            state["BODY_PART_LEFT_HAND_2"] = (int) BODY_PART.LeftHand2;
            state["BODY_PART_LEFT_HAND_3"] = (int) BODY_PART.LeftHand3;
            state["BODY_PART_RIGHT_HAND_1"] = (int) BODY_PART.RightHand1;
            state["BODY_PART_RIGHT_HAND_2"] = (int) BODY_PART.RightHand2;
            state["BODY_PART_RIGHT_HAND_3"] = (int) BODY_PART.RightHand3;

            state["BODY_PART_LEFT_LEG_1"] = (int) BODY_PART.LeftLeg1;
            state["BODY_PART_LEFT_LEG_2"] = (int) BODY_PART.LeftLeg2;
            state["BODY_PART_LEFT_LEG_3"] = (int) BODY_PART.LeftLeg3;
            state["BODY_PART_RIGHT_LEG_1"] = (int) BODY_PART.RightLeg1;
            state["BODY_PART_RIGHT_LEG_2"] = (int) BODY_PART.RightLeg2;
            state["BODY_PART_RIGHT_LEG_3"] = (int) BODY_PART.RightLeg3;

            state["HAIR_TR1"] = (int) HairType.TR1;
            state["HAIR_TR2"] = (int) HairType.TR2;
            state["HAIR_TR3"] = (int) HairType.TR3;
            state["HAIR_TR4_KID_1"] = (int) HairType.TR4_Kid1;
            state["HAIR_TR4_KID_2"] = (int) HairType.TR4_Kid2;
            state["HAIR_TR4_OLD"] = (int) HairType.TR4_Old;
            state["HAIR_TR5_KID_1"] = (int) HairType.TR5_Kid1;
            state["HAIR_TR5_KID_2"] = (int) HairType.TR5_Kid2;
            state["HAIR_TR5_OLD"] = (int) HairType.TR5_Old;

            state["M_PI"] = Math.PI;

            state["FONTSTYLE_CONSOLE_INFO"] = (int) FontStyle.ConsoleInfo;
            state["FONTSTYLE_CONSOLE_WARNING"] = (int) FontStyle.ConsoleWarning;
            state["FONTSTYLE_CONSOLE_EVENT"] = (int) FontStyle.ConsoleEvent;
            state["FONTSTYLE_CONSOLE_NOTIFY"] = (int) FontStyle.ConsoleNotify;
            state["FONTSTYLE_MENU_TITLE"] = (int) FontStyle.MenuTitle;
            state["FONTSTYLE_MENU_HEADING1"] = (int) FontStyle.MenuHeading1;
            state["FONTSTYLE_MENU_HEADING2"] = (int) FontStyle.MenuHeading2;
            state["FONTSTYLE_MENU_ITEM_ACTIVE"] = (int) FontStyle.MenuItemActive;
            state["FONTSTYLE_MENU_ITEM_INACTIVE"] = (int) FontStyle.MenuItemInactive;
            state["FONTSTYLE_MENU_CONTENT"] = (int) FontStyle.MenuContent;
            state["FONTSTYLE_STATS_TITLE"] = (int) FontStyle.StatsTitle;
            state["FONTSTYLE_STATS_CONTENT"] = (int) FontStyle.StatsContent;
            state["FONTSTYLE_NOTIFIER"] = (int) FontStyle.Notifier;
            state["FONTSTYLE_SAVEGAMELIST"] = (int) FontStyle.SavegameList;
            state["FONTSTYLE_GENERIC"] = (int) FontStyle.Generic;

            state["FONT_PRIMARY"] = (int) FontType.Primary;
            state["FONT_SECONDARY"] = (int) FontType.Secondary;
            state["FONT_CONSOLE"] = (int) FontType.Console;
        }

        public List<string> GetGlobals()
        {
            return state.GetGlobals();
        }

        public void ParseScreen(ScreenInfo sc)
        {
            var scr = statedyn["screen"];
            sc.X = (short) scr["x"];
            sc.Y = (short) scr["y"];
            sc.W = (short) scr["width"];
            sc.Wunit = sc.W / ScreenMeteringResolution;
            sc.H = (short) scr["height"];
            sc.Hunit = sc.H / ScreenMeteringResolution;
            sc.FSflag = (bool) scr["fullscreen"];
            sc.ShowDebugInfo = (bool) scr["debug_info"];
            sc.Fov = (float) scr["fov"];
            sc.Vsync = (bool) scr["vsync"];
        }

        public void ParseRender(RenderSettings rs)
        {
            var ren = statedyn["render"];
            rs.MipmapMode = (uint) ren["mipmap_mode"];
            rs.Mipmaps = (uint) ren["mipmaps"];
            rs.LodBias = (float) ren["lod_bias"];
            rs.Anisotropy = (uint) ren["anisotropy"];
            rs.Antialias = (bool) ren["antialias"];
            rs.AntialiasSamples = (int) ren["antialias_samples"];
            rs.TextureBorder = (int) ren["texture_border"];
            rs.SaveTextureMemory = (bool) ren["save_texture_memory"];
            rs.Zdepth = (int) ren["z_depth"];
            rs.FogEnabled = (bool) ren["fog_enabled"];
            rs.FogStart_depth = (float) ren["fog_start_depth"];
            rs.FogEnd_depth = (float) ren["fog_end_depth"];
            rs.FogColor[0] = (float) ren["fog_color"]["r"] / 255.0f;
            rs.FogColor[1] = (float) ren["fog_color"]["g"] / 255.0f;
            rs.FogColor[2] = (float) ren["fog_color"]["b"] / 255.0f;
            rs.FogColor[3] = 1;

            rs.UseGL3 = (bool) ren["use_gl3"];

            if (rs.Zdepth != 8 && rs.Zdepth != 16 && rs.Zdepth != 24)
                rs.Zdepth = 24;
        }

        public void ParseAudio(AudioSettings au)
        {
            var aud = statedyn["audio"];
            au.MusicVolume = (float) aud["music_volume"];
            au.SoundVolume = (float) aud["sound_volume"];
            au.UseEffects = (bool) aud["use_effects"];
            au.ListenerIsPlayer = (bool) aud["listener_is_player"];
            au.StreamBufferSize = (int) aud["stream_buffer_size"] * 1024;
            if (au.StreamBufferSize <= 0)
                au.StreamBufferSize = 131072; // 128 * 1024
        }

        public void ParseConsole(ConsoleInfo cn)
        {
            var con = statedyn["console"];
            var r = (float) con["background_color"]["r"];
            var g = (float) con["background_color"]["g"];
            var b = (float) con["background_color"]["b"];
            var a = (float) con["background_color"]["a"];
            cn.SetBackgroundColor(r / 255, g / 255, b / 255, a / 255);

            var tmpF = (float) con["spacing"];
            if (tmpF.IsBetween(CON_MIN_LINE_INTERVAL, CON_MAX_LINE_INTERVAL))
                cn.Spacing = tmpF;

            var tmpI = (int) con["line_size"];
            if (tmpI.IsBetween(CON_MIN_LINE_SIZE, CON_MAX_LINE_SIZE))
                cn.LineSize = (ushort)tmpI;

            tmpI = (ushort)con["showing_lines"];
            if (tmpI.IsBetween(CON_MIN_LINES, CON_MAX_LINES))
                cn.VisibleLines = tmpI;

            tmpI = (ushort)con["log_size"];
            if (tmpI.IsBetween(CON_MIN_LOG, CON_MAX_LOG))
                cn.HistorySize = tmpI;

            tmpI = (ushort)con["lines_count"];
            if (tmpI.IsBetween(CON_MIN_LOG, CON_MAX_LOG))
                cn.BufferSize = tmpI;

            var tmpB = (bool) con["show"];
            cn.IsVisible = tmpB;

            tmpF = (float) con["show_cursor_period"];
            cn.BlinkPeriod = tmpF;
        }

        public void ParseControls(ControlSettings cs)
        {
            var con = statedyn["controls"];
            cs.MouseSensitivity = (float) con["mouse_sensitivity"];
            cs.MouseScaleX = (float) con["mouse_scale_x"];
            cs.MouseScaleY = (float) con["mouse_scale_y"];
            cs.UseJoy = (bool) con["use_joy"];
            cs.JoyNumber = (int) con["joy_number"];
            cs.JoyRumble = (bool) con["joy_rumble"];
            cs.JoyAxisMap[(int) AXES.LookX] = (int) con["joy_look_axis_x"];
            cs.JoyAxisMap[(int) AXES.LookY] = (int) con["joy_look_axis_y"];
            cs.JoyAxisMap[(int) AXES.MoveX] = (int) con["joy_move_axis_x"];
            cs.JoyAxisMap[(int) AXES.MoveY] = (int) con["joy_move_axis_y"];
            cs.JoyLookInvertX = (bool) con["joy_look_invert_x"];
            cs.JoyLookInvertY = (bool) con["joy_look_invert_y"];
            cs.JoyLookSensitivity = (float) con["joy_look_sensitivity"];
            cs.JoyLookDeadzone = (short) con["joy_look_deadzone"];
            cs.JoyMoveInvertX = (bool) con["joy_move_invert_x"];
            cs.JoyMoveInvertY = (bool) con["joy_move_invert_y"];
            cs.JoyMoveSensitivity = (float) con["joy_move_sensitivity"];
            cs.JoyMoveDeadzone = (short) con["joy_move_deadzone"];
        }

        public void ParseSystem(SystemSettings ss)
        {
            ss.Logging = (bool) statedyn["system"]["logging"];
        }

        protected void CheckStack()
        {
            ConsoleInfo.Instance.Notify(Strings.SYSNOTE_LUA_STACK_INDEX, LuaCore.LuaGetTop(state.GetLuaState()));
        }

        public NLua.Lua state;

        private dynamic statedyn => (dynamic) state;

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

        /// <summary>
        /// Called when something goes absolutely horribly wrong in Lua, and tries
        /// to produce some debug output. Lua calls abort afterwards, so sending
        /// the output to the internal console is not an option.
        /// </summary>
        private static int panic(LuaState state)
        {
            if(LuaCore.LuaGetTop(state) < 1)
            {
                Console.Error.WriteLine("Fatal lua error (no details provided).");
            }
            else
            {
                uint tmp;
                Console.Error.WriteLine("Fatal lua error: {0}", LuaCore.LuaToLString(state, 1, out tmp));
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

        public void LoopEntity(uint objectID)
        {
            var ent = EngineWorld.GetEntityByID(objectID);
            // TODO: Add warning if null
            if(ent!= null && ent.Active)
            {
                Call("loopEntity", objectID);
            }
        }

        public void ExecEntity(int idCallback, int idObject, int idActivator = -1)
        {
            if (idActivator >= 0)
                Call("execEntity", idCallback, idObject, idActivator);
            else
                Call("execEntity", idCallback, idObject);
        }

        public void ExecEffect(int id, int caller = -1, int operand = -1)
        {
            Call("execFlipeffect", id, caller, operand);
        }

        public void AddKey(int keycode, bool state)
        {
            Call("addKey", keycode, state);
        }

        public static void BindKey(int act, int primary, int? secondary = null)
        {
            if(!act.IsBetween(0, (int)ACTIONS.LastIndex - 1))
            {
                ConsoleInfo.Instance.Warning(Strings.SYSWARN_WRONG_ACTION_NUMBER);
            }
            ControlMapper.ActionMap[act].Primary = primary;
            if (secondary != null)
                ControlMapper.ActionMap[act].Secondary = (int) secondary;
        }

        public bool GetOverridedSamplesInfo(out int numSamples, out int numSounds, out string samplesNameMask)
        {
            var res = Call("getOverridedSamplesInfo", (int) EngineWorld.EngineVersion);
            samplesNameMask = (string) res[0];
            numSounds = (int) res[1];
            numSamples = (int) res[2];

            return numSounds != -1 && numSamples != -1 && samplesNameMask != "NONE";
        }

        public bool GetOverridedSample(int soundID, out int firstSampleNumber, out int samplesCount)
        {
            var res = Call("getOverridedSample", (int) EngineWorld.EngineVersion, GameflowManager.LevelID, soundID);
            firstSampleNumber = (int) res[0];
            samplesCount = (int) res[1];
            return firstSampleNumber != -1 && samplesCount != -1;
        }

        public int GetGlobalSound(int globalSoundID)
        {
            return (int) Call("getGlobalSound", (int) EngineWorld.EngineVersion, globalSoundID)[0];
        }

        public int GetSecretTrackNumber()
        {
            return (int)Call("getSecretTrackNumber", (int)EngineWorld.EngineVersion)[0];
        }

        public int GetNumTracks()
        {
            return (int)Call("getNumTracks", (int)EngineWorld.EngineVersion)[0];
        }

        public bool GetSoundtrack(int trackIndex, out string trackPath, out TR_AUDIO_STREAM_METHOD loadMethod, out TR_AUDIO_STREAM_TYPE streamType)
        {
            var res = Call("getTrackInfo", (int) EngineWorld.EngineVersion, trackIndex);
            trackPath = (string) res[0];
            streamType = (TR_AUDIO_STREAM_TYPE) (int) res[1];
            loadMethod = (TR_AUDIO_STREAM_METHOD) (int) res[2];
            return streamType != TR_AUDIO_STREAM_TYPE.Unknown;
        }

        public string GetLoadingScreen(uint levelIndex)
        {
            return
                ((string)
                    Call("getLoadingScreen", (int) GameflowManager.GameID, (int) GameflowManager.LevelID, levelIndex)[0]).ClampStr(MAX_ENGINE_PATH);
        }

        public string GetString(int stringID)
        {
            return (string) Call("getString", stringID)[0];
        }

        public string GetSysNotify(int stringID)
        {
            return (string)Call("getSysNotify", stringID)[0];
        }

        public static int ParseToken(string data, int index, out string token);

        public static float ParseFloat(string str, int index);

        public static int ParseInt(string str, int index);

        private void registerMainFunctions()
        {
            // Register globals

            state.DoString(CVAR_LUA_TABLE_NAME + " = {}");

            Game.RegisterLUAFunctions(this);

            // Register script functions

            RegisterC("checkStack", this, typeof(ScriptEngine).GetMethod("CheckStack"));
            registerLuaFunc("dumpModel");
            registerLuaFunc("dumpRoom");
            registerLuaFunc("setRoomEnabled");

            registerLuaFunc("playSound");
            registerLuaFunc("stopSound");

            registerLuaFunc("playStream");
            registerLuaFunc("stopStreams");

            registerLuaFunc("setLevel");
            registerLuaFunc("getLevel");

            registerLuaFunc("setGame");
            registerLuaFunc("loadMap");

            registerLuaFunc("camShake");

            registerLuaFunc("fadeOut");
            registerLuaFunc("fadeIn");
            registerLuaFunc("fadeCheck");

            registerLuaFunc("flashSetup");
            registerLuaFunc("flashStart");

            registerLuaFunc("getEngineVersion");

            registerLuaFunc("setFlipMap");
            registerLuaFunc("getFlipMap");
            registerLuaFunc("setFlipState");
            registerLuaFunc("getFlipState");

            registerLuaFunc("setModelCollisionMapSize");
            registerLuaFunc("setModelCollisionMap");
            registerLuaFunc("getAnimCommandTransform");
            registerLuaFunc("setAnimCommandTransform");
            registerLuaFunc("setStateChangeRange");
            registerLuaFunc("setAnimVerticalSpeed");

            registerLuaFunc("addItem");
            registerLuaFunc("removeItem");
            registerLuaFunc("removeAllItems");
            registerLuaFunc("getItemsCount");
            registerLuaFunc("createBaseItem");
            registerLuaFunc("deleteBaseItem");
            registerLuaFunc("printItems");

            registerLuaFunc("canTriggerEntity");
            registerLuaFunc("spawnEntity");
            registerLuaFunc("deleteEntity");
            registerLuaFunc("enableEntity");
            registerLuaFunc("disableEntity");

            registerLuaFunc("isInRoom");
            registerLuaFunc("sameRoom");
            registerLuaFunc("sameSector");
            registerLuaFunc("newSector");
            registerLuaFunc("similarSector");
            registerLuaFunc("getSectorHeight");

            registerLuaFunc("moveEntityGlobal");
            registerLuaFunc("moveEntityLocal");
            registerLuaFunc("moveEntityToSink");
            registerLuaFunc("moveEntityToEntity");
            registerLuaFunc("rotateEntity");
            registerLuaFunc("rotateEntityToEntity");

            registerLuaFunc("getEntityModelID");

            registerLuaFunc("getEntityVector");
            registerLuaFunc("getEntityDirDot");
            registerLuaFunc("getEntityOrientation");
            registerLuaFunc("getEntityDistance");
            registerLuaFunc("getEntityPos");
            registerLuaFunc("setEntityPos");
            registerLuaFunc("getEntityAngles");
            registerLuaFunc("setEntityAngles");
            registerLuaFunc("getEntityScaling");
            registerLuaFunc("setEntityScaling");
            registerLuaFunc("getEntitySpeed");
            registerLuaFunc("setEntitySpeed");
            registerLuaFunc("getEntitySpeedLinear");
            registerLuaFunc("setEntityCollision");
            registerLuaFunc("setEntityCollisionFlags");
            registerLuaFunc("getEntityAnim");
            registerLuaFunc("setEntityAnim");
            registerLuaFunc("setEntityAnimFlag");
            registerLuaFunc("setEntityBodyPartFlag");
            registerLuaFunc("setModelBodyPartFlag");
            registerLuaFunc("getEntityModel");
            registerLuaFunc("getEntityVisibility");
            registerLuaFunc("setEntityVisibility");
            registerLuaFunc("getEntityActivity");
            registerLuaFunc("setEntityActivity");
            registerLuaFunc("getEntityEnability");
            registerLuaFunc("getEntityOCB");
            registerLuaFunc("setEntityOCB");
            registerLuaFunc("getEntityTimer");
            registerLuaFunc("setEntityTimer");
            registerLuaFunc("getEntityFlags");
            registerLuaFunc("setEntityFlags");
            registerLuaFunc("getEntityTypeFlag");
            registerLuaFunc("setEntityTypeFlag");
            registerLuaFunc("getEntityStateFlag");
            registerLuaFunc("setEntityStateFlag");
            registerLuaFunc("getEntityCallbackFlag");
            registerLuaFunc("setEntityCallbackFlag");
            registerLuaFunc("getEntityState");
            registerLuaFunc("setEntityState");
            registerLuaFunc("setEntityRoomMove");
            registerLuaFunc("getEntityMoveType");
            registerLuaFunc("setEntityMoveType");
            registerLuaFunc("getEntityResponse");
            registerLuaFunc("setEntityResponse");
            registerLuaFunc("getEntityMeshCount");
            registerLuaFunc("setEntityMeshswap");
            registerLuaFunc("setModelMeshReplaceFlag");
            registerLuaFunc("setModelAnimReplaceFlag");
            registerLuaFunc("copyMeshFromModelToModel");

            registerLuaFunc("createEntityGhosts");
            registerLuaFunc("setEntityBodyMass");
            registerLuaFunc("pushEntityBody");
            registerLuaFunc("lockEntityBodyLinearFactor");

            registerLuaFunc("getEntityTriggerLayout");
            registerLuaFunc("setEntityTriggerLayout");
            registerLuaFunc("getEntityMask");
            registerLuaFunc("setEntityMask");
            registerLuaFunc("getEntityEvent");
            registerLuaFunc("setEntityEvent");
            registerLuaFunc("getEntityLock");
            registerLuaFunc("setEntityLock");
            registerLuaFunc("getEntitySectorStatus");
            registerLuaFunc("setEntitySectorStatus");

            registerLuaFunc("getEntityActivationOffset");
            registerLuaFunc("setEntityActivationOffset");
            registerLuaFunc("getEntitySectorIndex");
            registerLuaFunc("getEntitySectorFlags");
            registerLuaFunc("getEntitySectorMaterial");
            registerLuaFunc("getEntitySubstanceState");

            registerLuaFunc("addEntityRagdoll");
            registerLuaFunc("removeEntityRagdoll");

            registerLuaFunc("getCharacterParam");
            registerLuaFunc("setCharacterParam");
            registerLuaFunc("changeCharacterParam");
            registerLuaFunc("getCharacterCurrentWeapon");
            registerLuaFunc("setCharacterCurrentWeapon");
            registerLuaFunc("setCharacterWeaponModel");
            registerLuaFunc("getCharacterCombatMode");

            registerLuaFunc("addCharacterHair");
            registerLuaFunc("resetCharacterHair");

            registerLuaFunc("getSecretStatus");
            registerLuaFunc("setSecretStatus");

            registerLuaFunc("getActionState");
            registerLuaFunc("getActionChange");

            registerLuaFunc("genUVRotateAnimation");

            registerLuaFunc("getGravity");
            registerLuaFunc("setGravity");
            registerLuaFunc("dropEntity");
            RegisterC("bind", this, typeof(MainEngine).GetMethod("BindKey"));

            registerLuaFunc("addFont");
            registerLuaFunc("deleteFont");
            registerLuaFunc("addFontStyle");
            registerLuaFunc("deleteFontStyle");
        }

        private void registerLuaFunc(string name)
        {
            var m = typeof (luaFuncs).GetMethod("lua_" + name.FirstLetterUppercase()) ??
                    typeof (luaFuncs).GetMethod("lua_" + name) ??
                    typeof (luaFuncs).GetMethod(name);
            if(m != null) RegisterC(name, m);
        }
    }
}



