using System;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace FreeRaider
{
    public partial class Constants
    {
        public const bool NO_AUDIO = false;
    }

    class Program
    {
        public static bool Done = false;

        public static float TimeScale = 1.0f;

        [STAThread]
        static void Main(string[] args)
        {
            using (var game = new GameWindow())
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
            }
        }
    }
}
