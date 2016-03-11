using System;
using System.Collections.Generic;
using System.Linq;
using FreeRaider.Loader;
using OpenTK.Graphics.OpenGL;

namespace FreeRaider
{
    public partial class Helper
    {
        /// <summary>
        /// Fills an area of memory with a four-byte pattern pointed to.
        /// </summary>
        public static unsafe void memset_pattern4(void* b, void* pattern, int len)
        {
            var intb = (uint*) b;
            var patternValue = *(uint*) pattern;
            for (int i = 0; i < len / 4; i++)
                intb[-i] = patternValue;
        }
    }

    public class BorderedTextureAtlas
    {
        /// <summary>
        /// Identifies a corner
        /// </summary>
        public enum CornerLocation
        {
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight
        }

        /// <summary>
        /// An internal representation of a file texture. Note that this only 
        /// stores a reference to the canonical texture and how the corners 
        /// of the canonical texture map to this.
        /// </summary>
        public class FileObjectTexture
        {
            public uint CanonicalTextureIndex { get; set; }

            public CornerLocation[] CornerLocations { get; set; } = new CornerLocation[4];
        }

        /// <summary>
        /// The canonical texture. In TR, a lot of textures can refer to the 
        /// same rectangle of pixels, only in different winding orders. It is 
        /// not practical to treat these as different textures, so they are 
        /// all mapped to one canonical object texture. This structure 
        /// consists of two parts: Describing the original location, and 
        /// describing the new final location. The latter is only valid after
        /// the data in the texture atlas has been laid out.
        /// </summary>
        public class CanonicalObjectTexture
        {
            public byte Width;

            public byte Height;

            public ushort OriginalPage;

            public byte OriginalX;

            public byte OriginalY;

            public uint NewPage;

            public uint NewXWithBorder;

            public uint NewYWithBorder;
        }

        /// <summary>
        /// How much border to add.
        /// </summary>
        private int borderWidth;

        /// <summary>
        /// Result pages
        /// </summary>
        private uint resultPageWidth;

        private List<uint> resultPageHeights;

        /// <summary>
        /// Original data
        /// </summary>
        private List<DWordTexture> originalPages;

        /// <summary>
        /// Object textures in the file.
        /// </summary>
        private List<FileObjectTexture> fileObjectTextures;

        /// <summary>
        /// Sprite texture in the file.
        /// </summary>
        private List<uint> canonicalTexturesForSpriteTextures;

        /// <summary>
        /// Canonical object textures
        /// </summary>
        private List<CanonicalObjectTexture> canonicalObjectTextures;

        /// <summary>
        /// Lays out the texture data and switches the atlas to laid out mode.
        /// </summary>
        private void layOutTextures()
        {
            var sortedIndices = Enumerable.Range(0, canonicalObjectTextures.Count).ToArray();
            Array.Sort(sortedIndices, new TextureSizeComparator(this));

            // Find positions for the canonical textures
            var resultPages = new List<BSPTree2DNode>();
            resultPageHeights.Clear();

            for (var texture = 0; texture < canonicalObjectTextures.Count; texture++)
            {
                var canonical = canonicalObjectTextures[sortedIndices[texture]];

                // Try to find space in an existing page.
                var foundPlace = false;
                for (var page = 0; page < resultPageHeights.Count; page++)
                {
                    // TODO: Why loop here, since resultPageHeights' been cleared at line 112?
                    foundPlace = resultPages[page].FindSpaceFor(
                        canonical.Width + 2 * (uint) borderWidth,
                        canonical.Height + 2 * (uint) borderWidth,
                        ref canonical.NewXWithBorder,
                        ref canonical.NewYWithBorder);
                    if (foundPlace)
                    {
                        canonical.NewPage = (uint) page;

                        var highestY = canonical.NewYWithBorder + canonical.Height + 2 * (uint) borderWidth;
                        if (highestY + 1 > resultPageHeights[page])
                            resultPageHeights[page] = highestY;

                        break;
                    }
                }

                // No existing page has enough remaining space so open new one.
                if (!foundPlace)
                {
                    resultPages.Add(new BSPTree2DNode(0, 0, resultPageWidth, resultPageWidth));
                    canonical.NewPage = (uint) resultPageHeights.Count;

                    resultPages.Last().FindSpaceFor(
                        canonical.Width + 2 * (uint) borderWidth,
                        canonical.Height + 2 * (uint) borderWidth,
                        ref canonical.NewXWithBorder,
                        ref canonical.NewYWithBorder);

                    resultPageHeights.Add(canonical.NewYWithBorder + canonical.Height + 2 * (uint) borderWidth);
                }
            }

            // Fix up heights if necessary
            resultPageHeights = resultPageHeights.Select(Helper.NextPowerOf2).ToList();
        }

