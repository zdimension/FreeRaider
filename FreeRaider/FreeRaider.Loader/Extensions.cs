using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace FreeRaider
{
    internal static class Extensions
    {
        public static string ParseString(this BinaryReader br, long strLength, bool stopAtZero = false)
        {
            var str = new byte[strLength];
            for (long i = 0; i < strLength; i++)
                str[i] = br.ReadByte();
            var ret = Encoding.ASCII.GetString(str);
            if (stopAtZero) return ret.Substring(0, ret.IndexOf('\0'));
            return ret;
        }

        public static T[] AddArray<T>(this T[] a, params T[] b)
        {
            var r = new T[a.Length + b.Length];
            a.CopyTo(r, 0);
            b.CopyTo(r, a.Length);
            return r;
        }

        public static void WriteStringASCII(this BinaryWriter bw, string s)
        {
            bw.Write(Encoding.ASCII.GetBytes(s));
        }

        public static void WriteStringASCII(this BinaryWriter bw, string s, int length)
        {
            bw.Write(Encoding.ASCII.GetBytes(s).Resize(length));
        }

        public static ushort[] ReadUInt16Array(this BinaryReader br, long arrLength)
        {
            var arr = new ushort[arrLength];
            for (long i = 0; i < arrLength; i++)
                arr[i] = br.ReadUInt16();
            return arr;
        }

        public static void WriteUInt16Array(this BinaryWriter bw, ushort[] arr)
        {
            for (var i = 0; i < arr.Length; i++)
                bw.Write(arr[i]);
        }

        public static uint[] ReadUInt32Array(this BinaryReader br, long arrLength)
        {
            var arr = new uint[arrLength];
            for (long i = 0; i < arrLength; i++)
                arr[i] = br.ReadUInt32();
            return arr;
        }

        public static void WriteUInt32Array(this BinaryWriter bw, uint[] arr)
        {
            for (var i = 0; i < arr.Length; i++)
                bw.Write(arr[i]);
        }

        public static short[] ReadInt16Array(this BinaryReader br, long arrLength)
        {
            var arr = new short[arrLength];
            for (long i = 0; i < arrLength; i++)
                arr[i] = br.ReadInt16();
            return arr;
        }

        public static void WriteInt16Array(this BinaryWriter bw, short[] arr)
        {
            for (var i = 0; i < arr.Length; i++)
                bw.Write(arr[i]);
        }

        public static string[] ReadStringArray(this BinaryReader br, long arrLength, byte xorKey = 0)
        {
            var arr = new string[arrLength];
            var stringOffsets = br.ReadUInt16Array(arrLength).ToList();
            var stringDataSize = br.ReadUInt16();
            var tmp = new char[stringDataSize];
            for (ushort i = 0; i < stringDataSize; i++)
            {
                tmp[i] = (char) (br.ReadByte() ^ xorKey);
            }
            for (var i = 0; i < arrLength; i++)
            {
                var st = stringOffsets[i];
                var s = new string(tmp, st, i == arrLength - 1 ? tmp.Length - st : stringOffsets[i + 1] - st);
                if (s[s.Length - 1] == '\0') s = s.Remove(s.Length - 1);
                arr[i] = s.ConvertFromTR2Accent();
            }
            return arr;
        }

        public static void WriteStringArray(this BinaryWriter bw, string[] arr, byte xorKey = 0)
        {
            var converted = arr.Select(x => (Encoding.ASCII.GetString(Encoding.ASCII.GetBytes((x ?? "").ConvertToTR2Accent())) + "\0").XOR(xorKey)).ToArray();
            var strOffsets = new ushort[converted.Length];
            ushort totalOffset = 0;
            for (var i = 0; i < converted.Length; i++)
            {
                strOffsets[i] = totalOffset;         
                totalOffset += (ushort)converted[i].Length;
            }
            bw.WriteUInt16Array(strOffsets);
            bw.Write(totalOffset);
            foreach (var s in converted)
            {
                bw.Write(s.Select(x => (byte)x).ToArray());
            }
        }

        public static string[] ReadStringArray(this BinaryReader br, ushort[] offsetTable, byte xorKey = 0)
        {
            var arr = new string[offsetTable.Length];
            var start = br.BaseStream.Position;

            for (var i = 0; i < offsetTable.Length; i++)
            {
                br.BaseStream.Position = start + offsetTable[i];
                arr[i] = br.ReadASCIIStringUntilNull().XOR(xorKey);
            }

            return arr;
        }

        public static string ReadASCIIStringUntilNull(this BinaryReader br)
        {
            var ret = new StringBuilder();

            while (br.PeekChar() != 0)
                ret.Append((char) br.ReadByte());

            return ret.ToString();
        }

        public static T[] ReadArray<T>(this BinaryReader br, long arrLength, Func<T> rd = null)
        {
            var arr = new List<T>();
            var tc = Type.GetTypeCode(typeof (T));
            Func<dynamic> reader = null;

            if (rd != null) reader = () => rd();

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

        public static bool HasFlagUns(this object a, object f)
        {
            return (Convert.ToUInt64(a) & Convert.ToUInt64(f)) == Convert.ToUInt64(f);
        }

        public static void WriteArray<T>(this BinaryWriter bw, IEnumerable<T> arr, Action<T> wr = null, int len = -1)
        {
            var tc = Type.GetTypeCode(typeof (T));
            Action<dynamic> writer = null;

            if (wr != null) writer = x => wr(x);

            if (writer == null)
            {
                switch (tc)
                {
                    case TypeCode.Byte:
                        writer = x => bw.Write((byte) x);
                        break;
                    case TypeCode.SByte:
                        writer = x => bw.Write((sbyte) x);
                        break;
                    case TypeCode.UInt16:
                        writer = x => bw.Write((ushort) x);
                        break;
                    case TypeCode.UInt32:
                        writer = x => bw.Write((uint) x);
                        break;
                    case TypeCode.UInt64:
                        writer = x => bw.Write((ulong) x);
                        break;
                    case TypeCode.Int16:
                        writer = x => bw.Write((short) x);
                        break;
                    case TypeCode.Int32:
                        writer = x => bw.Write((int) x);
                        break;
                    case TypeCode.Int64:
                        writer = x => bw.Write((long) x);
                        break;
                    case TypeCode.Decimal:
                        writer = x => bw.Write((decimal) x);
                        break;
                    case TypeCode.Double:
                        writer = x => bw.Write((double) x);
                        break;
                    case TypeCode.Single:
                        writer = x => bw.Write((float) x);
                        break;
                    default:
                        var m =
                            typeof (T).GetMethods(BindingFlags.Public | BindingFlags.Static)
                                .FirstOrDefault(x => x.Name == "Write");
                        if (m != null)
                        {
                            writer = x => m.Invoke(x, new[] {bw});
                        }
                        break;
                }
            }

            if (writer != null)
            {
                if (len != -1)
                {
                    var a = arr.ToArray();
                    for (var i = 0; i < len; i++)
                    {
                        writer(a[i]);
                    }
                }
                else
                    foreach (var i in arr)
                        writer(i);
            }
        }

        internal static Dictionary<string, string> AccentHack = new Dictionary<string, string>
        {
            {"Red)marrer un niveau", "Redémarrer un niveau"}
        };

        public static string ConvertFromTR2Accent(this string s)
        {
            s = s.Trim();

            if (AccentHack.ContainsKey(s)) return AccentHack[s];

            s = Regex.Replace(s, @"\)(\w)", "$1\u0301");
            s = Regex.Replace(s, @"\((\w)", "$1\u0302");
            s = Regex.Replace(s, @"\$(\w)", "$1\u0300");
            s = Regex.Replace(s, @"~(\w)", "$1\u0308");
            s = s.Replace('=', 'ß');
            s = s.Normalize(NormalizationForm.FormC);
            return s;
        }

        public static string ConvertToTR2Accent(this string s)
        {
            s = s.Trim();

            if (AccentHack.ContainsValue(s)) return AccentHack.First(x => x.Value == s).Key;

            s = s.Normalize(NormalizationForm.FormD);
            s = Regex.Replace(s, @"(\w)\u0301", ")$1");
            s = Regex.Replace(s, @"(\w)\u0302", "($1");
            s = Regex.Replace(s, @"(\w)\u0300", "$$$1");
            s = Regex.Replace(s, @"(\w)\u0308", "~$1");
            s = s.Replace('ß', '=');
            
            return s;
        }

        public static string XOR(this string s, byte key)
        {
            var sb = new StringBuilder();
            foreach (var t in s)
                sb.Append((char) (byte)((byte)t ^ key));
            return sb.ToString();
        }

        public static void Replace<T>(this T[] items, T oldValue, T newValue)
        {
            for (var i = 0; i < items.Length; i++)
                if (items[i].Equals(oldValue))
                    items[i] = newValue;
        }

        public static bool HasFlagEx(this object theField, object theFlag)
        {
            return (Enum.ToObject(theFlag.GetType(), theField) as Enum).HasFlag(theFlag as Enum);
        }

        public static bool IsAnyOf(this object val, params object[] vals)
        {
            return vals.Any(x => val == x || x == val || val.Equals(x) || x.Equals(val));
        }

        /// <summary>
        /// A FX 3.5 way to mimic the FX4 "HasFlag" method.
        /// </summary>
        /// <param name="variable">The tested enum.</param>
        /// <param name="value">The value to test.</param>
        /// <returns>True if the flag is set. Otherwise false.</returns>
        public static bool HasFlag(this Enum variable, Enum value)
        {
            // check if from the same type.
            if (variable.GetType() != value.GetType())
            {
                throw new ArgumentException("The checked flag is not from the same type as the checked variable.");
            }

            ulong num = Convert.ToUInt64(value);
            ulong num2 = Convert.ToUInt64(variable);

            return (num2 & num) == num;
        }

        public static T[] Resize<T>(this T[] arr, int size)
        {
            var res = new T[size];
            Array.Copy(arr, 0, res, 0, Math.Min(size, arr.Length));
            return res;
        }
    }
}
