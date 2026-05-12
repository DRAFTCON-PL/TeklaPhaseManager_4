using System;
using System.IO;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;
using Tekla.Structures.Model;
using System.IO.Pipes;

namespace TeklaPhaseManager_4
{
    public partial class Form1 : Form
    {
        private Model _model;
        private TextBox _cellEditor;
        private ListViewItem _editingItem;
        private int _editingColumn = -1;
        private int _editingOriginalPhaseNumber = -1;
        private readonly Color[] _teklaColors = new Color[]
        {
            Color.WhiteSmoke, Color.Gray, Color.Red, Color.LimeGreen, Color.Cyan,
            Color.Blue, Color.Yellow, Color.Magenta, Color.SaddleBrown, Color.DarkGreen,
            Color.DarkBlue, Color.DarkSlateGray, Color.Orange, Color.Silver, Color.Firebrick
        };

        // Transparency: labels shown in the UI, mapped to Tekla .rep file values (scale 0-10)
        private static readonly string[] _transLabels = { "jak jest", "widoczne", "50%", "70%", "90%", "ukryty" };
        private static readonly Dictionary<string, int> _transLabelToRepVal = new Dictionary<string, int>
        {
            { "jak jest", 10 }, { "widoczne", 10 }, { "50%", 5 }, { "70%", 3 }, { "90%", 1 }, { "ukryty", 0 }
        };

        public Form1()
        {
            InitializeComponent();
            this.TopMost = true;
            UpdatePinButtonAppearance();
            _model = new Model();

            listViewPhases.OwnerDraw = true;
            listViewPhases.DrawColumnHeader += listViewPhases_DrawColumnHeader;
            listViewPhases.DrawItem += listViewPhases_DrawItem;
            listViewPhases.DrawSubItem += listViewPhases_DrawSubItem;

            _cellEditor = new TextBox();
            _cellEditor.Visible = false;
            _cellEditor.BorderStyle = BorderStyle.FixedSingle;
            _cellEditor.KeyDown += CellEditor_KeyDown;
            _cellEditor.LostFocus += CellEditor_LostFocus;
            listViewPhases.Controls.Add(_cellEditor);
        }

