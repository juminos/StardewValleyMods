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
        internal static float fruitTreeDensityModifier;
        internal static float wildTreeDensityModifier;
        internal static int spreadRadius;
        internal static int fruitDaysToFinalStage;
        internal static int fruitFarmDaysToFinalStage;
        internal static int wildDaysToFinalStage;
        internal static int wildFarmDaysToFinalStage;
        internal static int daysToLargeTree;
        internal static float largeTreeChance;
        internal static bool winterGrowth;
        internal static int energyCost;

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
            fruitDaysToFinalStage = config.FruitDaysToFinalStage.Value;
            fruitFarmDaysToFinalStage = config.FruitFarmDaysToFinalStage.Value;
            wildDaysToFinalStage = config.WildDaysToFinalStage.Value;
            wildFarmDaysToFinalStage = config.WildFarmDaysToFinalStage.Value;
            daysToLargeTree = config.DaysToLargeTree.Value;
            largeTreeChance = config.LargeTreeChance.Value;
            winterGrowth = config.WinterGrowth.Value;
            energyCost = config.EnergyCost;
        }

        // Turn off wild tree spreading, allow tree planting anywhere
        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Data/WildTrees"))
            {
                e.Edit(asset =>
                {
                    var data = asset.AsDictionary<string, JObject>().Data;

                    if (data != null)
                    {
                        foreach (var kvp in data)
                        {
                            var treeId = kvp.Key;
                            var treeData = kvp.Value;

                            treeData["SeedPlantChance"] = "0.0";

                            data[treeId] = treeData;
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
            if (Game1.IsWinter)
            {
                Monitor.Log("It's winter! Tree spreading logic will be skipped.", LogLevel.Info);
            }
            else
            {
                foreach (GameLocation location in Game1.locations)
                {
                    TreeSpread.SpreadTrees(location, Monitor);
                }
            }
            TreeGrowth.UpdateTreeGrowth(Monitor);
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

            // Check if using Hoe and target tile
            if (e.Button.IsUseToolButton() && Game1.player.CurrentTool is Hoe)
            {
                ToolHelper.DigSapling(sender, e);
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
