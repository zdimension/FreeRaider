using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KeraLua;
//using KeraLua;
using static FreeRaider.Global;
//using static KeraLua.Lua;
using LuaCore = KeraLua.Lua;
using static FreeRaider.font_Style;
using NLua;
using Lua = NLua.Lua;

namespace FreeRaider
{
    public static unsafe partial class Global
    {
        public const string CVAR_LUA_TABLE_NAME = "cvars";

        public static void Script_LoadConstants(Lua lua);
        public static bool Script_LuaInit();
        public static int Script_DoLuaFile(Lua lua, string local_path);
        public static void Script_LuaClearTasks();
        public static void Script_LuaRegisterFuncs(Lua lua);

        public static string SC_ParseToken(string data, string token);
        public static float SC_ParseFloat(ref string ch);
        public static int SC_ParseInt(ref string ch);

        public static int lua_print(LuaState lua)
        {
            var top = LuaLib.LuaGetTop(lua);

            if (top == 0)
            {
                Con_AddLine("nil", FONTSTYLE_CONSOLE_EVENT);
            }

            for (var i = 1; i <= top; i++)
            {
                switch (LuaLib.LuaType(lua, i))
                {
                    case LuaTypes.Number:
                    case LuaTypes.String:
                        Con_AddLine(LuaLib.LuaToString(lua, i), FONTSTYLE_CONSOLE_EVENT);
                        break;

                    case LuaTypes.Boolean:
                        Con_AddLine(LuaLib.LuaToBoolean(lua, i) ? ("true") : ("false"), FONTSTYLE_CONSOLE_EVENT);
                        break;

                    case LuaTypes.Function:
                        Con_AddLine("function", FONTSTYLE_CONSOLE_EVENT);
                        break;

                    case LuaTypes.Table:
                        Con_AddLine("table", FONTSTYLE_CONSOLE_EVENT);
                        break;

                    case LuaTypes.Thread:
                        Con_AddLine("thread", FONTSTYLE_CONSOLE_EVENT);
                        break;

                    case LuaTypes.LightUserdata:
                        Con_AddLine("light user data", FONTSTYLE_CONSOLE_EVENT);
                        break;

                    case LuaTypes.Nil:
                        Con_AddLine("nil", FONTSTYLE_CONSOLE_EVENT);
                        break;

                    case LuaTypes.None:
                        Con_AddLine("none", FONTSTYLE_CONSOLE_EVENT);
                        break;

                    default:
                        Con_AddLine("none or nil", FONTSTYLE_CONSOLE_EVENT);
                        break;
                }
            }

            return 0;
        }

        public static void lua_BindKey(int act, int key_id1, int? key_id2 = null)
        {
            if (act < 0 || act >= (int) ACTIONS.ACT_LASTINDEX)
            {
                Con_Warning("wrong action number");
            }

            else
            {
                control_mapper.action_map[act].primary = key_id1;
                if (key_id2 != null)
                    control_mapper.action_map[act].secondary = (int)key_id2;
            }
        }

        public static bool lua_CallAndLog(Lua l, int n, int r, int f)
        {
            return lua_CallWithError(l, n, r, f, __FILE__, __LINE__);
        }
        public static bool lua_CallWithError(Lua lua, int nargs, int nresults, int errfunc, string cfile,
            int cline);

        public static int Script_ParseScreen(Lua lua, ref screen_info_s sc)
        {
            if (lua != null)
            {
                var s = lua.GetTable("screen");

                sc.x = (short) s["x"];

                sc.y = (short) s["y"];

                sc.w = (short) s["width"];

                sc.h = (short) s["height"];

                sc.fullscreen = (uint) s["fullscreen"];

                sc.debug_view_state = (uint) s["debug_view_state"];

                sc.crosshair = (uint) s["crosshair"];

                sc.fov = (float) s["fov"];

                return 1;
            }

            return -1;
        }

        public static int Script_ParseRender(Lua lua, ref render_settings_s rs)
        {
            if (lua != null)
            {
                var r = lua.GetTable("render");

                rs.mipmap_mode = (uint) r["mipmap_mode"];

                rs.mipmaps = (uint) r["mipmaps"];

                rs.lod_bias = (uint) r["lod_bias"];

                rs.anisotropy = (uint) r["anisotropy"];

                rs.antialias = (sbyte) r["antialias"];

                rs.antialias_samples = (sbyte) r["antialias_samples"];

                rs.texture_border = (sbyte) r["texture_border"];

                rs.z_depth = (sbyte) r["z_depth"];

                rs.fog_enabled = (sbyte) r["fog_enabled"];

                rs.fog_start_depth = (float) r["fog_start_depth"];

                rs.fog_end_depth = (float) r["fog_end_depth"];

                var fc = r["fog_color"] as LuaTable;
                if (fc != null)
                {
                    fixed (float* c = rs.fog_color)
                    {
                        c[0] = (float) fc["r"] / 255;

                        c[1] = (float) fc["g"] / 255;

                        c[2] = (float) fc["b"] / 255;

                        c[3] = 1.0f; // Not sure if we need this at all...
                    }
                }

                if (rs.z_depth != 8 && rs.z_depth != 16 && rs.z_depth != 24)
                {
                    rs.z_depth = 24;
                }

                return 1;
            }

            return -1;
        }