        private void CmbClassColor_DrawItem(object sender, DrawItemEventArgs e) { }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            if (_model == null || !_model.GetConnectionStatus()) _model = new Model();
            if (!_model.GetConnectionStatus())
            {
                MessageBox.Show("Brak połączenia z Tekla Structures.", "TPM", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            listViewPhases.Items.Clear();
            PhaseCollection phases = _model.GetPhases();
            var existingColors = ReadExistingPhaseColors();
            var existingVisibility = ReadExistingVisibility();
            var existingTransparency = ReadExistingPhaseTransparency();

            var phaseList = new List<Phase>();
            foreach (Phase ph in phases) phaseList.Add(ph);
            phaseList = phaseList.OrderBy(p => p.PhaseNumber).ToList();

            foreach (Phase phase in phaseList)
            {
                ListViewItem item = new ListViewItem(phase.PhaseNumber.ToString());
                item.SubItems.Add(phase.PhaseName ?? "");
                item.SubItems.Add(phase.PhaseComment ?? "");

                if (existingColors.ContainsKey(phase.PhaseNumber))
                {
                    string colorId = existingColors[phase.PhaseNumber];
                    item.SubItems.Add(colorId);
                    if (int.TryParse(colorId, out int cId) && cId >= 0 && cId < _teklaColors.Length)
                    {
                        item.UseItemStyleForSubItems = false;
                        item.SubItems[3].BackColor = _teklaColors[cId];
                        item.SubItems[3].ForeColor = (cId == 0 || cId == 6 || cId == 13) ? Color.Black : Color.White;
                    }
                    else if (colorId.StartsWith("#") && TryParseStoredColor(colorId, out Color customColor))
                    {
                        item.UseItemStyleForSubItems = false;
                        item.SubItems[3].BackColor = customColor;
                        item.SubItems[3].ForeColor = ((customColor.R * 0.299) + (customColor.G * 0.587) + (customColor.B * 0.114) > 160)
                            ? Color.Black : Color.White;
                    }
                }
                else item.SubItems.Add("");

                bool isVisible = existingVisibility.ContainsKey(phase.PhaseNumber) ? existingVisibility[phase.PhaseNumber] : false;
                item.SubItems.Add(isVisible ? "1" : "0");

                string transLabel = existingTransparency.ContainsKey(phase.PhaseNumber)
                    ? existingTransparency[phase.PhaseNumber]
                    : "jak jest";
                item.SubItems.Add(transLabel);

                listViewPhases.Items.Add(item);
            }
        }

        // Reads transparency from the .rep file (3rd value in SECTION_OBJECT_REP).
        // Confirmed mapping from Tekla: 10=widoczne, 5=50%, 3=70%, 1=90%, 0=ukryty.
        private Dictionary<int, string> ReadExistingPhaseTransparency()
        {
            var result = new Dictionary<int, string>();
            try
            {
                string repPath = Path.Combine(_model.GetInfo().ModelPath, "attributes", "+TPM_kolory.rep");
                if (!File.Exists(repPath)) return result;
                string[] lines = File.ReadAllLines(repPath, Encoding.Default);
                for (int i = 0; i < lines.Length; i++)
                {
                    if (!lines[i].Contains("+TPM_F_")) continue;
                    string line = lines[i].Trim();
                    int start = line.IndexOf("_F_") + 3;
                    if (!int.TryParse(line.Substring(start).Trim(), out int pNum)) continue;
                    // i+1 = colorRef, i+2 = transparency rep value
                    if (i + 2 < lines.Length && int.TryParse(lines[i + 2].Trim(), out int repVal))
                        result[pNum] = RepValToLabel(repVal);
                }
            }
            catch { }
            return result;
        }

        private string RepValToLabel(int repVal)
        {
            switch (repVal)
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
            var phaseColors = new Dictionary<int, string>();
            try
            {
                string repPath = Path.Combine(_model.GetInfo().ModelPath, "attributes", "+TPM_kolory.rep");
                if (File.Exists(repPath))
                {
                    string[] lines = File.ReadAllLines(repPath, Encoding.Default);
                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (lines[i].Contains("+TPM_F_") && i + 1 < lines.Length)
                        {
                            string line = lines[i].Trim();
                            int start = line.IndexOf("_F_") + 3;
                            if (int.TryParse(line.Substring(start).Trim(), out int pNum))
                            {
                                int parsedIndex = -1;
                                int rawValue = -1;
                                int rgbR = -1;
                                int rgbG = -1;
                                int rgbB = -1;

                                if (int.TryParse(lines[i + 1].Trim(), out int repValue))
                                    rawValue = repValue;

                                for (int j = i + 1; j < Math.Min(i + 25, lines.Length); j++)
                                {
                                    if (!lines[j].Contains("SECTION_OBJECT_REP_RGB_VALUE")) continue;

                                    if (j + 4 < lines.Length &&
                                        int.TryParse(lines[j + 2].Trim(), out int r) &&
                                        int.TryParse(lines[j + 3].Trim(), out int g) &&
                                        int.TryParse(lines[j + 4].Trim(), out int b))
                                    {
                                        rgbR = r; rgbG = g; rgbB = b;
                                    }
                                    break;
                                }

                                if (rgbR >= 0 && rgbG >= 0 && rgbB >= 0)
                                    parsedIndex = FindColorIndexByRGB(rgbR, rgbG, rgbB);
                                else if (rgbR == -1 && rgbG == -1 && rgbB == -1 && rawValue >= 0)
                                    parsedIndex = FindColorIndexByColorRef(rawValue);

                                if (parsedIndex < 0 && rawValue >= 0 && rawValue < _teklaColors.Length)
                                    parsedIndex = rawValue;

                                if (parsedIndex >= 0)
                                    phaseColors[pNum] = parsedIndex.ToString();
                                else if (rgbR >= 0 && rgbG >= 0 && rgbB >= 0)
                                    phaseColors[pNum] = EncodeColorToken(Color.FromArgb(rgbR, rgbG, rgbB));
                                else if (rgbR == -1 && rgbG == -1 && rgbB == -1 && rawValue >= _teklaColors.Length)
                                    phaseColors[pNum] = EncodeColorToken(ColorFromColorRef(rawValue));
                            }
                        }
                    }
                }
            }
            catch { }
            return phaseColors;
        }

