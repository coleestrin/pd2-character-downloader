using System.Collections.Generic;
using System;
using D2SLib.Configuration;
using D2SLib.Model.Save;

namespace D2SLib.Services
{
    public class InventoryStashService
    {
        private readonly ItemCreationService _itemCreationService;

        public InventoryStashService(ItemCreationService itemCreationService)
        {
            _itemCreationService = itemCreationService ?? throw new System.ArgumentNullException(nameof(itemCreationService));
        }

        public void FillInventoryAndStash(D2S character)
        {
            FillInventoryWithUtilities(character);
            FillStashWithRunes(character);
            FillStashWithGems(character);
            FillStashWithCraftingMaterials(character);
            FillStashWithMaps(character);
            FillStashWithEndgameMaps(character);
            AddMagicCharms(character);
        }

        private void FillInventoryWithUtilities(D2S character)
        {
            // Add tomes and cube to inventory
            AddItem(character, 0, 2, AppSettings.InventoryPage, "box", 1);
            AddItem(character, 0, 0, AppSettings.InventoryPage, "rid", 1);
            AddItem(character, 1, 0, AppSettings.InventoryPage, "rtp", 1); 

            // Fill inventory with rejuvenation potions
            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 10; col++)
                {
                    AddItem(character, (byte)col, (byte)row, AppSettings.InventoryPage, "rvl", 1);
                }
            }

            // Fill belt with rejuvenation potions (16 slots)
            for (int i = 0; i < 16; i++)
            {
                AddItem(character, (byte)i, 0, 0, "rvl", 1, true); // true = belt
            }

