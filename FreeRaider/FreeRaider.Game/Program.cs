using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace FreeRaider.Game
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            using (var game = new GameWindow())
            {
                game.Load += (sender, e) => 
                {
                    game.VSync = VSyncMode.On;
                    LevelManager.SetLevel(args[0]);
                };
                game.Resize += (sender, e) => { GL.Viewport(0, 0, game.Width, game.Height); };
                game.UpdateFrame += (sender, e) =>
                {
                    if (game.Keyboard[Key.Escape])
                    {
                        game.Exit();
                    }
                };
                game.RenderFrame += (sender, e) =>
                {
                    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                    GL.MatrixMode(MatrixMode.Projection);
                    GL.LoadIdentity();
                    GL.Ortho(-1.0, 1.0, -1.0, 1.0, 0.0, 4.0);

                    GL.Begin(PrimitiveType.Triangles);

                    GL.Color3(Color.MidnightBlue);
                    GL.Vertex2(-1.0f, 1.0f);
                    GL.Color3(Color.SpringGreen);
                    GL.Vertex2(0.0f, -1.0f);
                    GL.Color3(Color.Ivory);
                    GL.Vertex2(1.0f, 1.0f);

                    GL.End();

                    game.SwapBuffers();
                };
                game.Run(60.0);
            }
        }
    }
}