        /// <summary>
        /// Adds an object texture to the list.
        /// </summary>
        /// <param name="texture"></param>
        private void addObjectTexture(ObjectTexture texture)
        {
            // Determine the canonical texture for this texture.
            // Use only first three vertices to find min, max, because for triangles the last will be 0,0 with no other marker that this is a triangle. As long as all textures are axis-aligned rectangles, this will always return the right result anyway.
            var max = new[]
            {
                texture.Vertices.Max(x => x.Xpixel),
                texture.Vertices.Max(x => x.Ypixel)
            };
            var min = new[]
            {
                texture.Vertices.Min(x => x.Xpixel),
                texture.Vertices.Min(x => x.Ypixel)
            };
            var width = max[0] - min[0];
            var height = max[1] - min[1];

            // See whether it already exists
            var canonicalIndex = -1;
            for(var i = 0; i < canonicalObjectTextures.Count; i++)
            {
                var cand = canonicalObjectTextures[i];

                if(cand.OriginalPage == (texture.TileAndFlag & Loader.Constants.TextureIndexMaskTr4)
                    && cand.OriginalX == min[0]
                    && cand.OriginalY == min[1]
                    && cand.Width == width
                    && cand.Height == height)
                {
                    canonicalIndex = i;
                    break;
                }
            }

            // Create it if not.
            if(canonicalIndex == -1)
            {
                canonicalIndex = canonicalObjectTextures.Count;

                canonicalObjectTextures.Add(new CanonicalObjectTexture
                {
                    Width = (byte)width,
                    Height = (byte)height,
                    OriginalPage = (ushort)(texture.TileAndFlag & Loader.Constants.TextureIndexMaskTr4),
                    OriginalX = min[0],
                    OriginalY = min[1]
                });
            }

            // Create file object texture.
            var fot = new FileObjectTexture {CanonicalTextureIndex = (uint) canonicalIndex};
            for(var i = 0; i < 4; i++)
            {
                var v = texture.Vertices[i];
                if(v.Xpixel == min[0])
                {
                    if(v.Ypixel == min[1])
                        fot.CornerLocations[i] = CornerLocation.TopLeft;
                    else
                        fot.CornerLocations[i] = CornerLocation.BottomLeft;
                }
                else
                {
                    if (v.Ypixel == min[1])
                        fot.CornerLocations[i] = CornerLocation.TopRight;
                    else
                        fot.CornerLocations[i] = CornerLocation.BottomRight;
                }
            }
            fileObjectTextures.Add(fot);
        }

