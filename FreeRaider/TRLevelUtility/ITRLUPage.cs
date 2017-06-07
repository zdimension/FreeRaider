using System;
namespace TRLevelUtility
{
    public delegate void SSCHdlr(bool can);

    public interface ITRLUPage
    {
        void CreateNew();

        void Open(string filename, params dynamic[] args);

        event SSCHdlr SaveStateChanged;

        void Save();

        void SaveAs();

        Gtk.Window ParentWnd { get; set; }

        string FileFilter { get; }
    }
}
