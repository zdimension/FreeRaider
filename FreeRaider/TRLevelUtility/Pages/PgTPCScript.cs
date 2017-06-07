using System;
using Gtk;
using System.Collections.Generic;
using System.Linq;
using FreeRaider.Loader;
using System.Globalization;

namespace TRLevelUtility
{
    [System.ComponentModel.ToolboxItem(true)]
    public partial class PgTPCScript : Gtk.Bin, ITRLUPage
    {
        public PgTPCScript()
        {
            this.Build();
        }

        private string tpcLastDesc;
        private TextIter tpcLastCur;
        private bool tpcDescChanging;

        private void Init()
        {
            SaveStateChanged(true);

            larTPCLevels.AddColumns("Level description", "Level filename", "Picture filename");
            larTPCLevels.InitStore();

            larTPCrpl.InitStore();
            larTPCcut.InitStore();
            larTPClegal.InitStore();
            larTPCgs.InitStore(true);
            larTPCps.InitStore(true);
            larTPCpss.InitStore(true);
            larTPCPuzzle.AddColumns("Level", "Puzzle 1", "Puzzle 2", "Puzzle 3", "Puzzle 4");
            larTPCPuzzle.InitStore(true);
            (larTPCPuzzle.TreeView.Columns[0].CellRenderers[0] as CellRendererText).Editable = false;
            larTPCPickups.AddColumns("Level", "Pickup 1", "Pickup 2");
            larTPCPickups.InitStore(true);
            (larTPCPickups.TreeView.Columns[0].CellRenderers[0] as CellRendererText).Editable = false;
            larTPCKeys.AddColumns("Level", "Key 1", "Key 2", "Key 3", "Key 4");
            larTPCKeys.InitStore(true);
            (larTPCKeys.TreeView.Columns[0].CellRenderers[0] as CellRendererText).Editable = false;
            larTPCSecrets.AddColumns("Level", "Secret 1", "Secret 2", "Secret 3", "Secret 4");
            larTPCSecrets.InitStore(true);
            (larTPCSecrets.TreeView.Columns[0].CellRenderers[0] as CellRendererText).Editable = false;
            larTPCSpecial.AddColumns("Level", "Special 1", "Special 2");
            larTPCSpecial.InitStore(true);
            (larTPCSpecial.TreeView.Columns[0].CellRenderers[0] as CellRendererText).Editable = false;

            larTPCdemolvls.InitStore();
            (larTPCdemolvls.TreeView.Columns[0].CellRenderers[0] as CellRendererText).Editable = false;

            sbDemoLvl.Numeric = false;

            OnCbxTPCPSXToggled(this, null);

            tpcFirstOption.OnGetLevelName = tpcGetLvName;
            tpcTitleReplace.OnGetLevelName = tpcGetLvName;
            tpcOnDeathDemoMode.OnGetLevelName = tpcGetLvName;
            tpcOnDeathInGame.OnGetLevelName = tpcGetLvName;
            tpcOnDemoInterrupt.OnGetLevelName = tpcGetLvName;
            tpcOnDemoEnd.OnGetLevelName = tpcGetLvName;

            larTPCscCommands.InitStore();
            (larTPCscCommands.TreeView.Columns[0].CellRenderers[0] as CellRendererText).Editable = false;
            larTPCscSlots.InitStore(true);
            (larTPCscSlots.TreeView.Columns[0].CellRenderers[0] as CellRendererText).Editable = false;
            larTPCscSlots.AddRow("Frontend");
            larTPCscSlots.SetSizeRequest(200, -1);
            sbTPCOperand.Numeric = false;

            sbSingleLevel.Numeric = false;
            sbSingleLevel.Value = -1;



            foreach (var k in tpcDescriptions) cbxTPCLoadDesc.AppendText(k.Key);

            foreach (var c in tpcCommands) cbxTPCCommand.AppendText(c.Key + ": " + c.Value.Split('|').Last());

            tvTPCDesc.Buffer.Changed += (sender, e) =>
            {
                if (tpcDescChanging) return;
                var l = tvTPCDesc.Buffer.CharCount;
                if (l > 256)
                {
                    tpcDescChanging = true;
                    tvTPCDesc.Buffer.Text = tpcLastDesc;
                    tvTPCDesc.Buffer.PlaceCursor(tpcLastCur);
                    tpcDescChanging = false;
                }
                else
                {
                    lblTPCDesc.Text = "Description\n\n" + (256 - tvTPCDesc.Buffer.CharCount);
                    tpcLastDesc = tvTPCDesc.Buffer.Text;
                    tpcLastCur = tvTPCDesc.Buffer.GetIterAtMark(tvTPCDesc.Buffer.InsertMark).Copy();
                }
            };
        }

        private List<List<Tuple<int, int?>>> tpcScript = new List<List<Tuple<int, int?>>>
    {
        new List<Tuple<int, int?>>()
    };

        private List<int> tpcDemoLevels = new List<int>();

        private string tpcGetLvName(int id, int type)
        {
			if (id == 0) return "do nothing";
            var theArr = type == 0 ? larTPCLevels : larTPCdemolvls;
            if (id < theArr.Store.IterNChildren())
                return theArr[id, 0];
            return "invalid";
        }

