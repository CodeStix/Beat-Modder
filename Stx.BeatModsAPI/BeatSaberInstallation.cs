using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ImpromptuInterface;
using Dynamitey;
using Semver;

namespace Stx.BeatModsAPI
{
    public class BeatSaberInstallation
    {
        public List<InstalledMod> InstalledMods
        {
            get
            {
                return config.installedMods;
            }
        }

        public ModDownloadType BeatSaberType
        {
            get
            {
                return config.beatSaberType;
            }
            set
            {
                config.beatSaberType = value;
                SaveConfig();
            }
        }

        public string BeatSaberDirectory { get; }
        public string BeatSaberDotExe { get; }
        public string BeatSaberPluginDir { get; }
        public string BeatSaberVersion { get; }
        public bool DidBeatSaberUpdate { get; } = false;
        public bool IsIPAInstalled
        {
            get
            {
                return InstalledMods.Any((e) => string.Compare(e.Name, Mod.BSIPA, StringComparison.OrdinalIgnoreCase) == 0);
            }
        }
        public bool RemoveModArchivesAfterInstall { get; set; } = true;
        public string ModArchivesDownloadLocation { get; set; } = "Downloaded mods";
        public string ConfigFileName { get; private set; }
        public static bool IsBeatSaberRunning
        {
            get
            {
                return Process.GetProcessesByName("Beat Saber").Length > 0;
            }
        }
        

        private Config config;
        private BeatMods beatMods;

        public static string[] OriginalBeatSaberFiles { get; } = new[] { "Beat Saber_Data", "Beat Saber.exe", "MonoBleedingEdge",
                        "UnityPlayer.dll", "UnityCrashHandler64.exe", "WinPixEventRuntime.dll" };

        public BeatSaberInstallation(string beatSaberDirectory, BeatMods beatModsConnection)
        {
            BeatSaberDirectory = beatSaberDirectory;
            beatMods = beatModsConnection;

            if (!Directory.Exists(beatSaberDirectory))
                throw new FileNotFoundException("The given Beat Saber root directory does not exist.");

            BeatSaberDotExe = Path.Combine(beatSaberDirectory, "Beat Saber.exe");
            BeatSaberPluginDir = Path.Combine(beatSaberDirectory, "Plugins");
            ConfigFileName = Path.Combine(beatSaberDirectory, "beatModderConfig.json");

            // Detecting Beat Saber version.
            // Current Beat Saber version is located in 'globalgamemanagers' file at byte offset 0x00001200
            try
            {
                string versionFile = Path.Combine(BeatSaberDirectory, "Beat Saber_Data", "globalgamemanagers");

                using (FileStream streamReader = new FileStream(versionFile, FileMode.Open))
                {
                    streamReader.Position = 0x1200;
                    byte[] asciiByteVersion = new byte[12];
                    streamReader.Read(asciiByteVersion, 0, asciiByteVersion.Length);
                    BeatSaberVersion = Encoding.ASCII.GetString(asciiByteVersion).Trim('\0');
                }
            }
            catch(Exception e)
            {
                throw new FileLoadException("Could not grab Beat Saber's version, is the game running?", e);
            }

            if (!File.Exists(BeatSaberDotExe))
                throw new FileNotFoundException("'Beat Saber.exe' was not found in the given root directory.");

            if (!File.Exists(ConfigFileName))
            {
                config = new Config
                {
                    ogFiles = new List<string>(OriginalBeatSaberFiles.Select((e) => Path.Combine(BeatSaberDirectory, e))),
                    beatSaberType = BeatSaberDirectory.ToLower().Contains("steam") ? ModDownloadType.Steam : ModDownloadType.Oculus,
                    installedMods = new List<InstalledMod>()
                };
            }
            else
            {
                try
                {
                    config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigFileName));
                }
                catch (JsonException e1)
                {
                    throw new Exception("Could not parse config file, was it modified?", e1);
                }
                catch (IOException e2)
                {
                    throw new Exception("Could not read config file.", e2);
                }
            }

            DidBeatSaberUpdate = SemVersion.Parse(BeatSaberVersion) > (config.lastBeatSaberVersion ?? "0.0");
            config.lastBeatSaberVersion = BeatSaberVersion;

