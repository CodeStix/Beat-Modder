using Semver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stx.BeatModsAPI
{
    public static class BeatModsSortExtensions
    {
        public static IEnumerable<Mod> OnlyKeepCompatibleWith(this IEnumerable<Mod> mods, string gameVersion)
        {
            if (string.IsNullOrEmpty(gameVersion))
                return mods;

            return mods.Where((e) => e.IsCompatibleWith(gameVersion));
        }

        public static IEnumerable<Mod> OnlyKeepMostRecentMods(this IEnumerable<Mod> mods)
        {
            return mods.GroupBy(x => x.Name.ToUpper()).Select(x => x.OrderByDescending(e => SemVersionExtenions.AsNumber(e.Version)).First());
        }

        public static IOrderedEnumerable<Mod> SortModList(this BeatModsSort sort, IEnumerable<Mod> modsList, bool descending = true)
        {
            if (sort == BeatModsSort.None)
                return modsList.OrderBy((e) => 1);

            Func<Mod, string> ordener = (e) =>
            {
                switch (sort)
                {
                    default:
                    case BeatModsSort.None:
                        return string.Empty;

                    case BeatModsSort.Category:
                        return e.category;

                    case BeatModsSort.Name:
                        return e.Name;

                    case BeatModsSort.Status:
                        return e.status;

                    case BeatModsSort.Author:
                        return e.author.username;

                    case BeatModsSort.UpdatedDate:
                        return e.updatedDate;
                }
            };

            return descending ? 
                modsList.OrderByDescending(ordener) : 
                modsList.OrderBy(ordener);
        }

        public static string GetSortName(this BeatModsSort sort)
        {
            switch (sort)
            {
                default:
                case BeatModsSort.None:
                    return string.Empty;

                case BeatModsSort.Category:
                    return "category_lower";

                case BeatModsSort.Name:
                    return "name_lower";

                case BeatModsSort.Status:
                    return "status_lower";

                case BeatModsSort.Author:
                    return "author.username_lower";

                case BeatModsSort.UpdatedDate:
                    return "updatedDate";
            }
        }
    }
}
