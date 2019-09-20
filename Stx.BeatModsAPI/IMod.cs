using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stx.BeatModsAPI
{
    public interface IMod
    {
        string Id { get; }
        string Name { get; }
        string Version { get; }
        string GameVersion { get; }
    }
}
