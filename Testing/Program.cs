using Stx.BeatModsAPI;
using System;

namespace Testing
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Is BeatMods available? ");
            Console.WriteLine(BeatModsUrlBuilder.IsBeatModsAvailable() ? "yes" : "no"); 
        }
    }
}
