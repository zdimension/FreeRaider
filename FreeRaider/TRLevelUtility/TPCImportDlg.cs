using System;
using System.Linq;
using Gtk;

namespace TRLevelUtility
{
    public partial class TPCImportDlg : Gtk.Dialog
    {
        public TPCImportDlg()
        {
            this.Build();
			rbPC.Active = true;
			built = true;
			rbs = new[] { rbTR2, rbTR2b, rbTR3, rbTR4, rbTR5 };
        }

		private bool built = false;

        private bool _save = false;

        public bool Save { get { return _save; }set{Title = global::Mono.Unix.Catalog.GetString(((_save = value) ? "Save" : "Open") + " DAT file"); } }

		private RadioButton[] rbs;

        public int Game
        {
            get
            {
				return rbs.Select(x => x.Active).IndexOf(true) + 1;
            }
            set
            {
				foreach (var r in rbs) r.Active = false;
				rbs[value - 1].Active = true;
            }
        }

        public int Platform
        {
            get
            {
                return rbPSX.Active ? 1 : 0;
            }
            set
            {
				rbPSX.Active = !(rbPC.Active = value == 0); // here be dragons
            }
        }

        protected void OnRbTR2bToggled(object sender, EventArgs e)
        {
            rbPC.Sensitive = !rbTR2b.Active;
            if (rbTR2b.Active) rbPSX.Active = true;
        }

        protected void OnRbPCToggled(object sender, EventArgs e)
        {
			if (!built) return;
            rbTR2b.Sensitive = !rbPC.Active;
            if (rbPC.Active && rbTR2b.Active) rbTR2.Active = true;
        }

		protected void OnRbTR4Toggled(object sender, EventArgs e)
		{
			rbPC.Sensitive = rbPSX.Sensitive = !((Gtk.RadioButton)sender).Active;
			if (((Gtk.RadioButton)sender).Active) radiobutton2.Active = true;
		}

		protected void OnRbTR2Toggled(object sender, EventArgs e)
		{
			if (!rbPC.Active && !rbPSX.Active) rbPC.Active = true;
		}

		public bool ShowTR2
		{
			get { return vbox2.Visible; }
			set { vbox2.Visible = vbox3.Visible = value; }
		}

		public bool ShowTR4
		{
			get { return vbox4.Visible; }
			set { vbox4.Visible = value; }
		}
	}
}
