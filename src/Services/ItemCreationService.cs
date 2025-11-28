using D2SLib.Configuration;
using D2SLib.Model.Api;
using D2SLib.Model.Save;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows.Markup;

namespace D2SLib.Services
{
    public class ItemCreationService
    {
        public Item CreateItemFromApiData(D2Item apiItem)
        {
            if (apiItem == null) return null;
            var flags = CreateItemFlags(apiItem);
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
                TotalNumberOfSockets = (byte)apiItem.SocketCount,
                Flags = flags,
                StatLists = new List<ItemStatList> { new() { Stats = new List<ItemStat>() } }
            };

            //if (newItem.Code == "usk")
            //{
            //    var json = JsonSerializer.Serialize(apiItem, new JsonSerializerOptions
            //    {
            //        WriteIndented = true // makes it pretty-printed
                    
            //    });
            //    Console.WriteLine("Created item:");
            //    Console.WriteLine(json);
            //    Console.WriteLine($"{apiItem.Socketed.Count}");
            //    newItem.TotalNumberOfSockets = (byte)3;
            //}

            //AddSocketsToItem(newItem);
            AddModifiersToItem(newItem, apiItem.Modifiers);
            return newItem;
        }

        public (int life, int lifeperlevel, int vitality, int vitalityperlevel, int percentmaxlife, int mana, int manaperlevel, int energy, int energyperlevel, int percentmaxmana) CalculateTotals(IEnumerable<Item> items)
        {
            int life = 0;
            int lifeperlevel = 0;
            int vitality = 0;
            int vitalityperlevel = 0;
            int percentmaxlife = 0;
            int mana = 0;
            int manaperlevel = 0;
            int energy = 0;
            int energyperlevel = 0;
            int percentmaxmana = 0;

            foreach (var item in items)
            {
                if (item?.StatLists == null)
                    continue;

                foreach (var statList in item.StatLists)
                {
                    if (statList?.Stats == null)
                        continue;

                    foreach (var stat in statList.Stats)
                    {
                        switch (stat.Stat.ToLowerInvariant())
                        {
                            case "maxhp":
                                life += stat.Value;
                                break;

                            case "item_hp_perlevel":
                                lifeperlevel += stat.Value / 8;
                                break;

                            case "vitality":
                                vitality += stat.Value;
                                break;

                            case "item_vitality_perlevel":
                                vitalityperlevel += stat.Value / 8;
                                break;

                            case "item_maxhp_percent":
                                percentmaxlife += stat.Value;
                                break;

                            case "maxmana":
                                mana += stat.Value;
                                break;

                            case "item_mana_perlevel":
                                manaperlevel += stat.Value / 8;
                                break;

                            case "energy":
                                energy += stat.Value;
                                break;

                            case "item_energy_perlevel":
                                energyperlevel += stat.Value / 8;
                                break;

                            case "item_maxmana_percent":
                                percentmaxmana += stat.Value;
                                break;
                        }
                    }

                }
            }
            return (life, lifeperlevel, vitality, vitalityperlevel, percentmaxlife, mana, manaperlevel, energy, energyperlevel, percentmaxmana);
        }

        //public Item CreateSimpleItem(byte x, byte y, byte page, string code, ushort quantity)
        //{
        //    var flags = CreateItemFlags();

        //    return new Item
        //    {
        //        Flags = flags,
        //        Mode = 0,
        //        Location = 0,
        //        Version = "101",
        //        ItemLevel = AppSettings.MaxItemLevel,
        //        Quality = ItemQuality.Normal,
        //        Id = AppSettings.DefaultItemId,
        //        Quantity = quantity,
        //        X = x,
        //        Y = y,
        //        Page = page,
        //        Code = code,
        //        StatLists = new List<ItemStatList> { new() { Stats = new List<ItemStat>() } }
        //    };
        //}

        public Item CreateSimpleItem(byte x, byte y, byte page, string code, ushort quantity)
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

        //public Item CreateMagicCharm(byte x, byte y, byte page, string code, List<ItemStat> stats)
        //{
        //    var flags = CreateItemFlags();

        //    var item = new Item
        //    {
        //        Flags = flags,
        //        Mode = 0,
        //        Location = 0,
        //        Quality = ItemQuality.Magic,
        //        ItemLevel = 1,
        //        X = x,
        //        Y = y,
        //        Page = page,
        //        Code = code,
        //        StatLists = new List<ItemStatList> { new() { Stats = stats } }
        //    };

        //    return item;
        //}

        public Item CreateMagicCharm(byte x, byte y, byte page, string code, List<ItemStat> stats)
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

        //private BitArray CreateItemFlags()
        //{
        //    var flags = new BitArray(32);
        //    flags[4] = true;  // IsIdentified
        //    flags[11] = false; // IsSocketed
        //    flags[13] = false; // IsNew
        //    flags[16] = false; // IsEar
        //    flags[17] = false; // IsStarterItem
        //    flags[21] = false; // IsCompact
        //    flags[22] = false; // IsEthereal
        //    flags[24] = false; // IsPersonalized
        //    flags[26] = false; // IsRuneword
        //    return flags;
        //}

        private BitArray CreateItemFlags(D2Item apiItem)
        {
            var flags = new BitArray(32);
            flags[4] = apiItem.IsIdentified;  // IsIdentified
            flags[11] = apiItem.IsSocketed; // IsSocketed
            flags[13] = false; // IsNew
            flags[16] = false; // IsEar
            flags[17] = false; // IsStarterItem
            flags[21] = false; // IsCompact
            flags[22] = apiItem.IsEthereal; // IsEthereal
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
                //AddSocketsToItem(item, modifier);
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

        //private void AddSocketsToItem(Item item)
        //{
        //    if (item.IsSocketed)
        //    {
        //        if (item.TotalNumberOfSockets > 0)
        //        {
        //            Console.WriteLine("Adding sockets to item...", item.TotalNumberOfSockets);
        //            item.TotalNumberOfSockets = item.Socketed.Count;
        //        }
        //    }
        //}

        private void AddGenericModifier(Item item, D2ItemModifier modifier)
        {
            if (modifier.Name.Contains("_perlevel"))
            {
                if (modifier.Name.Contains("item_tohit_perlevel"))
                {
                    item.StatLists[0].Stats.Add(new ItemStat
                    {
                        Stat = modifier.Name,
                        Value = (int)(modifier.Values[0] * 64)
                    });
                }
                else
                {
                    item.StatLists[0].Stats.Add(new ItemStat
                    {
                        Stat = modifier.Name, // For `perlevel` mods if its not an int we div by 8
                        Value = (int)((modifier.Values[0] % 1 != 0) ? modifier.Values[0] / 0.125 : modifier.Values[0])
                    });
                }        
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