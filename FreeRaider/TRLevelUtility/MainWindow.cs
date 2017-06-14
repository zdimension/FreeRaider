using System;
using System.Collections.Generic;
using Gtk;
using System.Linq;
using System.Reflection;
using TRLevelUtility;
using FreeRaider.Loader;
using System.Globalization;




public partial class MainWindow : Gtk.Window
{
    private ITRLUPage currentPage
    {
        get
        {
            return notebook1.GetNthPage(notebook1.Page) as ITRLUPage;
        }
    }

    public MainWindow() : base(Gtk.WindowType.Toplevel)
    {
        Build();

        this.Resize(1060, 760);

        IconList = new[]
        {
            new Gdk.Pixbuf(null, "TRLevelUtility.TRLU_16.png", 16, 16),
            new Gdk.Pixbuf(null, "TRLevelUtility.TRLU_24.png", 24, 24),
            new Gdk.Pixbuf(null, "TRLevelUtility.TRLU_32.png", 32, 32),
            new Gdk.Pixbuf(null, "TRLevelUtility.TRLU_48.png", 48, 48),
            new Gdk.Pixbuf(null, "TRLevelUtility.TRLU_64.png", 64, 64),
        };

        lblVersion.Text += Assembly.GetExecutingAssembly().GetName().Version.ToString();

        for (var i = 1; i < notebook1.NPages; i++)
        {
            var g = notebook1.GetNthPage(i) as ITRLUPage;
            if (g != null)
                g.ParentWnd = this;
        }

		this.Focus = null;
    }




    protected void OnDeleteEvent(object sender, DeleteEventArgs a)
    {
        Application.Quit();
        a.RetVal = true;
    }




    protected void OnBtnAboutClicked(object sender, EventArgs e)
    {
        var abt = new AboutDialog();
        abt.ProgramName = "TRLevelUtility";
        abt.Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        abt.Copyright = "Copyright © 2017 zdimension";
        abt.Comments = 
@"Free, open-source editor for almost all classic TR files.

Thanks to the TRosettaStone authors for level and other game files reverse-engineering.
Thanks to IceBerg for the TOMBPC.DAT information.
Thanks to aktrekker for the TRLE .PRJ format documentation.
Thanks to sapper for the CUTSEQ.BIN/.PAK format documentation.
Special thanks to b122251 and Cochrane from TRF.";
        #region LGPLv3
        abt.License =
@"                   GNU LESSER GENERAL PUBLIC LICENSE
                       Version 3, 29 June 2007

 Copyright (C) 2007 Free Software Foundation, Inc. <http://fsf.org/>
 Everyone is permitted to copy and distribute verbatim copies
 of this license document, but changing it is not allowed.


  This version of the GNU Lesser General Public License incorporates
the terms and conditions of version 3 of the GNU General Public
License, supplemented by the additional permissions listed below.

  0. Additional Definitions.

  As used herein, ""this License"" refers to version 3 of the GNU Lesser
General Public License, and the ""GNU GPL"" refers to version 3 of the GNU
General Public License.

  ""The Library"" refers to a covered work governed by this License,
other than an Application or a Combined Work as defined below.

  An ""Application"" is any work that makes use of an interface provided
by the Library, but which is not otherwise based on the Library.
Defining a subclass of a class defined by the Library is deemed a mode
of using an interface provided by the Library.

  A ""Combined Work"" is a work produced by combining or linking an
Application with the Library.  The particular version of the Library
with which the Combined Work was made is also called the ""Linked
Version"".

  The ""Minimal Corresponding Source"" for a Combined Work means the
Corresponding Source for the Combined Work, excluding any source code
for portions of the Combined Work that, considered in isolation, are
based on the Application, and not on the Linked Version.

  The ""Corresponding Application Code"" for a Combined Work means the
object code and/or source code for the Application, including any data
and utility programs needed for reproducing the Combined Work from the
Application, but excluding the System Libraries of the Combined Work.

  1. Exception to Section 3 of the GNU GPL.

  You may convey a covered work under sections 3 and 4 of this License
without being bound by section 3 of the GNU GPL.

  2. Conveying Modified Versions.

  If you modify a copy of the Library, and, in your modifications, a
facility refers to a function or data to be supplied by an Application
that uses the facility (other than as an argument passed when the
facility is invoked), then you may convey a copy of the modified
version:

   a) under this License, provided that you make a good faith effort to
   ensure that, in the event an Application does not supply the
   function or data, the facility still operates, and performs
   whatever part of its purpose remains meaningful, or

