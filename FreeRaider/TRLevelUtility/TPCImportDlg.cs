using System;
namespace TRLevelUtility
{
    public partial class TPCImportDlg : Gtk.Dialog
    {
        public TPCImportDlg()
        {
            this.Build();
        }

        public int Game
        {
            get
            {
                if (rbTR2b.Active) return 2;
                if (rbTR3.Active) return 3;
                return 1;
            }
            set
            {
                rbTR2.Active = rbTR2b.Active = rbTR3.Active = false;
                if (value == 2) rbTR2b.Active = true;
                if (value == 3) rbTR3.Active = true;
                else rbTR2.Active = true;
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
                if (value == 1)
                    rbPSX.Active = !(rbPC.Active = false);
                else
                    rbPC.Active = !(rbPSX.Active = false); // here be dragons
            }
        }

        protected void OnRbTR2bToggled(object sender, EventArgs e)
        {
            rbPC.Sensitive = !rbTR2b.Active;
            if (rbTR2b.Active) rbPSX.Active = true;
        }

        protected void OnRbPCToggled(object sender, EventArgs e)
        {
            rbTR2b.Sensitive = !rbPC.Active;
            if (rbPC.Active && rbTR2b.Active) rbTR2.Active = true;
        }
    }
}
