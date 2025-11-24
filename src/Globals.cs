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
        public static readonly string savePath = ""; // for reading local D2S files -- Change this to your .d2s save file path
        public static readonly string savePath2 = ""; // for reading local D2s file to compare -- Change this to a second .d2s save file path for comparison

        public static TXT txt_pd2 = new TXT();
        public static bool readLocalD2S = false; // temporary, for debugging -- Prints information about a local save file from savePath
        public static bool printOutItemDetails = false; // temporary, for debugging -- Prints information about each item in the local save file. Needs readLocalD2S = true
        public static bool printOutCharacterStats = false; // temporary, for debugging -- Prints information about the character being exported. Needs readLocalD2S = false
        public static bool compareLocalD2S = false; // temporary, for debugging -- Compares two local save files from savePath and savePath2. Needs readLocalD2s = true
        public static bool printoutItemComparison = false; // temporary, for debugging -- Prints comparison of items between two local save files. Needs compareLocalD2S = true
        public static bool printOutOffsets = false; // temporary, for debugging -- Prints offsets to file in solutionfolder for comparison between two local save files. Needs compareLocalD2S = true 
        public static bool writeConsole_D2SRead = false; // temporary, for debugging
        public static bool writeConsole_ItemsRead = false; // temporary, for debugging
        public static bool writeConsole_Stash = true; // temporary, for debugging
        public static byte[] space = new byte[000];
        public static bool pd2_char_formatting = false;
        public static bool pd2_stash_formatting = false; // flag for .stash and .stash.hc files
    }
}