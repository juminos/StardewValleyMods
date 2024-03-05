using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using xTile.Dimensions;

namespace BetterFruitTrees
{
    public class ModEntry : Mod
    {
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
        private int energyCost;

        internal static bool IsEnabled = true;

        public override void Entry(IModHelper helper)
        {
            config = helper.ReadConfig<ModConfig>();

            helper.Events.GameLoop.DayStarted += OnDayStarted;
            helper.Events.Input.ButtonPressed += OnButtonPressed;
            helper.Events.GameLoop.SaveCreated += OnSaveCreated;

            fruitSpreadChance = config.FruitSpreadChance.Value;
            wildSpreadChance = config.WildSpreadChance.Value;
            fruitTreeDensityModifier = config.FruitTreeDensityModifier.Value;
            wildTreeDensityModifier = config.WildTreeDensityModifier.Value;
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
                    TreeSpread.SpreadFruitTrees(location);
                }
            }
            TreeGrowth.UpdateFruitTrees(this.Monitor);
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

            // Check if using Hoe and target tile
            if (e.Button.IsUseToolButton() && Game1.player.CurrentTool is Hoe)
            {
                Vector2 playerPosition = new Vector2(Game1.player.Tile.X, Game1.player.Tile.Y);
                Vector2 targetTile = ToolHelper.GetTargetTile(playerPosition, Game1.player.FacingDirection);

                if (Game1.currentLocation.terrainFeatures.TryGetValue(targetTile, out TerrainFeature terrainFeature) && terrainFeature is FruitTree fruitTree)
                {
                    // Check growth stage
                    if (fruitTree.growthStage.Value == 0 || fruitTree.growthStage.Value == 1 || fruitTree.growthStage.Value == 2)
                    {
                        // Hoe dirt, use stamina
                        Game1.player.stamina -= energyCost;

                        bool treeDestroyed = Game1.currentLocation.terrainFeatures.Remove(targetTile);

                        if (treeDestroyed)
                        {
                            // Get sapling item and drop it
                            string treeTypeId = fruitTree.treeId.ToString();
                            if (Game1.objectData.ContainsKey(treeTypeId))
                            {
                                Game1.createItemDebris(new StardewValley.Object(treeTypeId, 1), targetTile * Game1.tileSize, -1);
                            }
                        }
                    }
                }    
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
                        TreeReplacer.ReplaceTree(tree, location, config);
                    }
                }
            }
        }
    }
}
