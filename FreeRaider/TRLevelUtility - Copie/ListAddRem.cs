using System;
using System.Linq;
using Gtk;

namespace TRLevelUtility
{
    [System.ComponentModel.ToolboxItem(true)]
    public partial class ListAddRem : Gtk.Bin
    {
        public delegate void RowAddedHdlr(int newID);
        public delegate void RowRemovedHdlr(int id);
        public delegate void RowEditedHdlr(int row, int column);
        public delegate void EmptyHdlr();
        public delegate void RowMovedHdlr(int old, bool down);
        public event RowAddedHdlr RowAdded = delegate { };
        public event RowRemovedHdlr RowRemoved = delegate { };
        public event RowEditedHdlr RowEdited = delegate { };
        public event EmptyHdlr SelectionChanged = delegate { };
        public event RowMovedHdlr RowMoved = delegate { };


        public ListAddRem()
        {
            this.Build();
            tvMain.Selection.Mode = SelectionMode.Single;
            tvMain.Selection.Changed += Selection_Changed;
            tvMain.EnableGridLines = TreeViewGridLines.Both;
            tvMain.ShowAll();
        }

        public TreeView TreeView
        {
            get { return tvMain; }
            set { tvMain = value; }
        }

        public ListStore Store
        {
            get;
            set;
        }

        public bool HideAddRem
        {
            get { return !hbox2.Visible; }
            set
            {
                if (value) hbox2.HideAll();
                else hbox2.ShowAll();
            }
        }

        private int currentColumn = 0;

        public void AddColumn(string name)
        {
            var rd = new CellRendererText();
            var id = currentColumn++;
            rd.Edited += (o, args) => CellEdited(id, o, args);
            rd.Editable = true;
            var clmn = new TreeViewColumn("\n" + name + "2\n", rd, "text", id);
            clmn.Expand = true;
            var g = new Label(name);
            g.SetPadding(0, 3);
            g.Show();
            clmn.Widget = g;
            clmn.Alignment = 0.5f;
            tvMain.AppendColumn(clmn);
        }

        public void AddColumns(params string[] names)
        {
            foreach (var name in names) AddColumn(name);
        }

        public string[] GetColumn(int id)
        {
            return Enumerable.Range(0, Store.IterNChildren()).Select(x => this[x, id]).ToArray();
        }

        public void InitStore(bool hide = false)
        {
            if (currentColumn == 0)
            {
                AddColumn("<empty>");
                tvMain.HeadersVisible = false;
            }
            Store = new ListStore(Enumerable.Range(0, currentColumn).Select(x => typeof(string)).ToArray());
            tvMain.Model = Store;
            tvMain.ShowAll();
            HideAddRem = hide;
        }

        public string this[int row, int col]
        {
            get
            {
                TreeIter iter;
                Store.GetIter(out iter, new TreePath(new[] { row }));
                return this[iter, col];
            }
            set
            {
                TreeIter iter;
                Store.GetIter(out iter, new TreePath(new[] { row }));
                this[iter, col] = value;
            }
        }

        public string[] this[int row]
        {
            get
            {
                TreeIter iter;
                Store.GetIter(out iter, new TreePath(new[] { row }));
                return Enumerable.Range(0, currentColumn).Select(x => Store.GetValue(iter, x) as string).ToArray();
            }
            set
            {
                TreeIter iter;
                Store.GetIter(out iter, new TreePath(new[] { row }));
                Store.SetValues(iter, value.Cast<object>().ToArray());
            }
        }

        public string this[TreeIter row, int col]
        {
            get
            {
                return Store.GetValue(row, col) as string;
            }
            set
            {
                Store.SetValue(row, col, value);
            }
        }

        public int SelectedRow
        {
            get
            {
                if (tvMain.Selection.CountSelectedRows() == 0) return -1;
                TreeModel model;
                return tvMain.Selection.GetSelectedRows(out model)[0].Indices[0];
            }
        }