        private readonly Dictionary<string, string> tpcDescriptions = new Dictionary<string, string>
    {
        {"Tomb Raider II – Internal Development Version", "Tomb Raider II PC Internal Development Version (c) Core Design Ltd 1997" },
        {"Tomb Raider II – Dagger of Xian (1.0)", "Tomb Raider II Script. Final Release Version 1.0 (c) Core Design Ltd 1997"},
        {"Tomb Raider II – Dagger of Xian (1.1)", "Tomb Raider II Script. Final Release Version 1.1 (c) Core Design Ltd 1997"},
        {"Tomb Raider II – Demo 1: The Great Wall (September 21, 1997)", "Tomb Raider II Script. Mag Preview (c) Core Design Ltd 1997"},
        {"Tomb Raider II – Demo 2: Venice (April 15, 1998)","Tomb Raider II Script. Internet Demo 15/4/98 (c) Core Design Ltd 1998"},
        {"Tomb Raider II Gold – Golden Mask ", "Tomb Raider II Script. Final Release Version 1.1 (c) Core Design Ltd 1997"},
        {"Tomb Raider II Gold – Demo 1: The Cold War", "Tomb Raider II Script. Final Release Version 1.1 (c) Core Design Ltd 1997"},
        {"Tomb Raider II Gold – Demo 2: Fool's Gold", "Tomb Raider II Script. Final Release Version 1.1 (c) Core Design Ltd 1997"},
        {"Tomb Raider III – Adventures of Lara Croft", "Tomb Raider III Script. Final Release (c) Core Design Ltd 1998"}, // Ignoring the "forgotten" text from the original scripts
        {"Tomb Raider III – Demo (October 6, 1998)", "Tomb Raider III Script. E3 Release (c) Core Design Ltd 1998"},
        {"Tomb Raider III – Demo (October 22, 1998)", "Tomb Raider III Script. E3 Release (c) Core Design Ltd 1998"},
        {"Tomb Raider III Gold – The Lost Artifact", "Tomb Raider III Script. E3 Release (c) Core Design Ltd 1998"}
    };

        protected void OnCbxTPCLoadDescChanged(object sender, EventArgs e)
        {
            tvTPCDesc.Buffer.Text = tpcDescriptions[cbxTPCLoadDesc.ActiveText];
        }

        bool tpcFlagsSetting = false;

        private void tpcSetFlags(int val)
        {
            tpcFlagsSetting = true;
            tbeTPCFlags.Value = val;
            foreach (var cbx in vboxTPCFlags.Children.Where(x => x is CheckButton))
            {
                ((CheckButton)cbx).Active = (val & (1 << (int.Parse(cbx.Name.Substring(10)) - 1))) != 0;
            }
            tpcFlagsSetting = false;
        }

        protected void tpcFlagToggle(object sender, EventArgs e)
        {
            if (tpcFlagsSetting) return;
            var actives = vboxTPCFlags.Children.Where(x => (x as CheckButton)?.Active ?? false).Select(x => 1 << (int.Parse(x.Name.Substring(10)) - 1));
            var val = 0;
            if (actives.Any())
                val = actives.Aggregate((x, y) => x | y);
            tpcSetFlags(val);
        }

        protected void OnTbeTPCFlagsChanged(object sender, EventArgs e)
        {
            if (tpcFlagsSetting) return;
            tpcSetFlags(tbeTPCFlags.Value);
        }

        private bool tpcIsTxt = false;
        private string tpcFilename = "";
        private string tpcStringsFilename = "";

        public void Open(string filename)
        {
            var ext = System.IO.Path.GetExtension(filename).ToUpper();
            var dlg = new TPCImportDlg();
            dlg.IconList = ParentWnd.IconList;
            if (System.IO.Path.GetFileNameWithoutExtension(filename).ToUpper() == "TOMBPSX")
                dlg.Platform = 1;
            dlg.ParentWindow = this.GdkWindow;
            dlg.Run();
            cbxTPCTR3.Active = dlg.Game == 3;
            cbxTPCtr2beta.Active = dlg.Game == 2;
            cbxTPCPSX.Active = dlg.Platform == 1;
            dlg.Destroy();
            tpcFilename = filename;
            if (ext == ".DAT")
            {
                tpcIsTxt = false;

                load_tpc(TOMBPCFile.ParseDAT(filename, dlg.Platform == 1, dlg.Game == 2));
            }
            else if (ext == ".TXT")
            {
                tpcIsTxt = true;
                var f = TOMBPCFile.ParseTXT(
                    filename, dlg.Game == 3 ? TOMBPCGameVersion.TR3 : TOMBPCGameVersion.TR2, dlg.Platform == 1,
                    () => Helper.getFile(ParentWnd, "Strings file", false, "Strings file (*.txt)|*.TXT"), false);
                tpcStringsFilename = f.stringsFilename;
                load_tpc(f);
            }
        }

