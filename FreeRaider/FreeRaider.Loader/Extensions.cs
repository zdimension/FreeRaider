using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace FreeRaider
{
    public  static class Extensions
    {
        public static string StopAtNull(this string s)
        {
            var i = s.IndexOf('\0');
            if (i == -1) return s;
            return s.Substring(0, i);
        }

        public static byte[] GetBytes<T>(this T s)
            where T : struct
        {
            var size = Marshal.SizeOf(s);
            var ptr = Marshal.AllocHGlobal(size);
            var buf = new byte[size];
            Marshal.StructureToPtr(s, ptr, true);
            Marshal.Copy(ptr, buf, 0, size);
            Marshal.FreeHGlobal(ptr);
            return buf;
        }

        public static unsafe void ReadToPtr(this BinaryReader br, void* ptr, int length)
        {
            var buf = new byte[length];
            br.BaseStream.Read(buf, 0, length);
            Marshal.Copy(buf, 0, (IntPtr)ptr, length);
        }

        public static void WriteStruct<T>(this BinaryWriter bw, T s)
            where T : struct
        {
            bw.Write(s.GetBytes());
        }

        public static void WriteStructArray<T>(this BinaryWriter bw, T[] arr, long length = -1)
            where T : struct
        {
            if (length == -1) length = arr.Length;
            for (long i = 0; i < length; i++)
            {
                bw.Write(arr[i].GetBytes());
            }
        }

        public static string ParseString(this BinaryReader br, long strLength, bool stopAtZero = false)
        {
            var str = new byte[strLength];
            for (long i = 0; i < strLength; i++)
                str[i] = br.ReadByte();
            var ret = Encoding.ASCII.GetString(str);
            if (stopAtZero) return ret.StopAtNull();
            return ret;
        }

        public static T[] AddArray<T>(this T[] a, params T[] b)
        {
            var r = new T[a.Length + b.Length];
            a.CopyTo(r, 0);
            b.CopyTo(r, a.Length);
            return r;
        }

        public static unsafe void WriteString(this BinaryWriter bw, string s, byte xorKey = 0, Encoding enc = null, int length = -1)
        {
            if (enc == null) enc = Encoding.ASCII;
            var bs = enc.GetBytes(s);
            var ltow = Math.Min(length, bs.Length);
            if (xorKey != 0)
                fixed(byte* ptr = bs)
                for (var i = 0; i < ltow; i++) ptr[i] = (byte)(ptr[i] ^ xorKey);
            if (length != bs.Length)
            {
                var buf = new byte[length];
                Buffer.BlockCopy(bs, 0, buf, 0, ltow);
                bw.Write(buf);
            }
            else bw.Write(bs);
        }

        public static void WriteStringASCII(this BinaryWriter bw, string s, int length)
        {
            bw.Write(Encoding.ASCII.GetBytes(s).Resize(length));
        }

        public static unsafe T ReadStruct<T>(this BinaryReader br)
            where T : struct
        {
            T ret;
            var buf = br.ReadBytes(Marshal.SizeOf(typeof (T)));
            fixed (byte* p = buf)
                ret = (T)Marshal.PtrToStructure((IntPtr) p, typeof (T));
            return ret;
        }

        public static unsafe T[] ReadStructArray<T>(this BinaryReader br, long arrLength)
            where T : struct
        {
            var ret = new T[arrLength];
            var sz = Marshal.SizeOf(typeof (T));

            byte[] buf = new byte[sz];
            
            for (long i = 0; i < arrLength; i++)
            {
                buf = br.ReadBytes(sz);
                fixed (byte* b = buf)
                {
                    ret[i] = (T) Marshal.PtrToStructure((IntPtr)b, typeof (T));
                }
            }
            return ret;
        }

        public static unsafe sbyte[] ReadSByteArray(this BinaryReader br, long arrLength)
        {
            var arr = new sbyte[arrLength];
            fixed(sbyte* ptr = arr)
            for (long i = 0; i < arrLength; i++)
                ptr[i] = br.ReadSByte();
            return arr;
        }

        public static unsafe ushort[] ReadUInt16Array(this BinaryReader br, long arrLength)
        {
            var arr = new ushort[arrLength];
            fixed(ushort* ptr = arr)
            for (long i = 0; i < arrLength; i++)
                ptr[i] = br.ReadUInt16();
            return arr;
        }

        public static unsafe void WriteUInt16Array(this BinaryWriter bw, ushort[] arr)
        {
            fixed(ushort* ptr = arr)
            for (var i = 0; i < arr.Length; i++)
                bw.Write(ptr[i]);
        }

        public static unsafe uint[] ReadUInt32Array(this BinaryReader br, long arrLength)
        {
            var arr = new uint[arrLength];
            fixed(uint* ptr = arr)
            for (long i = 0; i < arrLength; i++)
                ptr[i] = br.ReadUInt32();
            return arr;
        }

        public static unsafe void WriteUInt32Array(this BinaryWriter bw, uint[] arr)
        {
            fixed(uint* ptr = arr)
            for (var i = 0; i < arr.Length; i++)
                bw.Write(ptr[i]);
        }

        public static unsafe short[] ReadInt16Array(this BinaryReader br, long arrLength)
        {
            var arr = new short[arrLength];
            fixed(short* ptr = arr)
            for (long i = 0; i < arrLength; i++)
                ptr[i] = br.ReadInt16();
            return arr;
        }

        public static unsafe void WriteInt16Array(this BinaryWriter bw, short[] arr)
        {
            fixed(short* ptr = arr)
            for (var i = 0; i < arr.Length; i++)
                bw.Write(ptr[i]);
        }

        public static unsafe void WriteSByteArray(this BinaryWriter bw, sbyte[] arr)
        {
            fixed(sbyte* ptr = arr)
            for (var i = 0; i < arr.Length; i++)
                bw.Write(ptr[i]);
        }

        public static unsafe string[] ReadStringArray(this BinaryReader br, long arrLength, byte xorKey = 0)
        {
            var arr = new string[arrLength];
            var stringOffsets = br.ReadUInt16Array(arrLength).ToList();
            var stringDataSize = br.ReadUInt16();
            var tmp = new char[stringDataSize];
            fixed(char* ptr = tmp)
            for (ushort i = 0; i < stringDataSize; i++)
            {
                ptr[i] = (char)(br.ReadByte() ^ xorKey);
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

        public static string[] ReadStringArray(this BinaryReader br, ushort[] offsetTable, byte xorKey = 0, Encoding enc = null)
        {
            var arr = new string[offsetTable.Length];
            var start = br.BaseStream.Position;

            for (var i = 0; i < offsetTable.Length; i++)
            {
                br.BaseStream.Position = start + offsetTable[i];
                arr[i] = br.ReadStringUntil(xorKey, enc);
            }

            return arr;
        }

        public static string ReadString(this BinaryReader br, int length, Encoding enc = null)
        {
            return (enc ?? Encoding.ASCII).GetString(br.ReadBytes(length));
        }

        public static string ReadStringUntil(this BinaryReader br, byte xor = 0, Encoding enc = null, int ch = 0, int length = 0)
        {
            if (enc == null) enc = Encoding.ASCII;
            var ret = new StringBuilder();

            if (length != 0)
            {
                var bs = br.ReadBytes(length);
                for (var i = 0; i < length; i++)
                {
                    if (bs[i] == ch) break;
                    ret.Append(enc.GetChars(new[] {(byte) (bs[i] ^ xor)})[0]);
                }

            }
            else
            {
                while (br.PeekChar() != ch)
                    ret.Append(enc.GetChars(new[] {(byte) (br.ReadByte() ^ xor)})[0]);
                br.ReadByte();
            }

            return ret.ToString();
        }

        public static T[] ReadArray<T>(this BinaryReader br, long arrLength, Func<T> rd = null)
        {
            var arr = new List<T>();
            var tc = Type.GetTypeCode(typeof(T));
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
                            typeof(T).GetMethods(BindingFlags.Public | BindingFlags.Static)
                                .FirstOrDefault(x => x.Name == "Parse");
                        if (m != null)
                        {
                            reader = () => m.Invoke(null, new[] { br });
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
            var tc = Type.GetTypeCode(typeof(T));
            Action<dynamic> writer = null;

            if (wr != null) writer = x => wr(x);

            if (writer == null)
            {
                switch (tc)
                {
                    case TypeCode.Byte:
                        writer = x => bw.Write((byte)x);
                        break;
                    case TypeCode.SByte:
                        writer = x => bw.Write((sbyte)x);
                        break;
                    case TypeCode.UInt16:
                        writer = x => bw.Write((ushort)x);
                        break;
                    case TypeCode.UInt32:
                        writer = x => bw.Write((uint)x);
                        break;
                    case TypeCode.UInt64:
                        writer = x => bw.Write((ulong)x);
                        break;
                    case TypeCode.Int16:
                        writer = x => bw.Write((short)x);
                        break;
                    case TypeCode.Int32:
                        writer = x => bw.Write((int)x);
                        break;
                    case TypeCode.Int64:
                        writer = x => bw.Write((long)x);
                        break;
                    case TypeCode.Decimal:
                        writer = x => bw.Write((decimal)x);
                        break;
                    case TypeCode.Double:
                        writer = x => bw.Write((double)x);
                        break;
                    case TypeCode.Single:
                        writer = x => bw.Write((float)x);
                        break;
                    default:
                        var m =
                            typeof(T).GetMethods(BindingFlags.Public | BindingFlags.Static)
                                .FirstOrDefault(x => x.Name == "Write");
                        if (m != null)
                        {
                            writer = x => m.Invoke(x, new[] { bw });
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
                sb.Append((char)(byte)((byte)t ^ key));
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

        public static T[] Resize<T>(this T[] arr, int size, T def = default(T))
        {
            var res = new T[size];
            for (var i = 0; i < size; i++) res[i] = def;
            if (arr != null)
                Array.Copy(arr, 0, res, 0, Math.Min(size, arr.Length));
            return res;
        }

        public static string RemoveComments(this string s)
        {
            s = s.Replace("\r\n", "\n");
            if (s.Contains("\n"))
            {
                var res = "";
                var comment = false;
                for (var i = 0; i < s.Length; i++)
                {
                    if (s[i] == '\n')
                    {
                        comment = false;
                    }
                    if (comment) continue;
                    if (i != s.Length - 1 && s[i] == '/' && s[i + 1] == '/')
                    {
                        comment = true;
                        continue;
                    }
                    res += s[i];
                }
                return res;
            }
            var id = s.IndexOf("//");
            if (id != -1)
                return s.Substring(0, s.IndexOf("//"));
            return s;
        }

        public static string[] SplitAtFirst(this string s, char sep)
        {
            var idx = s.IndexOf(sep);
            if (idx == -1) return new[] { "", "" };
            return new[] { s.Substring(0, idx), idx + 1 < s.Length ? s.Substring(idx + 1) : "" };
        }

        public static T[][] ReadMultiArray<T>(this BinaryReader br, Func<T[]> gen, int n)
        {
            var ret = new T[n][];
            for (var i = 0; i < n; i++)
                ret[i] = gen();
            return ret;
        }

        public static string Dump(this object o)
        {
            var fields = o.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public)
                .Select(x => new {Name = x.Name, Value = x.GetValue(o)})
                .Concat(o.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Select(x => new {Name = x.Name, Value = x.GetValue(o, null)}));
            return string.Join("\n", fields.Select(x => x.Name + " = " + x.Value));
        }
    }
}
