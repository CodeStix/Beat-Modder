namespace Stx.BeatModder
{
    partial class FormModInfo
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.buttonOk = new System.Windows.Forms.Button();
            this.linkLabel = new System.Windows.Forms.LinkLabel();
            this.treeViewAdvanced = new System.Windows.Forms.TreeView();
            this.textBoxDescription = new System.Windows.Forms.TextBox();
            this.textBoxName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonAdvancedInfo = new System.Windows.Forms.Button();
            this.labelVersion = new System.Windows.Forms.Label();
            this.textBoxAdvancedSelected = new System.Windows.Forms.TextBox();
            this.buttonDirectDownload = new System.Windows.Forms.Button();
            this.groupBox = new System.Windows.Forms.GroupBox();
            this.textBoxBinaryFile = new System.Windows.Forms.TextBox();
            this.labelGameVersion = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // buttonOk
            // 
            this.buttonOk.Location = new System.Drawing.Point(318, 178);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(64, 29);
            this.buttonOk.TabIndex = 2;
            this.buttonOk.Text = "Ok";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // linkLabel
            // 
            this.linkLabel.AutoSize = true;
            this.linkLabel.Location = new System.Drawing.Point(9, 186);
            this.linkLabel.Name = "linkLabel";
            this.linkLabel.Size = new System.Drawing.Size(109, 13);
            this.linkLabel.TabIndex = 3;
            this.linkLabel.TabStop = true;
            this.linkLabel.Text = "Go to reference link...";
            this.linkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel_LinkClicked);
            // 
            // treeViewAdvanced
            // 
            this.treeViewAdvanced.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.treeViewAdvanced.FullRowSelect = true;
            this.treeViewAdvanced.Location = new System.Drawing.Point(12, 258);
            this.treeViewAdvanced.Name = "treeViewAdvanced";
            this.treeViewAdvanced.Size = new System.Drawing.Size(370, 229);
            this.treeViewAdvanced.TabIndex = 4;
            this.treeViewAdvanced.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView_AfterSelect);
            // 
            // textBoxDescription
            // 
            this.textBoxDescription.BackColor = System.Drawing.SystemColors.Menu;
            this.textBoxDescription.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBoxDescription.Location = new System.Drawing.Point(12, 64);
            this.textBoxDescription.Multiline = true;
            this.textBoxDescription.Name = "textBoxDescription";
            this.textBoxDescription.ReadOnly = true;
            this.textBoxDescription.Size = new System.Drawing.Size(373, 90);
            this.textBoxDescription.TabIndex = 5;
            this.textBoxDescription.Text = "Description";
            // 
            // textBoxName
            // 
            this.textBoxName.BackColor = System.Drawing.SystemColors.Menu;
            this.textBoxName.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBoxName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxName.Location = new System.Drawing.Point(12, 6);
            this.textBoxName.Multiline = true;
            this.textBoxName.Name = "textBoxName";
            this.textBoxName.ReadOnly = true;
            this.textBoxName.Size = new System.Drawing.Size(300, 52);
            this.textBoxName.TabIndex = 6;
            this.textBoxName.Text = "Mod name\r\nby test\r\n";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 242);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(113, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Advanced information:";
            // 
            // buttonAdvancedInfo
            // 
            this.buttonAdvancedInfo.Location = new System.Drawing.Point(231, 178);
            this.buttonAdvancedInfo.Name = "buttonAdvancedInfo";
            this.buttonAdvancedInfo.Size = new System.Drawing.Size(81, 29);
            this.buttonAdvancedInfo.TabIndex = 8;
            this.buttonAdvancedInfo.Text = "Advanced \\/";
            this.buttonAdvancedInfo.UseVisualStyleBackColor = true;
            this.buttonAdvancedInfo.Click += new System.EventHandler(this.buttonAdvancedInfo_Click);
            // 
            // labelVersion
            // 
            this.labelVersion.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelVersion.Location = new System.Drawing.Point(312, 6);
            this.labelVersion.Name = "labelVersion";
            this.labelVersion.Size = new System.Drawing.Size(73, 23);
            this.labelVersion.TabIndex = 9;
            this.labelVersion.Text = "v1.0";
            this.labelVersion.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textBoxAdvancedSelected
            // 
            this.textBoxAdvancedSelected.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxAdvancedSelected.Location = new System.Drawing.Point(12, 493);
            this.textBoxAdvancedSelected.Name = "textBoxAdvancedSelected";
            this.textBoxAdvancedSelected.ReadOnly = true;
            this.textBoxAdvancedSelected.Size = new System.Drawing.Size(370, 20);
            this.textBoxAdvancedSelected.TabIndex = 10;
            // 
            // buttonDirectDownload
            // 
            this.buttonDirectDownload.Location = new System.Drawing.Point(231, 229);
            this.buttonDirectDownload.Name = "buttonDirectDownload";
            this.buttonDirectDownload.Size = new System.Drawing.Size(151, 23);
            this.buttonDirectDownload.TabIndex = 11;
            this.buttonDirectDownload.Text = "Direct mod download...";
            this.buttonDirectDownload.UseVisualStyleBackColor = true;
            this.buttonDirectDownload.Click += new System.EventHandler(this.buttonDirectDownload_Click);
            // 
            // groupBox
            // 
            this.groupBox.Location = new System.Drawing.Point(12, 212);
            this.groupBox.Name = "groupBox";
            this.groupBox.Size = new System.Drawing.Size(370, 10);
            this.groupBox.TabIndex = 12;
            this.groupBox.TabStop = false;
            // 
            // textBoxBinaryFile
            // 
            this.textBoxBinaryFile.BackColor = System.Drawing.SystemColors.Menu;
            this.textBoxBinaryFile.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBoxBinaryFile.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxBinaryFile.Location = new System.Drawing.Point(12, 160);
            this.textBoxBinaryFile.Multiline = true;
            this.textBoxBinaryFile.Name = "textBoxBinaryFile";
            this.textBoxBinaryFile.ReadOnly = true;
            this.textBoxBinaryFile.Size = new System.Drawing.Size(370, 17);
            this.textBoxBinaryFile.TabIndex = 13;
            // 
            // labelGameVersion
            // 
            this.labelGameVersion.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelGameVersion.Location = new System.Drawing.Point(312, 29);
            this.labelGameVersion.Name = "labelGameVersion";
            this.labelGameVersion.Size = new System.Drawing.Size(73, 23);
            this.labelGameVersion.TabIndex = 14;
            this.labelGameVersion.Text = "game v1.0 ";
            this.labelGameVersion.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // FormModInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(394, 522);
            this.Controls.Add(this.labelGameVersion);
            this.Controls.Add(this.textBoxBinaryFile);
            this.Controls.Add(this.groupBox);
            this.Controls.Add(this.buttonDirectDownload);
            this.Controls.Add(this.textBoxAdvancedSelected);
            this.Controls.Add(this.labelVersion);
            this.Controls.Add(this.buttonAdvancedInfo);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxName);
            this.Controls.Add(this.textBoxDescription);
            this.Controls.Add(this.treeViewAdvanced);
            this.Controls.Add(this.linkLabel);
            this.Controls.Add(this.buttonOk);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FormModInfo";
            this.Text = "More mod information...";
            this.Load += new System.EventHandler(this.FormModInfo_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.LinkLabel linkLabel;
        private System.Windows.Forms.TreeView treeViewAdvanced;
        private System.Windows.Forms.TextBox textBoxDescription;
        private System.Windows.Forms.TextBox textBoxName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonAdvancedInfo;
        private System.Windows.Forms.Label labelVersion;
        private System.Windows.Forms.TextBox textBoxAdvancedSelected;
        private System.Windows.Forms.Button buttonDirectDownload;
        private System.Windows.Forms.GroupBox groupBox;
        private System.Windows.Forms.TextBox textBoxBinaryFile;
        private System.Windows.Forms.Label labelGameVersion;
    }
}