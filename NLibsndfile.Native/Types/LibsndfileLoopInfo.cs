using System;
using System.Runtime.InteropServices;

namespace NLibsndfile.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public struct LibsndfileLoopInfo
    {
        public short TimeSignatureNumerator;
        public short TimeSignatureDenominator;
        public LibsndfileLoopMode LoopMode;
        public int BeatsCount;
        public float BeatsPerMinute;
        public int RootKey;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public int[] Future;
    }
}