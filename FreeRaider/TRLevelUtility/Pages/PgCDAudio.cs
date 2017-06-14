using System;
using System.IO;
using System.Media;
using System.Runtime.ExceptionServices;
using System.Threading;
using FreeRaider.Loader;
using Gtk;

namespace TRLevelUtility
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PgCDAudio : Gtk.Bin
	{
		public PgCDAudio()
		{
			this.Build();
			player = new SoundPlayer();
			larMain.SelectionChanged += LarMain_SelectionChanged;
		}

		private SoundPlayer player;

		public string FileFilter => "CDAUDIO.WAD|*.WAD";

		public Gtk.Window ParentWnd
		{
			get; set;
		}

		public event SSCHdlr SaveStateChanged = (can) => { };

		public void CreateNew()
		{
			throw new NotImplementedException();
		}

		private CDAUDIO curFile = null;

		private Timer tmr = null;

		public void Open(string filename, params dynamic[] args)
		{
			larMain.AddColumns("Name", "Offset (absolute)", "Length (bytes)", "Length (seconds)", "Length");
			foreach (var c in larMain.TreeView.Columns)
				(c.CellRenderers[0] as CellRendererText).Editable = false;
			larMain.InitStore(true);
			try
			{
				var file = CDAUDIO.Read(filename);

				curFile = file;

				var tally = CDAUDIO.DATA_START;
				foreach (var ent in curFile.Entries)
				{
					var len = Helper.GetWavLength(ent.Item2);
					larMain.AddRow(ent.Item1,
								   tally.ToString(),
								   ent.Item2.Length.ToString(),
								   len.ToString("F2"),
								   len.MinSec());
					tally += ent.Item2.Length;
				}

				larMain.SelectedRow = 0;
			}
			catch (Exception e)
			{
				Helper.Die(e, "An error occured while loading the file.", ParentWnd);
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

		void LarMain_SelectionChanged()
		{
			OnBtnStopClicked(this, null);
			btnPlay.Sensitive = btnSaveSel.Sensitive = larMain.SelectedRow != -1 && curFile.Entries[larMain.SelectedRow].Item2.Length >= 32;
		}

		public const int INTERVAL = 125;
		public const double INTERVAL_D = INTERVAL / 1000.0d;

		protected void OnBtnPlayClicked(object sender, EventArgs e)
		{
			var ms = new MemoryStream(curFile.Entries[larMain.SelectedRow].Item2, false);
			player.Stream = ms;

			btnStop.Sensitive = true;
			btnPlay.Sensitive = false;
			curLen = double.Parse(larMain[larMain.SelectedRow, 3]);
			scMedia.SetRange(0, curLen);
			lblDur.Text = curLen.MinSec();
			curPos = 0;
			player.Play();
			tmr = new Timer(HandleTimerCallback, null, TimeSpan.FromMilliseconds(INTERVAL), TimeSpan.FromMilliseconds(INTERVAL));
		}

		private double curLen = 0;
		private double curPos = 0;

		void HandleTimerCallback(object state)
		{
			curPos += INTERVAL_D;
			Gtk.Application.Invoke(delegate
			{
				if (curPos >= curLen)
				{
					OnBtnStopClicked(this, null);
				}
				scMedia.Value = curPos;
				lblCur.Text = curPos.MinSec();
			});
		}

		protected void OnBtnSaveSelClicked(object sender, EventArgs e)
		{
		}

		protected void OnBtnStopClicked(object sender, EventArgs e)
		{
			if (tmr != null)
			{
				tmr.Dispose();
				tmr = null;
			}
			lblCur.Text = lblDur.Text = "0:00";
			player.Stop();
			scMedia.Value = 0;
			btnStop.Sensitive = false;
			btnPlay.Sensitive = true;
			if (player.Stream != null)
			{
				player.Stream.Dispose();
				player.Stream = null;
			}
		}


	}
}
