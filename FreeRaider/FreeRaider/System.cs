using System;
using System.IO;

namespace FreeRaider
{
    public partial class Constants
    {
        public const string LOG_FILENAME = "debug.log";

        public const string LUA_LOG_FILENAME = "lua.log";

        public const string GL_LOG_FILENAME = "gl.log";
    }

    public class SystemSettings
    {
        public bool Logging;
    }

    public class ScreenInfo
    {
        public short X;

        public short Y;

        public short W;

        public float Wunit; // Metering unit

        public short H;

        public float Hunit; // Metering unit

        public float Fps;

        public float Fov;

        public float ScaleFactor;

        public bool FSflag;

        public bool ShowDebugInfo;

        public bool Vsync;
    }

    public partial class Global
    {
        public static ScreenInfo ScreenInfo;

        public static SystemSettings SystemSettings;

        public static TextLine SystemFps;
    }

    public class Sys
    {
        public static void Printf(string fmt, params object[] args)
        {
            Console.Error.WriteLine(Helper.Format(fmt, args));
        }

        public static void Init()
        {
            Global.SystemFps = new TextLine
            {
                Text = "",
                X = 10.0f,
                Y = 10.0f,
                Xanchor = HorizontalAnchor.Right,
                Yanchor = VerticalAnchor.Bottom,
                FontID = FontType.Primary,
                StyleID = FontStyle.MenuTitle,
                Show = true
            };

            Gui.AddLine(Global.SystemFps);
        }

        public static void InitGlobals()
        {
            Global.ScreenInfo = new ScreenInfo
            {
                X = 50,
                Y = 20,
                W = 800,
                H = 600,
                FSflag = false,
                ShowDebugInfo = false,
                Fov = 75.0f
            };

            Global.SystemSettings = new SystemSettings
            {
                Logging = true
            };
        }

        public static void Destroy()
        {
            Global.SystemFps.Show = false;
            Global.SystemFps.Text = "";
        }

        public static void Error(string error, params object[] args)
        {
            DebugLog(Constants.LOG_FILENAME, "System error: {0}", error, args);
            Engine.Shutdown(1);
        }

        public static void Warn(string warning, params object[] args)
        {
            DebugLog(Constants.LOG_FILENAME, "Warning: {0}", warning, args);
        }

        public static void DebugLog(string file, string fmt, params object[] args)
        {
            if (!Global.SystemSettings.Logging) return;

            Stream fp;
            try
            {
                fp = File.Open(file, FileMode.Append);
            }
            catch
            {
                fp = Console.OpenStandardError();
            }
            using (var sw = new StreamWriter(fp))
            {
                sw.WriteLine(Helper.Format(fmt, args));
            }
            fp.Close();
        }

        public static void LogCurrPlace()
        {
            DebugLog(Constants.LOG_FILENAME, "\"{0}\" str = {1}", Helper.__FILE__, Helper.__LINE__);
        }

        public static void extError(string error, params object[] args)
        {
            LogCurrPlace();
            Error(error, args);
        }

        public static void extWarn(string warning, params object[] args)
        {
            LogCurrPlace();
            Warn(warning, args);
        }
    }
}
