using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
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
                    .SetQueryParam("status", null/*"approved"*/)
                    .SetQueryParam("sortDirection", 1);
            }
        }
        private string BeatSaberFile
        {
            get
            {
                return GetBeatSaberFile("Beat Saber.exe");
            }
        }
        private string IPAFile
        {
            get
            {
                return GetBeatSaberFile("IPA.exe");
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

        public FormMain()
        {
            InitializeComponent();
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
            if (config.installedMods == null)
                return default(InstalledMod);

            return config.installedMods.FirstOrDefault((e) => e.Is(m));
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
            await RunIPA(launch: true);

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
                if (File.Exists(loc + @"\Beat Saber\Beat Saber.exe"))
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

            return actualPath; //@"C:\Users\Stijn Rogiest\Desktop\test";
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

        private async Task RunIPA(bool revert = false, bool launch = false)
        {
            await Task.Run(() =>
            {
                ProcessStartInfo psi = new ProcessStartInfo()
                {
                    Arguments = $"\"{ BeatSaberFile }\" --nowait" + (revert ? " --revert" : "") + (launch ? " --launch" : ""),
                    FileName = IPAFile,
                    WorkingDirectory = config.beatSaberLocation,
                    WindowStyle = ProcessWindowStyle.Minimized

                };

                Process p = Process.Start(psi);

                p.WaitForExit();
            });
        }

        private async Task<bool> InstallMod(Mod mod)
        {
            return await Task.Run<bool>(async () =>
            {
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

                        foreach (Mod dep in mod.dependencies)
                            if (!IsAnyVersionInstalled(dep))
                                await InstallMod(dep);

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

        private async Task<bool> InstallIPA()
        {
            Mod ipaMod = mods.First((e) => string.Compare(e.name, "BSIPA", StringComparison.OrdinalIgnoreCase) == 0);

            return await InstallMod(ipaMod);
        }

        private async Task InstallCoreComponents()
        {
            foreach (Mod m in mods.Where((e) => e.IsCoreComponent))
            {
                Mod latest = mods.Where((e) => string.Compare(e.name, m.name, StringComparison.OrdinalIgnoreCase) == 0)
                    .OrderByDescending((e) => StringUtil.StringVersionToNumber(e.version)).First();

                await InstallMod(latest);
            }
        }

        private async Task UninstallAllMods()
        {
            await RunIPA(true);

            var toRemove = new List<InstalledMod>(config.installedMods);

            foreach(InstalledMod mod in toRemove)
                await UninstallMod(mod);
        }

        private async Task UninstallAllModsAndData()
        {
            await RunIPA(true);

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
                    if (MessageBox.Show("Do you want to mod Beat Saber right now?\n" +
                        "The core components will get installed.\n" +
                        "You can undo all the mods at any time in the settings tab.", "Let's mod?", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) != DialogResult.OK)
                    {
                        SelfDestruct();
                    }

                    config.ogFiles.AddRange(Directory.GetFiles(config.beatSaberLocation));
                    config.ogFiles.AddRange(Directory.GetDirectories(config.beatSaberLocation));
                    SaveConfig();

                    SetStatus("Installing IPA...", false);

                    if (await InstallIPA())
                    {
                        SetStatus("Installation of IPA succeeded!", true);

                        await Task.Delay(1000);
                    }
                    else
                    {
                        SetStatus("Installation of IPA failed!", true);

                        MessageBox.Show("Yikes, could not install Illusion Plugin Architecture.\nMaybe run as administrator?",
                            "Could not install...", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        SelfDestruct();
                    }

                    SetStatus("Installing core components...", false);

                    await InstallCoreComponents();

                    SetStatus("Installation of core components succeeded!", true);

                    BeginInvoke(new Action(() => ShowMods()));
                }

                string beatSaberHash = FileUtil.ComputeFileHash(BeatSaberFile);
                if (config.lastBeatSaberHash != beatSaberHash)
                {
                    MessageBox.Show("Good news! There was a Beat Saber update, this means that some of the mods have to get updated too.\n" +
                        "If the update was released recently, be aware that some mods could be broken.\n" +
                        "At any moment you can open this program to automatically check for and install mod updates!", "Oh snap!", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    SetStatus("Patching Beat Saber...", false);

                    await RunIPA();

                    SetStatus("Patched Beat Saber!", true);

                    config.lastBeatSaberHash = beatSaberHash;
                    SaveConfig();
                }

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
                    }
                    else
                    {
                        SetStatus($"Could not update { m.ToString() }!", true);

                        await Task.Delay(1000);
                    }
                }
            }
            ));
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

                await RunIPA();
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

            if (mod.IsCoreComponent)
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

                await RunIPA();
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

            ShowMods();
            ShowNotification($"{ installedCount } mods were installed into Beat Saber successfully.");

            await RunIPA();
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
                if (!File.Exists(Path.Combine(fbd.SelectedPath, "Beat Saber.exe")))
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

            foreach (Mod m in mods)
            {
                ShowMod(m);
            }
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
        public string lastBeatSaberHash;
        public List<string> ogFiles;
        public ModDownloadType beatSaberType;
        public List<InstalledMod> installedMods;

        public Config()
        {
            ogFiles = new List<string>();
            installedMods = new List<InstalledMod>();
        }
    }
}
