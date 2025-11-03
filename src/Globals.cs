using System;
using System.IO;

namespace D2SLib
{
    public static class Globals
    {
        public static readonly string PROJECT_DIRECTORY = Directory
            .GetParent(Environment.CurrentDirectory)
            .FullName;
        public static readonly string TEXT_DIR =
            Path.Combine(PROJECT_DIRECTORY, "src", "main", "TEXT") + Path.DirectorySeparatorChar;
        public static readonly string INPUT_DIR =
            Path.Combine(PROJECT_DIRECTORY, "src", "main", "input") + Path.DirectorySeparatorChar;
        public static readonly string OUTPUT_DIR =
            Path.Combine(PROJECT_DIRECTORY, "src", "main", "output") + Path.DirectorySeparatorChar;
        public static readonly string CFE = ".d2s"; // character file extension

        public static TXT txt_pd2 = new TXT();
        public static bool writeConsole_D2SRead = false; // temporary, for debugging
        public static bool writeConsole_ItemsRead = false; // temporary, for debugging
        public static bool writeConsole_Stash = true; // temporary, for debugging
        public static byte[] space = new byte[000];
        public static bool pd2_char_formatting = false;
        public static bool pd2_stash_formatting = false; // flag for .stash and .stash.hc files
    }
}