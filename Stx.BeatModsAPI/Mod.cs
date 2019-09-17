using Newtonsoft.Json;
using Semver;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Stx.BeatModsAPI
{
    [Serializable]
    public class Mod : IMod
    {
        [JsonProperty("_id")]
        public string Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("version")]
        public string Version { get; set; }
        public ModAuthor author;
        public string authorId;
        public string category;
        public List<Mod> dependencies;
        public string description;
        public Download[] downloads;
        public string link;
        public string status;
        public bool required;
        public string updatedDate;
        public string uploadDate;
        public string gameVersion;

        [JsonIgnore]
        public static readonly string BSIPA = "BSIPA";

        public static explicit operator Mod(string modId)
        {
            return new Mod()
            {
                Id = modId
            };
        }

        [JsonIgnore]
        public bool IsInformationKnown
        {
            get
            {
                return !string.IsNullOrEmpty(Name);
            }
        }

        [JsonIgnore]
        public ModStatus Status
        {
            get
            {
                switch (status.Trim().ToLower())
                {
                    case "inactive":
                        return ModStatus.Inactive;

                    case "declined":
                        return ModStatus.Declined;

                    case "approved":
                    default:
                        return ModStatus.Approved;
                }
            }
        }

        [JsonIgnore]
        public ModCategory Category
        {
            get
            {
                switch (category.Trim().ToLower())
                {
                    case "ui enhancements":
                    case "uienhancements":
                        return ModCategory.UIEnhancements;

                    case "streaming tools":
                    case "streaming":
                        return ModCategory.StreamingTools;

                    case "practice / training":
                        return ModCategory.PracticeOrTraining;

                    case "libraries":
                        return ModCategory.Libraries;

                    case "gameplay":
                        return ModCategory.Gameplay;

                    case "cosmetic":
                        return ModCategory.Cosmetic;

                    case "core":
                        return ModCategory.Core;

                    case "lighting":
                        return ModCategory.Lighting;

                    case "tweaks / tools":
                        return ModCategory.Tools;

                    case "uncategorized":
                    default:
                        return ModCategory.Uncategorized;
                }
            }
        }

        public Download GetBestDownloadFor(BeatSaberInstalledType type)
        {
            if (!downloads.Any((m) => type == m.Type) && (type != BeatSaberInstalledType.Universal))
                return downloads.FirstOrDefault((m) => BeatSaberInstalledType.Universal == m.Type);
            else
                return downloads.FirstOrDefault((m) => type == m.Type);
        }

        public Download.File GetPluginBinaryFile(BeatSaberInstalledType type)
        {
            if (downloads.Length == 0)
                return default;

            if (Name.Equals(Mod.BSIPA, StringComparison.OrdinalIgnoreCase))
                return GetBestDownloadFor(type).archiveFiles.FirstOrDefault((e) => e.file.Equals("IPA.exe", StringComparison.OrdinalIgnoreCase));

            return GetBestDownloadFor(type).archiveFiles
                .Where((e) => e.file.EndsWith(".dll") || e.file.EndsWith(".exe"))
                .OrderByDescending((e) => e.file.Contains("Plugins/"))
                .ThenByDescending((e) => e.file.EndsWith(".dll"))
                .FirstOrDefault();
        }

        public bool IsCompatibleWith(string gameVersion)
        {
            SemVersion modVersion = SemVersion.Parse(this.gameVersion.TrimOddVersion());
            SemVersion version = SemVersion.Parse(gameVersion.TrimOddVersion());

            return modVersion.Major == version.Major && modVersion.Minor == version.Minor;
        }

        public override string ToString()
        {
            return $"{ Name }-{ Version }";
        }

        public static bool operator >(Mod left, IMod right)
        {
            return SemVersion.Parse(left.Version) > right.Version;
        }

        public static bool operator <(Mod left, IMod right)
        {
            return SemVersion.Parse(left.Version) < right.Version;
        }

        public static bool operator >=(Mod left, IMod right)
        {
            return SemVersion.Parse(left.Version) >= right.Version;
        }

        public static bool operator <=(Mod left, IMod right)
        {
            return SemVersion.Parse(left.Version) <= right.Version;
        }

        [Serializable]
        public struct ModAuthor
        {
            [JsonProperty("_id")]
            public string id;
            public string username;
            public string lastLogin;
        }

        [Serializable]
        public struct Download
        {
            [JsonProperty("hashMD5")]
            public File[] archiveFiles;
            public string type; // universal, steam, oculus
            public string url;

            [JsonIgnore]
            public string DownloadUrl
            {
                get
                {
                    return @"https://beatmods.com" + url;
                }
            }

            [JsonIgnore]
            public BeatSaberInstalledType Type
            {
                get
                {
                    switch (type.Trim().ToLower())
                    {
                        case "steam":
                            return BeatSaberInstalledType.Steam;

                        case "oculus":
                            return BeatSaberInstalledType.Oculus;

                        case "universal":
                        default:
                            return BeatSaberInstalledType.Universal;
                    }
                }
            }

            [Serializable]
            public struct File
            {
                public string hash;
                public string file;

                public static bool operator ==(File left, File right)
                {
                    return left.hash == right.hash && string.Compare(left.file, right.file, StringComparison.OrdinalIgnoreCase) == 0;
                }

                public static bool operator !=(File left, File right)
                {
                    return left.hash != right.hash || string.Compare(left.file, right.file, StringComparison.OrdinalIgnoreCase) != 0;
                }
            }
        }
    }
}
