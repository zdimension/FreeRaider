using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using static FreeRaider.Global;

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
                PixelFormat.Format24bppRgb);
            GL.ReadPixels(0, 0, width, height, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.UnsignedByte, data.Scan0);
            bmp.UnlockBits(data);
            bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);

            bmp.Save(fname, ImageFormat.Png);

            ScreenshotCount++;
        }
    }
}