   b) under the GNU GPL, with none of the additional permissions of
   this License applicable to that copy.

  3. Object Code Incorporating Material from Library Header Files.

  The object code form of an Application may incorporate material from
a header file that is part of the Library.  You may convey such object
code under terms of your choice, provided that, if the incorporated
material is not limited to numerical parameters, data structure
layouts and accessors, or small macros, inline functions and templates
(ten or fewer lines in length), you do both of the following:

   a) Give prominent notice with each copy of the object code that the
   Library is used in it and that the Library and its use are
   covered by this License.

   b) Accompany the object code with a copy of the GNU GPL and this license
   document.

  4. Combined Works.

  You may convey a Combined Work under terms of your choice that,
taken together, effectively do not restrict modification of the
portions of the Library contained in the Combined Work and reverse
engineering for debugging such modifications, if you also do each of
the following:

   a) Give prominent notice with each copy of the Combined Work that
   the Library is used in it and that the Library and its use are
   covered by this License.

   b) Accompany the Combined Work with a copy of the GNU GPL and this license
   document.

   c) For a Combined Work that displays copyright notices during
   execution, include the copyright notice for the Library among
   these notices, as well as a reference directing the user to the
   copies of the GNU GPL and this license document.

   d) Do one of the following:

       0) Convey the Minimal Corresponding Source under the terms of this
       License, and the Corresponding Application Code in a form
       suitable for, and under terms that permit, the user to
       recombine or relink the Application with a modified version of
       the Linked Version to produce a modified Combined Work, in the
       manner specified by section 6 of the GNU GPL for conveying
       Corresponding Source.

       1) Use a suitable shared library mechanism for linking with the
       Library.  A suitable mechanism is one that (a) uses at run time
       a copy of the Library already present on the user's computer
       system, and (b) will operate properly with a modified version
       of the Library that is interface-compatible with the Linked
       Version.

   e) Provide Installation Information, but only if you would otherwise
   be required to provide such information under section 6 of the
   GNU GPL, and only to the extent that such information is
   necessary to install and execute a modified version of the
   Combined Work produced by recombining or relinking the
   Application with a modified version of the Linked Version. (If
   you use option 4d0, the Installation Information must accompany
   the Minimal Corresponding Source and Corresponding Application
   Code. If you use option 4d1, you must provide the Installation
   Information in the manner specified by section 6 of the GNU GPL
   for conveying Corresponding Source.)

  5. Combined Libraries.

  You may place library facilities that are a work based on the
Library side by side in a single library together with other library
facilities that are not Applications and are not covered by this
License, and convey such a combined library under terms of your
choice, if you do both of the following:

   a) Accompany the combined library with a copy of the same work based
   on the Library, uncombined with any other library facilities,
   conveyed under the terms of this License.

   b) Give prominent notice with the combined library that part of it
   is a work based on the Library, and explaining where to find the
   accompanying uncombined form of the same work.

  6. Revised Versions of the GNU Lesser General Public License.

  The Free Software Foundation may publish revised and/or new versions
of the GNU Lesser General Public License from time to time. Such new
versions will be similar in spirit to the present version, but may
differ in detail to address new problems or concerns.

  Each version is given a distinguishing version number. If the
Library as you received it specifies that a certain numbered version
of the GNU Lesser General Public License ""or any later version""
applies to it, you have the option of following the terms and
conditions either of that published version or of any later version
published by the Free Software Foundation. If the Library as you
received it does not specify a version number of the GNU Lesser
General Public License, you may choose any version of the GNU Lesser
General Public License ever published by the Free Software Foundation.

  If the Library as you received it specifies that a proxy can decide
