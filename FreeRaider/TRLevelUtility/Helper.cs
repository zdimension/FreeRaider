using System;
using Gtk;
using System.Linq;
using System.Runtime.InteropServices;


namespace TRLevelUtility
{
    public static class Helper
    {
        public static string getFile(Window parent, string title, bool save, params string[] filters)
        {
            return getFile2(parent, title, save, filters).Item1;
        }

        public static Tuple<string, int> getFile2(Window parent, string title, bool save, params string[] filters)
        {
            string ret = null;
            var dlg = new FileChooserDialog(title, parent, save ? FileChooserAction.Save : FileChooserAction.Open);
            dlg.AddButton(Stock.Cancel, ResponseType.Cancel);
            dlg.AddButton(save ? Stock.Save : Stock.Open, ResponseType.Ok);
            filters.All(x =>
            {
                var fil = new FileFilter();
                var sp = x.Split('|');
                fil.Name = sp[0];
                foreach (var p in sp[1].Split(';'))
                {
                    fil.AddPattern(p.ToUpper());
                    fil.AddPattern(p.ToLower());
                }
                dlg.AddFilter(fil);
                return true;
            });
            if (dlg.Run() == (int)ResponseType.Ok)
            {
                ret = dlg.Filename;
            }
            var rett = new Tuple<string, int>(ret, Array.IndexOf(dlg.Filters, dlg.Filter));
            dlg.Destroy();
            return rett;
        }

		[DllImport("kernel32.dll")]
		private static extern IntPtr LoadLibrary(string path);

		[DllImport("kernel32.dll")]
		private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

		[DllImport("libdl.so")]
		static extern IntPtr dlopen(string filename, int flags);

		public static T GetFunction<T>(string dllPath, string functionName)
			where T : class
		{
			var hModule = LoadLibrary(dllPath);
			var functionAddress = GetProcAddress(hModule, functionName);
			return Marshal.GetDelegateForFunctionPointer(functionAddress, typeof (T)) as T;
		}
    }
}