            SaveConfig();
        }

        ~BeatSaberInstallation()
        {
            if (File.Exists(ConfigFileName))
                SaveConfig();
        }

        public bool IsInstalledExactVersion(Mod mod)
        {
            return InstalledMods.Any((e) => e.EqualsModExactVersion(mod, BeatSaberType));
        }

        public bool IsInstalledAnyVersion(Mod mod)
        {
            return InstalledMods.Any((e) => e.EqualsModIgnoreVersion(mod, BeatSaberType));
        }

        public InstalledMod GetInstalledModIgnoreVersion(Mod mod)
        {
            return InstalledMods.FirstOrDefault((e) => string.Compare(mod.GetPluginBinaryFile(BeatSaberType).file, e.binaryFile.file) == 0);
        }

        public InstalledMod GetInstalledModExactVersion(Mod mod)
        {
            return InstalledMods.FirstOrDefault((e) => mod.GetPluginBinaryFile(BeatSaberType) == e.binaryFile);
        }

        public InstalledMod GetInstalledMod(string name)
        {
            return InstalledMods.FirstOrDefault((e) => string.Compare(name, e.Name, StringComparison.OrdinalIgnoreCase) == 0);
        }

        public IEnumerable<KeyValuePair<InstalledMod, Mod>> EnumerateOutdatedMods()
        {
            foreach(InstalledMod mod in InstalledMods)
            {
                Mod newestVersion = beatMods.GetMostRecentModWithName(mod.Name, BeatSaberVersion);

                // A new compatible version does not yet exist
                if (newestVersion == null)
                    continue;

                if (newestVersion > mod)
                {
                    yield return new KeyValuePair<InstalledMod, Mod>(mod, newestVersion);
                }
            }
        }

        public async Task UninstallAllMods(bool showIPAConsole = false, IProgress<ProgressReport> progress = null)
        {
            progress?.Report(new ProgressReport("Unpatching...", 0.1f));

            await RunIPA(revert: true, wait: showIPAConsole, shown: showIPAConsole);

            List<InstalledMod> toRemove = config.installedMods.OrderBy((e) => e.usedBy.Count).ToList();
            for(int m = 0; m < toRemove.Count; m++)
            {
                InstalledMod mod = toRemove[m];

                progress?.Report(new ProgressReport($"Removing { mod }...", 0.25f + (float)(m + 1) / toRemove.Count * 0.7f));

                await UninstallMod(mod, true);
            }
                
            config.installedMods.Clear();
            SaveConfig();

            progress?.Report(new ProgressReport($"Done removing { toRemove.Count } mods.", 1f));
        }

        public async Task UninstallAllModsAndData(bool showIPAConsole = false)
        {
            await RunIPA(revert: true, wait: showIPAConsole, shown: showIPAConsole);

            List<string> ogFiles = config.ogFiles.Select((e) => Path.Combine(BeatSaberDirectory, e)).ToList();

            foreach (string file in Directory.GetFiles(BeatSaberDirectory))
            {
                if (!ogFiles.Contains(file))
                    File.Delete(file);
            }

            foreach (string directory in Directory.GetDirectories(BeatSaberDirectory))
            {
                if (!ogFiles.Contains(directory))
                    Directory.Delete(directory, true);
            }

            File.Delete(ConfigFileName);

            config.installedMods.Clear();
        }

        public void DetectManualModInstallOrUninstall(out List<Mod> wasManuallyInstalled, out List<InstalledMod> wasManuallyUninstalled)
        {
            wasManuallyInstalled = new List<Mod>();
            wasManuallyUninstalled = new List<InstalledMod>();

            foreach (Mod m in beatMods.AllMods.OnlyKeepMostRecentMods())
            {
                Mod.Download.File pluginDll = m.GetPluginBinaryFile(BeatSaberType);

                if (string.IsNullOrEmpty(pluginDll.file))
                    continue;

                string pluginDllPath = Path.Combine(BeatSaberDirectory, pluginDll.file);
                bool onDisk = File.Exists(pluginDllPath);

                if (IsInstalledAnyVersion(m) && !onDisk)
                {
                    // Plugin was uninstalled from outside this application.

                    Console.WriteLine($"Plugin was uninstalled from outside this application (md5: { pluginDll.hash }): { pluginDllPath }");

                    wasManuallyUninstalled.Add(GetInstalledModIgnoreVersion(m));
                }
                else if (!IsInstalledAnyVersion(m) && onDisk)
                {
                    // Plugin was installed from outside this application.

                    string diskDllHash = Hashing.CalculateMD5(pluginDllPath);

                    Mod installedVersion = beatMods.GetModsWithName(m.Name).FirstOrDefault((e) => 
                        string.Compare(e.GetPluginBinaryFile(BeatSaberType).hash, diskDllHash, StringComparison.OrdinalIgnoreCase) == 0);

                    if (installedVersion == null)
                    {
                        installedVersion = beatMods.GetModsWithName(m.Name).Last();

                        Console.WriteLine($"Plugin was installed from outside this application, but the version could not be found for { m.Name }, using oldest version");
                    }
                    else
                    {
                        Console.WriteLine($"Plugin was installed from outside this application (md5: { pluginDll.hash }): { installedVersion }");
                    }

                    wasManuallyInstalled.Add(installedVersion);
                }
            }

            SaveConfig();
        }

        [Obsolete("Just install any mod using InstallMod(), BSIPA will be added as dependency")]
        public Task<bool> InstallIPA(IProgress<ProgressReport> progress = null)
        {
            return Task.Run(async () => 
            {
                if (!IsIPAInstalled)
                {
                    progress?.Report(new ProgressReport($"Patch install: Finding best download for BSIPA...", 0.05f));

                    Mod bsipa = beatMods.GetModsWithName(Mod.BSIPA).OnlyKeepCompatibleWith(BeatSaberVersion).First();

                    if (await InstallMod(bsipa, progress) == null)
                    {
                        progress?.Report(new ProgressReport($"Patch failed: BSIPA could not be installed.", 1f));

                        return false;
                    }
                }

                progress?.Report(new ProgressReport($"Patching...", 0.85f));

                await RunIPA();

                progress?.Report(new ProgressReport($"Patch done: Beat Saber is now patched.", 1f));

                return true;
            });
        }

        public Task<InstalledMod> UpdateMod(InstalledMod currentModVersion, Mod newModVersion, IProgress<ProgressReport> progress = null)
        {
            return Task.Run(async () =>
            {
                if (currentModVersion >= newModVersion)
                {
                    progress?.Report(new ProgressReport($"Update skipped: a newer or equal version was already installed.", 1f));

                    return currentModVersion;
                }

                progress?.Report(new ProgressReport($"Removing old version: { currentModVersion.Version } ...", 0.1f));

                if (await UninstallMod(currentModVersion, true, ProgressReport.Partial(progress, 0.1f, 0.4f)))
                {
                    InstalledMod newInstalledVersion = await InstallMod(newModVersion, ProgressReport.Partial(progress, 0.5f, 0.5f));

                    if (newInstalledVersion != null)
                    {
                        newInstalledVersion.usedBy = currentModVersion.usedBy;
                        newInstalledVersion.uses = currentModVersion.uses;
                        newInstalledVersion.preventRemoval = currentModVersion.preventRemoval;

                        progress?.Report(new ProgressReport($"Update succeeded: { currentModVersion } -> { newModVersion }", 1f));
                        return newInstalledVersion;
                    }
                    else
                    {
                        progress?.Report(new ProgressReport($"Update failed: Could not install new version.", 1f));
                        return null;
                    }
                }
                else
                {
                    progress?.Report(new ProgressReport($"Update failed: Could not remove old version.", 1f));
                    return null;
                }
            });
        }

        public static bool KillBeatSaberProcess()
        {
            Process p = Process.GetProcessesByName("Beat Saber").FirstOrDefault();

            if (p == null)
                return false;

            p.Kill();

            return true;
        }

        public Task<InstalledMod> InstallMod(Mod mod, IProgress<ProgressReport> progress = null)
        {
            return Task.Run(async () =>
            {
                progress?.Report(new ProgressReport($"Starting to install { mod } ...", 0.1f));

                if (IsInstalledAnyVersion(mod))
                {
                    progress?.Report(new ProgressReport($"Install skipped: another version of this mod was already installed.", 1f));
                    return GetInstalledModIgnoreVersion(mod);
                }

                // Every mod should require BSIPA
                if (string.Compare(mod.Name, Mod.BSIPA, StringComparison.OrdinalIgnoreCase) != 0 && 
                    !mod.dependencies.Any((e) => string.Compare(e.Name, Mod.BSIPA, StringComparison.OrdinalIgnoreCase) == 0))
                {
                    Mod bsipa = beatMods.GetMostRecentModWithName(Mod.BSIPA, BeatSaberVersion);

                    if (bsipa == null)
                    {
                        progress?.Report(new ProgressReport($"Install failed: BSIPA (required) could not be found as a dependency.", 1f));
                        return null;
                    }

                    mod.dependencies.Add(bsipa);
                }

                Mod.Download md = mod.GetBestDownloadFor(config.beatSaberType);

                if (string.IsNullOrEmpty(md.DownloadUrl))
                {
                    progress?.Report(new ProgressReport($"Install failed: there was no compatible plugin file available for { config.beatSaberType }", 1f));
                    return null;
                }

                string zipDownloadLocation = Path.Combine(ModArchivesDownloadLocation, GetArchiveNameFor(mod));
                new FileInfo(zipDownloadLocation).Directory.Create();

                DownloadZip:
                if (!File.Exists(zipDownloadLocation))
                {
                    if (beatMods.IsOffline)
                    {
                        if (mod is OfflineMod offlineMod)
                        {
                            beatMods.OfflineMods.Remove(offlineMod);
                            progress?.Report(new ProgressReport($"Install failed: offline mod { mod } is not available anymore.", 1f));
                        }
                        else
                        {
                            progress?.Report(new ProgressReport($"Install failed: mod { mod } is not offline available.", 1f));
                        }
                        return null;
                    }

                    using (WebClient wc = new WebClient())
                    {
                        wc.DownloadProgressChanged += (sender, e) =>
                            progress?.Report(new ProgressReport($"Downloading { mod } ...", 0.1f + e.ProgressPercentage / 100f * 0.3f));

                        await wc.DownloadFileTaskAsync(md.DownloadUrl, zipDownloadLocation);
                    }
                }

                InstalledMod localMod = new InstalledMod(mod, BeatSaberType);

                try
                {
                    using (FileStream fs = new FileStream(zipDownloadLocation, FileMode.Open))
                    using (ZipArchive archive = new ZipArchive(fs))
                    {
                        Progress<float> extractProgress = new Progress<float>((i) =>
                            progress?.Report(new ProgressReport($"Extracting { mod.ToString() } ...", 0.4f + i * 0.3f)));

                        localMod.affectedFiles = archive.ExtractToDirectory(BeatSaberDirectory, true, false, extractProgress);
                    }
                }
                catch(InvalidDataException)
                {
                    File.Delete(zipDownloadLocation);

                    goto DownloadZip;
                }
               
                for (int d = 0; d < mod.dependencies.Count; d++)
                {
                    Mod dependency = mod.dependencies[d];
                    if (!dependency.IsInformationKnown)
                    {
                        dependency = beatMods.GetMostRecentModFromId(dependency.Id);

                        if (dependency == null)
                        {
                            progress?.Report(new ProgressReport($"Dependency with id { dependency.Id } not found, skipping.", 0.7f + (float)(d + 1) / mod.dependencies.Count * 0.3f));
                            continue;
                        }
                    }
                    else
                    {
                        Mod mostRecentCompatible = beatMods.GetMostRecentModWithName(dependency.Name, BeatSaberVersion);

                        if (mostRecentCompatible != null)
                            dependency = mostRecentCompatible;
                    }

                    progress?.Report(new ProgressReport($"Installing dependencies: { dependency.ToString() } ...", 0.7f + (float)(d + 1) / mod.dependencies.Count * 0.3f));

                    if (!localMod.uses.Contains(dependency.Name))
                        localMod.uses.Add(dependency.Name);

                    InstalledMod installedDependency = await InstallMod(dependency);
                    if (installedDependency != null)
                    {
                        if (!installedDependency.usedBy.Contains(localMod.Name))
                            installedDependency.usedBy.Add(localMod.Name);
                    }
                    else
                    {
                        progress?.Report(new ProgressReport($"Could not install dependency: { dependency.ToString() }, skipped.", 0.7f + (float)(d + 1) / mod.dependencies.Count * 0.3f));
                    }
                }

                if (RemoveModArchivesAfterInstall)
                {
                    progress?.Report(new ProgressReport($"Removing archive...", 0.95f));

                    File.Delete(zipDownloadLocation);
                }
                else
                {
                    if (!beatMods.OfflineMods.Contains(mod))
                        beatMods.OfflineMods.Add(mod);
                }

                if (string.Compare(mod.Name, Mod.BSIPA, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    progress?.Report(new ProgressReport($"Patching Beat Saber...", 1f));

                    await RunIPA();
                }

                progress?.Report(new ProgressReport($"Mod { mod.ToString() } was installed successfully.", 1f));

                InstalledMods.Add(localMod);
                SaveConfig();

                return localMod;
            });
        }

        public Task<bool> UninstallMod(InstalledMod mod, bool skipPreventRemoveCheck = false, IProgress<ProgressReport> progess = null)
        {
            return Task.Run(() =>
            {
                progess?.Report(new ProgressReport($"Preparing to uninstall { mod } ...", 0.1f));

                if (!skipPreventRemoveCheck && mod.usedBy.Count > 0)
                {
                    progess?.Report(new ProgressReport($"Could not uninstall mod: other mods are depending on it.", 1f));
                    return false;
                }

                if (!skipPreventRemoveCheck && mod.preventRemoval)
                {
                    progess?.Report(new ProgressReport($"Could not uninstall mod: the removal of this mod was prevented.", 1f));
                    return false;
                }

                for(int f = 0; f < mod.affectedFiles.Count; f++)
                {
                    string file = Path.Combine(BeatSaberDirectory, mod.affectedFiles[f]);

                    progess?.Report(new ProgressReport($"Removing { mod.Name } ... { file }", (float)(f + 1) / mod.affectedFiles.Count * 0.9f));

                    if (File.Exists(file))
                        File.Delete(file);

                    DirectoryExtensions.DeleteEmptyParentDirectories(Path.GetDirectoryName(file));
                }

                foreach(string uses in mod.uses)
                {
                    InstalledMod usesMod = GetInstalledMod(uses);

                    usesMod?.usedBy.Remove(mod.Name);
                }

                progess?.Report(new ProgressReport($"Mod { mod.ToString() } was uninstalled successfully.", 1f));

                InstalledMods.Remove(mod);
                SaveConfig();

                return true;
            });
        }

        public Task<bool> RunIPA(bool revert = false, bool launch = false, bool wait = false, bool shown = false)
        {
            return Task.Run(() =>
            {
                string ipa = Path.Combine(BeatSaberDirectory, "IPA.exe");

                if (!File.Exists(ipa))
                    return false;

                ProcessStartInfo psi = new ProcessStartInfo()
                {
                    Arguments = $"\"{ BeatSaberDotExe }\"" + (wait ? "" : " --nowait") + (revert ? " --revert" : "") + (launch ? " --launch" : ""),
                    FileName = ipa,
                    WorkingDirectory = BeatSaberDirectory,
                    WindowStyle = shown ? ProcessWindowStyle.Normal : ProcessWindowStyle.Minimized
                };

                Process p = Process.Start(psi);

                p.WaitForExit();

                return true;
            });
        }

        public async Task<int> InstallMultipleMods(List<Mod> mods, IProgress<ProgressReport> progress = null)
        {
            if (mods.Count == 0)
                return 0;

            int installedCount = 0;

            progress?.Report(new ProgressReport($"Installing { mods.Count } mods...", 0f));

            for (int i = 0; i < mods.Count; i++)
            {
                Mod m = mods[i];

                if (await InstallMod(m, ProgressReport.Partial(progress, i * (1f / mods.Count), 1f / mods.Count)) != null)
                    installedCount++;
            }

            progress?.Report(new ProgressReport($"{ mods.Count } of { installedCount } mods were installed successfully.", 0f));

            return installedCount;
        }

        public async Task<int> UninstallMultipleMods(List<InstalledMod> mods, bool skipPreventRemoveCheck = true, IProgress<ProgressReport> progress = null)
        {
            if (mods.Count == 0)
                return 0;

            int uninstalledCount = 0;

            mods = mods.OrderBy((e) => e.usedBy.Count).ToList();

            progress?.Report(new ProgressReport($"Uninstalling { mods.Count } mods...", 0f));

            for (int i = 0; i < mods.Count; i++)
            {
                InstalledMod m = mods[i];

                if (await UninstallMod(m, skipPreventRemoveCheck, ProgressReport.Partial(progress, i * (1f / mods.Count), 1f / mods.Count)))
                    uninstalledCount++;
            }

            progress?.Report(new ProgressReport($"{ mods.Count } of { uninstalledCount } mods were uninstalled successfully.", 0f));

            return uninstalledCount;
        }

        private void SaveConfig()
        {
            try
            {
                File.WriteAllText(ConfigFileName, JsonConvert.SerializeObject(config));
            }
            catch (IOException e)
            {
                throw new Exception("Could not save config file.", e);
            }
        }

        public static string GetArchiveNameFor(IMod mod)
        {
            return $"{ mod.Name }-{ mod.Version }.zip";
        }

        internal class Config
        {
            public string lastBeatSaberVersion;
            public ModDownloadType beatSaberType;
            public List<string> ogFiles;
            public List<InstalledMod> installedMods;
        }
    }
}
