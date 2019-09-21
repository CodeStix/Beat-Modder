using Newtonsoft.Json;
using Semver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Stx.BeatModsAPI
{
    public class BeatMods : IDisposable
    {
        public List<string> AllGameVersions { get; private set; }
        public List<Mod> AllMods { get; private set; }
        public List<Mod> OfflineMods { get; private set; }
        public DateTime LastDidRefreshMods { get; private set; }
        public bool IsOffline { get; private set; }

        private const string OFFLINE_FILE = "offlineMods.json";
        private const string ALL_MODS_FILE = "allMods.json";

        [Serializable]
        private class CacheConfig
        {
            public string cacheMaxGameVersion;
            public List<string> allGameVersions;
            public List<Mod> cachedMods;
        }

        internal BeatMods()
        { }

        ~BeatMods()
        {
            Dispose();
        }

        public void Dispose()
        {
            Console.WriteLine($"Saving { OfflineMods.Count } offline available mods.");
            SaveOfflineModCache();
            OfflineMods.Clear();
            AllMods.Clear();
        }

        public static Task<BeatMods> CreateSession(bool useCachedOldMods = true)
        {
            return Task.Run(async () =>
            {
                // Delete old files...
                if (File.Exists("cachedMods.json"))
                    File.Delete("cachedMods.json");
                if (File.Exists("cachedModsUnapproved.json"))
                    File.Delete("cachedModsUnapproved.json");

                BeatMods session = new BeatMods();
                await session.RefreshMods(useCachedOldMods);
                return session;
            });
        }

        /// <summary>
        /// Refreshes the <see cref="AllMods"/> list with freshly new downloaded mod informations. 
        /// Keeps the information about mods up-to-date.
        /// </summary>
        public Task RefreshMods(bool useCachedOldMods = false, int timeout = 12000)
        {
            return Task.Run(async () =>
            {
                AllMods = new List<Mod>();
                OfflineMods = new List<Mod>();
                AllGameVersions = new List<string>();
                LastDidRefreshMods = DateTime.Now;

                CancellationTokenSource cts = new CancellationTokenSource(timeout);

                using (HttpClient hp = new HttpClient())
                {
                    string cacheFile = ALL_MODS_FILE;
                    CacheConfig cache = null;
                    if (File.Exists(cacheFile))
                    {
                        cache = JsonConvert.DeserializeObject<CacheConfig>(File.ReadAllText(cacheFile));
                    }

                    // Load offline available mods
                    if (File.Exists(OFFLINE_FILE))
                    {
                        OfflineMods.AddRange(JsonConvert.DeserializeObject<List<Mod>>(File.ReadAllText(OFFLINE_FILE)));
                        Console.WriteLine($"Loaded { OfflineMods.Count } offline available mods.");
                    }

                    try
                    {
                        HttpResponseMessage gameVersionsResponse = await hp.GetAsync(BeatModsUrlBuilder.AllGameVersionsUrl, cts.Token);
                        gameVersionsResponse.EnsureSuccessStatusCode();
                        string allGameVersionsJson = await gameVersionsResponse.Content.ReadAsStringAsync();
                        AllGameVersions = JsonConvert.DeserializeObject<List<string>>(allGameVersionsJson)
                            .OrderByDescending((e) => SemVersionExtenions.AsNumber(e)).ToList();

                        if (cache != null && (cache.allGameVersions == null || !cache.allGameVersions.SequenceEqual(AllGameVersions)))
                        {
                            cache.allGameVersions = AllGameVersions;
                            File.WriteAllText(cacheFile, JsonConvert.SerializeObject(cache));

                            Console.WriteLine("Cache file game versions changed.");
                        }

                        IsOffline = false;
                    }
                    catch(Exception e) when (e is HttpRequestException || e is TaskCanceledException)
                    {
                        IsOffline = true;

                        if (cache != null)
                            AllGameVersions.AddRange(cache.allGameVersions);
                        AllMods.AddRange(OfflineMods);
                        return;
                    }
                   
                    string mostRecentGameVersion = AllGameVersions.First();
                    BeatModsQuery query = BeatModsQuery.All;

                    // If mod cache is enabled, only download mod info about the most recent mods
                    if (cache != null && useCachedOldMods && cache.cacheMaxGameVersion == mostRecentGameVersion)
                    {
                        Console.WriteLine($"Loading { cache.cachedMods.Count } mods from cache '{ cacheFile }' until game version { cache.cacheMaxGameVersion }.");

                        query.forGameVersion = cache.cacheMaxGameVersion;
                        AllMods.AddRange(cache.cachedMods);
                    }

                    HttpResponseMessage modsResponse = await hp.GetAsync(query.CreateRequestUrl(), cts.Token);
                    modsResponse.EnsureSuccessStatusCode();
                    string modsJson = await modsResponse.Content.ReadAsStringAsync();

                    AllMods.AddRange(JsonConvert.DeserializeObject<List<Mod>>(modsJson));
                    AllMods = AllMods.Union(OfflineMods).ToList();

                    if (useCachedOldMods)
                    {
                        // Cache file does not exist, create it and cache all mods but the recent version
                        if (cache == null || cache.cacheMaxGameVersion != mostRecentGameVersion)
                        {
                            cache = new CacheConfig()
                            {
                                cachedMods = AllMods.Where((e) => e.GetGameVersion() < mostRecentGameVersion.FixOddVersion()).ToList(),
                                cacheMaxGameVersion = mostRecentGameVersion,
                                allGameVersions = AllGameVersions
                            };

                            File.WriteAllText(cacheFile, JsonConvert.SerializeObject(cache));

                            Console.WriteLine($"Created new cache file '{ cacheFile }' for { cache.cachedMods.Count } mods with max game version { cache.cacheMaxGameVersion }.");
                        }
                    }

                    Console.WriteLine($"Loaded in total { AllMods.Count } mods.");
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

        [Obsolete]
        public Mod GetMostRecentModFromId(string anyModVersionId, SemVersion compatibleGameVersion = null, bool onlyApproved = true)
        {
            Mod mod = GetModFromId(anyModVersionId);

            if (mod == null)
                return null;

            return GetMostRecentModWithName(mod.Name, compatibleGameVersion, onlyApproved);
        }

        public IEnumerable<Mod> GetModsWithName(string modName, bool onlyApproved = true)
        {
            return AllMods.Where((e) => string.Compare(e.Name, modName, StringComparison.OrdinalIgnoreCase) == 0)
                  .OnlyKeepStatus(onlyApproved ? ModStatus.Approved : ModStatus.All)
                  .OrderByDescending((e) => SemVersionExtenions.AsNumber(e.Version));
        }

        public Mod GetMostRecentModWithName(string modName, SemVersion compatibleGameVersion = null, bool onlyApproved = true)
        {
            return GetModsWithName(modName, onlyApproved)
                .OnlyKeepCompatibleWith(compatibleGameVersion)
                .FirstOrDefault();
        }

        public bool IsOutdated(IMod currentVersion, SemVersion compatibleGameVersion = null, bool onlyCompareApproved = true)
        {
            Mod mostRecent = GetMostRecentModWithName(currentVersion.Name, compatibleGameVersion, onlyCompareApproved);

            if (mostRecent == null)
                return false;

            return mostRecent > currentVersion;
        }

        public Mod GetModFromLocal(InstalledMod mod)
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
            foreach (Mod dep in mod.dependencies)
            {
                Mod dependency = dep;

                if (!dependency.IsInformationKnown)
                    dependency = GetModFromId(dependency.Id);

                if (dependency == null)
                    continue;

                yield return dependency;

                foreach (Mod subDep in EnumerateAllDependencies(dependency))
                    yield return subDep;
            }
        }

        public void SaveOfflineModCache()
        {
            File.WriteAllText(OFFLINE_FILE, JsonConvert.SerializeObject(OfflineMods));
        }
    }
}