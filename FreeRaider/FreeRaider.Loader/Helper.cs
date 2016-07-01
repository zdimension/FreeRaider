using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ionic.Zlib;

namespace FreeRaider.Loader
{
    public partial class Helper
    {
        public static BinaryReader Decompress(byte[] compressed)
        {
            var uncompBuffer = ZlibStream.UncompressBuffer(compressed);

            return new BinaryReader(new MemoryStream(uncompBuffer));
        }

        public static byte[] Encompress(byte[] uncompressed)
        {
            return ZlibStream.CompressBuffer(uncompressed);
        }

        /// <summary>
        /// http://stackoverflow.com/a/27375621/2196124
        /// </summary>
        internal static int closestColor1(IEnumerable<ByteColor> colors, ByteColor target)
        {
            var hue1 = target.GetHue();
            var diffs = colors.Select<ByteColor, float>(n => getHueDistance(n.GetHue(), hue1));
            var diffMin = diffs.Min(n => n);
            return diffs.ToList().FindIndex(n => n == diffMin);
        }

        internal static float getHueDistance(float hue1, float hue2)
        {
            float d = Math.Abs(hue1 - hue2); return d > 180 ? 360 - d : d;
        }
    }
}
