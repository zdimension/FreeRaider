using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Audio.OpenAL;
using UniRaider.Loader;

namespace UniRaider
{
    public partial class Helper
    {
        public static Loader.Game ParseVersion(BinaryReader br, string fext)
        {
            fext = fext.ToUpper();
            var check = br.ReadBytes(4);
            var ver = check[0] | (uint)(check[1] << 8) | (uint)(check[2] << 16) | (uint)(check[3] << 24);
            switch (fext)
            {
                case ".PHD":
                    if (ver == 0x00000020)
                        return Loader.Game.TR1;
                    break;
                case ".TUB":
                    if (ver == 0x00000020)
                        return Loader.Game.TR1UnfinishedBusiness;
                    break;
                case ".TR2":
                    if (ver == 0x0000002D)
                        return Loader.Game.TR2;
                    else if ((check[0] == 0x38 || check[0] == 0x34) &&
                             (check[1] == 0x00) &&
                             (check[2] == 0x18 || check[2] == 0x08) &&
                             (check[3] == 0xFF))
                        return Loader.Game.TR3;
                    break;
                case ".TR4":
                    if (ver == 0x00345254 || ver == 0x63345254 || ver == 0xFFFFFFF0)
                        return Loader.Game.TR4;
                    break;
                case ".TRC":
                    if (ver == 0x00345254)
                        return Loader.Game.TR5;
                    break;
            }
            return Loader.Game.Unknown;
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
    }

    public static class ALExt
    {
        public static void Listener(ALListener3f param, Vector3 value)
        {
            AL.Listener(param, value.X, value.Y, value.Z);
        }
    }
}
