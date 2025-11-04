using D2SLib.Model;
using D2SLib.Model.Save;
using D2SLib.Model.TXT;
using System;
using System.IO;
using System.Security.Cryptography;

namespace D2SLib
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                if (Globals.readLocalD2S)
                {
                    RunDebugMode();
                }
                else
                {
                    RunNormalMode(args);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                Environment.Exit(1);
            }
        }

        private static void RunNormalMode(string[] args)
        {
            string username;

            if (args.Length == 0)
            {
                Console.Write("Enter character name: ");
                username = Console.ReadLine();
            }
            else
            {
                username = args[0];
            }

            if (string.IsNullOrWhiteSpace(username))
            {
                Console.WriteLine("Error: Username cannot be empty");
                Environment.Exit(1);
            }

            using var exporter = new CharacterExporter();
            var base64Result = exporter.ExportCharacter(username);

            var characterBytes = Convert.FromBase64String(base64Result);
            var outputPath = Path.Combine(Globals.OUTPUT_DIR, $"{username}.d2s");

            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            File.WriteAllBytes(outputPath, characterBytes);

            Console.WriteLine($"Character exported successfully to: {outputPath}");
        }

        private static void RunDebugMode()
        {
            //Console.Write("Enter path to .d2s save file: ");
            string savePath;
            if (Globals.savePath == null)
            {
                Console.Write("Enter path to .d2s save file: ");
                savePath = Console.ReadLine();
            }
            else
            {
                savePath = Globals.savePath;
            }

            if (string.IsNullOrWhiteSpace(savePath) || !File.Exists(savePath))
            {
                Console.WriteLine("Invalid or missing file path.");
                return;
            }

            if (Globals.compareLocalD2S)
            {
                string savePath2 = Globals.savePath2;

                if (string.IsNullOrWhiteSpace(savePath2) || !File.Exists(savePath2))
                {
                    Console.Write("Enter path to second .d2s save file for comparison or turn off comparisonboolean in Globals");
                    return;
                }

                InitializeGameData();

                var character1 = Core.ReadD2S(savePath);
                var character2 = Core.ReadD2S(savePath2);

                bool areEqual = character1.Equals(character2);
                Console.WriteLine($"Comparison result: The two characters are {(areEqual ? "identical" : "different")}.");

                //test

                //for (int i = 0; i < Math.Max(character1.PlayerItemList.Items.Count, character2.PlayerItemList.Items.Count); i++)
                //{
                //    var item1 = i < character1.PlayerItemList.Items.Count ? character1.PlayerItemList.Items[i] : null;
                //    var item2 = i < character2.PlayerItemList.Items.Count ? character2.PlayerItemList.Items[i] : null;

                //    if (item1 == null && item2 == null)
                //        continue;

                //    Console.WriteLine($"Slot {i}:");
                //    Console.WriteLine($"  character1: {(item1 != null ? item1.Code + $"({item1.TotalNumberOfSockets}) + $({item1.ToString})" : "empty")}");
                //    Console.WriteLine($"  character2: {(item2 != null ? item2.Code + $"({item2.TotalNumberOfSockets}) + $({item2.ToString})" : "empty")}");
                //}

                byte[] character1Bytes = File.ReadAllBytes(savePath);
                byte[] character2Bytes = File.ReadAllBytes(savePath2);
                for (int i = 0; i < Math.Min(character1Bytes.Length, character2Bytes.Length); i++)
                {
                    if (character1Bytes[i] != character2Bytes[i])
                        Console.WriteLine($"Difference at offset {i:X4}: {character1Bytes[i]:X2} vs {character2Bytes[i]:X2}");
                }

            }
            else
            {
                InitializeGameData();

                var character = Core.ReadD2S(savePath);
                Console.WriteLine(" Successfully parsed header!");
                Console.WriteLine($"Character Name: {character.Name}");
                Console.WriteLine($"Class: {character.ClassId}");
                Console.WriteLine($"Level: {character.Level}");
                Console.WriteLine($"Stats: {character.Attributes.Stats.Values}");
                Console.WriteLine($"Checksum: {character.Header.Checksum}");
                Console.WriteLine();

                Console.WriteLine("=== Inventory Items ===");
                if (character.PlayerItemList.Items != null && character.PlayerItemList.Items.Count > 0)
                {
                    foreach (var item in character.PlayerItemList.Items)
                    {
                        Console.WriteLine($"• {item.Code} ({item.Quality}) - {item.Code}");
                        if (item.IsSocketed)
                            Console.WriteLine($"  Sockets: {item.TotalNumberOfSockets}");
                    }
                }
                else
                {
                    Console.WriteLine("No items found in inventory.");
                }
            }
        }

        private static void InitializeGameData()
        {
            try
            {
                Globals.pd2_char_formatting = true;

                Globals.txt_pd2.ItemStatCostTXT = ItemStatCostTXT.Read(
                    Globals.TEXT_DIR + @"ItemStatCost.txt");

                Globals.txt_pd2.ItemsTXT.ArmorTXT = ArmorTXT.Read(
                    Globals.TEXT_DIR + @"Armor.txt");

                Globals.txt_pd2.ItemsTXT.WeaponsTXT = WeaponsTXT.Read(
                    Globals.TEXT_DIR + @"Weapons.txt");

                Globals.txt_pd2.ItemsTXT.MiscTXT = MiscTXT.Read(
                    Globals.TEXT_DIR + @"Misc.txt");

                Core.TXT = Globals.txt_pd2;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to initialize game data", ex);
            }
        }
    }
}