        private int FindColorIndexByRGB(int r, int g, int b)
        {
            for (int i = 0; i < _teklaColors.Length; i++)
            {
                var c = _teklaColors[i];
                if (c.R == r && c.G == g && c.B == b) return i;
            }
            return -1;
        }

        private int FindColorIndexByColorRef(int colorRef)
        {
            int r = colorRef & 0xFF;
            int g = (colorRef >> 8) & 0xFF;
            int b = (colorRef >> 16) & 0xFF;
            return FindColorIndexByRGB(r, g, b);
        }

        private Color ColorFromColorRef(int colorRef)
        {
            int r = colorRef & 0xFF;
            int g = (colorRef >> 8) & 0xFF;
            int b = (colorRef >> 16) & 0xFF;
            return Color.FromArgb(r, g, b);
        }

        private bool TryParseStoredColor(string token, out Color color)
        {
            color = Color.Empty;
            if (string.IsNullOrWhiteSpace(token)) return false;

            if (int.TryParse(token, out int colorIndex) && colorIndex >= 0 && colorIndex < _teklaColors.Length)
            {
                color = _teklaColors[colorIndex];
                return true;
            }

            if (token.Length == 7 && token[0] == '#')
            {
                if (int.TryParse(token.Substring(1, 2), System.Globalization.NumberStyles.HexNumber, null, out int r) &&
                    int.TryParse(token.Substring(3, 2), System.Globalization.NumberStyles.HexNumber, null, out int g) &&
                    int.TryParse(token.Substring(5, 2), System.Globalization.NumberStyles.HexNumber, null, out int b))
                {
                    color = Color.FromArgb(r, g, b);
                    return true;
                }
            }

            return false;
        }

        private string EncodeColorToken(Color color)
        {
            int idx = FindColorIndexByRGB(color.R, color.G, color.B);
            if (idx >= 0) return idx.ToString();
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        private Dictionary<int, bool> ReadExistingVisibility()
        {
            var phaseVisibility = new Dictionary<int, bool>();
            try
            {
                string attrPath = Path.Combine(_model.GetInfo().ModelPath, "attributes");
                string[] visibilityFiles =
                {
                    Path.Combine(attrPath, "+TPM_widocznosc.VObjGrp"),
                    Path.Combine(attrPath, "+TPM_widocznosc.PObjGrp")
                };

                string visPath = visibilityFiles.FirstOrDefault(File.Exists);
                if (!string.IsNullOrEmpty(visPath))
                {
                    string[] lines = File.ReadAllLines(visPath, Encoding.Default);
                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (lines[i].Contains("albl_Equals"))
                        {
                            if (i + 1 < lines.Length && int.TryParse(lines[i + 1].Trim(), out int phaseNum))
                                phaseVisibility[phaseNum] = true;
                        }
                    }
                }
            }
            catch { }
            return phaseVisibility;
        }

        private void listViewPhases_SelectedIndexChanged(object sender, EventArgs e) { }

        private void listViewPhases_MouseClick(object sender, MouseEventArgs e)
        {
            ListViewItem item = listViewPhases.GetItemAt(e.X, e.Y);
            if (item == null) return;

            if (_cellEditor.Visible)
                CommitCellEdit();

            int clickedColumn = GetClickedColumnIndex(e.X);

            if (clickedColumn == 1 || clickedColumn == 2)
            {
                BeginCellEdit(item, clickedColumn);
                return;
            }

            if (clickedColumn == 3)
            {
                ShowColorContextMenu(item, e.X, e.Y);
                return;
            }

            if (clickedColumn == 4 && item.SubItems.Count > 4)
            {
                bool currentVisibility = IsVisibleCellValue(item.SubItems[4].Text);
                item.SubItems[4].Text = currentVisibility ? "0" : "1";
                listViewPhases.Invalidate();
                SaveFiles();
                return;
            }

            if (clickedColumn == 5)
            {
                ShowTransparencyContextMenu(item, e.X, e.Y);
            }
        }

        private bool IsVisibleCellValue(string value)
        {
            return value == "1" || value == "☑";
        }

