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
using System.Threading.Tasks;
using System.Windows.Forms;
using Flurl;
using Newtonsoft.Json;
using Ookii.Dialogs.WinForms;

namespace Stx.BeatModder
{
    public partial class FormMain : Form
    {
        public const string ConfigFile = @"mods.json";
        public const string IPAExecutable = "IPA.exe";
        public const string BeatSaberExecutable = "Beat Saber.exe";
        public const string BeatSaberVersionFile = "BeatSaberVersion.txt";

        private ListViewItem selected = null;
        private Config config;
        private List<Mod> mods;

        private string ModListUrl
        {
            get
            {
                return "https://beatmods.com"
                    .AppendPathSegments("api", "v1", "mod")
                    .SetQueryParam("search", null, Flurl.NullValueHandling.Ignore)
                    .SetQueryParam("status", config.allowNonApproved ? null : "approved")
                    .SetQueryParam("sortDirection", 1);
            }
        }
        private string BeatSaberFile
        {
            get
            {
                return GetBeatSaberFile(BeatSaberExecutable);
            }
        }
        private string IPAFile
        {
            get
            {
                return GetBeatSaberFile(IPAExecutable);
            }
        }
        private bool BeatSaberFound
        {
            get
            {
                try
                {
                    return File.Exists(BeatSaberFile);
                }
                catch
                {
                    return false;
                }
            }
        }
        private bool IPAFound
        {
            get
            {
                if (!BeatSaberFound)
                    return false;

                try
                {
                    return File.Exists(IPAFile);
                }
                catch
                {
                    return false;
                }
            }
        }
        private bool BeatSaberVersionFileFound
        {
            get
            {
                if (!BeatSaberFound)
                    return false;

                try
                {
                    return File.Exists(GetBeatSaberFile(BeatSaberVersionFile));
                }
                catch
                {
                    return false;
                }
            }
        }
        private string BeatSaberVersion
        {
            get
            {
                try
                {
                    return File.ReadAllText(GetBeatSaberFile(BeatSaberVersionFile));
                }
                catch
                {
                    return null;
                }
            }
        }

        public FormMain()
        {
            InitializeComponent();

            linkLabelAbout.LinkArea = new LinkArea(0, 0);
            linkLabelAbout.Links.Add(16, 8, @"https://github.com/CodeStix");
            linkLabelAbout.Links.Add(39, 13, @"https://beatmods.com");

            labelVersion.Text = $"Version: { Assembly.GetExecutingAssembly().GetName().Version }";
        }

        public string GetBeatSaberFile(string name)
        {
            return Path.Combine(config.beatSaberLocation, name);
        }

        private void LoadConfig()
        {
            try
            {
                if (!File.Exists(ConfigFile))
                {
                    config = new Config();
                    //config.mods.Add(new Mod("Mod 1", "Stijn", "1.0", "Test mod 1", "0.13.1", ModInstallStatus.CanInstall, "https://mod.net"));
                }
                else
                {
                    config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigFile));
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
                File.WriteAllText(ConfigFile, JsonConvert.SerializeObject(config));
            }
            catch
            { }
        }

        private bool IsAnyVersionInstalled(Mod m)
        {
            return config.installedMods.Any((e) => e.Is(m));
        }

        private bool IsInstalled(Mod m)
        {
            return config.installedMods.Any((e) => e.IsExact(m));
        }

        private InstalledMod GetInstalledMod(Mod m)
        {
            return config.installedMods.FirstOrDefault((e) => e.Is(m));
        }

        private Mod GetNewestModWithName(string modName)
        {
            return mods.Where((e) => string.Compare(e.name, modName, StringComparison.OrdinalIgnoreCase) == 0)
                    .OrderByDescending((e) => StringUtil.StringVersionToNumber(e.version)).FirstOrDefault();
        }

