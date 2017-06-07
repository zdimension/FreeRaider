using System;
namespace TRLevelUtility
{
    public partial class TR4ImportDlg : Gtk.Dialog
    {
        public TR4ImportDlg()
        {
            this.Build();
        }

        public int Game
        {
            get
            {
                return rbTR5.Active ? 5 : 4;
            }
            set
            {
                rbTR4.Active = value == 4;
                rbTR5.Active = value == 5;
            }
        }

    }
}
