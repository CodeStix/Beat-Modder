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
                int.TryParse(s[i], out k);

                j += ((int)Math.Pow(1000, s.Length - i - 1)) * k;
            }
            return j;
        }
    }
}
