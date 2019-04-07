using System;
using System.IO;
using System.Security.Cryptography;

namespace Stx.BeatModder
{
    public static class FileUtil
    {
        public static string ComputeFileHash(string file)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(file))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower();
                }
            }
        }

        public static void DeleteEmptyParentDirectories(string dir)
        {
            if (!Directory.Exists(dir) || Directory.GetFiles(dir).Length != 0 || Directory.GetDirectories(dir).Length != 0)
                return;

            Directory.Delete(dir);

            DeleteEmptyParentDirectories(Directory.GetParent(dir).FullName);
        }
    }
}