        public static int Script_ParseAudio(Lua lua, ref audio_settings_s @as)
        {
            if (lua != null)
            {
                var a = lua.GetTable("audio");

                @as.music_volume = (float) a["music_volume"];

                @as.sound_volume = (float) a["sound_volume"];

                @as.use_effects = (uint) a["use_effects"];

                @as.listener_is_player = (uint) a["listener_is_player"];

                @as.stream_buffer_size = (uint) a["stream_buffer_size"]*1024;

                if (@as.stream_buffer_size <= 0)
                {
                    @as.stream_buffer_size = 128*1024;
                }

                return 1;
            }

            return -1;
        }

        public static int Script_ParseConsole(Lua lua)
        {
            if (lua != null)
            {
                var c = lua.GetTable("console");

                var bc = c["background_color"] as LuaTable;
                if (bc != null)
                {
                    Con_SetBackgroundColor(new[] {(float) bc["r"] / 255.0f, (float)bc["g"] / 255.0f , (float)bc["b"] / 255.0f , (float)bc["a"] / 255.0f });

                    Con_SetLineInterval((float) c["spacing"]);

                    Con_SetMaxLineLenght((ushort) c["line_size"]);

                    Con_SetLinesCount((ushort) c["lines_count"]);

                    Con_SetLogLinesCount((ushort) c["log_size"]);

                    Con_SetShowingLines((ushort) c["showing_lines"]);

                    Con_SetShown((int) c["show"]);

                    Con_SetShowCursorPeriod((float) c["show_cursor_period"]);

                    return 1;
                }
            }

            return -1;
        }

        public static int Script_ParseControls(Lua lua, control_settings_s cs)
        {
            if (lua != null)
            {
                var c = lua.GetTable("controls");

                cs.mouse_sensitivity = (float)c["mouse_sensitivity"];

                cs.use_joy = (sbyte) c["use_joy"];

                cs.joy_number = (sbyte) c["joy_number"];

                cs.joy_rumble = (sbyte) c["joy_rumble"];

                cs.joy_axis_map[(int) AXES.AXIS_LOOK_X] = (sbyte) c["joy_look_axis_x"];

                cs.joy_axis_map[(int) AXES.AXIS_LOOK_Y] = (sbyte) c["joy_look_axis_y"];

                cs.joy_look_invert_x = (sbyte) c["joy_look_invert_x"];

                cs.joy_look_invert_y = (sbyte) c["joy_look_invert_y"];

                cs.joy_look_sensitivity = (float) c["joy_look_sensitivity"];

                cs.joy_look_deadzone = (short) c["joy_look_deadzone"];

                cs.joy_axis_map[(int)AXES.AXIS_MOVE_X] = (sbyte)c["joy_move_axis_x"];

                cs.joy_axis_map[(int)AXES.AXIS_MOVE_Y] = (sbyte)c["joy_move_axis_y"];

                cs.joy_move_invert_x = (sbyte)c["joy_move_invert_x"];

                cs.joy_move_invert_y = (sbyte)c["joy_move_invert_y"];

                cs.joy_move_sensitivity = (float)c["joy_move_sensitivity"];

                cs.joy_move_deadzone = (short)c["joy_move_deadzone"];

                return 1;
            }

            return -1;
        }

        public static bool Script_GetOverridedSamplesInfo(Lua lua, int* num_samples, int* num_sounds,
            ref string sample_name_mask)
        {
            var result = false;

            if (lua != null)
            {
                var c = lua.GetFunction("getOverridedSamplesInfo")?.Call(World_GetVersion());
                if (c != null)
                {
                    sample_name_mask = (string) c[2];
                    *num_sounds = (int) c[1];
                    *num_samples = (int) c[0];

                    if (*num_sounds != -1 && *num_samples != -1 && sample_name_mask != "NONE")
                        result = true;
                }
            }

            return result;
        }

