using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ionic.Zlib;

namespace FreeRaider.Loader
{
    public static class PAKFile
    {
        public static uint GetLength(string fname)
        {
            using (var fs = File.OpenRead(fname))
            {
                var b = new byte[sizeof (uint)];
                fs.Read(b, 0, b.Length);
                return BitConverter.ToUInt32(b, 0);
            }
        }

        public static byte[] Read(string fname)
        {
            using (var fs = File.OpenRead(fname))
            {
                return Read(fs);
            }
        }

        public static byte[] Read(Stream s)
        {
            using (var br = new BinaryReader(s))
            {
                return Read(br);
            }
        }

        public static byte[] Read(BinaryReader br)
        {
            var uncompSize = br.ReadUInt32();
            var ret = new byte[uncompSize];

            using (var ms = new MemoryStream(ret))
            using (var z = new ZlibStream(br.BaseStream, CompressionMode.Decompress))
                z.CopyTo(ms);

            return ret;
        }

        public static void Read(Stream s, Stream output)
        {
            s.Position += sizeof(uint);
            using (var z = new ZlibStream(s, CompressionMode.Decompress))
                z.CopyTo(output);
        }

        public static void Write(string fname, byte[] data)
        {
            if (File.Exists(fname))
            {
                File.Delete(fname);
            }
            using (var fs = File.OpenWrite(fname))
            {
                Write(fs, data);
            }
        }

        public static void Write(Stream s, byte[] data)
        {
            using (var ms = new MemoryStream(data))
                Write(s, ms);
        }

        public static void Write(Stream s, Stream data)
        {
            s.Write(BitConverter.GetBytes((uint)s.Length), 0, 4);
            using (var z = new ZlibStream(data, CompressionMode.Compress))
                z.CopyTo(s);
        }
    }
}
