using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gtk;
using Pango;

namespace TRLevelUtility
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class HexViewWgt : Gtk.Bin
	{
		private List<EncodingInfo> encs = new List<EncodingInfo>();

		public HexViewWgt()
		{
			this.Build();
			_data = new byte[0];
			textview1.Buffer.TagTable.Add(new TextTag("monospace") { Family = "monospace" });
			foreach (var enc in Encoding.GetEncodings())
			{
				try
				{
					var g = enc.GetEncoding(); // check if encoding works with mono
					encs.Add(enc);
					cbxEncoding.AppendText(enc.Name + " - " + enc.DisplayName);
				}
				catch
				{
					// mono hack
				}
			}
			cbxEncoding.Active = encs
				.IndexOf(x => x.GetEncoding().HeaderName == Encoding.Default.HeaderName);
			textview1.LeftMargin = 10;
			textview1.ModifyFont(FontDescription.FromString("monospace"));
		}

		private byte[] _data;

		public byte[] Data
		{
			get { return _data; }
			set { _data = value; refreshScroll(); refreshView(); }
		}

		private void refreshScroll()
		{
			vscrollbar1.SetRange(0, (int)Math.Ceiling(_data.Length / double.Parse(cbxWidth.ActiveText)));
		}

		public uint CurrentOffset
		{
			get;
			private set;
		} = 0;

		private readonly byte[] bs = Enumerable.Range(0, 256).Select(x => (byte)x).ToArray();

		private void refreshView()
		{
			try
			{
				var width = int.Parse(cbxWidth.ActiveText);
				var enc = encs[cbxEncoding.Active].GetEncoding();
				var sb = new StringBuilder();

				sb.AppendLine();
				sb.Append(" Offset   ");
				sb.Append(string.Join(" ", Enumerable.Range(0, width).Select(x => x.ToString("X2"))));
				sb.AppendLine(new string(' ', width + 1));
				var curPos = CurrentOffset;

				var t = textview1.CreatePangoLayout(null);
				t.SetMarkup(enc.GetString(bs));
				t.FontDescription = Pango.FontDescription.FromString("monospace");
				int w, h;
				t.GetPixelSize(out w, out h);

				var height = textview1.Allocation.Height / h - 6;

				for (var i = 0; i < height; i++)
				{
					sb.Append(curPos.ToString("X8") + "  ");
					int j;
					for (j = 0; j < width && curPos + j < _data.Length; j++)
					{
						sb.Append(_data[curPos + j].ToString("X2") + " ");
					}
					sb.Append(new string(' ', (width * 3) + 1 - (j * 3)));
					for (var k = 0; k < j; k++)
					{
						var c = _data[curPos + k];
						sb.Append(char.IsControl((char)c) ? '.' : enc.GetChars(new byte[] { c })[0]);
					}
					sb.AppendLine();
					curPos += (uint)width;
					if (curPos >= _data.Length) break;
				}

				/*textview1.Buffer.Clear();
				var iter = textview1.Buffer.StartIter;
				textview1.Buffer.InsertWithTagsByName(ref iter, sb.ToString(), "monospace");*/
				textview1.Buffer.Text = sb.ToString();
			}
			catch (Exception ex)
			{
				Helper.Die(ex, "An error occured while refreshing the view.");
			}
		}

		protected void OnVscrollbar1ValueChanged(object sender, EventArgs e)
		{
			CurrentOffset = (uint)(vscrollbar1.Value) * byte.Parse(cbxWidth.ActiveText);
			refreshView();
		}

		protected void OnCbxWidthChanged(object sender, EventArgs e)
		{
			CurrentOffset = 0;	
			vscrollbar1.Value = 0;
			refreshScroll();
			refreshView();
		}

		protected void OnCbxEncodingChanged(object sender, EventArgs e)
		{
			refreshView();
		}

		protected void OnTextview1ExposeEvent(object o, ExposeEventArgs args)
		{
			refreshView();
		}

		protected void OnTextview1SizeAllocated(object o, SizeAllocatedArgs args)
		{
            refreshView();
		}
	}
}
