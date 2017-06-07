using System;
using System.Runtime.InteropServices;
using Gtk;

namespace TRLevelUtility
{
    public static class Extensions
    {
        public static T GetField<T>(this object a, string name)
            where T : class
        {
            return a.GetType().GetField(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(a) as T;
        }

        [DllImport("libgtk-win32-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr gtk_text_iter_copy(IntPtr raw);

        public static TextIter CopyEx(this TextIter a)
        {
            IntPtr num = Marshal.AllocHGlobal(Marshal.SizeOf((object)a));
            Marshal.StructureToPtr((object)a, num, false);
            var textIter = TextIter.New(gtk_text_iter_copy(num));
            Marshal.FreeHGlobal(num);
            return textIter;
        }
    }
}