        private void load_tpc(TOMBPCFile file)
        {
            Init();

            // Levels
            for (var i = 0; i < file.Level_Names.Length; i++)
            {
                larTPCLevels.AddRow(file.Level_Names[i], file.Level_Filenames[i], i < file.Picture_Filenames.Length ? file.Picture_Filenames[i] : "");
            }
            larTPCLevels.SelectedRow = -1;

            // FMV / Cutscenes
            foreach (var rpl in file.FMV_Filenames) larTPCrpl.AddRow(rpl);
            foreach (var cut in file.Cutscene_Filenames) larTPCcut.AddRow(cut);
            foreach (var legal in file.Title_Filenames) larTPClegal.AddRow(legal);

            // Options
            cbxTPCPSX.Active = file.IsPSX;
            //cbxTPCtr2beta.Active = (file.Secrets.All(x => x.Length > 0) && file.Secrets.All(x => x.Length > 0));
            var d = file.Description.Trim('\0');
            if (tpcDescriptions.Any(x => x.Value == d))
            {
                cbxTPCLoadDesc.Active = Array.IndexOf(tpcDescriptions.ToArray(), tpcDescriptions.First(x => x.Value == d));
            }
            else tvTPCDesc.Buffer.Text = d;

            tbeTPCCypher.Value = file.Cypher_Code;
            cbxTPCLanguage.Active = (int)file.Language;
            tpcFirstOption.Value = file.FirstOption;
            tpcTitleReplace.Value = (uint)file.Title_Replace;
            tpcOnDeathDemoMode.Value = file.OnDeath_Demo_Mode;
            tpcOnDeathInGame.Value = file.OnDeath_InGame;
            tpcOnDemoInterrupt.Value = file.On_Demo_Interrupt;
            tpcOnDemoEnd.Value = file.On_Demo_End;
            tpcNoInputTime.Value = file.NoInput_Time;
            tpcTitleSound.Value = file.Title_Track;
            sbSingleLevel.Value = file.SingleLevel == 0xFFFF ? -1 : (int)file.SingleLevel;
            tpcSecretSound.Value = file.Secret_Track;
            tbeTPCFlags.Value = (int)file.Flags;

            // Script
            for (var i = 0; i < file.Script.Length; i++)
            {
                var l = new List<Tuple<int, int?>>();
                var cur = file.Script[i];
                for (var j = 0; j < cur.Length; j++)
                {
                    int cmd = cur[j];
                    if (cmd < tpcCommands.Count)
                    {
                        if (cmd == 9) continue;
                        int? val = null;
                        if (cmd > 8) cmd--;
                        if (cmd >= 18) cmd++;
                        if (tpcCommands.Values.ToArray()[cmd][0] != '|')
                        {
                            if (j < cur.Length - 1)
                            {
                                j++;
                                val = cur[j];
                            }
                        }
                        if (val != null && cmd == 17 && val >= 1000)
                        {
                            cmd = 18;
                            val -= 1000;
                        }
                        l.Add(new Tuple<int, int?>(cmd, val));
                    }
                }
                tpcScript[i] = l;
            }
            larTPCscSlots.TreeView.Selection.UnselectAll();
            larTPCscSlots.SelectedRow = 0;

            larTPCdemolvls.Store.Clear();
            tpcDemoLvlsLoading = true;
            tpcDemoLevels = file.DemoLevelIDs.Select(x => (int)x).ToList();
            foreach (var dl in tpcDemoLevels) larTPCdemolvls.AddRow(dl < file.Level_Names.Length ? file.Level_Names[dl] : "invalid level");
            tpcDemoLvlsLoading = false;

            for (var i = 0; i < file.PSXFMVInfo.Length; i++)
            {
                tpcFmvInfo[i] = file.PSXFMVInfo[i];
            }
            larTPCrpl.SelectedRow = -1;

            // Strings
            larTPCgs.Store.Clear();
            foreach (var gs in file.Game_Strings) larTPCgs.AddRow(gs);
            larTPCps.Store.Clear();
            foreach (var ps in file.PC_Strings) larTPCps.AddRow(ps);
            larTPCpss.Store.Clear();
            foreach (var pss in file.PSX_Strings) larTPCpss.AddRow(pss);
            larTPCPuzzle.Store.Clear();
            for (var i = 0; i < file.Level_Names.Length; i++) larTPCPuzzle.AddRow(file.Level_Names[i], file.Puzzles[0][i], file.Puzzles[1][i], file.Puzzles[2][i], file.Puzzles[3][i]);
            larTPCPickups.Store.Clear();
            for (var i = 0; i < file.Level_Names.Length; i++) larTPCPickups.AddRow(file.Level_Names[i], file.Pickups[0][i], file.Pickups[1][i]);
            larTPCKeys.Store.Clear();
            for (var i = 0; i < file.Level_Names.Length; i++) larTPCKeys.AddRow(file.Level_Names[i], file.Keys[0][i], file.Keys[1][i], file.Keys[2][i], file.Keys[3][i]);
            if (cbxTPCtr2beta.Active)
            {
                larTPCSecrets.Store.Clear();
                for (var i = 0; i < file.Level_Names.Length; i++) larTPCSecrets.AddRow(file.Level_Names[i], file.Secrets[0][i], file.Secrets[1][i], file.Secrets[2][i], file.Secrets[3][i]);
                larTPCSpecial.Store.Clear();
                for (var i = 0; i < file.Level_Names.Length; i++) larTPCSpecial.AddRow(file.Level_Names[i], file.Special[0][i], file.Special[1][i]);
            }
        }

        private bool tpcDemoLvlsLoading = false;

        public void CreateNew()
        {
            Init();
        }

        protected void OnCbxTPCtr2betaToggled(object sender, EventArgs e)
        {
            if (cbxTPCtr2beta.Active)
            {
                cbxTPCTR3.Active = false;
            }
        }

        protected void OnLarTPCLevelsRowAdded(int newID, bool dupl)
        {
            larTPCPuzzle.AddRow(newID, dupl ? larTPCPuzzle[newID - 1] : larTPCLevels[newID, 0].AddArr(new[] { "P1", "P2", "P3", "P4" }));
            larTPCPickups.AddRow(newID, dupl ? larTPCPickups[newID - 1] : larTPCLevels[newID, 0].AddArr(new[] { "P1", "P2" }));
            larTPCKeys.AddRow(newID, dupl ? larTPCKeys[newID - 1] : larTPCLevels[newID, 0].AddArr(new[] { "K1", "K2", "K3", "K4" }));
            larTPCSecrets.AddRow(newID, dupl ? larTPCSecrets[newID - 1] : larTPCLevels[newID, 0].AddArr(new[] { "S1", "S2", "S3", "S4" }));
            larTPCSpecial.AddRow(newID, dupl ? larTPCSpecial[newID - 1] : larTPCLevels[newID, 0].AddArr(new[] { "S1", "S2" }));
            larTPCscSlots.AddRow(newID + 1, larTPCLevels[newID, 0]);
            tpcScript.Insert(newID + 1, dupl ? tpcScript[newID] : new List<Tuple<int, int?>>());

            if (larTPCLevels.Count != 1)
            {
                foreach (var slot in tpcScript)
                {
                    for (var i = 0; i < slot.Count; i++)
                    {
                        var cmd = slot[i];
                        if (new[] { 0, 4, 12 }.Contains(cmd.Item1) && cmd.Item2 >= newID)
                        {
                            slot[i] = new Tuple<int, int?>(cmd.Item1, cmd.Item2 + 1);
                        }
                    }
                }

                foreach (var t in new[] { tpcFirstOption, tpcTitleReplace, tpcOnDeathDemoMode, tpcOnDeathInGame, tpcOnDemoInterrupt, tpcOnDemoEnd })
                {
                    if (t.Command == 0 && t.UncompiledValue >= newID)
                    {
                        t.UncompiledValue++;
                    }
                }
            }
        }

