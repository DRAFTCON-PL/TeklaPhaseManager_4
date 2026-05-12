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
            this.listViewPhases = new System.Windows.Forms.ListView();
            this.columnHeaderNum = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderComment = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderColor = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderVisible = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderTransparency = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnLoad
            // 
            this.btnLoad.Location = new System.Drawing.Point(12, 12);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(320, 27);
            this.btnLoad.TabIndex = 20;
            this.btnLoad.Text = "Wczytaj fazy";
            this.btnLoad.UseVisualStyleBackColor = true;
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            // 
            // btnPin
            // 
            this.btnPin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPin.Location = new System.Drawing.Point(336, 12);
            this.btnPin.Name = "btnPin";
            this.btnPin.Size = new System.Drawing.Size(26, 27);
            this.btnPin.TabIndex = 25;
            this.btnPin.Text = "📌";
            this.btnPin.UseVisualStyleBackColor = false;
            this.btnPin.Click += new System.EventHandler(this.btnPin_Click);
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
            this.columnHeaderTransparency});
            this.listViewPhases.FullRowSelect = true;
            this.listViewPhases.GridLines = true;
            this.listViewPhases.HideSelection = false;
            this.listViewPhases.Location = new System.Drawing.Point(12, 45);
            this.listViewPhases.Name = "listViewPhases";
            this.listViewPhases.Size = new System.Drawing.Size(367, 383);
            this.listViewPhases.TabIndex = 14;
            this.listViewPhases.UseCompatibleStateImageBehavior = false;
            this.listViewPhases.View = System.Windows.Forms.View.Details;
            this.listViewPhases.SelectedIndexChanged += new System.EventHandler(this.listViewPhases_SelectedIndexChanged);
            this.listViewPhases.MouseClick += new System.Windows.Forms.MouseEventHandler(this.listViewPhases_MouseClick);
            // 
            // columnHeaderNum
            // 
            this.columnHeaderNum.Text = "Nr";
            this.columnHeaderNum.Width = 28;
            // 
            // columnHeaderName
            // 
            this.columnHeaderName.Text = "Nazwa";
            this.columnHeaderName.Width = 50;
            // 
            // columnHeaderComment
            // 
            this.columnHeaderComment.Text = "Komentarz";
            this.columnHeaderComment.Width = 97;
            // 
            // columnHeaderColor
            // 
            this.columnHeaderColor.Text = "Kolor";
            this.columnHeaderColor.Width = 48;
            // 
            // columnHeaderVisible
            // 
            this.columnHeaderVisible.Text = "Widoczna";
            this.columnHeaderVisible.Width = 55;
            // 
            // columnHeaderTransparency
            // 
            this.columnHeaderTransparency.Text = "Przezrocz.";
            this.columnHeaderTransparency.Width = 50;
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button1.Location = new System.Drawing.Point(12, 434);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(180, 24);
            this.button1.TabIndex = 21;
            this.button1.Text = "Otwórz Menadżer Faz";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.btnOpenPhaseManager_Click);
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button2.Location = new System.Drawing.Point(213, 434);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(164, 24);
            this.button2.TabIndex = 22;
            this.button2.Text = "Odśwież widok w Tekla";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.btnRefreshTeklaView_Click);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.SystemColors.Control;
            this.label1.Location = new System.Drawing.Point(44, 475);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(129, 16);
            this.label1.TabIndex = 23;
            this.label1.Text = "2026.05.12 Freeware";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(247, 475);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(101, 16);
            this.label2.TabIndex = 24;
            this.label2.Text = "DRAFTCON.PL";
            // 
            // Form1
            // 
            this.ClientSize = new System.Drawing.Size(389, 500);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.listViewPhases);
            this.Controls.Add(this.btnPin);
            this.Controls.Add(this.btnLoad);
            this.MinimumSize = new System.Drawing.Size(388, 539);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Menedżer Faz TPM";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.Button btnPin;
        private System.Windows.Forms.ListView listViewPhases;
        private System.Windows.Forms.ColumnHeader columnHeaderNum;
        private System.Windows.Forms.ColumnHeader columnHeaderName;
        private System.Windows.Forms.ColumnHeader columnHeaderComment;
        private System.Windows.Forms.ColumnHeader columnHeaderColor;
        private System.Windows.Forms.ColumnHeader columnHeaderVisible;
        private System.Windows.Forms.ColumnHeader columnHeaderTransparency;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
    }
}