whether future versions of the GNU Lesser General Public License shall
apply, that proxy's public statement of acceptance of any version is
permanent authorization for you to choose that version for the
Library.";
        #endregion
        abt.Website = "http://www.zdimension.ml";
        abt.WebsiteLabel = "Website";
        abt.IconList = IconList;
		abt.Logo = IconList[4];
        abt.ParentWindow = this.GdkWindow;
        abt.Run();
        abt.Destroy();
    }


    protected void OnBtnOpenScriptClicked(object sender, EventArgs e)
    {
		try
		{
			var fn = Helper.getFile2(this, "Open a script file", false, pgtpcscript.FileFilter, pgtr4script.FileFilter);
			if (fn.Item1 == null) return;
			var dlg = new TPCImportDlg();
			dlg.IconList = this.IconList;
			if (System.IO.Path.GetFileNameWithoutExtension(fn.Item1).ToUpper() == "TOMBPSX")
				dlg.Platform = 1;
			dlg.ParentWindow = this.GdkWindow;
			dlg.Run();
			dlg.Destroy();
			if (dlg.Game <= 3)
			{
				pgtpcscript.Open(fn.Item1, dlg.Game, dlg.Platform);
				setCurPage(1);
			}
			else
			{
				pgtpcscript.Open(fn.Item1, dlg.Game);
				setCurPage(2);
			}
		}
		catch (Exception ex)
		{
			Helper.Die(ex, "An error occured while opening the script.", this);
		}
    }


    protected void OnBtnNewScriptTPCClicked(object sender, EventArgs e)
    {
        pgtpcscript.CreateNew();
        setCurPage(1);
    }

    private void setCurPage(int id)
    {
        notebook1.Page = id;
        //currentPage = notebook1.GetNthPage(id) as ITLUPage;
    }

    protected void OnBtnNewScriptTR4Clicked(object sender, EventArgs e)
    {
        pgtr4script.CreateNew();
        notebook1.CurrentPage = 2;
    }




    protected void OnBtnSaveClicked(object sender, EventArgs e)
    {
        currentPage.Save();
        //if (notebook1.Page == 1)
        {
            /*if (tpcFilename == "")
            {
                OnBtnSaveAsClicked(sender, e);
                return;
            }

            if (tpcIsTxt)
            {
                saveTpcTXT(tpcFilename, tpcStringsFilename, cbxTPCPSX.Active ? 1 : 0, cbxTPCTR3.Active ? 3 : (cbxTPCtr2beta.Active ? 2 : 1));
            }
            else
            {
                saveTpcDAT(tpcFilename, cbxTPCPSX.Active ? 1 : 0, cbxTPCTR3.Active ? 3 : (cbxTPCtr2beta.Active ? 2 : 1));
            }*/
        }
    }

    protected void OnBtnSaveAsClicked(object sender, EventArgs e)
    {
        currentPage.SaveAs();
        //if (notebook1.Page == 1)
        {
            /*var fn = Helper.getFile2(this, "Save a script file", true, "TR2-3 script file (TOMBPC.DAT, TOMBPSX.DAT)|*.DAT", "Uncompiled TR2-3 script file (PCfinal.txt, PSXfinal.txt)|*.TXT");
            var dlg = new TPCImportDlg();
            dlg.IconList = IconList;
            dlg.ParentWindow = this.GdkWindow;
            dlg.Game = cbxTPCTR3.Active ? 3 : (cbxTPCtr2beta.Active ? 2 : 1);
            dlg.Platform = cbxTPCPSX.Active ? 1 : 0;
            dlg.Run();
            dlg.Destroy();
            if (fn.Item2 == 0)
            {
                saveTpcDAT(fn.Item1, dlg.Platform, dlg.Game);
            }
            else
            {
                saveTpcTXT(fn.Item1, Helper.getFile(this, "Strings file", true, "Strings file (*.txt)|*.TXT"), dlg.Platform, dlg.Game);
            }*/
        }
    }

    protected void SetSave(bool can)
    {
        btnSave.Visible = btnSaveAs.Visible = can;
    }

	protected void OnBtnOpenRawClicked(object sender, EventArgs e)
	{
		var fn = Helper.getFile2(this, "Open a raw file", false, pgraw1.FileFilter);
		if (fn.Item1 == null) return;
		pgraw1.Open(fn.Item1, fn.Item2);
		setCurPage(4);
	}

	protected void OnBtnOpenCDAClicked(object sender, EventArgs e)
	{
		var fn = Helper.getFile2(this, "Open a CDAUDIO file", false, pgcdaudio1.FileFilter);
		if (fn.Item1 == null) return;
		pgcdaudio1.Open(fn.Item1);
		setCurPage(5);
	}
}
