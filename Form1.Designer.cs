namespace TeklaPhaseManager_4
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.btnLoad = new System.Windows.Forms.Button();
            this.btnPin = new System.Windows.Forms.Button();
            this.txtFilter = new System.Windows.Forms.TextBox();
            this.listViewPhases = new System.Windows.Forms.ListView();
            this.columnHeaderNum = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderName = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderComment = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderColor = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderVisible = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderTransparency = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderCount = new System.Windows.Forms.ColumnHeader();
            this.btnAllVisible = new System.Windows.Forms.Button();
            this.btnAllHidden = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.btnSavePreset = new System.Windows.Forms.Button();
            this.btnLoadPreset = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.statusLabelConnection = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusLabelModel = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();

            // btnLoad
            this.btnLoad.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLoad.Location = new System.Drawing.Point(12, 12);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(446, 27);
            this.btnLoad.TabIndex = 20;
            this.btnLoad.Text = "Wczytaj fazy";
            this.btnLoad.UseVisualStyleBackColor = true;
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);

            // btnPin
            this.btnPin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPin.Location = new System.Drawing.Point(462, 12);
            this.btnPin.Name = "btnPin";
            this.btnPin.Size = new System.Drawing.Size(28, 27);
            this.btnPin.TabIndex = 25;
            this.btnPin.Text = "📌";
            this.btnPin.UseVisualStyleBackColor = false;
            this.btnPin.Click += new System.EventHandler(this.btnPin_Click);

            // txtFilter
            this.txtFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFilter.Location = new System.Drawing.Point(12, 45);
            this.txtFilter.Name = "txtFilter";
            this.txtFilter.Size = new System.Drawing.Size(478, 23);
            this.txtFilter.TabIndex = 30;
            this.txtFilter.TextChanged += new System.EventHandler(this.txtFilter_TextChanged);

            // listViewPhases
            this.listViewPhases.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewPhases.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                this.columnHeaderNum,
                this.columnHeaderName,
                this.columnHeaderComment,
                this.columnHeaderColor,
                this.columnHeaderVisible,
                this.columnHeaderTransparency,
                this.columnHeaderCount });
            this.listViewPhases.FullRowSelect = true;
            this.listViewPhases.GridLines = true;
            this.listViewPhases.HideSelection = false;
            this.listViewPhases.Location = new System.Drawing.Point(12, 74);
            this.listViewPhases.MultiSelect = true;
            this.listViewPhases.Name = "listViewPhases";
            this.listViewPhases.Size = new System.Drawing.Size(478, 368);
            this.listViewPhases.TabIndex = 14;
            this.listViewPhases.UseCompatibleStateImageBehavior = false;
            this.listViewPhases.View = System.Windows.Forms.View.Details;
            this.listViewPhases.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listViewPhases_ColumnClick);
            this.listViewPhases.SelectedIndexChanged += new System.EventHandler(this.listViewPhases_SelectedIndexChanged);
            this.listViewPhases.MouseClick += new System.Windows.Forms.MouseEventHandler(this.listViewPhases_MouseClick);

            // columns
            this.columnHeaderNum.Text = "Nr";
            this.columnHeaderNum.Width = 30;
            this.columnHeaderName.Text = "Nazwa";
            this.columnHeaderName.Width = 82;
            this.columnHeaderComment.Text = "Komentarz";
            this.columnHeaderComment.Width = 96;
            this.columnHeaderColor.Text = "Kolor";
            this.columnHeaderColor.Width = 50;
            this.columnHeaderVisible.Text = "Widoczna";
            this.columnHeaderVisible.Width = 60;
            this.columnHeaderTransparency.Text = "Przezrocz.";
            this.columnHeaderTransparency.Width = 68;
            this.columnHeaderCount.Text = "Obiektów";
            this.columnHeaderCount.Width = 60;

            // btnAllVisible
            this.btnAllVisible.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAllVisible.Location = new System.Drawing.Point(12, 448);
            this.btnAllVisible.Name = "btnAllVisible";
            this.btnAllVisible.Size = new System.Drawing.Size(116, 24);
            this.btnAllVisible.TabIndex = 31;
            this.btnAllVisible.Text = "Wszystkie widoczne";
            this.btnAllVisible.UseVisualStyleBackColor = true;
            this.btnAllVisible.Click += new System.EventHandler(this.btnAllVisible_Click);

            // btnAllHidden
            this.btnAllHidden.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAllHidden.Location = new System.Drawing.Point(134, 448);
            this.btnAllHidden.Name = "btnAllHidden";
            this.btnAllHidden.Size = new System.Drawing.Size(108, 24);
            this.btnAllHidden.TabIndex = 32;
            this.btnAllHidden.Text = "Wszystkie ukryte";
            this.btnAllHidden.UseVisualStyleBackColor = true;
            this.btnAllHidden.Click += new System.EventHandler(this.btnAllHidden_Click);

            // button1 (Menadżer Faz)
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(248, 448);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(112, 24);
            this.button1.TabIndex = 21;
            this.button1.Text = "Menadżer Faz";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.btnOpenPhaseManager_Click);

            // button2 (Odśwież widok)
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button2.Location = new System.Drawing.Point(366, 448);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(124, 24);
            this.button2.TabIndex = 22;
            this.button2.Text = "Odśwież widok";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.btnRefreshTeklaView_Click);

            // btnSavePreset
            this.btnSavePreset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSavePreset.Location = new System.Drawing.Point(12, 478);
            this.btnSavePreset.Name = "btnSavePreset";
            this.btnSavePreset.Size = new System.Drawing.Size(160, 24);
            this.btnSavePreset.TabIndex = 33;
            this.btnSavePreset.Text = "Zapisz preset";
            this.btnSavePreset.UseVisualStyleBackColor = true;
            this.btnSavePreset.Click += new System.EventHandler(this.btnSavePreset_Click);

            // btnLoadPreset
            this.btnLoadPreset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnLoadPreset.Location = new System.Drawing.Point(178, 478);
            this.btnLoadPreset.Name = "btnLoadPreset";
            this.btnLoadPreset.Size = new System.Drawing.Size(160, 24);
            this.btnLoadPreset.TabIndex = 34;
            this.btnLoadPreset.Text = "Wczytaj preset";
            this.btnLoadPreset.UseVisualStyleBackColor = true;
            this.btnLoadPreset.Click += new System.EventHandler(this.btnLoadPreset_Click);

            // label1
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 510);
            this.label1.Name = "label1";
            this.label1.TabIndex = 23;
            this.label1.Text = "2026.05.12 Freeware";

            // label2
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(390, 510);
            this.label2.Name = "label2";
            this.label2.TabIndex = 24;
            this.label2.Text = "DRAFTCON.PL";

            // statusStrip1
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.statusLabelConnection,
                this.statusLabelModel });
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.TabIndex = 35;

            // statusLabelConnection
            this.statusLabelConnection.Name = "statusLabelConnection";
            this.statusLabelConnection.Text = "● Rozłączony";

            // statusLabelModel
            this.statusLabelModel.Name = "statusLabelModel";
            this.statusLabelModel.Spring = true;
            this.statusLabelModel.Text = "";
            this.statusLabelModel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            // Form1
            this.ClientSize = new System.Drawing.Size(502, 530);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnLoadPreset);
            this.Controls.Add(this.btnSavePreset);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btnAllHidden);
            this.Controls.Add(this.btnAllVisible);
            this.Controls.Add(this.listViewPhases);
            this.Controls.Add(this.txtFilter);
            this.Controls.Add(this.btnPin);
            this.Controls.Add(this.btnLoad);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.MinimumSize = new System.Drawing.Size(502, 570);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Menedżer Faz TPM";
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.Button btnPin;
        private System.Windows.Forms.TextBox txtFilter;
        private System.Windows.Forms.ListView listViewPhases;
        private System.Windows.Forms.ColumnHeader columnHeaderNum;
        private System.Windows.Forms.ColumnHeader columnHeaderName;
        private System.Windows.Forms.ColumnHeader columnHeaderComment;
        private System.Windows.Forms.ColumnHeader columnHeaderColor;
        private System.Windows.Forms.ColumnHeader columnHeaderVisible;
        private System.Windows.Forms.ColumnHeader columnHeaderTransparency;
        private System.Windows.Forms.ColumnHeader columnHeaderCount;
        private System.Windows.Forms.Button btnAllVisible;
        private System.Windows.Forms.Button btnAllHidden;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button btnSavePreset;
        private System.Windows.Forms.Button btnLoadPreset;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel statusLabelConnection;
        private System.Windows.Forms.ToolStripStatusLabel statusLabelModel;
    }
}
