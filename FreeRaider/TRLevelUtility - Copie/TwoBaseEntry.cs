using System;
namespace TRLevelUtility
{
    [System.ComponentModel.ToolboxItem(true)]
    public partial class TwoBaseEntry : Gtk.Bin
    {
        public event EventHandler Changed = delegate { };

        public TwoBaseEntry()
        {
            TheBase = 16;
            this.Build();
            tbeSb2.Numeric = false;
        }

        public int TheBase
        {
            get; set;
        }

        public int Min { get { double a, b; tbeSb1.GetRange(out a, out b); return (int)a; } set { tbeSb1.SetRange(value, Max); tbeSb2.SetRange(value, Max); } }

        public int Max { get { double a, b; tbeSb1.GetRange(out a, out b); return (int)b; } set { tbeSb1.SetRange(Min, value); tbeSb2.SetRange(Min, value); } }

        public int Value
        {
            get { return tbeSb1.ValueAsInt; }
            set
            {
                tbeSb1.Value = value;
            }
        }

        bool setting = false;

        protected void OntbeSb1Changed(object sender, EventArgs e)
        {
            if (setting) return;
            setting = true;
            tbeSb2.Value = tbeSb1.Value;
            setting = false;
            Changed(sender, e);
        }

        protected void OntbeSb2Changed(object sender, EventArgs e)
        {
            if (setting) return;
            setting = true;
            tbeSb1.Value = tbeSb2.Value;
            setting = false;
            Changed(sender, e);
        }

        protected void OntbeSb2Input(object o, Gtk.InputArgs args)
        {
            var tmp = tbeSb2.ValueAsInt;
            try
            {
                tmp = Convert.ToInt32(tbeSb2.Text, TheBase);
            }
            catch
            {
            }
            args.NewValue = tmp;
            args.RetVal = 1;
        }

        protected void OntbeSb2Output(object o, Gtk.OutputArgs args)
        {
            if (TheBase != 0)
                tbeSb2.Text = Convert.ToString(tbeSb2.ValueAsInt, TheBase).ToUpper();
            args.RetVal = 1;
        }
    }
}
