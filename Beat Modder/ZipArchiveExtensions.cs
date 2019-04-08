using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stx.BeatModder
{
    public static class ZipArchiveExtensions
    {
        public static List<string> ExtractToDirectory(this ZipArchive archive, string destinationDirectoryName, bool overwrite)
        {
            List<string> affectedFiles = new List<string>();

            if (!overwrite)
            {
                archive.ExtractToDirectory(destinationDirectoryName);
                return affectedFiles;
            }

            DirectoryInfo di = Directory.CreateDirectory(destinationDirectoryName);
            string destinationDirectoryFullPath = di.FullName;

            foreach (ZipArchiveEntry file in archive.Entries)
            {
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

                affectedFiles.Add(completeFileName);

                file.ExtractToFile(completeFileName, true);
            }

            return affectedFiles;
        }
    }
}
