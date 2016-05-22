using System;
using System.Runtime.InteropServices;

namespace NLibsndfile.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public struct LibsndfileInstrumentInfo
    {
        public int Gain;
        public short BaseNote;
        public short Detune;
        public short VelocityLow;
        public short VelocityHigh;
        public short KeyLow;
        public short KeyHigh;
        public int LoopCount;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public LibsndfileLoop[] Loops;

        [StructLayout(LayoutKind.Sequential)]
        public struct LibsndfileLoop
        {
            public int Mode;
            public uint Start;
            public uint End;
            public uint Count;
        }
    }
}