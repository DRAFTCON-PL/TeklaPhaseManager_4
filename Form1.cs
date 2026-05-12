using System;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using Tekla.Structures.Model;

namespace TeklaPhaseManager_4
{
    public partial class Form1 : Form
    {
        // ── UI palette (Windows light) ───────────────────────────────────────
        private static readonly Color DkFormBg   = Color.FromArgb(243, 243, 243);
        private static readonly Color DkListBg   = Color.FromArgb(255, 255, 255);
        private static readonly Color DkListAlt  = Color.FromArgb(248, 248, 248);
        private static readonly Color DkListHdr  = Color.FromArgb(235, 235, 235);
        private static readonly Color DkListSel  = Color.FromArgb(204, 228, 247);
        private static readonly Color DkGrid     = Color.FromArgb(220, 220, 220);
        private static readonly Color DkText     = Color.FromArgb(15,  15,  15);
        private static readonly Color DkTextDim  = Color.FromArgb(110, 110, 110);
        private static readonly Color DkBtnBg    = Color.FromArgb(225, 225, 225);
        private static readonly Color DkBtnAccent= Color.FromArgb(0,   120, 212);
        private static readonly Color DkBorder   = Color.FromArgb(180, 180, 180);
        private static readonly Color DkStatusBg = Color.FromArgb(235, 235, 235);
        private static readonly Color DkCtxBg    = Color.FromArgb(255, 255, 255);

        // ── Tekla colour palette ────────────────────────────────────────────
        private readonly Color[] _teklaColors = new Color[]
        {
            Color.WhiteSmoke, Color.Gray, Color.Red, Color.LimeGreen, Color.Cyan,
            Color.Blue, Color.Yellow, Color.Magenta, Color.SaddleBrown, Color.DarkGreen,
            Color.DarkBlue, Color.DarkSlateGray, Color.Orange, Color.Silver, Color.Firebrick
        };

        // ── Transparency labels / mapping ───────────────────────────────────
        private static readonly string[] _transLabels = { "as is", "visible", "50%", "70%", "90%", "hidden" };
        private static readonly Dictionary<string, int> _transLabelToRepVal = new Dictionary<string, int>
        {
            { "as is", 10 }, { "visible", 10 }, { "50%", 5 }, { "70%", 3 }, { "90%", 1 }, { "hidden", 0 }
        };

        // ── State ───────────────────────────────────────────────────────────
        private Model    _model;
        private TextBox  _cellEditor;
        private ListViewItem _editingItem;
        private int      _editingColumn              = -1;
        private int      _editingOriginalPhaseNumber = -1;

        private readonly List<ListViewItem> _allItems = new List<ListViewItem>();
        private const    string FilterPlaceholder    = "Search phases...";
        private          string _filterText          = "";

        private int       _sortColumn = -1;
        private SortOrder _sortOrder  = SortOrder.None;

        private int    _activePhaseNumber = -1;
        private string _currentPresetPath = "";

        private System.Windows.Forms.Timer _phaseTimer;

        // ── Constructor ─────────────────────────────────────────────────────
        public Form1()
        {
            InitializeComponent();

            this.TopMost = true;
            UpdatePinButtonAppearance();
            _model = new Model();

            listViewPhases.OwnerDraw = true;
            listViewPhases.DrawColumnHeader += listViewPhases_DrawColumnHeader;
            listViewPhases.DrawItem         += listViewPhases_DrawItem;
            listViewPhases.DrawSubItem      += listViewPhases_DrawSubItem;

            _cellEditor = new TextBox { Visible = false, BorderStyle = BorderStyle.FixedSingle };
            _cellEditor.KeyDown   += CellEditor_KeyDown;
            _cellEditor.LostFocus += CellEditor_LostFocus;
            listViewPhases.Controls.Add(_cellEditor);

            SetFilterPlaceholder();
            txtFilter.GotFocus  += (s, e) => { if (txtFilter.Text == FilterPlaceholder) { txtFilter.Text = ""; txtFilter.ForeColor = SystemColors.WindowText; } };
            txtFilter.LostFocus += (s, e) => { if (string.IsNullOrEmpty(txtFilter.Text)) SetFilterPlaceholder(); };

            _phaseTimer = new System.Windows.Forms.Timer { Interval = 2000 };
            _phaseTimer.Tick += PhaseTimer_Tick;
            _phaseTimer.Start();

            ApplyDarkTheme();
            UpdateStatus();
        }

        // ── Dark theme ───────────────────────────────────────────────────────
        private void ApplyDarkTheme()
        {
            this.BackColor = DkFormBg;

            foreach (var btn in new[] { btnLoad, btnPin, btnAllVisible, btnAllHidden, button1, btnSavePreset, btnLoadPreset })
            {
                btn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
                btn.FlatAppearance.BorderColor = DkBorder;
                btn.FlatAppearance.BorderSize  = 1;
                btn.BackColor = DkBtnBg;
                btn.ForeColor = DkText;
                btn.Cursor    = Cursors.Hand;
            }
            btnLoad.BackColor = DkBtnAccent;
            btnLoad.ForeColor = Color.White;
            btnLoad.FlatAppearance.BorderColor = Color.FromArgb(0, 90, 170);

            txtFilter.BackColor    = Color.White;
            txtFilter.ForeColor    = DkText;
            txtFilter.BorderStyle  = BorderStyle.FixedSingle;

            txtPresetFile.BackColor   = Color.White;
            txtPresetFile.ForeColor   = DkText;
            txtPresetFile.BorderStyle = BorderStyle.FixedSingle;

            _cellEditor.BackColor = Color.White;
            _cellEditor.ForeColor = DkText;

            listViewPhases.BackColor = DkListBg;
            listViewPhases.ForeColor = DkText;

            statusStrip1.BackColor = DkStatusBg;
            statusStrip1.Renderer  = new DarkStripRenderer();
            statusLabelModel.ForeColor = DkTextDim;
            statusLabelModel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            statusLabelBrand.ForeColor = Color.FromArgb(0, 80, 160);
            statusLabelBrand.Font      = new Font(statusStrip1.Font, FontStyle.Bold);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _phaseTimer?.Stop();
            _phaseTimer?.Dispose();
            base.OnFormClosed(e);
        }

        private void PhaseTimer_Tick(object sender, EventArgs e)
        {
            if (_model == null || !_model.GetConnectionStatus()) return;
            try
            {
                int cp = _model.GetInfo().CurrentPhase;
                if (cp != _activePhaseNumber)
                {
                    _activePhaseNumber = cp;
                    listViewPhases.Invalidate();
                }
            }
            catch { }
        }

        private void SetFilterPlaceholder()
        {
            txtFilter.Text      = FilterPlaceholder;
            txtFilter.ForeColor = SystemColors.GrayText;
        }

        // ── Status bar ──────────────────────────────────────────────────────
        private void UpdateStatus()
        {
            bool ok = _model != null && _model.GetConnectionStatus();
            statusLabelConnection.ForeColor = ok ? Color.FromArgb(16, 124, 16) : Color.FromArgb(196, 43, 28);
            statusLabelConnection.Text      = ok ? "● Connected" : "● Disconnected";
            statusLabelModel.Text           = ok ? _model.GetInfo().ModelName : "";
        }

