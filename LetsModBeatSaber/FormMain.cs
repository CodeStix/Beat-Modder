using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Ookii.Dialogs;
using Ookii.Dialogs.WinForms;

namespace LetsModBeatSaber
{
    public partial class FormMain : Form
    {
        public const string ConfigFile = @"mods.json";
        public const string ModListUrl = @"https://beatmods.com/api/v1/mod?search=&status=approved&sort=category_lower&sortDirection=1";

        private ListViewItem selected = null;
        private Config config;
        private List<Mod> mods;
        //private string[] supportedBeatSaberVersions = new string[] { "0.13.0", "0.13.1", "0.13.2" };

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

        public string GetBeatSaberFile(string name)
        {
            return Path.Combine(config.beatSaberLocation, name);
        }

        public FormMain()
        {
            InitializeComponent();
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

                Environment.Exit(0);
                return;
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

        private async Task DownloadModInformations()
        {
            /*var web = new HtmlWeb();
            var doc = web.Load(@"https://beatmods.com");

            await Task.Delay(2000);

            MessageBox.Show(doc.Text);*/

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

            foreach(Mod m in mods)
            {
                ShowMod(m);
            }
        }

        public void ShowMod(Mod m)
        {
            m.AddToList(listView, !IsInstalled(m));
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

            if (s != null && IsInstalled(s.Value))
            {
                e.Item.Checked = false;
            }
        }

        private bool IsInstalled(Mod m)
        {
            if (config.installedMods == null)
                return false;

            return config.installedMods.Any((e) => e.Is(m));
        }

        private InstalledMod GetInstalledMod(Mod m)
        {
            if (config.installedMods == null)
                return default(InstalledMod);

            return config.installedMods.FirstOrDefault((e) => e.Is(m));
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
                    SetStatus("Installing IPA...", false);

                    if (await InstallIPA())
                    {
                        SetStatus("Installation of IPA succeeded!", true);

                        await Task.Delay(1000);
                    }
                    else
                    {
                        SetStatus("Installation of IPA failed!", true);

                        MessageBox.Show("Yikes, could not install Illusion Plugin Architecture.\nMaybe run as administrator?", "Could not install...", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        Environment.Exit(0);
                        return;
                    }

                    SetStatus("Installing core components...", false);

                    await InstallCoreComponents();

                    SetStatus("Installation of core components succeeded!", true);

                    BeginInvoke(new Action(() => ShowMods()));
                }

                string beatSaberHash = ComputeFileHash(BeatSaberFile);
                if (config.lastBeatSaberHash != beatSaberHash)
                {
                    MessageBox.Show("There is a beat saber update.");

                    SetStatus("Patching Beat Saber...", false);

                    await RunIPA();

                    SetStatus("Patched Beat Saber!", true);

                    config.lastBeatSaberHash = beatSaberHash;
                    SaveConfig();
                }
            }
            ));
        }

        private void listView_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && selected != null)
            {
                contextMenu.Show(listView, e.Location);
                contextMenu.Items[0].Text = selected.SubItems[0].Text;

                bool installed = IsInstalled((Mod)selected.Tag);
                contextMenu.Items[3].Enabled = !installed;
                contextMenu.Items[4].Enabled = installed;
            }
        }

        private void FormMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            SaveConfig();
        }

        private async Task RunIPA()
        {
            await Task.Run(() =>
            {
                ProcessStartInfo psi = new ProcessStartInfo()
                {
                    Arguments = $"\"{ BeatSaberFile }\" --nowait",
                    FileName = IPAFile,
                    WorkingDirectory = config.beatSaberLocation,
                    WindowStyle = ProcessWindowStyle.Minimized

                };

                Process p = Process.Start(psi);

                p.WaitForExit();
            });
        }

        private void RunBeatSaber()
        {
            //TODO
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
                    Environment.Exit(0);
                    return null;
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
                    Environment.Exit(0);
                    return null;
                }

                actualPath = (string)fls.Result;
            }

            beatSaberType = actualPath.ToLower().Contains("steam") ? ModDownloadType.Steam : ModDownloadType.Oculus;

            return actualPath; //@"C:\Users\Stijn Rogiest\Desktop\test";
        }

        private async Task<bool> InstallIPA()
        {
            Mod ipaMod = mods.First((e) => string.Compare(e.name, "BSIPA", StringComparison.OrdinalIgnoreCase) == 0);

            return await InstallMod(ipaMod); 
        }

        private async Task InstallCoreComponents()
        {
            var coreMods = mods.Where((e) => e.Category == ModCategory.Core || e.Category == ModCategory.Libraries);

            foreach (Mod m in coreMods)
            {
                await InstallMod(m);
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

                await RunIPA();
            }
            else
            {
                SetStatus($"Installation of { m.ToString() } failed!", true);
            }
        }

        private async void uninstallToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InstalledMod m = GetInstalledMod((Mod)selected.Tag);

            SetStatus($"Removing mod { m.ToString() }...", false);

            if (await UninstallMod(m))
            {
                SetStatus($"Removal of { m.ToString() } succeeded.", true);

                ShowMods();

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
            foreach(ListViewItem item in listView.CheckedItems)
            {
                Mod m = (Mod)item.Tag;
                SetStatus($"Installing mod { m.ToString() }...", false);

                if (await InstallMod(m))
                    SetStatus($"Installation of { m.ToString() } succeeded.", true);
                else
                    SetStatus($"Installation of { m.ToString() } failed!", true);
            }

            ShowMods();

            await RunIPA();
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

                        string dir = Path.GetDirectoryName(file);

                        if (Directory.Exists(dir) && Directory.GetFiles(dir).Length == 0)
                            Directory.Delete(dir);
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
                            if (!IsInstalled(dep))
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

        private string ComputeFileHash(string file)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(file))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower();
                }
            }
        }

        private void buttonMoreInfo_Click(object sender, EventArgs e)
        {
            Process.Start(((Mod)selected.Tag).link);
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
    }

    [Serializable]
    public class Config
    {
        public string beatSaberLocation;
        public string lastBeatSaberHash;
        public ModDownloadType beatSaberType;
        public List<InstalledMod> installedMods;

        public Config()
        {
            lastBeatSaberHash = null;
            installedMods = new List<InstalledMod>();
        }
    }
}