        protected void OnLarTPCLevelsRowRemoved(int id)
        {
            larTPCPuzzle.RemoveRow(id);
            larTPCPickups.RemoveRow(id);
            larTPCKeys.RemoveRow(id);
            larTPCSecrets.RemoveRow(id);
            larTPCSpecial.RemoveRow(id);
            larTPCscSlots.RemoveRow(id + 1);
            tpcScript.RemoveAt(id + 1);

            foreach (var slot in tpcScript)
            {
                for (var i = 0; i < slot.Count; i++)
                {
                    var cmd = slot[i];
                    if (cmd.Item1 == 4 && cmd.Item2 >= id)
                    {
                        slot[i] = new Tuple<int, int?>(cmd.Item1, cmd.Item2 - 1);
                    }
                }
            }
        }

        protected void OnCbxTPCPSXToggled(object sender, EventArgs e)
        {
        }

        protected void OnCbxTPCCommandChanged(object sender, EventArgs e)
        {
            if (!cbxTPCCommand.Sensitive) return;
            var slotID = larTPCscSlots.SelectedRow;
            var cmdID = larTPCscCommands.SelectedRow;
            var d = tpcCommands[cbxTPCCommand.ActiveText.Split(':')[0]];
            var i = d.IndexOf('|');
            //lblTPCcmdHelp.Text = d.Substring(d.LastIndexOf('|') + 1);
            if (i == 0)
            {
                lblTPCoperandName.Text = "No operand";
                sbTPCOperand.Sensitive = false;
            }
            else
            {
                lblTPCoperandName.Text = d.Substring(0, i);
                sbTPCOperand.Sensitive = true;
            }
            //tpcScript[slotID][cmdID] = Tuple.Create(cbxTPCCommand.Active, sbTPCOperand.Sensitive ? (int?)sbTPCOperand.ValueAsInt : null);
            sbTPCOperand.Value = sbTPCOperand.Value;
            writeToTpcScript();
        }

        protected void OnLarTPCLevelsRowEdited(int row, int column)
        {
            larTPCPuzzle[row, 0] = larTPCLevels[row, 0];
            larTPCPickups[row, 0] = larTPCLevels[row, 0];
            larTPCKeys[row, 0] = larTPCLevels[row, 0];
            larTPCSecrets[row, 0] = larTPCLevels[row, 0];
            larTPCSpecial[row, 0] = larTPCLevels[row, 0];
            larTPCscSlots[row + 1, 0] = larTPCLevels[row, 0];
        }

        private readonly Dictionary<string, string> tpcCommands = new Dictionary<string, string>
    {
        {"PICTURE","Picture ID||Unused. On PC, crashes if used and file missing. Otherwise, no action."},
        {"LIST_START","||Unused. Maybe PSX."},
        {"LIST_END", "||Unused. Maybe PSX."},
        {"FMV", "FMV ID|Display FMV '{0}'|Display Full Motion Video."},
        {"GAME", "Level ID|Start level {0}|Start a playable level."},
        {"CUT", "Cutscene ID|Display cutscene '{0}'|Display cut scene sequence."},
        {"COMPLETE", "|Display level-completion statistics panel"},
        {"DEMO / PCDEMO", "Demo ID|Display demo sequence {0}|Display demo sequence."},
        {"JUMPTOSEQUENCE", "Sequence ID|Jump to sequence '{0}'|Jump to another sequence. Operand is sequence ID."},
     //   {"END", "|Closes script sequences."},
        {"TRACK / PCTRACK", "Track ID|Play soundtrack {0}|Play Soundtrack (it precedes opcodes of associated levels)."},
        {"SUNSET", "||Unknown. Nothing changes in-game. Maybe this is an ancestor of the TR4 LensFlare command, not actually implemented under TR2."},
			{"LOAD_PIC", "Picture ID|Show chapter screen '{0}'|Show chapter screen under TR2 (PSX only) and TR3."},
        {"DEADLY_WATER", "||Unknown. Nothing changes in-game. Maybe this is an ancestor of the TR3 Death_By_Drowning effect, not actually implemented under TR2."},
        {"REMOVE_WEAPONS", "|Lara starts the level with no weapons."},
        {"GAMECOMPLETE", "|End of game, shows the final statistics and starts the credits sequence with music ID = 52."},
        {"CUTANGLE", "HRotation|The animation of the camera of the cut scene is rotated by {0}|Matches the North-South orientation of the Room Editor and the North-South orientation of the 3D animated characters from a CAD application."},
        {"NOFLOOR", "Depth|Lara dies when her feet reach depth: {0} (relative to the depth when starting the level)|Death_By_Depth. Lara dies when her feet reach the given depth. If falling, 4 to 5 extra blocks are added to Depth."},
      //  {"STARTINV / BONUS", "Item ID|Items given to Lara at level-start or at all-secrets-found."},
        {"BONUS", "Item ID|Give item {0} when all secrets found|Items given to Lara at all-secrets-found."},
        {"STARTINV", "Item ID|Give item {0} at level start|Items given to Lara at level-start."},
        {"STARTANIM", "Anim ID|Start level with animation {0}|Special Lara's animation when the level starts."},
        {"SECRETS", "Number of secrets|The {0}|If zero, the level does not account for secrets. Non-zero value means the level must be accounted for secrets."},
        {"KILLTOCOMPLETE", "|Kill all enemies to finish the level."},
        {"REMOVE_AMMO", "|Lara starts the level without ammunition or medi packs."}
    };


