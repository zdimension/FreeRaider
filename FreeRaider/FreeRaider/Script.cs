using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using NLua;

namespace FreeRaider
{
    public partial class Constants
    {
        public const int CVAR_NAME_SIZE = 32;

        public const string CVAR_LUA_TABLE_NAME = "cvars";
    }

    public partial class Global
    {
        public static Script.MainEngine EngineLua;
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


        public void ExposeConstants();

        public List<string> GetGlobals();

        public void ParseScreen(ScreenInfo sc);

        public void ParseRender(RenderSettings rs);

        public void ParseAudio(AudioSettings au);

        public void ParseConsole(ConsoleInfo cn);

        public void ParseControls(ControlSettings cs);

        public void ParseSystem(SystemSettings ss);

        protected void CheckStack();

        private NLua.Lua state;

        private static int print(NLua.Lua state);

        private static int panic(NLua.Lua state);
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

        public static void BindKey(int act, int primary, object secondary);

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
