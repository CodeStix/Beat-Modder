﻿using Flurl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Stx.BeatModsAPI
{
    public static class BeatModsUrlBuilder
    {
        public static string BeatModsHost => "beatmods.com";
        public static string BeatModsUrl => "https://" + BeatModsHost;

        public static string AllModsUrl => BeatModsUrl.AppendPathSegments("api", "v1", "mod");
        public static string AllGameVersionsUrl => BeatModsUrl.AppendPathSegments("api", "v1", "version");

        public static bool IsReachable
        {
            get
            {
                try
                {
                    using (var client = new WebClient())
                    using (client.OpenRead(BeatModsUrl))
                        return true;
                }
                catch
                {
                    return false;
                }
            }
        }

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
