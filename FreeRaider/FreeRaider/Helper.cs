﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Audio.OpenAL;
using FreeRaider.Loader;
using Ionic.Zlib;

namespace FreeRaider
{
    public partial class Helper
    {
        public static Game ParseVersion(BinaryReader br, string fext)
        {
            fext = fext.ToUpper();
            var check = br.ReadBytes(4);
            var ver = check[0] | (uint)(check[1] << 8) | (uint)(check[2] << 16) | (uint)(check[3] << 24);
            switch (fext)
            {
                case ".PHD":
                    if (ver == 0x00000020)
                        return Game.TR1;
                    break;
                case ".TUB":
                    if (ver == 0x00000020)
                        return Game.TR1UnfinishedBusiness;
                    break;
                case ".TR2":
                    if (ver == 0x0000002D)
                        return Game.TR2;
                    else if ((check[0] == 0x38 || check[0] == 0x34) &&
                             (check[1] == 0x00) &&
                             (check[2] == 0x18 || check[2] == 0x08) &&
                             (check[3] == 0xFF))
                        return Game.TR3;
                    break;
                case ".TR4":
                    if (ver == 0x00345254 || ver == 0x63345254 || ver == 0xFFFFFFF0)
                        return Game.TR4;
                    break;
                case ".TRC":
                    if (ver == 0x00345254)
                        return Game.TR5;
                    break;
            }
            return Game.Unknown;
        }

        public static Random Random = null;


        public static int CPPRand()
        {
            if (Random == null) return 0;
            return Random.Next(0, ushort.MaxValue);
        }

        public static string __FILE__ => new StackTrace(new StackFrame(true)).GetFrame(0).GetFileName();

        public static int __LINE__ => new StackTrace(new StackFrame(true)).GetFrame(0).GetFileLineNumber();

        public static void Quat_SetRotation(ref Quaternion quat, Vector3 axis, float angle)
        {
            float d = axis.Length;
            // TODO: Assert d != 0
            float s = (float)Math.Sin(angle * 0.5) / d;
            quat = new Quaternion(axis * s, (float)Math.Cos(angle * 0.5));
        }

        public static Vector3 ZeroW = Vector3.Zero;

        public static void SetInterpolate3(ref Vector3 v, Vector3 v0, Vector3 v1, float rt)
        {
            var s = 1.0f - rt;
            v.X = s * v0.X + rt * v1.X;
            v.Y = s * v0.Y + rt * v1.Y;
            v.Z = s * v0.Z + rt * v1.Z;
        }

        public static BinaryReader Decompress(byte[] compressed)
        {
            var uncompBuffer = ZlibStream.UncompressBuffer(compressed);

            return new BinaryReader(new MemoryStream(uncompBuffer));
        }


        // http://stackoverflow.com/a/22867582/2196124
        public static T[] FillArray<T>(T val, int count)
        {
            var value = new[] { val };
            var destinationArray = new T[count];


            // set the initial array value
            Array.Copy(value, destinationArray, value.Length);

            int arrayToFillHalfLength = destinationArray.Length / 2;
            int copyLength;

            for (copyLength = value.Length; copyLength < arrayToFillHalfLength; copyLength <<= 1)
            {
                Array.Copy(destinationArray, 0, destinationArray, copyLength, copyLength);
            }

            Array.Copy(destinationArray, 0, destinationArray, copyLength, destinationArray.Length - copyLength);



            return destinationArray;
        }

        public static void FillArray<T>(T val, T[] destinationArray)
        {
            var value = new[] { val };


            // set the initial array value
            Array.Copy(value, destinationArray, value.Length);

            int arrayToFillHalfLength = destinationArray.Length / 2;
            int copyLength;

            for (copyLength = value.Length; copyLength < arrayToFillHalfLength; copyLength <<= 1)
            {
                Array.Copy(destinationArray, 0, destinationArray, copyLength, copyLength);
            }

            Array.Copy(destinationArray, 0, destinationArray, copyLength, destinationArray.Length - copyLength);
        }

        [STAThread]
        public static string GetClipboardText()
        {
            if (IsRunningOnMono)
            {
                /*var fullExePath = new FileInfo(Assembly.GetExecutingAssembly().Location);
                var gtkAss = Assembly.LoadFile(Path.Combine(fullExePath.DirectoryName, "GtkClipboard.dll"));
                var assMethod = gtkAss.GetTypes()[0].GetMethod("GetGtkText");
                txtClipboard.Text = assMethod.Invoke(null, new object[] { }) as string;*/
                try
                {
                    return Clipboard.GetText();
                }
                catch
                {
                    return "";
                }
            }
            else {
                return Clipboard.GetText();
            }
        }

        private static bool IsRunningOnMono => (Type.GetType("Mono.Runtime") != null);

        public static MethodInfo GetMethodInfo(Delegate d)
        {
            return d.Method;
        }

        public static T CreateInstance<T>(params object[] args)
        {
            return (T) System.Activator.CreateInstance(typeof (T), args);
        }

        public static unsafe float[] GetArrayFromPointer(float* ptr, int count)
        {
            var ret = new float[count];
            for (var i = 0; i < count; i++)
            {
                ret[i] = ptr[i];
            }
            return ret;
        }
    }

    public static class ALExt
    {
        public static void Listener(ALListener3f param, Vector3 value)
        {
            AL.Listener(param, value.X, value.Y, value.Z);
        }
    }
}
