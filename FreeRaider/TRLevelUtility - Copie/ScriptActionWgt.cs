using System;
using System.Linq;
namespace TRLevelUtility
{
    [System.ComponentModel.ToolboxItem(true)]
    public partial class ScriptActionWgt : Gtk.Bin
    {
        public ScriptActionWgt()
        {
            this.Build();
            sbVal.Numeric = false;
        }

        protected void OnCbxInstrChanged(object sender, EventArgs e)
        {
            sbVal.Value = sbVal.Value;
            if (cbxInstr.Active > 4)
            {
                sbVal.Value = 0;
                sbVal.Sensitive = false;
            }
            else
            {
                sbVal.Sensitive = true;
            }
        }

        private uint[] instructions = { 0x00000000, 0x000000FF, 0x00000200, 0x00000300, 0x00000400, 0x00000500, 0x00000700, 0xFFFFFFFF };

        public uint Value
        {
            get
            {
                return instructions[cbxInstr.Active] + (uint)sbVal.Value;
            }
            set
            {
                int id = Array.IndexOf(instructions, value);
                if (id == -1)
                {
                    var tmp = instructions.Last(x => x <= value);
                    id = Array.IndexOf(instructions, tmp);
                    sbVal.Value = Math.Min(255, value - id);
                }
                cbxInstr.Active = id;
            }
        }

        public Func<int, int, string> OnGetLevelName = delegate { return "invalid"; };

        protected void OnSbValOutput(object o, Gtk.OutputArgs args)
        {
            sbVal.Text = sbVal.ValueAsInt.ToString();
            if (new[] { 0, 2, 3 }.Contains(cbxInstr.Active))
            {
                sbVal.Text += " (" + OnGetLevelName(sbVal.ValueAsInt, cbxInstr.Active) + ")";
            }
            args.RetVal = 1;
        }
    }
}