        /// <summary>
        /// Adds a sprite texture to the list.
        /// </summary>
        /// <param name="texture"></param>
        private void addSpriteTexture(SpriteTexture texture)
        {
            // Determine the canonical texture for this texture.
            var x = texture.X0;
            var y = texture.Y0;
            var width = texture.X1 - texture.X0; // TODO: Add Width and Height properties for that
            var height = texture.Y1 - texture.Y0;

            // See whether it already exists
            var canonicalIndex = -1;
            for (var i = 0; i < canonicalObjectTextures.Count; i++)
            {
                var cand = canonicalObjectTextures[i];

                if (cand.OriginalPage == (texture.Tile & Loader.Constants.TextureIndexMaskTr4)
                    && cand.OriginalX == x
                    && cand.OriginalY == y
                    && cand.Width == width
                    && cand.Height == height)
                {
                    canonicalIndex = i;
                    break;
                }
            }

            // Create it if not.
            if (canonicalIndex == -1)
            {
                canonicalIndex = canonicalObjectTextures.Count;

                canonicalObjectTextures.Add(new CanonicalObjectTexture
                {
                    Width = (byte)width,
                    Height = (byte)height,
                    OriginalPage = (ushort)(texture.Tile & Loader.Constants.TextureIndexMaskTr4),
                    OriginalX = (byte)x,
                    OriginalY = (byte)y
                });
            }

            // Create sprite texture assignment.
            canonicalTexturesForSpriteTextures.Add((uint)canonicalIndex);
        }

        /// <summary>
        /// Create a new Bordered texture atlas with the specified border width
        ///  and textures. This lays out all the data for the textures, but 
        /// does not upload anything to OpenGL yet.
        /// </summary>
        /// <param name="border">The border width around each texture.</param>
        public BorderedTextureAtlas(
            int border,
            bool conserveMemory,
            List<DWordTexture> pages,
            List<ObjectTexture> objectTextures,
            List<SpriteTexture> spriteTextures)
        {
            borderWidth = border;
            resultPageWidth = 0;
            resultPageHeights = new List<uint>();
            originalPages = pages.ToList();
            fileObjectTextures = new List<FileObjectTexture>();
            canonicalTexturesForSpriteTextures = new List<uint>();
            canonicalObjectTextures = new List<CanonicalObjectTexture>();

            var maxTextureEdgeLength = Math.Min(GL.GetInteger(GetPName.MaxTextureSize), 4096);

            if (conserveMemory)
            {
                long areaSum = objectTextures.Sum(t => t.Width * t.Height) +
                               spriteTextures.Sum(t => Math.Abs((t.X1 - t.X0) * (t.Y1 - t.Y0)));

                resultPageWidth =
                    (uint)
                        Math.Min(maxTextureEdgeLength,
                            Helper.NextPowerOf2((uint) (Math.Sqrt(areaSum) * Constants.Sqrt2)));
            }
            else
            {
                resultPageWidth = Helper.NextPowerOf2((uint) maxTextureEdgeLength);
            }

            foreach (var tex in objectTextures)
            {
                addObjectTexture(tex);
            }

            foreach (var tex in spriteTextures)
            {
                addSpriteTexture(tex);
            }

            layOutTextures();
        }

        /// <summary>
        /// Returns the texture coordinates of the specified texture. This must
        /// only be
        /// called after all pages and object texture coordinates have been 
        /// added.
        /// Otherwise the size calculation code won't work.
        /// </summary>
        /// <param name="texture">The texture.</param>
        /// <param name="reverse">Whether to reverse the order of texture coordinates on output.</param>
        public void GetCoordinates(uint texture, bool reverse, Polygon poly, int shift = 0, bool split = false)
        {
            Assert.That(poly.Vertices.Count <= 4);

            Assert.That(texture < fileObjectTextures.Count);
            var fileObjectTexture = fileObjectTextures[(int) texture];
            var canonical = canonicalObjectTextures[(int) fileObjectTexture.CanonicalTextureIndex];

            poly.TexIndex = (ushort) canonical.NewPage;
            for (var i = 0; i < poly.Vertices.Count; i++)
            {
                long xCoord = 0;
                long yCoord = 0;

                switch (fileObjectTexture.CornerLocations[i])
                {
                    case CornerLocation.TopLeft:
                        xCoord = canonical.NewXWithBorder + borderWidth;
                        yCoord = canonical.NewYWithBorder + borderWidth - shift;

                        if (split)
                        {
                            yCoord += (canonical.Height / 2);
                        }
                        break;
                    case CornerLocation.TopRight:
                        xCoord = canonical.NewXWithBorder + borderWidth + canonical.Width;
                        yCoord = canonical.NewYWithBorder + borderWidth - shift;

                        if (split)
                        {
                            yCoord += (canonical.Height / 2);
                        }
                        break;
                    case CornerLocation.BottomLeft:
                        xCoord = canonical.NewXWithBorder + borderWidth;
                        yCoord = canonical.NewYWithBorder + borderWidth + canonical.Height - shift;
                        break;
                    case CornerLocation.BottomRight:
                        xCoord = canonical.NewXWithBorder + borderWidth + canonical.Width;
                        yCoord = canonical.NewYWithBorder + borderWidth + canonical.Height - shift;
                        break;
                }

                var index = reverse ? (poly.Vertices.Count - i - 1) : i;

                poly.Vertices[index].TexCoord[0] = (float) xCoord / resultPageWidth;
                poly.Vertices[index].TexCoord[1] = (float) yCoord / resultPageHeights[(int) canonical.NewPage];
            }
        }

