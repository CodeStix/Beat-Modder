using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
    class Program
    {
        const string BEAT_SAVER_KEY_DOWNLOAD = "https://beatsaver.com/api/download/key/";

        static void Main(string[] args)
        {
            if (args.Length == 0 || args[0] == "!bsr")
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("\tBeatMapInstaller.exe <bsr> [extractLocation]");
                Console.WriteLine("\tBeatMapInstaller.exe <bsr1>,<bsr2>...<bsrN> [extractLocation]");
                Console.WriteLine();
                Console.WriteLine("If no extract location is specified, 2 of the following things can happen:");
                Console.WriteLine("\t- If 'config.json' exists, the location is red from the 'beatSaberLocation' field.");
                Console.WriteLine("\t- The beat map is downloaded and extracted in the current directory.");
                Console.WriteLine();
                Console.WriteLine("A <bsr> parameter looks like this:");
                Console.WriteLine("\t!bsr1ef6");
                Console.WriteLine("\t1ef6");
                Console.WriteLine("\tbeatsaver://1ef6/");
                Console.WriteLine("\tbeatsaver://1ef6");
                Console.WriteLine("\tNOT: !bsr 1ef6");
                Console.WriteLine();

                return;
            }

            string[] bsrs = args[0].Split(',');
            string extractLocation = ".";

            if (args.Length >= 2)
            {
                extractLocation = string.Join(" ", args, 1, args.Length - 1);
            }
            else if (File.Exists("config.json"))
            {
                JObject json = JObject.Parse(File.ReadAllText("config.json"));

                string beatSaberLocation = json["beatSaberLocation"]?.ToString();

                if (!string.IsNullOrEmpty(beatSaberLocation))
                    extractLocation = Path.Combine(beatSaberLocation, "Beat Saber_Data", "CustomLevels");
            }

            extractLocation = Path.Combine(extractLocation, "TempBeatMap");

            foreach (string rawBsr in bsrs)
            {
                string bsr = rawBsr;

                if (bsr.StartsWith("!bsr"))
                    bsr = bsr.Substring("!bsr".Length).Trim();
                if (bsr.StartsWith("beatsaver://"))
                    bsr = bsr.Substring("beatsaver://".Length).Trim('/');

                Console.Write(bsr);

                try
                {
                    DownloadAndExtract(bsr, extractLocation);
                }
                catch(Exception e)
                {
                    Console.Write(" Error: " + e.Message);
                    return;
                }

                string infoFile = Directory.GetFiles(extractLocation, "info*", SearchOption.AllDirectories).FirstOrDefault();
                if (!string.IsNullOrEmpty(infoFile))
                {
                    BeatMapInfo bmi = JsonConvert.DeserializeObject<BeatMapInfo>(File.ReadAllText(infoFile));

                    Console.Write($" Song name: { bmi.songName }");

                    string newLocation = Path.Combine(new DirectoryInfo(extractLocation).Parent.FullName, bmi.songName);

                    if (Directory.Exists(newLocation))
                        Directory.Delete(newLocation, true);

                    Directory.Move(extractLocation, newLocation);

                    Console.Write($" -> { newLocation }");
                }
                else
                {
                    Console.Write($" -> { extractLocation }");
                }


                Console.WriteLine();
            }
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
