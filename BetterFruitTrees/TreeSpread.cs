using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace BetterFruitTrees
{
    public class TreeSpread
    {
        // Iterate through all mature trees
        public static void SpreadTrees(GameLocation location)
        {
            float customFruitCountdownFloat;
            float customWildCountdownFloat;
            if (location is Farm)
            {
                customFruitCountdownFloat = Math.Max(ModEntry.fruitDaysToFinalStage / ModEntry.farmFruitModifier, 1);
                customWildCountdownFloat = Math.Max(ModEntry.wildDaysToFinalStage / ModEntry.farmWildModifier, 1);
            }
            else
            {
                customFruitCountdownFloat = ModEntry.fruitDaysToFinalStage;
                customWildCountdownFloat = ModEntry.wildDaysToFinalStage;
            }
            int customFruitCountdown = (int)customFruitCountdownFloat;
            int customWildCountdown = (int)customWildCountdownFloat;

            // Check for spreading fruit trees
            var treesAndFruitTrees = location.terrainFeatures.Pairs
                .Where(pair => pair.Value is Tree || pair.Value is FruitTree)
                .ToArray();
            foreach (KeyValuePair<Vector2, TerrainFeature> pair in treesAndFruitTrees)
            {
                if (pair.Value is FruitTree fruitTree && fruitTree.growthStage.Value == 4)
                {
                    TrySpreadFruitTree(location, pair.Key, fruitTree, customFruitCountdown);
                }
            }

            // Check for spreading wild trees
            foreach (KeyValuePair<Vector2, TerrainFeature> pair in treesAndFruitTrees)
            {
                if (pair.Value is Tree wildTree && wildTree.growthStage.Value == 5)
                {
                    TrySpreadWildTree(location, pair.Key, wildTree, customWildCountdown);
                }
            }
        }

        // Check surrounding area for adding new fruit trees
        public static void TrySpreadFruitTree(GameLocation location, Vector2 treeTile, FruitTree fruitTree, int customFruitCountdown)
        {
            bool neighboringTreesPresent = false;

            // Iterate over surrounding tiles within spread radius
            for (int x = (int)treeTile.X - ModEntry.spreadFruitRadius; x <= (int)treeTile.X + ModEntry.spreadFruitRadius; x++)
            {
                for (int y = (int)treeTile.Y - ModEntry.spreadFruitRadius; y <= (int)treeTile.Y + ModEntry.spreadFruitRadius; y++)
                {
                    Vector2 tileLocation = new Vector2(x, y);

                    if (tileLocation == treeTile ||
                        !location.isTileOnMap(tileLocation) ||
                        !location.isTilePassable(tileLocation) ||
                        location.IsTileOccupiedBy(tileLocation))
                        continue;

                    // Check if the neighboring tile contains a tree
                    if (IsSurroundingAreaOvershadowed(tileLocation, location))
                    {
                        // Set the flag to true if neighboring trees are present
                        neighboringTreesPresent = true;
                        break;
                    }

                    int surroundedCount = CountOccupiedSurroundingTiles(tileLocation, location);

                    float adjustedSpreadChance = ModEntry.fruitSpreadChance / MathF.Pow((float)surroundedCount + 0.1f, 1.1f);

                    // Check if the tile is a suitable terrain type
                    string terrainType = location.doesTileHaveProperty((int)tileLocation.X, (int)tileLocation.Y, "Type", "Back");
                    if (terrainType == "Dirt" || terrainType == "Grass")
                    {
                        if (neighboringTreesPresent)
                        {
                            adjustedSpreadChance *= ModEntry.fruitTreeDensityModifier;
                        }
                        if (Game1.random.NextDouble() < adjustedSpreadChance)
                        {
                            string fruitTreeId = fruitTree.treeId.Value;
                            FruitTree newFruitTree = new FruitTree(fruitTreeId, 0);
                            newFruitTree.modData["CustomCountdown"] = customFruitCountdown.ToString();
                            int countdownFallback = 0;
                            newFruitTree.modData["CountdownFallback"] = countdownFallback.ToString();
                            location.terrainFeatures.Add(tileLocation, newFruitTree);
                        }
                    }
                }
            }
        }

        // Check surrounding area for adding new wild trees
        public static void TrySpreadWildTree(GameLocation location, Vector2 treeTile, Tree wildTree, int customWildCountdown)
        {
            bool neighboringTreesPresent = false;

            for (int x = (int)treeTile.X - ModEntry.spreadWildRadius; x <= (int)treeTile.X + ModEntry.spreadWildRadius; x++)
            {
                for (int y = (int)treeTile.Y - ModEntry.spreadWildRadius; y <= (int)treeTile.Y + ModEntry.spreadWildRadius; y++)
                {
                    Vector2 tileLocation = new Vector2(x, y);

                    if (tileLocation == treeTile ||
                        !location.isTileOnMap(tileLocation) ||
                        !location.isTilePassable(tileLocation) ||
                        location.IsTileOccupiedBy(tileLocation))
                        continue;

                    // Check if the neighboring tile contains a tree
                    if (IsSurroundingAreaOvershadowed(tileLocation, location))
                    {
                        // Set the flag to true if neighboring trees are present
                        neighboringTreesPresent = true;
                        break;
                    }

                    int surroundedCount = CountOccupiedSurroundingTiles(tileLocation, location);

                    float adjustedSpreadChance = ModEntry.wildSpreadChance / MathF.Pow((float)surroundedCount + 0.1f, 1.1f);

                    // Check if the tile is a suitable terrain type
                    string terrainType = location.doesTileHaveProperty((int)tileLocation.X, (int)tileLocation.Y, "Type", "Back");
                    if (terrainType == "Dirt" || terrainType == "Grass")
                    {
                        if (neighboringTreesPresent)
                        {
                            adjustedSpreadChance *= ModEntry.wildTreeDensityModifier;
                        }
                        if (Game1.random.NextDouble() < adjustedSpreadChance)
                        {
                            string wildTreeId = wildTree.treeType.Value.ToString();
                            Tree newWildTree = new Tree(wildTreeId, 0);
                            newWildTree.modData["CustomCountdown"] = customWildCountdown.ToString();
                            int countdownFallback = 0;
                            newWildTree.modData["CountdownFallback"] = countdownFallback.ToString();
                            location.terrainFeatures.Add(tileLocation, newWildTree);
                        }
                    }
                }
            }
        }

        // Check for nearby larger trees
        public static bool IsSurroundingAreaOvershadowed(Vector2 treeTile, GameLocation location)
        {
            Vector2[] surroundingTileLocationsArray = Utility.getSurroundingTileLocationsArray(treeTile);
            foreach (Vector2 tileLocation in surroundingTileLocationsArray)
            {
                if (location.terrainFeatures.TryGetValue(tileLocation, out TerrainFeature terrainFeature))
                { 
                    if (terrainFeature is FruitTree || terrainFeature is Tree)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        // Count occupied surrounding tiles
        public static int CountOccupiedSurroundingTiles(Vector2 treeTile, GameLocation location)
        {
            int occupiedCount = 0;
            Vector2[] surroundingTileLocationsArray = Utility.getSurroundingTileLocationsArray(treeTile);
            foreach (Vector2 tileLocation in surroundingTileLocationsArray )
            {
                if(!location.isTileOnMap(tileLocation))
                {
                    continue;
                }

                if (location.IsTileOccupiedBy(tileLocation))
                {
                    occupiedCount++;
                }
            }
            return occupiedCount;
        }
    }
}
