using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace NLibsndfile.Native.Types
{
    public struct SF_VIRTUAL_IO
    {
        public sf_vio_get_filelen get_filelen;
        public sf_vio_seek seek;
        public sf_vio_read read;
        public sf_vio_write write;
        public sf_vio_tell tell;

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
    }

    public unsafe struct MemBufferFileIo
    {
        public MemBufferFileIo(byte* data, long dataSize)
        {
            m_data = data;
            m_dataSize = dataSize;
            m_where = 0;
        }

        public static long getFileLength(void* userData)
        {
            var self = (MemBufferFileIo*) userData;
            return self->m_dataSize;
        }

        public static long doSeek(long offset, SEEK whence, void* userData)
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

        public static long doRead(void* ptr, long count, void* userData)
        {
            var self = (MemBufferFileIo*)userData;
            if (self->m_where + count > self->m_dataSize)
                count = self->m_dataSize - self->m_where;

            var buf = (byte*) ptr;
            for (long i = 0; i < count; i++)
                buf[i] = self->m_data[self->m_where + i];
            self->m_where += count;
            return count;
        }

        public static long doWrite(void* ptr, long count, void* userData)
        {
            return 0; // Read-only
        }

        public static long doTell(void* userData)
        {
            var self = (MemBufferFileIo*)userData;
            return self->m_where;
        }

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

        private byte* m_data;
        private long m_dataSize;
        private long m_where;
    }
}
