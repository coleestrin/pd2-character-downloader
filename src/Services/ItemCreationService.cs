using D2SLib.Configuration;
using D2SLib.Model.Api;
using D2SLib.Model.Save;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace D2SLib.Services
{
    public class ItemCreationService
    {
        public Item CreateItemFromApiData(D2Item apiItem)
        {
            if (apiItem == null) return null;
            var flags = CreateItemFlags();
            var newItem = new Item
            {
                Code = apiItem.Base.Id,
                Id = (uint)apiItem.Id,
                Quantity = 1,
                Page = apiItem.Base.Id.StartsWith("cm") ? AppSettings.InventoryPage : (byte)0,
                ItemLevel = (byte)apiItem.ItemLevel,
                Location = (ItemLocation)apiItem.Location.EquipmentId,
                Mode = apiItem.Location.Zone == "Equipped" ? ItemMode.Equipped : ItemMode.Stored,
                X = apiItem.Base.Id.StartsWith("cm") ? (byte)apiItem.Position.Column : AppSettings.InvalidPosition,
                Y = apiItem.Base.Id.StartsWith("cm") ? (byte)apiItem.Position.Row : AppSettings.InvalidPosition,
                FileIndex = CalculateFileIndex(apiItem.Unique),
                Quality = (ItemQuality)apiItem.Quality.Id,
                Armor = CalculateArmor(apiItem.Defense),
                Flags = flags,
                StatLists = new List<ItemStatList> { new() { Stats = new List<ItemStat>() } }
            };
            AddModifiersToItem(newItem, apiItem.Modifiers);
            return newItem;
        }

        public Item CreateSimpleItem(byte x, byte y, byte page, string code, ushort quantity)
        {
            var flags = CreateItemFlags();

            return new Item
            {
                Flags = flags,
                Mode = 0,
                Location = 0,
                Version = "101",
                ItemLevel = AppSettings.MaxItemLevel,
                Quality = ItemQuality.Normal,
                Id = AppSettings.DefaultItemId,
                Quantity = quantity,
                X = x,
                Y = y,
                Page = page,
                Code = code,
                StatLists = new List<ItemStatList> { new() { Stats = new List<ItemStat>() } }
            };
        }

        public Item CreateMagicCharm(byte x, byte y, byte page, string code, List<ItemStat> stats)
        {
            var flags = CreateItemFlags();

            var item = new Item
            {
                Flags = flags,
                Mode = 0,
                Location = 0,
                Quality = ItemQuality.Magic,
                ItemLevel = 1,
                X = x,
                Y = y,
                Page = page,
                Code = code,
                StatLists = new List<ItemStatList> { new() { Stats = stats } }
            };

            return item;
        }

        private BitArray CreateItemFlags()
        {
            var flags = new BitArray(32);
            flags[4] = true;  // IsIdentified
            flags[11] = false; // IsSocketed
            flags[13] = false; // IsNew
            flags[16] = false; // IsEar
            flags[17] = false; // IsStarterItem
            flags[21] = false; // IsCompact
            flags[22] = false; // IsEthereal
            flags[24] = false; // IsPersonalized
            flags[26] = false; // IsRuneword
            return flags;
        }

        private uint CalculateFileIndex(UniqueO unique)
        {
            if (unique == null) return 0;
            return unique.Id == 122 ? unique.Id : unique.Id - 1;
        }

        private ushort CalculateArmor(D2Defense defense)
        {
            return defense?.Base != null ? (ushort)defense.Base : (ushort)0;
        }

        private void AddModifiersToItem(Item item, List<D2ItemModifier> modifiers)
        {
            if (modifiers == null) return;

            foreach (var modifier in modifiers)
            {
                AddModifierToItem(item, modifier);
            }
        }

        private void AddModifierToItem(Item item, D2ItemModifier modifier)
        {
            switch (modifier.Name)
            {
                case "max_damage":
                    item.StatLists[0].Stats.Add(new ItemStat { Stat = "maxdamage", Value = (int)modifier.Values[0] });
                    break;

                case "min_damage":
                    item.StatLists[0].Stats.Add(new ItemStat { Stat = "mindamage", Value = (int)modifier.Values[0] });
                    break;

                case "lightmindam":
                case "lightdam":
                    AddElementalDamage(item, "light", modifier.Values);
                    break;

                case "coldmindam":
                case "colddam":
                    AddColdDamage(item, modifier.Values);
                    break;

                case "firemindam":
                case "firedam":
                    AddFireDamage(item, modifier.Values);
                    break;

                case "poisonmindam":
                case "poisondam":
                    AddPoisonDamage(item, modifier.Values);
                    break;


                case "item_skillonhit":
                case "item_skilloncast":
                case "item_skillonlevelup":
                case "item_skillondeath":
                    AddSkillOnHit(item, modifier.Values, modifier.Name);
                    break;

                case "all_resist":
                    AddAllResistances(item, (int)modifier.Values[0]);
                    break;

                case "all_attributes":
                    AddAllAttributes(item, (int)modifier.Values[0]);
                    break;

                case "maxdamage_percent":
                    AddPercentDamage(item, (int)modifier.Values[0]);
                    break;

                case "item_addskill_tab":
                    AddSkillTab(item, modifier.Values);
                    break;

                case "item_charged_skill":
                    AddChargedSkill(item, modifier.Values);
                    break;

                default:
                    AddGenericModifier(item, modifier);
                    break;
            }
        }

        private void AddElementalDamage(Item item, string element, List<float> values)
        {
            if (values.Count >= 2)
            {
                item.StatLists[0].Stats.Add(new ItemStat { Stat = $"{element}mindam", Value = (int)values[0] });
                item.StatLists[0].Stats.Add(new ItemStat { Stat = $"{element}maxdam", Value = (int)values[1] });
            }
            else if (values.Count == 1)
            {
                item.StatLists[0].Stats.Add(new ItemStat { Stat = $"{element}mindam", Value = 1 });
                item.StatLists[0].Stats.Add(new ItemStat { Stat = $"{element}maxdam", Value = (int)values[0] });
            }
        }

        private void AddColdDamage(Item item, List<float> values)
        {
            if (values.Count >= 3)
            {
                item.StatLists[0].Stats.Add(new ItemStat { Stat = "coldmindam", Value = (int)values[0] });
                item.StatLists[0].Stats.Add(new ItemStat { Stat = "coldmaxdam", Value = (int)values[1] });
                item.StatLists[0].Stats.Add(new ItemStat { Stat = "coldlength", Value = (int)values[2] });
            }
            else if (values.Count >= 2)
            {
                item.StatLists[0].Stats.Add(new ItemStat { Stat = "coldmindam", Value = (int)values[0] });
                item.StatLists[0].Stats.Add(new ItemStat { Stat = "coldmaxdam", Value = (int)values[1] });
            }
            else if (values.Count == 1)
            {
                item.StatLists[0].Stats.Add(new ItemStat { Stat = "coldmindam", Value = 1 });
                item.StatLists[0].Stats.Add(new ItemStat { Stat = "coldmaxdam", Value = (int)values[0] });
            }
        }

        private void AddFireDamage(Item item, List<float> values)
        {
            if (values.Count >= 2)
            {
                item.StatLists[0].Stats.Add(new ItemStat { Stat = "firemindam", Value = (int)values[0] });
                item.StatLists[0].Stats.Add(new ItemStat { Stat = "firemaxdam", Value = (int)values[1] });
            }
            else if (values.Count == 1)
            {
                item.StatLists[0].Stats.Add(new ItemStat { Stat = "firemindam", Value = 1 });
                item.StatLists[0].Stats.Add(new ItemStat { Stat = "firemaxdam", Value = (int)values[0] });
            }
        }

        private void AddPoisonDamage(Item item, List<float> values)
        {
            if (values.Count >= 3)
            {
                item.StatLists[0].Stats.Add(new ItemStat { Stat = "poisonmindam", Value = (int)values[0] });
                item.StatLists[0].Stats.Add(new ItemStat { Stat = "poisonmaxdam", Value = (int)values[1] });
                item.StatLists[0].Stats.Add(new ItemStat { Stat = "poisonlength", Value = (int)values[2] });
            }
            else if (values.Count >= 2)
            {
                item.StatLists[0].Stats.Add(new ItemStat { Stat = "poisonmindam", Value = (int)values[0] });
                item.StatLists[0].Stats.Add(new ItemStat { Stat = "poisonmaxdam", Value = (int)values[1] });
            }
            else if (values.Count == 1)
            {
                item.StatLists[0].Stats.Add(new ItemStat { Stat = "poisonmindam", Value = 1 });
                item.StatLists[0].Stats.Add(new ItemStat { Stat = "poisonmaxdam", Value = (int)values[0] });
            }
        }

        private void AddSkillOnHit(Item item, List<float> values, string modifierName)
        {
            if (values.Count >= 3)
            {
                item.StatLists[0].Stats.Add(new ItemStat
                {
                    Stat = modifierName,
                    SkillLevel = (int)values[0],
                    SkillId = (int)values[1],
                    Value = (int)values[2]
                });
            }
        }

        private void AddAllResistances(Item item, int value)
        {
            var resistances = new[] { "fireresist", "coldresist", "lightresist", "poisonresist" };
            foreach (var resistance in resistances)
            {
                item.StatLists[0].Stats.Add(new ItemStat { Stat = resistance, Value = value });
            }
        }

        private void AddAllAttributes(Item item, int value)
        {
            var attributes = new[] { "strength", "energy", "vitality", "dexterity" };
            foreach (var attribute in attributes)
            {
                item.StatLists[0].Stats.Add(new ItemStat { Stat = attribute, Value = value });
            }
        }

        private void AddPercentDamage(Item item, int value)
        {
            item.StatLists[0].Stats.Add(new ItemStat
            {
                Stat = "item_maxdamage_percent",
                Value = value,
                Param = value
            });
            item.StatLists[0].Stats.Add(new ItemStat
            {
                Stat = "item_mindamage_percent",
                Value = value,
                Param = value
            });
        }

        private void AddSkillTab(Item item, List<float> values)
        {
            if (values.Count >= 2)
            {
                item.StatLists[0].Stats.Add(new ItemStat
                {
                    Stat = "item_addskill_tab",
                    SkillTab = (int)values[0],
                    Value = (int)values[1]
                });
            }
        }

        private void AddChargedSkill(Item item, List<float> values)
        {
            if (values.Count >= 4)
            {
                item.StatLists[0].Stats.Add(new ItemStat
                {
                    Stat = "item_charged_skill",
                    SkillLevel = (int)values[0],
                    SkillId = (int)values[1],
                    Value = (int)values[3],
                    MaxCharges = (int)values[3]
                });
            }
        }

        private void AddGenericModifier(Item item, D2ItemModifier modifier)
        {
            if (modifier.Name.Contains("_perlevel"))
            {
                item.StatLists[0].Stats.Add(new ItemStat
                {
                    Stat = modifier.Name,
                    Value = (int)(modifier.Values[0] / 0.125)
                });
            }
            else if (modifier.Values.Count == 1)
            {
                item.StatLists[0].Stats.Add(new ItemStat
                {
                    Stat = modifier.Name,
                    Value = (int)modifier.Values[0]
                });
            }
            else if (modifier.Values.Count >= 2)
            {
                item.StatLists[0].Stats.Add(new ItemStat
                {
                    Stat = modifier.Name,
                    Param = (int)modifier.Values[0],
                    Value = (int)modifier.Values[1]
                });
            }
        }
    }
}