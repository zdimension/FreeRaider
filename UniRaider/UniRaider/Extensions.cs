using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace UniRaider
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

        public static ushort[] ReadUInt16Array(this BinaryReader br, long arrLength)
        {
            var arr = new ushort[arrLength];
            for (long i = 0; i < arrLength; i++)
                arr[i] = br.ReadUInt16();
            return arr;
        }

        public static uint[] ReadUInt32Array(this BinaryReader br, long arrLength)
        {
            var arr = new uint[arrLength];
            for (long i = 0; i < arrLength; i++)
                arr[i] = br.ReadUInt32();
            return arr;
        }

        public static short[] ReadInt16Array(this BinaryReader br, long arrLength)
        {
            var arr = new short[arrLength];
            for (long i = 0; i < arrLength; i++)
                arr[i] = br.ReadInt16();
            return arr;
        }

        public static string[] ReadStringArray(this BinaryReader br, long arrLength, byte xorKey = 0)
        {
            var arr = new string[arrLength];
            var stringOffsets = br.ReadUInt16Array(arrLength).ToList();
            var stringDataSize = br.ReadUInt16();
            var tmp = new char[stringDataSize];
            for (ushort i = 0; i < stringDataSize; i++)
            {
                tmp[i] = (char)(br.ReadByte() ^ xorKey);
            }
            for (var i = 0; i < arrLength; i++)
            {
                var st = stringOffsets[i];
                var s = new string(tmp, st, i == arrLength - 1 ? tmp.Length - st : stringOffsets[i + 1] - st);
                if (s[s.Length - 1] == '\0') s = s.Remove(s.Length - 1);
                arr[i] = s.ConvertTR2Accent();
            }
            return arr;
        }

        public static string[] XORArray(this IList<string> arr, int key)
        {
            return arr.Select(x => x.XOR(key)).ToArray();
        }

        public static T[] ReadArray<T>(this BinaryReader br, long arrLength, Func<T> rd = null)
        {
            var arr = new List<T>();
            var tc = Type.GetTypeCode(typeof(T));
            Func<dynamic> reader = null;

            if(rd != null) reader = () => rd();

            if (reader == null)
            {
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
            }

            if (reader != null)
            {
                for (long i = 0; i < arrLength; i++)
                {
                    arr.Add(reader());
                }
            }
            return arr.ToArray();
        }

        public static string ConvertTR2Accent(this string s)
        {
            var repl = new Dictionary<string, string>
            {
                {"Red)marrer un niveau", "Redémarrer un niveau" }
            };

            if (repl.ContainsKey(s)) return repl[s];

            var t = s;

            t = Regex.Replace(t, @"\)(\w)", "$1\u0301");
            t = Regex.Replace(t, @"\((\w)", "$1\u0302");
            t = Regex.Replace(t, @"\$(\w)", "$1\u0300");
            t = t.Normalize(NormalizationForm.FormC);
            return t;
        }

        public static string XOR(this string s, int key)
        {
            var sb = new StringBuilder();
            foreach (var t in s)
                sb.Append((char)(t ^ key));
            return sb.ToString();
        }

        public static void Replace<T>(this T[] items, T oldValue, T newValue)
        {
            for (var i = 0; i < items.Length; i++)
                if (items[i].Equals(oldValue))
                    items[i] = newValue;
        }
    }
}
