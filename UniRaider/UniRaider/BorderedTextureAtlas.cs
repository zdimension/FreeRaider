using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniRaider
{
    public class BorderedTextureAtlas
    {
        public enum CornerLocation
        {
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight
        }

        public class FileObjectTexture
        {
            public uint CanonicalTextureIndex { get; set; }

            public CornerLocation[] CornerLocations { get; set; }
        }

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

        private int m_borderWidth;

        private uint m_resultPageWith;

        private List<uint> m_resultPageHeights;

        private List<Loader.DWordTexture> m_originalPages;

        private List<FileObjectTexture> m_fileObjectTextures;

        private List<uint> m_canonicalTexturesForSpriteTextures;

        private List<CanonicalObjectTexture> m_canonicalObjectTextures;

        private void layOutTextures()
        {
            var sortedIndices = Enumerable.Range(0, m_canonicalObjectTextures.Count).ToArray();
            Array.Sort(sortedIndices, new TextureSizeComparator(this));

            var resultPages = new List<bsp>();
        }

        private void addObjectTexture(Loader.ObjectTexture texture);

        private void addSpriteTexture(Loader.SpriteTexture texture);

        public BorderedTextureAtlas(
            int border, 
            bool conserveMemory,
            List<Loader.DWordTexture> pages,
            List<Loader.ObjectTexture> objectTextures, 
            List<Loader.SpriteTexture> spriteTextures);

        public class TextureSizeComparator : IComparer<int>
        {
            public BorderedTextureAtlas Context { get; set; }

            public TextureSizeComparator(BorderedTextureAtlas context)
            {
                Context = context;
            }

            public int Compare(int index1, int index2)
            {
                var texture1 = Context.m_canonicalObjectTextures[index1];
                var texture2 = Context.m_canonicalObjectTextures[index2];

                return texture1.Height > texture2.Height || texture1.Width > texture2.Width ? 1 : 0;
            }
        }
    }
}
