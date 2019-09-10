using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace Stx.BeatModder
{
    public static class FileUtil
    {
        public static IEnumerable<string> FindFile(string rootDir, params string[] names)
        {
            rootDir = rootDir.Trim();

            IEnumerable<string> files;

            try
            {
                files = Directory.EnumerateFiles(rootDir);
            }
            catch
            {
                yield break;
            }

            foreach (string file in files)
            {
                FileInfo fi = new FileInfo(file);

                foreach (string name in names)
                {
                    if (string.Compare(fi.Name, name, StringComparison.OrdinalIgnoreCase) == 0)
                        yield return file;
                }
            }

            IEnumerable<string> dirs;

            try
            {
                dirs = Directory.EnumerateDirectories(rootDir);
            }
            catch
            {
                yield break;
            }

            foreach (string dir in dirs)
            {
                foreach (string file in FindFile(dir, names))
                    yield return file;
            }
        }
    }
}