        private readonly Dictionary<string, string> items2 = new Dictionary<string, string>
    {
        {"PISTOLS", "Standard pistols (2)"},
{"SHOTGUN", "Shotgun (1)"},
{"AUTOPISTOLS", "Automatic Pistols (2)"},
{"UZIS", "Uzis (2)"},
{"HARPOON", "Harpoon gun (1)"},
{"M16", "M16 (1)"},
{"ROCKET", "Grenade launcher (1)"},
{"PISTOLS_AMMO", "Pistol clip (no effect, infinite by default)"},
{"SHOTGUN_AMMO", "Shotgun-shell box (adds 2 shells)"},
{"AUTOPISTOLS_AMMO", "Automatic Pistols clip (adds 2 shells)"},
{"UZI_AMMO", "Uzi clip (adds 2 shells)"},
{"HARPOON_AMMO", "Harpoon bundle (adds 2 harpoons)"},
{"M16_AMMO", "M16 clip (adds 2 shells)"},
{"ROCKET_AMMO", "Grenade pack (adds 1 grenade)"},
{"FLARES", "Flare box (adds 1 flare)"},
{"MEDI", "Small medi pack (adds 1 pack)"},
{"BIGMEDI", "Big medi pack (adds 1 pack)"},
{"PICKUP1", "Pickup item 1"},
{"PICKUP2", "Pickup item 2"},
{"PUZZLE1", "Puzzle item 1"},
{"PUZZLE2", "Puzzle item 2"},
{"PUZZLE3", "Puzzle item 3"},
{"PUZZLE4", "Puzzle item 4"},
{"KEY1", "Key item 1"},
{"KEY2", "Key item 2"},
{"KEY3", "Key item 3"},
{"KEY4", "Key item 4"}
    };

        private readonly Dictionary<string, string> items3 = new Dictionary<string, string>
    {
        {"PISTOLS", "Standard pistols (2)"},
{"SHOTGUN", "Shotgun (1)"},
        {"DESERTEAGLE", "Desert Eagle (1)"},
{"UZIS", "Uzis (2)"},
{"HARPOON", "Harpoon gun (1)"},
{"MP5", "MP5 (1)"},
{"ROCKET", "Rocket launcher (1)"},
        {"GRENADE", "Grenade launcher (1)"},
{"PISTOLS_AMMO", "Pistol clip (no effect, infinite by default)"},
{"SHOTGUN_AMMO", "Shotgun-shell box (adds 2 shells)"},
{"DESERTEAGLE_AMMO", "Desert Eagle clip (adds 5 shells)"},
{"UZI_AMMO", "Uzi clip (adds 2 shells)"},
{"HARPOON_AMMO", "Harpoon bundle (adds 2 harpoons)"},
{"MP5_AMMO", "MP5 clip (adds 2 shells)"},
{"ROCKET_AMMO", "Rocket pack (adds 1 rocket)"},
        {"GRENADE_AMMO", "Grenade pack (adds 1 grenade)"},
{"FLARES", "Flare box (adds 1 flare)"},
{"MEDI", "Small medi pack (adds 1 pack)"},
{"BIGMEDI", "Big medi pack (adds 1 pack)"},
{"PICKUP1", "Pickup item 1"},
{"PICKUP2", "Pickup item 2"},
{"PUZZLE1", "Puzzle item 1"},
{"PUZZLE2", "Puzzle item 2"},
{"PUZZLE3", "Puzzle item 3"},
{"PUZZLE4", "Puzzle item 4"},
{"KEY1", "Key item 1"},
{"KEY2", "Key item 2"},
{"KEY3", "Key item 3"},
        {"KEY4", "Key item 4"},
        {"CRYSTAL", "Save crystal"}
    };

        private string textForCmdValue(int cmd, int val)
        {
            string ret = null;
            string[] arr = null;
            switch (cmd)
            {
                case 0: // PICTURE
                case 11: // LOAD_PIC
                    arr = larTPCLevels.GetColumn(2).Select(x => string.IsNullOrWhiteSpace(x) ? "invalid" : x).ToArray();
                    break;
                case 2: // PSX_FMV
                case 3: // FMV
                    arr = larTPCrpl.GetColumn(0);
                    break;
                case 4: // GAME
                    arr = larTPCLevels.GetColumn(0).Select((x, i) => "'" + x + "' <" + larTPCLevels.GetColumn(1)[i] + ">").ToArray();
                    break;
                case 5: // CUT
                    arr = larTPCcut.GetColumn(0);
                    break;
                case 8: // JUMPTOSEQUENCE:
                    arr = larTPCscSlots.GetColumn(0);
                    break;
                case 15: // CUTANGLE
                    ret = (360m * val / 65536) + " degrees";
                    break;
                case 16: // NOFLOOR
                    ret = (val / 1024m) + " blocks";
                    break;
                case 17: // STARTINV
                case 18: // BONUS
                    arr = (cbxTPCTR3.Active ? items3 : items2).Values.ToArray();
                    break;
                case 20: // SECRETS
                    ret = val == 0 ? "level does not account for secrets" : "must be accounted for secrets";
                    break;
            }
            if (ret == null && arr != null)
            {
                ret = "invalid";
                if (val < arr.Length)
                {
                    ret = arr[val];

                    if (cmd == 17 || cmd == 18)
                    {
                        var tmp = (cbxTPCTR3.Active ? items3 : items2).Keys.ToArray()[val];
                        var rw = larTPCscSlots.SelectedRow - 1;
                        if (tmp.StartsWith("PICKUP"))
                            ret += " (" + larTPCPickups[rw, int.Parse(tmp.Substring(6))] + ")";
                        else if (tmp.StartsWith("PUZZLE"))
                            ret += " (" + larTPCPuzzle[rw, int.Parse(tmp.Substring(6))] + ")";
                        else if (tmp.StartsWith("KEY"))
                            ret += " (" + larTPCKeys[rw, int.Parse(tmp.Substring(3))] + ")";
                    }
                }
            }

            return ret;
        }

        protected void OnSbTPCOperandOutput(object o, OutputArgs args)
        {
            if (!sbTPCOperand.Sensitive) sbTPCOperand.Text = "";
            else
            {
                sbTPCOperand.Text = sbTPCOperand.ValueAsInt.ToString();
                var ret = textForCmdValue(cbxTPCCommand.Active, sbTPCOperand.ValueAsInt);
                if (ret != null)
                {
                    sbTPCOperand.Text += " (" + ret + ")";
                }
            }
            args.RetVal = 1;
        }

