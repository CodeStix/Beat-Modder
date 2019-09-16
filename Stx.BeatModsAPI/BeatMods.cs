using Newtonsoft.Json;
using Semver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Stx.BeatModsAPI
{
    public class BeatMods
    {
        public List<string> AllGameVersions { get; private set; }
        public List<Mod> AllMods { get; private set; }
        public DateTime LastDidRefreshMods { get; private set; }

        private const string CACHE_FILE = "cachedMods.json";
        private const string CACHE_FILE_UNAPPROVED = "cachedModsUnapproved.json";

        [Serializable]
        private class CacheConfig
        {
            public string cacheMaxGameVersion;
            public List<Mod> cachedMods;
        }

        internal BeatMods()
        { }

        public static Task<BeatMods> CreateSession(bool useCachedOldMods = true, bool onlyDownloadApproved = true)
        {
            return Task.Run(async () =>
            {
                BeatMods session = new BeatMods();
                await session.RefreshMods(useCachedOldMods, onlyDownloadApproved);
                return session;
            });
        }

        /// <summary>
        /// Refreshes the <see cref="AllMods"/> list with freshly new downloaded mod informations. 
        /// Keeps the information about mods up-to-date.
        /// </summary>
        public Task RefreshMods(bool useCachedOldMods = false, bool onlyDownloadApproved = true)
        {
            AllMods = new List<Mod>();
            LastDidRefreshMods = DateTime.Now;

            return Task.Run(() =>
            {
                using (WebClient wc = new WebClient())
                {
                    string allGameVersionsJson = wc.DownloadString(BeatModsUrlBuilder.AllGameVersionsUrl);
                    AllGameVersions = JsonConvert.DeserializeObject<List<string>>(allGameVersionsJson)
                        .OrderByDescending((e) => SemVersionExtenions.AsNumber(e)).ToList();
                    string mostRecentGameVersion = AllGameVersions.First();

                    BeatModsQuery query = onlyDownloadApproved ? BeatModsQuery.AllApproved : BeatModsQuery.All;


                    // If mod cache is enabled, only download mod info about the most recent mods
                    string cacheFile = onlyDownloadApproved ? CACHE_FILE : CACHE_FILE_UNAPPROVED;
                    CacheConfig cache = null;
                    if (useCachedOldMods && File.Exists(cacheFile))
                    {
                        cache = JsonConvert.DeserializeObject<CacheConfig>(File.ReadAllText(cacheFile));

                        if (cache.cacheMaxGameVersion == mostRecentGameVersion)
                        {
                            Console.WriteLine($"Loading { cache.cachedMods.Count } mods from cache '{ cacheFile }' until game version { cache.cacheMaxGameVersion }.");

                            query.forGameVersion = cache.cacheMaxGameVersion;
                            AllMods.AddRange(cache.cachedMods); //.Where((e) => SemVersion.Parse(e.gameVersion.TrimOddVersion()) < cache.cacheMaxGameVersion)
                        }
                    }

                    string modsJson = wc.DownloadString(query.CreateRequestUrl());
                    AllMods.AddRange(JsonConvert.DeserializeObject<List<Mod>>(modsJson));

                    if (useCachedOldMods)
                    {
                        // Cache file does not exist, create it and cache all mods but the recent version
                        if (cache == null || cache.cacheMaxGameVersion != mostRecentGameVersion)
                        {
                            cache = new CacheConfig()
                            {
                                cachedMods = AllMods.Where((e) => SemVersion.Parse(e.gameVersion.TrimOddVersion()) < mostRecentGameVersion).ToList(),
                                cacheMaxGameVersion = mostRecentGameVersion
                            };

                            File.WriteAllText(cacheFile, JsonConvert.SerializeObject(cache));

                            Console.WriteLine($"Created new cache file '{ cacheFile }' for { cache.cachedMods.Count } mods with max game version { cache.cacheMaxGameVersion }.");
                        }
                    }
                }
            });
        }

        public string GetMostRecentGameVersion()
        {
            return AllGameVersions.First();
        }

        public Mod GetModFromId(string id)
        {
            return AllMods.FirstOrDefault((e) => string.Compare(e.Id, id, StringComparison.OrdinalIgnoreCase) == 0);
        }

        public Mod GetMostRecentModFromId(string anyModVersionId, string compatibleGameVersion = null)
        {
            Mod mod = GetModFromId(anyModVersionId);

            if (mod == null)
                return null;

            return GetMostRecentModWithName(mod.Name, compatibleGameVersion);
        }

        public IEnumerable<Mod> GetModsWithName(string modName)
        {
            return AllMods.Where((e) => string.Compare(e.Name, modName, StringComparison.OrdinalIgnoreCase) == 0)
                    .OrderByDescending((e) => SemVersionExtenions.AsNumber(e.Version));
        }

        public Mod GetMostRecentModWithName(string modName, string compatibleGameVersion = null)
        {
            return GetModsWithName(modName).OnlyKeepCompatibleWith(compatibleGameVersion).FirstOrDefault();
        }

        public bool IsOutdated(IMod currentVersion, string compatibleGameVersion = null)
        {
            Mod mostRecent = GetMostRecentModWithName(currentVersion.Name, compatibleGameVersion);

            if (mostRecent == null)
                return false;

            return mostRecent > currentVersion;
        }

        public Mod GetModFromLocal(LocalMod mod)
        {
            return AllMods.FirstOrDefault((e) => e.Version == mod.Version && string.Compare(e.Name, mod.Name, StringComparison.OrdinalIgnoreCase) == 0);
        }

        public Task<List<Mod>> GetModsOnline(BeatModsQuery query)
        {
            return Task.Run(() =>
            {
                List<Mod> matchingMods = new List<Mod>();

                using (WebClient wc = new WebClient())
                {
                    string allModsJson = wc.DownloadString(query.CreateRequestUrl());

                    AllMods = JsonConvert.DeserializeObject<List<Mod>>(allModsJson);
                }

                return matchingMods;
            });
        }

        public IOrderedEnumerable<Mod> GetModsCached(BeatModsQuery query)
        {
            return query.sort.SortModList(AllMods.Where((e) => query.MatchesIgnoreSort(e)), !query.sortDescending);
        }

        public IOrderedEnumerable<Mod> GetModsSortedBy(BeatModsSort sort, bool descending = true)
        {
            return sort.SortModList(AllMods, descending);
        }

        public IEnumerable<Mod> EnumerateAllDependencies(Mod mod)
        {
            /*if (string.Compare(mod.Name, Mod.BSIPA, StringComparison.OrdinalIgnoreCase) != 0 &&
                    !mod.dependencies.Any((e) => string.Compare(e.Name, Mod.BSIPA, StringComparison.OrdinalIgnoreCase) == 0))
            {
                mod.dependencies.Add(GetMostRecentModWithName(Mod.BSIPA, mod.gameVersion));
            }*/

            foreach (Mod dep in mod.dependencies)
            {
                Mod dependency = dep;

                if (!dependency.IsInformationKnown)
                    dependency = GetMostRecentModFromId(dependency.Id);

                if (dependency == null)
                    continue;

                yield return dependency;

                foreach (Mod subDep in EnumerateAllDependencies(dependency))
                    yield return subDep;
            }
        }
    }
}