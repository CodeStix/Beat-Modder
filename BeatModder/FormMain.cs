using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Flurl;
using Newtonsoft.Json;
using Ookii.Dialogs.WinForms;
using Semver;
using Stx.BeatModsAPI;

namespace Stx.BeatModder
{
    public partial class FormMain : Form
    {
        public BeatSaberInstallation beatSaber;
        public BeatMods beatMods;
        public Progress<ProgressReport> progress;

        private const string CONFIG_FILE = @"config.json";

        private ListViewItem selected = null;
        private Config config;

        private GitHubBasedUpdateCheck updateCheck = new GitHubBasedUpdateCheck("CodeStix", "Beat-Modder", "Installer/latestVersion.txt");

        private Task progressBarDoneDelayTask = Task.CompletedTask;

        public FormMain()
        {
            progress = new Progress<ProgressReport>(HandleProgressChange);

            InitializeComponent();
        }


        private void ProgressChange(string status, float progress = 1f)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new MethodInvoker(() => ProgressChange(status, progress)));
                return;
            }

            Console.WriteLine($"[ProgressChange] { status } ({ (progress * 100f) }%)");

            progressBar.Value = Math.Min(100, (int)(progress * 100f));

            if (progress == 1f || progressBarDoneDelayTask.IsCompleted)
            {
                statusLabel.Text = "Status: " + status;
            }

            if (progress <= 0.25f)
            {
                progressBar.Visible = true;
                SetUI(false);
            }

            if (progress >= 1f && progressBarDoneDelayTask.IsCompleted)
            {
                progressBarDoneDelayTask = Task.Delay(1000).ContinueWith((e) =>
                {
                    BeginInvoke(new MethodInvoker(() =>
                    {
                        progressBar.Visible = false;
                        SetUI(true);
                    }));
                });
            }
        }

        private void SetUI(bool enabled)
        {
            if(InvokeRequired)
            {
                BeginInvoke(new MethodInvoker(() => SetUI(enabled)));
                return;
            }

            buttonInstall.Enabled = enabled;
            contextMenu.Enabled = enabled;
            groupBoxAdvanced.Enabled = enabled;
            groupBoxBeatSaber.Enabled = enabled;
            groupBoxDangerZone.Enabled = enabled;
            buttonCheckForUpdatesNow.Enabled = enabled;
            buttonPlayAndExit.Enabled = enabled;
        }

        private void HandleProgressChange(ProgressReport pr)
            => ProgressChange(pr.status, pr.progress);

        private void FormMain_Load(object sender, EventArgs e)
        {
            linkLabelAbout.LinkArea = new LinkArea(0, 0);
            linkLabelAbout.Links.Add(16, 8, @"https://github.com/CodeStix");
            linkLabelAbout.Links.Add(39, 13, @"https://beatmods.com");

            labelVersion.Text = $"Version: { StringUtil.GetCurrentVersion(3) }";

            checkBoxAllowNonApproved.Enabled = Properties.Settings.Default.AllowUnApproved;

            ProgressChange("Loading files...", 0.1f);

            LoadConfig();
            config.allowNonApproved &= Properties.Settings.Default.AllowUnApproved;

            checkBoxConsole.Checked = config.showConsole;
            checkBoxAllowNonApproved.Checked = config.allowNonApproved;
            checkBoxAutoUpdate.Checked = config.autoUpdate;
            checkBoxKeepModDownloads.Checked = config.keepModArchives;

            if (!config.firstTime)
                Size = config.windowSize;

            if (string.IsNullOrEmpty(config.beatSaberLocation) || 
                !File.Exists(Path.Combine(config.beatSaberLocation, "Beat Saber.exe")))
            {
                string[] searchLocationPaths = Properties.Resources.locations.Split('\n');
                List<string> searchLocations = new List<string>();

                foreach (DriveInfo d in DriveInfo.GetDrives())
                    foreach (string s in searchLocationPaths)
                        searchLocations.Add(d.Name + s.Trim());

                FormFindFile fff = new FormFindFile(@"Beat Saber.exe", searchLocations, "Please select the location of " +
                    "Beat Saber you would like to mod. If the right Beat Saber installation is not listed, please use " +
                    "the 'Browse' button to locate it yourself.");

                if (fff.ShowDialog() != DialogResult.OK)
                    SelfDestruct();

                config.beatSaberLocation = fff.SelectedDirectory;
                SaveConfig();
            }

            Task.Run(async () =>
            {
                bool updateAvailable = await updateCheck.CheckForUpdate(StringUtil.GetCurrentVersion(3));
                if (updateAvailable)
                {
                    if (MessageBox.Show("There is an update available for Beat Modder, please download the newest version " +
                        "to ensure that everything can work correctly. Go to the download page right now?", "An update!", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                    {
                        Process.Start(@"https://github.com/CodeStix/Beat-Modder/releases");

                        SelfDestruct();
                    }
                }
            });

            Task.Run(new Action(async () =>
            {
                ProgressChange("Checking for Beat Saber process...", 0.15f);

                if (BeatSaberInstallation.IsBeatSaberRunning)
                {
                    if (MessageBox.Show("It looks like Beat Saber is still running, to install or uninstall " +
                        "mods, Beat Saber must be closed.\nDo you want to close Beat Saber right now?\n" +
                        "You can press the play button in the bottom right corner to restart Beat Saber.",
                        "Beat Saber is running", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.No)
                    {
                        SelfDestruct();
                    }

                    BeatSaberInstallation.KillBeatSaberProcess();
                }

                ProgressChange("Communicating with beatmods...", 0.2f);
                beatMods = await BeatMods.CreateSession(true);

                ProgressChange("Gathering game information...", 0.3f);
                beatSaber = new BeatSaberInstallation(config.beatSaberLocation, beatMods)
                {
                    RemoveModArchivesAfterInstall = !config.keepModArchives
                };

                ProgressChange("Checking for manual installs/uninstalls...", 0.35f);

                beatSaber.DetectManualModInstallOrUninstall(out List<Mod> wasManuallyInstalled, out List<InstalledMod> wasManuallyUninstalled);
                await beatSaber.InstallMultipleMods(wasManuallyInstalled, ProgressReport.Partial(progress, 0.35f, 0.1f));
                await beatSaber.UninstallMultipleMods(wasManuallyUninstalled, true, ProgressReport.Partial(progress, 0.45f, 0.1f));

                UpdateModList();

                ProgressChange("List of mods has been refreshed.", 0.55f);

                if (beatSaber.DidBeatSaberUpdate)
                {
                    MessageBox.Show($"Good news!" +
                        $"\nThere was a Beat Saber update: { beatSaber.BeatSaberVersionString }\n" +
                        $"This means that some of the mods have to get updated too.\n" +
                        $"If the update was released recently, be aware that some mods could be broken.\n\n" +
                        $"At any moment you can open this program to automatically repatch, check for and install mod updates!", "Oh snap!", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    ProgressChange("Patching Beat Saber...", 0.55f);

                    await beatSaber.RunIPA(revert: false, wait: config.showConsole, shown: config.showConsole);

                    ProgressChange("Patched Beat Saber!", 0.6f);
                }

                if (config.autoUpdate)
                    await CheckForAndInstallModUpdates(ProgressReport.Partial(progress, 0.6f, 0.4f));
                else
                    ProgressChange($"Mod updates on startup are disabled in settings.", 1f);

                UpdateModList();

                BeginInvoke(new Action(() =>
                {
                    labelBeatSaberType.Text = "Beat Saber type: " + beatSaber.BeatSaberType;
                    textBoxBeatSaberLocation.Text = config.beatSaberLocation;
                    labelBeatSaberVersion.ForeColor = Color.Green;
                    labelBeatSaberVersion.Text = $"Beat Saber version: { beatSaber.BeatSaberVersionString }";
                }));

                config.firstTime = false;
                SaveConfig();

                /*while (beatMods.IsOffline)
                {
                    await Task.Delay(3000);

                    Console.WriteLine("Checking if BeatMods is reachable...");
                    if (BeatModsUrlBuilder.IsReachable)
                    {
                        Console.WriteLine("BeatMods is available, creating new session...");

                        await Task.Delay(1000);

                        beatMods.Dispose();
                        beatMods = await BeatMods.CreateSession(true);

                        UpdateModList();
                        ProgressChange("BeatMods went available, list was refreshed.", 1f);
                    }
                }*/
            }
            ));
        }

        private Task<int> CheckForAndInstallModUpdates(IProgress<ProgressReport> progress = null)
        {
            return Task.Run(async () =>
            {
                progress?.Report(new ProgressReport($"Checking for mod updates...", 0f));

                List<KeyValuePair<InstalledMod, Mod>> outDatedMods = beatSaber.EnumerateOutdatedMods().ToList();
                int updatedCount = 0;

                for (int i = 0; i < outDatedMods.Count; i++)
                {
                    InstalledMod oldVersion = outDatedMods[i].Key;
                    Mod newVersion = outDatedMods[i].Value;

                    if (null != await beatSaber.UpdateMod(oldVersion, newVersion, ProgressReport.Partial(progress, (float)i / outDatedMods.Count * (1f / outDatedMods.Count), 1f / outDatedMods.Count)))
                        updatedCount++;
                }

                if (updatedCount > 0)
                    progress?.Report(new ProgressReport($"{ updatedCount } mods were updated succesfully!", 1f));
                else if (outDatedMods.Count == 0)
                    progress?.Report(new ProgressReport($"All mods are up-to-date!", 1f));
                else
                    progress?.Report(new ProgressReport($"There was a problem updating mods.", 1f));

                return updatedCount;
            });
        }

        private void FormMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            config.windowSize = Size;

            SaveConfig();
        }

        private void listView_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && selected != null)
            {
                contextMenu.Show(listView, e.Location);
                contextMenu.Items[0].Text = selected.SubItems[0].Text;

                bool installed = selected.Tag is InstalledMod;
                contextMenu.Items[3].Enabled = !installed || (installed && beatMods.IsOutdated((InstalledMod)selected.Tag, beatSaber.BeatSaberVersion));
                contextMenu.Items[4].Enabled = installed;
            }
        }

        private async void installOrUpdateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (selected.Tag is InstalledMod installedMod)
            {
                Mod newest = beatMods.GetMostRecentModWithName(installedMod.Name, onlyApproved: !checkBoxAllowNonApproved.Checked);
                if (newest != null && SemVersion.Parse(installedMod.Version) < newest.Version)
                {
                    ProgressChange($"Updating mod { installedMod }...", 0f);

                    if (await beatSaber.UpdateMod(installedMod, newest, progress) != null)
                        UpdateModList();
                }
            }
            else if (selected.Tag is Mod mod)
            {
                ProgressChange($"Installing mod { mod.ToString() }...", 0f);

                if (await beatSaber.InstallMod(mod, progress) != null)
                    UpdateModList();
            }
        }

        private async void uninstallToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InstalledMod mod = (InstalledMod)selected.Tag;

            if (mod.preventRemoval)
            {
                MessageBox.Show($"Removal of '{ mod.ToString() }' was canceled because " +
                    $"this plugin is required for all mods to work.\nIf you want to remove all mods, please go to the settings tab.", "Uninstall canceled.",
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (mod.usedBy.Count > 0)
            {
                MessageBox.Show($"You are willing to remove '{ mod.ToString() }', please not that this mod is currently being used by { mod.usedBy.Count } other mods:\n\n" +
                    string.Join("\n", mod.usedBy) +
                    $"\n\nYou must first uninstall the mods above to succeed uninstalling this mod!", "Uninstall canceled.", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            ProgressChange($"Removing mod { mod.ToString() }...", 0f);

            if (await beatSaber.UninstallMod(mod, false, progress))
            {
                ProgressChange($"Removal of { mod.ToString() } succeeded.", 1f);

                UpdateModList();
                ShowNotification($"The mod { mod.ToString() } was successfully removed from Beat Saber.");
            }
            else
            {
                ProgressChange($"Removal of { mod.ToString() } failed!", 1f);
            }
        }

        private void viewInformationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Mod m;

            if (selected.Tag is InstalledMod)
                m = beatMods.GetModFromLocal((InstalledMod)selected.Tag);
            else
                m = (Mod)selected.Tag;

            FormModInfo fmi = new FormModInfo(m, selected.Tag as InstalledMod);

            fmi.Show(this);
        }

        private async void buttonInstall_Click(object sender, EventArgs e)
        {
            if (listView.CheckedItems.Count <= 0)
            {
                MessageBox.Show("You want to install nothing? Please check the boxes in front of the mods " +
                    "you want to install in the list and then press this button again.",
                    "That isn't working...", MessageBoxButtons.OK, MessageBoxIcon.Question);

                return;
            }

            List<Mod> mods = new List<Mod>();
            foreach (ListViewItem m in listView.CheckedItems)
                mods.Add((Mod)m.Tag);

            await beatSaber.InstallMultipleMods(mods, progress);
            UpdateModList();
        }



        private void buttonMoreInfo_Click(object sender, EventArgs e)
        {
            if (selected == null)
                return;

            FormModInfo fmi = new FormModInfo(selected.Tag as Mod, selected.Tag as InstalledMod);

            fmi.Show(this);
        }

        private void buttonChangeBeatSaberLocation_Click(object sender, EventArgs e)
        {
            string[] searchLocationPaths = Properties.Resources.locations.Split('\n');
            List<string> searchLocations = new List<string>();

            foreach (DriveInfo d in DriveInfo.GetDrives())
                foreach (string s in searchLocationPaths)
                    searchLocations.Add(d.Name + s.Trim());

            FormFindFile fff = new FormFindFile(@"Beat Saber.exe", searchLocations, "Please select the location of " +
                "Beat Saber you would like to mod. If the right Beat Saber installation is not listed, please use " +
                "the 'Browse' button to locate it yourself.");

            if (fff.ShowDialog() != DialogResult.OK)
                return;

            config.beatSaberLocation = fff.SelectedDirectory;
            SaveConfig();

            RestartAndSelfDestruct();
        }

        private void buttonChangeBeatSaberType_Click(object sender, EventArgs e)
        {
            FormListSelect fls = new FormListSelect("Please select where you got Beat Saber from:", "Change type...", "Steam", "Oculus Store");

            if (fls.ShowDialog() == DialogResult.OK)
            {
                if ((string)fls.Result == "Steam")
                {
                    beatSaber.BeatSaberType = BeatSaberInstalledType.Steam;
                    SaveConfig();
                }
                else if ((string)fls.Result == "Oculus Store")
                {
                    beatSaber.BeatSaberType = BeatSaberInstalledType.Oculus;
                    SaveConfig();
                }
                else
                {
                    beatSaber.BeatSaberType = BeatSaberInstalledType.Universal;
                    SaveConfig();
                }

                labelBeatSaberType.Text = "Beat Saber type: " + beatSaber.BeatSaberType;
            }
        }

        private async void buttonRemoveMods_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you very sure you want to remove all Beat Saber mods?\n" +
                "Data like custom songs, custom saber,... will be kept.", "Living on the edge.", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation) == DialogResult.Yes)
            {
                ProgressChange("Uninstalling all mods...", 0f);

                await beatSaber.UninstallAllMods(config.showConsole, progress);

                ProgressChange("All mods were removed!", 1f);

                ShowNotification($"All Beat Saber mods were removed successfully.");

                UpdateModList();

                /*await Task.Delay(1000);
                SelfDestruct();*/
            }
        }

        private async void buttonRemoveModsAndData_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you very VERY sure you want to remove all Beat Saber mods and mod data?\n" +
                "Data like custom songs, custom saber,... will also get removed.", "Living on the edge.", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation) == DialogResult.Yes)
            {
                ProgressChange("Uninstalling all mods with data...", 0f);

                await beatSaber.UninstallAllModsAndData();

                ProgressChange("All mods with data were removed!", 1f);

                ShowNotification($"All Beat Saber mods and data were removed successfully.");

                await Task.Delay(1000);

                SelfDestruct();
            }
        }

        private void listView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            selected = e.Item;
            buttonMoreInfo.Enabled = selected != null;

            if (selected == null)
                return;

            IMod selectedMod = (IMod)e.Item.Tag;
            Mod mod = selectedMod is Mod ? (Mod)selectedMod : beatMods.GetModFromLocal((InstalledMod)selectedMod);

            textBoxDescription.Text = $"{ selectedMod.Name }\r\n\tby { mod?.author.username }\r\n\r\n{ mod?.description }\r\n\r\nCategory: { mod?.Category.ToString() }";
        }

        private void listView_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            Mod selectedMod = e.Item.Tag as Mod;

            if (selectedMod == null || beatSaber.IsInstalledAnyVersion(selectedMod))
            {
                e.Item.Checked = false;
            }
        }

        private void linkLabelCodeStix_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(@"https://github.com/CodeStix");
        }

        private void linkLabelBeatMods_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(@"https://beatmods.com");
        }

        private void linkLabelDiscord_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(@"https://discord.gg/beatsabermods");
        }

        private void linkLabelAbout_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(e.Link.LinkData.ToString());
        }

        private void checkBoxConsole_CheckedChanged(object sender, EventArgs e)
        {
            if (beatSaber == null)
                return;

            config.showConsole = checkBoxConsole.Checked;
            SaveConfig();
        }

        private async void checkBoxAllowNonApproved_CheckedChanged(object sender, EventArgs e)
        {
            if (beatMods == null)
                return;

            config.allowNonApproved = checkBoxAllowNonApproved.Checked;
            config.autoUpdate = false;
            SaveConfig();

            await beatMods.RefreshMods(true);

            UpdateModList();
            BeginInvoke(new Action(() => checkBoxAutoUpdate.Checked = false));
        }

        private void checkBoxAutoUpdate_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxAutoUpdate.Checked && config.allowNonApproved)
            {
                if (MessageBox.Show("Are you sure you want to enable auto update with NOT-approved mods?\n" +
                    "This can be very game breaking!", "Uhmmm?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation) != DialogResult.Yes)

                {
                    checkBoxAutoUpdate.Checked = false;

                    return;
                }
            }

            config.autoUpdate = checkBoxAutoUpdate.Checked;
            SaveConfig();
        }

        private async void buttonCheckForUpdatesNow_Click(object sender, EventArgs e)
        {
            if (config.allowNonApproved)
            {
                if (MessageBox.Show("Are you sure you want to check for and install mods updates with NOT-approved mods?\n" +
                    "This can be very game breaking!", "Uhmmm?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation) != DialogResult.Yes)
                {
                    return;
                }
            }

            await beatMods.RefreshMods(true);
            await CheckForAndInstallModUpdates(progress);

            UpdateModList();
        }

        private void RestartAndSelfDestruct()
        {
            Process.Start(Application.ExecutablePath);
            SelfDestruct();
        }

        private void SelfDestruct()
        {
            Environment.Exit(0);
            while (true) ;
        }

        private void ShowNotification(string text)
        {
            BeginInvoke(new Action(() =>
            {
                notifyIcon.Visible = true;
                notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
                notifyIcon.Icon = SystemIcons.Information;
                notifyIcon.BalloonTipText = text;
                notifyIcon.ShowBalloonTip(5000);
            }));
        }

        /*[Obsolete("Use HandleProgressChange() instead.")]
        public void SetStatus(string status, bool done)
        {
            BeginInvoke(new Action(() =>
            {
                statusLabel.Text = "Status: " + status;

                progressBar.Style = done ? System.Windows.Forms.ProgressBarStyle.Continuous : System.Windows.Forms.ProgressBarStyle.Marquee;
                progressBar.Visible = !done;

                buttonPlayAndExit.Enabled = done;
                contextMenu.Enabled = done;
                buttonInstall.Enabled = done;
                buttonMoreInfo.Enabled = done;
                buttonCheckForUpdatesNow.Enabled = done;
                groupBoxAdvanced.Enabled = done;
                groupBoxBeatSaber.Enabled = done;
                groupBoxDangerZone.Enabled = done;
            }));
        }*/

        private void UpdateModList()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new MethodInvoker(UpdateModList));
                return;
            }

            listView.Items.Clear();
            Point p = listView.AutoScrollOffset;

            foreach(InstalledMod localMod in beatSaber.InstalledMods.OrderBy((e) => e.usedBy.Count))
            {
                Mod mod = beatMods.GetModFromLocal(localMod);

                ListViewItem lvi = new ListViewItem(new string[]
                {
                    localMod.Name,
                    mod?.author.username,
                    localMod.Version,
                    localMod.GameVersion,
                    mod?.description
                });

                lvi.Group = listView.GetOrCreateGroup("Installed");
                lvi.Tag = localMod;
                lvi.ImageKey = mod?.Category.ToString() ?? ModCategory.Other.ToString();
                lvi.ForeColor = Color.ForestGreen;
              
                FontStyle fontStyle = localMod.usedBy.Count > 0 ? FontStyle.Regular : FontStyle.Bold;
                lvi.Font = new Font(FontFamily.GenericSansSerif, 8.5f, fontStyle);

                if (!beatMods.IsOffline) // Update information is unavailable offline
                {
                    Mod mostRecentMod = beatMods.GetMostRecentModWithName(localMod.Name, beatSaber.BeatSaberVersion, mod?.Status == ModStatus.Approved);
                    if (mostRecentMod == null) // There is no mod available that is compatible with the current BeatSaber version, awaiting update...
                    {
                        lvi.SubItems[0].Text += " (Waiting for update)";
                        lvi.ForeColor = Color.DarkOrange;
                    }
                    else if (localMod.GetVersion() < mostRecentMod.Version)
                    {
                        lvi.SubItems[0].Text += " (Update available)";
                        lvi.ForeColor = Color.DarkRed;
                    }
                    else if (localMod.GetVersion() > mostRecentMod.Version)
                    {
                        lvi.SubItems[0].Text += " (From the future)";
                        lvi.ForeColor = Color.ForestGreen;
                    }
                }

                if (mod != null && mod.Status != ModStatus.Approved)
                {
                    lvi.SubItems[0].Text += $" ({ mod.Status.ToString() })";
                    lvi.ForeColor = Color.Purple;
                }

                listView.Items.Add(lvi);
            }

            foreach (Mod m in beatMods
                .GetModsSortedBy(BeatModsSort.None)
                .OnlyKeepStatus(checkBoxAllowNonApproved.Checked ? ModStatus.All : ModStatus.Approved)
                .OnlyKeepMostRecentMods()
                .OnlyKeepCompatibleWith(beatSaber.BeatSaberVersion)
                .Where((e) => !beatSaber.IsInstalledExactVersion(e)))
            {
                ListViewItem lvi = new ListViewItem(new string[] 
                {
                    m.Name,
                    m.author.username,
                    m.Version,
                    m.GameVersion,
                    m.description
                });
                lvi.Group = listView.GetOrCreateGroup(m.Category.ToString());
                lvi.Tag = m;
                lvi.ImageKey = m.Category.ToString();

                if (m.Status != ModStatus.Approved)
                {
                    lvi.SubItems[0].Text += $" ({ m.Status.ToString() })";
                    lvi.ForeColor = Color.Purple;
                }

                if (m.required)
                    lvi.BackColor = Color.WhiteSmoke;

                if (beatMods.OfflineMods.Any((e) => e == m))
                    lvi.Font = new Font(FontFamily.GenericSansSerif, 8.5f, FontStyle.Italic);

                listView.Items.Add(lvi);
            }

            listView.AutoScrollOffset = p;
        }

        private void buttonPlayAndExit_Click(object sender, EventArgs e)
        {
            RunBeatSaberAndExit();
        }

        private void LoadConfig()
        {
            try
            {
                if (!File.Exists(CONFIG_FILE))
                {
                    config = new Config();
                }
                else
                {
                    config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(CONFIG_FILE));
                }
            }
            catch
            {
                MessageBox.Show("Yikes, could not load required files.\nMaybe run as administrator?", "Could not load config...", MessageBoxButtons.OK, MessageBoxIcon.Error);

                SelfDestruct();
            }
        }

        private async void RunBeatSaberAndExit()
        {
            ProgressChange("Starting Beat Saber in patched state...", 0.4f);

            if (!await beatSaber.RunIPA(revert: false, launch: true, wait: config.showConsole, shown: config.showConsole))
            {
                ProgressChange("Starting Beat Saber in normal state...", 0.6f);

                Process.Start(beatSaber.BeatSaberDotExe);
            }

            ProgressChange("Beat Saber is running.", 1f);

            SelfDestruct();
        }

        private void SaveConfig()
        {
            try
            {
                File.WriteAllText(CONFIG_FILE, JsonConvert.SerializeObject(config));
            }
            catch
            { }
        }

        private void CheckBoxKeepModDownloads_CheckedChanged(object sender, EventArgs e)
        {
            if (beatSaber == null)
                return;

            config.keepModArchives = checkBoxKeepModDownloads.Checked;
            SaveConfig();

            if (!config.keepModArchives && Directory.Exists(beatSaber.ModArchivesDownloadLocation))
            {
                string[] archives = Directory.GetFiles(beatSaber.ModArchivesDownloadLocation);

                if (archives.Length > 0 && MessageBox.Show("The program will from now on remove every mod archive after it was installed." +
                    "You will not be able to install/uninstall mods while offline from now on. " +
                    "You can re-enable this feature at any time.\n" +
                    "Do you wish to keep all past mod archives?",
                    "Remove archives?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    ProgressChange("Removing old mod archives...", 0f);

                    Directory.Delete(beatSaber.ModArchivesDownloadLocation, true);
                    beatMods.SaveOfflineModCache();

                    ProgressChange($"Removed { archives.Length } archives.", 1f);
                }
            }
        }

        private void FormMain_Resize(object sender, EventArgs e)
        {
            listView.Columns[listView.Columns.Count - 1].Width = 460 + Width - 1057;
        }

        private void ButtonListOfflineMods_Click(object sender, EventArgs e)
        {
            FormListSelect fls = new FormListSelect("The following mods are offline available:", "Offline mods", beatMods.OfflineMods.ToArray());
            fls.ReadOnly = true;
            fls.ShowDialog();
            
        }
    }

    [Serializable]
    internal class Config
    {
        public Size windowSize;
        public string beatSaberLocation;
        public bool showConsole = false;
        public bool allowNonApproved = false;
        public bool autoUpdate = true;
        public bool firstTime = true;
        public bool keepModArchives = true;
    }
}
