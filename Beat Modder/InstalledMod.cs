using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stx.BeatModder
{
    [Serializable]
    public class InstalledMod
    {
        public string name;
        public string version;
        public string description;
        public string author;
        public List<string> affectedFiles;
        public List<string> usedBy;
        public List<string> uses;
        public bool preventRemoval = false;

        public InstalledMod()
        { }

        public InstalledMod(string name, string author, string version, string description, bool preventRemoval = false)
        {
            this.name = name;
            this.author = author;
            this.version = version;
            this.description = description;
            this.preventRemoval = preventRemoval;
            affectedFiles = new List<string>();
            usedBy = new List<string>();
            uses = new List<string>();
        }

        public InstalledMod(Mod mod, List<string> affectedFiles, bool preventRemoval = false)
        {
            name = mod.name;
            author = mod.author.username;
            version = mod.version;
            description = mod.description;
            this.preventRemoval = preventRemoval;
            this.affectedFiles = affectedFiles;
            usedBy = new List<string>();
            uses = new List<string>();
        }

        public bool EqualsModIgnoreVersion(Mod mod)
        {
            return string.Compare(name, mod.name, StringComparison.OrdinalIgnoreCase) == 0 && 
                string.Compare(author, mod.author.username, StringComparison.OrdinalIgnoreCase) == 0;
        }

        public bool EqualsMod(Mod mod)
        {
            return string.Compare(name, mod.name, StringComparison.OrdinalIgnoreCase) == 0 &&
                string.Compare(author, mod.author.username, StringComparison.OrdinalIgnoreCase) == 0 &&
                string.Compare(version, mod.version, StringComparison.OrdinalIgnoreCase) == 0;
        }

        public override string ToString()
        {
            return $"{ name.ToLower() }-{ version }";
        }
    }
}