        /// <summary>
        /// Same as above, but for sprite textures. This always returns four 
        /// coordinates (eight float values), in the order top right, top left,
        /// bottom left, bottom right.
        /// </summary>
        public void GetSpriteCoordinates(uint spriteTexture, out uint outPage, float[] coordinates)
        {
            Assert.That(spriteTexture < canonicalTexturesForSpriteTextures.Count);

            var canonicalIndex = canonicalTexturesForSpriteTextures[(int) spriteTexture];
            var canonical = canonicalObjectTextures[(int) canonicalIndex];

            outPage = canonical.NewPage;

            var pixelCoords = new[]
            {
                // top right
                canonical.NewXWithBorder + borderWidth + canonical.Width,
                canonical.NewYWithBorder + borderWidth + canonical.Height,

                // top left
                canonical.NewXWithBorder + borderWidth,
                canonical.NewYWithBorder + borderWidth + canonical.Height,

                // bottom left
                canonical.NewXWithBorder + borderWidth,
                canonical.NewYWithBorder + borderWidth,

                // bottom right
                canonical.NewXWithBorder + borderWidth + canonical.Width,
                canonical.NewYWithBorder + borderWidth
            };

            for(var i = 0; i < 4; i++)
            {
                coordinates[i * 2 + 0] = pixelCoords[i * 2 + 0] / (float)resultPageWidth;
                coordinates[i * 2 + 1] = pixelCoords[i * 2 + 1] / (float)resultPageHeights[(int)canonical.NewPage];
            }
        }

        /// <summary>
        /// Returns the number of texture atlas pages that have been created.
        /// Triggers a layout if none has happened so far.
        /// </summary>
        public uint NumAtlasPages => (uint) resultPageHeights.Count;

        /// <summary>
        /// Returns height of specified file object texture.
        /// </summary>
        public uint GetTextureHeight(uint texture)
        {
            Assert.That(texture < fileObjectTextures.Count);

            var fileObjectTexture = fileObjectTextures[(int) texture];
            var canonical = canonicalObjectTextures[(int) fileObjectTexture.CanonicalTextureIndex];

            return canonical.Height;
        }

