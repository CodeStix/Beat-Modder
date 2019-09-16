using Flurl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Stx.BeatModsAPI
{
    public struct BeatModsQuery
    {
        public string search;
        public ModStatus status;
        public string forGameVersion;
        public BeatModsSort sort;
        public bool sortDescending;

        public static BeatModsQuery All => new BeatModsQuery()
        {
            search = null,
            status = ModStatus.All,
            forGameVersion = null,
            sort = BeatModsSort.None,
            sortDescending = false
        };

        public static BeatModsQuery AllApproved => new BeatModsQuery()
        {
            search = null,
            status = ModStatus.Approved,
            forGameVersion = null,
            sort = BeatModsSort.None,
            sortDescending = false
        };

        public BeatModsQuery(string forGameVersion)
        {
            search = null;
            status = ModStatus.Approved;
            this.forGameVersion = forGameVersion;
            sort = BeatModsSort.None;
            sortDescending = false;
        }

        public bool MatchesIgnoreSort(Mod mod)
        {
            if (!string.IsNullOrEmpty(search))
            {
                Regex r = new Regex(search);
                if (!r.IsMatch(mod.Name) && !r.IsMatch(mod.description) && !r.IsMatch(mod.author.username))
                    return false;
            }

            if (status != mod.Status)
                return false;

            if (!string.IsNullOrEmpty(forGameVersion) && forGameVersion == mod.gameVersion)
                return false;

            return true;
        }

        public override string ToString()
        {
            return $"(beatmods.com Query)";
        }
    }
}
