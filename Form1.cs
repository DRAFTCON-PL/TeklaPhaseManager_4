using System;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Tasks = System.Threading.Tasks;
using System.Windows.Forms;
using Tekla.Structures.Model;

namespace TeklaPhaseManager_4
{
    public partial class Form1 : Form
    {
        // ── Dark theme palette ──────────────────────────────────────────────
        private static readonly Color _darkBg       = Color.FromArgb(45,  45,  48);
        private static readonly Color _darkSurface  = Color.FromArgb(62,  62,  66);
        private static readonly Color _darkText     = Color.FromArgb(240, 240, 240);
        private static readonly Color _darkAccent   = Color.FromArgb(0,   122, 204);
        private static readonly Color _darkListBg   = Color.FromArgb(37,  37,  38);
        private static readonly Color _darkListAlt  = Color.FromArgb(45,  45,  47);
        private static readonly Color _darkBorder   = Color.FromArgb(80,  80,  80);
        private static readonly Color _darkSubText  = Color.FromArgb(160, 160, 160);

        // ── Tekla colour palette ────────────────────────────────────────────
        private readonly Color[] _teklaColors = new Color[]
        {
            Color.WhiteSmoke, Color.Gray, Color.Red, Color.LimeGreen, Color.Cyan,
            Color.Blue, Color.Yellow, Color.Magenta, Color.SaddleBrown, Color.DarkGreen,
            Color.DarkBlue, Color.DarkSlateGray, Color.Orange, Color.Silver, Color.Firebrick
        };

        // ── Transparency labels / mapping ───────────────────────────────────
        private static readonly string[] _transLabels = { "jak jest", "widoczne", "50%", "70%", "90%", "ukryty" };
        private static readonly Dictionary<string, int> _transLabelToRepVal = new Dictionary<string, int>
        {
            { "jak jest", 10 }, { "widoczne", 10 }, { "50%", 5 }, { "70%", 3 }, { "90%", 1 }, { "ukryty", 0 }
        };

        // ── State ───────────────────────────────────────────────────────────
        private Model    _model;
        private TextBox  _cellEditor;
        private ListViewItem _editingItem;
        private int      _editingColumn           = -1;
        private int      _editingOriginalPhaseNumber = -1;

        private readonly List<ListViewItem> _allItems = new List<ListViewItem>();
        private const    string FilterPlaceholder = "Szukaj fazy...";
        private          string _filterText       = "";

        private int       _sortColumn = -1;
        private SortOrder _sortOrder  = SortOrder.None;

        // ── Constructor ─────────────────────────────────────────────────────
        public Form1()
        {
            InitializeComponent();
            ApplyDarkTheme();

            this.TopMost = true;
            UpdatePinButtonAppearance();
            _model = new Model();

            listViewPhases.OwnerDraw = true;
            listViewPhases.DrawColumnHeader += listViewPhases_DrawColumnHeader;
            listViewPhases.DrawItem         += listViewPhases_DrawItem;
            listViewPhases.DrawSubItem      += listViewPhases_DrawSubItem;

            _cellEditor = new TextBox
            {
                Visible     = false,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor   = _darkSurface,
                ForeColor   = _darkText
            };
            _cellEditor.KeyDown    += CellEditor_KeyDown;
            _cellEditor.LostFocus  += CellEditor_LostFocus;
            listViewPhases.Controls.Add(_cellEditor);

            SetFilterPlaceholder();
            txtFilter.GotFocus  += (s, e) => { if (txtFilter.Text == FilterPlaceholder) { txtFilter.Text = ""; txtFilter.ForeColor = _darkText; } };
            txtFilter.LostFocus += (s, e) => { if (string.IsNullOrEmpty(txtFilter.Text)) SetFilterPlaceholder(); };

            UpdateStatus();
        }

        // ── Dark theme ──────────────────────────────────────────────────────
        private void ApplyDarkTheme()
        {
            this.BackColor = _darkBg;
            this.ForeColor = _darkText;

            foreach (Control c in this.Controls)
            {
                if (c is Button btn)
                {
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.BackColor = _darkSurface;
                    btn.ForeColor = _darkText;
                    btn.FlatAppearance.BorderColor = _darkBorder;
                }
                else if (c is Label lbl)
                {
                    lbl.BackColor = Color.Transparent;
                    lbl.ForeColor = _darkSubText;
                }
                else if (c is TextBox tb)
                {
                    tb.BackColor   = _darkListBg;
                    tb.ForeColor   = _darkText;
                    tb.BorderStyle = BorderStyle.FixedSingle;
                }
                else if (c is ListView lv)
                {
                    lv.BackColor = _darkListBg;
                    lv.ForeColor = _darkText;
                }
                else if (c is StatusStrip ss)
                {
                    ss.BackColor  = Color.FromArgb(30, 30, 30);
                    ss.ForeColor  = _darkText;
                    ss.Renderer   = new DarkStatusRenderer();
                }
            }
        }

