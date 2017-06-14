using System;
using System.IO;
using Gtk;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Reflection;

namespace TRLevelUtility
{
    class MainClass
    {
		public static bool IsWindows
		{
			get
			{
				return (int)Environment.OSVersion.Platform <= 3 && Environment.OSVersion.Platform >= 0;
			}
		}

		public static void Main(string[] args)
		{
			GLib.ExceptionManager.UnhandledException += arg => 
			{
				arg.ExitApplication = Helper.Die(null, "An unhandled exception has been caught.\n" +
				                                 "Do you want to exit the application?\n" +
				                                 "Everything not saved will be lost™.", bt: ButtonsType.YesNo)
					== ResponseType.Yes;
			};
			if (IsWindows)
				CheckWindowsGtk();
			Application.Init();
			Gtk.Settings.Default.SetLongProperty("gtk-button-images", 1, "");
			if (IsWindows) // If Linux/macOS, keep user theme, otherwise use MurrinaCandido
				using (var s = typeof(MainClass).Assembly.GetManifestResourceStream("TRLevelUtility.gtkrc")) // Default MS-Windows theme is ugly
				using (var tr = new StreamReader(s))
					Gtk.Rc.ParseString(tr.ReadToEnd());
			MainWindow win = new MainWindow();
			win.Show();
			Application.Run();
		}

		// https://forums.xamarin.com/discussion/15568/unable-to-load-dll-libgtk-win32-2-0-0-dll#Comment_50617
		static bool CheckWindowsGtk()
		{
			string location = null;
			Version version = null;
			Version minVersion = new Version(2, 12, 22);

			using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Xamarin\GtkSharp\InstallFolder"))
			{
				if (key != null)
					location = key.GetValue(null) as string;
			}
			using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Xamarin\GtkSharp\Version"))
			{
				if (key != null)
					Version.TryParse(key.GetValue(null) as string, out version);
			}

			//TODO: check build version of GTK# dlls in GAC
			if (version == null || version < minVersion || location == null || !File.Exists(Path.Combine(location, "bin", "libgtk-win32-2.0-0.dll")))
			{
				Console.WriteLine("Did not find required GTK# installation");
				/*string url = "http://monodevelop.com/Download";
				string caption = "Fatal Error";
				string message =
					"{0} did not find the required version of GTK#. Please click OK to open the download page, where " +
					"you can download and install the latest version.";
				if (DisplayWindowsOkCancelMessage(
					string.Format(message, BrandingService.ApplicationName, url), caption)
				)
				{
					Process.Start(url);
				}*/
				return false;
			}

			Console.WriteLine("Found GTK# version " + version);

			var path = Path.Combine(location, @"bin");
			try
			{
				if (SetDllDirectory(path))
				{
					return true;
				}
			}
			catch (EntryPointNotFoundException)
			{
			}
			// this shouldn't happen unless something is weird in Windows
			Console.WriteLine("Unable to set GTK+ dll directory");
			return true;
		}

		[System.Runtime.InteropServices.DllImport("kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode, SetLastError = true)]
		[return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
		static extern bool SetDllDirectory(string lpPathName);

		public static bool IsRunningOnMono()
		{
			return Type.GetType("Mono.Runtime") != null;
		}
    }
}
