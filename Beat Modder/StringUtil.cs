using System;

namespace Stx.BeatModder
{
    public static class StringUtil
    {
        public static int StringVersionToNumber(string version)
        {
            string[] s = version.Split('.');
            int j = 1;
            for (int i = 0; i < s.Length; i++)
            {
                int k = 0;
                int.TryParse(s[0], out k);

                j += (int)Math.Pow(10, i) * ++k;
            }
            return j;
        }
    }
}
