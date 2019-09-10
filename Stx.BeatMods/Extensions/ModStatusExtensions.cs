using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stx.BeatModsAPI
{
    public static class ModStatusExtensions
    {
        public static string GetStatusName(this ModStatus status)
        {
            if (status == ModStatus.All)
                return string.Empty;

            return status.ToString().ToLower();
        }
    }
}
