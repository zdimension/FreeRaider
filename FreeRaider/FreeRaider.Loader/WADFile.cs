using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace FreeRaider.Loader
{
    // Source: http://www.xythobuz.de/tr_docs/TR_WAD_file_format.pdf and http://www.trsearch.org/v5/wiki/Source:WAD.h
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe class WADFile
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct vertex3s
        {
            public short x;
            public short y;
            public short z;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct color3
        {
            public byte R;
            public byte G;
            public byte B;
        }

        /*[StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct WAD_Face
        {
            public ushort FaceType;        // If it's 8, this face is a triangle, If it's 9, it's a rectangle
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public ushort[] Vertices;        // 3 vertices for a triangle and 4 for a rectangle
            public ushort Texture;            // Texture
            public ushort Flags;            // Flags 
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct WAD_TextInfo
        {
            public byte x;
            public byte y;
            public ushort Tile;
            public byte FlipX;
            public byte XSize;
            public byte FlipY;
            public byte YSize;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct WAD_Mesh
        {
            public vertex3s Centre;
            public uint CollesionSize;
            public ushort NumVertices;
            public vertex3s* Vertices;
            public short NumNormals;
            public vertex3s[] Normals;        // If NumNormals is positive
            public ushort* Lights;        // If NumNormals is negative
            public ushort NumFaces;
            [MarshalAs(UnmanagedType.LPArray)]
            public WAD_Face[] Faces;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct WAD_Animation
        {
            public uint FrameOffset;
            public byte FrameRate;
            public byte FrameSize;
            public ushort StateID;
            public ushort Unknown1;
            public ushort Speed;
            public float Allec;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] Unknown2;
            public ushort FrameStart;
            public ushort FrameEnd;
            public ushort NextAnimation;
            public ushort NextFrame;
            public ushort NumStateChanges;
            public ushort StateChange;
            public ushort NumAnimCommands;
            public ushort AnimCommands;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct WAD_StateChange
        {
            public ushort StateID;
            public ushort NumAnimDispatches;
            public ushort AnimDispatches;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct WAD_AnimDispatch
        {
            public ushort Low;
            public ushort High;
            public ushort NextAnimation;
            public ushort NextFrame;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct WAD_Moveable
        {
            public uint ObjectID;
            public ushort NumMeshes;
            public ushort FirstMesh;
            public uint MeshTree;
            public uint Frame;
            public short Animation;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct WAD_StaticMesh
        {
            public uint ObjectID;
            public ushort Mesh;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]public vertex3s BoundBox1;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public vertex3s BoundBox2;
            public ushort Flags;
        }*/

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct WAD_TextureSample
        {
            /// <summary>anchor corner x pixel position.</summary>
            public byte x;
            /// <summary>anchor corner y pixel position.</summary>
            public byte y;
            /// <summary>page where the texture sample is stored. </summary>
            public ushort page;
            /// <summary>horizontal flip, yes or no, -1 or 0.</summary>
            public sbyte flipX;
            /// <summary>number of pixels to add to the width.</summary>
            public byte addW;
            /// <summary>vertical flip, yes or no, -1 or 0.</summary>
            public sbyte addY;
            /// <summary>number of pixels to add to the height</summary>
            public byte addH;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct WAD_Texture
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256 * 256)]
            public color3[] Data;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct WAD_BoundingSphere
        {
            public vertex3s Centre;
            public ushort radius;
            public ushort unknown;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct WAD_MeshData
        {
            
        }


        /// <summary>A valid Tomb Raider WAD file has a value of 129 in its File_ID field.</summary>
        public uint File_ID = 129;
        /// <summary>Records containing the position, size and attitude of each texture sample stored in the WAD file.</summary>
        public WAD_TextureSample[] Texture_Samples_Table = new WAD_TextureSample[0];
        public WAD_Texture[] Textures = new WAD_Texture[0];


        public static WADFile Read(string fname)
        {
            using (var fs = File.OpenRead(fname))
            {
                return Read(fs);
            }
        }

        public static WADFile Read(Stream s)
        {
            using (var br = new BinaryReader(s))
            {
                return Read(br);
            }
        }

        public static WADFile Read(BinaryReader br)
        {
            var ret = new WADFile();



            return ret;
        }

        public void Write(string fname)
        {
            if (File.Exists(fname))
            {
                File.Delete(fname);
            }
            using (var fs = File.OpenWrite(fname))
            {
                Write(fs);
            }
        }

        public void Write(Stream s)
        {
            using (var bw = new BinaryWriter(s))
            {
                Write(bw);
            }
        }

        public void Write(BinaryWriter bw)
        {

        }
    }

    public class VarArrayMarshaler<TArr, TLength> : ICustomMarshaler
        where TLength: IConvertible
    {
        public static ICustomMarshaler GetInstance(String cookie)
        {
            return new VarArrayMarshaler<TArr, TLength>();
        }

        public object MarshalNativeToManaged(IntPtr pNativeData)
        {
            if (pNativeData == IntPtr.Zero) return null;
            long length;
            if (typeof (TLength) == typeof (sbyte)) length = (sbyte)Marshal.ReadByte(pNativeData);
            else if (typeof (TLength) == typeof (byte)) length = Marshal.ReadByte(pNativeData);
            else if (typeof (TLength) == typeof (short)) length = Marshal.ReadInt16(pNativeData);
            else if (typeof (TLength) == typeof (ushort)) length = (ushort)Marshal.ReadInt16(pNativeData);
            else if (typeof (TLength) == typeof (uint)) length = (uint)Marshal.ReadInt32(pNativeData);
            else length = Marshal.ReadInt32(pNativeData);
            var arr = new TArr[length];
            var size = Marshal.SizeOf(typeof(TArr));
            var start = pNativeData + Marshal.SizeOf(typeof (TLength));
            for (var i = 0; i < length; i++)
            {
                arr[i] = (TArr)Marshal.PtrToStructure(start + i * size, typeof(TArr));
            }
            return arr;
        }

        public IntPtr MarshalManagedToNative(object ManagedObj)
        {
            if(ManagedObj == null) return IntPtr.Zero;
            var arr = (TArr[]) ManagedObj;
            long length = (long) (dynamic) (TLength)(dynamic)arr.Length;
            if (length < 0) length = (long) (dynamic)(TLength)typeof (TLength).GetField("MaxValue").GetValue(null);
            var size = Marshal.SizeOf(typeof (TArr));
            var totalSize = Marshal.SizeOf(typeof (TLength)) + size;
            var ptr = Marshal.AllocHGlobal(totalSize);
            if (typeof(TLength) == typeof(sbyte)) Marshal.WriteByte(ptr, (byte)length);
            else if (typeof(TLength) == typeof(byte)) Marshal.WriteByte(ptr, (byte)length);
            else if (typeof(TLength) == typeof(short)) Marshal.WriteInt16(ptr, (short)length);
            else if (typeof(TLength) == typeof(ushort)) Marshal.WriteInt16(ptr, (short)length);
            else if (typeof(TLength) == typeof(uint)) Marshal.WriteInt32(ptr, (int)length);
            else Marshal.WriteInt32(ptr, (int)length);
            var start = ptr + Marshal.SizeOf(typeof(TLength));
            for (var i = 0; i < length; i++)
            {
                Marshal.StructureToPtr(arr[i], start + i * size, false);
            }
            return ptr;
        }

        public void CleanUpNativeData(IntPtr pNativeData)
        {
            Marshal.FreeHGlobal(pNativeData);
        }

        public void CleanUpManagedData(object ManagedObj)
        {
            
        }

        public int GetNativeDataSize()
        {
            return Marshal.SizeOf(typeof (TLength)) + Marshal.SizeOf(typeof (TArr));
        }
    }
}