        public TreeIter SelectedIter
        {
            get
            {
                TreeModel model;
                TreeIter iter;
                var path = tvMain.Selection.GetSelectedRows(out model)[0];
                Store.GetIter(out iter, path);
                return iter;
            }
            set
            {
                tvMain.Selection.SelectIter(value);
            }
        }

        public void AddRow(params string[] fields)
        {
            TreeIter it;
            if (SelectedRow != -1)
                it = Store.InsertWithValues(SelectedRow + 1, fields);
            else
                it = Store.AppendValues(fields);
            var newID = Store.GetPath(it).Indices[0];
            RowAdded(newID);
            tvMain.Selection.SelectIter(it);
        }

        public void AddRow(int id, params string[] fields)
        {
            Store.InsertWithValues(id, fields);
            RowAdded(id);
        }

        public void RemoveRow(int id)
        {
            TreeIter iter;
            Store.GetIter(out iter, new TreePath(new[] { id }));
            Store.Remove(ref iter);
            RowRemoved(id);
        }

        private void CellEdited(int clmn, object sender, EditedArgs args)
        {
            var path = new TreePath(args.Path);
            TreeIter iter;
            Store.GetIter(out iter, path);
            Store.SetValue(iter, clmn, args.NewText);
            RowEdited(path.Indices[0], clmn);
        }

        void Selection_Changed(object sender, EventArgs e)
        {
            btnRemove.Sensitive = btnDuplicate.Sensitive = btnMoveUp.Sensitive = btnMoveDown.Sensitive = tvMain.Selection.CountSelectedRows() != 0;
            if (SelectedRow == Store.IterNChildren() - 1) btnMoveDown.Sensitive = false;
            if (SelectedRow == 0) btnMoveUp.Sensitive = false;
            SelectionChanged();
        }

        protected void OnBtnAddClicked(object sender, EventArgs e)
        {
            AddRow();
            //RowAdded();
        }

        protected void OnBtnRemoveClicked(object sender, EventArgs e)
        {
            var iter = SelectedIter;
            var id = SelectedRow;
            Store.Remove(ref iter);
            RowRemoved(SelectedRow);
        }

        protected void selection_changed(object sender, EventArgs e)
        {
        }

        public void ResizeRows(int count, string filler = "")
        {
            var current = Store.IterNChildren();
            if (count == current) return;
            if (count > current)
            {
                var rem = count - current;
                for (var i = 0; i < rem; i++)
                    AddRow(filler);
            }
            else
            {
                var add = current - count;
                for (var i = 0; i < add; i++)
                    RemoveRow(current - 1 - i);
            }
        }

        public void MoveRow(int id, bool down)
        {
            if (down) MoveDown(id);
            else MoveUp(id);
        }

        public void MoveDown(int id)
        {
            TreeIter it1;
            Store.GetIter(out it1, new TreePath(new[] { id }));
            TreeIter it2;
            Store.GetIter(out it2, new TreePath(new[] { id + 1 }));
            Store.MoveAfter(it1, it2);
            RowMoved(id, true);
        }

        public void MoveUp(int id)
        {
            TreeIter it1;
            Store.GetIter(out it1, new TreePath(new[] { id }));
            TreeIter it2;
            Store.GetIter(out it2, new TreePath(new[] { id - 1 }));
            Store.MoveBefore(it1, it2);
            RowMoved(id, false);
        }

        public void DuplicateRow(int id)
        {
            var newID = id + 1;
            AddRow(newID, this[id]);
            RowAdded(newID);
        }

        protected void OnBtnDuplicateClicked(object sender, EventArgs e)
        {
            DuplicateRow(SelectedRow);
        }

        protected void OnBtnMoveUpClicked(object sender, EventArgs e)
        {
            MoveUp(SelectedRow);
        }

        protected void OnBtnMoveDownClicked(object sender, EventArgs e)
        {
            MoveDown(SelectedRow);
        }
    }
}
