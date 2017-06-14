using System;
using System.IO;
using Gtk;
using System.Drawing.Imaging;
using System.Drawing;
using FreeRaider.Loader;
using Gdk;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Collections.Generic;

namespace TRLevelUtility
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PgRAW : Gtk.Bin, ITRLUPage
	{
		private static readonly List<PixelFormat> pformats_ok =
			Enum.GetValues(typeof(PixelFormat)).Cast<PixelFormat>().Where(x => 
		{
			try
			{
				return System.Drawing.Image.GetPixelFormatSize(x) != 0;
			}
			catch
			{
				return false;
			}
		}).OrderBy(x => System.Drawing.Image.GetPixelFormatSize(x))
		           .ThenBy(x => x.ToString()).ToList();

		public PgRAW()
		{
			this.Build();

			foreach (var g in pformats_ok)
			{
				cbxPixelFormat.AppendText(g.ToString().Substring(6));
			}
			cbxPixelFormat.Active = (int)PixelFormat.Format24bppRgb;
			imgRAW.Events |= EventMask.ScrollMask;
			imgRAW.AddEvents((int)EventMask.ScrollMask);
		}

		public string FileFilter => "RAW files|*.raw|PAK (compressed) files|*.pak";

		public Gtk.Window ParentWnd
		{
			get; set;
		}

		public event SSCHdlr SaveStateChanged = (can) => { };

		public void CreateNew()
		{
			throw new NotImplementedException();
		}

		public void Open(string filename, params object[] args)
		{
			hexviewwgt1.Data = args[0].Equals(1) ? PAKFile.Read(filename) : File.ReadAllBytes(filename);
			setPF(PixelFormat.Format24bppRgb);
			guessImage();
		}

		private void setPF(PixelFormat pf)
		{
			cbxPixelFormat.Active = pformats_ok.IndexOf(pf);
		}

		private void guessImage()
		{
			Console.WriteLine("guessing image for data");
			Console.WriteLine("len = " + hexviewwgt1.Data.Length);
			var len = hexviewwgt1.Data.Length;

			var database = new Dictionary<int, object[]>()
			{
				{393216, new object[]{ "512", "256", PixelFormat.Format24bppRgb, true}},
				{7296, new object[]{"128", "57", PixelFormat.Format8bppIndexed, false}},
				{270336, new object[]{"512", "256", PixelFormat.Format16bppRgb555, false}}
			};

			txtIMGOffset.Text = "0";

			if (database.ContainsKey(len))
			{
				var i = database[len];
				txtIMGWidth.Text = (string)i[0];
				txtIMGHeight.Text = (string)i[1];
				setPF((PixelFormat)i[2]);
				cbxBGR.Active = (bool)i[3];
				OnBtnLoadImgClicked(this, null);
			}
		}

		public void Save()
		{
			throw new NotImplementedException();
		}

		public void SaveAs()
		{
			throw new NotImplementedException();
		}

		private System.Drawing.Image curImg;

		private double zoomFactor = 1.0d;

		[HandleProcessCorruptedStateExceptions]
		protected unsafe void OnBtnLoadImgClicked(object sender, EventArgs e)
		{
			zoomFactor = 1.0d;
			btnSaveIMG.Sensitive = false;
			if (curImg != null)
			{
				curImg.Dispose();
				curImg = null;
			}
			int width, height, offset;
			if (!int.TryParse(txtIMGOffset.Text, out offset) || offset < 0 || offset > hexviewwgt1.Data.Length)
			{
				Helper.MsgBox("Invalid offset: " + txtIMGOffset.Text, mt: MessageType.Error, parent: ParentWnd);
				return;
			}
			if (!int.TryParse(txtIMGWidth.Text, out width) || width <= 0 || width > 2048)
			{
				Helper.MsgBox("Invalid width: " + txtIMGWidth.Text, mt: MessageType.Error, parent: ParentWnd);
				return;
			}
			if (!int.TryParse(txtIMGHeight.Text, out height) || height <= 0 || height > 2048)
			{
				Helper.MsgBox("Invalid height: " + txtIMGHeight.Text, mt: MessageType.Error, parent: ParentWnd);
				return;
			}
			if (cbxPixelFormat.Active < 0)
			{
				Helper.MsgBox("Invalid pixel format.", mt: MessageType.Error, parent: ParentWnd);
				return;
			}
			try
			{
				var fdata = hexviewwgt1.Data.ToArray();
				System.GC.KeepAlive(fdata);
				var pf = (PixelFormat)Enum.Parse(typeof(PixelFormat), "Format" + cbxPixelFormat.ActiveText);
				var stride = width * System.Drawing.Image.GetPixelFormatSize(pf) / 8;
				if (stride * height + offset > fdata.Length)
				{
					throw new ArgumentException("Not enough data for the specified image size and pixel format. Loading would cause a buffer overflow.");
				}
				fixed (byte* bs = fdata)
				{
					var ptr = bs + offset;
					var bmp_32bpp = Helper.ConvertPFto32argb(width, height, ptr, pf);
					var g = bmp_32bpp.GetPixel(0, 0);
					Console.WriteLine(g.ToString());
					if (cbxBGR.Active)
					{
						var data = bmp_32bpp.LockBits(new System.Drawing.Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, bmp_32bpp.PixelFormat);
						var scan0 = (byte*)data.Scan0.ToPointer();
						long length = Math.Abs(data.Stride) * bmp_32bpp.Height;
						// Invert R & B
						for (long i = 0; i < length; i += 4)
						{
							var dummy = scan0[i];
							scan0[i] = scan0[i + 2];
							scan0[i + 2] = dummy;
						}
						bmp_32bpp.UnlockBits(data);
					}
					setImg(bmp_32bpp);
					curImg = bmp_32bpp;
					btnSaveIMG.Sensitive = true;
				}
				fdata = null;
			}
			catch (Exception ex)
			{
				Helper.Die(ex, "An error occured while creating the image.", ParentWnd);
			}
		}

		private void setImg(Bitmap bmp_N)
		{
			using (var ms = new MemoryStream())
			{
				bmp_N.Save(ms, ImageFormat.Png);
				ms.Position = 0;
				imgRAW.Pixbuf = new Gdk.Pixbuf(ms);
			}
		}

		protected void OnImgRAWScrollEvent(object o, ScrollEventArgs args)
		{
			if (curImg == null) return;
			var nzf = zoomFactor + (args.Event.Direction == ScrollDirection.Up ? 0.5d : -0.5d);
			var scW = (int)(curImg.Width * nzf);
			var scH = (int)(curImg.Height * nzf);
			if (scW * scH == 0) return;
			zoomFactor = nzf;
			var bmp = new Bitmap(scW, scH, curImg.PixelFormat);
			using (var gr = Graphics.FromImage(bmp))
			{
				gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
				gr.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.None;
				gr.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
				gr.DrawImage(curImg, 0, 0, scW, scH);
			}
			setImg(bmp);
		}

		[HandleProcessCorruptedStateExceptions]
		protected void OnBtnSaveIMGClicked(object sender, EventArgs e)
		{
			var formats = new Dictionary<ImageFormat, string>()
			{
				{ImageFormat.Bmp, "Bitmap file (*.bmp)|*.bmp"},
				{ImageFormat.Gif, "GIF file (*.gif)|*.gif"},
				{ImageFormat.Icon, "Windows icon (*.ico)|*.ico"},
				{ImageFormat.Jpeg, "JPEG file (*.jpeg)|*.jpeg"},
				{ImageFormat.Png, "PNG file (*.png)|*.png"},
				{ImageFormat.Tiff, "Tagged image (*.tiff)|*.tiff"},
			};
			if (!MainClass.IsRunningOnMono())
			{
				formats.Add(ImageFormat.Emf, "Enhanced metafile (*.emf)|*.emf");
				formats.Add(ImageFormat.Exif, "Exchangeable Image file (*.exif)|*.exif");
				formats.Add(ImageFormat.Wmf, "Windows metafile (*.wmf)|*.wmf");
			}

			var fn = Helper.getFile2(ParentWnd, "Save to file", true, string.Join("|", formats.Values));
			if (fn.Item1 == null) return;
			try
			{
				curImg.Save(fn.Item1, formats.Keys.ToArray()[fn.Item2]);
			}
			catch (Exception ex)
			{
				Helper.Die(ex, "An error occured while saving the image.", ParentWnd);
			}
		}
	}
}
