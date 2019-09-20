using Semver;

namespace Stx.BeatModsAPI
{
    public static class IModExtensions
    {
        public static SemVersion GetVersion(this IMod mod)
        {
            return SemVersion.Parse(mod.Version);
        }

        public static SemVersion GetGameVersion(this IMod mod)
        {
            return SemVersion.Parse(mod.GameVersion.FixOddVersion());
        }

        public static string GetArchiveName(this IMod mod)
        {
            return $"{ mod.Name }-{ mod.Version }.zip";
        }
    }
}
