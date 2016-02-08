using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LibSndFile;
using OpenTK;
using OpenTK.Audio.OpenAL;

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
                tmp[i] = (char) (br.ReadByte() ^ xorKey);
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

        public static string ConvertTR2Accent(this string s)
        {
            var repl = new Dictionary<string, string>
            {
                {"Red)marrer un niveau", "Redémarrer un niveau"}
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
                sb.Append((char) (t ^ key));
            return sb.ToString();
        }

        public static void Replace<T>(this T[] items, T oldValue, T newValue)
        {
            for (var i = 0; i < items.Length; i++)
                if (items[i].Equals(oldValue))
                    items[i] = newValue;
        }

        public static Vector3 SafeNormalize(this Vector3 v)
        {
            var abs = v.Absolute();
            var maxIndex = abs.MaxAxis();
            if (abs[maxIndex] > 0)
            {
                v /= abs[maxIndex];
                return v /= v.Length;
            }
            return v = new Vector3(1.0f, 0.0f, 0.0f);
        }

        public static Vector3 Absolute(this Vector3 v)
        {
            return new Vector3(Math.Abs(v.X), Math.Abs(v.Y), Math.Abs(v.Z));
        }

        public static int MaxAxis(this Vector3 v)
        {
            return v.X < v.Y ? (v.Y < v.Z ? 2 : 1) : (v.X < v.Z ? 2 : 0);
        }

        public static int MinAxis(this Vector3 v)
        {
            return v.X < v.Y ? (v.X < v.Z ? 0 : 2) : (v.Y < v.Z ? 1 : 2);
        }

        public static Vector3 DivideByVector3(this float f, Vector3 v)
        {
            return new Vector3(f / v.X, f / v.Y, f / v.Z);
        }

        public static bool IsBetween(this float f, float a, float b, bool inclusive = true, bool reorder = true)
        {
            return ((double) f).IsBetween(a, b, inclusive, reorder);
        }

        public static bool IsBetween(this int f, int a, int b, bool inclusive = true, bool reorder = true)
        {
            return ((double) f).IsBetween(a, b, inclusive, reorder);
        }

        public static bool IsBetween(this double f, double a, double b, bool inclusive = true, bool reorder = true)
        {
            var c = a;
            var d = b;
            if (reorder)
            {
                c = Math.Min(a, b);
                d = Math.Max(a, b);
            }
            if (inclusive) return f >= c && f <= d;
            return f > c & f < d;
        }

        public static bool IsBetween(this Vector3 v, Vector3 a, Vector3 b, bool inclusive = true, bool reorder = true)
        {
            return v.X.IsBetween(a.X, b.X, inclusive, reorder)
                   && v.Y.IsBetween(a.Y, b.Y, inclusive, reorder)
                   && v.Z.IsBetween(a.Z, b.Z, inclusive, reorder);
        }

        public static bool LowerThan(this Vector3 v, Vector3 a, bool orequal = false)
        {
            if (orequal) return v.X <= a.X && v.Y <= a.Y && v.Z <= a.Z;
            return v.X < a.X && v.Y < a.Y && v.Z < a.Z;
        }

        public static bool HigherThan(this Vector3 v, Vector3 a, bool orequal = false)
        {
            if (orequal) return v.X >= a.X && v.Y >= a.Y && v.Z >= a.Z;
            return v.X > a.X && v.Y > a.Y && v.Z > a.Z;
        }

        public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector)
        {
            var cpr = Comparer<TKey>.Default;
            using (var it1 = source.GetEnumerator())
            {
                if (!it1.MoveNext())
                {
                    throw new InvalidOperationException("no items");
                }
                var max = it1.Current;
                var mkey = selector(max);
                while (it1.MoveNext())
                {
                    var c = it1.Current;
                    var k = selector(c);
                    if (cpr.Compare(k, mkey) > 0)
                    {
                        max = c;
                        mkey = k;
                    }
                }
                return max;
            }
        }

        public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector)
        {
            var cpr = Comparer<TKey>.Default;
            using (var it1 = source.GetEnumerator())
            {
                if (!it1.MoveNext())
                {
                    throw new InvalidOperationException("no items");
                }
                var max = it1.Current;
                var mkey = selector(max);
                while (it1.MoveNext())
                {
                    var c = it1.Current;
                    var k = selector(c);
                    if (cpr.Compare(k, mkey) < 0)
                    {
                        max = c;
                        mkey = k;
                    }
                }
                return max;
            }
        }

        public static T Clamp<T>(this T v, T min, T max) where T : IComparable<T>
        {
            if (v.CompareTo(max) > 0) return max;
            else if (v.CompareTo(min) < 0) return min;
            else return v;
        }

        public static SndFileInfo GetSndFileInfo(this SndFile f)
        {
            return
                typeof (SndFileInfo).GetField("sndFileInfo", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(f)
                    as SndFileInfo;
        }

        public static EffectsExtension.EfxEaxReverb ToEfxEaxReverb(this EffectsExtension.EaxReverb rb)
        {
            var ret = new EffectsExtension.EfxEaxReverb();
            EffectsExtension.GetEaxFromEfxEax(ref rb, out ret);
            return ret;
        }

        // http://stackoverflow.com/a/12232162/2196124
        public static void Resize<T>(this List<T> list, int sz, T c = default(T))
        {
            var cur = list.Count;
            if (sz < cur)
                list.RemoveRange(sz, cur - sz);
            else if (sz > cur)
                list.AddRange(Enumerable.Repeat(c, sz - cur));
        }

        public static Vector3 Rotate(this Vector3 vec, Vector3 wAxis, float angle)
        {
            var o = wAxis * wAxis.Dot(vec);
            var x = vec - o;
            var y = wAxis.Cross(vec);

            return o + x * (float)Math.Cos(angle) + y * (float)Math.Sin(angle);
        }

        public static bool HasFlagEx(this object theField, object theFlag)
        {
            return (Enum.ToObject(theFlag.GetType(), theField) as Enum).HasFlag(theFlag as Enum);
        }

        public static float Dot(this Vector3 v1, Vector3 v2)
        {
            return Vector3.Dot(v1, v2);
        }

        public static Vector3 Cross(this Vector3 v1, Vector3 v2)
        {
            return Vector3.Cross(v1, v2);
        }

        public static Vector3 MultiplyByVector(this Matrix3 m, Vector3 v)
        {
            return new Vector3(m.Row0.Dot(v), m.Row1.Dot(v), m.Row2.Dot(v));
        }

        public static Vector3 Dot3(this Vector3 v, Vector3 v0, Vector3 v1, Vector3 v2)
        {
            return new Vector3(v.Dot(v0), v.Dot(v1), v.Dot(v2));
        }

        public static Vector3 Lerp(this Vector3 a, Vector3 b, float bend)
        {
            return Vector3.Lerp(a, b, bend);
        }

        public static float Distance2(this Vector3 a, Vector3 b)
        {
            return (b - a).LengthSquared;
        }

        public static T Square<T>(this T t)
        {
            return (T) Convert.ChangeType((dynamic) t * (dynamic) t, typeof (T));
        }
    }
}
