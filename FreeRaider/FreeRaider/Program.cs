using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Platform;
using SDL2;
using static SDL2.SDL;
using static SDL2.SDL_image;

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
        public static uint texture = 0;
        [STAThread]
        public static unsafe void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");

            /*var video_flags = SDL_WindowFlags.SDL_WINDOW_OPENGL | SDL_WindowFlags.SDL_WINDOW_MOUSE_FOCUS |
                             SDL_WindowFlags.SDL_WINDOW_INPUT_FOCUS;
            var sdl_window = SDL_CreateWindow("FreeRaider",100, 100, 720,
                400, video_flags);
            var sdl_gl_context = SDL_GL_CreateContext(sdl_window);
            SDL_GL_MakeCurrent(sdl_window, sdl_gl_context);
            var TKWindow = Utilities.CreateSdl2WindowInfo(sdl_window);
            var GLContext = new GraphicsContext(new ContextHandle(sdl_gl_context),
                SDL_GL_GetProcAddress, // implement GetAddress via SDL
                () => new ContextHandle(SDL_GL_GetCurrentContext()));
            GLContext.MakeCurrent(TKWindow);

            var flags = SDL_image.IMG_InitFlags.IMG_INIT_JPG | SDL_image.IMG_InitFlags.IMG_INIT_PNG;
            var init = IMG_Init(flags);

            var texture_ = IMG_Load("resource/graphics/legal.png");
            var texture = (SDL_Surface*)texture_;
            var fmt = (*(texture->format));
            /*var rend = SDL.SDL_CreateRenderer(sdl_window, -1, SDL_RendererFlags.SDL_RENDERER_ACCELERATED);
            var texture__ = SDL.SDL_CreateTextureFromSurface(rend, texture_);
            var texture___ = (SDL_Texture*)SDL.SDL_CreateTextureFromSurface(rend, texture_);
            SDL.SDL_FreeSurface(texture_);*/

            /*SDL.SDL_RenderCopy(rend, texture__, IntPtr.Zero, IntPtr.Zero);
            SDL.SDL_RenderPresent(rend);*/

            /*var textgl = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, textgl);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, texture->w, texture->h, 0, PixelFormat.Rgb, PixelType.UnsignedByte, (IntPtr)texture->pixels);
            //SDL.SDL_FreeSurface(texture_);

            GL.BindTexture(TextureTarget.Texture2D, textgl);
            GL.Begin(PrimitiveType.Quads);

            GL.TexCoord2(0, 0);
            GL.Vertex3(0, 0, 0);

            GL.TexCoord2(1, 0);
            GL.Vertex3(256, 0, 0);

            GL.TexCoord2(1, 1);
            GL.Vertex3(256, 256, 0);

            GL.TexCoord2(0, 1);
            GL.Vertex3(0, 256, 0);

            GL.End();

            SDL.SDL_GL_SwapWindow(sdl_window);

            Console.ReadLine();
            return;*/
            // SDL
            Engine.Start();

            // Entering main loop.
            var sw = new Stopwatch();
            sw.Start();

            while(!Global.Done)
            {
                var delta = sw.Elapsed.TotalSeconds;

                Engine.Frame((float)(delta * Global.TimeScale));
                Engine.Display();
            }
            /*int texture = 0;
            using (var game = new GameWindow())
            {
                game.Load += (sender, e) =>
                {
                    // setup settings, load textures, sounds
                    //Engine.Start();

                    var bmp = (Bitmap)Image.FromFile("resource/graphics/legal.png");
                    var textureFormat = PixelFormat.Bgra;
                    var colorDepth = PixelInternalFormat.Rgba;

                    // Drop previously assigned texture, if exists

                    // Have OpenGL generate a texture object handle for us
                    texture = GL.GenTexture();

                    // Bind the texture object
                    GL.BindTexture(TextureTarget.Texture2D, texture);

                    // Set the texture's stretching properties
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

                    // Edit the texture object's image data using the information SDL_Surface gives us
                    var data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly,
                        System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                        PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

                    bmp.UnlockBits(data);
                };

                game.Resize += (sender, e) =>
                {
                    GL.Viewport(0, 0, game.Width, game.Height);
                };

                game.UpdateFrame += (sender, e) =>
                {
                    // add game logic, input handling
                    if (Global.Done)
                    {
                        game.Exit();
                    }
                };

                game.RenderFrame += (sender, e) =>
                {
                    // render graphics
                    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                    GL.MatrixMode(MatrixMode.Projection);
                    GL.PushMatrix();
                    GL.LoadIdentity();

                    GL.Ortho(0, 800, 0, 600, -1, 1);

                    GL.MatrixMode(MatrixMode.Modelview);
                    GL.PushMatrix();
                    GL.LoadIdentity();

                    GL.Disable(EnableCap.Lighting);

                    GL.Enable(EnableCap.Texture2D);

                    GL.BindTexture(TextureTarget.Texture2D, texture);

                    GL.Begin(BeginMode.Quads);

                    GL.TexCoord2(0, 0);
                    GL.Vertex3(0, 0, 0);

                    GL.TexCoord2(1, 0);
                    GL.Vertex3(256, 0, 0);

                    GL.TexCoord2(1, 1);
                    GL.Vertex3(256, 256, 0);

                    GL.TexCoord2(0, 1);
                    GL.Vertex3(0, 256, 0);

                    GL.End();

                    GL.Disable(EnableCap.Texture2D);
                    GL.PopMatrix();

                    GL.MatrixMode(MatrixMode.Projection);
                    GL.PopMatrix();

                    GL.MatrixMode(MatrixMode.Modelview);



                    game.SwapBuffers();
                };

                // Run the game at 60 updates per second
                game.Run(60.0);
            }*/

            // Main loop interrupted; shutting down.

            sw.Stop();
            Engine.Shutdown(0);
            Environment.Exit(0);
        }
    }
}
