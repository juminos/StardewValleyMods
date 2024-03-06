using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.TerrainFeatures;

namespace BetterFruitTrees
{
    public class TreeSpread
    {
        // Iterate through all mature trees
        public static void SpreadTrees(GameLocation location)
        {
            // Check for spreading fruit trees
            var treesAndFruitTrees = location.terrainFeatures.Pairs
                .Where(pair => pair.Value is Tree || pair.Value is FruitTree)
                .ToArray();
            foreach (KeyValuePair<Vector2, TerrainFeature> pair in treesAndFruitTrees)
            {
                if (pair.Value is FruitTree fruitTree &&
                    fruitTree.growthStage.Value == 4) // && pass random check against fruitSpreadChance before trySpreadFruitTree, update method to add 1 sapling at random with weighted conditions
                {
                    TrySpreadFruitTree(location, pair.Key, fruitTree);
                }
            }


            // Check for spreading wild trees
            foreach (KeyValuePair<Vector2, TerrainFeature> pair in treesAndFruitTrees)
            {
                if (pair.Value is Tree wildTree && wildTree.growthStage.Value == 5)
                {
                    TrySpreadWildTree(location, pair.Key, wildTree);
                }
            }
        }

        // Check surrounding area for adding new fruit trees
        public static void TrySpreadFruitTree(GameLocation location, Vector2 treeTile, FruitTree fruitTree)
        {
            bool neighboringTreesPresent = false;

            // Iterate over surrounding tiles within spread radius
            for (int x = (int)treeTile.X - ModEntry.spreadRadius; x <= (int)treeTile.X + ModEntry.spreadRadius; x++)
            {
                for (int y = (int)treeTile.Y - ModEntry.spreadRadius; y <= (int)treeTile.Y + ModEntry.spreadRadius; y++)
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
                            location.terrainFeatures.Add(tileLocation, newFruitTree);
                        }
                    }
                }
            }
        }

        // Check surrounding area for adding new wild trees
        public static void TrySpreadWildTree(GameLocation location, Vector2 treeTile, Tree wildTree)
        {
            bool neighboringTreesPresent = false;

            for (int x = (int)treeTile.X - ModEntry.spreadRadius; x <= (int)treeTile.X + ModEntry.spreadRadius; x++)
            {
                for (int y = (int)treeTile.Y - ModEntry.spreadRadius; y <= (int)treeTile.Y + ModEntry.spreadRadius; y++)
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
                foreach (Building building in location.buildings)
                {
                    if (building.occupiesTile(tileLocation) && building.modData.ContainsKey("IsLargeTree"))
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
