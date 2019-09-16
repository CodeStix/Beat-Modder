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
    public class LocalMod : IMod
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("version")]
        public string Version { get; set; }
        public string gameVersion;
        public List<string> affectedFiles;
        public List<string> usedBy;
        public List<string> uses;
        public Mod.Download.File binaryFile;
        public bool preventRemoval = false;

        public LocalMod()
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

        public LocalMod(Mod mod, ModDownloadType type)
        {
            Id = mod.Id;
            Name = mod.Name;
            Version = mod.Version;
            gameVersion = mod.gameVersion;
            binaryFile = mod.GetPluginBinaryFile(type);
            preventRemoval = mod.required;
            affectedFiles = mod.GetBestDownloadFor(type).archiveFiles.Select((e) => e.file).ToList();
            usedBy = new List<string>();
            uses = new List<string>();
        }

        public bool EqualsModIgnoreVersion(Mod mod, ModDownloadType type)
        {
            return string.Compare(mod.GetPluginBinaryFile(type).file, binaryFile.file) == 0;

            //return string.Compare(Name, mod.Name, StringComparison.OrdinalIgnoreCase) == 0;
        }

        public bool EqualsModExactVersion(Mod mod, ModDownloadType type)
        {
            return mod.GetPluginBinaryFile(type) == binaryFile;

            /*return string.Compare(Name, mod.Name, StringComparison.OrdinalIgnoreCase) == 0 &&
                string.Compare(Version, mod.Version, StringComparison.OrdinalIgnoreCase) == 0;*/
        }

        public static bool operator >(LocalMod left, IMod right)
        {
            return SemVersion.Parse(left.Version) > right.Version;
        }

        public static bool operator <(LocalMod left, IMod right)
        {
            return SemVersion.Parse(left.Version) < right.Version;
        }

        public static bool operator >=(LocalMod left, IMod right)
        {
            return SemVersion.Parse(left.Version) >= right.Version;
        }

        public static bool operator <=(LocalMod left, IMod right)
        {
            return SemVersion.Parse(left.Version) <= right.Version;
        }

        public override string ToString()
        {
            return $"{ Name }-{ Version }";
        }

    }
}
