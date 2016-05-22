using System;
using System.Runtime.InteropServices;

namespace NLibsndfile.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public struct LibsndfileEmbedFileInfo
    {
        public long Offset;
        public long Length;
    }
}