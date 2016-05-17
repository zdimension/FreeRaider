using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace FreeRaider
{
    public partial class Constants
    {
        public const bool NO_AUDIO = false;

        public const bool REDIRECT_LOG = true;

        public const bool AUDIO_OPENAL_FLOAT = false;
    }

    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");

            // SDL
            Engine.Start();

            // Entering main loop.

            //var prev_time = DateTime.Now;
            var sw = new Stopwatch();
            sw.Start();

            while(!Global.Done)
            {
                //var now = DateTime.Now;
                //var delta = ((now - prev_time).TotalMilliseconds / 1000) / 1.0e6;
                //var delta = (now - prev_time).TotalSeconds;
                //prev_time = now;

                var delta = sw.Elapsed.TotalSeconds;

                Engine.Frame((float)(delta * Global.TimeScale));
                Engine.Display();
            }

            // Main loop interrupted; shutting down.

            sw.Stop();
            Engine.Shutdown(0);
            Environment.Exit(0);
        }
    }
}
