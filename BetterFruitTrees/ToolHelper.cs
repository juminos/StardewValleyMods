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
            Vector2 selectedTile = Game1.player.GetToolLocation();

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
                            DelayedAction delayedAction = new DelayedAction(400, () => DropItem(selectedTile, treeTypeId));
                            Game1.delayedActions.Add(delayedAction);
                        }
                    }
                }
                // Protect larger growing trees
                else if (fruitTree.growthStage.Value > 1)
                {
                    float originalHealth = fruitTree.health.Value;

                    // Dead tree is indestructible
                    fruitTree.health.Value = -99f;

                    // Restore original health after delay
                    Game1.delayedActions.Add(new DelayedAction(500, () => RestoreHealth(fruitTree, originalHealth)));
                }
            }
        }
        private static void RestoreHealth(FruitTree fruitTree, float originalHealth)
        {
            fruitTree.health.Value = originalHealth;
        }
        private static void DropItem(Vector2 tileLocation, string treeTypeId)
        {
            Game1.createItemDebris(new StardewValley.Object(treeTypeId, 1), tileLocation * Game1.tileSize, -1);
        }
    }
}
