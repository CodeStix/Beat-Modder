using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stx.BeatModsAPI
{
    public static class DirectoryExtensions
    {
        public static void DeleteEmptyParentDirectories(string dir)
        {
            if (!Directory.Exists(dir) || Directory.GetFiles(dir).Length != 0 || Directory.GetDirectories(dir).Length != 0)
                return;

            Directory.Delete(dir);

            DeleteEmptyParentDirectories(Directory.GetParent(dir).FullName);
        }
    }
}
