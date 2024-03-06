using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace BetterFruitTrees
{
    public class ToolHelper
    {
        public static void DigSapling(object sender, ButtonPressedEventArgs e)
        {
            Vector2 selectedTile = Game1.currentCursorTile;

            if (Game1.currentLocation.terrainFeatures.TryGetValue(selectedTile, out TerrainFeature terrainFeature) && terrainFeature is FruitTree fruitTree)
            {
                // Check growth stage
                if (fruitTree.growthStage.Value <= 1)
                {
                    // Hoe dirt, use stamina
                    Game1.player.stamina -= ModEntry.energyCost;

                    bool treeDestroyed = Game1.currentLocation.terrainFeatures.Remove(selectedTile);

                    if (treeDestroyed)
                    {
                        // Get sapling item and drop it
                        string treeTypeId = fruitTree.treeId.ToString();
                        if (Game1.objectData.ContainsKey(treeTypeId))
                        {
                            Game1.createItemDebris(new StardewValley.Object(treeTypeId, 1), selectedTile * Game1.tileSize, -1);
                        }
                    }
                }
            }

        }
    }
}
