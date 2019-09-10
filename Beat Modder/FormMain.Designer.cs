namespace Stx.BeatModder
{
    partial class FormMain
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.buttonPlayAndExit = new System.Windows.Forms.ToolStripDropDownButton();
            this.buttonCheckForUpdatesNow = new System.Windows.Forms.ToolStripDropDownButton();
            this.labelBeatSaberVersion = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.progressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPageMods = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonInstall = new System.Windows.Forms.Button();
            this.buttonMoreInfo = new System.Windows.Forms.Button();
            this.listView = new System.Windows.Forms.ListView();
            this.columnHeaderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderAuthor = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderVersion = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderInfo = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.imageListListIcons = new System.Windows.Forms.ImageList(this.components);
            this.textBoxDescription = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tabPageSettings = new System.Windows.Forms.TabPage();
            this.groupBoxAdvanced = new System.Windows.Forms.GroupBox();
            this.checkBoxAutoUpdate = new System.Windows.Forms.CheckBox();
            this.checkBoxAllowNonApproved = new System.Windows.Forms.CheckBox();
            this.checkBoxConsole = new System.Windows.Forms.CheckBox();
            this.groupBoxDangerZone = new System.Windows.Forms.GroupBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.buttonRemoveModsAndData = new System.Windows.Forms.Button();
            this.buttonRemoveMods = new System.Windows.Forms.Button();
            this.groupBoxBeatSaber = new System.Windows.Forms.GroupBox();
            this.buttonChangeBeatSaberLocation = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxBeatSaberLocation = new System.Windows.Forms.TextBox();
            this.buttonChangeBeatSaberType = new System.Windows.Forms.Button();
            this.labelBeatSaberType = new System.Windows.Forms.Label();
            this.tabPageAbout = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.labelVersion = new System.Windows.Forms.Label();
            this.linkLabelDiscord = new System.Windows.Forms.LinkLabel();
            this.panelAbout = new System.Windows.Forms.Panel();
            this.linkLabelAbout = new System.Windows.Forms.LinkLabel();
            this.contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.itemNameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.viewInformationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.installOrUpdateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.uninstallToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.statusStrip.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.tabPageMods.SuspendLayout();
            this.tableLayoutPanel.SuspendLayout();
            this.tabPageSettings.SuspendLayout();
            this.groupBoxAdvanced.SuspendLayout();
            this.groupBoxDangerZone.SuspendLayout();
            this.groupBoxBeatSaber.SuspendLayout();
            this.tabPageAbout.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.panelAbout.SuspendLayout();
            this.contextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonPlayAndExit,
            this.buttonCheckForUpdatesNow,
            this.labelBeatSaberVersion,
            this.progressBar,
            this.statusLabel});
            this.statusStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.statusStrip.Location = new System.Drawing.Point(0, 635);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.statusStrip.Size = new System.Drawing.Size(1041, 22);
            this.statusStrip.TabIndex = 0;
            this.statusStrip.Text = "statusStrip";
            // 
            // buttonPlayAndExit
            // 
            this.buttonPlayAndExit.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.buttonPlayAndExit.Image = global::Stx.BeatModder.Properties.Resources.play;
            this.buttonPlayAndExit.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonPlayAndExit.Name = "buttonPlayAndExit";
            this.buttonPlayAndExit.ShowDropDownArrow = false;
            this.buttonPlayAndExit.Size = new System.Drawing.Size(107, 20);
            this.buttonPlayAndExit.Text = "Play Beat Saber";
            this.buttonPlayAndExit.ToolTipText = "Play Beat Saber and close this program.";
            this.buttonPlayAndExit.Click += new System.EventHandler(this.buttonPlayAndExit_Click);
            // 
            // buttonCheckForUpdatesNow
            // 
            this.buttonCheckForUpdatesNow.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.buttonCheckForUpdatesNow.Image = ((System.Drawing.Image)(resources.GetObject("buttonCheckForUpdatesNow.Image")));
            this.buttonCheckForUpdatesNow.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonCheckForUpdatesNow.Name = "buttonCheckForUpdatesNow";
            this.buttonCheckForUpdatesNow.ShowDropDownArrow = false;
            this.buttonCheckForUpdatesNow.Size = new System.Drawing.Size(160, 20);
            this.buttonCheckForUpdatesNow.Text = "Check for mod updates...";
            this.buttonCheckForUpdatesNow.ToolTipText = "Check for mod updates right now...";
            this.buttonCheckForUpdatesNow.Click += new System.EventHandler(this.buttonCheckForUpdatesNow_Click);
            // 
            // labelBeatSaberVersion
            // 
            this.labelBeatSaberVersion.ForeColor = System.Drawing.Color.Maroon;
            this.labelBeatSaberVersion.Name = "labelBeatSaberVersion";
            this.labelBeatSaberVersion.Size = new System.Drawing.Size(118, 17);
            this.labelBeatSaberVersion.Text = "Beat Saber version: ...";
            // 
            // statusLabel
            // 
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(77, 17);
            this.statusLabel.Text = "Status: Ready";
            // 
            // progressBar
            // 
            this.progressBar.MarqueeAnimationSpeed = 10;
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(150, 16);
            this.progressBar.Visible = false;
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabPageMods);
            this.tabControl.Controls.Add(this.tabPageSettings);
            this.tabControl.Controls.Add(this.tabPageAbout);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(1041, 635);
            this.tabControl.TabIndex = 1;
            // 
            // tabPageMods
            // 
            this.tabPageMods.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.tabPageMods.Controls.Add(this.tableLayoutPanel);
            this.tabPageMods.Location = new System.Drawing.Point(4, 22);
            this.tabPageMods.Name = "tabPageMods";
            this.tabPageMods.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageMods.Size = new System.Drawing.Size(1033, 609);
            this.tabPageMods.TabIndex = 0;
            this.tabPageMods.Text = "Mods";
            // 
            // tableLayoutPanel
            // 
            this.tableLayoutPanel.ColumnCount = 4;
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 73F));
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 341F));
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 220F));
            this.tableLayoutPanel.Controls.Add(this.label1, 0, 3);
            this.tableLayoutPanel.Controls.Add(this.buttonInstall, 3, 3);
            this.tableLayoutPanel.Controls.Add(this.buttonMoreInfo, 3, 2);
            this.tableLayoutPanel.Controls.Add(this.listView, 0, 0);
            this.tableLayoutPanel.Controls.Add(this.textBoxDescription, 3, 0);
            this.tableLayoutPanel.Controls.Add(this.label3, 2, 3);
            this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel.Name = "tableLayoutPanel";
            this.tableLayoutPanel.RowCount = 4;
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 90.36144F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 9.638555F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 39F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel.Size = new System.Drawing.Size(1027, 603);
            this.tableLayoutPanel.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.Gray;
            this.label1.Location = new System.Drawing.Point(3, 563);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(384, 39);
            this.label1.TabIndex = 6;
            this.label1.Text = resources.GetString("label1.Text");
            // 
            // buttonInstall
            // 
            this.buttonInstall.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonInstall.Location = new System.Drawing.Point(810, 566);
            this.buttonInstall.Name = "buttonInstall";
            this.buttonInstall.Size = new System.Drawing.Size(214, 34);
            this.buttonInstall.TabIndex = 1;
            this.buttonInstall.Text = "Install Checked &Mods";
            this.buttonInstall.UseVisualStyleBackColor = true;
            this.buttonInstall.Click += new System.EventHandler(this.buttonInstall_Click);
            // 
            // buttonMoreInfo
            // 
            this.buttonMoreInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonMoreInfo.Enabled = false;
            this.buttonMoreInfo.Location = new System.Drawing.Point(810, 516);
            this.buttonMoreInfo.Name = "buttonMoreInfo";
            this.buttonMoreInfo.Size = new System.Drawing.Size(214, 44);
            this.buttonMoreInfo.TabIndex = 4;
            this.buttonMoreInfo.Text = "More info...\r\n";
            this.buttonMoreInfo.UseVisualStyleBackColor = true;
            this.buttonMoreInfo.Click += new System.EventHandler(this.buttonMoreInfo_Click);
            // 
            // listView
            // 
            this.listView.AllowColumnReorder = true;
            this.listView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listView.CheckBoxes = true;
            this.listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderName,
            this.columnHeaderAuthor,
            this.columnHeaderVersion,
            this.columnHeaderInfo});
            this.tableLayoutPanel.SetColumnSpan(this.listView, 3);
            this.listView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView.FullRowSelect = true;
            this.listView.GridLines = true;
            this.listView.HideSelection = false;
            this.listView.Location = new System.Drawing.Point(3, 3);
            this.listView.Name = "listView";
            this.tableLayoutPanel.SetRowSpan(this.listView, 3);
            this.listView.ShowItemToolTips = true;
            this.listView.Size = new System.Drawing.Size(801, 557);
            this.listView.SmallImageList = this.imageListListIcons;
            this.listView.TabIndex = 0;
            this.listView.UseCompatibleStateImageBehavior = false;
            this.listView.View = System.Windows.Forms.View.Details;
            this.listView.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.listView_ItemChecked);
            this.listView.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.listView_ItemSelectionChanged);
            this.listView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.listView_MouseClick);
            // 
            // columnHeaderName
            // 
            this.columnHeaderName.Text = "Name";
            this.columnHeaderName.Width = 150;
            // 
            // columnHeaderAuthor
            // 
            this.columnHeaderAuthor.Text = "Author";
            this.columnHeaderAuthor.Width = 70;
            // 
            // columnHeaderVersion
            // 
            this.columnHeaderVersion.Text = "Version";
            this.columnHeaderVersion.Width = 80;
            // 
            // columnHeaderInfo
            // 
            this.columnHeaderInfo.Text = "Description";
            this.columnHeaderInfo.Width = 500;
            // 
            // imageListListIcons
            // 
            this.imageListListIcons.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListListIcons.ImageStream")));
            this.imageListListIcons.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListListIcons.Images.SetKeyName(0, "Uncategorized.ico");
            this.imageListListIcons.Images.SetKeyName(1, "UIEnhancements.ico");
            this.imageListListIcons.Images.SetKeyName(2, "Core.ico");
            this.imageListListIcons.Images.SetKeyName(3, "Cosmetic.ico");
            this.imageListListIcons.Images.SetKeyName(4, "Gameplay.ico");
            this.imageListListIcons.Images.SetKeyName(5, "Installed.ico");
            this.imageListListIcons.Images.SetKeyName(6, "Libraries.ico");
            this.imageListListIcons.Images.SetKeyName(7, "Lighting.ico");
            this.imageListListIcons.Images.SetKeyName(8, "Other.png");
            this.imageListListIcons.Images.SetKeyName(9, "PracticeOrTraining.ico");
            this.imageListListIcons.Images.SetKeyName(10, "StreamingTools.ico");
            this.imageListListIcons.Images.SetKeyName(11, "Tools.ico");
            // 
            // textBoxDescription
            // 
            this.textBoxDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxDescription.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxDescription.Location = new System.Drawing.Point(810, 3);
            this.textBoxDescription.Multiline = true;
            this.textBoxDescription.Name = "textBoxDescription";
            this.tableLayoutPanel.SetRowSpan(this.textBoxDescription, 2);
            this.textBoxDescription.Size = new System.Drawing.Size(214, 507);
            this.textBoxDescription.TabIndex = 3;
            this.textBoxDescription.Text = "Press any mod to display information.";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(469, 563);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(290, 26);
            this.label3.TabIndex = 5;
            this.label3.Text = "To install/uninstall a mod, right click on any mod.\r\nTo install multiple, check t" +
    "he boxes and press the button  -->\r\n";
            // 
            // tabPageSettings
            // 
            this.tabPageSettings.BackColor = System.Drawing.SystemColors.Window;
            this.tabPageSettings.Controls.Add(this.groupBoxAdvanced);
            this.tabPageSettings.Controls.Add(this.groupBoxDangerZone);
            this.tabPageSettings.Controls.Add(this.groupBoxBeatSaber);
            this.tabPageSettings.Location = new System.Drawing.Point(4, 22);
            this.tabPageSettings.Name = "tabPageSettings";
            this.tabPageSettings.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageSettings.Size = new System.Drawing.Size(1033, 609);
            this.tabPageSettings.TabIndex = 1;
            this.tabPageSettings.Text = "Settings";
            // 
            // groupBoxAdvanced
            // 
            this.groupBoxAdvanced.Controls.Add(this.checkBoxAutoUpdate);
            this.groupBoxAdvanced.Controls.Add(this.checkBoxAllowNonApproved);
            this.groupBoxAdvanced.Controls.Add(this.checkBoxConsole);
            this.groupBoxAdvanced.Location = new System.Drawing.Point(243, 116);
            this.groupBoxAdvanced.Name = "groupBoxAdvanced";
            this.groupBoxAdvanced.Size = new System.Drawing.Size(202, 128);
            this.groupBoxAdvanced.TabIndex = 4;
            this.groupBoxAdvanced.TabStop = false;
            this.groupBoxAdvanced.Text = "Advanced";
            // 
            // checkBoxAutoUpdate
            // 
            this.checkBoxAutoUpdate.AutoSize = true;
            this.checkBoxAutoUpdate.Checked = true;
            this.checkBoxAutoUpdate.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxAutoUpdate.Location = new System.Drawing.Point(14, 46);
            this.checkBoxAutoUpdate.Name = "checkBoxAutoUpdate";
            this.checkBoxAutoUpdate.Size = new System.Drawing.Size(150, 17);
            this.checkBoxAutoUpdate.TabIndex = 5;
            this.checkBoxAutoUpdate.Text = "Auto-update mods on start\r\n";
            this.checkBoxAutoUpdate.UseVisualStyleBackColor = true;
            this.checkBoxAutoUpdate.CheckedChanged += new System.EventHandler(this.checkBoxAutoUpdate_CheckedChanged);
            // 
            // checkBoxAllowNonApproved
            // 
            this.checkBoxAllowNonApproved.AutoSize = true;
            this.checkBoxAllowNonApproved.Enabled = false;
            this.checkBoxAllowNonApproved.Location = new System.Drawing.Point(14, 65);
            this.checkBoxAllowNonApproved.Name = "checkBoxAllowNonApproved";
            this.checkBoxAllowNonApproved.Size = new System.Drawing.Size(175, 30);
            this.checkBoxAllowNonApproved.TabIndex = 4;
            this.checkBoxAllowNonApproved.Text = "Allow not-approved mods\r\n(warning: can break your game)";
            this.checkBoxAllowNonApproved.UseVisualStyleBackColor = true;
            this.checkBoxAllowNonApproved.CheckedChanged += new System.EventHandler(this.checkBoxAllowNonApproved_CheckedChanged);
            // 
            // checkBoxConsole
            // 
            this.checkBoxConsole.AutoSize = true;
            this.checkBoxConsole.Location = new System.Drawing.Point(14, 19);
            this.checkBoxConsole.Name = "checkBoxConsole";
            this.checkBoxConsole.Size = new System.Drawing.Size(137, 17);
            this.checkBoxConsole.TabIndex = 3;
            this.checkBoxConsole.Text = "Show patching console";
            this.checkBoxConsole.UseVisualStyleBackColor = true;
            this.checkBoxConsole.CheckedChanged += new System.EventHandler(this.checkBoxConsole_CheckedChanged);
            // 
            // groupBoxDangerZone
            // 
            this.groupBoxDangerZone.Controls.Add(this.label6);
            this.groupBoxDangerZone.Controls.Add(this.label5);
            this.groupBoxDangerZone.Controls.Add(this.buttonRemoveModsAndData);
            this.groupBoxDangerZone.Controls.Add(this.buttonRemoveMods);
            this.groupBoxDangerZone.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.groupBoxDangerZone.Location = new System.Drawing.Point(17, 116);
            this.groupBoxDangerZone.Name = "groupBoxDangerZone";
            this.groupBoxDangerZone.Size = new System.Drawing.Size(199, 128);
            this.groupBoxDangerZone.TabIndex = 2;
            this.groupBoxDangerZone.TabStop = false;
            this.groupBoxDangerZone.Text = "Danger Zone";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label6.Location = new System.Drawing.Point(6, 103);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(140, 13);
            this.label6.TabIndex = 4;
            this.label6.Text = "(but don\'t keep ANYTHING)";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label5.Location = new System.Drawing.Point(6, 46);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(138, 13);
            this.label5.TabIndex = 3;
            this.label5.Text = "(but keep custom songs, ...)";
            // 
            // buttonRemoveModsAndData
            // 
            this.buttonRemoveModsAndData.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.buttonRemoveModsAndData.Location = new System.Drawing.Point(6, 76);
            this.buttonRemoveModsAndData.Name = "buttonRemoveModsAndData";
            this.buttonRemoveModsAndData.Size = new System.Drawing.Size(185, 24);
            this.buttonRemoveModsAndData.TabIndex = 2;
            this.buttonRemoveModsAndData.Text = "Remove all mods and mod data\r\n\r\n";
            this.buttonRemoveModsAndData.UseVisualStyleBackColor = true;
            this.buttonRemoveModsAndData.Click += new System.EventHandler(this.buttonRemoveModsAndData_Click);
            // 
            // buttonRemoveMods
            // 
            this.buttonRemoveMods.ForeColor = System.Drawing.SystemColors.ControlText;
            this.buttonRemoveMods.Location = new System.Drawing.Point(6, 19);
            this.buttonRemoveMods.Name = "buttonRemoveMods";
            this.buttonRemoveMods.Size = new System.Drawing.Size(185, 24);
            this.buttonRemoveMods.TabIndex = 1;
            this.buttonRemoveMods.Text = "Remove all mods\r\n\r\n\r\n";
            this.buttonRemoveMods.UseVisualStyleBackColor = true;
            this.buttonRemoveMods.Click += new System.EventHandler(this.buttonRemoveMods_Click);
            // 
            // groupBoxBeatSaber
            // 
            this.groupBoxBeatSaber.Controls.Add(this.buttonChangeBeatSaberLocation);
            this.groupBoxBeatSaber.Controls.Add(this.label2);
            this.groupBoxBeatSaber.Controls.Add(this.textBoxBeatSaberLocation);
            this.groupBoxBeatSaber.Controls.Add(this.buttonChangeBeatSaberType);
            this.groupBoxBeatSaber.Controls.Add(this.labelBeatSaberType);
            this.groupBoxBeatSaber.Location = new System.Drawing.Point(17, 17);
            this.groupBoxBeatSaber.Name = "groupBoxBeatSaber";
            this.groupBoxBeatSaber.Size = new System.Drawing.Size(608, 79);
            this.groupBoxBeatSaber.TabIndex = 0;
            this.groupBoxBeatSaber.TabStop = false;
            // 
            // buttonChangeBeatSaberLocation
            // 
            this.buttonChangeBeatSaberLocation.Enabled = false;
            this.buttonChangeBeatSaberLocation.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonChangeBeatSaberLocation.Location = new System.Drawing.Point(567, 44);
            this.buttonChangeBeatSaberLocation.Name = "buttonChangeBeatSaberLocation";
            this.buttonChangeBeatSaberLocation.Size = new System.Drawing.Size(32, 23);
            this.buttonChangeBeatSaberLocation.TabIndex = 5;
            this.buttonChangeBeatSaberLocation.Text = "..";
            this.buttonChangeBeatSaberLocation.UseVisualStyleBackColor = true;
            this.buttonChangeBeatSaberLocation.Click += new System.EventHandler(this.buttonChangeBeatSaberLocation_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 49);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(103, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Beat Saber location:\r\n";
            // 
            // textBoxBeatSaberLocation
            // 
            this.textBoxBeatSaberLocation.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxBeatSaberLocation.Location = new System.Drawing.Point(115, 46);
            this.textBoxBeatSaberLocation.Name = "textBoxBeatSaberLocation";
            this.textBoxBeatSaberLocation.ReadOnly = true;
            this.textBoxBeatSaberLocation.Size = new System.Drawing.Size(446, 20);
            this.textBoxBeatSaberLocation.TabIndex = 3;
            // 
            // buttonChangeBeatSaberType
            // 
            this.buttonChangeBeatSaberType.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonChangeBeatSaberType.Location = new System.Drawing.Point(146, 17);
            this.buttonChangeBeatSaberType.Name = "buttonChangeBeatSaberType";
            this.buttonChangeBeatSaberType.Size = new System.Drawing.Size(32, 23);
            this.buttonChangeBeatSaberType.TabIndex = 2;
            this.buttonChangeBeatSaberType.Text = "..";
            this.buttonChangeBeatSaberType.UseVisualStyleBackColor = true;
            this.buttonChangeBeatSaberType.Click += new System.EventHandler(this.buttonChangeBeatSaberType_Click);
            // 
            // labelBeatSaberType
            // 
            this.labelBeatSaberType.AutoSize = true;
            this.labelBeatSaberType.Location = new System.Drawing.Point(6, 22);
            this.labelBeatSaberType.Name = "labelBeatSaberType";
            this.labelBeatSaberType.Size = new System.Drawing.Size(122, 13);
            this.labelBeatSaberType.TabIndex = 1;
            this.labelBeatSaberType.Text = "Beat Saber type: Oculus";
            // 
            // tabPageAbout
            // 
            this.tabPageAbout.BackColor = System.Drawing.SystemColors.Window;
            this.tabPageAbout.Controls.Add(this.tableLayoutPanel1);
            this.tabPageAbout.Location = new System.Drawing.Point(4, 22);
            this.tabPageAbout.Name = "tabPageAbout";
            this.tabPageAbout.Size = new System.Drawing.Size(1033, 609);
            this.tabPageAbout.TabIndex = 2;
            this.tabPageAbout.Text = "About";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 5;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.tableLayoutPanel1.Controls.Add(this.labelVersion, 2, 3);
            this.tableLayoutPanel1.Controls.Add(this.linkLabelDiscord, 2, 4);
            this.tableLayoutPanel1.Controls.Add(this.panelAbout, 1, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 5;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1033, 609);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // labelVersion
            // 
            this.labelVersion.AutoSize = true;
            this.labelVersion.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelVersion.Location = new System.Drawing.Point(311, 425);
            this.labelVersion.Name = "labelVersion";
            this.labelVersion.Size = new System.Drawing.Size(407, 91);
            this.labelVersion.TabIndex = 3;
            this.labelVersion.Text = "Version 1.0";
            this.labelVersion.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // linkLabelDiscord
            // 
            this.linkLabelDiscord.AutoSize = true;
            this.linkLabelDiscord.Dock = System.Windows.Forms.DockStyle.Fill;
            this.linkLabelDiscord.Location = new System.Drawing.Point(311, 516);
            this.linkLabelDiscord.Name = "linkLabelDiscord";
            this.linkLabelDiscord.Size = new System.Drawing.Size(407, 93);
            this.linkLabelDiscord.TabIndex = 4;
            this.linkLabelDiscord.TabStop = true;
            this.linkLabelDiscord.Text = "Join Beat Saber modding server on discord";
            this.linkLabelDiscord.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.linkLabelDiscord.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelDiscord_LinkClicked);
            // 
            // panelAbout
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.panelAbout, 3);
            this.panelAbout.Controls.Add(this.linkLabelAbout);
            this.panelAbout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelAbout.Location = new System.Drawing.Point(157, 185);
            this.panelAbout.Name = "panelAbout";
            this.panelAbout.Size = new System.Drawing.Size(715, 237);
            this.panelAbout.TabIndex = 2;
            // 
            // linkLabelAbout
            // 
            this.linkLabelAbout.DisabledLinkColor = System.Drawing.SystemColors.ControlText;
            this.linkLabelAbout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.linkLabelAbout.Location = new System.Drawing.Point(0, 0);
            this.linkLabelAbout.Name = "linkLabelAbout";
            this.linkLabelAbout.Size = new System.Drawing.Size(715, 237);
            this.linkLabelAbout.TabIndex = 0;
            this.linkLabelAbout.TabStop = true;
            this.linkLabelAbout.Text = "Beat Modder\r\nby CodeStix\r\n\r\npowered by beatmods.com\r\nby vanZeben";
            this.linkLabelAbout.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.linkLabelAbout.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelAbout_LinkClicked);
            // 
            // contextMenu
            // 
            this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.itemNameToolStripMenuItem,
            this.toolStripSeparator1,
            this.viewInformationToolStripMenuItem,
            this.installOrUpdateToolStripMenuItem,
            this.uninstallToolStripMenuItem});
            this.contextMenu.Name = "contextMenu";
            this.contextMenu.Size = new System.Drawing.Size(166, 98);
            // 
            // itemNameToolStripMenuItem
            // 
            this.itemNameToolStripMenuItem.Enabled = false;
            this.itemNameToolStripMenuItem.Name = "itemNameToolStripMenuItem";
            this.itemNameToolStripMenuItem.Size = new System.Drawing.Size(165, 22);
            this.itemNameToolStripMenuItem.Text = "Item Name";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(162, 6);
            // 
            // viewInformationToolStripMenuItem
            // 
            this.viewInformationToolStripMenuItem.Name = "viewInformationToolStripMenuItem";
            this.viewInformationToolStripMenuItem.Size = new System.Drawing.Size(165, 22);
            this.viewInformationToolStripMenuItem.Text = "View Information";
            this.viewInformationToolStripMenuItem.Click += new System.EventHandler(this.viewInformationToolStripMenuItem_Click);
            // 
            // installOrUpdateToolStripMenuItem
            // 
            this.installOrUpdateToolStripMenuItem.Name = "installOrUpdateToolStripMenuItem";
            this.installOrUpdateToolStripMenuItem.Size = new System.Drawing.Size(165, 22);
            this.installOrUpdateToolStripMenuItem.Text = "Install/Update";
            this.installOrUpdateToolStripMenuItem.Click += new System.EventHandler(this.installOrUpdateToolStripMenuItem_Click);
            // 
            // uninstallToolStripMenuItem
            // 
            this.uninstallToolStripMenuItem.Name = "uninstallToolStripMenuItem";
            this.uninstallToolStripMenuItem.Size = new System.Drawing.Size(165, 22);
            this.uninstallToolStripMenuItem.Text = "Uninstall";
            this.uninstallToolStripMenuItem.Click += new System.EventHandler(this.uninstallToolStripMenuItem_Click);
            // 
            // notifyIcon
            // 
            this.notifyIcon.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.notifyIcon.BalloonTipTitle = "Beat Modder Notification";
            this.notifyIcon.Text = "Beat Modder Notification";
            this.notifyIcon.Visible = true;
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1041, 657);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.statusStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(668, 377);
            this.Name = "FormMain";
            this.Text = "Beat Modder - A Beat Saber mod installer and remover";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormMain_FormClosed);
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.tabControl.ResumeLayout(false);
            this.tabPageMods.ResumeLayout(false);
            this.tableLayoutPanel.ResumeLayout(false);
            this.tableLayoutPanel.PerformLayout();
            this.tabPageSettings.ResumeLayout(false);
            this.groupBoxAdvanced.ResumeLayout(false);
            this.groupBoxAdvanced.PerformLayout();
            this.groupBoxDangerZone.ResumeLayout(false);
            this.groupBoxDangerZone.PerformLayout();
            this.groupBoxBeatSaber.ResumeLayout(false);
            this.groupBoxBeatSaber.PerformLayout();
            this.tabPageAbout.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.panelAbout.ResumeLayout(false);
            this.contextMenu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.ToolStripProgressBar progressBar;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPageMods;
        private System.Windows.Forms.TabPage tabPageSettings;
        private System.Windows.Forms.TabPage tabPageAbout;
        private System.Windows.Forms.ListView listView;
        private System.Windows.Forms.ColumnHeader columnHeaderName;
        private System.Windows.Forms.ColumnHeader columnHeaderAuthor;
        private System.Windows.Forms.ColumnHeader columnHeaderVersion;
        private System.Windows.Forms.ColumnHeader columnHeaderInfo;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
        private System.Windows.Forms.ContextMenuStrip contextMenu;
        private System.Windows.Forms.ToolStripMenuItem viewInformationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem installOrUpdateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem uninstallToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem itemNameToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.GroupBox groupBoxBeatSaber;
        private System.Windows.Forms.Label labelBeatSaberType;
        private System.Windows.Forms.Button buttonChangeBeatSaberType;
        private System.Windows.Forms.Button buttonChangeBeatSaberLocation;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxBeatSaberLocation;
        private System.Windows.Forms.GroupBox groupBoxDangerZone;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button buttonRemoveModsAndData;
        private System.Windows.Forms.Button buttonRemoveMods;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panelAbout;
        private System.Windows.Forms.Label labelVersion;
        private System.Windows.Forms.LinkLabel linkLabelDiscord;
        private System.Windows.Forms.LinkLabel linkLabelAbout;
        private System.Windows.Forms.GroupBox groupBoxAdvanced;
        private System.Windows.Forms.CheckBox checkBoxConsole;
        private System.Windows.Forms.CheckBox checkBoxAutoUpdate;
        private System.Windows.Forms.CheckBox checkBoxAllowNonApproved;
        private System.Windows.Forms.ToolStripDropDownButton buttonCheckForUpdatesNow;
        private System.Windows.Forms.ToolStripStatusLabel labelBeatSaberVersion;
        private System.Windows.Forms.ToolStripDropDownButton buttonPlayAndExit;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxDescription;
        private System.Windows.Forms.Button buttonInstall;
        private System.Windows.Forms.Button buttonMoreInfo;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ImageList imageListListIcons;
    }
}

