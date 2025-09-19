using System;
using System.IO;
using D2SLib.Configuration;
using D2SLib.Model.Api;
using D2SLib.Model.Save;
using D2SLib.Model.TXT;
using D2SLib.Services;

namespace D2SLib
{
    public class CharacterExporter : IDisposable
    {
        private readonly CharacterApiService _apiService;
        private readonly CharacterModificationService _characterService;
        private readonly InventoryStashService _inventoryService;
        private readonly ItemCreationService _itemService;

        public CharacterExporter()
        {
            _apiService = new CharacterApiService();
            _itemService = new ItemCreationService();
            _characterService = new CharacterModificationService(_itemService);
            _inventoryService = new InventoryStashService(_itemService);
        }

        public CharacterExporter(CharacterApiService apiService,
                               CharacterModificationService characterService,
                               InventoryStashService inventoryService,
                               ItemCreationService itemService)
        {
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
            _characterService = characterService ?? throw new ArgumentNullException(nameof(characterService));
            _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
            _itemService = itemService ?? throw new ArgumentNullException(nameof(itemService));
        }

        public string ExportCharacter(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username cannot be null or empty", nameof(username));

            try
            {
                // Initialize game data
                InitializeGameData();

                // Fetch character data from API
                var apiCharacterData = _apiService.GetCharacterData(username);


                if (apiCharacterData?.Character?.Class?.Name == null)
                {
                    throw new InvalidOperationException($"Invalid character data received for username: {username}. Missing character class information.");
                }

                // Load base character template
                var character = LoadBaseCharacter(apiCharacterData.Character.Class.Name);


                // Apply basic character data
                character.Level = (byte)apiCharacterData.Character.Level;
                character.Attributes.Stats["level"] = apiCharacterData.Character.Level;
                character.Attributes.Stats["experience"] = (int)apiCharacterData.Character.Experience;

                // Complete progression
                _characterService.CompleteAllQuests(character);
                _characterService.ActivateAllDifficulties(character);
                _characterService.UnlockAllWaypoints(character);

                // Clear and add items
                character.PlayerItemList.Items.Clear();
                character.PlayerItemList.Count = 0;

                // Add items from API data
                _characterService.AddItemsFromApiData(character, apiCharacterData.Items);

                // Apply skills
                _characterService.ApplySkills(character, apiCharacterData.Character.Skills);

                // Setup NPC dialogs
                _characterService.SetupNpcDialogs(character);

                // ApplyHC status
                if (apiCharacterData.Character.Status.IsHardcore)
                {
                    character.Status.IsHardcore = true;
                }

                // Set gold and stats
                character.Attributes.Stats["goldbank"] = AppSettings.MaxGold;
                character.Attributes.Stats["vitality"] = apiCharacterData.Character.Attributes.Vitality;
                character.Attributes.Stats["strength"] = apiCharacterData.Character.Attributes.Strength;
                character.Attributes.Stats["energy"] = apiCharacterData.Character.Attributes.Energy;
                character.Attributes.Stats["dexterity"] = apiCharacterData.Character.Attributes.Dexterity;
                character.Attributes.Stats["hitpoints"] = apiCharacterData.Character.Life;
                character.Attributes.Stats["maxhp"] = apiCharacterData.Character.Life;
                character.Attributes.Stats["mana"] = apiCharacterData.Character.Mana;
                character.Attributes.Stats["maxmana"] = apiCharacterData.Character.Mana;
                character.Attributes.Stats["gold"] = AppSettings.CharacterGold;

                // Set character name
                character.Name = apiCharacterData.Character.Name;

                // Fill with rejuvs and stash items
                _inventoryService.FillInventoryAndStash(character);

                // Handle merc
                //Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(apiCharacterData.Mercenary));
                /*if (apiCharacterData.Mercenary?.Items?.Count > 0)
                {
                    _characterService.ApplyMercenaryData(character, apiCharacterData.Mercenary);
                }*/

                // Validate character
                ValidateCharacterData(character);

                // Convert to bytes and return as base64
                try
                {
                    var saveBytes = Core.WriteD2S(character);
                    return Convert.ToBase64String(saveBytes);
                }
                catch (NullReferenceException ex)
                {
                    throw new InvalidOperationException($"Null reference error during character serialization. This usually indicates missing required character data. Inner exception: {ex.Message}", ex);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to export character for username: {username}", ex);
            }
        }

        private void InitializeGameData()
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

        private D2S LoadBaseCharacter(string className)
        {
            try
            {
                var characterPath = Globals.INPUT_DIR + className.ToLowerInvariant() + Globals.CFE;

                if (!File.Exists(characterPath))
                    throw new FileNotFoundException($"Base character file not found: {characterPath}");

                var characterBytes = File.ReadAllBytes(characterPath);
                return Core.ReadD2S(characterBytes);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load base character for class: {className}", ex);
            }
        }

        private void ValidateCharacterData(D2S character)
        {
            if (character == null)
                throw new InvalidOperationException("Character is null");

            if (character.PlayerItemList == null)
                throw new InvalidOperationException("PlayerItemList is null");

            if (character.PlayerItemList.Items == null)
                throw new InvalidOperationException("PlayerItemList.Items is null");

            if (character.PlayerCorpses == null)
                throw new InvalidOperationException("PlayerCorpses is null");

            if (character.MercenaryItemList == null)
                throw new InvalidOperationException("MercenaryItemList is null");

            if (character.Golem == null)
                throw new InvalidOperationException("Golem is null");

            if (character.Attributes == null)
                throw new InvalidOperationException("Attributes is null");

            if (character.Attributes.Stats == null)
                throw new InvalidOperationException("Attributes.Stats is null");

            if (character.Quests == null)
                throw new InvalidOperationException("Quests is null");

            if (character.Waypoints == null)
                throw new InvalidOperationException("Waypoints is null");

            if (character.NPCDialog == null)
                throw new InvalidOperationException("NPCDialog is null");

            if (character.ClassSkills == null)
                throw new InvalidOperationException("ClassSkills is null");
        }

        public void Dispose()
        {
            _apiService?.Dispose();
        }
    }
}