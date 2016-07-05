using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreeRaider.Loader
{
    /// <summary>
    /// Thanks trsearch.org for the specs
    /// </summary>
    public class CUTSEQFile
    {
        public List<CutsceneData> Cutscenes;

        public class CutsceneData
        {
            /// <summary>
            /// Actor 1 is always Lara
            /// </summary>
            public ushort NumActors;
            public ushort NumFrames;
            /// <summary>
            /// In TR world coordinates. Negative Y is up.
            /// </summary>
            public Vertex Origin;
            /// <summary>
            /// -1 means no audio track
            /// </summary>
            public int AudioTrackIndex;

            public List<CameraData> CameraData;
            public List<ActorData> ActorData;
        }

        public class CameraData
        {
            
        }

        public class PositionHeader
        {
            public short X;
            public short Y;
            public short Z;
            /// <summary>
            /// Bitsizes for each axis (X, Y, Z)
            /// </summary>
            public ushort Bitsizes;
            /// <summary>
            /// Number of values packed in X axis data
            /// </summary>
            public ushort PackedValuesX;
            /// <summary>
            /// Number of values packed in Y axis data
            /// </summary>
            public ushort PackedValuesY;
            /// <summary>
            /// Number of values packed in Z axis data
            /// </summary>
            public ushort PackedValuesZ;
        }

        public class RotationHeader
        {
            public short X;
            public short Y;
            public short Z;

            public float Xdegrees
            {
                get { return (X * 1024 / 360.0f) % 360; }
                set { X = (short) ((value / 360 * 1024) % 1024); }
            }
            public float Ydegrees
            {
                get { return (Y * 1024 / 360.0f) % 360; }
                set { Y = (short)((value / 360 * 1024) % 1024); }
            }
            public float Zdegrees
            {
                get { return (Z * 1024 / 360.0f) % 360; }
                set { Z = (short)((value / 360 * 1024) % 1024); }
            }
            /// <summary>
            /// Bitsizes for each axis (X, Y, Z)
            /// </summary>
            public ushort Bitsizes;
            /// <summary>
            /// Number of values packed in X axis data
            /// </summary>
            public ushort PackedValuesX;
            /// <summary>
            /// Number of values packed in Y axis data
            /// </summary>
            public ushort PackedValuesY;
            /// <summary>
            /// Number of values packed in Z axis data
            /// </summary>
            public ushort PackedValuesZ;
        }



        public class ActorData
        {
            
        }
    }
}
