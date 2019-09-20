using Newtonsoft.Json;
using Semver;
using Stx.BeatModsAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stx.BeatModsAPI
{
    [Serializable]
    public class InstalledMod : IMod
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("version")]
        public string Version { get; set; }
        [JsonProperty("gameVersion")]
        public string GameVersion { get; set; }
        public List<string> affectedFiles;
        public List<string> usedBy;
        public List<string> uses;
        public Mod.Download.File binaryFile;
        public bool preventRemoval = false;

        public InstalledMod()
        { }

        /*[Obsolete]
        public LocalMod(string id, string name, string version, Mod.Download.File binaryFile, bool preventRemoval = false)
        {
            Id = id;
            Name = name;
            Version = version;
            this.binaryFile = binaryFile;
            this.preventRemoval = preventRemoval;
            affectedFiles = new List<string>();
            usedBy = new List<string>();
            uses = new List<string>();
        }*/

        public InstalledMod(Mod mod, BeatSaberInstalledType type)
        {
            Id = mod.Id;
            Name = mod.Name;
            Version = mod.Version;
            GameVersion = mod.GameVersion;
            binaryFile = mod.GetPluginBinaryFile(type);
            preventRemoval = mod.required;
            affectedFiles = mod.GetBestDownloadFor(type).archiveFiles.Select((e) => e.file).ToList();
            usedBy = new List<string>();
            uses = new List<string>();
        }

        public string GetPluginBinaryFile()
        {
            if (Name.Equals(Mod.BSIPA, StringComparison.OrdinalIgnoreCase))
                return affectedFiles.FirstOrDefault((e) => e.Contains("IPA.exe"));
            else
                return affectedFiles
                    .Where((e) => e.EndsWith(".dll") || e.EndsWith(".exe"))
                    .OrderByDescending((e) => e.Contains("Plugins/"))
                    .ThenByDescending((e) => e.EndsWith(".dll"))
                    .FirstOrDefault();
        }

        public bool EqualsModIgnoreVersion(Mod mod, BeatSaberInstalledType type)
        {
            return string.Compare(mod.GetPluginBinaryFile(type).file, binaryFile.file) == 0;

            //return string.Compare(Name, mod.Name, StringComparison.OrdinalIgnoreCase) == 0;
        }

        public bool EqualsModExactVersion(Mod mod, BeatSaberInstalledType type)
        {
            return mod.GetPluginBinaryFile(type) == binaryFile;

            /*return string.Compare(Name, mod.Name, StringComparison.OrdinalIgnoreCase) == 0 &&
                string.Compare(Version, mod.Version, StringComparison.OrdinalIgnoreCase) == 0;*/
        }

        public static bool operator >(InstalledMod left, IMod right)
        {
            return SemVersion.Parse(left.Version) > right.Version;
        }

        public static bool operator <(InstalledMod left, IMod right)
        {
            return SemVersion.Parse(left.Version) < right.Version;
        }

        public static bool operator >=(InstalledMod left, IMod right)
        {
            return SemVersion.Parse(left.Version) >= right.Version;
        }

        public static bool operator <=(InstalledMod left, IMod right)
        {
            return SemVersion.Parse(left.Version) <= right.Version;
        }

        public override string ToString()
        {
            return $"{ Name }-{ Version }";
        }

    }
}
