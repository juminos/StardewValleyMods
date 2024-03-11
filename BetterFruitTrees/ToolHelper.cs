using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.TerrainFeatures;
using xTile.Tiles;

namespace BetterFruitTrees
{
    public class ToolHelper
    {
        public static void DigSapling(object sender, ButtonPressedEventArgs e)
        {
            // Make surrounding fruit trees indestructible
            Dictionary<FruitTree, float> surroundingTrees = new Dictionary<FruitTree, float>();
            foreach (Vector2 surroundingTile in Utility.getSurroundingTileLocationsArray(Game1.player.Tile))
            {
                if (Game1.currentLocation.terrainFeatures.TryGetValue(surroundingTile, out TerrainFeature surroundingTerrainFeature) && surroundingTerrainFeature is FruitTree surroundingFruitTree)
                {
                    surroundingTrees.Add(surroundingFruitTree, surroundingFruitTree.health.Value);

                    // Dead tree is indestructible
                    surroundingFruitTree.health.Value = -99f;
                }
            }

            // Restore original health after delay
            Game1.delayedActions.Add(new DelayedAction(500, () =>
            {
                foreach (KeyValuePair<FruitTree, float> pair in surroundingTrees)
                {
                    // Restore health for all affected trees
                    if (pair.Key != null)
                    {
                        pair.Key.health.Value = pair.Value;
                    }
                }
            }));
            
            Vector2 targetTile = e.Cursor.GrabTile;

            // Check if the player is using a controller
            bool isUsingController = Game1.input.GetGamePadState().IsButtonDown(Buttons.X);

            // Move the tile further from the player if they're using a controller
            if (isUsingController)
            {
                targetTile = Game1.player.FacingDirection switch
                {
                    Game1.up => Game1.player.Tile - new Vector2(0, 1),
                    Game1.down => Game1.player.Tile + new Vector2(0, 1),
                    Game1.left => Game1.player.Tile - new Vector2(1, 0),
                    _ => Game1.player.Tile + new Vector2(1, 0)
                };
            }

            ModEntry.SMonitor.Log($"Checking tile {targetTile}");

            if (Game1.currentLocation.terrainFeatures.TryGetValue(targetTile, out TerrainFeature terrainFeature) &&
                terrainFeature is FruitTree fruitTree && fruitTree.growthStage.Value <= 1)
            {
                Game1.delayedActions.Add(new DelayedAction(200, () =>
                {
                    // Remove fruit tree
                    Game1.currentLocation.terrainFeatures.Remove(targetTile);
                    // Use stamina, hoe dirt
                    Game1.player.stamina -= ModEntry.energyCost;
                    Game1.currentLocation.makeHoeDirt(targetTile);
                }));

                // Drop sapling item at the location of the destroyed tree
                Game1.delayedActions.Add(new DelayedAction(300, () =>
                {
                    Game1.createItemDebris(new StardewValley.Object(fruitTree.treeId.ToString(), 1), targetTile * Game1.tileSize, -1);
                }));
                ModEntry.SMonitor.Log($"Sapling item dropped at tile: {targetTile}");
            }
        }
    }
}
