using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stx.BeatModsAPI
{
    public static class ZipArchiveExtensions
    {
        public static List<string> ExtractToDirectory(this ZipArchive archive, string destinationDirectoryName, bool overwrite, bool returnAbsolutePaths = true, IProgress<float> progress = null)
        {
            List<string> affectedFiles = new List<string>();

            if (!overwrite)
            {
                archive.ExtractToDirectory(destinationDirectoryName);
                return affectedFiles;
            }

            DirectoryInfo di = Directory.CreateDirectory(destinationDirectoryName);
            string destinationDirectoryFullPath = di.FullName;

            for(int i = 0; i < archive.Entries.Count; i++)
            {
                ZipArchiveEntry file = archive.Entries[i];

                string completeFileName = Path.GetFullPath(Path.Combine(destinationDirectoryFullPath, file.FullName));

                if (!completeFileName.StartsWith(destinationDirectoryFullPath, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(file.Name))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(completeFileName));

                    continue;
                }

                new FileInfo(completeFileName).Directory.Create();

                affectedFiles.Add(returnAbsolutePaths ? completeFileName : file.FullName);

                file.ExtractToFile(completeFileName, true);

                progress?.Report((i + 1f) / archive.Entries.Count);
            }

            return affectedFiles;
        }
    }
}
