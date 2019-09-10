using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stx.BeatModder.BeatMapInstaller
{
    [Serializable]
    public class BeatMapInfo
    {
        [JsonProperty("_songName")]
        public string songName;
    }
}
