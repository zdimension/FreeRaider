using System;
using System.IO;
using JetBrains.Annotations;
using static FreeRaider.Constants;
using static FreeRaider.Global;

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
        public static ScreenInfo ScreenInfo = new ScreenInfo();

        public static SystemSettings SystemSettings = new SystemSettings();

        public static TextLine SystemFps;
    }

    public class Sys
    {
        [StringFormatMethod("fmt")]
        public static void Printf(string fmt, params object[] args)
        {
            Console.Error.WriteLine(Helper.Format(fmt, args));
        }

        public static void Init()
        {
            SystemFps = new TextLine
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

            Gui.AddLine(SystemFps);
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
            SystemFps.Show = false;
            SystemFps.Text = "";
        }

        [StringFormatMethod("error")]
        public static void Error(string error, params object[] args)
        {
            DebugLog(LOG_FILENAME, "System error: {0}", Helper.Format(error, args));
            Engine.Shutdown(1);
        }

        [StringFormatMethod("warning")]
        public static void Warn(string warning, params object[] args)
        {
            DebugLog(LOG_FILENAME, "Warning: {0}", Helper.Format(warning, args));
        }

        [StringFormatMethod("fmt")]
        public static void DebugLog(string file, string fmt, params object[] args)
        {
            if (!Global.SystemSettings.Logging) return;

            var str = Helper.Format(fmt, args);
            if(REDIRECT_LOG) Console.WriteLine(str);

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
                sw.WriteLine(str);
            }
            fp.Close();
        }

        public static void LogCurrPlace()
        {
            DebugLog(LOG_FILENAME, "\"{0}\" str = {1}", Helper.__FILE__, Helper.__LINE__);
        }

        [StringFormatMethod("error")]
        public static void extError(string error, params object[] args)
        {
            LogCurrPlace();
            Error(error, args);
        }

        [StringFormatMethod("warning")]
        public static void extWarn(string warning, params object[] args)
        {
            LogCurrPlace();
            Warn(warning, args);
        }
    }
}
