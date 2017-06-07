using System;
using Gtk;
using System.Collections.Generic;
using System.Linq;
using FreeRaider.Loader;
using System.Globalization;

namespace TRLevelUtility
{
    [System.ComponentModel.ToolboxItem(true)]
    public partial class PgTR4Script : Gtk.Bin, ITRLUPage
    {
        public PgTR4Script()
        {
            this.Build();
        }

        public string FileFilter => "TR4-5 script file (SCRIPT.DAT, SCRIPT.txt)|*.DAT;*.TXT";

        public Window ParentWnd
        {
            get; set;
        }

        public event SSCHdlr SaveStateChanged = (can) => { };

        public void CreateNew()
        {
            throw new NotImplementedException();
        }

        public void Open(string filename, params dynamic[] args)
        {
            throw new NotImplementedException();
        }

        public void Save()
        {
            throw new NotImplementedException();
        }

        public void SaveAs()
        {
            throw new NotImplementedException();
        }

        bool tr4FlagsSetting = false;

        private void tr4SetFlags(int val)
        {
            tr4FlagsSetting = true;
            tbeTR4Flags.Value = val;
            foreach (CheckButton cbx in hboxTR4Flags.Children)
            {
                cbx.Active = (val & (1 << (int.Parse(cbx.Name.Substring(10)) - 1))) != 0;
            }
            tr4FlagsSetting = false;
        }

        protected void tr4FlagToggle(object sender, EventArgs e)
        {
            if (tr4FlagsSetting) return;
            var actives = hboxTR4Flags.Children.Where(x => ((CheckButton)x).Active).Select(x => 1 << (int.Parse(x.Name.Substring(10)) - 1));
            var val = 0;
            if (actives.Any())
                val = actives.Aggregate((x, y) => x | y);
            tr4SetFlags(val);
        }

        protected void OnTbeTR4FlagsChanged(object sender, EventArgs e)
        {
            if (tr4FlagsSetting) return;
            tr4SetFlags(tbeTR4Flags.Value);
        }



        private bool isTR5 = false;
        private string tr4Filename = "";
        private string tr4LngFilename;

        private void Init()
        {
            SaveStateChanged(true);

            larTR4Levels.InitStore();
            larTR4Filenames.InitStore();
        }

        protected void tpcFlagToggle(object sender, EventArgs e)
        {
        }
    }
}