        // ── Load phases ─────────────────────────────────────────────────────
        private void btnLoad_Click(object sender, EventArgs e)
        {
            if (_model == null || !_model.GetConnectionStatus()) _model = new Model();
            if (!_model.GetConnectionStatus())
            {
                MessageBox.Show("No connection to Tekla Structures.", "TPM", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                UpdateStatus();
                return;
            }

            _allItems.Clear();
            listViewPhases.Items.Clear();

            PhaseCollection phases           = _model.GetPhases();
            var existingColors               = ReadExistingPhaseColors();
            var existingVisibility           = ReadExistingVisibility();
            var existingTransparency         = ReadExistingPhaseTransparency();

            var phaseList = new List<Phase>();
            foreach (Phase ph in phases) phaseList.Add(ph);
            phaseList = phaseList.OrderBy(p => p.PhaseNumber).ToList();

            foreach (Phase phase in phaseList)
            {
                ListViewItem item = new ListViewItem(phase.PhaseNumber.ToString());
                item.SubItems.Add(phase.PhaseName    ?? "");
                item.SubItems.Add(phase.PhaseComment ?? "");

                if (existingColors.ContainsKey(phase.PhaseNumber))
                {
                    string colorId = existingColors[phase.PhaseNumber];
                    item.SubItems.Add(colorId);
                    if (int.TryParse(colorId, out int cId) && cId >= 0 && cId < _teklaColors.Length)
                    {
                        item.UseItemStyleForSubItems = false;
                        item.SubItems[3].BackColor   = _teklaColors[cId];
                        item.SubItems[3].ForeColor   = (cId == 0 || cId == 6 || cId == 13) ? Color.Black : Color.White;
                    }
                    else if (colorId.StartsWith("#") && TryParseStoredColor(colorId, out Color cc))
                    {
                        item.UseItemStyleForSubItems = false;
                        item.SubItems[3].BackColor   = cc;
                        item.SubItems[3].ForeColor   = ((cc.R * 0.299) + (cc.G * 0.587) + (cc.B * 0.114) > 160) ? Color.Black : Color.White;
                    }
                }
                else item.SubItems.Add("");

                bool isVis = existingVisibility.ContainsKey(phase.PhaseNumber) && existingVisibility[phase.PhaseNumber];
                item.SubItems.Add(isVis ? "1" : "0");

                string trans = existingTransparency.ContainsKey(phase.PhaseNumber) ? existingTransparency[phase.PhaseNumber] : "as is";
                item.SubItems.Add(trans);

                item.SubItems.Add("-");   // col 6: object count loaded async

                _allItems.Add(item);
            }

            ApplyFilter();
            UpdateStatus();
            LoadObjectCounts();
            try { _activePhaseNumber = _model.GetInfo().CurrentPhase; } catch { }
            listViewPhases.Invalidate();
        }

        // ── Filter ──────────────────────────────────────────────────────────
        private void txtFilter_TextChanged(object sender, EventArgs e)
        {
            _filterText = (txtFilter.Text == FilterPlaceholder) ? "" : txtFilter.Text.Trim();
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            listViewPhases.BeginUpdate();
            listViewPhases.Items.Clear();
            foreach (ListViewItem item in _allItems)
            {
                if (string.IsNullOrEmpty(_filterText) ||
                    item.Text.IndexOf(_filterText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (item.SubItems.Count > 1 && item.SubItems[1].Text.IndexOf(_filterText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (item.SubItems.Count > 2 && item.SubItems[2].Text.IndexOf(_filterText, StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    listViewPhases.Items.Add(item);
                }
            }
            listViewPhases.EndUpdate();
        }

        // ── Column sort ─────────────────────────────────────────────────────
        private void listViewPhases_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column == _sortColumn)
                _sortOrder = _sortOrder == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
            else { _sortColumn = e.Column; _sortOrder = SortOrder.Ascending; }

            listViewPhases.ListViewItemSorter = new ListViewItemComparer(_sortColumn, _sortOrder);
            listViewPhases.Sort();
        }

        private class ListViewItemComparer : System.Collections.IComparer
        {
            private readonly int _col;
            private readonly SortOrder _order;
            public ListViewItemComparer(int col, SortOrder order) { _col = col; _order = order; }
            public int Compare(object x, object y)
            {
                var a = (ListViewItem)x;
                var b = (ListViewItem)y;
                string sa = _col == 0 ? a.Text : (a.SubItems.Count > _col ? a.SubItems[_col].Text : "");
                string sb = _col == 0 ? b.Text : (b.SubItems.Count > _col ? b.SubItems[_col].Text : "");
                if ((_col == 0 || _col == 6) && int.TryParse(sa, out int ia) && int.TryParse(sb, out int ib))
                    return _order == SortOrder.Ascending ? ia.CompareTo(ib) : ib.CompareTo(ia);
                int cmp = string.Compare(sa, sb, StringComparison.CurrentCultureIgnoreCase);
                return _order == SortOrder.Ascending ? cmp : -cmp;
            }
        }

        // ── Object count — STA thread, own Model instance ───────────────────
        private void LoadObjectCounts()
        {
            foreach (ListViewItem it in _allItems)
                if (it.SubItems.Count > 6) it.SubItems[6].Text = "...";
            listViewPhases.Invalidate();

            Thread t = new Thread(() =>
            {
                var counts = new Dictionary<int, int>();
                bool success = false;
                try
                {
                    Model m = new Model();
                    if (m.GetConnectionStatus())
                    {
                        var objs = m.GetModelObjectSelector()
                                    .GetAllObjects();
                        while (objs.MoveNext())
                        {
                            if (objs.Current is Part p)
                            {
                                p.GetPhase(out Phase ph);
                                if (!counts.ContainsKey(ph.PhaseNumber)) counts[ph.PhaseNumber] = 0;
                                counts[ph.PhaseNumber]++;
                            }
                        }
                        success = true;
                    }
                }
                catch { }

                if (IsDisposed) return;
                try
                {
                    Invoke((Action)(() =>
                    {
                        foreach (ListViewItem it in _allItems)
                        {
                            if (int.TryParse(it.Text, out int n) && it.SubItems.Count > 6)
                                it.SubItems[6].Text = success
                                    ? (counts.ContainsKey(n) ? counts[n].ToString() : "0")
                                    : "?";
                        }
                        listViewPhases.Invalidate();
                    }));
                }
                catch { }
            });
            t.SetApartmentState(ApartmentState.STA);
            t.IsBackground = true;
            t.Start();
        }

        // ── All visible / hidden ─────────────────────────────────────────────
        private void btnAllVisible_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in _allItems)
                if (item.SubItems.Count > 4) item.SubItems[4].Text = "1";
            listViewPhases.Invalidate();
            SaveFiles();
        }

        private void btnAllHidden_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in _allItems)
                if (item.SubItems.Count > 4) item.SubItems[4].Text = "0";
            listViewPhases.Invalidate();
            SaveFiles();
        }

        // ── Presets ─────────────────────────────────────────────────────────
        private void btnSavePreset_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.Title      = "Save Preset";
                dlg.Filter     = "TPM Preset (*.tpm)|*.tpm|All files (*.*)|*.*";
                dlg.DefaultExt = "tpm";

