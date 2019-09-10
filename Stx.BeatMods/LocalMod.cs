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
        public List<string> affectedFiles;
        public List<string> usedBy;
        public List<string> uses;
        public bool preventRemoval = false;

        public LocalMod()
        { }

        public LocalMod(string id, string name, string version, bool preventRemoval = false)
        {
            this.Id = id;
            this.Name = name;
            this.Version = version;
            this.preventRemoval = preventRemoval;
            affectedFiles = new List<string>();
            usedBy = new List<string>();
            uses = new List<string>();
        }

        public LocalMod(Mod mod)
        {
            Id = mod.Id;
            Name = mod.Name;
            Version = mod.Version;
            preventRemoval = mod.required;
            affectedFiles = mod.downloads[0].archiveFiles.Select((e) => e.file).ToList();
            usedBy = new List<string>();
            uses = new List<string>();
        }

        public bool EqualsModIgnoreVersion(Mod mod)
        {
            return string.Compare(Name, mod.Name, StringComparison.OrdinalIgnoreCase) == 0;
        }

        public bool EqualsModExactVersion(Mod mod)
        {
            return string.Compare(Name, mod.Name, StringComparison.OrdinalIgnoreCase) == 0 &&
                string.Compare(Version, mod.Version, StringComparison.OrdinalIgnoreCase) == 0;
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
