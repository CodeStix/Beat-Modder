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

        public FormMain()
        {
            progress = new Progress<ProgressReport>(HandleProgressChange);

            InitializeComponent();

           
        }

        public string GetBeatSaberFile(string name)
        {
            return Path.Combine(config.beatSaberLocation, name);
        }




        private async void RunBeatSaberAndExit()
        {
            await beatSaber.RunIPA(revert: false, launch: true, wait: config.showConsole, shown: config.showConsole);

            SelfDestruct();
        }

        private Task progressBarDoneDelayTask = Task.CompletedTask;

        private void ProgressChange(string status, float progress = 1f)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new MethodInvoker(() => ProgressChange(status, progress)));
                return;
            }

            Console.WriteLine($"[ProgressChange] { status } ({ (progress * 100f) }%)");

            progressBar.Value = (int)(progress * 100f);

            if (progress == 1f || progressBarDoneDelayTask.IsCompleted)
                statusLabel.Text = "Status: " + status;

            if (progress <= 0.25f)
                progressBar.Visible = true;

            if (progress >= 1f && progressBarDoneDelayTask.IsCompleted)
            {
                progressBarDoneDelayTask = Task.Delay(1000).ContinueWith((e) =>
                {
                    BeginInvoke(new MethodInvoker(() => progressBar.Visible = false));//progressBar.Value < 100
                });
            }
        }

        private void HandleProgressChange(ProgressReport pr)
            => ProgressChange(pr.status, pr.progress);

        private void FormMain_Load(object sender, EventArgs e)
        {
            linkLabelAbout.LinkArea = new LinkArea(0, 0);
            linkLabelAbout.Links.Add(16, 8, @"https://github.com/CodeStix");
            linkLabelAbout.Links.Add(39, 13, @"https://beatmods.com");

            labelVersion.Text = $"Version: { StringUtil.GetCurrentVersion(2) }";

            checkBoxAllowNonApproved.Enabled = Properties.Settings.Default.AllowUnApproved;

            ProgressChange("Loading files...", 0.2f);

            LoadConfig();
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
                bool updateAvailable = await updateCheck.CheckForUpdate(StringUtil.GetCurrentVersion(2));

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
                ProgressChange("Communicating with beatmods...", 0.5f);
                beatMods = await BeatMods.CreateSession(true);

                ProgressChange("Gathering game information...", 0.8f);
                beatSaber = new BeatSaberInstallation(config.beatSaberLocation, beatMods);

                UpdateModList();

                /*if (!beatSaber.IsIPAInstalled)
                {
                    ProgressChange("Getting ready...", 0.8f);

                    if (MessageBox.Show("Do you want to mod Beat Saber right now?\n" +
                        "The core modding components will get installed, these are needed for all mods to function.\n" +
                        "You can undo all the mods at any time in the settings tab.", "Let's mod?", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK)
                    {
                        ProgressChange("Patching Beat Saber...", 0.8f);
                        await beatSaber.InstallIPA(progress);
                    }
                }*/

                ProgressChange("List of mods has been refreshed.", 1f);

                if (beatSaber.DidBeatSaberUpdate)
                {
                    MessageBox.Show($"Good news!" +
                        $"\nThere was a Beat Saber update: { beatSaber.BeatSaberVersion }\n" +
                        $"This means that some of the mods have to get updated too.\n" +
                        $"If the update was released recently, be aware that some mods could be broken.\n\n" +
                        $"At any moment you can open this program to automatically repatch, check for and install mod updates!", "Oh snap!", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    ProgressChange("Patching Beat Saber...", 0.5f);

                    await beatSaber.RunIPA(revert: false, wait: config.showConsole, shown: config.showConsole);

                    ProgressChange("Patched Beat Saber!", 1f);
                }

                if (config.autoUpdate)
                    await CheckForAndInstallModUpdates();
                else
                    ProgressChange($"Mod updates on startup are disabled in settings.", 1f);

                UpdateModList();

                BeginInvoke(new Action(() =>
                {
                    labelBeatSaberType.Text = "Beat Saber type: " + beatSaber.BeatSaberType;
                    textBoxBeatSaberLocation.Text = config.beatSaberLocation;
                    labelBeatSaberVersion.ForeColor = Color.Green;
                    labelBeatSaberVersion.Text = $"Beat Saber version: { beatSaber.BeatSaberVersion }";
                }));

                config.firstTime = false;
                SaveConfig();
            }
            ));
        }

        private Task<int> CheckForAndInstallModUpdates()
        {
            return Task.Run(async () =>
            {
                ProgressChange($"Checking for mod updates...", 0f);

                List<KeyValuePair<LocalMod, Mod>> outDatedMods = beatSaber.EnumerateOutdatedMods().ToList();
                int updatedCount = 0;

                for (int i = 0; i < outDatedMods.Count; i++)
                {
                    LocalMod oldVersion = outDatedMods[i].Key;
                    Mod newVersion = outDatedMods[i].Value;

                    if (null != await beatSaber.UpdateMod(oldVersion, newVersion, ProgressReport.Partial(progress, (float)i / outDatedMods.Count * (1f / outDatedMods.Count), 1f / outDatedMods.Count)))
                        updatedCount++;
                }

                if (updatedCount > 0)
                    ProgressChange($"{ updatedCount } mods were updated succesfully!", 1f);
                else if (outDatedMods.Count == 0)
                    ProgressChange($"All mods are up-to-date!", 1f);
                else
                    ProgressChange($"There was a problem updating 1 mod.", 1f);

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

                bool installed = selected.Tag is LocalMod;
                contextMenu.Items[3].Enabled = !installed || (installed && beatMods.IsOutdated((LocalMod)selected.Tag, beatSaber.BeatSaberVersion));
                contextMenu.Items[4].Enabled = installed;
            }
        }

        private async void installOrUpdateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Mod m = (Mod)selected.Tag;
            ProgressChange($"Installing mod { m.ToString() }...", 0f);

            LocalMod installedMod = beatSaber.GetInstalledModIgnoreVersion(m);

            if (installedMod != null)
            {
                if (SemVersion.Parse(installedMod.Version) < m.Version)
                {
                    if (await beatSaber.UpdateMod(installedMod, m, progress) != null)
                        UpdateModList();
                }
            }
            else
            {
                if (await beatSaber.InstallMod(m, progress) != null)
                    UpdateModList();
            }
        }

        private async void uninstallToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LocalMod mod = (LocalMod)selected.Tag;

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

            if (selected.Tag is LocalMod)
                m = beatMods.GetModFromLocal((LocalMod)selected.Tag);
            else
                m = (Mod)selected.Tag;

            FormModInfo fmi = new FormModInfo(m);

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

            int installedCount = await InstallMultipleMods(mods);
            if (installedCount > 0)
            {
                UpdateModList();
                ShowNotification($"{ installedCount } mods were installed into Beat Saber successfully.");
            }
        }

        private async Task<int> InstallMultipleMods(List<Mod> mods)
        {
            int installedCount = 0;

            for (int i = 0; i < mods.Count; i++)
            {
                Mod m = mods[i];

                if (await beatSaber.InstallMod(m, ProgressReport.Partial(progress, i * (1f / listView.CheckedItems.Count), 1f / listView.CheckedItems.Count)) != null)
                    installedCount++;
            }

            return installedCount;
        }

        private void buttonMoreInfo_Click(object sender, EventArgs e)
        {
            if (selected == null)
                return;

            FormModInfo fmi = new FormModInfo((Mod)selected.Tag);

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

            Process.Start(Application.ExecutablePath);
            SelfDestruct();
        }

        private void buttonChangeBeatSaberType_Click(object sender, EventArgs e)
        {
            FormListSelect fls = new FormListSelect("Please select where you got Beat Saber from:", "Change type...", "Steam", "Oculus Store");

            if (fls.ShowDialog() == DialogResult.OK)
            {
                if ((string)fls.Result == "Steam")
                {
                    beatSaber.BeatSaberType = ModDownloadType.Steam;
                    SaveConfig();
                }
                else if ((string)fls.Result == "Oculus Store")
                {
                    beatSaber.BeatSaberType = ModDownloadType.Oculus;
                    SaveConfig();
                }
                else
                {
                    beatSaber.BeatSaberType = ModDownloadType.Universal;
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

                await Task.Delay(1000);

                SelfDestruct();
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
            Mod s = e.Item.Tag as Mod;

            selected = e.Item;

            if (s != null && selected != null)
            {
                textBoxDescription.Text = $"{ s.Name }\r\n\tby { s.author.username }\r\n\r\n{ s.description }\r\n\r\nCategory: { s.Category.ToString() }";
            }

            buttonMoreInfo.Enabled = selected != null;
        }

        private void listView_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            Mod s = e.Item.Tag as Mod;

            if (s == null || beatSaber.IsInstalledAnyVersion(s))
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
            config.showConsole = checkBoxConsole.Checked;
            SaveConfig();
        }

        private async void checkBoxAllowNonApproved_CheckedChanged(object sender, EventArgs e)
        {
            config.allowNonApproved = checkBoxAllowNonApproved.Checked;
            config.autoUpdate = false;
            SaveConfig();

            await beatMods.RefreshMods(true, !checkBoxAllowNonApproved.Checked);

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

            await beatMods.RefreshMods(true, !config.allowNonApproved);
            await CheckForAndInstallModUpdates();
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

        [Obsolete("Use HandleProgressChange() instead.")]
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
        }

        private void UpdateModList()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new MethodInvoker(UpdateModList));
                return;
            }

            listView.Items.Clear();
            Point p = listView.AutoScrollOffset;

            foreach(LocalMod localMod in beatSaber.InstalledMods.OrderBy((e) => e.usedBy.Count))
            {
                Mod mostRecentMod = beatMods.GetMostRecentModWithName(localMod.Name, beatSaber.BeatSaberVersion);
                Mod mod = beatMods.GetModFromLocal(localMod);

                ListViewItem lvi = new ListViewItem(new string[] { mod.Name, mod.author.username, mod.Version, mod.description });
                lvi.Group = listView.GetOrCreateGroup("Installed");
                lvi.Tag = localMod;
                lvi.ImageKey = mod.Category.ToString() + ".ico";

                FontStyle fontStyle = localMod.usedBy.Count > 0 ? FontStyle.Regular : FontStyle.Bold;

                if (mostRecentMod == null || !mod.IsCompatibleWith(beatSaber.BeatSaberVersion)) // This mod requires an update
                {
                    lvi.SubItems[0].Text += " (Incompatible)";
                    lvi.ForeColor = Color.DarkOrange;
                    lvi.Font = new Font(FontFamily.GenericSansSerif, 8.5f, fontStyle);
                }
                else if (mostRecentMod.Version == localMod.Version) // This mod is up-to-date
                {
                    lvi.ForeColor = Color.ForestGreen;
                    lvi.Font = new Font(FontFamily.GenericSansSerif, 8.5f, fontStyle);
                }
                else // This mod is out of date
                {
                    lvi.SubItems[0].Text += " (Outdated)";
                    lvi.ForeColor = Color.DarkRed;
                    lvi.Font = new Font(FontFamily.GenericSansSerif, 8.5f, fontStyle);
                }

                listView.Items.Add(lvi);
            }

            foreach (Mod m in beatMods
                .GetModsSortedBy(BeatModsSort.None)
                .OnlyKeepMostRecentMods()
                .OnlyKeepCompatibleWith(beatSaber.BeatSaberVersion)
                .Where((e) => !beatSaber.IsInstalledExactVersion(e)))
            {
                ListViewItem lvi = new ListViewItem(new string[] { m.Name, m.author.username, m.Version, m.description });
                lvi.Group = listView.GetOrCreateGroup(m.Category.ToString());
                lvi.Tag = m;
                lvi.ImageKey = m.Category.ToString() + ".ico";

                if (m.required)
                    lvi.BackColor = Color.WhiteSmoke;

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
            config.keepModArchives = checkBoxKeepModDownloads.Checked;
            SaveConfig();

            if (config.keepModArchives && Directory.Exists(beatSaber.ModArchivesDownloadLocation))
            {
                string[] archives = Directory.GetFiles(beatSaber.ModArchivesDownloadLocation);

                if (archives.Length > 0 && MessageBox.Show("The program will from now on remove every mod archive after it was installed." +
                    "You will not be able to install/uninstall mods while offline from now on. " +
                    "You can re-enable this feature at any time.\n" +
                    "Do you wish to keep all past mod archives?",
                    "Remove archives?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    ProgressChange("Removing old mod archives...", 0f);

                    Directory.Delete(beatSaber.ModArchivesDownloadLocation, true);

                    ProgressChange($"Removed { archives.Length } archives.", 1f);
                }
            }
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
        public bool keepModArchives = false;
    }
}