        private IEnumerable<Mod> CheckForModUpdates()
        {
            foreach (Mod m in mods)
            {
                if (!IsAnyVersionInstalled(m))
                    continue;

                InstalledMod im = GetInstalledMod(m);

                if (StringUtil.StringVersionToNumber(im.version) < StringUtil.StringVersionToNumber(m.version))
                {
                    yield return m;
                }
            }
        }

        private async void RunBeatSaberAndExit()
        {
            await RunIPA(revert: false, launch: true, wait: config.showConsole, shown: config.showConsole);

            SelfDestruct();
        }

        private string FindBeatSaberLocation(out ModDownloadType beatSaberType)
        {
            SetStatus("Finding beat saber location...", false);

            beatSaberType = ModDownloadType.Universal;
            string[] searchLocationPaths = Properties.Resources.locations.Split('\n', '\r');
            List<string> searchLocations = new List<string>();
            List<string> foundLocations = new List<string>();

            foreach (DriveInfo d in DriveInfo.GetDrives())
                foreach (string s in searchLocationPaths)
                    searchLocations.Add(d.Name + s);

            Parallel.ForEach(searchLocations, (loc) =>
            {
                if (File.Exists(loc + @"\Beat Saber\" + BeatSaberExecutable))
                    foundLocations.Add(loc + @"\Beat Saber");
            });

            string actualPath = null;

            if (foundLocations.Count == 0)
            {
                SetStatus("Could not find beat saber location, please select manually...", false);

                FolderBrowserDialog ofd = new FolderBrowserDialog();
                ofd.Description = "Please select the location of your beat saber installation.";

                if (ofd.ShowDialog() != DialogResult.OK)
                {
                    SelfDestruct();
                }

                actualPath = ofd.SelectedPath;
            }
            else if (foundLocations.Count == 1)
            {
                SetStatus("Found beat saber location: " + foundLocations[0], true);

                actualPath = foundLocations[0];
            }
            else
            {
                SetStatus("Found multiple beat saber locations: " + string.Join(", ", foundLocations), true);

                FormListSelect fls = new FormListSelect("Multiple Beat Saber installations found, please select the one this program will be using",
                    "Select your Beat Saber installation...", foundLocations);

                if (fls.ShowDialog() != DialogResult.OK)
                {
                    SelfDestruct();
                }

                actualPath = (string)fls.Result;
            }

            beatSaberType = actualPath.ToLower().Contains("steam") ? ModDownloadType.Steam : ModDownloadType.Oculus;

            return actualPath;
        }


        private async Task DownloadModInformations()
        {
            SetStatus("Updating mod information...", false);

            mods = new List<Mod>();

            var request = WebRequest.Create(ModListUrl);

            await Task.Factory.FromAsync<WebResponse>(request.BeginGetResponse, request.EndGetResponse, null).ContinueWith((Action<Task<WebResponse>>)((Task<WebResponse> task) =>
            {
                HttpWebResponse response = (HttpWebResponse)task.Result;

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    StreamReader reader = new StreamReader(response.GetResponseStream());
                    string raw = reader.ReadToEnd();

                    mods = JsonConvert.DeserializeObject<List<Mod>>(raw);
                    SaveConfig();

                    SetStatus("Mod information updated!", true);
                }
                else
                {
                    SetStatus("Could not update mod information.", true);
                }
            }));
        }

        private async Task RunIPA(bool revert = false, bool launch = false, bool wait = false, bool shown = false)
        {
            await Task.Run(() =>
            {
                ProcessStartInfo psi = new ProcessStartInfo()
                {
                    Arguments = $"\"{ BeatSaberFile }\"" + (wait ? "" : " --nowait") + (revert ? " --revert" : "") + (launch ? " --launch" : ""),
                    FileName = IPAFile,
                    WorkingDirectory = config.beatSaberLocation,
                    WindowStyle = shown? ProcessWindowStyle.Normal : ProcessWindowStyle.Minimized
                };

                Process p = Process.Start(psi);

                p.WaitForExit();
            });
        }

        private async Task<bool> InstallMod(Mod mod)
        {
            return await Task.Run<bool>(async () =>
            {
                if (IsInstalled(mod))
                    return true;

                using (WebClient wc = new WebClient())
                {
                    string file = GetBeatSaberFile($@"Plugins\{ mod.ToString() }.zip");

                    try
                    {
                        ModDownload md = mod.GetBestDownloadFor(config.beatSaberType);

                        if (string.IsNullOrEmpty(md.DownloadUrl))
                            return false;

                        new FileInfo(file).Directory.Create();

                        wc.DownloadFile(md.DownloadUrl, file);

                        List<string> affectedFiles = new List<string>();

                        using (FileStream fs = new FileStream(file, FileMode.Open))
                        using (ZipArchive archive = new ZipArchive(fs))
                        {
                            affectedFiles = archive.ExtractToDirectory(config.beatSaberLocation, true);
                        }

                        config.installedMods.Add(new InstalledMod(mod, affectedFiles));
                        SaveConfig();

                        // Installing required dependencies for this mod to work.
                        foreach (Mod dep in mod.dependencies)
                        {
                            Mod latest = GetNewestModWithName(dep.name);

                            if (!IsInstalled(latest))
                            {
                                // Uninstall old dependency version.
                                if (IsAnyVersionInstalled(latest))
                                {
                                    await UninstallMod(GetInstalledMod(latest));
                                }

                                await InstallMod(latest);
                            }

                        }
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                    finally
                    {
                        if (File.Exists(file))
                            File.Delete(file);
                    }
                }
            });
        }

        [Obsolete("Use InstallCoreComponents instead.")]
        private async Task<bool> InstallIPA()
        {
            Mod ipaMod = mods.First((e) => string.Compare(e.name, "BSIPA", StringComparison.OrdinalIgnoreCase) == 0);

            return await InstallMod(ipaMod);
        }

        private async Task InstallCoreComponents()
        {
            foreach (Mod m in mods.Where((e) => e.IsRequired))
            {
                Mod latest = GetNewestModWithName(m.name);
                    /*mods.Where((e) => string.Compare(e.name, m.name, StringComparison.OrdinalIgnoreCase) == 0)
                    .OrderByDescending((e) => StringUtil.StringVersionToNumber(e.version)).First()*/;

                await InstallMod(latest);
            }
        }

        private async Task UninstallAllMods()
        {
            await RunIPA(revert: true, wait: config.showConsole, shown: config.showConsole);

            var toRemove = new List<InstalledMod>(config.installedMods);

            foreach(InstalledMod mod in toRemove)
                await UninstallMod(mod);
        }

        private async Task UninstallAllModsAndData()
        {
            await RunIPA(revert: true, wait: config.showConsole, shown: config.showConsole);

            File.Delete(ConfigFile);

            foreach(string file in Directory.GetFiles(config.beatSaberLocation))
            {
                if (!config.ogFiles.Contains(file))
                    File.Delete(file);
            }

            foreach (string directory in Directory.GetDirectories(config.beatSaberLocation))
            {
                if (!config.ogFiles.Contains(directory))
                    Directory.Delete(directory, true);
            }
        }

        private async Task<bool> UninstallMod(InstalledMod mod)
        {
            return await Task.Run<bool>(() =>
            {
                try
                {
                    foreach (string file in mod.affectedFiles)
                    {
                        if (File.Exists(file))
                            File.Delete(file);

                        FileUtil.DeleteEmptyParentDirectories(Path.GetDirectoryName(file));
                    }

                    config.installedMods.Remove(mod);
                    SaveConfig();

                    return true;
                }
                catch
                {
                    return false;
                }               
            });
        }

        [Obsolete]
        private async Task<bool> CreateBackup(string file)
        {
            return await Task.Run<bool>(async () =>
            {
                ProgressDialog pd = new ProgressDialog()
                {
                    ShowCancelButton = false,
                    Description = "We are creating a original backup of beat saber, as fail-safe.",
                    MinimizeBox = false,
                    ProgressBarStyle = Ookii.Dialogs.WinForms.ProgressBarStyle.MarqueeProgressBar,
                    ShowTimeRemaining = false,
                    Text = "Creating a backup...",
                    WindowTitle = "Create backup..."
                };

                pd.DoWork += (sender, e) =>
                {
                    try
                    {
                        if (File.Exists(file))
                            File.Delete(file);

                        ZipFile.CreateFromDirectory(config.beatSaberLocation, file, CompressionLevel.Fastest, false);
                    }
                    catch
                    { }
                };

                pd.ShowDialog();

                while (pd.IsBusy)
                    await Task.Delay(500);

                return true;
            });
        }


        private void FormMain_Load(object sender, EventArgs e)
        {
            Task.Run(new Action(async () =>
            {
                LoadConfig();

                checkBoxConsole.Checked = config.showConsole;
                checkBoxAllowNonApproved.Checked = config.allowNonApproved;
                checkBoxAutoUpdate.Checked = config.autoUpdate;

                if (!BeatSaberFound)
                {
                    config.beatSaberLocation = FindBeatSaberLocation(out config.beatSaberType);
                    SaveConfig();
                }

                await DownloadModInformations();

                BeginInvoke(new Action(() =>
                {
                    ShowMods();

                    labelBeatSaberType.Text = "Beat Saber type: " + config.beatSaberType;
                    textBoxBeatSaberLocation.Text = config.beatSaberLocation;
                }));

                if (!IPAFound)
                {
                    if (!BeatSaberVersionFileFound)
                    {
                        MessageBox.Show("Hey you!\n" +
                            "Before modding Beat Saber, you must at least play it once without mods.\n" +
                            "Please open and close Beat Saber and come back in a moment.", "Too excited?", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                        SelfDestruct();
                    }

                    if (MessageBox.Show("Do you want to mod Beat Saber right now?\n" +
                        "The core components will get installed.\n" +
                        "You can undo all the mods at any time in the settings tab.", "Let's mod?", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) != DialogResult.OK)
                    {
                        SelfDestruct();
                    }

                    config.ogFiles.AddRange(Directory.GetFiles(config.beatSaberLocation));
                    config.ogFiles.AddRange(Directory.GetDirectories(config.beatSaberLocation));
                    SaveConfig();

                    SetStatus("Installing core components...", false);

                    await InstallCoreComponents();

                    SetStatus("Installation of core components succeeded!", true);

                    BeginInvoke(new Action(() => ShowMods()));
                }

                string redVersion = BeatSaberVersion;
                if (StringUtil.StringVersionToNumber(config.lastBeatSaberVersion) < StringUtil.StringVersionToNumber(redVersion))
                {
                    MessageBox.Show($"Good news!" +
                        $"\nThere was a Beat Saber update ({ config.lastBeatSaberVersion } -> { redVersion }),\n" +
                        $"this means that some of the mods have to get updated too.\n" +
                        $"If the update was released recently, be aware that some mods could be broken.\n\n" +
                        $"At any moment you can open this program to automatically repatch, check for and install mod updates!", "Oh snap!", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    SetStatus("Patching Beat Saber...", false);

                    await RunIPA(revert: false, wait: config.showConsole, shown: config.showConsole);

                    SetStatus("Patched Beat Saber!", true);

                    config.lastBeatSaberVersion = redVersion;
                    SaveConfig();
                }

                if (config.autoUpdate)
                {
                    if (!await CheckForAndInstallModUpdates())
                    {
                        SetStatus($"All mods are up-to-date!", true);
                    }
                }
                else
                {
                    SetStatus($"Auto mod updates are disabled in settings.", true);
                }

                BeginInvoke(new Action(() =>
                {
                    ShowMods();
                }));
            }
            ));
        }

        private async Task<bool> CheckForAndInstallModUpdates()
        {
            bool anyGotUpdated = false;

            foreach (Mod m in CheckForModUpdates())
            {
                SetStatus($"Updating { m.ToString() }...", false);

                if (!await UninstallMod(GetInstalledMod(m)))
                {
                    SetStatus($"Could not remove old version, overwriting...", false);

                    await Task.Delay(1000);
                }

                if (await InstallMod(m))
                {
                    SetStatus($"Updated { m.ToString() }!", true);

                    ShowNotification($"Updated Beat Saber plugin:\n{ m.ToString() }");

                    anyGotUpdated = true;
                }
                else
                {
                    SetStatus($"Could not update { m.ToString() }!", true);

                    await Task.Delay(1000);
                }
            }

            return anyGotUpdated;
        }

        private void FormMain_FormClosed(object sender, FormClosedEventArgs e) => SaveConfig();

        private void listView_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && selected != null)
            {
                contextMenu.Show(listView, e.Location);
                contextMenu.Items[0].Text = selected.SubItems[0].Text;

                bool installed = IsAnyVersionInstalled((Mod)selected.Tag);
                contextMenu.Items[3].Enabled = !installed;
                contextMenu.Items[4].Enabled = installed;
            }
        }

        private async void installToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Mod m = (Mod)selected.Tag;
            SetStatus($"Installing mod { m.ToString() }...", false);

            if (await InstallMod(m))
            {
                SetStatus($"Installation of { m.ToString() } succeeded.", true);

                ShowMods();
                ShowNotification($"Mod { m.ToString() } was successfully installed into Beat Saber!");

                //await RunIPA(revert: false, wait: config.showConsole, shown: config.showConsole);
            }
            else
            {
                SetStatus($"Installation of { m.ToString() } failed!", true);
            }
        }

        private async void uninstallToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Mod mod = (Mod)selected.Tag;

            InstalledMod m = GetInstalledMod(mod);

            if (mod.IsRequired || mod.Category == ModCategory.Libraries)
            {
                if (MessageBox.Show($"You are willing to remove a core component, please note that removing these can cause the game to break.\n" +
                    $"Are you sure you want to remove { mod.ToString() }?", "Sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.No)
                    return;
            }

            SetStatus($"Removing mod { m.ToString() }...", false);

            if (await UninstallMod(m))
            {
                SetStatus($"Removal of { m.ToString() } succeeded.", true);

                ShowMods();
                ShowNotification($"The mod { m.ToString() } was successfully removed from Beat Saber.");

                //await RunIPA(revert: false, wait: config.showConsole, shown: config.showConsole);
            }
            else
            {
                SetStatus($"Removal of { m.ToString() } failed!", true);
            }
        }

        private void viewInformationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormModInfo fmi = new FormModInfo((Mod)selected.Tag);

            fmi.Show(this);
        }

        private async void buttonInstall_Click(object sender, EventArgs e)
        {
            int installedCount = 0;

            foreach (ListViewItem item in listView.CheckedItems)
            {
                Mod m = (Mod)item.Tag;
                SetStatus($"Installing mod { m.ToString() }...", false);

                if (await InstallMod(m))
                {
                    SetStatus($"Installation of { m.ToString() } succeeded.", true);

                    installedCount++;
                }
                else
                {
                    SetStatus($"Installation of { m.ToString() } failed!", true);
                }
            }

            if (installedCount > 0)
            {
                ShowMods();
                ShowNotification($"{ installedCount } mods were installed into Beat Saber successfully.");

                //await RunIPA(revert: false, wait: config.showConsole, shown: config.showConsole);
            }
        }

        private void buttonMoreInfo_Click(object sender, EventArgs e)
        {
            FormModInfo fmi = new FormModInfo((Mod)selected.Tag);

            fmi.Show(this);
        }

        private void buttonChangeBeatSaberLocation_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Please select your Beat Saber installation folder.";

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                if (!File.Exists(Path.Combine(fbd.SelectedPath, BeatSaberExecutable)))
                {
                    var r = MessageBox.Show("The Beat Saber executable was not found in this folder, \n" +
                        "are you sure you want to use this location?", "Uhm?", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Exclamation);

                    if (r == DialogResult.Abort)
                    {
                        return;
                    }
                    else if (r == DialogResult.Retry)
                    {
                        buttonChangeBeatSaberLocation.PerformClick();
                        return;
                    }
                }

                config.beatSaberLocation = fbd.SelectedPath;
                SaveConfig();

                textBoxBeatSaberLocation.Text = config.beatSaberLocation;
            }
        }

        private void buttonChangeBeatSaberType_Click(object sender, EventArgs e)
        {
            FormListSelect fls = new FormListSelect("Please select where you got Beat Saber from:", "Change type...", "Steam", "Oculus Store", "I don't know?");

            if (fls.ShowDialog() == DialogResult.OK)
            {
                if ((string)fls.Result == "Steam")
                {
                    config.beatSaberType = ModDownloadType.Steam;
                    SaveConfig();
                }
                else if ((string)fls.Result == "Oculus Store")
                {
                    config.beatSaberType = ModDownloadType.Oculus;
                    SaveConfig();
                }
                else
                {
                    config.beatSaberType = ModDownloadType.Universal;
                    SaveConfig();
                }

                labelBeatSaberType.Text = "Beat Saber type: " + config.beatSaberType;
            }
        }

        private async void buttonRemoveMods_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you very sure you want to remove all Beat Saber mods?\n" +
                "Data like custom songs, custom saber,... will be kept.", "Living on the edge.", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation) == DialogResult.Yes)
            {
                SetStatus("Uninstalling all mods...", false);

                await UninstallAllMods();

                SetStatus("All mods were removed!", true);

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
                SetStatus("Uninstalling all mods with data...", false);

                await UninstallAllModsAndData();

                SetStatus("All mods with data were removed!", true);

                ShowNotification($"All Beat Saber mods and data were removed successfully.");

                await Task.Delay(1000);

                SelfDestruct();
            }
        }

        private void listView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            Mod? s = e.Item.Tag as Mod?;

            selected = e.Item;

            if (selected != null)
            {
                textBoxDescription.Text = $"{ s.Value.name }\r\n\tby { s.Value.author.username }\r\n\r\n{ s.Value.description }\r\n\r\nCategory: { s.Value.Category.ToString() }";
            }

            buttonMoreInfo.Enabled = selected != null;
        }

        private void listView_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            Mod? s = e.Item.Tag as Mod?;

            if (s != null && IsAnyVersionInstalled(s.Value))
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

            await DownloadModInformations();

            BeginInvoke(new Action(() =>
            {
                ShowMods();
                checkBoxAutoUpdate.Checked = false;
            }));
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

            SetStatus($"Checking for updates...", false);

            await Task.Delay(1000);

            if (!await CheckForAndInstallModUpdates())
            {
                SetStatus($"All mods are up-to-date!", true);
            }
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

        public void SetStatus(string status, bool done)
        {
            BeginInvoke(new Action(() =>
            {
                statusLabel.Text = "Status: " + status;

                progressBar.Style = done ? System.Windows.Forms.ProgressBarStyle.Continuous : System.Windows.Forms.ProgressBarStyle.Marquee;
                progressBar.Visible = !done;
            }));
        }

        private void ShowMods()
        {
            listView.Items.Clear();
            Point p = listView.AutoScrollOffset;

            foreach (Mod m in mods)
                ShowMod(m);

            listView.AutoScrollOffset = p;
        }

        public void ShowMod(Mod m)
        {
            m.AddToList(listView, !IsInstalled(m));
        }
    }

    [Serializable]
    public class Config
    {
        public string beatSaberLocation;
        public string lastBeatSaberVersion;
        public List<string> ogFiles;
        public ModDownloadType beatSaberType;
        public List<InstalledMod> installedMods;
        public bool showConsole = false;
        public bool allowNonApproved = false;
        public bool autoUpdate = true;

        public Config()
        {
            ogFiles = new List<string>();
            installedMods = new List<InstalledMod>();
        }
    }
}