        private void ShowTransparencyContextMenu(ListViewItem item, int x, int y)
        {
            string current = item.SubItems.Count > 5 ? item.SubItems[5].Text : "jak jest";
            ContextMenuStrip menu = new ContextMenuStrip();
            foreach (string label in _transLabels)
            {
                string lbl = label;
                ToolStripMenuItem mi = new ToolStripMenuItem(lbl);
                mi.Checked = (current == lbl);
                mi.Click += (s, ev) => ApplyTransparencyToItem(item, lbl);
                menu.Items.Add(mi);
            }
            menu.Show(listViewPhases, x, y);
        }

        private void ApplyTransparencyToItem(ListViewItem item, string label)
        {
            if (item.SubItems.Count <= 5) return;
            item.SubItems[5].Text = label;
            SaveFiles();
        }

        private void BeginCellEdit(ListViewItem item, int column)
        {
            if (item == null || column < 0 || column >= item.SubItems.Count) return;

            _editingItem = item;
            _editingColumn = column;
            int.TryParse(item.Text, out _editingOriginalPhaseNumber);

            Rectangle bounds = item.SubItems[column].Bounds;
            _cellEditor.Bounds = new Rectangle(bounds.X + 1, bounds.Y + 1, bounds.Width - 2, bounds.Height - 2);
            _cellEditor.Text = item.SubItems[column].Text;
            _cellEditor.Visible = true;
            _cellEditor.Focus();
            _cellEditor.SelectAll();
        }

