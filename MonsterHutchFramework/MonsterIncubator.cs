using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData;
using StardewValley.GameData.BigCraftables;
using StardewValley.GameData.Machines;


namespace MonsterHutchFramework
{
    internal class MonsterIncubator
    {
        public static readonly string monsterIncubatorNonQID = $"{ModEntry.Mod?.ModManifest?.UniqueID}.MonsterIncubator";
        public static readonly string monsterIncubatorQID = $"(BC){monsterIncubatorNonQID}";
        public static readonly string monsterIncubatorMailID = $"{ModEntry.Mod?.ModManifest?.UniqueID}.MonsterIncubatorMail";
        public static readonly string monsterIncubatorMailTriggerId = $"{ModEntry.Mod?.ModManifest?.UniqueID}.MonsterIncubatorTrigger";

        internal static void AddIncubatorAssetChanges(AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Data/CraftingRecipes"))
            {
                e.Edit((asset) =>
                {
                    var data = asset.AsDictionary<string, string>().Data;

                    var recipeConfig = ModEntry.Config.IncubatorRecipe;
                    var recipeConfigUnlock = ModEntry.Config.IncubatorRecipeUnlock;

                    if (string.IsNullOrWhiteSpace(recipeConfig))
                        recipeConfig = ModConfig.DefaultIncubatorRecipe;
                    else
                        recipeConfig = recipeConfig.Trim();
                    if (string.IsNullOrWhiteSpace(recipeConfigUnlock))
                        recipeConfigUnlock = ModConfig.DefaultIncubatorRecipeUnlock;
                    else
                        recipeConfigUnlock = recipeConfigUnlock.Trim();

                    data[monsterIncubatorNonQID] = $"{recipeConfig}/Home/{monsterIncubatorNonQID}/true/{recipeConfigUnlock}/";
                });
            }
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Mail") && (string.IsNullOrWhiteSpace(ModEntry.Config.IncubatorRecipeUnlock) || ModEntry.Config.IncubatorRecipeUnlock.Contains("f Wizard")))
            {
                e.Edit((asset) =>
                {
                    var data = asset.AsDictionary<string, string>().Data;
                    data[monsterIncubatorMailID] = $"[letterbg 2]{ModEntry.Mod.Helper.Translation.Get("WizardLetter.content")}%item craftingRecipe {monsterIncubatorNonQID} %%[#]{ModEntry.Mod.Helper.Translation.Get("WizardLetter.description")}";
                });
            }
            if (e.NameWithoutLocale.IsEquivalentTo("Data/TriggerActions") && (string.IsNullOrWhiteSpace(ModEntry.Config.IncubatorRecipeUnlock) || ModEntry.Config.IncubatorRecipeUnlock.Contains("f Wizard")))
            {
                e.Edit((asset) =>
                {
                    var data = asset.GetData<List<TriggerActionData>>();
                    if (data != null)
                    {
                        data.Add(new()
                        {
                            Id = monsterIncubatorMailTriggerId,
                            Trigger = "DayEnding",
                            Condition = "PLAYER_HEARTS Current Wizard 6",
                            Action = $"AddMail Current {monsterIncubatorMailID}",
                        });
                    }
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
                        WorkingEffectChance = 1,
                        AllowLoadWhenFull = false,
                        WobbleWhileWorking = ModEntry.Config.IncubatorWobblesWhileIncubating,
                        LightWhileWorking = null,
                        ShowNextIndexWhileWorking = false,
                        ShowNextIndexWhenReady = false,
                        AllowFairyDust = false,
                        IsIncubator = false,
                        OnlyCompleteOvernight = true,
                        ClearContentsOvernightCondition = null,
                        StatsToIncrementWhenLoaded = null,
                        StatsToIncrementWhenHarvested = null,
                        CustomFields = null
                    };
                    if (ModEntry.Config.IncubatorIsAffectedByCoopmaster)
                    {
                        var coopmaster = new QuantityModifier
                        {
                            Id = "Coopmaster",
                            Condition = "PLAYER_HAS_PROFESSION Target 2",
                            Modification = QuantityModifier.ModificationType.Multiply,
                            Amount = 0.45f,
                            RandomAmount = null
                        };
                        monsterIncubator.ReadyTimeModifiers = new List<QuantityModifier>() { coopmaster };
                    }
                    else
                        monsterIncubator.ReadyTimeModifiers = null;

                    var coinSound = new MachineSoundData
                    {
                        Id = "coin",
                        Delay = 0
                    };

                    var defaultMachineSpriteEffect = new MachineEffects
                    {
                        Id = "Default",
                        Condition = null,
                        Interval = 100,
                        Frames = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 },
                        ShakeDuration = 400,
                        Sounds = new List<MachineSoundData>() { coinSound }
                    };
                    monsterIncubator.LoadEffects = new List<MachineEffects>() { defaultMachineSpriteEffect };

                    var bubblesSound = new MachineSoundData
                    {
                        Id = "bubbles",
                        Delay = 0
                    };
                    var workingEffect = new MachineEffects
                    {
                        Id = "Default",
                        Condition = null,
                        Interval = 100,
                        Frames = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 },
                        ShakeDuration = 400,
                        Sounds = new List<MachineSoundData>() { bubblesSound },
                    };
                    monsterIncubator.WorkingEffects = new List<MachineEffects>() { workingEffect };

                    monsterIncubator.OutputRules = new List<MachineOutputRule>();
                    foreach (var monsterData in AssetHandler.monsterHutchData)
                    {
                        int incubatorDuration = Math.Max(1, monsterData.Value.IncubationTime);
                        var defaultOutput = new MachineOutputRule
                        {
                            Id = monsterData.Key,
                            UseFirstValidOutput = true,
                            MinutesUntilReady = incubatorDuration,
                            RecalculateOnCollect = false
                        };
                        var itemDisplayName = ItemRegistry.Create(monsterData.Value.InputItemId).DisplayName;
                        defaultOutput.InvalidCountMessage = ModEntry.Mod.Helper.Translation.Get("MonsterIncubator.InputWarning", new { itemName = itemDisplayName });

                        var itemTrigger = CreateMachineOutputTriggerRule(monsterData.Key, monsterData.Value);
                        defaultOutput.Triggers = new List<MachineOutputTriggerRule> { itemTrigger };

                        var itemOutputItem = CreateMachineItemOutput(monsterData.Key, monsterData.Value);
                        defaultOutput.OutputItem = new List<MachineItemOutput>() { itemOutputItem };

                        monsterIncubator.OutputRules.Add(defaultOutput);
                    }
                    if (ModEntry.Config.IncubatorAdditionalRequiredItemCount > 0)
                    {
                        int additionalItemCount = ModEntry.Config.IncubatorAdditionalRequiredItemCount;
                        string additionalItemID = ModEntry.Config.IncubatorAdditionalRequiredItemID ?? ModConfig.DefaultIncubatorAdditionalRequiredItemID;
                        Item additionalItem = ItemRegistry.Create(additionalItemID, allowNull: true);
                        if (additionalItem == null)
                            additionalItemID = ModConfig.DefaultIncubatorAdditionalRequiredItemID;
                        else
                            additionalItemID = additionalItem.QualifiedItemId;

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
                        monsterIncubator.AdditionalConsumedItems = null;

                    data[monsterIncubatorQID] = monsterIncubator;
                });
            }
        }
        private static MachineOutputTriggerRule CreateMachineOutputTriggerRule(string key, MonsterHutchData monsterHutchData)
        {
            return new MachineOutputTriggerRule
            {
                Id = key,
                Trigger = MachineOutputTrigger.ItemPlacedInMachine,
                RequiredItemId = monsterHutchData.InputItemId,
                RequiredTags = null,
                RequiredCount = monsterHutchData.InputItemCount,
                Condition = null
            };
        }

        private static MachineItemOutput CreateMachineItemOutput(string key, MonsterHutchData monsterHutchData)
        {
            string condition = null;
            int index = 1;
            condition = $"ITEM_ID Input {monsterHutchData.InputItemId}";
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
                Id = key,
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
