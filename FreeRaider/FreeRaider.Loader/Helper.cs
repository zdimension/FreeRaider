using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ionic.Zlib;

namespace FreeRaider.Loader
{
    internal partial class Helper
    {
        internal static readonly Encoding CP437 = Encoding.GetEncoding(437);

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

        public static void Add<T>(ref T[] arr, T item)
        {
            Array.Resize(ref arr, arr.Length + 1);
            arr[arr.Length - 1] = item;
        }

        public static List<T> GetSetFlags<T>(T fl)
        {
            if (!(fl is Enum))
                throw new ArgumentException("fl should be Enum", nameof(fl));
            var res = new List<T>();
            dynamic fle = fl;
            foreach (var v in Enum.GetValues(fle.GetType()))
            {
                if ((fle & v) != 0) res.Add(v);
            }
            return res;
        }

        public static void Set<T>(ref T[] a, int i, T item, T def = default(T))
        {
            var old = a.Length;
            var ns = i + 1;
            if (old < ns)
            {
                Array.Resize(ref a, ns);
                for (var j = old; j < ns; j++)
                    a[j] = def;
            }
            a[i] = item;
        }
    }
}
