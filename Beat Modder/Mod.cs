using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Stx.BeatModder
{
    [Serializable]
    public class Mod
    {
        public ModAuthor author;
        public string authorId;
        public string category;
        public Mod[] dependencies;
        public string description;
        public ModDownload[] downloads;
        public string link;
        public string name;
        public string status;
        public bool required;
        public string updatedDate;
        public string uploadDate;
        public string version;
        [JsonProperty("_id")]
        public string id;

        [JsonIgnore]
        public ModStatus Status
        {
            get
            {
                switch (status.Trim().ToLower())
                {
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

                    case "uncategorized":
                    default:
                        return ModCategory.Uncategorized;
                }
            }
        }

        [JsonIgnore]
        public bool IsRequired
        {
            get
            {
                return required;
            }
        }

        public ModDownload GetBestDownloadFor(ModDownloadType type)
        {
            if (!downloads.Any((m) => type == m.Type) && (type != ModDownloadType.Universal))
                return downloads.FirstOrDefault((m) => ModDownloadType.Universal == m.Type);
            else
                return downloads.FirstOrDefault((m) => type == m.Type);
        }

        public void AddToList(ListView listView, bool enable)
        {
            ListViewItem lvi = new ListViewItem(new string[] { name, author.username, version, description });
            lvi.Group = listView.GetOrCreateGroup(Category.ToString());
            lvi.Tag = this;
            lvi.SetEnabled(enable);
            if (IsRequired)
                lvi.BackColor = Color.WhiteSmoke;
            listView.Items.Add(lvi);
        }

        public override string ToString()
        {
            return $"{ name.ToLower() }-{ version }";
        }

        public static implicit operator Mod(string from)
        {
            return new Mod()
            {
                id = from
            };
        }
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
    public struct ModDownload
    {
        public object hashMD5;
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
        public ModDownloadType Type
        {
            get
            {
                switch (type.Trim().ToLower())
                {
                    case "steam":
                        return ModDownloadType.Steam;

                    case "oculus":
                        return ModDownloadType.Oculus;

                    case "universal":
                    default:
                        return ModDownloadType.Universal;
                }
            }
        }
    }

    public enum ModDownloadType
    {
        Universal,
        Steam,
        Oculus
    }

    public enum ModStatus
    {
        Approved
    }

    public enum ModCategory
    {
        Uncategorized,
        UIEnhancements,
        StreamingTools,
        PracticeOrTraining,
        Libraries,
        Gameplay,
        Cosmetic,
        Core
    }

    public enum ModInstallStatus
    {
        NotInstalled = 0,
        Installed = 1
    }
}
