using System;

namespace Stx.BeatModsAPI
{
    [Flags]
    public enum ModStatus
    {
        Approved = 1,
        Inactive = 2,
        Declined = 4,
        Pending = 8,
        All = 15
    }
}
