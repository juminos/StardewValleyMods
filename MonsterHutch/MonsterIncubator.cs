using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData;
using StardewValley.GameData.BigCraftables;
using StardewValley.GameData.Machines;
using System;
using System.Collections.Generic;

namespace MonsterHutch
{
    internal class MonsterIncubator
    {
        public static readonly string monsterIncubatorNonQID = $"{ModEntry.Mod?.ModManifest?.UniqueID}.MonsterIncubator";
        public static readonly string monsterIncubatorQID = $"(BC){monsterIncubatorNonQID}";

        internal static void AddIncubatorAssetChanges(AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Data/CraftingRecipes"))
            {
                e.Edit((asset) =>
                {
                    IDictionary<string, string> data = asset.AsDictionary<string, string>().Data;

                    var recipeConfig = ModEntry.Config.IncubatorRecipe;
                    var recipeConfigUnlock = ModEntry.Config.IncubatorRecipeUnlock;

                    if (string.IsNullOrWhiteSpace(recipeConfig))
                    {
                        recipeConfig = ModConfig.DefaultIncubatorRecipe;
                    }
                    else
                    {
                        recipeConfig = recipeConfig.Trim();
                    }
                    if (string.IsNullOrWhiteSpace(recipeConfigUnlock))
                    {
                        recipeConfigUnlock = ModConfig.DefaultIncubatorRecipeUnlock;
                    }
                    else
                    {
                        recipeConfigUnlock = recipeConfigUnlock.Trim();
                    }

                    data[monsterIncubatorNonQID] = $"{recipeConfig}/Home/{monsterIncubatorNonQID}/true/{recipeConfigUnlock}/";
                });
            }

            if (e.NameWithoutLocale.IsEquivalentTo("Data/BigCraftables"))
            {
                e.Edit(asset =>
                {
                    var data = asset.AsDictionary<string, BigCraftableData>().Data;

                    if (data != null)
                    {
                        var monsterIncubator = new BigCraftableData
                        {
                            Name = monsterIncubatorNonQID,
                            DisplayName = ModEntry.Mod.Helper.Translation.Get("MonsterIncubator.name"),
                            Description = ModEntry.Mod.Helper.Translation.Get("MonsterIncubator.description"),
                            Price = 0,
                            Fragility = 0,
                            CanBePlacedOutdoors = true,
                            CanBePlacedIndoors = false,
                            IsLamp = false,
                            Texture = ModEntry.Mod.MonsterIncubatorAssetPath,
                            SpriteIndex = 0,
                            ContextTags = null,
                            CustomFields = null
                        };

                        data[monsterIncubatorNonQID] = monsterIncubator;
                    }
                });
            }

            if (e.NameWithoutLocale.IsEquivalentTo("Data/Machines"))
            {
                e.Edit((asset) =>
                {
                    IDictionary<string, MachineData> data = asset.AsDictionary<string, MachineData>().Data;

                    // most of the values of the monster incubator were set to be identical to the slime incubator
                    // in case the default values of these constructors change in the future, all values were defined

                    var monsterIncubator = new MachineData
                    {
                        HasInput = false,
                        HasOutput = false,
                        InteractMethod = null,
                        PreventTimePass = null,
                        ReadyTimeModifierMode = QuantityModifier.QuantityModifierMode.Stack,
                        InvalidItemMessage = null,
                        InvalidItemMessageCondition = null,
                        InvalidCountMessage = null,
                        WorkingEffects = null,
                        WorkingEffectChance = 0.33f,
                        AllowLoadWhenFull = false,
                        WobbleWhileWorking = ModEntry.Config.IncubatorWobblesWhileIncubating,
                        LightWhileWorking = null,
                        ShowNextIndexWhileWorking = false,
                        ShowNextIndexWhenReady = false,
                        AllowFairyDust = false,
                        IsIncubator = false, // this is not a bug, slime incubators are also not "incubators"
                        OnlyCompleteOvernight = true,
                        ClearContentsOvernightCondition = null,
                        StatsToIncrementWhenLoaded = null,
                        StatsToIncrementWhenHarvested = null,
                        CustomFields = null
                    };

                    bool coopmasterWouldResultInLessThanOneDay = ModEntry.Config.IncubatorDurationIsInDaysInsteadOfMinutes && ModEntry.Config.IncubatorDuration <= 1;

                    if (ModEntry.Config.IncubatorIsAffectedByCoopmaster && !coopmasterWouldResultInLessThanOneDay)
                    {
                        var coopmaster = new QuantityModifier
                        {
                            Id = "Coopmaster",
                            Condition = "PLAYER_HAS_PROFESSION Target 2",
                            Modification = QuantityModifier.ModificationType.Multiply,
                            Amount = ModEntry.Config.IncubatorDurationIsInDaysInsteadOfMinutes ? 0.45f : 0.5f,
                            RandomAmount = null
                        };

                        monsterIncubator.ReadyTimeModifiers = new List<QuantityModifier>() { coopmaster };
                    }
                    else
                    {
                        monsterIncubator.ReadyTimeModifiers = null;
                    }

                    var coinSound = new MachineSoundData
                    {
                        Id = "coin",
                        Delay = 0
                    };

                    var defaultMachineSpriteEffect = new MachineEffects
                    {
                        Id = "Default",
                        Condition = null,
                        Interval = -1,
                        Frames = null,
                        ShakeDuration = -1,
                        TemporarySprites = null,
                        Sounds = new List<MachineSoundData>() { coinSound }
                    };

                    monsterIncubator.LoadEffects = new List<MachineEffects>()
                    {
                        defaultMachineSpriteEffect
                    };

                    int incubatorDuration = Math.Max(1, ModEntry.Config.IncubatorDuration);

                    var defaultOutput = new MachineOutputRule
                    {
                        Id = "Default",
                        UseFirstValidOutput = true,
                        MinutesUntilReady = ModEntry.Config.IncubatorDurationIsInDaysInsteadOfMinutes ? -1 : incubatorDuration,
                        DaysUntilReady = ModEntry.Config.IncubatorDurationIsInDaysInsteadOfMinutes ? incubatorDuration : -1,
                        RecalculateOnCollect = false
                    };


                    var coalDisplayName = ItemRegistry.Create(ModEntry.coalId).DisplayName;
                    var bugMeatDisplayName = ItemRegistry.Create(ModEntry.bugMeatId).DisplayName;
                    var mummifiedBatDisplayName = ItemRegistry.Create(ModEntry.mummifiedBatId).DisplayName;
                    var cinderShardDisplayName = ItemRegistry.Create(ModEntry.cinderShardId).DisplayName;

                    defaultOutput.InvalidCountMessage = ModEntry.Mod.Helper.Translation.Get("MonsterIncubator.InputWarning"
                        , new { coalName = coalDisplayName, 
                            bugMeatName = bugMeatDisplayName, 
                            mummifiedBatName = mummifiedBatDisplayName, 
                            cinderShardName = cinderShardDisplayName
                        });

                    var coalTrigger = CreateMachineOutputTriggerRule(ModEntry.coalId);
                    var bugMeatTrigger = CreateMachineOutputTriggerRule(ModEntry.bugMeatId);
                    var mummifiedBatTrigger = CreateMachineOutputTriggerRule(ModEntry.mummifiedBatId);
                    var cinderShardTrigger = CreateMachineOutputTriggerRule(ModEntry.cinderShardId);

                    defaultOutput.Triggers = new List<MachineOutputTriggerRule> { 
                        coalTrigger, 
                        bugMeatTrigger, 
                        mummifiedBatTrigger, 
                        cinderShardTrigger };

                    var coalOutputItem = CreateMachineItemOutput("Coal");
                    var bugMeatOutputItem = CreateMachineItemOutput("BugMeat");
                    var mummifiedBatOutputItem = CreateMachineItemOutput("MummifiedBat");
                    var cinderShardOutputItem = CreateMachineItemOutput("CinderShard");

                    defaultOutput.OutputItem = new List<MachineItemOutput>() { 
                        coalOutputItem, 
                        bugMeatOutputItem, 
                        mummifiedBatOutputItem, 
                        cinderShardOutputItem };

                    monsterIncubator.OutputRules = new List<MachineOutputRule>() { defaultOutput };

                    if (ModEntry.Config.IncubatorAdditionalRequiredItemCount > 0)
                    {
                        int additionalItemCount = ModEntry.Config.IncubatorAdditionalRequiredItemCount;
                        string additionalItemID = ModEntry.Config.IncubatorAdditionalRequiredItemID ?? ModConfig.DefaultIncubatorAdditionalRequiredItemID;

                        Item additionalItem = ItemRegistry.Create(additionalItemID, allowNull: true);
                        if (additionalItem == null)
                        {
                            additionalItemID = ModConfig.DefaultIncubatorAdditionalRequiredItemID;
                        }
                        else
                        {
                            additionalItemID = additionalItem.QualifiedItemId;
                        }

                        var machineAdditionalConsumedItem = new MachineItemAdditionalConsumedItems
                        {
                            ItemId = additionalItemID,
                            RequiredCount = additionalItemCount
                        };

                        var additionalItemDisplayName = ItemRegistry.Create(machineAdditionalConsumedItem.ItemId).DisplayName;

                        machineAdditionalConsumedItem.InvalidCountMessage = ModEntry.Mod.Helper.Translation.Get("MonsterIncubator.additionalItemWarning"
                            , new { additionalItemName = additionalItemDisplayName, additionalItemCount = machineAdditionalConsumedItem.RequiredCount.ToString() });

                        monsterIncubator.AdditionalConsumedItems = new List<MachineItemAdditionalConsumedItems>() { machineAdditionalConsumedItem };
                    }
                    else
                    {
                        monsterIncubator.AdditionalConsumedItems = null;
                    }

                    data[monsterIncubatorQID] = monsterIncubator;
                });
            }
        }
        private static MachineOutputTriggerRule CreateMachineOutputTriggerRule(string requiredItemId)
        {
            if (requiredItemId == ModEntry.mummifiedBatId)
            {
                return new MachineOutputTriggerRule
                {
                    Id = "ItemPlacedInMachine",
                    Trigger = MachineOutputTrigger.ItemPlacedInMachine,
                    RequiredItemId = requiredItemId,
                    RequiredTags = null,
                    RequiredCount = 1,
                    Condition = null
                };
            }
            else
            {
                return new MachineOutputTriggerRule
                {
                    Id = "ItemPlacedInMachine",
                    Trigger = MachineOutputTrigger.ItemPlacedInMachine,
                    RequiredItemId = requiredItemId,
                    RequiredTags = null,
                    RequiredCount = 10,
                    Condition = null
                };
            }
        }

