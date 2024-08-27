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

namespace BetterFruitTrees
{
    public class ModEntry : Mod
    {
        internal static IMonitor SMonitor;
        internal static IModHelper SHelper;

        private ModConfig? config;
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

            config = helper.ReadConfig<ModConfig>();

            helper.Events.GameLoop.DayStarted += OnDayStarted;
            helper.Events.Input.ButtonPressed += OnButtonPressed;
            helper.Events.GameLoop.SaveCreated += OnSaveCreated;
            helper.Events.Content.AssetRequested += OnAssetRequested;

            fruitSpreadChance = config.FruitSpreadChance.Value;
            wildSpreadChance = config.WildSpreadChance.Value;
            spreadRadius = config.SpreadRadius.Value;
            denseTrees = config.DenseTrees.Value;
            fruitDaysToFinalStage = config.FruitDaysToFinalStage.Value;
            fruitFarmDaysToFinalStage = config.FruitFarmDaysToFinalStage.Value;
            wildDaysToFinalStage = config.WildDaysToFinalStage.Value;
            wildFarmDaysToFinalStage = config.WildFarmDaysToFinalStage.Value;
            daysToLargeTree = config.DaysToLargeTree.Value;
            largeTreeChance = config.LargeTreeChance.Value;
            winterGrowth = config.WinterGrowth.Value;
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
            foreach (GameLocation location in Game1.locations)
            {
                if (location.IsWinterHere() && !ModEntry.winterGrowth)
                {
                    Monitor.Log($"It's winter in {location.DisplayName}! Tree growth logic will be skipped.", LogLevel.Info);
                    TreeSpread.SpreadTrees(location, Monitor);
                }
                else
                {
                    TreeGrowth.UpdateTreeGrowth(location, Monitor);
                    TreeSpread.SpreadTrees(location, Monitor);
                }
                if (Game1.dayOfMonth.Equals(1))
                {
                    FertilizerExpansion.Unfertilize(location, Monitor);
                }
                FertilizerExpansion.GrowFruit(location, Monitor);
            }
        }

        // Hoe small fruit trees to pick up sapling
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
            foreach (GameLocation location in Game1.locations)
            {
                foreach (TerrainFeature feature in location.terrainFeatures.Values)
                {
                    if (feature is Tree tree)
                    {
                        TreeReplacer.ReplaceTree(tree, location, config, Monitor);
                    }
                }
            }
        }
    }
}
