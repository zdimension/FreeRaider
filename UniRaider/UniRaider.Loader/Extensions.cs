using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace UniRaider.Loader
{
    public static class Extensions
    {
        public static string ParseString(this BinaryReader br, long strLength)
        {
            var str = new byte[strLength];
            for (long i = 0; i < strLength; i++)
                str[i] = br.ReadByte();
            return Encoding.ASCII.GetString(str);
        }

        public static byte[] ReadByteArray(this BinaryReader br, long arrLength)
        {
            var arr = new byte[arrLength];
            for (long i = 0; i < arrLength; i++)
                arr[i] = br.ReadByte();
            return arr;
        }

        public static ushort[] ReadUInt16Array(this BinaryReader br, long arrLength)
        {
            var arr = new ushort[arrLength];
            for (long i = 0; i < arrLength; i++)
                arr[i] = br.ReadUInt16();
            return arr;
        }

        public static string[] ReadStringArray(this BinaryReader br, long arrLength)
        {
            var arr = new List<string>();
            var stringOffsets = br.ReadUInt16Array(arrLength).ToList();
            var stringDataSize = br.ReadUInt16();
            var current = "";
            for(ushort i = 0; i < stringDataSize; i++)
            {
                if(i != 0 && stringOffsets.Contains(i))
                {
                    arr.Add(current);
                    current = "";
                    stringOffsets.Remove(i);
                }
                else
                {
                    current += (char) br.ReadByte();
                }
            }
            return arr.ToArray();
        }

        public static string[] XORArray(this IList<string> arr, int key)
        {
            return arr.Select(x => x.XOR(key)).ToArray();
        }

        public static T[] ReadArray<T>(this BinaryReader br, long arrLength)
        {
            var arr = new List<T>();
            var tc = Type.GetTypeCode(typeof(T));
            var reader = new Func<dynamic>(() => null);


            switch (tc)
            {
                case TypeCode.Byte:
                    reader = () => br.ReadByte();
                    break;
                case TypeCode.SByte:
                    reader = () => br.ReadSByte();
                    break;
                case TypeCode.UInt16:
                    reader = () => br.ReadUInt16();
                    break;
                case TypeCode.UInt32:
                    reader = () => br.ReadUInt32();
                    break;
                case TypeCode.UInt64:
                    reader = () => br.ReadUInt64();
                    break;
                case TypeCode.Int16:
                    reader = () => br.ReadInt16();
                    break;
                case TypeCode.Int32:
                    reader = () => br.ReadInt32();
                    break;
                case TypeCode.Int64:
                    reader = () => br.ReadInt64();
                    break;
                case TypeCode.Decimal:
                    reader = () => br.ReadDecimal();
                    break;
                case TypeCode.Double:
                    reader = () => br.ReadDouble();
                    break;
                case TypeCode.Single:
                    reader = () => br.ReadSingle();
                    break;
                default:
                    var m =
                        typeof (T).GetMethods(BindingFlags.Public | BindingFlags.Static)
                            .FirstOrDefault(x => x.Name == "Parse");
                    if (m != null)
                    {
                        reader = () => m.Invoke(null, new[] {br});
                    }
                    break;
            }

            for (long i = 0; i < arrLength; i++)
            {
                arr.Add(reader());
            }
            return arr.ToArray();
        }
    }
}
