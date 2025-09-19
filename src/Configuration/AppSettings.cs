using System.IO;

namespace D2SLib.Configuration
{
    public static class AppSettings
    {
        private static readonly string ProjectDirectory = Directory
            .GetParent(System.Environment.CurrentDirectory)
            .Parent.FullName;

        public static readonly string TextDir = Path.Combine(ProjectDirectory, "pd2-char-exporter", "src", "main", "TEXT");
        public static readonly string InputDir = Path.Combine(ProjectDirectory, "pd2-char-exporter", "src", "main", "input");
        public static readonly string OutputDir = Path.Combine(ProjectDirectory, "pd2-char-exporter", "src", "main", "output");

        public const string CharacterFileExtension = ".d2s";
        public const string ApiBaseUrl = "https://api.projectdiablo2.com/game/character/";

        public const int MaxGold = 8000000;
        public const int CharacterGold = 1000000;
        public const int ResistanceCharmValue = 30;
        public const int AttributeCharmValue = 15;
        public const int HealthCharmValue = 10000;
        public const int StackSize = 50;
        public const byte MaxItemLevel = 85;

        // Item positioning
        public const byte InventoryPage = 1;
        public const byte StashPage = 5;
        public const byte InvalidPosition = 255;

        public const uint DefaultItemId = 1116338226;
        public const ushort MercenaryItemListHeader = 19786;
    }
}