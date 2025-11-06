using D2SLib.Configuration;
using D2SLib.Model.Api;
using D2SLib.Model.Save;
using D2SLib.Model.TXT;
using D2SLib.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;

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

                // A list of all items for total life and mana calculation
                //List<Item> allItems = new List<Item>();

                ////Loop it
                //foreach (var apiItem in apiCharacterData.Items)
                //{
                //    var newItem = _itemService.CreateItemFromApiData(apiItem);
                //    if (newItem != null)
                //        allItems.Add(newItem);
                //}

                // Calculate totals for substraction on stats
                //var totals = _itemService.CalculateTotals(allItems);

                int baselife = 0;
                int basemana = 0;
                int basevit = 0;
                int baseenergy = 0;               
                double vitalitycoeff = 0;
                double energycoeff = 0;

                switch (apiCharacterData.Character.Class.Id)
                {
                    case 0: // Amazon
                        baselife = (int)(50 + 60 + (2 * (character.Level - 1)));
                        basemana = (int)(15 + (2 * (character.Level - 1)));
                        basevit = 20;
                        baseenergy = 15;
                        vitalitycoeff = 3;
                        energycoeff = 1.5;
                        break;

                    case 1: // Sorceress
                        baselife = (int)(40 + 60 + (1 * (character.Level - 1)));
                        basemana = (int)(35 + (2 * (character.Level - 1)));
                        basevit = 10;
                        baseenergy = 35;
                        vitalitycoeff = 2;
                        energycoeff = 2;
                        break;

                    case 2: // Necromancer
                        baselife = (int)Math.Round(45 + 60 + (1.5 * (character.Level - 1)));
                        basemana = (int)(25 + (2 * (character.Level - 1)));
                        basevit = 15;
                        baseenergy = 25;
                        vitalitycoeff = 2;
                        energycoeff = 2;
                        break;
                    
                    case 3: // Paladin
                        baselife = (int)(55 + 60 + (2 * (character.Level - 1)));
                        basemana = (int)(15 + (2 * (character.Level - 1)));
                        basevit = 25;
                        baseenergy = 15;
                        vitalitycoeff = 3;
                        energycoeff = 1.5;
                        break;
                    
                    case 4: // Barbarian
                        baselife = (int)(55 + 60 + (2 * (character.Level - 1)));
                        basemana = (int)Math.Round(10 + (1.5 * (character.Level - 1)));
                        basevit = 25;
                        baseenergy = 10;
                        vitalitycoeff = 4;
                        energycoeff = 1;
                        break;
                    
                    case 5: // Druid
                        baselife = (int)Math.Round(55 + 60 + (1.5 * (character.Level - 1)));
                        basemana = (int)(20 + (2 * (character.Level - 1)));
                        basevit = 25;
                        baseenergy = 20;
                        vitalitycoeff = 2;
                        energycoeff = 2;
                        break;
                    
                    case 6: // Assassin
                        baselife = (int)(50 + 60 + (2 * (character.Level - 1)));
                        basemana = (int)(25 + (2 * (character.Level - 1)));
                        basevit = 20;
                        baseenergy = 25;
                        vitalitycoeff = 3;
                        energycoeff = 1.75;
                        break;
                }

                int lifeFromAttributeVitality = (int)Math.Round((apiCharacterData.Character.Attributes.Vitality - basevit) * vitalitycoeff);
                //int LifeFromVitalityOnItems = (int)Math.Round((totals.vitality + (totals.vitalityperlevel * character.Level)) * vitalitycoeff);
                //int LifeFromPercentMaxLife = (int)Math.Round((baselife + lifeFromAttributeVitality + totals.life + (totals.lifeperlevel * character.Level)) * (totals.percentmaxlife / 100.0));
                //int TotalLife = baselife + lifeFromAttributeVitality + LifeFromVitalityOnItems + totals.life + (totals.lifeperlevel * character.Level) + LifeFromPercentMaxLife;
                int newTotalLife = baselife + lifeFromAttributeVitality;

                int ManaFromAttributeEnergy = (int)Math.Round((apiCharacterData.Character.Attributes.Energy - baseenergy) * energycoeff);
                //int ManaFromEnergyOnItems = (int)Math.Round((totals.energy + (totals.energyperlevel * character.Level)) * energycoeff);
                //int ManaFromPercentMaxMana = (int)Math.Round((basemana + ManaFromAttributeEnergy + totals.mana + (totals.manaperlevel * character.Level)) * (totals.percentmaxmana / 100.0));
                //int TotalMana = basemana + ManaFromAttributeEnergy + ManaFromEnergyOnItems + totals.mana + (totals.manaperlevel * character.Level) + ManaFromPercentMaxMana;
                int newTotalMana = basemana + ManaFromAttributeEnergy;

                //Console.WriteLine($"---------------------LIFE---------------------------");
                //Console.WriteLine($"Total +Life from all items: {totals.life}");
                //Console.WriteLine($"Total +Life per level from all items: {totals.lifeperlevel * character.Level}");
                //Console.WriteLine($"Total +Vitality from all items: {totals.vitality}");
                //Console.WriteLine($"Total +Vitality per level from items: {totals.vitalityperlevel * character.Level}"); 
                //Console.WriteLine($"Total +% Maximum Life from all items: {totals.percentmaxlife}");
                //Console.WriteLine($"Total Baselife: {baselife}");               
                //Console.WriteLine($"Life from attribute Vitality: {lifeFromAttributeVitality}");
                //Console.WriteLine($"Life from Vitality On Items: {LifeFromVitalityOnItems}");
                //Console.WriteLine($"Life from percent Max Life: {LifeFromPercentMaxLife}");               
                //Console.WriteLine($"Total life: {TotalLife}");
                //Console.WriteLine($"---------------------MANA---------------------------");
                //Console.WriteLine($"Total +Mana from all items: {totals.mana}");
                //Console.WriteLine($"Total +Mana per level from all items: {totals.manaperlevel * character.Level}");
                //Console.WriteLine($"Total +Energy from all items: {totals.energy}");
                //Console.WriteLine($"Total +Energy per level from all items: {totals.energyperlevel * character.Level}");
                //Console.WriteLine($"Total +% Maximum Mana from all items: {totals.percentmaxmana}");
                //Console.WriteLine($"Total Basemana: {basemana}");
                //Console.WriteLine($"Mana from attribute Energy: {ManaFromAttributeEnergy}");
                //Console.WriteLine($"Mana from Energy on Items: {ManaFromEnergyOnItems}");
                //Console.WriteLine($"Mana from percent Max Mana: {ManaFromPercentMaxMana}");
                //Console.WriteLine($"Total mana: {TotalMana}");
                //Console.WriteLine($"---------------------STATS---------------------------");


                // Set gold and stats
                character.Attributes.Stats["goldbank"] = AppSettings.MaxGold;
                character.Attributes.Stats["vitality"] = apiCharacterData.Character.Attributes.Vitality;
                character.Attributes.Stats["strength"] = apiCharacterData.Character.Attributes.Strength;
                character.Attributes.Stats["energy"] = apiCharacterData.Character.Attributes.Energy;
                character.Attributes.Stats["dexterity"] = apiCharacterData.Character.Attributes.Dexterity;
                character.Attributes.Stats["maxhp"] = newTotalLife;
                character.Attributes.Stats["hitpoints"] = newTotalLife;
                character.Attributes.Stats["mana"] = newTotalMana;
                character.Attributes.Stats["maxmana"] = newTotalMana;
                character.Attributes.Stats["gold"] = AppSettings.CharacterGold;
                character.Attributes.Stats["newskills"] = 255; // Extra skill points
                character.Attributes.Stats["statpts"] = 1000; // Extra stat points

                // Set character name
                character.Name = apiCharacterData.Character.Name;

                // Fill with rejuvs and stash items
                _inventoryService.FillInventoryAndStash(character);

                // Handle mercenary
                if (apiCharacterData.Mercenary?.Items?.Count > 0)
                {
                    _characterService.ApplyMercenaryData(character, apiCharacterData.Mercenary);
                }

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