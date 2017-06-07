﻿using System;
using System.Runtime.InteropServices;
using Gtk;
using System.Reflection;

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


    }
}
