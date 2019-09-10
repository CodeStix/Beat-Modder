using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Stx.BeatModder.BeatMapInstaller
{
    [Serializable]
    class Config
    {
        public string beatSaberLocation;
    }

    class Program
    {
        const string BEAT_SAVER_KEY_DOWNLOAD = "https://beatsaver.com/api/download/key/";

        private Config config;

        static void Main(string[] args)
        {
            args = new string[] { "!bsr 56b8" };

            if (args.Length == 0)
            {
                Console.WriteLine("Usage:");

                Console.WriteLine($"Usage:\n" +
                    $"\tBeatMapInstaller.exe <bsr> [extractLocation]\n" +
                    $"\tBeatMapInstaller.exe <bsr1>,<bsr2>...<bsrN> [extractLocation]\n\n");

                return;
            }

            string[] bsrs = args[0].Split(',');

            foreach(string rawBsr in bsrs)
            {
                string bsr = rawBsr;

                if (bsr.StartsWith("!bsr"))
                    bsr = bsr.Substring(4).Trim();

                Console.Write(bsr);

                string extractLocation = args.Length <= 1 ? $@".\BeatMap{ bsr }" : args[1];

                try
                {
                    DownloadAndExtract(bsr, extractLocation);
                }
                catch(Exception e)
                {
                    Console.Write(" Error: " + e.Message);
                }

                Console.WriteLine();
            }

            Console.ReadKey();
        }

        public static void DownloadAndExtract(string bsr, string extractLocation)
        {
            string downloadUrl = BEAT_SAVER_KEY_DOWNLOAD + bsr;
            string tempPath = Path.GetTempFileName();

            if (Directory.Exists(extractLocation))
                Directory.Delete(extractLocation, true);

            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.DownloadFile(downloadUrl, tempPath);
                }

                ZipFile.ExtractToDirectory(tempPath, extractLocation);

                string infoFile = FileUtil.FindFile(extractLocation, "info.json", "info.dat").FirstOrDefault();

                if (!string.IsNullOrEmpty(infoFile))
                {
                    BeatMapInfo bmi = JsonConvert.DeserializeObject<BeatMapInfo>(File.ReadAllText(infoFile));

                    Console.Write($" Song name: { bmi.songName }");

                    string newLocation = Path.Combine(new DirectoryInfo(extractLocation).Parent.FullName, bmi.songName);

                    if (Directory.Exists(newLocation))
                        Directory.Delete(newLocation, true);

                    Directory.Move(extractLocation, newLocation);
                }

                File.Delete(tempPath);
            }
            catch(IOException e)
            {
                throw new Exception("Could not extract beatmap into the specified folder.", e);
            }
            catch(WebException e)
            {
                throw new Exception("Could not download the beatmap.", e);
            }
        }

    }
}
