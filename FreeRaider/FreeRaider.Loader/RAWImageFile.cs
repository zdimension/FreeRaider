using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using Ionic.Zlib;

namespace FreeRaider.Loader
{
    public static class RAWImageFile
    {
        public static Bitmap Read(string fname, int width = 512, int height = 256)
        {
            using (var fs = File.OpenRead(fname))
            {
                return Read(fs, width, height);
            }
        }

        public static unsafe Bitmap Read(Stream s, int width = 512, int height = 256)
        {
            var bs = new byte[s.Length];
            using (var ms = new MemoryStream(bs))
            {
                var buffer = new byte[16 * 1024];
                int read;
                while ((read = s.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
            }
            fixed (byte* ptr = bs)
            {
                var bmp = new Bitmap(width, height, width * 3, PixelFormat.Format24bppRgb, (IntPtr)ptr);
                var data = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, bmp.PixelFormat);
                var scan0 = (byte*)data.Scan0.ToPointer();
                long length = Math.Abs(data.Stride) * bmp.Height;
                // Invert R & B
                for (long i = 0; i < length; i += 3)
                {
                    var dummy = scan0[i];
                    scan0[i] = scan0[i + 2];
                    scan0[i + 2] = dummy;
                }
                bmp.UnlockBits(data);
                return bmp;
            }
        }
    }
}
