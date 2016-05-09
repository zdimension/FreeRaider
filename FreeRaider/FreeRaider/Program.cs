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

            var prev_time = DateTime.Now;

            while(!Global.Done)
            {
                var now = DateTime.Now;
                //var delta = ((now - prev_time).TotalMilliseconds / 1000) / 1.0e6;
                var delta = (now - prev_time).TotalSeconds;
                prev_time = now;

                Engine.Frame((float)(delta * Global.TimeScale));
                Engine.Display();
            }

            // Main loop interrupted; shutting down.

            Engine.Shutdown(0);
            Environment.Exit(0);

            /*using (var game = new GameWindow())
            {
                game.Load += (sender, e) =>
                {
                    game.VSync = VSyncMode.On;
                    Engine.Start();
                };

                game.Resize += (sender, e) =>
                {
                    GL.Viewport(0, 0, game.Width, game.Height);
                };

                game.UpdateFrame += (sender, e) =>
                {
                    if (game.Keyboard[Key.Escape] || Done)
                    {
                        game.Exit();
                    }
                };
                var sp = new Stopwatch();
                game.RenderFrame += (sender, e) =>
                {
                    var delta = (float)sp.ElapsedMicroseconds() / 1.0e6f;
                    sp.Restart();

                    Engine.Frame(delta * TimeScale);
                    Engine.Display();
                };

                sp.Start();
                game.Run(60.0f);

                Engine.Shutdown(0);
                Environment.Exit(0);
            }*/
        }
    }
}
