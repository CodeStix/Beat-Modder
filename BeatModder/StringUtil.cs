using System;
using System.Reflection;

namespace Stx.BeatModder
{
    public static class StringUtil
    {
        public static string GetCurrentVersion(int components = 4)
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString(components);
        }
    }
}