        private void CellEditor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                CommitCellEdit();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                CancelCellEdit();
                e.SuppressKeyPress = true;
            }
        }

        private void CellEditor_LostFocus(object sender, EventArgs e)
        {
            if (_cellEditor.Visible)
                CommitCellEdit();
        }

        private void CommitCellEdit()
        {
            if (_editingItem == null || _editingColumn < 0)
            {
                CancelCellEdit();
                return;
            }

            string newValue = _cellEditor.Text ?? string.Empty;
            string previousValue = _editingColumn == 0 ? _editingItem.Text : _editingItem.SubItems[_editingColumn].Text;

            if (_editingColumn == 0)
            {
                if (!int.TryParse(newValue, out int parsedNumber) || parsedNumber <= 0)
                {
                    MessageBox.Show("Numer fazy musi być dodatnią liczbą całkowitą.");
                    CancelCellEdit();
                    return;
                }
                _editingItem.Text = newValue;
            }
            else
            {
                _editingItem.SubItems[_editingColumn].Text = newValue;
            }

            if (_editingColumn == 0 || _editingColumn == 1 || _editingColumn == 2)
            {
                bool updated = TryUpdatePhaseFromRow(_editingItem, _editingOriginalPhaseNumber);
                if (!updated)
                {
                    if (_editingColumn == 0) _editingItem.Text = previousValue;
                    else _editingItem.SubItems[_editingColumn].Text = previousValue;
                }
                else if (_editingColumn == 0)
                {
                    SaveFiles();
                }
            }

            CancelCellEdit();
        }

        private void CancelCellEdit()
        {
            _cellEditor.Visible = false;
            _editingItem = null;
            _editingColumn = -1;
            _editingOriginalPhaseNumber = -1;
        }

        private bool TryUpdatePhaseFromRow(ListViewItem item, int originalPhaseNumber)
        {
            try
            {
                if (originalPhaseNumber < 0) return false;
                if (!int.TryParse(item.Text, out int newPhaseNumber)) return false;

                PhaseCollection phases = _model.GetPhases();
                Phase phaseToUpdate = null;

                foreach (Phase phase in phases)
                {
                    if (phase.PhaseNumber == newPhaseNumber && phase.PhaseNumber != originalPhaseNumber)
                    {
                        MessageBox.Show("Faza o podanym numerze już istnieje.");
                        return false;
                    }
                    if (phase.PhaseNumber == originalPhaseNumber)
                        phaseToUpdate = phase;
                }

                if (phaseToUpdate == null) return false;

                phaseToUpdate.PhaseNumber = newPhaseNumber;
                phaseToUpdate.PhaseName = item.SubItems[1].Text;
                phaseToUpdate.PhaseComment = item.SubItems[2].Text;

                if (!phaseToUpdate.Modify()) return false;

                _model.CommitChanges();
                return true;
            }
            catch { }
            return false;
        }

        private int GetClickedColumnIndex(int clickX)
        {
            int currentX = 0;
            for (int i = 0; i < listViewPhases.Columns.Count; i++)
            {
                currentX += listViewPhases.Columns[i].Width;
                if (clickX <= currentX) return i;
            }
            return -1;
        }

        private void ShowColorContextMenu(ListViewItem item, int x, int y)
        {
            ContextMenuStrip contextMenu = new ContextMenuStrip();
            contextMenu.ShowImageMargin = true;

            ToolStripMenuItem noColorItem = new ToolStripMenuItem("Jak jest / Brak koloru");
            noColorItem.Click += (s, e) => ApplyColorToItem(item, "brak");
            contextMenu.Items.Add(noColorItem);
            contextMenu.Items.Add(new ToolStripSeparator());

            for (int i = 0; i < _teklaColors.Length; i++)
            {
                int colorIndex = i;
                Color color = _teklaColors[colorIndex];
                ToolStripMenuItem colorItem = new ToolStripMenuItem($"Kolor {colorIndex}");
                colorItem.Image = CreateColorSwatchImage(color);
                colorItem.Click += (s, e) => ApplyColorToItem(item, colorIndex.ToString());
                contextMenu.Items.Add(colorItem);
            }

            contextMenu.Items.Add(new ToolStripSeparator());
            ToolStripMenuItem chooseAnyColorItem = new ToolStripMenuItem("Wybierz kolor...");
            chooseAnyColorItem.Click += (s, e) =>
            {
                using (ColorDialog colorDialog = new ColorDialog())
                {
                    colorDialog.AnyColor = true;
                    colorDialog.FullOpen = true;
                    if (colorDialog.ShowDialog(this) == DialogResult.OK)
                        ApplyColorToItem(item, EncodeColorToken(colorDialog.Color));
                }
            };
            contextMenu.Items.Add(chooseAnyColorItem);
            contextMenu.Show(listViewPhases, x, y);
        }

        private Bitmap CreateColorSwatchImage(Color color)
        {
            Bitmap bmp = new Bitmap(16, 16);
            using (Graphics g = Graphics.FromImage(bmp))
            using (Pen borderPen = new Pen(Color.Black, 1f))
            using (SolidBrush brush = new SolidBrush(color))
            {
                Rectangle rect = new Rectangle(0, 0, 15, 15);
                g.FillRectangle(brush, rect);
                g.DrawRectangle(borderPen, rect);
            }
            return bmp;
        }

        private void ApplyColorToItem(ListViewItem item, string colorId)
        {
            if (item.SubItems.Count <= 3) return;

            if (colorId == "brak")
            {
                item.SubItems[3].Text = "";
                item.SubItems[3].BackColor = SystemColors.Window;
                item.SubItems[3].ForeColor = SystemColors.WindowText;
            }
            else
            {
                item.SubItems[3].Text = colorId;
                if (TryParseStoredColor(colorId, out Color parsedColor))
                {
                    item.UseItemStyleForSubItems = false;
                    item.SubItems[3].BackColor = parsedColor;
                    item.SubItems[3].ForeColor = ((parsedColor.R * 0.299) + (parsedColor.G * 0.587) + (parsedColor.B * 0.114) > 160)
                        ? Color.Black : Color.White;
                }
            }
            SaveFiles();
        }

        private void listViewPhases_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true;
        }

        private void listViewPhases_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            e.DrawDefault = true;
        }

        private void listViewPhases_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            if (e.ColumnIndex == 3)
            {
                Color back = e.SubItem.BackColor;
                if (back == Color.Empty) back = listViewPhases.BackColor;

                using (SolidBrush bgBrush = new SolidBrush(back))
                    e.Graphics.FillRectangle(bgBrush, e.Bounds);

                string txt = e.SubItem.Text ?? string.Empty;
                if (txt.StartsWith("#")) txt = "Custom";
                Color fore = e.SubItem.ForeColor;
                if (fore == Color.Empty) fore = listViewPhases.ForeColor;

                TextRenderer.DrawText(e.Graphics, txt, listViewPhases.Font, e.Bounds, fore,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.LeftAndRightPadding);
                return;
            }

            if (e.ColumnIndex == 4)
            {
                Color cellBg = e.Item.Selected ? SystemColors.Highlight : listViewPhases.BackColor;
                using (SolidBrush bgBrush = new SolidBrush(cellBg))
                    e.Graphics.FillRectangle(bgBrush, e.Bounds);

                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                Rectangle iconRect = new Rectangle(
                    e.Bounds.X + (e.Bounds.Width - 18) / 2,
                    e.Bounds.Y + (e.Bounds.Height - 12) / 2,
                    18, 12);

                bool isVisible = IsVisibleCellValue(e.SubItem.Text);
                if (isVisible)
                {
                    using (Pen eyePen = new Pen(Color.DarkBlue, 1.5f))
                    {
                        e.Graphics.DrawEllipse(eyePen, iconRect);
                        Rectangle pupil = new Rectangle(iconRect.X + 5, iconRect.Y + 3, 8, 6);
                        using (SolidBrush pupilBrush = new SolidBrush(Color.DarkBlue))
                            e.Graphics.FillEllipse(pupilBrush, pupil);
                    }
                }
                else
                {
                    using (SolidBrush grayBrush = new SolidBrush(Color.FromArgb(180, Color.Gray)))
                        e.Graphics.FillEllipse(grayBrush, iconRect);
                    using (Pen crossPen = new Pen(Color.Firebrick, 2f))
                        e.Graphics.DrawLine(crossPen,
                            iconRect.X + 2, iconRect.Y + iconRect.Height / 2,
                            iconRect.Right - 2, iconRect.Y + iconRect.Height / 2);
                }
                return;
            }

            e.DrawDefault = true;
        }

        private void btnUpdateFiles_Click(object sender, EventArgs e)
        {
            SaveFiles();
        }

        private int MapManagerToTekla(string managerColor)
        {
            if (string.IsNullOrEmpty(managerColor)) return -1;
            if (int.TryParse(managerColor, out int v)) return v;
            return -1;
        }

        private void btnOpenPhaseManager_Click(object sender, EventArgs e)
        {
            try
            {
                if (Tekla.Structures.Model.Operations.Operation.RunMacro("+TeklaOpenPhaseManagerMacro.cs"))
                    return;
            }
            catch { }
        }

        private void btnRefreshTeklaView_Click(object sender, EventArgs e)
        {
            try
            {
                if (Tekla.Structures.Model.Operations.Operation.RunMacro("+TeklaRedrawView.cs") ||
                    Tekla.Structures.Model.Operations.Operation.RunMacro("+TeklaRedrawView"))
                    return;
            }
            catch { }
        }

        private void SaveFiles()
        {
            string attrPath = Path.Combine(_model.GetInfo().ModelPath, "attributes");
            if (!Directory.Exists(attrPath)) Directory.CreateDirectory(attrPath);

            var phaseData = new List<KeyValuePair<int, Color>>();
            var allPhaseNumbers = new List<int>();

            // Build per-phase transparency rep values (10=widoczne, 5=50%, 3=70%, 1=90%, 0=ukryty)
            var phaseRepTrans = new Dictionary<int, int>();

            foreach (ListViewItem item in listViewPhases.Items)
            {
                if (!int.TryParse(item.Text, out int phaseNum)) continue;
                allPhaseNumbers.Add(phaseNum);

                string managerColor = item.SubItems.Count > 3 ? item.SubItems[3].Text : "";
                if (TryParseStoredColor(managerColor, out Color selectedColor))
                    phaseData.Add(new KeyValuePair<int, Color>(phaseNum, selectedColor));

                string tLabel = item.SubItems.Count > 5 ? item.SubItems[5].Text : "jak jest";
                phaseRepTrans[phaseNum] = _transLabelToRepVal.ContainsKey(tLabel) ? _transLabelToRepVal[tLabel] : 10;
            }

            // Remove .PObjGrp files for phases without colors
            foreach (var phaseNum in allPhaseNumbers)
            {
                string fileName = $"+TPM_F_{phaseNum}.PObjGrp";
                string filePath = Path.Combine(attrPath, fileName);
                if (File.Exists(filePath))
                {
                    var existingPhase = phaseData.FirstOrDefault(p => p.Key == phaseNum);
                    if (existingPhase.Key == 0) File.Delete(filePath);
                }
            }

            // Create .PObjGrp per colored phase
            foreach (var phase in phaseData)
            {
                StringBuilder objGroup = new StringBuilder();
                objGroup.Append("TITLE_OBJECT_GROUP \r\n{\r\n    Version= 1.05 \r\n");
                objGroup.Append("    Count= 1 \r\n");
                objGroup.Append("    SECTION_OBJECT_GROUP \r\n    {\r\n        0 \r\n        1 \r\n        co_object \r\n        proPHASE \r\n        albl_Phase \r\n        == \r\n        albl_Equals \r\n");
                objGroup.Append($"        {phase.Key} \r\n");
                objGroup.Append("        0 \r\n        && \r\n        }\r\n    }\r\n");
                string fileName = $"+TPM_F_{phase.Key}.PObjGrp";
                File.WriteAllText(Path.Combine(attrPath, fileName), objGroup.ToString(), Encoding.Default);
            }

            // Build visibility VObjGrp
            List<int> visiblePhases = new List<int>();
            foreach (ListViewItem item in listViewPhases.Items)
            {
                if (item.SubItems.Count > 4 && IsVisibleCellValue(item.SubItems[4].Text))
                {
                    if (int.TryParse(item.Text, out int phaseNum)) visiblePhases.Add(phaseNum);
                }
            }

            StringBuilder vobj = new StringBuilder();
            vobj.Append("TITLE_OBJECT_GROUP \r\n{\r\n    Version= 1.05 \r\n");
            vobj.Append($"    Count= {visiblePhases.Count} \r\n");
            for (int i = 0; i < visiblePhases.Count; i++)
            {
                vobj.Append("    SECTION_OBJECT_GROUP \r\n    {\r\n        0 \r\n        1 \r\n        co_object \r\n        proPHASE \r\n        albl_Phase \r\n        == \r\n        albl_Equals \r\n");
                vobj.Append($"        {visiblePhases[i]} \r\n");
                vobj.Append("        0 \r\n        || \r\n        }\r\n");
            }
            vobj.Append("}\r\n");
            File.WriteAllText(Path.Combine(attrPath, "+TPM_widocznosc.VObjGrp"), vobj.ToString(), Encoding.Default);

            // Build +TPM_kolory.rep — format matched exactly to Tekla's own file format
            StringBuilder rep = new StringBuilder();
            rep.Append("REPRESENTATIONS \r\n{\r\n    Version= 1.04 \r\n");
            rep.Append($"    Count= {phaseData.Count + 1} \r\n");

            foreach (var phase in phaseData)
            {
                string subName = $"+TPM_F_{phase.Key}";
                Color color = phase.Value;
                int rgbR = color.R, rgbG = color.G, rgbB = color.B;
                int colorRef = rgbB * 65536 + rgbG * 256 + rgbR;
                int repTransVal = phaseRepTrans.ContainsKey(phase.Key) ? phaseRepTrans[phase.Key] : 10;

                rep.Append("    SECTION_UTILITY_LIMITS \r\n    {\r\n        0 \r\n        0 \r\n        0 \r\n        0 \r\n        }\r\n");
                rep.Append("    SECTION_OBJECT_REP \r\n    {\r\n");
                rep.Append($"        {subName} \r\n");
                rep.Append($"        {colorRef} \r\n");
                rep.Append($"        {repTransVal} \r\n        }}\r\n");
                rep.Append("    SECTION_OBJECT_REP_BY_ATTRIBUTE \r\n    {\r\n        SECTION_OBJECT_GROUP \r\n        }\r\n");
                rep.Append("    SECTION_OBJECT_REP_RGB_VALUE \r\n    {\r\n");
                rep.Append($"        {rgbR} \r\n");
                rep.Append($"        {rgbG} \r\n");
                rep.Append($"        {rgbB} \r\n");
                rep.Append("        }\r\n");
            }

            // Trailing "All" section (Kolor wg klasy, Widoczne)
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
                Type viewHandlerType = Type.GetType("Tekla.Structures.Model.UI.ViewHandler, Tekla.Structures.Model.UI");
                if (viewHandlerType == null) return;

                // Apply our representation file so Tekla uses our colors and transparency
                var setRepresentation = viewHandlerType.GetMethod("SetRepresentation", new[] { typeof(string) });
                if (setRepresentation != null)
                    setRepresentation.Invoke(null, new object[] { "+TPM_kolory" });

                var redrawViews = viewHandlerType.GetMethod("RedrawViews");
                if (redrawViews != null)
                {
                    redrawViews.Invoke(null, null);
                    return;
                }

                var getVisibleViews = viewHandlerType.GetMethod("GetVisibleViews");
                var redrawView = viewHandlerType.GetMethods().FirstOrDefault(m => m.Name == "RedrawView" && m.GetParameters().Length == 1);
                if (getVisibleViews == null || redrawView == null) return;

                object viewsEnum = getVisibleViews.Invoke(null, null);
                if (viewsEnum == null) return;

                var moveNext = viewsEnum.GetType().GetMethod("MoveNext");
                var current = viewsEnum.GetType().GetProperty("Current");
                if (moveNext == null || current == null) return;

                while ((bool)moveNext.Invoke(viewsEnum, null))
                {
                    object view = current.GetValue(viewsEnum, null);
                    if (view != null) redrawView.Invoke(null, new[] { view });
                }
            }
            catch { }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            int nextNumber = 1;
            foreach (ListViewItem li in listViewPhases.Items)
            {
                if (int.TryParse(li.Text, out int existing))
                {
                    if (existing >= nextNumber) nextNumber = existing + 1;
                }
            }

            Phase p = new Phase { PhaseNumber = nextNumber, PhaseName = "Nowa faza", PhaseComment = "" };
            if (p.Insert())
            {
                _model.CommitChanges();
                btnLoad_Click(null, null);
            }
        }

        private void btnModify_Click(object sender, EventArgs e)
        {
            if (listViewPhases.SelectedItems.Count == 0) return;
            if (int.TryParse(listViewPhases.SelectedItems[0].Text, out int num))
            {
                PhaseCollection ps = _model.GetPhases();
                string newName = listViewPhases.SelectedItems[0].SubItems[1].Text;
                string newComment = listViewPhases.SelectedItems[0].SubItems[2].Text;
                foreach (Phase p in ps) { if (p.PhaseNumber == num) { p.PhaseName = newName; p.PhaseComment = newComment; p.Modify(); break; } }
                _model.CommitChanges();
                btnLoad_Click(null, null);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (listViewPhases.SelectedItems.Count == 0) return;
            if (int.TryParse(listViewPhases.SelectedItems[0].Text, out int num))
            {
                PhaseCollection ps = _model.GetPhases();
                foreach (Phase p in ps) { if (p.PhaseNumber == num) { p.Delete(); break; } }
                _model.CommitChanges();
                btnLoad_Click(null, null);
            }
        }

        private void btnPin_Click(object sender, EventArgs e)
        {
            this.TopMost = !this.TopMost;
            UpdatePinButtonAppearance();
        }

        private void UpdatePinButtonAppearance()
        {
            if (this.TopMost)
            {
                btnPin.BackColor = Color.CornflowerBlue;
                btnPin.ForeColor = Color.White;
            }
            else
            {
                btnPin.BackColor = SystemColors.Control;
                btnPin.ForeColor = SystemColors.ControlText;
            }
        }

        private void btnLock_Click(object sender, EventArgs e)
        {
            if (listViewPhases.SelectedItems.Count == 0) return;
            if (int.TryParse(listViewPhases.SelectedItems[0].Text, out int num))
            {
                ModelObjectEnumerator os = _model.GetModelObjectSelector().GetAllObjectsWithType(ModelObject.ModelObjectEnum.UNKNOWN);
                while (os.MoveNext())
                {
                    Part pt = os.Current as Part;
                    if (pt != null)
                    {
                        pt.GetPhase(out Phase ph);
                        if (ph.PhaseNumber == num) { pt.SetUserProperty("LOCKED", 1); pt.Modify(); }
                    }
                }
                _model.CommitChanges();
                MessageBox.Show("Zablokowano.");
            }
        }
    }
}
