using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace FreeRaider
{
    public partial class Global
    {
        public static int ScreenshotCount = 0;
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

            var fname = "screen_" + Global.ScreenshotCount + ".png";

            var width = viewport[2];
            var height = viewport[3];
            var bmp = new Bitmap(width, height);
            var data = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly,
                System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            GL.ReadPixels(0, 0, width, height, PixelFormat.Rgba, PixelType.UnsignedByte, data.Scan0);
            bmp.UnlockBits(data);
            bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);

            bmp.Save(fname, ImageFormat.Png);

            Global.ScreenshotCount++;
        }
    }
}
