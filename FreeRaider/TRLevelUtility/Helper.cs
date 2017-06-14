using System;
using Gtk;
using System.Linq;
using System.Runtime.InteropServices;
using System.IO;
using System.Reflection;
using System.Drawing.Imaging;
using System.Drawing;

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
			filters = string.Join("|", filters).SplitEveryNocc('|');
			string ret = null;
			var dlg = new FileChooserDialog(title, parent, save ? FileChooserAction.Save : FileChooserAction.Open);
			dlg.DoOverwriteConfirmation = save;

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
				/*if(save && string.IsNullOrWhiteSpace(Path.GetExtension(ret)))
					ret += "." + dlg.Filter.*/
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
			return Marshal.GetDelegateForFunctionPointer(functionAddress, typeof(T)) as T;
		}

		public static ResponseType MsgBox(string text,
								  DialogFlags df = DialogFlags.Modal,
								  MessageType mt = MessageType.Info,
								 ButtonsType bt = ButtonsType.Ok,
								 Window parent = null)
		{
			var md = new MessageDialog(parent, df, mt, bt, false, text);
			var ret = (ResponseType)md.Run();
			md.Destroy();
			return ret;
		}

		public static ResponseType Die(Exception ex, string msg = "An error occured.", Window par = null, ButtonsType bt = ButtonsType.Ok)
		{
			if (ex is TargetInvocationException)
			{
				msg = "(Target invocation) " + msg;
				if (ex.InnerException != null)
					ex = ex.InnerException;
			}
			if (ex is FileNotFoundException)
			{
				var f = (FileNotFoundException)ex;
				msg += "\nFile name: " + f.FileName;
			}
			var txt = msg;
			if(ex != null) txt += "\n" +
							  ex.GetType().Name + " : " + ex.Message + "\n\n" + ex.StackTrace;
			Console.Error.WriteLine(txt);
			return Helper.MsgBox(txt, mt: MessageType.Error, parent: par, bt: bt);
		}

		public static double GetWavLength(byte[] bs)
		{
			if (bs.Length < 32) return 0;
			//return (bs.Length - 8) * 8.0 / (BitConverter.ToInt32(new[] { bs[28], bs[29], bs[30], bs[31] }, 0) * 8);
			return (bs.Length - 8.0) / BitConverter.ToInt32(new[] { bs[28], bs[29], bs[30], bs[31] }, 0);
		}

		public static readonly PixelFormat[] Libgdiplus_4_2_Supported =
		{
			PixelFormat.Format1bppIndexed,
			PixelFormat.Format4bppIndexed,
			PixelFormat.Format8bppIndexed,
			PixelFormat.Format24bppRgb,
			PixelFormat.Format32bppArgb,
			PixelFormat.Format32bppPArgb,
			PixelFormat.Format32bppRgb
		};

		public const ushort MASK_565_RED = 0xF800;
		public const ushort MASK_565_GREEN = 0x07E0;
		public const ushort MASK_565_BLUE = 0x001F;

		public const ushort MASK_555_RED = 0x7C00;
		public const ushort MASK_555_GREEN = 0x03E0;
		public const ushort MASK_555_BLUE = 0x001F;

		public const ushort MASK_1555_ALPHA = 0x8000;

		public static readonly PixelFormat[] GdiP_not_supported =
		{
			PixelFormat.Format16bppGrayScale
		};

		public static byte Conv16to8(ushort u, bool is15 = false)
		{
			if (is15) u &= 0x7FFF;
			var max = is15 ? 127 : 255;
			return (byte)Math.Min(max, Math.Round(u / (double)max));
		}

		public static ushort Get16(byte HI, byte LO)
		{
			return 0;
		}

		public static unsafe Bitmap ConvertPFto32argb(int width, int height, byte* src, PixelFormat sf)
		{
			var ret = new Bitmap(width, height, PixelFormat.Format32bppArgb);
			var rsize = new Rectangle(0, 0, width, height);
			var s_bits = System.Drawing.Image.GetPixelFormatSize(sf);
			if (
#if FORCE_USE_OUR_CODE
				false &&
#endif
				((MainClass.IsRunningOnMono() && Libgdiplus_4_2_Supported.Contains(sf))
				 ||
				(MainClass.IsWindows && !MainClass.IsRunningOnMono()
				 && !GdiP_not_supported.Contains(sf))))
			{
				// hack the stride so that it's a multiple of 4
				// otherwise GDI+ will shit brix
				var bmp = new Bitmap(width, height, (width * s_bits / 8 + 3) & ~0x03, sf, (IntPtr)src);
				using (var g = Graphics.FromImage(ret))
					g.DrawImage(bmp, rsize);
			}
			else
			{
				Console.WriteLine("Format not supported out of the box, using our code");
				Console.WriteLine("If you use 48bppRgb, beware, here be dragons");
				var length = width * height * s_bits / 8;
				// so, if we're here
				// either we're on Mono/libgdiplus and half of the formats ain't supported
				// either we're on Windows/GDI+ and Microsoft "forgot" to implement
				// the 16bpp grayscale format they created in the '90 hoping that one day
				// it'd be supported. Spoiler: no one ever gave a fuck about it
				var bdata = ret.LockBits(rsize, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
				var bp = (byte*)bdata.Scan0;
				var up = (ushort*)src;
				switch (sf)
				{
					case PixelFormat.Format16bppGrayScale:
						for (var i = 0; i < length / 2; i++)
						{
							var val1 = Math.Round(up[i] / 255.0);
							var val = (byte)(val1 >= 256 ? 255 : val1);
							bp[i * 4 + 0] = val;
							bp[i * 4 + 1] = val;
							bp[i * 4 + 2] = val;
							bp[i * 4 + 3] = 0xFF;
						}
						break;
					case PixelFormat.Format16bppRgb555:
						for (var i = 0; i < length / 2; i++)
						{
							bp[i * 4 + 0] = (byte)((up[i] & MASK_555_BLUE) << 3);
							bp[i * 4 + 1] = (byte)(((up[i] & MASK_555_GREEN) >> 5) << 3);
							bp[i * 4 + 2] = (byte)(((up[i] & MASK_555_RED) >> 10) << 3);
							bp[i * 4 + 3] = 0xFF;
						}
						break;
					case PixelFormat.Format16bppRgb565:
						for (var i = 0; i<length / 2; i++)
						{
							bp[i * 4 + 0] = (byte)((up[i] & MASK_565_BLUE) << 3);
							bp[i * 4 + 1] = (byte)(((up[i] & MASK_565_GREEN) >> 5) << 3);
							bp[i * 4 + 2] = (byte)(((up[i] & MASK_565_RED) >> 11) << 3);
							bp[i * 4 + 3] = 0xFF;
						}
						break;
					case PixelFormat.Format16bppArgb1555:
						for (var i = 0; i<length / 2; i++)
						{
							bp[i * 4 + 0] = (byte)((up[i] & MASK_555_BLUE) << 3);
							bp[i * 4 + 1] = (byte)(((up[i] & MASK_555_GREEN) >> 5) << 3);
							bp[i * 4 + 2] = (byte)(((up[i] & MASK_555_RED) >> 10) << 3);
							bp[i * 4 + 3] = (byte)((up[i] & MASK_1555_ALPHA) != 0 ? 0xFF : 0);
						}
						break;
					case PixelFormat.Format48bppRgb:
						
						for (var i = 0; i < length / 6; i++)
						{
							bp[i * 4 + 0] = (byte)Math.Min(255, 8 * Conv16to8(up[i * 3 + 0])); // TODO:
							bp[i * 4 + 1] = (byte)Math.Min(255, 8 * Conv16to8(up[i * 3 + 1])); // How the fuck did
							bp[i * 4 + 2] = (byte)Math.Min(255, 8 * Conv16to8(up[i * 3 + 2])); // Microsoft impl. this?
							bp[i * 4 + 3] = 0xFF;
						}
						break;
					case PixelFormat.Format64bppArgb:
						for (var i = 0; i < length / 8; i++)
						{
							bp[i * 4 + 0] = Conv16to8(up[i * 4 + 0]);
							bp[i * 4 + 1] = Conv16to8(up[i * 4 + 1]);
							bp[i * 4 + 2] = Conv16to8(up[i * 4 + 2]);
							bp[i * 4 + 3] = Conv16to8(up[i * 4 + 3], true);
						}
						break;
					case PixelFormat.Format64bppPArgb:
						for (var i = 0; i < length / 8; i++)
						{
							var alpha = (up[i * 4 + 3] & 0x7FFF) / 127.0;
							bp[i * 4 + 0] = Conv16to8((ushort)Math.Round(up[i * 4 + 0] / alpha));
							bp[i * 4 + 1] = Conv16to8((ushort)Math.Round(up[i * 4 + 1] / alpha));
							bp[i * 4 + 2] = Conv16to8((ushort)Math.Round(up[i * 4 + 2] / alpha));
							bp[i * 4 + 3] = Conv16to8(up[i * 4 + 3], true);
						}
						break;
				}
				ret.UnlockBits(bdata);
			}
			ret.Save("test.png");
			return ret;
		}
    }
}
