using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;

namespace WilderTrees
{
    internal class FertilizerExpansion
    {
        public static void FertilizeFruitTrees(object sender, ButtonPressedEventArgs e)
        {
            Vector2 selectedTile = Game1.GetPlacementGrabTile();
            if (Game1.IsPerformingMousePlacement())
            {
                selectedTile = Game1.currentCursorTile;
            }

            if (Game1.currentLocation.terrainFeatures.TryGetValue(selectedTile, out TerrainFeature terrainFeature))
            {
                if ((terrainFeature is FruitTree fruitTreeW || terrainFeature is Tree wildTreeW) && Game1.IsWinter)
                {
                    Game1.addHUDMessage(new HUDMessage("Wait until spring to fertilize trees", 3));
                }
                else if (terrainFeature is FruitTree fruitTree)
                {
                    if (!fruitTree.modData.ContainsKey("Fertilized") || (fruitTree.modData.ContainsKey("Fertilized") && fruitTree.modData["Fertilized"] != "true"))
                    {
                        fruitTree.modData["Fertilized"] = "true";
                        Game1.player.reduceActiveItemByOne();
                        Game1.playSound("hoeHit");
                    }
                    else
                    {
                        Game1.addHUDMessage(new HUDMessage("Tree is already fertilized", 3));
                    }
                }
                else if (terrainFeature is Tree wildTree && 
                    wildTree.growthStage.Value >= 5)
                {
                    if (!wildTree.modData.ContainsKey("Fertilized") || (wildTree.modData.ContainsKey("Fertilized") && wildTree.modData["Fertilized"] != "true"))
                    {
                        wildTree.modData["Fertilized"] = "true";
                        Game1.player.reduceActiveItemByOne();
                        Game1.playSound("hoeHit");
                    }
                    else
                    {
                        Game1.addHUDMessage(new HUDMessage("Tree is already fertilized", 3));

                    }
                }
            }
        }
        public static void Unfertilize(GameLocation location, IMonitor monitor, ModConfig config)
        {
            foreach (KeyValuePair<Vector2, TerrainFeature> pair in location.terrainFeatures.Pairs)
            {
                if (pair.Value is FruitTree fruitTree && fruitTree.modData.ContainsKey("Fertilized"))
                {
                    fruitTree.modData.Remove("Fertilized");
                }
                if (pair.Value is Tree wildTree && location.IsWinterHere() && !config.WinterGrowth)
                {
                    if (wildTree.modData.ContainsKey("Fertilized"))
                    {
                        wildTree.modData.Remove("Fertilized");
                    }
                    if (wildTree.fertilized.Value == true)
                    {
                        wildTree.fertilized.Value = false;
                    }
                }
            }
        }
        public static void GrowFruit (GameLocation location, IMonitor monitor)
        {
            foreach (KeyValuePair<Vector2, TerrainFeature> pair in location.terrainFeatures.Pairs)
            {
                if (pair.Value is FruitTree fruitTree && fruitTree.IsInSeasonHere() && fruitTree.modData.ContainsKey("Fertilized") && fruitTree.modData["Fertilized"] == "true")
                {
                    if (Game1.random.NextDouble() < 0.7)
                    {
                        fruitTree.TryAddFruit();
                        if (Game1.random.NextDouble() < 0.2)
                        {
                            fruitTree.TryAddFruit();
                        }
                    }
                }
            }
        }
    }
}
