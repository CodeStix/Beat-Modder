using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stx.BeatModsAPI
{
    public static class SemVersionExtenions
    {
        public static int AsNumber(string version)
        {
            if (string.IsNullOrEmpty(version))
                return 0;

            version = version.Trim().ToLower();

            string[] s = version.Split('.');
            int j = 1;
            for (int i = 0; i < s.Length; i++)
            {
                int.TryParse(s[i], out int k);

                j += ((int)Math.Pow(1000, s.Length - i - 1)) * k;
            }
            return j;
        }

        public static string FixOddVersion(this string oddVersionString)
        {
            string abc = "abcdefghijlkmnopqrstuvwxyz";
            for (int i = 0; i < abc.Length; i++)
            {
                int j = oddVersionString.IndexOf(abc[i]);
                if (j >= 0)
                    oddVersionString = oddVersionString.Substring(0, j);
            }
            return oddVersionString;
            /*string f = oddVersionString.Replace("p", ".").Replace("pl", ".");
            string allowed = "0123456789.";

            for(int i = 0; i < f.Length; i++)
            {
                if (!allowed.Contains(f[i]))
                {
                    f = f.Remove(i--, 1);
                }
            }

            return f;*/
        }
    }
}
