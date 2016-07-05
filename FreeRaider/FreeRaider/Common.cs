using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;
using SDL2;
using static FreeRaider.Global;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace FreeRaider
{
    public partial class Global
    {
        public static int ScreenshotCount;
    }

    public class Common
    {
        public static void Init()
        {

        }

        public static void Destroy()
        {

        }

        public static void TakeScreenshot()
        {
            var viewport = new int[4];
            GL.GetInteger(GetPName.Viewport, viewport);

            var fname = "screen_" + ScreenshotCount + ".png";

            var width = viewport[2];
            var height = viewport[3];
            var bmp = new Bitmap(width, height);
            var data = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.ReadPixels(0, 0, width, height, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            bmp.UnlockBits(data);
            bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);

            bmp.Save(fname, ImageFormat.Png);

            /*var pixels = new byte[strSize * height];
            GL.ReadPixels(0, 0, width, height, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
            var buf = new byte[strSize];
            fixed (byte* pixData = pixels)
            {
                fixed (byte* bufData = buf)
                {
                    for (var h = 0; h < height / 2; h++)
                    {
                        Helper.memcpy(bufData, &pixData[h * strSize], strSize);
                        Helper.memcpy(&pixData[h * strSize], &pixData[(height - h - 1) * strSize], strSize);
                        Helper.memcpy(&pixData[(height - h - 1) * strSize], bufData, strSize);
                    }


                    var surface =
                        (SDL.SDL_Surface*)
                            SDL.SDL_CreateRGBSurfaceFrom(IntPtr.Zero, width, height, 32, strSize, 0x000000FF,
                                0x00000FF00,
                                0x00FF0000, 0xFF000000);
                    surface->format->format = SDL.SDL_PIXELFORMAT_RGBA8888;
                    var tmp = Marshal.AllocHGlobal(pixels.Length);
                    Marshal.Copy(pixels, 0, tmp, pixels.Length);
                    surface->pixels = (byte*)tmp;
                    SDL_image.IMG_SavePNG((IntPtr) surface, fname);

                    surface->pixels = (byte*) 0;
                    Marshal.FreeHGlobal(tmp);
                    SDL.SDL_FreeSurface((IntPtr) surface);
                }
            }*/
            ScreenshotCount++;
        }
    }
}
