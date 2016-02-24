using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeRaider
{
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

            /// <summary>
            /// Length 4
            /// </summary>
            public CornerLocation[] CornerLocations { get; set; }

            public FileObjectTexture()
            {
                CornerLocations = new CornerLocation[4];
            }
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
            public byte Width { get; set; }

            public byte Height { get; set; }

            public ushort OriginalPage { get; set; }

            public byte OriginalX { get; set; }

            public byte OriginalY { get; set; }

            public uint NewPage { get; set; }

            public uint NewXWithBorder { get; set; }

            public uint NewYWithBorder { get; set; }
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
        private List<Loader.DWordTexture> originalPages;

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
            }
        }

        /// <summary>
        /// Adds an object texture to the list.
        /// </summary>
        /// <param name="texture"></param>
        private void addObjectTexture(Loader.ObjectTexture texture);

        /// <summary>
        /// Adds a sprite texture to the list.
        /// </summary>
        /// <param name="texture"></param>
        private void addSpriteTexture(Loader.SpriteTexture texture);

        /// <summary>
        /// Create a new Bordered texture atlas with the specified border width
        ///  and textures. This lays out all the data for the textures, but 
        /// does not upload anything to OpenGL yet.
        /// </summary>
        /// <param name="border">The border width around each texture.</param>
        public BorderedTextureAtlas(
            int border, 
            bool conserveMemory,
            List<Loader.DWordTexture> pages,
            List<Loader.ObjectTexture> objectTextures, 
            List<Loader.SpriteTexture> spriteTextures);

        ~BorderedTextureAtlas();

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
            // TODO: assert poly.Vertices.Count <= 4

            // TODO: assert texture < fileObjectTextures.Count
            var fileObjectTexture = fileObjectTextures[(int)texture];
            var canonical = canonicalObjectTextures[(int)fileObjectTexture.CanonicalTextureIndex];

            poly.TexIndex = (ushort) canonical.NewPage;
            for (var i = 0; i < poly.Vertices.Count; i++)
            {
                long xCoord = 0;
                long yCoord = 0;

                switch(fileObjectTexture.CornerLocations[i])
                {
                    case CornerLocation.TopLeft:
                        xCoord = canonical.NewXWithBorder + borderWidth;
                        yCoord = canonical.NewYWithBorder + borderWidth - shift;

                        if(split)
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
                        yCoord = canonical.NewYWithBorder + borderWidth  + canonical.Height - shift;
                        break;
                    case CornerLocation.BottomRight:
                        xCoord = canonical.NewXWithBorder + borderWidth + canonical.Width;
                        yCoord = canonical.NewYWithBorder + borderWidth + canonical.Height - shift;
                        break;
                }

                var index = reverse ? (poly.Vertices.Count - i - 1) : i;

                poly.Vertices[index].TexCoord[0] = (float) xCoord / resultPageWidth;
                poly.Vertices[index].TexCoord[1] = (float) yCoord / resultPageHeights[(int)canonical.NewPage];
            }
        }

        /// <summary>
        /// Same as above, but for sprite textures. This always returns four 
        /// coordinates (eight float values), in the order top right, top left,
        /// bottom left, bottom right.
        /// </summary>
        public void GetSpriteCoordinates(uint spriteTexture, uint outPage, float coordinates);

        /// <summary>
        /// Returns the number of texture atlas pages that have been created.
        /// Triggers a layout if none has happened so far.
        /// </summary>
        public uint GetNumAtlasPages();

        /// <summary>
        /// Returns height of specified file object texture.
        /// </summary>
        public uint GetTextureHeight(uint texture);

        /// <summary>
        /// Uploads the current data to OpenGL, as one or more texture pages.
        /// textureNames has to have a length of at least GetNumAtlasPages and will
        /// contain the names of the pages on return.
        /// </summary>
        /// <param name="textureNames">The names of the textures.</param>
        /// <param name="additionalTextureNames">How many texture names to create in addition to the needed ones.</param>
        public void CreateTextures(uint textureNames, uint additionalTextureNames);

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