        private static MachineItemOutput CreateMachineItemOutput(string outputID)
        {
            string condition = null;
            int index = 1;
            switch (outputID)
            {
                case "Coal":
                    condition = $"ITEM_ID Input {ModEntry.coalId}";
                    index = 6;
                    break;

                case "BugMeat":
                    condition = $"ITEM_ID Input {ModEntry.bugMeatId}";
                    index = 9;
                    break;

                case "MummifiedBat":
                    condition = $"ITEM_ID Input {ModEntry.mummifiedBatId}";
                    index = 4;
                    break;

                case "CinderShard":
                    condition = $"ITEM_ID Input {ModEntry.cinderShardId}";
                    index = 8;
                    break;
            }
            return new MachineItemOutput
            {
                CustomData = null,
                OutputMethod = null,
                CopyColor = false,
                CopyPrice = false,
                CopyQuality = false,
                PreserveType = null,
                PreserveId = null,
                IncrementMachineParentSheetIndex = index,
                PriceModifiers = null,
                PriceModifierMode = QuantityModifier.QuantityModifierMode.Stack,
                Condition = condition,
                Id = outputID,
                ItemId = "DROP_IN",
                RandomItemId = null,
                MaxItems = null,
                MinStack = -1,
                MaxStack = -1,
                Quality = -1,
                ObjectInternalName = null,
                ObjectDisplayName = null,
                ToolUpgradeLevel = -1,
                IsRecipe = false,
                StackModifiers = null,
                StackModifierMode = QuantityModifier.QuantityModifierMode.Stack,
                QualityModifiers = null,
                QualityModifierMode = QuantityModifier.QuantityModifierMode.Stack,
                PerItemCondition = null
            };
        }
    }
}
