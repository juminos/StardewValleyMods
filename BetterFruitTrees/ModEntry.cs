using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.GameData.WildTrees;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using xTile.Dimensions;

namespace WilderTrees
{
    public class ModEntry : Mod
    {
        internal static IMonitor SMonitor;
        internal static IModHelper SHelper;

        private ModConfig Config;
        internal static float fruitSpreadChance;
        internal static float wildSpreadChance;
        internal static int spreadRadius;
        internal static bool denseTrees;
        internal static int fruitDaysToFinalStage;
        internal static int fruitFarmDaysToFinalStage;
        internal static int wildDaysToFinalStage;
        internal static int wildFarmDaysToFinalStage;
        internal static int daysToLargeTree;
        internal static float largeTreeChance;
        internal static bool winterGrowth;

        internal static bool IsEnabled = true;

        public override void Entry(IModHelper helper)
        {
            SMonitor = Monitor;
            SHelper = helper;

            Config = helper.ReadConfig<ModConfig>();

            helper.Events.GameLoop.DayStarted += OnDayStarted;
            helper.Events.Input.ButtonPressed += OnButtonPressed;
            helper.Events.GameLoop.SaveCreated += OnSaveCreated;
            helper.Events.Content.AssetRequested += OnAssetRequested;
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        }

        // Turn off wild tree spreading, allow tree planting anywhere
        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Data/WildTrees"))
            {
                e.Edit(asset =>
                {
                    var data = asset.AsDictionary<string, WildTreeData>().Data;

                    if (data != null)
                    {
                        foreach (var kvp in data)
                        {
                            kvp.Value.SeedSpreadChance = 0;
                        }
                    }
                    else
                    {
                        Monitor.Log("Failed to load or edit WildTrees data.");
                    }
                });
            }
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Locations"))
            {
                e.Edit(asset =>
                {
                    var data = asset.AsDictionary<string, StardewValley.GameData.Locations.LocationData>().Data;

                    if (data != null)
                    {
                        foreach (var kvp in data)
                        {
                            kvp.Value.CanPlantHere = true;
                        }
                    }
                    else
                    {
                        Monitor.Log("Failed to load or edit Locations data.");
                    }
                });
            }
        }

        // Update trees in all locations
        private void OnDayStarted(object sender, EventArgs e)
        {
            TreeSpread.SpreadTrees(Monitor, Config);
            TreeGrowth.UpdateTreeGrowth(Monitor, Config);
            if (Game1.dayOfMonth.Equals(1))
            {
                FertilizerExpansion.Unfertilize(Monitor, Config);
            }
            FertilizerExpansion.GrowFruit(Monitor);
            if (Config.ResetTrees == "remove" || Config.ResetTrees == "remove (farm)")
            {
                TreeReplacer.ResetTrees(Monitor, Config);
                Config.ResetTrees = "none";
            }
            if (Config.ResetTrees == "replace")
            {
                TreeReplacer.ReplaceTree(Config, Monitor);
                Config.ResetTrees = "none";
            }
        }

        // Enable fruit tree fertilizing
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            if (!IsEnabled)
                return;

            if (Game1.activeClickableMenu != null)
                return;

