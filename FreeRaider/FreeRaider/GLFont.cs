using System;
using OpenTK.Graphics.OpenGL;
using SharpFont;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace FreeRaider
{
    public struct CharInfo
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
        public Library FTLibrary;

        public Face FTFace;

        public ushort FontSize;

        public CharInfo[] Glyphs = new CharInfo[0];

        public ushort GLRealTexIndexesCount;

        public uint[] GLTexIndexes;

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

                Array.Resize(ref glf.Glyphs, glf.FTFace.GlyphCount);

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

                Array.Resize(ref glf.Glyphs, glf.FTFace.GlyphCount);

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
                var charsInRow = 1 + (int) Math.Sqrt(glf.Glyphs.Length);
                glf.GLTexWidth = Math.Min(Helper.NextPowerOf2((fontSize + padding) * charsInRow), glf.GLMaxTexWidth);

                // create new atlas
                charsInRow = glf.GLTexWidth / (fontSize + padding);
                var charsInColumn = glf.Glyphs.Length / charsInRow + 1;
                Array.Resize(ref glf.GLTexIndexes, charsInColumn * (fontSize + padding) / glf.GLTexWidth + 1);
                GL.GenTextures(glf.GLTexIndexes.Length, glf.GLTexIndexes);

                var bufferSize = glf.GLTexWidth * glf.GLTexWidth * sizeof (byte);
                var buffer = new byte[bufferSize];

                for (i = 0, x = 0, y = 0; i < glf.Glyphs.Length; i++)
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
                            buffer[(y + yy) * glf.GLTexWidth + x + xx] = g.Bitmap.BufferData[yy * g.Bitmap.Width + xx];
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

                for (ii = i0; ii < glf.Glyphs.Length; ii++)
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

        public static unsafe float GetStringLen(FontTexture glf, string text, int n)
        {
            var x = 0.0f;

            if (glf?.FTFace != null)
            {
                var ch = Helper.GetStringBPUTF8(text);
                uint curr_utf32, next_utf32;
                var nch = Helper.UTF8ToUTF32(ch, &curr_utf32);
                curr_utf32 = glf.FTFace.GetCharIndex(curr_utf32);

                for (var i = 0; (*ch != 0) && !(n >= 0 && i >= n); i++)
                {
                    var nch2 = Helper.UTF8ToUTF32(nch, &next_utf32);
                    next_utf32 = glf.FTFace.GetCharIndex(next_utf32);
                    ch = nch;
                    nch = nch2;

                    var kern = glf.FTFace.GetKerning(curr_utf32, next_utf32, KerningMode.Unscaled);
                    curr_utf32 = next_utf32;
                    x += (kern.X.ToSingle() + glf.Glyphs[(int)curr_utf32].AdvanceX) / 64.0f;
                }
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

        public static unsafe void GetStringBB(FontTexture glf, string text, int n, ref float x0, ref float y0, ref float x1, ref float y1)
        {
            x0 = 0;
            x1 = 0;
            y0 = 0;
            y1 = 0;

            if(glf?.FTFace != null)
            {
                byte* nch2;
                byte* ch = Helper.GetStringBPUTF8(text);
                var x = 0.0f;
                var y = 0.0f;
                float xx0, xx1, yy0, yy1;
                uint curr_utf32, next_utf32;

                var nch = Helper.UTF8ToUTF32(ch, &curr_utf32);
                curr_utf32 = glf.FTFace.GetCharIndex(curr_utf32);

                for (var i = 0; (*ch != 0) && !(n >= 0 && i >= n); i++)
                {
                    fixed (CharInfo* g = &glf.Glyphs[(int) curr_utf32])
                    {
                        nch2 = Helper.UTF8ToUTF32(nch, &next_utf32);

                        next_utf32 = glf.FTFace.GetCharIndex(next_utf32);
                        ch = nch;
                        nch = nch2;

                        var kern = glf.FTFace.GetKerning(curr_utf32, next_utf32, KerningMode.Unscaled);
                        curr_utf32 = next_utf32;

                        xx0 = x + g->Left;
                        xx1 = xx0 + g->Width;
                        yy0 = y + g->Top;
                        yy1 = yy0 - g->Height;
                        Helper.BBoxAdd(xx0, xx1, yy0, yy1, ref x0, ref x1, ref y0, ref y1);

                        x += (kern.X.ToSingle() + g->AdvanceX) / 64.0f;
                        y += (kern.Y.ToSingle() + g->AdvanceY) / 64.0f;
                    }
                }
            }
        }

        public static unsafe void RenderStr(FontTexture glf, float x, float y, string text)
        {
            byte* nch;
            byte* ch = Helper.GetStringBPUTF8(text);
            FTVector26Dot6 kern;

            if(glf?.FTFace == null || text == null || text[0] == '\0')
            {
                return;
            }

            FontBuffer.Bind();

            if (glf.GLRealTexIndexesCount == 1)
            {
                var p = FontBuffer.ResizeAndMap(48 * (int)Helper.UTF8StrLen(text) * sizeof (float));
                uint elementsCount = 0;
                uint curr_utf32, next_utf32;
                nch = Helper.UTF8ToUTF32(ch, &curr_utf32);
                curr_utf32 = glf.FTFace.GetCharIndex(curr_utf32);

                while(*ch != 0)
                {
                    byte* nch2 = Helper.UTF8ToUTF32(nch, &next_utf32);

                    next_utf32 = glf.FTFace.GetCharIndex(next_utf32);
                    ch = nch;
                    nch = nch2;

                    fixed (CharInfo* g = &glf.Glyphs[(int) curr_utf32])
                    {
                        kern = glf.FTFace.GetKerning(curr_utf32, next_utf32, KerningMode.Unscaled);
                        curr_utf32 = next_utf32;

                        if (g->TexIndex != 0)
                        {
                            var x0 = x + g->Left;
                            var x1 = x0 + g->Width;
                            var y0 = y + g->Top;
                            var y1 = y0 - g->Height;

                            *p = x0;
                            p++;
                            *p = y0;
                            p++;
                            *p = g->TexX0;
                            p++;
                            *p = g->TexY0;
                            p++;
                            fixed (float* glfc = glf.GLFontColor) Helper.Vec4CopyPointer(p, glfc);
                            p += 4;

                            *p = x1;
                            p++;
                            *p = y0;
                            p++;
                            *p = g->TexX1;
                            p++;
                            *p = g->TexY0;
                            p++;
                            fixed (float* glfc = glf.GLFontColor) Helper.Vec4CopyPointer(p, glfc);
                            p += 4;

                            *p = x1;
                            p++;
                            *p = y1;
                            p++;
                            *p = g->TexX1;
                            p++;
                            *p = g->TexY1;
                            p++;
                            fixed (float* glfc = glf.GLFontColor) Helper.Vec4CopyPointer(p, glfc);
                            p += 4;
                            elementsCount++;

                            *p = x0;
                            p++;
                            *p = y0;
                            p++;
                            *p = g->TexX0;
                            p++;
                            *p = g->TexY0;
                            p++;
                            fixed (float* glfc = glf.GLFontColor) Helper.Vec4CopyPointer(p, glfc);
                            p += 4;

                            *p = x1;
                            p++;
                            *p = y1;
                            p++;
                            *p = g->TexX1;
                            p++;
                            *p = g->TexY1;
                            p++;
                            fixed (float* glfc = glf.GLFontColor) Helper.Vec4CopyPointer(p, glfc);
                            p += 4;

                            *p = x0;
                            p++;
                            *p = y1;
                            p++;
                            *p = g->TexX0;
                            p++;
                            *p = g->TexY1;
                            p++;
                            fixed (float* glfc = glf.GLFontColor) Helper.Vec4CopyPointer(p, glfc);
                            p += 4;
                            elementsCount++;
                        }
                        x += (kern.X.ToSingle() + g->AdvanceX) / 64.0f;
                        y += (kern.Y.ToSingle() + g->AdvanceY) / 64.0f;
                    }
                }
                FontBuffer.Unmap();
                // RENDER
                if(elementsCount != 0)
                {
                    GL.BindTexture(TextureTarget.Texture2D, glf.GLTexIndexes[0]);
                    GL.DrawArrays(PrimitiveType.Triangles, 0, (int)elementsCount * 3);
                }
            }
            else
            {
                uint activeTexture = 0;
                uint curr_utf32, next_utf32;
                nch = Helper.UTF8ToUTF32(ch, &curr_utf32);
                curr_utf32 = glf.FTFace.GetCharIndex(curr_utf32);

                for (;*ch != 0;)
                {
                    var p = FontBuffer.ResizeAndMap(32 * sizeof(float));

                    byte* nch2 = Helper.UTF8ToUTF32(nch, &next_utf32);

                    next_utf32 = glf.FTFace.GetCharIndex(next_utf32);
                    ch = nch;
                    nch = nch2;

                    fixed (CharInfo* g = &glf.Glyphs[(int)curr_utf32])
                    {
                        kern = glf.FTFace.GetKerning(curr_utf32, next_utf32, KerningMode.Unscaled);
                        curr_utf32 = next_utf32;

                        if (g->TexIndex != 0)
                        {
                            if(activeTexture != g->TexIndex)
                            {
                                GL.BindTexture(TextureTarget.Texture2D, g->TexIndex);
                            }
                            // RENDER
                            var x0 = x + g->Left;
                            var x1 = x0 + g->Width;
                            var y0 = y + g->Top;
                            var y1 = y0 - g->Height;

                            *p = x0;
                            p++;
                            *p = y0;
                            p++;
                            *p = g->TexX0;
                            p++;
                            *p = g->TexY0;
                            p++;
                            fixed (float* glfc = glf.GLFontColor) Helper.Vec4CopyPointer(p, glfc);
                            p += 4;

                            *p = x1;
                            p++;
                            *p = y0;
                            p++;
                            *p = g->TexX1;
                            p++;
                            *p = g->TexY0;
                            p++;
                            fixed (float* glfc = glf.GLFontColor) Helper.Vec4CopyPointer(p, glfc);
                            p += 4;

                            *p = x1;
                            p++;
                            *p = y1;
                            p++;
                            *p = g->TexX1;
                            p++;
                            *p = g->TexY1;
                            p++;
                            fixed (float* glfc = glf.GLFontColor) Helper.Vec4CopyPointer(p, glfc);
                            p += 4;

                            *p = x0;
                            p++;
                            *p = y1;
                            p++;
                            *p = g->TexX0;
                            p++;
                            *p = g->TexY1;
                            p++;
                            fixed (float* glfc = glf.GLFontColor) Helper.Vec4CopyPointer(p, glfc);

                            FontBuffer.Unmap();

                            GL.DrawArrays(PrimitiveType.TriangleFan, 0, 4);
                        }
                        x += (kern.X.ToSingle() + g->AdvanceX) / 64.0f;
                        y += (kern.Y.ToSingle() + g->AdvanceY) / 64.0f;
                    }
                }
            }
        }
    }

    public partial class Helper
    {
        public static string UTF8NextSymbol(string utf8)
        {
            var b = (byte)utf8[0];

            // save ASC symbol as is
            if(!b.HasFlagUns(0x80))
            {
                return utf8.Substring(1);
            }

            // calculate length
            while(b.HasFlagUns(0x80))
            {
                b <<= 1;
                utf8 = utf8.Substring(1);
            }

            return utf8;
        }

        public static uint UTF8StrLen(string str)
        {
            uint i = 0;
            var ch = str;

            for (; ch.Length > 0; i++)
            {
                ch = UTF8NextSymbol(ch);
            }

            return i;
        }

        public static unsafe byte* UTF8ToUTF32(byte* utf8, uint* utf32)
        {
            var u_utf8 = utf8;
            var b = *u_utf8++;
            var len = 0;

            // save ASC symbol as is
            if (!b.HasFlagUns(0x80))
            {
                *utf32 = b;
                return utf8 + 1;
            }

            // calculate length
            while (b.HasFlagUns(0x80))
            {
                b <<= 1;
                len++;
            }

            uint c = b;
            var shift = (uint)(6 - len);

            while(--len != 0)
            {
                c <<= (int)shift;
                c |= (uint)(*u_utf8++ & 0x3f);
                shift = 6;
            }

            *utf32 = c;
            return u_utf8;
        }

        public static void Vec4Copy(float[] dest, float[] src)
        {
            Array.Copy(src, dest, 4);
        }

        public static unsafe void Vec4CopyPointer(float* x, float* y)
        {
            x[0] = y[0];
            x[1] = y[1];
            x[2] = y[2];
            x[3] = y[3];
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

        public static unsafe byte* GetStringBPUTF8(string str)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(str);
            fixed(byte* p = bytes)
            {
                return p;
            }
        }
    }
}