        /// <summary>
        /// Uploads the current data to OpenGL, as one or more texture pages.
        /// textureNames has to have a length of at least GetNumAtlasPages and will
        /// contain the names of the pages on return.
        /// </summary>
        /// <param name="textureNames">The names of the textures.</param>
        /// <param name="additionalTextureNames">How many texture names to create in addition to the needed ones.</param>
        public unsafe void CreateTextures(uint* textureNames, uint additionalTextureNames)
        {
            GL.GenTextures(resultPageHeights.Count + (int)additionalTextureNames, textureNames);

            for (var page = 0; page < resultPageHeights.Count; page++)
            {
                var data = new byte[4 * resultPageWidth * resultPageWidth];
                for (var texture = 0; texture < canonicalObjectTextures.Count; texture++)
                {
                    var canonical = canonicalObjectTextures[texture];
                    if (canonical.NewPage != page)
                        continue;

                    fixed (uint* pixels = originalPages[canonical.OriginalPage].Pixels[0])
                    {
                        // Add top border
                        for (var border = 0; border < borderWidth; border++)
                        {
                            var x = canonical.NewXWithBorder;
                            var y = canonical.NewYWithBorder + border;
                            var oldX = canonical.OriginalX;
                            var oldY = canonical.OriginalY;

                            fixed (byte* ptr = data)
                            {
                                // expand top-left pixel
                                Helper.memset_pattern4(&ptr[(y * resultPageWidth + x) * 4],
                                    &pixels[oldY * 256 + oldX],
                                    4 * borderWidth);
                                // copy top line
                                Helper.memcpy(&ptr[(y * resultPageWidth + x + borderWidth) * 4],
                                    &pixels[oldY * 256 + oldY],
                                    canonical.Width * 4);
                                // expand top-right pixel
                                Helper.memset_pattern4(
                                    &ptr[(y * resultPageWidth + x + borderWidth + canonical.Width) * 4],
                                    &pixels[oldY * 256 + oldX + canonical.Width],
                                    4 * borderWidth);
                            }
                        }

                        // Copy main content
                        for (var line = 0; line < canonical.Height; line++)
                        {
                            var x = canonical.NewXWithBorder;
                            var y = canonical.NewYWithBorder + borderWidth + line;
                            var oldX = canonical.OriginalX;
                            var oldY = canonical.OriginalY + line;

                            fixed (byte* ptr = data)
                            {
                                // expand left pixel
                                Helper.memset_pattern4(&ptr[(y * resultPageWidth + x) * 4],
                                    &pixels[oldY * 256 + oldX],
                                    4 * borderWidth);
                                // copy line
                                Helper.memcpy(&ptr[(y * resultPageWidth + x + borderWidth) * 4],
                                    &pixels[oldY * 256 + oldY],
                                    canonical.Width * 4);
                                // expand right pixel
                                Helper.memset_pattern4(
                                    &ptr[(y * resultPageWidth + x + borderWidth + canonical.Width) * 4],
                                    &pixels[oldY * 256 + oldX + canonical.Width],
                                    4 * borderWidth);
                            }
                        }

                        // Add bottom border
                        for (var border = 0; border < borderWidth; border++)
                        {
                            var x = canonical.NewXWithBorder;
                            var y = canonical.NewYWithBorder + canonical.Height + borderWidth + border;
                            var oldX = canonical.OriginalX;
                            var oldY = canonical.OriginalY + canonical.Height;

                            fixed (byte* ptr = data)
                            {
                                // expand bottom-left pixel
                                Helper.memset_pattern4(&ptr[(y * resultPageWidth + x) * 4],
                                    &pixels[oldY * 256 + oldX],
                                    4 * borderWidth);
                                // copy bottom line
                                Helper.memcpy(&ptr[(y * resultPageWidth + x + borderWidth) * 4],
                                    &pixels[oldY * 256 + oldY],
                                    canonical.Width * 4);
                                // expand bottom-right pixel
                                Helper.memset_pattern4(
                                    &ptr[(y * resultPageWidth + x + borderWidth + canonical.Width) * 4],
                                    &pixels[oldY * 256 + oldX + canonical.Width],
                                    4 * borderWidth);
                            }
                        }
                    }
                }

                GL.BindTexture(TextureTarget.Texture2D, textureNames[page]);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, (int) resultPageWidth,
                    (int) resultPageHeights[page], 0, PixelFormat.Rgba, PixelType.UnsignedByte, data);
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            }
        }

        public class TextureSizeComparator : IComparer<int>
        {
            public BorderedTextureAtlas Context { get; set; }

            public TextureSizeComparator(BorderedTextureAtlas context)
            {
                Context = context;
            }

            public int Compare(int index1, int index2)
            {
                var texture1 = Context.canonicalObjectTextures[index1];
                var texture2 = Context.canonicalObjectTextures[index2];

                return texture1.Height > texture2.Height || texture1.Width > texture2.Width ? 1 : 0;
            }
        }
    }
}
