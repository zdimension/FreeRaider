using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using FreeRaider.Loader;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace FreeRaider.Game
{
    public class Texture8
    {
        public static int ToGLTexture(tr2_textile8 tex)
        {
            var id = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, id);

            if (Config.TrilinearFiltering)
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                    (int) TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                    (int) TextureMinFilter.LinearMipmapLinear);
            }
            else
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                    (int) TextureMagFilter.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                    (int) TextureMinFilter.Nearest);
            }

            var bmp = new Bitmap(256, 256, PixelFormat.Format32bppRgb);
            for (var x = 0; x < 256; x++)
            {
                for (var y = 0; y < 256; y++)
                {
                    bmp.SetPixel(x, y, (Color) LevelManager.Palette8[tex.Tile[y * 256 + x]]);
                }
            }
            var bData = bmp.LockBits(new Rectangle(0, 0, 256, 256), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 256, 256, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bData.Scan0);

            bmp.UnlockBits(bData);

            return id;
        }
    }

    public class Texture16
    {

        public static int ToGLTexture(tr2_textile16 tex)
        {
            var id = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, id);

            if (Config.TrilinearFiltering)
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                    (int) TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                    (int) TextureMinFilter.LinearMipmapLinear);
            }
            else
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                    (int) TextureMagFilter.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                    (int) TextureMinFilter.Nearest);
            }

            var bmp = new Bitmap(256, 256, PixelFormat.Format32bppRgb);
            for (var x = 0; x < 256; x++)
            {
                for (var y = 0; y < 256; y++)
                {
                    bmp.SetPixel(x, y, (Color) LevelManager.Palette16[tex.Tile[y * 256 + x]]);
                }
            }
            var bData = bmp.LockBits(new Rectangle(0, 0, 256, 256), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 256, 256, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bData.Scan0);

            bmp.UnlockBits(bData);

            return id;
        }
    }

    public class RoomSprite
    {
        public Vector3 Position { get; set; }
        public ushort Texture { get; set; }

        public RoomSprite(Vector3 pos, ushort tex)
        {
            Position = pos;
            Texture = tex;
        }
    }

    public class BoundingBox
    {
        public List<Vector3> Corners { get; set; }
        public List<Vector3> ColorsLine { get; set; }
        public List<Vector3> ColorsPoint { get; set; }
        public List<int> IndexLine { get; set; }
        public List<Vector4> Vertices { get; set; }

        public BoundingBox(Vector3 min, Vector3 max)
        {
            Corners = new List<Vector3>
            {
                min,
                new Vector3(max.X, min.Y, min.Z),
                new Vector3(min.X, max.Y, min.Z),
                new Vector3(min.X, min.Y, max.Z),
                new Vector3(max.X, max.Y, min.Z),
                new Vector3(min.X, max.Y, max.Z),
                new Vector3(max.X, min.Y, max.Z),
                max
            };
        }

        public bool IsInBox(Vector3 v)
        {
            return v.Y >= Corners[0].Y && v.Y <= Corners[7].Y && IsInBoxPlane(v);
        }

        public bool IsInBoxPlane(Vector3 v)
        {
            return v.X >= Corners[0].X && v.X <= Corners[7].X && v.Z >= Corners[0].Z && v.Z <= Corners[7].Z;
        }

        public void Display(Matrix4 m, Vector3 cl, Vector3 cd)
        {
            var s = Vertices.Count;

            foreach(var c in Corners)
            {
                Vertices.Add(Vector4.Transform(new Vector4(c, 1.0f), m));
                ColorsLine.Add(cl);
                ColorsPoint.Add(cd);
            }

            IndexLine.Add(s);
            IndexLine.Add(s + 2);
            IndexLine.Add(s + 2);
            IndexLine.Add(s + 4);
            IndexLine.Add(s + 4);
            IndexLine.Add(s + 1);
            IndexLine.Add(s + 1);
            IndexLine.Add(s + 6);
            IndexLine.Add(s + 6);
            IndexLine.Add(s + 7);
            IndexLine.Add(s + 7);
            IndexLine.Add(s + 5);
            IndexLine.Add(s + 5);
            IndexLine.Add(s + 3);
            IndexLine.Add(s + 3);
            IndexLine.Add(s);
            IndexLine.Add(s);
            IndexLine.Add(s + 1);
            IndexLine.Add(s + 4);
            IndexLine.Add(s + 7);
            IndexLine.Add(s + 6);
            IndexLine.Add(s + 3);
            IndexLine.Add(s + 5);
            IndexLine.Add(s + 2);
        }
    }

    public class Portal
    {
        public int AdjoiningRoom { get; set; }
        public BoundingBox BoundingBox { get; private set; }
        public BoundingBox BoundingBoxNormal { get; private set; }
        public bool ShowBoundingBox { get; set; }
    }
}