        public static bool Script_GetOverridedSample(Lua lua, int sound_id, int* first_sample_number,
            int* samples_count)
        {
            var result = false;

            if (lua != null)
            {
                var c = lua.GetFunction("getOverridedSample")?.Call(World_GetVersion(), gameflow.getCurrentLevelID(), sound_id);
                if (c != null)
                {
                    *first_sample_number = (int) c[0];
                    *samples_count = (int) c[1];

                    if (*first_sample_number != -1 && *samples_count != -1)
                        result = true;
                }
            }

            return result;
        }

        public static int Script_GetGlobalSound(Lua lua, int global_sound_id)
        {
            var sound_id = 0;

            if (lua != null)
            {
                sound_id = (int)lua.GetFunction("getGlobalSound").Call(World_GetVersion(), global_sound_id)[0];
            }

            return sound_id;
        }

        public static int Script_GetSecretTrackNumber(Lua lua)
        {
            var track_number = 0;

            if (lua != null)
            {
                track_number = (int)lua.GetFunction("getSecretTrackNumber").Call(World_GetVersion())[0];
            }

            return track_number;
        }

        public static int Script_GetNumTracks(Lua lua)
        {
            var sound_id = 0;

            if (lua != null)
            {
                sound_id = (int)lua.GetFunction("getNumTracks").Call(World_GetVersion())[0];
            }

            return sound_id;
        }

        public static bool Script_GetSoundtrack(Lua lua, int track_index, ref string track_path, int file_path_len,
            out TR_AUDIO_STREAM_METHOD load_method, out TR_AUDIO_STREAM_TYPE stream_type)
        {
            var result = false;

            load_method = (TR_AUDIO_STREAM_METHOD) (-1);
            stream_type = (TR_AUDIO_STREAM_TYPE) (-1);

            if (lua != null)
            {
                var c = lua.GetFunction("getTrackInfo")?.Call(World_GetVersion(), track_index);
                if (c != null)
                {
                    track_path = Engine_GetBasePath() + (string) c[0];
                    stream_type = (TR_AUDIO_STREAM_TYPE)(int) c[1];
                    load_method = (TR_AUDIO_STREAM_METHOD)(int) c[2];

                    if ((int)stream_type != -1)
                        result = true;
                }
            }

            return result;
        }

        public static bool Script_GetLoadingScreen(Lua lua, int level_index, ref string pic_path)
        {
            var result = false;

            if (lua != null)
            {
                var c = lua.GetFunction("getLoadingScren")?.Call(gameflow.getCurrentGameID(), gameflow.getCurrentLevelID(), level_index);
                if (c != null)
                {
                    pic_path = (string)c[0];
                    result = true;
                }
            }

            return result;
        }

        public static bool Script_GetString(Lua lua, int string_index, uint string_size, ref string buffer)
        {
            var result = false;

            if (lua != null)
            {
                var c = lua.GetFunction("getString")?.Call(string_index);
                if (c != null)
                {
                    buffer = (string) c[0];
                    result = true;
                }
            }

            return result;
        }

        public static void Script_LoopEntity(Lua lua, int object_id)
        {
            var ent = World_GetEntityByID((uint) object_id);
            if (lua != null && (ent.state_flags & ENTITY_STATE_ACTIVE) != 0)
            {
                lua.GetFunction("loopEntity")?.Call(object_id);
            }
        }

        public static int Script_ExecEntity(Lua lua, int id_callback, int id_object, int id_activator = -1)
        {
            var ret = ENTITY_TRIGGERING_NOT_READY;

            var f = lua.GetFunction("execEntity");
            if (f != null)
            {
                ret = (int)(id_activator >= 0
                    ? f.Call(id_callback, id_object, id_activator)
                    : f.Call(id_callback, id_object))[0];
            }

            return ret;
        }

        public static int Script_DoTasks(Lua lua, float time)
        {
            lua["frame_time"] = time;

            Script_CallVoidFunc(lua, "doTasks");
            Script_CallVoidFunc(lua, "clearKeys");

            return 0;
        }

        public static bool Script_CallVoidFunc(Lua lua, string func_name, bool destroy_after_call = false)
        {
            var f = lua.GetFunction(func_name);

            if (f == null) return false;

            f.Call();

            if (destroy_after_call)
            {
                lua[func_name] = null;
            }

            return true;
        }

        public static void Script_AddKey(Lua lua, int keycode, int state)
        {
            lua.GetFunction("addKey")?.Call(keycode, state);
        }

        // Response constants
        public const int RESP_KILL = 0;
        public const int RESP_VERT_COLLIDE = 1;
        public const int RESP_HOR_COLLIDE = 2;
        public const int RESP_SLIDE = 3;

        // Entity timer constants
        public const int TICK_IDLE = 0;
        public const int TICK_STOPPED = 1;
        public const int TICK_ACTIVE = 2;
    }
}