        protected void OnCbxTPCTR3Toggled(object sender, EventArgs e)
        {
            if (cbxTPCTR3.Active)
            {
                cbxTPCtr2beta.Active = false;
            }
        }

        protected void OnLarTPCscCommandsSelectionChanged()
        {
            tpcCommSelecting = true;
            if (larTPCscCommands.TreeView.Selection.CountSelectedRows() == 0)
            {

                cbxTPCCommand.Sensitive = sbTPCOperand.Sensitive = false;
                cbxTPCCommand.Active = -1;
                sbTPCOperand.Value = 0;
            }
            else
            {
                cbxTPCCommand.Sensitive = true;
                var slotID = larTPCscSlots.SelectedRow;
                var cmdID = larTPCscCommands.SelectedRow;
                var cmd = tpcScript[slotID][cmdID].Item1;
                var val = tpcScript[slotID][cmdID].Item2;
                //if (correctCommand(ref cmd, val)) val -= 1000;
                cbxTPCCommand.Active = cmd;
                if (val != null) sbTPCOperand.Value = (int)val;
            }
            tpcCommSelecting = false;
        }

        private string textForScCommand(int cmd, int? val = null)
        {
            //if (correctCommand(ref cmd, val)) val -= 1000;
            var d = tpcCommands.Values.ToArray()[cmd];
            var b = d.Split('|');
            var ret = b.Last();
            if (b.Length == 3)
            {
                if (string.IsNullOrWhiteSpace(b[1])) ret = tpcCommands.Keys.ToArray()[cmd] + ": " + ret;
                else ret = string.Format(b[1], textForCmdValue(cmd, (int)val) ?? val.ToString());
            }
            return ret;
        }

        private bool tpcCommLoading = false;
        private bool tpcCommSelecting = false;

        protected void OnLarTPCscSlotsSelectionChanged()
        {
            larTPCscCommands.Store.Clear();
            var slotID = larTPCscSlots.SelectedRow;
            if (slotID != -1)
            {
                tpcCommLoading = true;
                foreach (var t in tpcScript[slotID])
                {
                    larTPCscCommands.AddRow(textForScCommand(t.Item1, t.Item2));
                }
                tpcCommLoading = false;
                larTPCscCommands.TreeView.Selection.UnselectAll();
            }
        }

        protected void OnLarTPCscCommandsRowAdded(int newID, bool dupl)
        {
            if (tpcCommLoading) return;
            var slotID = larTPCscSlots.SelectedRow;
            var tu = dupl ? tpcScript[slotID][newID - 1] : new Tuple<int, int?>(4, 0);
            tpcScript[slotID].Insert(newID, tu);
            larTPCscCommands[newID, 0] = textForScCommand(tu.Item1, tu.Item2);
        }

        protected void OnLarTPCscCommandsRowRemoved(int id)
        {
            var slotID = larTPCscSlots.SelectedRow;
            tpcScript[slotID].RemoveAt(id);
        }

        private void writeToTpcScript()
        {
            if (!cbxTPCCommand.Sensitive || tpcCommSelecting) return;
            var slotID = larTPCscSlots.SelectedRow;
            var cmdID = larTPCscCommands.SelectedRow;
            var g = Tuple.Create(cbxTPCCommand.Active, sbTPCOperand.Sensitive ? (int?)sbTPCOperand.ValueAsInt : null);
            tpcScript[slotID][cmdID] = g;
            larTPCscCommands[cmdID, 0] = textForScCommand(g.Item1, g.Item2);
        }

        protected void OnSbTPCOperandValueChanged(object sender, EventArgs e)
        {
            writeToTpcScript();
        }

        protected void OnNotebook2SwitchPage(object o, SwitchPageArgs args)
        {
            if (args.PageNum == 3)
            {
                var rs = larTPCscSlots.SelectedRow;
                var rc = larTPCscCommands.SelectedRow;
                larTPCscSlots.TreeView.Selection.UnselectAll();
                larTPCscSlots.SelectedRow = rs;
                larTPCscCommands.SelectedRow = rc;
            }
        }

        protected void OnLarTPCscCommandsRowMoved(int oldID, bool down)
        {
            var slotID = larTPCscSlots.SelectedRow;
            var old = tpcScript[slotID][oldID];
            var actID = down ? oldID + 1 : oldID - 1;
            var act = tpcScript[slotID][actID];
            tpcScript[slotID][oldID] = act;
            tpcScript[slotID][actID] = old;
            larTPCscCommands.SelectedRow = actID;
        }

        protected void OnLarTPCLevelsRowMoved(int old, bool down)
        {
            larTPCPuzzle.MoveRow(old, down);
            larTPCPickups.MoveRow(old, down);
            larTPCKeys.MoveRow(old, down);
            larTPCSecrets.MoveRow(old, down);
            larTPCSpecial.MoveRow(old, down);
            var olds = tpcScript[old + 1];
            var tmp = down ? 1 : -1;
            var news = tpcScript[old + 1 + tmp];
            tpcScript[old + 1] = news;
            tpcScript[old + 1 + tmp] = olds;
            larTPCscSlots.MoveRow(old + 1, down);

            foreach (var slot in tpcScript)
            {
                for (var i = 0; i < slot.Count; i++)
                {
                    var cmd = slot[i];
                    if (cmd.Item1 == 4)
                        if (cmd.Item2 == old)
                            slot[i] = new Tuple<int, int?>(cmd.Item1, old + tmp);
                        else if (cmd.Item2 == old + tmp)
                            slot[i] = new Tuple<int, int?>(cmd.Item1, old);
                }
            }

            foreach (var t in new[] { tpcFirstOption, tpcTitleReplace, tpcOnDeathDemoMode, tpcOnDeathInGame, tpcOnDemoInterrupt, tpcOnDemoEnd })
            {
                if (t.Command == 0)
                    if (t.UncompiledValue == old)
                        t.UncompiledValue = (uint)(old + tmp);
                    else if (t.UncompiledValue == old + tmp)
                        t.UncompiledValue = (uint)(old);
            }
        }

