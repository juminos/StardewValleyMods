using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;

namespace BetterFruitTrees
{
    internal class FertilizerExpansion
    {
        public static void FertilizeFruitTrees(object sender, ButtonPressedEventArgs e)
        {
            Vector2 selectedTile = Game1.currentCursorTile;

            if (Game1.currentLocation.terrainFeatures.TryGetValue(selectedTile, out TerrainFeature terrainFeature))
            {
                if (terrainFeature is FruitTree fruitTree)
                {
                    fruitTree.modData["Fertilized"] = "true";
                    Game1.player.reduceActiveItemByOne();
                    Game1.playSound("hoeHit");
                }
                else if (terrainFeature is Tree wildTree)
                {
                    wildTree.modData["Fertilized"] = "true";
                    Game1.player.reduceActiveItemByOne();
                    Game1.playSound("hoeHit");
                }
            }
        }
    }
}
