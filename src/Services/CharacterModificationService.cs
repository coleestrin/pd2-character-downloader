using System;
using System.Reflection;
using D2SLib.Configuration;
using D2SLib.Model.Api;
using D2SLib.Model.Save;

namespace D2SLib.Services
{
    public class CharacterModificationService
    {
        private readonly ItemCreationService _itemCreationService;

        public CharacterModificationService(ItemCreationService itemCreationService)
        {
            _itemCreationService = itemCreationService ?? throw new System.ArgumentNullException(nameof(itemCreationService));
        }

        public void CompleteAllQuests(D2S character)
        {
            var difficulties = new[]
            {
                character.Quests.Normal,
                character.Quests.Nightmare,
                character.Quests.Hell
            };

            foreach (var difficulty in difficulties)
            {
                var acts = new dynamic[]
                {
                    difficulty.ActI,
                    difficulty.ActII,
                    difficulty.ActIII,
                    difficulty.ActIV,
                    difficulty.ActV
                };

                foreach (var act in acts)
                {
                    CompleteActQuests(act);
                }
            }
            character.Quests.Normal.ActV.PrisonOfIce.Custom3 = true;
            character.Quests.Nightmare.ActV.PrisonOfIce.Custom3 = true;
            character.Quests.Hell.ActV.PrisonOfIce.Custom3 = true;
        }

        public void UnlockAllWaypoints(D2S character)
        {
            var wpDifficulties = new[]
            {
                character.Waypoints.Normal,
                character.Waypoints.Nightmare,
                character.Waypoints.Hell
            };

            foreach (var difficulty in wpDifficulties)
            {
                var acts = new dynamic[]
                {
                    difficulty.ActI,
                    difficulty.ActII,
                    difficulty.ActIII,
                    difficulty.ActIV,
                    difficulty.ActV
                };

                foreach (var act in acts)
                {
                    UnlockActWaypoints(act);
                }
            }
        }

        public void ActivateAllDifficulties(D2S character)
        {
            character.Location.Normal.Active = false;
            character.Location.Normal.Act = 5;

            character.Location.Nightmare.Active = false;
            character.Location.Nightmare.Act = 5;

            character.Location.Hell.Active = false;
            character.Location.Hell.Act = 5;

            character.Progression = 15;
        }

        public void SetupNpcDialogs(D2S character)
        {
            var difficulties = new[]
            {
                character.NPCDialog.Normal,
                character.NPCDialog.Nightmare,
                character.NPCDialog.Hell
            };

            foreach (var difficulty in difficulties)
            {
                if (difficulty == null) continue;

                var npcProperties = difficulty
                    .GetType()
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance);

                foreach (var npcProperty in npcProperties)
                {
                    var npcObj = npcProperty.GetValue(difficulty);
                    if (npcObj == null) continue;

                    var introProp = npcObj.GetType().GetProperty("Introduction");
                    var congratsProp = npcObj.GetType().GetProperty("Congratulations");

                    if (introProp != null && congratsProp != null)
                    {
                        introProp.SetValue(npcObj, false);
                        congratsProp.SetValue(npcObj, false);
                    }
                }
            }
        }

        public void SetGoldAmounts(D2S character)
        {
            character.Attributes.Stats["goldbank"] = AppSettings.MaxGold;
            character.Attributes.Stats["gold"] = AppSettings.CharacterGold;
        }

        public void AddItemsFromApiData(D2S character, System.Collections.Generic.List<D2Item> items)
        {

            if (character.PlayerItemList?.Items == null || items == null)
            {
                return;
            }

            character.PlayerItemList.Items.Clear();
            character.PlayerItemList.Count = 0;

            int itemIndex = 0;
            foreach (var apiItem in items)
            {
                try
                {
                    if (apiItem == null) continue;

                    var item = _itemCreationService.CreateItemFromApiData(apiItem);
                    if (item != null && ShouldIncludeItem(apiItem))
                    {
                        character.PlayerItemList.Items.Add(item);
                        character.PlayerItemList.Count++;
                    }
                    itemIndex++;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        public void AddMercenaryItemsFromApiData(D2S character, System.Collections.Generic.List<D2Item> items)
        {
            foreach (var apiItem in items)
            {
                if ((apiItem.Location.EquipmentId <= 0 || apiItem.Location.Zone != "Equipped") && !apiItem.Base.Id.StartsWith("cm"))
                {
                    continue; 
                }

                var item = _itemCreationService.CreateItemFromApiData(apiItem);
                if (item != null)
                {
                    character.MercenaryItemList.ItemList.Count++;
                    character.MercenaryItemList.ItemList.Items.Add(item);
                }
            }
        }

        public void ApplySkills(D2S character, System.Collections.Generic.List<D2SLib.Model.Api.Skill> apiSkills)
        {
            foreach (var apiSkill in apiSkills)
            {
                foreach (var skill in character.ClassSkills.Skills)
                {
                    if (skill.Id == apiSkill.Id)
                    {
                        skill.Points = (byte)apiSkill.Level;
                    }
                }
            }
        }

        public void ApplyMercenaryData(D2S character, D2SLib.Model.Api.Mercenary mercenary)
        {
            character.Mercenary.Experience = (uint)mercenary.Experience;
            character.Mercenary.Id = (uint)mercenary.Id;
            character.Mercenary.IsDead = 0;
            character.Mercenary.TypeId = (ushort)mercenary.Type;
            character.Mercenary.NameId = (ushort)mercenary.NameId;

            character.MercenaryItemList.ItemList = new ItemList
            {
                Header = AppSettings.MercenaryItemListHeader,
                Items = new System.Collections.Generic.List<Item>(),
                Count = 0
            };

            AddMercenaryItemsFromApiData(character, mercenary.Items);
        }

        private void CompleteActQuests(dynamic act)
        {
            var properties = act.GetType().GetProperties();

            foreach (var prop in properties)
            {
                if (prop.PropertyType == typeof(Quest))
                {
                    var quest = prop.GetValue(act) as Quest;
                    if (quest != null)
                    {
                        quest.RewardGranted = true;
                        quest.RewardPending = false;
                        quest.Started = true;
                        quest.PrimaryGoalAchieved = true;
                        quest.CompletedNow = false;
                        quest.CompletedBefore = true;
                        quest.QuestLog = true;
                    }
                }
            }
        }

        private void UnlockActWaypoints(dynamic act)
        {
            var properties = act.GetType().GetProperties();

            foreach (var prop in properties)
            {
                if (prop.PropertyType == typeof(bool))
                {
                    prop.SetValue(act, true);
                }
            }
        }

        private bool ShouldIncludeItem(D2Item item)
        {
            return (item.Location.EquipmentId > 0 && item.Location.Zone == "Equipped") ||
                   item.Base.Id.StartsWith("cm");
        }
    }
}