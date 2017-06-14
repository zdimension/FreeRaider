using System;
using System.Runtime.InteropServices;
using Gtk;
using System.Reflection;
using System.Collections.Generic;

namespace TRLevelUtility
{
	public static class Extensions
	{
		public static T GetField<T>(this object a, string name)
			where T : class
		{
			return a.GetType().GetField(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(a) as T;
		}

		/*//[DllImport(typeof(Gtk.Global).GetField("GtkNativeDll").GetValue(null).ToString(), CallingConvention = CallingConvention.Cdecl)]
        public delegate IntPtr gtk_text_iter_copy_D(IntPtr raw);

		public static gtk_text_iter_copy_D gtk_text_iter_copy = 
			Helper.GetFunction<gtk_text_iter_copy_D>(
				((DllImportAttribute)typeof(Gtk.Global)
					.GetMethod ("gtk_set_locale", BindingFlags.Static | BindingFlags.NonPublic)
					.GetCustomAttributes (typeof(DllImportAttribute), false)[0]).Value
				, "gtk_text_iter_copy");

        public static TextIter CopyEx(this TextIter a)
        {
            IntPtr num = Marshal.AllocHGlobal(Marshal.SizeOf((object)a));
            Marshal.StructureToPtr((object)a, num, false);
            var textIter = TextIter.New(gtk_text_iter_copy(num));
            Marshal.FreeHGlobal(num);
            return textIter;
        }*/

		public static string[] AddArr(this string s, string[] arr)
		{
			var ret = new string[arr.Length + 1];
			ret[0] = s;
			arr.CopyTo(ret, 1);
			return ret;
		}

		public static int IndexOf<TSource>(this IEnumerable<TSource> source, TSource predicate)
		{
			var index = 0;
			foreach (var item in source)
			{
				if ((object)item == (object)predicate)
				{
					return index;
				}
				index++;
			}
			return -1;
		}

		public static int IndexOf<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
		{
			var index = 0;
			foreach (var item in source)
			{
				if (predicate.Invoke(item))
				{
					return index;
				}
				index++;
			}
			return -1;
		}

		public static string[] SplitEveryNocc(this string s, char c, int N = 2)
		{
			var ret = new List<string>();

			var curBuf = "";

			var curOcc = 0;

			for (var i = 0; i < s.Length; i++)
			{
				var cc = s[i];
				if (cc == c)
				{
					curOcc++;
					if (curOcc % 2 == 0)
					{
						ret.Add(curBuf);
						curBuf = "";
						continue;
					}
				}
				curBuf += cc;
			}

			if (!string.IsNullOrWhiteSpace(curBuf)) ret.Add(curBuf);

			return ret.ToArray();
		}

		public static string MinSec(this double d)
		{
			var m = d / 60;
			var tr = Math.Truncate(m);
			return tr.ToString() + ":" + ((m - tr) * 60).ToString("00");
		}
	}
}
