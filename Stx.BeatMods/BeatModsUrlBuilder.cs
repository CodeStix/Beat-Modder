using Flurl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stx.BeatModsAPI
{
    public static class BeatModsUrlBuilder
    {
        public static string BeatModsUrl => "https://beatmods.com";

        public static string AllModsUrl => BeatModsUrl.AppendPathSegments("api", "v1", "mod");
        public static string AllGameVersionsUrl => BeatModsUrl.AppendPathSegments("api", "v1", "version");

        /*
          return "https://beatmods.com"
                    .AppendPathSegments("api", "v1", "mod")
                    .SetQueryParam("gameVersion", BeatSaberVersion)
                    .SetQueryParam("search", null, Flurl.NullValueHandling.Ignore)
                    .SetQueryParam("status", config.allowNonApproved ? null : "approved")
                    .SetQueryParam("sortDirection", 1);
        */

        public static string CreateRequestUrl(this BeatModsQuery query)
        {
            return AllModsUrl
                .SetQueryParam("gameVersion", query.forGameVersion)
                .SetQueryParam("sort", query.sort.GetSortName())
                .SetQueryParam("status", query.status.GetStatusName())
                .SetQueryParam("search", query.search)
                .SetQueryParam("sortDirection", query.sortDescending ? -1 : 1);
        }
    }
}
