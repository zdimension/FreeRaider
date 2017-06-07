using System;
using Gtk;

namespace TRLevelUtility
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            Application.Init();
            Gtk.Settings.Default.SetLongProperty("gtk-button-images", 1, "");
            MainWindow win = new MainWindow();
            win.Show();
            Application.Run();
        }


    }
}