            // Fill cube with rejuvenation potions (4x4 grid)
            for (int i = 0; i < 4; i++)
            {
                for (int z = 0; z < 4; z++)
                {
                    AddItem(character, (byte)i, (byte)z, 4, "rvl", 1); // page 4 = cube
                }
            }
        }

        private void FillStashWithRunes(D2S character)
        {
            // Add all runes to stash
            var runeStacks = new[]
            {
                ("r01s", 0, 0), ("r02s", 1, 0), ("r03s", 2, 0), ("r04s", 3, 0), ("r05s", 4, 0),
                ("r06s", 5, 0), ("r07s", 6, 0), ("r08s", 7, 0), ("r09s", 8, 0), ("r10s", 9, 0),
                ("r11s", 0, 1), ("r12s", 1, 1), ("r13s", 2, 1), ("r14s", 3, 1), ("r15s", 4, 1),
                ("r16s", 5, 1), ("r17s", 6, 1), ("r18s", 7, 1), ("r19s", 8, 1), ("r20s", 9, 1),
                ("r21s", 0, 2), ("r22s", 1, 2), ("r23s", 2, 2), ("r24s", 3, 2), ("r25s", 4, 2),
                ("r26s", 5, 2), ("r27s", 6, 2), ("r28s", 7, 2), ("r29s", 8, 2), ("r30s", 9, 2),
                ("r31s", 0, 3), ("r32s", 1, 3), ("r33s", 2, 3)
            };

            foreach (var (code, x, y) in runeStacks)
            {
                AddItem(character, (byte)x, (byte)y, AppSettings.StashPage, code, AppSettings.StackSize);
            }

            var keys = new[]
            {
                ("pk1", 3, 3), ("pk2", 4, 3), ("pk3", 5, 3)
            };

            foreach (var (code, x, y) in keys)
            {
                AddItem(character, (byte)x, (byte)y, AppSettings.StashPage, code, AppSettings.StackSize);
            }
        }

        private void FillStashWithGems(D2S character)
        {
            var perfectGems = new[]
            {
                ("gpvs", 0, 6), ("gpys", 1, 6), ("gpbs", 2, 6), ("gpgs", 3, 6),
                ("gprs", 4, 6), ("gpws", 5, 6), ("skzs", 6, 6)
            };

            foreach (var (code, x, y) in perfectGems)
            {
                AddItem(character, (byte)x, (byte)y, AppSettings.StashPage, code, AppSettings.StackSize);
            }

            AddItem(character, 9, 5, AppSettings.StashPage, "rkey", 1);
        }

        private void FillStashWithCraftingMaterials(D2S character)
        {
            var craftingItems = new[]
            {
                ("wss", 9, 7), ("lbox", 8, 7), ("crfb", 4, 7), ("crfc", 0, 7),
                ("crfs", 3, 7), ("crfh", 2, 7), ("crfv", 6, 7), ("crfu", 1, 7), ("crfp", 5, 7)
            };

            foreach (var (code, x, y) in craftingItems)
            {
                AddItem(character, (byte)x, (byte)y, AppSettings.StashPage, code, AppSettings.StackSize);
            }
        }

        private void FillStashWithMaps(D2S character)
        {
            var maps = new[]
            {
                ("t11", 0, 12), ("t12", 1, 12), ("t13", 2, 12), ("t14", 3, 12),
                ("t15", 4, 12), ("t16", 5, 12), ("t21", 6, 12), ("t22", 7, 12), 
                ("t25", 0, 13), ("t26", 1, 13), ("t27", 2, 13), ("t28", 3, 13),
                ("t31", 4, 13), ("t32", 5, 13), ("t33", 6, 13), ("t34", 7, 13),
                ("t35", 8, 13), ("t36", 9, 13), ("t37", 0, 14), ("t38", 1, 14),
                ("t39", 2, 14), ("t3a", 3, 14), ("t23", 8, 12), ("t24", 9, 12),
                ("t41", 4, 14), ("t42", 5, 14), ("t43", 6, 14), ("t44", 7, 14),
                ("t51", 8, 14), ("t52", 9, 14)
            };

            foreach (var (code, x, y) in maps)
            {
                AddItem(character, (byte)x, (byte)y, AppSettings.StashPage, code, AppSettings.StackSize);
            }
        }

        private void FillStashWithEndgameMaps(D2S character)
        {
            var endgameMaps = new[]
            {
                ("rtma", 0, 9), ("rtma", 1, 9), ("rtma", 2, 9),
                ("uba", 3, 9), ("uba", 4, 9), ("uba", 5, 9),
                ("toa", 6, 9), ("toa", 7, 9), ("toa", 8, 9),
                ("ubtm", 0, 10), ("ubtm", 1, 10), ("ubtm", 2, 10),
                ("dcma", 3, 10), ("dcma", 4, 10), ("dcma", 5, 10)
            };

            foreach (var (code, x, y) in endgameMaps)
            {
                AddItem(character, (byte)x, (byte)y, AppSettings.StashPage, code, 1);
            }

            AddItem(character, 8, 11, AppSettings.StashPage, "rrra", AppSettings.StackSize);
            AddItem(character, 9, 11, AppSettings.StashPage, "irra", AppSettings.StackSize);
        }

        private void AddMagicCharms(D2S character)
        {
            // Resistance and attribute charm
            var resistanceStats = new List<ItemStat>
            {
                new() { Stat = "fireresist", Value = AppSettings.ResistanceCharmValue },
                new() { Stat = "coldresist", Value = AppSettings.ResistanceCharmValue },
                new() { Stat = "lightresist", Value = AppSettings.ResistanceCharmValue },
                new() { Stat = "poisonresist", Value = AppSettings.ResistanceCharmValue },
                new() { Stat = "strength", Value = AppSettings.AttributeCharmValue },
                new() { Stat = "vitality", Value = AppSettings.AttributeCharmValue },
                new() { Stat = "energy", Value = AppSettings.AttributeCharmValue },
                new() { Stat = "dexterity", Value = AppSettings.AttributeCharmValue }
            };

            var resistanceCharm = _itemCreationService.CreateMagicCharm(7, 4, AppSettings.StashPage, "cm1", resistanceStats);
            character.PlayerItemList.Items.Add(resistanceCharm);
            character.PlayerItemList.Count++;

            // Health charm
            var healthStats = new List<ItemStat>
            {
                new() { Stat = "maxhp", Value = AppSettings.HealthCharmValue }
            };

            var healthCharm = _itemCreationService.CreateMagicCharm(8, 4, AppSettings.StashPage, "cm1", healthStats);
            character.PlayerItemList.Items.Add(healthCharm);
            character.PlayerItemList.Count++;
        }

        private void AddItem(D2S character, byte x, byte y, byte page, string code, ushort quantity, bool belt = false)
        {
            var item = _itemCreationService.CreateSimpleItem(x, y, page, code, quantity);
            if (belt)
            {
                item.Mode = (ItemMode)2; // Belt mode
            }
            character.PlayerItemList.Items.Add(item);
            character.PlayerItemList.Count++;
        }
    }
}