        public void Save()
        {
            if (tpcFilename == "")
            {
                SaveAs();
                return;
            }

            if (tpcIsTxt)
            {
                saveTpcTXT(tpcFilename, tpcStringsFilename, cbxTPCPSX.Active ? 1 : 0, cbxTPCTR3.Active ? 3 : (cbxTPCtr2beta.Active ? 2 : 1));
            }
            else
            {
                saveTpcDAT(tpcFilename, cbxTPCPSX.Active ? 1 : 0, cbxTPCTR3.Active ? 3 : (cbxTPCtr2beta.Active ? 2 : 1));
            }
        }

        private TOMBPCFile getTPC(int plat, int ver)
        {
            var f = new TOMBPCFile();

            f.IsPSX = plat == 1;
            f.Script_Version = 3;
            f.Description = tvTPCDesc.Buffer.Text;

            f.FirstOption = tpcFirstOption.Value;
            f.Title_Replace = (int)tpcTitleReplace.Value;
            f.OnDeath_Demo_Mode = tpcOnDeathDemoMode.Value;
            f.OnDeath_InGame = tpcOnDeathInGame.Value;
            f.NoInput_Time = (uint)tpcNoInputTime.Value;
            f.On_Demo_Interrupt = tpcOnDemoInterrupt.Value;
            f.On_Demo_End = tpcOnDemoEnd.Value;

            f.Title_Track = (ushort)tpcTitleSound.ValueAsInt;
            f.SingleLevel = (ushort)sbSingleLevel.ValueAsInt;
            f.Flags = (FreeRaider.Loader.TOMBPCFlags)tbeTPCFlags.Value;
            f.Cypher_Code = (byte)tbeTPCCypher.Value;
            f.Language = (TOMBPCLanguage)cbxTPCLanguage.Active;
            f.Secret_Track = (ushort)tpcSecretSound.ValueAsInt;

            f.Level_Names = larTPCLevels.GetColumn(0).TakeWhile(x => !string.IsNullOrWhiteSpace(x)).ToArray();
            f.Picture_Filenames = larTPCLevels.GetColumn(2).TakeWhile(x => !string.IsNullOrWhiteSpace(x)).ToArray();
            f.Title_Filenames = larTPClegal.GetColumn(0).TakeWhile(x => !string.IsNullOrWhiteSpace(x)).ToArray();
            f.FMV_Filenames = larTPCrpl.GetColumn(0).TakeWhile(x => !string.IsNullOrWhiteSpace(x)).ToArray();
            f.Level_Filenames = larTPCLevels.GetColumn(1).TakeWhile(x => !string.IsNullOrWhiteSpace(x)).ToArray();
            f.Cutscene_Filenames = larTPCcut.GetColumn(0).TakeWhile(x => !string.IsNullOrWhiteSpace(x)).ToArray();

            f.Script = new ushort[tpcScript.Count][];
            for (var i = 0; i < tpcScript.Count; i++)
            {
                var cur = tpcScript[i];
                var nw = new List<ushort>();
                int cmd;
                int? val;
                foreach (var t in cur)
                {
                    cmd = t.Item1;
                    val = t.Item2;
                    if (cmd == 18)
                    {
                        cmd = 17;
                        val += 1000; // BONUS -> STARTINV + 1000
                    }
                    /*if (cmd < 19) cmd++;
                    if (cmd < 10) cmd--;*/
                    if (cmd >= 9 && cmd <= 17) cmd++;
                    nw.Add((ushort)cmd);
                    if (val != null) nw.Add((ushort)(int)val);
                }
                nw.Add(9); // END
                f.Script[i] = nw.ToArray();
            }

            f.DemoLevelIDs = tpcDemoLevels.Select(x => (ushort)x).ToArray();

            f.PSXFMVInfo = tpcFmvInfo.ToArray();

            f.Game_Strings = larTPCgs.GetColumn(0).TakeWhile(x => !string.IsNullOrWhiteSpace(x)).ToArray();
            f.PC_Strings = larTPCps.GetColumn(0).TakeWhile(x => !string.IsNullOrWhiteSpace(x)).ToArray();
            f.PSX_Strings = larTPCpss.GetColumn(0).TakeWhile(x => !string.IsNullOrWhiteSpace(x)).ToArray();

            f.Puzzles = larTPCPuzzle.ToArray(1);
            f.Secrets = larTPCSecrets.ToArray(1);
            f.Special = larTPCSpecial.ToArray(1);
            f.Pickups = larTPCPickups.ToArray(1);
            f.Keys = larTPCKeys.ToArray(1);

            return f;
        }

        private void saveTpcDAT(string fn, int plat, int ver)
        {
            getTPC(plat, ver).WriteDAT(fn, plat == 1, ver == 2);
        }

        private void saveTpcTXT(string fn, string str, int plat, int ver)
        {

        }

        public void SaveAs()
        {
            var fn = Helper.getFile2(ParentWnd, "Save a script file", true, "TR2-3 script file (TOMBPC.DAT, TOMBPSX.DAT)|*.DAT", "Uncompiled TR2-3 script file (PCfinal.txt, PSXfinal.txt)|*.TXT");
			if (fn.Item1 == null) return;
			var dlg = new TPCImportDlg();
            dlg.IconList = ParentWnd.IconList;
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
				var str = Helper.getFile(ParentWnd, "Strings file", true, "Strings file (*.txt)|*.TXT");
				if(str != null)
                saveTpcTXT(fn.Item1, str, dlg.Platform, dlg.Game);
            }
        }

        protected void OnSbSingleLevelOutput(object o, OutputArgs args)
        {
            var val = sbSingleLevel.ValueAsInt;
            var ret = val + " (";
            if (val == -1) ret += "disabled, multi-level mode";
            else if (val == 0) ret += "locks the game on Title Screen, do not use this";
            else
            {
                var lvls = larTPCLevels.GetColumn(0);
                if (val < lvls.Length)
                    ret += lvls[val];
                else
                    ret += "invalid level";
            }
            ret += ")";
            sbSingleLevel.Text = ret;
            args.RetVal = 1;
        }

