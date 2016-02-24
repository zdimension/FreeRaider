using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using SharpFont;
using Encoding = SharpFont.Encoding;

namespace FreeRaider
{
    public class CharInfo
    {
        public uint TexIndex;

        public int Width;

        public int Height;

        public int Left;

        public int Top;

        public float TexX0;

        public float TexY0;

        public float TexX1;

        public float TexY1;

        public float AdvanceX;

        public float AdvanceY;
    }

    public class FontTexture
    {
        public SharpFont.Library FTLibrary;

        public SharpFont.Face FTFace = null;

        public ushort FontSize;

        public List<CharInfo> Glyphs = new List<CharInfo>();

        public ushort GLRealTexIndexesCount;

        public uint[] GLTexIndexes = null;

        public int GLMaxTexWidth;

        public int GLTexWidth;

        public float[] GLFontColor = new float[4];

        ~FontTexture()
        {
            if(GLTexIndexes.Length > 0)
            {
                GL.DeleteTextures(GLTexIndexes.Length, GLTexIndexes);
            }
        }
    }

    public class GLF
    {
        public static FontTexture CreateFont(Library ftLibrary, string filename, ushort fontSize)
        {
            if(ftLibrary != null)
            {
                var glf = new FontTexture();

                Face face = null;
                try
                {
                    face = new Face(ftLibrary, filename, 0);
                }
                catch
                {
                    return null;
                }
                glf.FTFace = face;

                glf.Glyphs.Resize(glf.FTFace.GlyphCount);

                glf.GLMaxTexWidth = GL.GetInteger(GetPName.MaxTextureSize);
                glf.GLTexWidth = glf.GLMaxTexWidth;
                glf.GLTexIndexes = new uint[0];
                glf.GLRealTexIndexesCount = 0;
                glf.GLFontColor = new [] {0.0f, 0.0f, 0.0f, 1.0f};

                Resize(glf, fontSize);
                glf.FTFace.SelectCharmap(Encoding.Unicode);

                return glf;
            }

            return null;
        }

        public static FontTexture CreateFontMem(Library ftLibrary, byte[] faceData, ushort fontSize)
        {
            if (ftLibrary != null)
            {
                var glf = new FontTexture();

                Face face = null;
                try
                {
                    face = new Face(ftLibrary, faceData, 0);
                }
                catch
                {
                    return null;
                }
                glf.FTFace = face;

                glf.Glyphs.Resize(glf.FTFace.GlyphCount);

                glf.GLMaxTexWidth = GL.GetInteger(GetPName.MaxTextureSize);
                glf.GLTexWidth = glf.GLMaxTexWidth;
                glf.GLTexIndexes = new uint[0];
                glf.GLRealTexIndexesCount = 0;
                Resize(glf, fontSize);
                glf.FTFace.SelectCharmap(Encoding.Unicode);

                return glf;
            }

            return null;
        }

        public static void Resize(FontTexture glf, ushort fontSize)
        {
            if (glf?.FTFace != null)
            {
                var padding = 2;
                int x, y;
                int i, ii, i0 = 0;

                // clear old atlas, if exists
                if (glf.GLTexIndexes.Length > 0)
                {
                    GL.DeleteTextures(glf.GLTexIndexes.Length, glf.GLTexIndexes);
                }
                glf.GLTexIndexes = new uint[0];
                glf.GLRealTexIndexesCount = 0;

                // resize base font
                glf.FontSize = fontSize;
                glf.FTFace.SetCharSize(fontSize << 6, fontSize << 6, 0, 0);

                // calculate texture atlas size
                var charsInRow = 1 + (int) Math.Sqrt(glf.Glyphs.Count);
                glf.GLTexWidth = Math.Min(Helper.NextPowerOf2((fontSize + padding) * charsInRow), glf.GLMaxTexWidth);

                // create new atlas
                charsInRow = glf.GLTexWidth / (fontSize + padding);
                var charsInColumn = glf.Glyphs.Count / charsInRow + 1;
                Array.Resize(ref glf.GLTexIndexes, charsInColumn * (fontSize + padding) / glf.GLTexWidth + 1);
                GL.GenTextures(glf.GLTexIndexes.Length, glf.GLTexIndexes);

                var bufferSize = glf.GLTexWidth * glf.GLTexWidth * sizeof (byte);
                var buffer = new byte[bufferSize];

                for (i = 0, x = 0, y = 0; i < glf.Glyphs.Count; i++)
                {
                    GlyphSlot g;
                    glf.Glyphs[i].TexIndex = 0;

                    // load glyph image into the slot (erase previous one)
                    try
                    {
                        glf.FTFace.LoadGlyph((uint) i, LoadFlags.Render, LoadTarget.Normal);
                    }
                    catch
                    {
                        continue;
                    }

                    // convert to an anti-aliased bitmap
                    try
                    {
                        glf.FTFace.Glyph.RenderGlyph(RenderMode.Normal);
                    }
                    catch
                    {
                        continue;
                    }

                    g = glf.FTFace.Glyph;
                    glf.Glyphs[i].Width = g.Bitmap.Width;
                    glf.Glyphs[i].Height = g.Bitmap.Rows;
                    glf.Glyphs[i].AdvanceX = g.Advance.X.ToSingle();
                    glf.Glyphs[i].AdvanceY = g.Advance.Y.ToSingle();
                    glf.Glyphs[i].Left = g.BitmapLeft;
                    glf.Glyphs[i].Top = g.BitmapTop;

                    if (g.Bitmap.Width == 0 || g.Bitmap.Rows == 0)
                    {
                        continue;
                    }

                    if (g.Bitmap.Width + x > glf.GLTexWidth)
                    {
                        x = 0;
                        y += glf.FontSize + padding;
                        if (glf.FontSize + y > glf.GLTexWidth)
                        {
                            GL.BindTexture(TextureTarget.Texture2D, glf.GLTexIndexes[glf.GLRealTexIndexesCount]);
                            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                                (int) TextureMagFilter.Linear);
                            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                                (int) TextureMinFilter.Linear);
                            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS,
                                (int) TextureWrapMode.ClampToEdge);
                            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT,
                                (int) TextureWrapMode.ClampToEdge);
                            GL.TexImage2D(TextureTarget.Texture2D, 0, (PixelInternalFormat) PixelFormat.Red,
                                glf.GLTexWidth, glf.GLTexWidth, 0, (PixelFormat) PixelInternalFormat.R8,
                                PixelType.UnsignedByte, buffer);
                            for (var ii2 = i0; ii2 < i; ii2++)
                            {
                                glf.Glyphs[ii2].TexX0 /= glf.GLTexWidth;
                                glf.Glyphs[ii2].TexX1 /= glf.GLTexWidth;
                                glf.Glyphs[ii2].TexY0 /= glf.GLTexWidth;
                                glf.Glyphs[ii2].TexY1 /= glf.GLTexWidth;
                            }
                            buffer = new byte[bufferSize];
                            y = 0;
                            i0 = i;
                            glf.GLRealTexIndexesCount++;
                        }
                    }

