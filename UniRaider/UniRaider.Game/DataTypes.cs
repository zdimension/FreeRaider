using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using UniRaider.Loader;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace UniRaider.Game
{
    public class Texture8
    {
        public static int ToGLTexture(tr2_textile8 tex)
        {
            var id = GL.GenTexture();
            var bmp = new Bitmap(256, 256, PixelFormat.Format24bppRgb);
            for (var x = 0; x < 256; x++)
            {
                for (var y = 0; y < 256; y++)
                {
                    bmp.SetPixel(x, y, (Color)LevelManager.Palette8[tex.Tile[y * 256 + x]]);
                }
            }
            var bData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bData.Width, bData.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bData.Scan0);

            bmp.UnlockBits(bData);

            return id;
        }
    }
}