        private void SetFilterPlaceholder()
        {
            txtFilter.Text      = FilterPlaceholder;
            txtFilter.ForeColor = _darkSubText;
        }

        // ── Status bar ──────────────────────────────────────────────────────
        private void UpdateStatus()
        {
            bool connected = _model != null && _model.GetConnectionStatus();
            statusLabelConnection.ForeColor = connected ? Color.FromArgb(100, 210, 100) : Color.FromArgb(220, 80, 80);
            statusLabelConnection.Text      = connected ? "● Połączony" : "● Rozłączony";
            statusLabelModel.Text           = connected ? _model.GetInfo().ModelName : "";
        }

        // ── Load phases ─────────────────────────────────────────────────────
        private void btnLoad_Click(object sender, EventArgs e)
        {
            if (_model == null || !_model.GetConnectionStatus()) _model = new Model();
            if (!_model.GetConnectionStatus())
            {
                MessageBox.Show("Brak połączenia z Tekla Structures.", "TPM", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                UpdateStatus();
                return;
            }

            _allItems.Clear();
            listViewPhases.Items.Clear();

            PhaseCollection phases         = _model.GetPhases();
            var existingColors             = ReadExistingPhaseColors();
            var existingVisibility         = ReadExistingVisibility();
            var existingTransparency       = ReadExistingPhaseTransparency();

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

                string trans = existingTransparency.ContainsKey(phase.PhaseNumber) ? existingTransparency[phase.PhaseNumber] : "jak jest";
                item.SubItems.Add(trans);

                item.SubItems.Add("...");  // column 6 — object count loaded async

                _allItems.Add(item);
            }

            ApplyFilter();
            UpdateStatus();
            LoadObjectCountsAsync();
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

        // ── Object count (async) ────────────────────────────────────────────
        private async void LoadObjectCountsAsync()
        {
            Dictionary<int, int> counts = await Tasks.Task.Run(() =>
            {
                var result = new Dictionary<int, int>();
                try
                {
                    var objs = _model.GetModelObjectSelector()
                                     .GetAllObjectsWithType(ModelObject.ModelObjectEnum.UNKNOWN);
                    while (objs.MoveNext())
                    {
                        if (objs.Current is Part p)
                        {
                            p.GetPhase(out Phase ph);
                            if (!result.ContainsKey(ph.PhaseNumber)) result[ph.PhaseNumber] = 0;
                            result[ph.PhaseNumber]++;
                        }
                    }
                }
                catch { }
                return result;
            });

            if (IsDisposed || !IsHandleCreated) return;
            Invoke((Action)(() =>
            {
                foreach (ListViewItem item in _allItems)
                {
                    if (int.TryParse(item.Text, out int num) && item.SubItems.Count > 6)
                        item.SubItems[6].Text = counts.ContainsKey(num) ? counts[num].ToString() : "0";
                }
                listViewPhases.Invalidate();
            }));
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
                dlg.Title       = "Zapisz preset";
                dlg.Filter      = "Preset TPM (*.tpm)|*.tpm|Wszystkie pliki (*.*)|*.*";
                dlg.DefaultExt  = "tpm";
                if (_model != null && _model.GetConnectionStatus())
                    dlg.InitialDirectory = Path.Combine(_model.GetInfo().ModelPath, "attributes");
                if (dlg.ShowDialog() != DialogResult.OK) return;

                var sb = new StringBuilder();
                sb.AppendLine("# TeklaPhaseManager_4 Preset");
                sb.AppendLine("# Nr;Kolor;Widocznosc;Przezroczystosc");
                foreach (ListViewItem item in _allItems)
                {
                    string color = item.SubItems.Count > 3 ? item.SubItems[3].Text : "";
                    string vis   = item.SubItems.Count > 4 ? item.SubItems[4].Text : "0";
                    string trans = item.SubItems.Count > 5 ? item.SubItems[5].Text : "jak jest";
                    sb.AppendLine($"{item.Text};{color};{vis};{trans}");
                }
                File.WriteAllText(dlg.FileName, sb.ToString(), Encoding.UTF8);
                MessageBox.Show("Preset zapisany.", "TPM", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnLoadPreset_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title  = "Wczytaj preset";
                dlg.Filter = "Preset TPM (*.tpm)|*.tpm|Wszystkie pliki (*.*)|*.*";
                if (_model != null && _model.GetConnectionStatus())
                    dlg.InitialDirectory = Path.Combine(_model.GetInfo().ModelPath, "attributes");
                if (dlg.ShowDialog() != DialogResult.OK) return;

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
                    var (color, vis, trans) = presetData[num];
                    ApplyColorToItemSingle(item, string.IsNullOrEmpty(color) ? "brak" : color);
                    if (item.SubItems.Count > 4) item.SubItems[4].Text = vis;
                    if (item.SubItems.Count > 5) item.SubItems[5].Text = _transLabels.Contains(trans) ? trans : "jak jest";
                }
                listViewPhases.Invalidate();
                SaveFiles();
                MessageBox.Show("Preset wczytany.", "TPM", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // ── Click handling ───────────────────────────────────────────────────
        private void listViewPhases_MouseClick(object sender, MouseEventArgs e)
        {
            ListViewItem item = listViewPhases.GetItemAt(e.X, e.Y);
            if (item == null) return;

            if (_cellEditor.Visible) CommitCellEdit();

            int col = GetClickedColumnIndex(e.X);

            if (col == 1 || col == 2) { BeginCellEdit(item, col); return; }

            if (col == 3) { ShowColorContextMenu(item, e.X, e.Y); return; }

            if (col == 4 && item.SubItems.Count > 4)
            {
                bool cur = IsVisibleCellValue(item.SubItems[4].Text);
                foreach (ListViewItem t in GetTargetItems(item))
                    if (t.SubItems.Count > 4) t.SubItems[4].Text = cur ? "0" : "1";
                listViewPhases.Invalidate();
                SaveFiles();
                return;
            }

            if (col == 5) { ShowTransparencyContextMenu(item, e.X, e.Y); }
        }

        private List<ListViewItem> GetTargetItems(ListViewItem clicked)
        {
            if (listViewPhases.SelectedItems.Count > 1 && listViewPhases.SelectedItems.Contains(clicked))
                return listViewPhases.SelectedItems.Cast<ListViewItem>().ToList();
            return new List<ListViewItem> { clicked };
        }

        private bool IsVisibleCellValue(string v) => v == "1" || v == "☑";

        // ── Transparency context menu ────────────────────────────────────────
        private void ShowTransparencyContextMenu(ListViewItem item, int x, int y)
        {
            string current = item.SubItems.Count > 5 ? item.SubItems[5].Text : "jak jest";
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Renderer = new DarkMenuRenderer();
            menu.BackColor = _darkSurface;
            menu.ForeColor = _darkText;

            foreach (string label in _transLabels)
            {
                string lbl = label;
                ToolStripMenuItem mi = new ToolStripMenuItem(lbl) { Checked = (current == lbl) };
                mi.Click += (s, ev) => ApplyTransparencyToItem(item, lbl);
                menu.Items.Add(mi);
            }
            menu.Show(listViewPhases, x, y);
        }

        private void ApplyTransparencyToItem(ListViewItem clickedItem, string label)
        {
            foreach (ListViewItem item in GetTargetItems(clickedItem))
                if (item.SubItems.Count > 5) item.SubItems[5].Text = label;
            SaveFiles();
        }

        // ── Colour context menu ──────────────────────────────────────────────
        private void ShowColorContextMenu(ListViewItem item, int x, int y)
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Renderer  = new DarkMenuRenderer();
            menu.BackColor = _darkSurface;
            menu.ForeColor = _darkText;
            menu.ShowImageMargin = true;

            ToolStripMenuItem noColor = new ToolStripMenuItem("Jak jest / Brak koloru");
            noColor.Click += (s, e) => ApplyColorToItem(item, "brak");
            menu.Items.Add(noColor);
            menu.Items.Add(new ToolStripSeparator());

            for (int i = 0; i < _teklaColors.Length; i++)
            {
                int idx = i;
                ToolStripMenuItem mi = new ToolStripMenuItem($"Kolor {idx}");
                mi.Image  = CreateColorSwatchImage(_teklaColors[idx]);
                mi.Click += (s, e) => ApplyColorToItem(item, idx.ToString());
                menu.Items.Add(mi);
            }

            menu.Items.Add(new ToolStripSeparator());
            ToolStripMenuItem custom = new ToolStripMenuItem("Wybierz kolor...");
            custom.Click += (s, e) =>
            {
                using (ColorDialog cd = new ColorDialog { AnyColor = true, FullOpen = true })
                    if (cd.ShowDialog(this) == DialogResult.OK)
                        ApplyColorToItem(item, EncodeColorToken(cd.Color));
            };
            menu.Items.Add(custom);
            menu.Show(listViewPhases, x, y);
        }

        private void ApplyColorToItem(ListViewItem clickedItem, string colorId)
        {
            foreach (ListViewItem item in GetTargetItems(clickedItem))
                ApplyColorToItemSingle(item, colorId);
            SaveFiles();
        }

        private void ApplyColorToItemSingle(ListViewItem item, string colorId)
        {
            if (item.SubItems.Count <= 3) return;
            if (colorId == "brak")
            {
                item.SubItems[3].Text      = "";
                item.SubItems[3].BackColor = _darkListBg;
                item.SubItems[3].ForeColor = _darkText;
            }
            else
            {
                item.SubItems[3].Text = colorId;
                if (TryParseStoredColor(colorId, out Color c))
                {
                    item.UseItemStyleForSubItems  = false;
                    item.SubItems[3].BackColor    = c;
                    item.SubItems[3].ForeColor    = ((c.R * 0.299) + (c.G * 0.587) + (c.B * 0.114) > 160) ? Color.Black : Color.White;
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

        // ── OwnerDraw ────────────────────────────────────────────────────────
        private void listViewPhases_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            using (SolidBrush bg  = new SolidBrush(_darkSurface))
            using (Pen border     = new Pen(_darkBorder, 1f))
            {
                e.Graphics.FillRectangle(bg, e.Bounds);
                e.Graphics.DrawRectangle(border, new Rectangle(e.Bounds.X, e.Bounds.Y, e.Bounds.Width - 1, e.Bounds.Height - 1));
            }
            string arrow = "";
            if (e.ColumnIndex == _sortColumn)
                arrow = _sortOrder == SortOrder.Ascending ? " ▲" : " ▼";
            TextRenderer.DrawText(e.Graphics, e.Header.Text + arrow, e.Font, e.Bounds, _darkText,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.LeftAndRightPadding | TextFormatFlags.SingleLine);
        }

        private void listViewPhases_DrawItem(object sender, DrawListViewItemEventArgs e) { }

        private void listViewPhases_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            bool selected = e.Item.Selected;
            int  idx      = e.Item.Index;

            if (e.ColumnIndex == 3)
            {
                Color back = e.SubItem.BackColor;
                if (back == Color.Empty || back == SystemColors.Window) back = _darkListBg;
                using (SolidBrush b = new SolidBrush(back))
                    e.Graphics.FillRectangle(b, e.Bounds);
                string txt = e.SubItem.Text ?? "";
                if (txt.StartsWith("#")) txt = "Custom";
                Color fore = (e.SubItem.ForeColor == Color.Empty) ? _darkText : e.SubItem.ForeColor;
                TextRenderer.DrawText(e.Graphics, txt, listViewPhases.Font, e.Bounds, fore,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.LeftAndRightPadding);
                return;
            }

            if (e.ColumnIndex == 4)
            {
                Color bg = selected ? _darkAccent : (idx % 2 == 0 ? _darkListBg : _darkListAlt);
                using (SolidBrush b = new SolidBrush(bg))
                    e.Graphics.FillRectangle(b, e.Bounds);

                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                Rectangle ir = new Rectangle(
                    e.Bounds.X + (e.Bounds.Width - 18) / 2,
                    e.Bounds.Y + (e.Bounds.Height - 12) / 2, 18, 12);

                bool vis = IsVisibleCellValue(e.SubItem.Text);
                if (vis)
                {
                    using (Pen pen = new Pen(Color.FromArgb(80, 180, 255), 1.5f))
                        e.Graphics.DrawEllipse(pen, ir);
                    Rectangle pupil = new Rectangle(ir.X + 5, ir.Y + 3, 8, 6);
                    using (SolidBrush pb = new SolidBrush(Color.FromArgb(80, 180, 255)))
                        e.Graphics.FillEllipse(pb, pupil);
                }
                else
                {
                    using (SolidBrush gb = new SolidBrush(Color.FromArgb(100, Color.Gray)))
                        e.Graphics.FillEllipse(gb, ir);
                    using (Pen lp = new Pen(Color.FromArgb(200, 60, 60), 2f))
                        e.Graphics.DrawLine(lp, ir.X + 2, ir.Y + ir.Height / 2, ir.Right - 2, ir.Y + ir.Height / 2);
                }
                e.Graphics.SmoothingMode = SmoothingMode.Default;
                return;
            }

            // All other columns
            Color cellBg = selected ? _darkAccent : (idx % 2 == 0 ? _darkListBg : _darkListAlt);
            Color textFg = selected ? Color.White : _darkText;

            using (SolidBrush bgBrush = new SolidBrush(cellBg))
                e.Graphics.FillRectangle(bgBrush, e.Bounds);

            string text = e.ColumnIndex == 0 ? e.Item.Text : (e.SubItem?.Text ?? "");
            if (e.ColumnIndex == 6 && text == "...") textFg = _darkSubText;

            TextRenderer.DrawText(e.Graphics, text, listViewPhases.Font, e.Bounds, textFg,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.LeftAndRightPadding);
        }

        // ── Inline cell editor ───────────────────────────────────────────────
        private void BeginCellEdit(ListViewItem item, int column)
        {
            if (item == null || column < 0 || column >= item.SubItems.Count) return;
            _editingItem = item;
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
            if (e.KeyCode == Keys.Enter)  { CommitCellEdit(); e.SuppressKeyPress = true; }
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
                    MessageBox.Show("Numer fazy musi być dodatnią liczbą całkowitą.");
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
                    if (p.PhaseNumber == newNum && p.PhaseNumber != originalNum) { MessageBox.Show("Faza o podanym numerze już istnieje."); return false; }
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

            var phaseData    = new List<KeyValuePair<int, Color>>();
            var allNums      = new List<int>();
            var phaseRepTrans = new Dictionary<int, int>();

            foreach (ListViewItem item in _allItems)
            {
                if (!int.TryParse(item.Text, out int phaseNum)) continue;
                allNums.Add(phaseNum);

                string colorId = item.SubItems.Count > 3 ? item.SubItems[3].Text : "";
                if (TryParseStoredColor(colorId, out Color col))
                    phaseData.Add(new KeyValuePair<int, Color>(phaseNum, col));

                string tLabel = item.SubItems.Count > 5 ? item.SubItems[5].Text : "jak jest";
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

            // VObjGrp visibility
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

            // .rep file
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
                TryRedrawTeklaViews();
            }
            catch { }
        }

        private void TryRedrawTeklaViews()
        {
            try
            {
                Type vh = Type.GetType("Tekla.Structures.Model.UI.ViewHandler, Tekla.Structures.Model.UI");
                if (vh == null) return;
                var setRep = vh.GetMethod("SetRepresentation", new[] { typeof(string) });
                if (setRep != null) setRep.Invoke(null, new object[] { "+TPM_kolory" });
                var redraw = vh.GetMethod("RedrawViews");
                if (redraw != null) { redraw.Invoke(null, null); return; }
                var getViews  = vh.GetMethod("GetVisibleViews");
                var redrawOne = vh.GetMethods().FirstOrDefault(m => m.Name == "RedrawView" && m.GetParameters().Length == 1);
                if (getViews == null || redrawOne == null) return;
                object en = getViews.Invoke(null, null);
                if (en == null) return;
                var moveNext = en.GetType().GetMethod("MoveNext");
                var current  = en.GetType().GetProperty("Current");
                if (moveNext == null || current == null) return;
                while ((bool)moveNext.Invoke(en, null))
                {
                    object view = current.GetValue(en, null);
                    if (view != null) redrawOne.Invoke(null, new[] { view });
                }
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
                case 10: return "widoczne";
                case 5:  return "50%";
                case 3:  return "70%";
                case 1:  return "90%";
                case 0:  return "ukryty";
                default: return "jak jest";
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

        private int FindColorIndexByColorRef(int colorRef)
        {
            return FindColorIndexByRGB(colorRef & 0xFF, (colorRef >> 8) & 0xFF, (colorRef >> 16) & 0xFF);
        }

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
            btnPin.BackColor = this.TopMost ? _darkAccent : _darkSurface;
            btnPin.ForeColor = Color.White;
        }

        private void btnRefreshTeklaView_Click(object sender, EventArgs e) => AutoRefreshTeklaView();

        private void btnOpenPhaseManager_Click(object sender, EventArgs e)
        {
            try { Tekla.Structures.Model.Operations.Operation.RunMacro("+TeklaOpenPhaseManagerMacro.cs"); }
            catch { }
        }

        private void listViewPhases_SelectedIndexChanged(object sender, EventArgs e) { }

        // ── Unused legacy handlers kept for compatibility ──────────────────
        private void btnUpdateFiles_Click(object sender, EventArgs e) => SaveFiles();
        private void btnAdd_Click(object sender, EventArgs e)
        {
            int next = 1;
            foreach (ListViewItem li in listViewPhases.Items)
                if (int.TryParse(li.Text, out int ex) && ex >= next) next = ex + 1;
            Phase p = new Phase { PhaseNumber = next, PhaseName = "Nowa faza", PhaseComment = "" };
            if (p.Insert()) { _model.CommitChanges(); btnLoad_Click(null, null); }
        }
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (listViewPhases.SelectedItems.Count == 0) return;
            if (int.TryParse(listViewPhases.SelectedItems[0].Text, out int num))
            {
                foreach (Phase p in _model.GetPhases()) { if (p.PhaseNumber == num) { p.Delete(); break; } }
                _model.CommitChanges(); btnLoad_Click(null, null);
            }
        }
        private int MapManagerToTekla(string c) => int.TryParse(c, out int v) ? v : -1;

        // ── Dark menu renderer ────────────────────────────────────────────────
        private class DarkMenuRenderer : ToolStripProfessionalRenderer
        {
            public DarkMenuRenderer() : base(new DarkColorTable()) { }

            protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
            {
                Color bg = e.Item.Selected ? Color.FromArgb(0, 122, 204) : Color.FromArgb(62, 62, 66);
                using (SolidBrush b = new SolidBrush(bg))
                    e.Graphics.FillRectangle(b, new Rectangle(Point.Empty, e.Item.Size));
            }

            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
            {
                e.TextColor = Color.FromArgb(240, 240, 240);
                base.OnRenderItemText(e);
            }

            protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
            {
                using (SolidBrush b = new SolidBrush(Color.FromArgb(62, 62, 66)))
                    e.Graphics.FillRectangle(b, e.AffectedBounds);
            }

            protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
            {
                int y = e.Item.Height / 2;
                using (Pen p = new Pen(Color.FromArgb(80, 80, 80)))
                    e.Graphics.DrawLine(p, 0, y, e.Item.Width, y);
            }
        }

        private class DarkColorTable : ProfessionalColorTable
        {
            public override Color ToolStripDropDownBackground   => Color.FromArgb(62, 62, 66);
            public override Color MenuBorder                    => Color.FromArgb(80, 80, 80);
            public override Color MenuItemBorder                => Color.FromArgb(0, 122, 204);
            public override Color MenuItemSelected              => Color.FromArgb(0, 122, 204);
            public override Color ImageMarginGradientBegin      => Color.FromArgb(50, 50, 50);
            public override Color ImageMarginGradientMiddle     => Color.FromArgb(50, 50, 50);
            public override Color ImageMarginGradientEnd        => Color.FromArgb(50, 50, 50);
            public override Color SeparatorDark                 => Color.FromArgb(80, 80, 80);
            public override Color SeparatorLight                => Color.FromArgb(80, 80, 80);
            public override Color CheckBackground               => Color.FromArgb(0, 122, 204);
            public override Color CheckSelectedBackground       => Color.FromArgb(0, 100, 180);
            public override Color CheckPressedBackground        => Color.FromArgb(0, 100, 180);
        }

        // ── Dark status strip renderer ────────────────────────────────────────
        private class DarkStatusRenderer : ToolStripProfessionalRenderer
        {
            public DarkStatusRenderer() : base(new DarkStatusColorTable()) { }
            protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
            {
                using (SolidBrush b = new SolidBrush(Color.FromArgb(30, 30, 30)))
                    e.Graphics.FillRectangle(b, e.AffectedBounds);
            }
        }

        private class DarkStatusColorTable : ProfessionalColorTable
        {
            public override Color StatusStripGradientBegin => Color.FromArgb(30, 30, 30);
            public override Color StatusStripGradientEnd   => Color.FromArgb(30, 30, 30);
        }
    }
}