                    glf.Glyphs[i].TexX0 = x;
                    glf.Glyphs[i].TexY0 = y;
                    glf.Glyphs[i].TexX1 = x + g.Bitmap.Width;
                    glf.Glyphs[i].TexY1 = y + g.Bitmap.Rows;

                    glf.Glyphs[i].TexIndex = glf.GLTexIndexes[glf.GLRealTexIndexesCount];
                    for (var xx = 0; xx < g.Bitmap.Width; xx++)
                    {
                        for (var yy = 0; yy < g.Bitmap.Rows; yy++)
                        {
                            buffer[(y + yy) * glf.GLTexWidth + (x + xx)] = g.Bitmap.BufferData[yy * g.Bitmap.Width + xx];
                        }
                    }

                    x += g.Bitmap.Width + padding;
                }

                GL.BindTexture(TextureTarget.Texture2D, glf.GLTexIndexes[glf.GLRealTexIndexesCount]);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                    (int)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                    (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS,
                    (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT,
                    (int)TextureWrapMode.ClampToEdge);
                charsInColumn = Helper.NextPowerOf2(y + fontSize + padding);
                GL.TexImage2D(TextureTarget.Texture2D, 0, (PixelInternalFormat)PixelFormat.Red,
                    glf.GLTexWidth, charsInColumn, 0, PixelFormat.Red, 
                    PixelType.UnsignedByte, buffer);

                for (ii = i0; ii < glf.Glyphs.Count; ii++)
                {
                    glf.Glyphs[ii].TexX0 /= glf.GLTexWidth;
                    glf.Glyphs[ii].TexX1 /= glf.GLTexWidth;
                    glf.Glyphs[ii].TexY0 /= glf.GLTexWidth;
                    glf.Glyphs[ii].TexY1 /= glf.GLTexWidth;
                }
                buffer = new byte[bufferSize];
                glf.GLRealTexIndexesCount++;
            }
        }

        public static void Reface(FontTexture glf, string filename, ushort fontSize)
        {
            Face face;
            try
            {
                face = new Face(glf.FTLibrary, filename, 0);
            }
            catch
            {
                return;
            }
            glf.FTFace = face;
            Resize(glf, fontSize);
        }

        public static float GetStringLen(FontTexture glf, string text, int n)
        {
            var x = 0.0f;

            if (glf?.FTFace != null)
            {
                var c
            }

            return x;
        }

        public static float GetAscender(FontTexture glf)
        {
            if(glf.FontSize == 0 || glf.FTFace == null)
            {
                return 0.0f;
            }

            return glf.FTFace.Ascender / 64.0f;
        }

        public static ushort GetFontSize(FontTexture glf)
        {
            if(glf?.FTFace == null)
            {
                return 0;
            }

            return glf.FontSize;
        }

        public static void GetStringBB(FontTexture glf, string text, int n, float x0, float y0, float x1, float y1);

        public static void RenderStr(FontTexture glf, float x, float y, string text); // UTF-8
    }

    public partial class Helper
    {
        public static uint UTF8StrLen(string str);

        public static byte UTF8ToUTF32(byte utf8, ref uint utf32);

        public static void Vec4Copy(float[] src, float[] dest)
        {
            Array.Copy(src, dest, 4);
        }

        public static uint NextPowerOf2(uint n)
        {
            n--;

            n |= n >> 16;
            n |= n >> 8;
            n |= n >> 4;
            n |= n >> 2;
            n |= n >> 1;

            return n + 1;
        }

        public static int NextPowerOf2(int n)
        {
            n--;

            n |= n >> 16;
            n |= n >> 8;
            n |= n >> 4;
            n |= n >> 2;
            n |= n >> 1;

            return n + 1;
        }

        public static void BBoxAdd(float x0, float x1, float y0, float y1,
            ref float xmin, ref float xmax, ref float ymin, ref float ymax)
        {
            var min = Math.Min(x0, x1);
            var max = Math.Max(x0, x1);

            xmin = Math.Min(xmin, min);
            xmax = Math.Max(xmax, max);


            min = Math.Min(y0, y1);
            max = Math.Max(y0, y1);

            ymin = Math.Min(ymin, min);
            ymax = Math.Max(ymax, max);
        }
    }
}