        public List<PSXFMVInfo> tpcFmvInfo = new List<PSXFMVInfo>();

        protected void OnSbDemoLvlValueChanged(object sender, EventArgs e)
        {
            if (tpcDemoLvlLoading) return;
            tpcDemoLevels[larTPCdemolvls.SelectedRow] = sbDemoLvl.ValueAsInt;
            larTPCdemolvls[larTPCdemolvls.SelectedRow, 0] = sbDemoLvl.ValueAsInt < larTPCLevels.Count ? larTPCLevels.GetColumn(0)[sbDemoLvl.ValueAsInt] : "invalid level";
        }

        protected void OnSbDemoLvlOutput(object o, OutputArgs args)
        {
            if (sbDemoLvl.Sensitive == false)
            {
                sbDemoLvl.Text = "";
            }
            else
            {
                var val = sbDemoLvl.ValueAsInt;
                var ret = val + " (";
                var lvls = larTPCLevels.GetColumn(0);
                if (val < lvls.Length)
                    ret += lvls[val];
                else
                    ret += "invalid level";

                ret += ")";
                sbDemoLvl.Text = ret;
            }

            args.RetVal = 1;
        }

        private bool tpcDemoLvlLoading = false;
        private bool tpcFmvInfoLoading = false;
        private bool tpcFmvInfosLoading = false;

        public event SSCHdlr SaveStateChanged = (can) => { };

        public string FileFilter => "TR2-3 script file (TOMBPC.DAT, PCfinal.txt)|*.DAT;*.TXT";

        public Window ParentWnd
        {
            get; set;
        }

        protected void OnLarTPCdemolvlsSelectionChanged()
        {
            if (larTPCdemolvls.SelectedRow == -1)
            {
                sbDemoLvl.Sensitive = false;
            }
            else
            {
                sbDemoLvl.Sensitive = true;
                tpcDemoLvlLoading = true;
                sbDemoLvl.Value = tpcDemoLevels[larTPCdemolvls.SelectedRow];
                tpcDemoLvlLoading = false;
            }
        }

        protected void OnLarTPCdemolvlsRowAdded(int newID, bool dupl)
        {
            if (tpcDemoLvlsLoading) return;
            var nval = dupl ? tpcDemoLevels[newID - 1] : 0;
            tpcDemoLevels.Insert(newID, nval);
            larTPCdemolvls[newID, 0] = nval < larTPCLevels.Count ? larTPCLevels.GetColumn(0)[nval] : "invalid level";
        }

        protected void OnLarTPCdemolvlsRowRemoved(int id)
        {
            tpcDemoLevels.RemoveAt(id);
        }

        protected void OnLarTPCdemolvlsRowMoved(int old, bool down)
        {
            var oldid = tpcDemoLevels[old];
            var npos = old + (down ? 1 : -1);
            var newid = tpcDemoLevels[npos];
            tpcDemoLevels[old] = newid;
            tpcDemoLevels[npos] = oldid;
        }

        protected void OnTpcNoInputTimeOutput(object o, OutputArgs args)
        {
            tpcNoInputTime.Text = Math.Round(tpcNoInputTime.Value / 30, 3).ToString(CultureInfo.InvariantCulture);
            args.RetVal = 1;
        }

        protected void OnTpcNoInputTimeInput(object o, InputArgs args)
        {
            args.NewValue = double.Parse(tpcNoInputTime.Text, CultureInfo.InvariantCulture) * 30;
            args.RetVal = 1;
        }

        protected void OnSbTPCFMVStartValueChanged(object sender, EventArgs e)
        {
            if (tpcFmvInfoLoading) return;
            var cur = tpcFmvInfo[larTPCrpl.SelectedRow];
            cur = new PSXFMVInfo((uint)sbTPCFMVStart.Value, cur.End);
            tpcFmvInfo[larTPCrpl.SelectedRow] = cur;
        }

        protected void OnSbTPCFMVEndValueChanged(object sender, EventArgs e)
        {
            if (tpcFmvInfoLoading) return;
            var cur = tpcFmvInfo[larTPCrpl.SelectedRow];
            cur = new PSXFMVInfo(cur.Start, (uint)sbTPCFMVEnd.Value);
            tpcFmvInfo[larTPCrpl.SelectedRow] = cur;
        }

        protected void OnLarTPCrplSelectionChanged()
        {
            if (larTPCrpl.SelectedRow == -1)
            {
                sbTPCFMVStart.Sensitive = sbTPCFMVEnd.Sensitive = false;
            }
            else
            {
                sbTPCFMVStart.Sensitive = sbTPCFMVEnd.Sensitive = true;
                tpcFmvInfoLoading = true;
                var cur = tpcFmvInfo[larTPCrpl.SelectedRow];
                sbTPCFMVStart.Value = cur.Start;
                sbTPCFMVEnd.Value = cur.End;
                tpcFmvInfoLoading = false;
            }
        }

        protected void OnLarTPCrplRowAdded(int newID, bool dupl)
        {
            if (tpcFmvInfoLoading) return;
            var nval = dupl ? tpcFmvInfo[newID - 1] : new PSXFMVInfo(0, 0);
            tpcFmvInfo.Insert(newID, nval);
        }

        protected void OnLarTPCrplRowRemoved(int id)
        {
            tpcFmvInfo.RemoveAt(id);
        }

        protected void OnLarTPCrplRowMoved(int old, bool down)
        {
            var oldid = tpcFmvInfo[old];
            var npos = old + (down ? 1 : -1);
            var newid = tpcFmvInfo[npos];
            tpcFmvInfo[old] = newid;
            tpcFmvInfo[npos] = oldid;
        }

        protected void OnLarTPCdemolvlsRowEdited(int row, int column)
        {
        }
    }
}
