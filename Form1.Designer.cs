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
            this.columnHeaderNum = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderComment = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderColor = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderVisible = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderTransparency = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderCount = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btnAllVisible = new System.Windows.Forms.Button();
            this.btnAllHidden = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.btnSavePreset = new System.Windows.Forms.Button();
            this.btnLoadPreset = new System.Windows.Forms.Button();
            this.txtPresetFile = new System.Windows.Forms.TextBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.statusLabelConnection = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusLabelModel = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusLabelBrand = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnLoad
            // 
            this.btnLoad.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLoad.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLoad.Location = new System.Drawing.Point(339, 12);
            this.btnLoad.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(215, 27);
            this.btnLoad.TabIndex = 20;
            this.btnLoad.Text = "⟳  Load Phases";
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            // 
            // btnPin
            // 
            this.btnPin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPin.Location = new System.Drawing.Point(562, 12);
            this.btnPin.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnPin.Name = "btnPin";
            this.btnPin.Size = new System.Drawing.Size(27, 27);
            this.btnPin.TabIndex = 25;
            this.btnPin.Text = "📌";
            this.btnPin.Click += new System.EventHandler(this.btnPin_Click);
            // 
            // txtFilter
            // 
            this.txtFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFilter.Location = new System.Drawing.Point(12, 12);
            this.txtFilter.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtFilter.Name = "txtFilter";
            this.txtFilter.Size = new System.Drawing.Size(319, 27);
            this.txtFilter.TabIndex = 10;
            this.txtFilter.TextChanged += new System.EventHandler(this.txtFilter_TextChanged);
            // 
            // listViewPhases
            // 
            this.listViewPhases.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewPhases.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderNum,
            this.columnHeaderName,
            this.columnHeaderComment,
            this.columnHeaderColor,
            this.columnHeaderVisible,
            this.columnHeaderTransparency,
            this.columnHeaderCount});
            this.listViewPhases.FullRowSelect = true;
            this.listViewPhases.HideSelection = false;
            this.listViewPhases.Location = new System.Drawing.Point(12, 55);
            this.listViewPhases.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.listViewPhases.Name = "listViewPhases";
            this.listViewPhases.Size = new System.Drawing.Size(577, 419);
            this.listViewPhases.TabIndex = 14;
            this.listViewPhases.UseCompatibleStateImageBehavior = false;
            this.listViewPhases.View = System.Windows.Forms.View.Details;
            this.listViewPhases.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listViewPhases_ColumnClick);
            this.listViewPhases.SelectedIndexChanged += new System.EventHandler(this.listViewPhases_SelectedIndexChanged);
            this.listViewPhases.MouseClick += new System.Windows.Forms.MouseEventHandler(this.listViewPhases_MouseClick);
            // 
            // columnHeaderNum
            // 
            this.columnHeaderNum.Text = "No.";
            this.columnHeaderNum.Width = 40;
            // 
            // columnHeaderName
            // 
            this.columnHeaderName.Text = "Name";
            this.columnHeaderName.Width = 130;
            // 
            // columnHeaderComment
            // 
            this.columnHeaderComment.Text = "Comment";
            this.columnHeaderComment.Width = 155;
            // 
            // columnHeaderColor
            // 
            this.columnHeaderColor.Text = "Color";
            this.columnHeaderColor.Width = 55;
            // 
            // columnHeaderVisible
            // 
            this.columnHeaderVisible.Text = "Vis";
            this.columnHeaderVisible.Width = 36;
            // 
            // columnHeaderTransparency
            // 
            this.columnHeaderTransparency.Text = "Glass";
            this.columnHeaderTransparency.Width = 90;
            // 
            // columnHeaderCount
            // 
            this.columnHeaderCount.Text = "Objects";
            this.columnHeaderCount.Width = 58;
            // 
            // btnAllVisible
            // 
            this.btnAllVisible.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAllVisible.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAllVisible.Location = new System.Drawing.Point(12, 488);
            this.btnAllVisible.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnAllVisible.Name = "btnAllVisible";
            this.btnAllVisible.Size = new System.Drawing.Size(171, 27);
            this.btnAllVisible.TabIndex = 31;
            this.btnAllVisible.Text = "All Visible";
            this.btnAllVisible.Click += new System.EventHandler(this.btnAllVisible_Click);
            // 
            // btnAllHidden
            // 
            this.btnAllHidden.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAllHidden.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAllHidden.Location = new System.Drawing.Point(13, 525);
            this.btnAllHidden.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnAllHidden.Name = "btnAllHidden";
            this.btnAllHidden.Size = new System.Drawing.Size(169, 27);
            this.btnAllHidden.TabIndex = 32;
            this.btnAllHidden.Text = "All Hidden";
            this.btnAllHidden.Click += new System.EventHandler(this.btnAllHidden_Click);
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Location = new System.Drawing.Point(440, 488);
            this.button1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(149, 64);
            this.button1.TabIndex = 21;
            this.button1.Text = "Open Phase Manager";
            this.button1.Click += new System.EventHandler(this.btnOpenPhaseManager_Click);
            // 
            // btnSavePreset
            // 
            this.btnSavePreset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSavePreset.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSavePreset.Location = new System.Drawing.Point(311, 525);
            this.btnSavePreset.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnSavePreset.Name = "btnSavePreset";
            this.btnSavePreset.Size = new System.Drawing.Size(121, 27);
            this.btnSavePreset.TabIndex = 33;
            this.btnSavePreset.Text = "Save Preset";
            this.btnSavePreset.Click += new System.EventHandler(this.btnSavePreset_Click);
            // 
            // btnLoadPreset
            // 
            this.btnLoadPreset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLoadPreset.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLoadPreset.Location = new System.Drawing.Point(190, 525);
            this.btnLoadPreset.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnLoadPreset.Name = "btnLoadPreset";
            this.btnLoadPreset.Size = new System.Drawing.Size(113, 27);
            this.btnLoadPreset.TabIndex = 34;
            this.btnLoadPreset.Text = "Load Preset";
            this.btnLoadPreset.Click += new System.EventHandler(this.btnLoadPreset_Click);
            // 
            // txtPresetFile
            // 
            this.txtPresetFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPresetFile.Location = new System.Drawing.Point(190, 488);
            this.txtPresetFile.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtPresetFile.Name = "txtPresetFile";
            this.txtPresetFile.Size = new System.Drawing.Size(242, 27);
            this.txtPresetFile.TabIndex = 36;
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabelConnection,
            this.statusLabelModel,
            this.statusLabelBrand});
            this.statusStrip1.Location = new System.Drawing.Point(0, 569);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 18, 0);
            this.statusStrip1.Size = new System.Drawing.Size(602, 26);
            this.statusStrip1.TabIndex = 35;
            // 
            // statusLabelConnection
            // 
            this.statusLabelConnection.Name = "statusLabelConnection";
            this.statusLabelConnection.Size = new System.Drawing.Size(112, 20);
            this.statusLabelConnection.Text = "● Disconnected";
            // 
            // statusLabelModel
            // 
            this.statusLabelModel.Name = "statusLabelModel";
            this.statusLabelModel.Size = new System.Drawing.Size(470, 20);
            this.statusLabelModel.Spring = true;
            // 
            // statusLabelBrand
            // 
            this.statusLabelBrand.Name = "statusLabelBrand";
            this.statusLabelBrand.Size = new System.Drawing.Size(147, 20);
            this.statusLabelBrand.Text = "WWW.DRAFTCON.PL";
            this.statusLabelBrand.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(602, 595);
            this.Controls.Add(this.btnLoadPreset);
            this.Controls.Add(this.btnSavePreset);
            this.Controls.Add(this.txtPresetFile);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btnAllHidden);
            this.Controls.Add(this.btnAllVisible);
            this.Controls.Add(this.listViewPhases);
            this.Controls.Add(this.txtFilter);
            this.Controls.Add(this.btnPin);
            this.Controls.Add(this.btnLoad);
            this.Controls.Add(this.statusStrip1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MinimumSize = new System.Drawing.Size(620, 633);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Phase Manager TPM";
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
        private System.Windows.Forms.Button btnSavePreset;
        private System.Windows.Forms.Button btnLoadPreset;
        private System.Windows.Forms.TextBox txtPresetFile;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel statusLabelConnection;
        private System.Windows.Forms.ToolStripStatusLabel statusLabelModel;
        private System.Windows.Forms.ToolStripStatusLabel statusLabelBrand;
    }
}