                string dir = !string.IsNullOrEmpty(_currentPresetPath)
                    ? Path.GetDirectoryName(_currentPresetPath)
                    : (_model != null && _model.GetConnectionStatus() ? Path.Combine(_model.GetInfo().ModelPath, "attributes") : "");
                if (!string.IsNullOrEmpty(dir)) dlg.InitialDirectory = dir;

                string edited = txtPresetFile.Text.Trim();
                if (!string.IsNullOrEmpty(edited))
                    dlg.FileName = edited.EndsWith(".tpm", StringComparison.OrdinalIgnoreCase) ? edited : edited + ".tpm";

                if (dlg.ShowDialog() != DialogResult.OK) return;
                _currentPresetPath = dlg.FileName;
                txtPresetFile.Text = Path.GetFileNameWithoutExtension(dlg.FileName);

                var sb = new StringBuilder();
                sb.AppendLine("# TeklaPhaseManager_4 Preset");
                sb.AppendLine("# No.;Color;Visible;Transparency");
                foreach (ListViewItem item in _allItems)
                {
                    string color = item.SubItems.Count > 3 ? item.SubItems[3].Text : "";
                    string vis   = item.SubItems.Count > 4 ? item.SubItems[4].Text : "0";
                    string trans = item.SubItems.Count > 5 ? item.SubItems[5].Text : "as is";
                    sb.AppendLine($"{item.Text};{color};{vis};{trans}");
                }
                File.WriteAllText(dlg.FileName, sb.ToString(), Encoding.UTF8);
            }
        }

        private void btnLoadPreset_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title  = "Load Preset";
                dlg.Filter = "TPM Preset (*.tpm)|*.tpm|All files (*.*)|*.*";
                if (_model != null && _model.GetConnectionStatus())
                    dlg.InitialDirectory = Path.Combine(_model.GetInfo().ModelPath, "attributes");
                if (dlg.ShowDialog() != DialogResult.OK) return;
                _currentPresetPath = dlg.FileName;
                txtPresetFile.Text = Path.GetFileNameWithoutExtension(dlg.FileName);

                var presetData = new Dictionary<int, (string color, string vis, string trans)>();
                foreach (string line in File.ReadAllLines(dlg.FileName, Encoding.UTF8))
                {
                    if (line.StartsWith("#") || string.IsNullOrWhiteSpace(line)) continue;
                    string[] parts = line.Split(';');
                    if (parts.Length >= 4 && int.TryParse(parts[0], out int num))
                        presetData[num] = (parts[1], parts[2], parts[3].Trim());
                }

                foreach (ListViewItem item in _allItems)
                {
                    if (!int.TryParse(item.Text, out int num) || !presetData.ContainsKey(num)) continue;
                    var pd = presetData[num];
                    ApplyColorToItemSingle(item, string.IsNullOrEmpty(pd.color) ? "brak" : pd.color);
                    if (item.SubItems.Count > 4) item.SubItems[4].Text = pd.vis;
                    if (item.SubItems.Count > 5) item.SubItems[5].Text = _transLabels.Contains(pd.trans) ? pd.trans : "as is";
                }
                listViewPhases.Invalidate();
                SaveFiles();
            }
        }

        // ── Click handling ───────────────────────────────────────────────────
        private void listViewPhases_MouseClick(object sender, MouseEventArgs e)
        {
            ListViewItem item = listViewPhases.GetItemAt(e.X, e.Y);
            if (item == null) return;

            if (e.Button == MouseButtons.Right) { ShowPhaseContextMenu(item, e.X, e.Y); return; }

            if (_cellEditor.Visible) CommitCellEdit();

            int col = GetClickedColumnIndex(e.X);

            if (col == 1 || col == 2) { BeginCellEdit(item, col); return; }
            if (col == 3)             { ShowColorContextMenu(item, e.X, e.Y); return; }

            if (col == 4 && item.SubItems.Count > 4)
            {
                bool cur = IsVisibleCellValue(item.SubItems[4].Text);
                item.SubItems[4].Text = cur ? "0" : "1";
                listViewPhases.Invalidate();
                SaveFiles();
                return;
            }

            if (col == 5) ShowTransparencyContextMenu(item, e.X, e.Y);
        }

        private bool IsVisibleCellValue(string v) => v == "1" || v == "☑";

        // ── Transparency context menu ────────────────────────────────────────
        private void ShowTransparencyContextMenu(ListViewItem item, int x, int y)
        {
            string current = item.SubItems.Count > 5 ? item.SubItems[5].Text : "as is";
            ContextMenuStrip menu = new ContextMenuStrip();
            foreach (string label in _transLabels)
            {
                string lbl = label;
                ToolStripMenuItem mi = new ToolStripMenuItem(lbl) { Checked = (current == lbl) };
                mi.Click += (s, ev) => ApplyTransparencyToItem(item, lbl);
                menu.Items.Add(mi);
            }
            ApplyDarkContextMenu(menu);
            menu.Show(listViewPhases, x, y);
        }

        private void ApplyTransparencyToItem(ListViewItem item, string label)
        {
            if (item.SubItems.Count > 5) item.SubItems[5].Text = label;
            SaveFiles();
        }

        // ── Colour context menu ──────────────────────────────────────────────
        private void ShowColorContextMenu(ListViewItem item, int x, int y)
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.ShowImageMargin = true;

            ToolStripMenuItem noColor = new ToolStripMenuItem("No color");
            noColor.Click += (s, e) => ApplyColorToItem(item, "brak");
            menu.Items.Add(noColor);
            menu.Items.Add(new ToolStripSeparator());

            for (int i = 0; i < _teklaColors.Length; i++)
            {
                int idx = i;
                ToolStripMenuItem mi = new ToolStripMenuItem($"Color {idx}");
                mi.Image  = CreateColorSwatchImage(_teklaColors[idx]);
                mi.Click += (s, e) => ApplyColorToItem(item, idx.ToString());
                menu.Items.Add(mi);
            }

            menu.Items.Add(new ToolStripSeparator());
            ToolStripMenuItem custom = new ToolStripMenuItem("Custom...");
            custom.Click += (s, e) =>
            {
                using (ColorDialog cd = new ColorDialog { AnyColor = true, FullOpen = true })
                    if (cd.ShowDialog(this) == DialogResult.OK)
                        ApplyColorToItem(item, EncodeColorToken(cd.Color));
            };
            menu.Items.Add(custom);
            ApplyDarkContextMenu(menu);
            menu.Show(listViewPhases, x, y);
        }

        private void ApplyColorToItem(ListViewItem item, string colorId)
        {
            ApplyColorToItemSingle(item, colorId);
            SaveFiles();
        }

        private void ApplyColorToItemSingle(ListViewItem item, string colorId)
        {
            if (item.SubItems.Count <= 3) return;
            if (colorId == "brak")
            {
                item.SubItems[3].Text      = "";
                item.SubItems[3].BackColor = SystemColors.Window;
                item.SubItems[3].ForeColor = SystemColors.WindowText;
            }
            else
            {
                item.SubItems[3].Text = colorId;
                if (TryParseStoredColor(colorId, out Color c))
                {
                    item.UseItemStyleForSubItems = false;
                    item.SubItems[3].BackColor   = c;
                    item.SubItems[3].ForeColor   = ((c.R * 0.299) + (c.G * 0.587) + (c.B * 0.114) > 160) ? Color.Black : Color.White;
                }
            }
        }

        private Bitmap CreateColorSwatchImage(Color color)
        {
            Bitmap bmp = new Bitmap(16, 16);
            using (Graphics g = Graphics.FromImage(bmp))
            using (SolidBrush b = new SolidBrush(color))
            using (Pen p = new Pen(Color.Black, 1f))
            {
                g.FillRectangle(b, 0, 0, 15, 15);
                g.DrawRectangle(p, 0, 0, 15, 15);
            }
            return bmp;
        }

        // ── PPM context menu ─────────────────────────────────────────────────
        private void ShowPhaseContextMenu(ListViewItem clickedItem, int x, int y)
        {
            var selected = listViewPhases.SelectedItems.Cast<ListViewItem>().ToList();
            if (!selected.Contains(clickedItem)) { selected.Clear(); selected.Add(clickedItem); }
            if (selected.Count == 0) return;

            ContextMenuStrip menu = new ContextMenuStrip();

            // ── Set current phase (single only) ────────────────────────────
            if (selected.Count == 1 && int.TryParse(selected[0].Text, out int singleNum))
            {
                bool isActive = singleNum == _activePhaseNumber;
                var miActive = new ToolStripMenuItem(isActive ? "✔ Current Phase" : "Set as Current Phase");
                if (isActive) miActive.Enabled = false;
                else miActive.Click += (s, ev) =>
                {
                    _activePhaseNumber = singleNum;
                    listViewPhases.Invalidate();
                    SetActivePhaseApi(singleNum);
                };
                menu.Items.Add(miActive);
                menu.Items.Add(new ToolStripSeparator());
            }

            // ── Visibility ─────────────────────────────────────────────────
            var miVis = new ToolStripMenuItem("Visible");
            miVis.Click += (s, ev) => { foreach (var it in selected) if (it.SubItems.Count > 4) it.SubItems[4].Text = "1"; listViewPhases.Invalidate(); SaveFiles(); };
            menu.Items.Add(miVis);

            var miHid = new ToolStripMenuItem("Hidden");
            miHid.Click += (s, ev) => { foreach (var it in selected) if (it.SubItems.Count > 4) it.SubItems[4].Text = "0"; listViewPhases.Invalidate(); SaveFiles(); };
            menu.Items.Add(miHid);

            // ── Transparency ▶ ─────────────────────────────────────────────
            var miTrans = new ToolStripMenuItem("Transparency");
            foreach (string lbl in _transLabels)
            {
                string label = lbl;
                var mi = new ToolStripMenuItem(label);
                mi.Click += (s, ev) => { foreach (var it in selected) if (it.SubItems.Count > 5) it.SubItems[5].Text = label; listViewPhases.Invalidate(); SaveFiles(); };
                miTrans.DropDownItems.Add(mi);
            }
            menu.Items.Add(miTrans);

            // ── Color ▶ ────────────────────────────────────────────────────
            var miColorMenu = new ToolStripMenuItem("Color");
            var miNoColor = new ToolStripMenuItem("No color");
            miNoColor.Click += (s, ev) => { foreach (var it in selected) ApplyColorToItemSingle(it, "brak"); listViewPhases.Invalidate(); SaveFiles(); };
            miColorMenu.DropDownItems.Add(miNoColor);
            miColorMenu.DropDownItems.Add(new ToolStripSeparator());
            for (int i = 0; i < _teklaColors.Length; i++)
            {
                int idx = i;
                var mi = new ToolStripMenuItem($"Color {idx}") { Image = CreateColorSwatchImage(_teklaColors[idx]) };
                mi.Click += (s, ev) => { foreach (var it in selected) ApplyColorToItemSingle(it, idx.ToString()); listViewPhases.Invalidate(); SaveFiles(); };
                miColorMenu.DropDownItems.Add(mi);
            }
            miColorMenu.DropDownItems.Add(new ToolStripSeparator());
            var miCustomColor = new ToolStripMenuItem("Custom...");
            miCustomColor.Click += (s, ev) =>
            {
                using (ColorDialog cd = new ColorDialog { AnyColor = true, FullOpen = true })
                    if (cd.ShowDialog(this) == DialogResult.OK)
                    {
                        string tok = EncodeColorToken(cd.Color);
                        foreach (var it in selected) ApplyColorToItemSingle(it, tok);
                        listViewPhases.Invalidate();
                        SaveFiles();
                    }
            };
            miColorMenu.DropDownItems.Add(miCustomColor);
            menu.Items.Add(miColorMenu);

            menu.Items.Add(new ToolStripSeparator());

            // ── Select objects in model ────────────────────────────────────
            var miSelect = new ToolStripMenuItem(selected.Count == 1 ? "Select phase objects" : $"Select objects ({selected.Count} phases)");
            miSelect.Click += (s, ev) => SelectObjectsInPhases(selected);
            menu.Items.Add(miSelect);

            menu.Items.Add(new ToolStripSeparator());
            var miAbout = new ToolStripMenuItem("ℹ  About / Macro info...");
            miAbout.Click += (s, ev) => ShowAboutDialog();
            menu.Items.Add(miAbout);

            ApplyDarkContextMenu(menu);
            menu.Show(listViewPhases, x, y);
        }

        private void ShowAboutDialog()
        {
            string msg =
                "TeklaPhaseManager TPM  —  2026.05  —  Freeware\r\n" +
                "Author: DRAFTCON.PL\r\n" +
                "https://github.com/DRAFTCON-PL/TeklaPhaseManager_4\r\n" +
                "\r\n" +
                "─────────────────────────────────────────────────\r\n" +
                "REQUIRED: Tekla macro  +TeklaRedrawView.cs\r\n" +
                "─────────────────────────────────────────────────\r\n" +
                "This macro is needed for Apply / Save to refresh\r\n" +
                "the view representation and visibility filter.\r\n" +
                "\r\n" +
                "The macro is AUTO-GENERATED by TPM into the model\r\n" +
                "folder on first Save/Apply — no manual install needed.\r\n" +
                "\r\n" +
                "If the view does not refresh, manually copy the macro\r\n" +
                "from the model folder to your Tekla macros directory,\r\n" +
                "or download it from GitHub:\r\n" +
                "https://github.com/DRAFTCON-PL/TeklaPhaseManager";
            MessageBox.Show(msg, "About TeklaPhaseManager", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void SetActivePhaseApi(int phaseNumber)
        {
            try
            {
                if (_model == null || !_model.GetConnectionStatus()) return;
                PhaseCollection phases = _model.GetPhases();
                foreach (Phase phase in phases)
                {
                    if (phase.PhaseNumber == phaseNumber)
                    {
                        phase.IsCurrentPhase = 1;
                        phase.Modify();
                        _model.CommitChanges();
                        return;
                    }
                }
            }
            catch { }
        }

        private void SelectObjectsInPhases(List<ListViewItem> items)
        {
            var phaseNums = new HashSet<int>();
            foreach (var it in items)
                if (int.TryParse(it.Text, out int n)) phaseNums.Add(n);
            if (phaseNums.Count == 0) return;

            statusLabelConnection.Text      = "⟳ Selecting objects...";
            statusLabelConnection.ForeColor = Color.FromArgb(200, 160, 50);

            Thread t = new Thread(() =>
            {
                try
                {
                    Model m = new Model();
                    if (!m.GetConnectionStatus()) { Invoke((Action)UpdateStatus); return; }
                    var objs   = m.GetModelObjectSelector().GetAllObjects();
                    var result = new System.Collections.ArrayList();
                    while (objs.MoveNext())
                    {
                        if (objs.Current is Part p)
                        {
                            p.GetPhase(out Phase ph);
                            if (phaseNums.Contains(ph.PhaseNumber)) result.Add(p);
                        }
                    }
                    new Tekla.Structures.Model.UI.ModelObjectSelector().Select(result);
                }
                catch { }
                finally
                {
                    if (!IsDisposed)
                        try { Invoke((Action)UpdateStatus); } catch { }
                }
            });
            t.SetApartmentState(ApartmentState.STA);
            t.IsBackground = true;
            t.Start();
        }

        // ── OwnerDraw ────────────────────────────────────────────────────────
        private void listViewPhases_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            using (SolidBrush b = new SolidBrush(DkListHdr))
                e.Graphics.FillRectangle(b, e.Bounds);

            using (Pen p = new Pen(DkGrid))
            {
                e.Graphics.DrawLine(p, e.Bounds.Right - 1, e.Bounds.Top, e.Bounds.Right - 1, e.Bounds.Bottom);
                e.Graphics.DrawLine(p, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
            }

            string arrow = e.ColumnIndex == _sortColumn
                ? (_sortOrder == SortOrder.Ascending ? " ▲" : " ▼") : "";

            using (Font hf = new Font(listViewPhases.Font, FontStyle.Bold))
                TextRenderer.DrawText(e.Graphics, e.Header.Text + arrow, hf, e.Bounds,
                    DkText, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
        }

        private void listViewPhases_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            // background handled per-subitem; suppress default
        }

        private void listViewPhases_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            bool sel    = e.Item.Selected;
            bool alt    = e.ItemIndex % 2 == 1;
            Color rowBg = sel ? DkListSel : (alt ? DkListAlt : DkListBg);
            Color rowFg = DkText;

            // ── Col 0: phase number + active-phase highlight ─────────────
            if (e.ColumnIndex == 0)
            {
                if (_activePhaseNumber >= 0 && int.TryParse(e.SubItem.Text, out int pn) && pn == _activePhaseNumber)
                {
                    Color abg = sel ? Color.FromArgb(255, 220, 100) : Color.FromArgb(255, 243, 188);
                    Color afg = Color.FromArgb(100, 60, 0);
                    using (SolidBrush b = new SolidBrush(abg)) e.Graphics.FillRectangle(b, e.Bounds);
                    using (Font bold = new Font(listViewPhases.Font, FontStyle.Bold))
                        TextRenderer.DrawText(e.Graphics, e.SubItem.Text, bold, e.Bounds, afg,
                            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
                    DrawCellBorder(e.Graphics, e.Bounds);
                    return;
                }
                using (SolidBrush b = new SolidBrush(rowBg)) e.Graphics.FillRectangle(b, e.Bounds);
                TextRenderer.DrawText(e.Graphics, e.SubItem.Text, listViewPhases.Font, e.Bounds, rowFg,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
                DrawCellBorder(e.Graphics, e.Bounds);
                return;
            }

            // ── Col 3: colour swatch ─────────────────────────────────────
            if (e.ColumnIndex == 3)
            {
                Color swatchBg = (e.SubItem.BackColor != Color.Empty && e.SubItem.BackColor != SystemColors.Window)
                    ? e.SubItem.BackColor : rowBg;
                using (SolidBrush b = new SolidBrush(swatchBg)) e.Graphics.FillRectangle(b, e.Bounds);

                string txt = e.SubItem.Text ?? "";
                if (txt.StartsWith("#")) txt = "Custom";
                Color swatchFg = (e.SubItem.ForeColor != Color.Empty && e.SubItem.ForeColor != SystemColors.WindowText)
                    ? e.SubItem.ForeColor : rowFg;
                TextRenderer.DrawText(e.Graphics, txt, listViewPhases.Font, e.Bounds, swatchFg,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.LeftAndRightPadding);
                DrawCellBorder(e.Graphics, e.Bounds);
                return;
            }

            // ── Col 4: eye icon ───────────────────────────────────────────
            if (e.ColumnIndex == 4)
            {
                using (SolidBrush b = new SolidBrush(rowBg)) e.Graphics.FillRectangle(b, e.Bounds);
                DrawVisibilityIcon(e.Graphics, e.Bounds, IsVisibleCellValue(e.SubItem.Text));
                DrawCellBorder(e.Graphics, e.Bounds);
                return;
            }

            // ── All other columns ─────────────────────────────────────────
            using (SolidBrush b = new SolidBrush(rowBg)) e.Graphics.FillRectangle(b, e.Bounds);
            var pad = Rectangle.Inflate(e.Bounds, -3, 0);
            TextRenderer.DrawText(e.Graphics, e.SubItem.Text ?? "", listViewPhases.Font, pad, rowFg,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
            DrawCellBorder(e.Graphics, e.Bounds);
        }

        private void DrawVisibilityIcon(Graphics g, Rectangle cell, bool visible)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            const int iw = 18, ih = 10;
            float cx = cell.X + cell.Width  / 2f;
            float cy = cell.Y + cell.Height / 2f;
            float lx = cx - iw / 2f;
            float ty = cy - ih / 2f;
            float rx = lx + iw;
            float by = ty + ih;

            // almond shape: left-tip → top bezier → right-tip → bottom bezier
            using (var eye = new System.Drawing.Drawing2D.GraphicsPath())
            {
                eye.AddBezier(lx, cy,  lx + iw * 0.28f, ty,  rx - iw * 0.28f, ty,  rx, cy);
                eye.AddBezier(rx, cy,  rx - iw * 0.28f, by,  lx + iw * 0.28f, by,  lx, cy);
                eye.CloseFigure();

                if (visible)
                {
                    // white sclera
                    using (SolidBrush fill = new SolidBrush(Color.White))
                        g.FillPath(fill, eye);
                    // dark iris
                    float ir = ih * 0.32f;
                    using (SolidBrush iris = new SolidBrush(Color.FromArgb(50, 50, 50)))
                        g.FillEllipse(iris, cx - ir, cy - ir, ir * 2, ir * 2);
                    // dark outline
                    using (Pen pen = new Pen(Color.FromArgb(50, 50, 50), 1.2f))
                        g.DrawPath(pen, eye);
                }
                else
                {
                    // light gray sclera
                    using (SolidBrush fill = new SolidBrush(Color.FromArgb(205, 205, 205)))
                        g.FillPath(fill, eye);
                    // gray outline
                    using (Pen pen = new Pen(Color.FromArgb(140, 140, 140), 1.2f))
                        g.DrawPath(pen, eye);
                    // diagonal red slash clipped to eye shape
                    var state = g.Save();
                    g.SetClip(eye);
                    using (Pen slash = new Pen(Color.FromArgb(185, 45, 45), 1.8f))
                        g.DrawLine(slash, lx + 2, by - 1, rx - 2, ty + 1);
                    g.Restore(state);
                }
            }

            g.SmoothingMode = SmoothingMode.Default;
        }

        private void DrawCellBorder(Graphics g, Rectangle r)
        {
            using (Pen p = new Pen(DkGrid))
            {
                g.DrawLine(p, r.Right - 1, r.Top, r.Right - 1, r.Bottom - 1);
                g.DrawLine(p, r.Left, r.Bottom - 1, r.Right, r.Bottom - 1);
            }
        }

        // ── Inline cell editor ───────────────────────────────────────────────
        private void BeginCellEdit(ListViewItem item, int column)
        {
            if (item == null || column < 0 || column >= item.SubItems.Count) return;
            _editingItem   = item;
            _editingColumn = column;
            int.TryParse(item.Text, out _editingOriginalPhaseNumber);
            Rectangle b = item.SubItems[column].Bounds;
            _cellEditor.Bounds  = new Rectangle(b.X + 1, b.Y + 1, b.Width - 2, b.Height - 2);
            _cellEditor.Text    = item.SubItems[column].Text;
            _cellEditor.Visible = true;
            _cellEditor.Focus();
            _cellEditor.SelectAll();
        }

        private void CellEditor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)       { CommitCellEdit(); e.SuppressKeyPress = true; }
            else if (e.KeyCode == Keys.Escape) { CancelCellEdit(); e.SuppressKeyPress = true; }
        }

        private void CellEditor_LostFocus(object sender, EventArgs e)
        {
            if (_cellEditor.Visible) CommitCellEdit();
        }

        private void CommitCellEdit()
        {
            if (_editingItem == null || _editingColumn < 0) { CancelCellEdit(); return; }

            string newVal  = _cellEditor.Text ?? string.Empty;
            string prevVal = _editingColumn == 0 ? _editingItem.Text : _editingItem.SubItems[_editingColumn].Text;

            if (_editingColumn == 0)
            {
                if (!int.TryParse(newVal, out int n) || n <= 0)
                {
                    MessageBox.Show("Phase number must be a positive integer.");
                    CancelCellEdit(); return;
                }
                _editingItem.Text = newVal;
            }
            else
            {
                _editingItem.SubItems[_editingColumn].Text = newVal;
            }

            if (_editingColumn == 0 || _editingColumn == 1 || _editingColumn == 2)
            {
                bool ok = TryUpdatePhaseFromRow(_editingItem, _editingOriginalPhaseNumber);
                if (!ok)
                {
                    if (_editingColumn == 0) _editingItem.Text = prevVal;
                    else _editingItem.SubItems[_editingColumn].Text = prevVal;
                }
                else if (_editingColumn == 0) SaveFiles();
            }
            CancelCellEdit();
        }

        private void CancelCellEdit()
        {
            _cellEditor.Visible = false;
            _editingItem        = null;
            _editingColumn      = -1;
            _editingOriginalPhaseNumber = -1;
        }

        private bool TryUpdatePhaseFromRow(ListViewItem item, int originalNum)
        {
            try
            {
                if (originalNum < 0) return false;
                if (!int.TryParse(item.Text, out int newNum)) return false;
                PhaseCollection phases = _model.GetPhases();
                Phase target = null;
                foreach (Phase p in phases)
                {
                    if (p.PhaseNumber == newNum && p.PhaseNumber != originalNum) { MessageBox.Show("Phase number already exists."); return false; }
                    if (p.PhaseNumber == originalNum) target = p;
                }
                if (target == null) return false;
                target.PhaseNumber  = newNum;
                target.PhaseName    = item.SubItems[1].Text;
                target.PhaseComment = item.SubItems[2].Text;
                if (!target.Modify()) return false;
                _model.CommitChanges();
                return true;
            }
            catch { return false; }
        }

        private int GetClickedColumnIndex(int clickX)
        {
            int x = 0;
            for (int i = 0; i < listViewPhases.Columns.Count; i++)
            {
                x += listViewPhases.Columns[i].Width;
                if (clickX <= x) return i;
            }
            return -1;
        }

        // ── Save files ───────────────────────────────────────────────────────
        private void SaveFiles()
        {
            string attrPath = Path.Combine(_model.GetInfo().ModelPath, "attributes");
            if (!Directory.Exists(attrPath)) Directory.CreateDirectory(attrPath);

            var phaseData     = new List<KeyValuePair<int, Color>>();
            var allNums       = new List<int>();
            var phaseRepTrans = new Dictionary<int, int>();

            foreach (ListViewItem item in _allItems)
            {
                if (!int.TryParse(item.Text, out int phaseNum)) continue;
                allNums.Add(phaseNum);

                string colorId = item.SubItems.Count > 3 ? item.SubItems[3].Text : "";
                if (TryParseStoredColor(colorId, out Color col))
                    phaseData.Add(new KeyValuePair<int, Color>(phaseNum, col));

                string tLabel = item.SubItems.Count > 5 ? item.SubItems[5].Text : "as is";
                phaseRepTrans[phaseNum] = _transLabelToRepVal.ContainsKey(tLabel) ? _transLabelToRepVal[tLabel] : 10;
            }

            foreach (int pn in allNums)
            {
                string fp = Path.Combine(attrPath, $"+TPM_F_{pn}.PObjGrp");
                if (File.Exists(fp) && !phaseData.Any(p => p.Key == pn)) File.Delete(fp);
            }

            foreach (var phase in phaseData)
            {
                var sb = new StringBuilder();
                sb.Append("TITLE_OBJECT_GROUP \r\n{\r\n    Version= 1.05 \r\n    Count= 1 \r\n");
                sb.Append("    SECTION_OBJECT_GROUP \r\n    {\r\n        0 \r\n        1 \r\n        co_object \r\n        proPHASE \r\n        albl_Phase \r\n        == \r\n        albl_Equals \r\n");
                sb.Append($"        {phase.Key} \r\n");
                sb.Append("        0 \r\n        && \r\n        }\r\n    }\r\n");
                File.WriteAllText(Path.Combine(attrPath, $"+TPM_F_{phase.Key}.PObjGrp"), sb.ToString(), Encoding.Default);
            }

            var visPhases = new List<int>();
            foreach (ListViewItem item in _allItems)
                if (item.SubItems.Count > 4 && IsVisibleCellValue(item.SubItems[4].Text) && int.TryParse(item.Text, out int vn))
                    visPhases.Add(vn);

            var vobj = new StringBuilder();
            vobj.Append($"TITLE_OBJECT_GROUP \r\n{{\r\n    Version= 1.05 \r\n    Count= {visPhases.Count} \r\n");
            foreach (int vp in visPhases)
            {
                vobj.Append("    SECTION_OBJECT_GROUP \r\n    {\r\n        0 \r\n        1 \r\n        co_object \r\n        proPHASE \r\n        albl_Phase \r\n        == \r\n        albl_Equals \r\n");
                vobj.Append($"        {vp} \r\n");
                vobj.Append("        0 \r\n        || \r\n        }\r\n");
            }
            vobj.Append("}\r\n");
            File.WriteAllText(Path.Combine(attrPath, "+TPM_widocznosc.VObjGrp"), vobj.ToString(), Encoding.Default);

            var rep = new StringBuilder();
            rep.Append("REPRESENTATIONS \r\n{\r\n    Version= 1.04 \r\n");
            rep.Append($"    Count= {phaseData.Count + 1} \r\n");

            foreach (var phase in phaseData)
            {
                Color c = phase.Value;
                int colorRef = c.B * 65536 + c.G * 256 + c.R;
                int trans    = phaseRepTrans.ContainsKey(phase.Key) ? phaseRepTrans[phase.Key] : 10;

                rep.Append("    SECTION_UTILITY_LIMITS \r\n    {\r\n        0 \r\n        0 \r\n        0 \r\n        0 \r\n        }\r\n");
                rep.Append($"    SECTION_OBJECT_REP \r\n    {{\r\n        +TPM_F_{phase.Key} \r\n        {colorRef} \r\n        {trans} \r\n        }}\r\n");
                rep.Append("    SECTION_OBJECT_REP_BY_ATTRIBUTE \r\n    {\r\n        SECTION_OBJECT_GROUP \r\n        }\r\n");
                rep.Append($"    SECTION_OBJECT_REP_RGB_VALUE \r\n    {{\r\n        {c.R} \r\n        {c.G} \r\n        {c.B} \r\n        }}\r\n");
            }

            rep.Append("    SECTION_UTILITY_LIMITS \r\n    {\r\n        0 \r\n        0 \r\n        0 \r\n        0 \r\n        }\r\n");
            rep.Append("    SECTION_OBJECT_REP \r\n    {\r\n        All \r\n        -2 \r\n        10 \r\n        }\r\n");
            rep.Append("    SECTION_OBJECT_REP_BY_ATTRIBUTE \r\n    {\r\n        SECTION_OBJECT_GROUP \r\n        }\r\n");
            rep.Append("    SECTION_OBJECT_REP_RGB_VALUE \r\n    {\r\n        -1 \r\n        -1 \r\n        -1 \r\n        }\r\n");
            rep.Append("}\r\n");

            File.WriteAllText(Path.Combine(attrPath, "+TPM_kolory.rep"), rep.ToString(), Encoding.Default);
            AutoRefreshTeklaView();
        }

        private void AutoRefreshTeklaView()
        {
            try
            {
                if (_model == null || !_model.GetConnectionStatus()) return;
                _model.CommitChanges();
                WriteTeklaRedrawMacro();
                Tekla.Structures.Model.Operations.Operation.RunMacro("+TeklaRedrawView.cs");
            }
            catch { }
        }

        private void WriteTeklaRedrawMacro()
        {
            try
            {
                string macroDir = _model.GetInfo().ModelPath;
                if (!Directory.Exists(macroDir)) Directory.CreateDirectory(macroDir);

                string content =
                    "using System;\r\n" +
                    "using Tekla.Structures.Model;\r\n" +
                    "using Tekla.Structures.Model.UI;\r\n" +
                    "\r\n" +
                    "public class TeklaRedrawView\r\n" +
                    "{\r\n" +
                    "    public static void Run(Tekla.Structures.Model.Operations.Operation op)\r\n" +
                    "    {\r\n" +
                    "        try\r\n" +
                    "        {\r\n" +
                    "            ViewHandler.SetRepresentation(\"+TPM_kolory\");\r\n" +
                    "            ModelViewEnumerator views = ViewHandler.GetAllViews();\r\n" +
                    "            while (views.MoveNext())\r\n" +
                    "            {\r\n" +
                    "                View view = views.Current;\r\n" +
                    "                view.ViewFilter = \"+TPM_widocznosc\";\r\n" +
                    "                view.Modify();\r\n" +
                    "                ViewHandler.RedrawView(view);\r\n" +
                    "            }\r\n" +
                    "        }\r\n" +
                    "        catch { }\r\n" +
                    "    }\r\n" +
                    "}\r\n";

                File.WriteAllText(
                    Path.Combine(macroDir, "+TeklaRedrawView.cs"),
                    content,
                    Encoding.Default);
            }
            catch { }
        }

        // ── Read helpers ─────────────────────────────────────────────────────
        private Dictionary<int, string> ReadExistingPhaseTransparency()
        {
            var result = new Dictionary<int, string>();
            try
            {
                string path = Path.Combine(_model.GetInfo().ModelPath, "attributes", "+TPM_kolory.rep");
                if (!File.Exists(path)) return result;
                string[] lines = File.ReadAllLines(path, Encoding.Default);
                for (int i = 0; i < lines.Length; i++)
                {
                    if (!lines[i].Contains("+TPM_F_")) continue;
                    string line = lines[i].Trim();
                    int start   = line.IndexOf("_F_") + 3;
                    if (!int.TryParse(line.Substring(start).Trim(), out int pNum)) continue;
                    if (i + 2 < lines.Length && int.TryParse(lines[i + 2].Trim(), out int rv))
                        result[pNum] = RepValToLabel(rv);
                }
            }
            catch { }
            return result;
        }

        private string RepValToLabel(int v)
        {
            switch (v)
            {
                case 10: return "visible";
                case 5:  return "50%";
                case 3:  return "70%";
                case 1:  return "90%";
                case 0:  return "hidden";
                default: return "as is";
            }
        }

        private Dictionary<int, string> ReadExistingPhaseColors()
        {
            var result = new Dictionary<int, string>();
            try
            {
                string path = Path.Combine(_model.GetInfo().ModelPath, "attributes", "+TPM_kolory.rep");
                if (!File.Exists(path)) return result;
                string[] lines = File.ReadAllLines(path, Encoding.Default);
                for (int i = 0; i < lines.Length; i++)
                {
                    if (!lines[i].Contains("+TPM_F_") || i + 1 >= lines.Length) continue;
                    string line = lines[i].Trim();
                    int start   = line.IndexOf("_F_") + 3;
                    if (!int.TryParse(line.Substring(start).Trim(), out int pNum)) continue;

                    int rawValue = -1;
                    if (int.TryParse(lines[i + 1].Trim(), out int rv)) rawValue = rv;

                    int rgbR = -1, rgbG = -1, rgbB = -1;
                    for (int j = i + 1; j < Math.Min(i + 25, lines.Length); j++)
                    {
                        if (!lines[j].Contains("SECTION_OBJECT_REP_RGB_VALUE")) continue;
                        if (j + 4 < lines.Length &&
                            int.TryParse(lines[j + 2].Trim(), out int r) &&
                            int.TryParse(lines[j + 3].Trim(), out int g) &&
                            int.TryParse(lines[j + 4].Trim(), out int b))
                        { rgbR = r; rgbG = g; rgbB = b; }
                        break;
                    }

                    int idx = rgbR >= 0 ? FindColorIndexByRGB(rgbR, rgbG, rgbB) : FindColorIndexByColorRef(rawValue);
                    if (idx >= 0)
                        result[pNum] = idx.ToString();
                    else if (rgbR >= 0)
                        result[pNum] = EncodeColorToken(Color.FromArgb(rgbR, rgbG, rgbB));
                    else if (rawValue >= 0)
                        result[pNum] = EncodeColorToken(ColorFromColorRef(rawValue));
                }
            }
            catch { }
            return result;
        }

        private Dictionary<int, bool> ReadExistingVisibility()
        {
            var result = new Dictionary<int, bool>();
            try
            {
                string attrPath = Path.Combine(_model.GetInfo().ModelPath, "attributes");
                string visPath  = new[] {
                    Path.Combine(attrPath, "+TPM_widocznosc.VObjGrp"),
                    Path.Combine(attrPath, "+TPM_widocznosc.PObjGrp")
                }.FirstOrDefault(File.Exists);
                if (string.IsNullOrEmpty(visPath)) return result;
                string[] lines = File.ReadAllLines(visPath, Encoding.Default);
                for (int i = 0; i < lines.Length; i++)
                    if (lines[i].Contains("albl_Equals") && i + 1 < lines.Length && int.TryParse(lines[i + 1].Trim(), out int n))
                        result[n] = true;
            }
            catch { }
            return result;
        }

        // ── Colour helpers ───────────────────────────────────────────────────
        private int FindColorIndexByRGB(int r, int g, int b)
        {
            for (int i = 0; i < _teklaColors.Length; i++)
                if (_teklaColors[i].R == r && _teklaColors[i].G == g && _teklaColors[i].B == b) return i;
            return -1;
        }

        private int FindColorIndexByColorRef(int colorRef) =>
            FindColorIndexByRGB(colorRef & 0xFF, (colorRef >> 8) & 0xFF, (colorRef >> 16) & 0xFF);

        private Color ColorFromColorRef(int colorRef) =>
            Color.FromArgb(colorRef & 0xFF, (colorRef >> 8) & 0xFF, (colorRef >> 16) & 0xFF);

        private bool TryParseStoredColor(string token, out Color color)
        {
            color = Color.Empty;
            if (string.IsNullOrWhiteSpace(token)) return false;
            if (int.TryParse(token, out int idx) && idx >= 0 && idx < _teklaColors.Length)
                { color = _teklaColors[idx]; return true; }
            if (token.Length == 7 && token[0] == '#' &&
                int.TryParse(token.Substring(1, 2), System.Globalization.NumberStyles.HexNumber, null, out int r) &&
                int.TryParse(token.Substring(3, 2), System.Globalization.NumberStyles.HexNumber, null, out int g) &&
                int.TryParse(token.Substring(5, 2), System.Globalization.NumberStyles.HexNumber, null, out int b))
                { color = Color.FromArgb(r, g, b); return true; }
            return false;
        }

        private string EncodeColorToken(Color c)
        {
            int i = FindColorIndexByRGB(c.R, c.G, c.B);
            return i >= 0 ? i.ToString() : $"#{c.R:X2}{c.G:X2}{c.B:X2}";
        }

        // ── Buttons ──────────────────────────────────────────────────────────
        private void btnPin_Click(object sender, EventArgs e)
        {
            this.TopMost = !this.TopMost;
            UpdatePinButtonAppearance();
        }

        private void UpdatePinButtonAppearance()
        {
            btnPin.BackColor = this.TopMost ? Color.FromArgb(0, 120, 212) : DkBtnBg;
            btnPin.ForeColor = this.TopMost ? Color.White : DkText;
            btnPin.FlatAppearance.BorderColor = this.TopMost ? Color.FromArgb(0, 90, 170) : DkBorder;
        }

        private void btnOpenPhaseManager_Click(object sender, EventArgs e)
        {
            try { Tekla.Structures.Model.Operations.Operation.RunMacro("+TeklaOpenPhaseManagerMacro.cs"); }
            catch { }
        }

        private void listViewPhases_SelectedIndexChanged(object sender, EventArgs e) { }

        // ── Dark renderers ───────────────────────────────────────────────────
        private void ApplyDarkContextMenu(ContextMenuStrip menu)
        {
            menu.Renderer  = new DarkContextRenderer();
            menu.BackColor = DkCtxBg;
            menu.ForeColor = DkText;
            ApplyDarkMenuItems(menu.Items);
        }

        private void ApplyDarkMenuItems(System.Windows.Forms.ToolStripItemCollection items)
        {
            foreach (System.Windows.Forms.ToolStripItem item in items)
            {
                item.BackColor = DkCtxBg;
                item.ForeColor = DkText;
                if (item is ToolStripMenuItem mi && mi.HasDropDownItems)
                    ApplyDarkMenuItems(mi.DropDownItems);
            }
        }

        private class DarkStripRenderer : ToolStripProfessionalRenderer
        {
            public DarkStripRenderer() : base(new ProfessionalColorTable()) { }
            protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e) { }
            protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
            {
                using (SolidBrush b = new SolidBrush(Color.FromArgb(235, 235, 235)))
                    e.Graphics.FillRectangle(b, e.AffectedBounds);
            }
        }

        private class DarkContextRenderer : ToolStripProfessionalRenderer
        {
            private static readonly Color Bg  = Color.FromArgb(255, 255, 255);
            private static readonly Color Sel = Color.FromArgb(204, 228, 247);
            private static readonly Color Brd = Color.FromArgb(180, 180, 180);
            private static readonly Color Txt = Color.FromArgb(15,  15,  15);
            private static readonly Color Dim = Color.FromArgb(150, 150, 150);

            public DarkContextRenderer() : base(new ProfessionalColorTable()) { RoundedEdges = false; }

            protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
            {
                Color c = e.Item.Selected && e.Item.Enabled ? Sel : Bg;
                using (SolidBrush b = new SolidBrush(c))
                    e.Graphics.FillRectangle(b, new Rectangle(Point.Empty, e.Item.Size));
            }
            protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
            {
                using (SolidBrush b = new SolidBrush(Bg)) e.Graphics.FillRectangle(b, e.AffectedBounds);
            }
            protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
            {
                using (Pen p = new Pen(Brd))
                    e.Graphics.DrawRectangle(p, 0, 0, e.ToolStrip.Width - 1, e.ToolStrip.Height - 1);
            }
            protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
            {
                int y = e.Item.Height / 2;
                using (Pen p = new Pen(Brd)) e.Graphics.DrawLine(p, 28, y, e.Item.Width - 4, y);
            }
            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
            {
                e.TextColor = e.Item.Enabled ? Txt : Dim;
                base.OnRenderItemText(e);
            }
            protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
            {
                e.ArrowColor = Color.FromArgb(170, 170, 195);
                base.OnRenderArrow(e);
            }
            protected override void OnRenderImageMargin(ToolStripRenderEventArgs e)
            {
                using (SolidBrush b = new SolidBrush(Color.FromArgb(240, 240, 240)))
                    e.Graphics.FillRectangle(b, e.AffectedBounds);
            }
        }

        // ── Unused legacy stubs ──────────────────────────────────────────────
        private void btnUpdateFiles_Click(object sender, EventArgs e) => SaveFiles();
        private int MapManagerToTekla(string c) => int.TryParse(c, out int v) ? v : -1;
    }
}
