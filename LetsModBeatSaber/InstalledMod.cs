using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LetsModBeatSaber
{
    [Serializable]
    public struct InstalledMod
    {
        public string name;
        public string version;
        public string description;
        public string author;
        public List<string> affectedFiles;

        public InstalledMod(string name, string author, string version, string description)
        {
            this.name = name;
            this.author = author;
            this.version = version;
            this.description = description;
            affectedFiles = new List<string>();
        }

        public InstalledMod(Mod mod, List<string> affectedFiles)
        {
            name = mod.name;
            author = mod.author.username;
            version = mod.version;
            description = mod.description;
            this.affectedFiles = affectedFiles;
        }

        public bool Is(Mod mod)
        {
            return name == mod.name && author == mod.author.username && version == mod.version;
        }
    }
}
