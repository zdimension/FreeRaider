using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace NLibsndfile.Native.Types
{
    public struct SF_VIRTUAL_IO
    {
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public sf_vio_get_filelen get_filelen;
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public sf_vio_seek seek;
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public sf_vio_read read;
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public sf_vio_write write;
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public sf_vio_tell tell;
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public unsafe delegate long sf_vio_get_filelen(void* userData);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public unsafe delegate long sf_vio_seek(long offset, SEEK whence, void* userData);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public unsafe delegate long sf_vio_read(void* ptr, long count, void* userData);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public unsafe delegate long sf_vio_write(void* ptr, long count, void* userData);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public unsafe delegate long sf_vio_tell(void* userData);

    public unsafe struct MemBufferFileIo
    {
        public MemBufferFileIo(byte* data, long dataSize)
        {
            if (data == (byte*) 0)
            {
                throw new ArgumentException("data is null pointer", nameof(data));
            }

            m_data = data;
            m_dataSize = dataSize;
            m_where = 0;
        }

        public static long get_filelen(void* userData)
        {
            var self = (MemBufferFileIo*)userData;
            return self->m_dataSize;
        }

        public static long seek(long offset, SEEK whence, void* userData)
        {
            var self = (MemBufferFileIo*)userData;
            switch (whence)
            {
                case SEEK.SEEK_SET:
                    self->m_where = offset;
                    break;
                case SEEK.SEEK_CUR:
                    self->m_where += offset;
                    break;
                case SEEK.SEEK_END:
                    self->m_where = self->m_dataSize - offset;
                    break;
            }
            return self->m_where;
        }

        public static long read(void* ptr, long count, void* userData)
        {
            var self = (MemBufferFileIo*)userData;
            if (self->m_where + count > self->m_dataSize)
                count = self->m_dataSize - self->m_where;

            var buf = (byte*)ptr;
            for (long i = 0; i < count; i++)
                buf[i] = self->m_data[self->m_where + i];
            self->m_where += count;
            return count;
        }

        public static long write(void* ptr, long count, void* userData)
        {
            return 0; // Read-only
        }

        public static long tell(void* userData)
        {
            var self = (MemBufferFileIo*)userData;
            return self->m_where;
        }

        // I put delegates here as static fields so GC doesn't collect them
        // Took me 3 weeks to figure that out
        private static readonly sf_vio_get_filelen keepAlive_get_filelen = get_filelen;
        private static readonly sf_vio_seek keepAlive_seek = seek;
        private static readonly sf_vio_read keepAlive_read = read;
        private static readonly sf_vio_write keepAlive_write = write;
        private static readonly sf_vio_tell keepAlive_tell = tell;

        public SF_VIRTUAL_IO ToSfVirtualIo()
        {
            return new SF_VIRTUAL_IO
            {
                get_filelen = keepAlive_get_filelen,
                seek = keepAlive_seek,
                read = keepAlive_read,
                write = keepAlive_write,
                tell = keepAlive_tell
            };
        }

        public byte* m_data;
        public long m_dataSize;
        public long m_where;
    }

    public unsafe struct StreamFileIo
    {
        public static Dictionary<int, Stream> Streams = new Dictionary<int, Stream>();
        public static int NextID = 0;

        public StreamFileIo(Stream s) : this(s, s.Length, 0)
        {
            isFixed = false;
        }

        public StreamFileIo(Stream s, long length, long offset)
        {
            id = NextID;
            Streams[id] = s;
            this.length = length;
            this.offset = offset;
            NextID++;
            isFixed = true;
        }

        public static Stream GetStream(void* userData)
        {
            var self = (StreamFileIo*) userData;
            if (self->id == -1)
            {
                throw new ObjectDisposedException("StreamFileIo is closed");
            }
            return Streams[self->id];
        }

        public static long getFileLength(void* userData)
        {
            var self = (StreamFileIo*)userData;
            if (self->isFixed)
                return self->length;
            return GetStream(userData).Length;
        }

        public static long doSeek(long offset, SEEK whence, void* userData)
        {
            var self = (StreamFileIo*) userData;
            var st = GetStream(userData);
            switch (whence)
            {
                case SEEK.SEEK_SET:
                    if (self->isFixed)
                    {
                        var newPos = self->offset + offset;
                        if (newPos > self->offset + self->length - 1)
                            newPos = self->offset + self->length - 1;
                        st.Seek(newPos, SeekOrigin.Begin);
                    }
                    else st.Seek(offset, SeekOrigin.Begin);
                    break;
                case SEEK.SEEK_CUR:
                    st.Seek(offset, SeekOrigin.Current);
                    break;
                case SEEK.SEEK_END:
                    st.Seek(offset, SeekOrigin.End);
                    break;
            }
            if (st.Position < self->offset)
                st.Position = self->offset;
            else if (st.Position > self->offset + self->length - 1)
                st.Position = self->offset + self->length - 1;
            return st.Position;
        }

        private static int a = 0;

        public static long doRead(void* ptr, long count, void* userData)
        {
            a++;
            var st = GetStream(userData);
            var buf = new byte[count];

            var reads = st.Read(buf, 0, (int)count);
            Marshal.Copy(buf, 0, (IntPtr)ptr, reads);
            return reads;
        }

        public static long doWrite(void* ptr, long count, void* userData)
        {
            var st = GetStream(userData);
            var buf = new byte[count];
            Marshal.Copy((IntPtr)ptr, buf, 0, (int)count);
            st.Write(buf, 0, (int) count);
            return count;
        }

        public static long doTell(void* userData)
        {
            var self = (StreamFileIo*) userData;
            if (self->isFixed) return GetStream(userData).Position - self->offset;
            return GetStream(userData).Position;
        }

        private static readonly sf_vio_get_filelen keepAlive_get_filelen = getFileLength;
        private static readonly sf_vio_seek keepAlive_seek = doSeek;
        private static readonly sf_vio_read keepAlive_read = doRead;
        private static readonly sf_vio_write keepAlive_write = doWrite;
        private static readonly sf_vio_tell keepAlive_tell = doTell;

        public SF_VIRTUAL_IO ToSfVirtualIo()
        {
            return new SF_VIRTUAL_IO
            {
                get_filelen = getFileLength,
                seek = doSeek,
                read = doRead,
                write = doWrite,
                tell = doTell
            };
        }

        private int id;
        private long length;
        private long offset;
        private bool isFixed;

        public void Close()
        {
            if (id == -1)
            {
                throw new ObjectDisposedException("StreamFileIo is closed");
            }
            Streams[id].Dispose();
            Streams.Remove(id);
            id = -1;
        }
    }
}
