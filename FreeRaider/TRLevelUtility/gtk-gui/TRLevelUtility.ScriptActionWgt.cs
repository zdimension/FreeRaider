
// This file has been generated by the GUI designer. Do not modify.
namespace TRLevelUtility
{
	public partial class ScriptActionWgt
	{
		private global::Gtk.HBox hbox1;

		private global::Gtk.ComboBox cbxInstr;

		private global::Gtk.SpinButton sbVal;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget TRLevelUtility.ScriptActionWgt
			global::Stetic.BinContainer.Attach(this);
			this.Name = "TRLevelUtility.ScriptActionWgt";
			// Container child TRLevelUtility.ScriptActionWgt.Gtk.Container+ContainerChild
			this.hbox1 = new global::Gtk.HBox();
			this.hbox1.Name = "hbox1";
			this.hbox1.Homogeneous = true;
			this.hbox1.Spacing = 6;
			// Container child hbox1.Gtk.Box+BoxChild
			this.cbxInstr = global::Gtk.ComboBox.NewText();
			this.cbxInstr.AppendText(global::Mono.Unix.Catalog.GetString("Level"));
			this.cbxInstr.AppendText(global::Mono.Unix.Catalog.GetString("Demo (random)"));
			this.cbxInstr.AppendText(global::Mono.Unix.Catalog.GetString("Exit to title"));
			this.cbxInstr.AppendText(global::Mono.Unix.Catalog.GetString("Exit game"));
			this.cbxInstr.AppendText(global::Mono.Unix.Catalog.GetString("None"));
			this.cbxInstr.Name = "cbxInstr";
			this.hbox1.Add(this.cbxInstr);
			global::Gtk.Box.BoxChild w1 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.cbxInstr]));
			w1.Position = 0;
			// Container child hbox1.Gtk.Box+BoxChild
			this.sbVal = new global::Gtk.SpinButton(0D, 255D, 1D);
			this.sbVal.CanFocus = true;
			this.sbVal.Name = "sbVal";
			this.sbVal.Adjustment.PageIncrement = 10D;
			this.sbVal.ClimbRate = 1D;
			this.sbVal.Numeric = true;
			this.hbox1.Add(this.sbVal);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.sbVal]));
			w2.Position = 1;
			this.Add(this.hbox1);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
			this.cbxInstr.Changed += new global::System.EventHandler(this.OnCbxInstrChanged);
			this.sbVal.Output += new global::Gtk.OutputHandler(this.OnSbValOutput);
		}
	}
}