            // Fertilize trees
            if (e.Button.IsActionButton() && Game1.player.ActiveObject != null && Game1.player.ActiveObject.ParentSheetIndex == 805)
            {
                FertilizerExpansion.FertilizeFruitTrees(sender, e);
            }
        }

        // Randomize trees on save creation
        private void OnSaveCreated(object sender, EventArgs e)
        {
            TreeReplacer.ReplaceTree(Config, Monitor);
        }

        // Register config fields
        private void OnGameLaunched(object sender, EventArgs e)
        {
            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            // register mod
            configMenu.Register(
                mod: this.ModManifest,
                reset: () => this.Config = new ModConfig(),
                save: () => this.Helper.WriteConfig(this.Config)
            );

            configMenu.AddSectionTitle(
                mod: this.ModManifest,
                text: () => "Tree Spreading",
                tooltip: () => "Options for tree spreading logic"
                );

            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Fruit Tree Spread Chance",
                tooltip: () => "Chance a mature fruit tree will create a new sapling nearby each day it is in season (default 0.06)",
                getValue: () => this.Config.FruitSpreadChance,
                setValue: value => this.Config.FruitSpreadChance = value,
                min: 0.0f, 
                max: 1.0f,
                interval: 0.02f
                );

            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Wild Tree Spread Chance",
                tooltip: () => "Chance a mature wild tree will create a new sapling nearby each day (default 0.1)",
                getValue: () => this.Config.WildSpreadChance,
                setValue: value => this.Config.WildSpreadChance = value,
                min: 0.0f,
                max: 1.0f,
                interval: 0.02f
                );

            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Tree Spread Radius",
                tooltip: () => "Maximum tile radius saplings will grow around mature trees (default 3)",
                getValue: () => this.Config.SpreadRadius,
                setValue: value => this.Config.SpreadRadius = (int)value,
                min: 0,
                max: 8,
                interval: 1
                );

            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Tree Limit",
                tooltip: () => "Maximum number of trees per map before tree spreading stops (default 100)",
                getValue: () => this.Config.TreeLimit,
                setValue: value => this.Config.SpreadRadius = (int)value,
                min: 0,
                max: 1000,
                interval: 10
                );

            configMenu.AddSectionTitle(
                mod: this.ModManifest,
                text: () => "Tree Growth",
                tooltip: () => "Options for tree growth logic"
                );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Dense Spreading",
                tooltip: () => "Allow trees to spread adjacent to other trees (spreading logic will always favor non-adjacent tiles; default false)",
                getValue: () => this.Config.DenseTrees,
                setValue: value => this.Config.DenseTrees = value
                );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Even Denser Spreading",
                tooltip: () => "Allow trees to spread to adjacent tiles (spreading logic will always favor non-adjacent tiles; default false)",
                getValue: () => this.Config.DenserTrees,
                setValue: value => this.Config.DenserTrees = value
                );

            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Fruit Tree Growing Time",
                tooltip: () => "Average number of days for fruit trees to reach maturity (default 84)",
                getValue: () => this.Config.FruitDaysToFinalStage,
                setValue: value => this.Config.FruitDaysToFinalStage = (int)value,
                min: 4,
                max: 112,
                interval: 1
                );

            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Fruit Tree Growing Time (Farm)",
                tooltip: () => "Average number of days for fruit trees on the farm to reach maturity (default 84)",
                getValue: () => this.Config.FruitFarmDaysToFinalStage,
                setValue: value => this.Config.FruitFarmDaysToFinalStage = (int)value,
                min: 4,
                max: 112,
                interval: 1
                );

            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Wild Tree Growing Time",
                tooltip: () => "Average number of days for wild trees to reach maturity (default 28)",
                getValue: () => this.Config.WildDaysToFinalStage,
                setValue: value => this.Config.WildDaysToFinalStage = (int)value,
                min: 5,
                max: 112,
                interval: 1
                );

            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Wild Tree Growing Time (Farm)",
                tooltip: () => "Average number of days for wild trees on the farm to reach maturity (default 28)",
                getValue: () => this.Config.WildFarmDaysToFinalStage,
                setValue: value => this.Config.WildFarmDaysToFinalStage = (int)value,
                min: 5,
                max: 112,
                interval: 1
                );

            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Large Tree Growing Time",
                tooltip: () => "Minimum number of days until a mature tree grows into a large tree (default 56)",
                getValue: () => this.Config.DaysToLargeTree,
                setValue: value => this.Config.DaysToLargeTree = (int)value,
                min: 1,
                max: 112,
                interval: 1
                );

            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Large Tree Chance",
                tooltip: () => "Chance a fertilized mature tree will turn into a large tree each day after the growing time has passed (default 0.2)",
                getValue: () => this.Config.LargeTreeChance,
                setValue: value => this.Config.LargeTreeChance = value,
                min: 0.0f,
                max: 1.0f,
                interval: 0.02f
                );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Winter Growth",
                tooltip: () => "Allow trees to grow in winter (default false)",
                getValue: () => this.Config.WinterGrowth,
                setValue: value => this.Config.WinterGrowth = value
                );

            configMenu.AddSectionTitle(
                mod: this.ModManifest,
                text: () => "Tree Replacement",
                tooltip: () => "Options for tree replacement logic"
                );

            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Fruit Tree Replacement Chance",
                tooltip: () => "Chance of a wild tree being replaced by a fruit tree when tree replacement is run (default 0.04)",
                getValue: () => this.Config.FruitTreeChance,
                setValue: value => this.Config.FruitTreeChance = value,
                min: 0.0f,
                max: 1.0f,
                interval: 0.02f
                );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Random Tree Replacement",
                tooltip: () => "Make tree replacement completely randomized, ignoring weight configurations (default false)",
                getValue: () => this.Config.RandomTreeReplace,
                setValue: value => this.Config.RandomTreeReplace = value
                );

            configMenu.AddTextOption(
                mod: this.ModManifest,
                name: () => "Reset Trees\n(runs on next day start)",
                tooltip: () => "Remove: remove all trees (ignores farm)\nRemove (farm): remove all trees (including farm)\nReplace: run tree replacement on all wild trees",
                getValue: () => this.Config.ResetTrees,
                setValue: value => this.Config.ResetTrees = value,
                allowedValues: new string[] { "none", "remove", "remove (farm)", "replace" }
                );
        }
    }
}
