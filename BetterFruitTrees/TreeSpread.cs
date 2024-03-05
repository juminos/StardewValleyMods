using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace BetterFruitTrees
{
    public class TreeSpread
    {
        // Iterate through all mature trees
        public static void SpreadFruitTrees(GameLocation location)
        {
            List<Tuple<Vector2, string>> newFruitTreesInfo = new List<Tuple<Vector2, string>>();
            List<Tuple<Vector2, string>> newWildTreesInfo = new List<Tuple<Vector2, string>>();

            // Check for spreading fruit trees
            foreach (KeyValuePair<Vector2, TerrainFeature> pair in location.terrainFeatures.Pairs)
            {
                if (pair.Value is FruitTree fruitTree && fruitTree.growthStage.Value == 4) // && pass random check against fruitSpreadChance before trySpreadFruitTree, update method to add 1 sapling at random with weighted conditions
                {
                    TrySpreadFruitTree(location, pair.Key, fruitTree, newFruitTreesInfo);
                }
            }

            // Add new fruit trees to terrain features after all existing fruit trees have been checked
            foreach (Tuple<Vector2, string> fruitTreeInfo in newFruitTreesInfo)
            {
                Vector2 tileLocation = fruitTreeInfo.Item1;
                string treeId = fruitTreeInfo.Item2;

                FruitTree newFruitTree = new FruitTree(treeId, 0);
                location.terrainFeatures.Add(tileLocation, newFruitTree);
            }

            // Check for spreading wild trees
            foreach (KeyValuePair<Vector2, TerrainFeature> pair in location.terrainFeatures.Pairs)
            {
                if (pair.Value is Tree wildTree && wildTree.growthStage.Value == 5)
                {
                    TrySpreadWildTree(location, pair.Key, wildTree, newWildTreesInfo);
                }
            }

            // Add new wild trees to terrain features after all existing wild trees have been checked
            foreach (Tuple<Vector2, string> wildTreeInfo in newWildTreesInfo)
            {
                Vector2 tileLocation = wildTreeInfo.Item1;
                string treeId = wildTreeInfo.Item2;

                Tree newWildTree = new Tree(treeId, 0);
                location.terrainFeatures.Add(tileLocation, newWildTree);
            }

        }

        // Check surrounding area for adding new fruit trees
        public static void TrySpreadFruitTree(GameLocation location, Vector2 treeTile, FruitTree fruitTree, List<Tuple<Vector2, string>> newFruitTreesInfo)
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
                        location.IsTileOccupiedBy(tileLocation) ||
                        newFruitTreesInfo.Any(info => info.Item1 == tileLocation))
                        continue;

                    // Check if the neighboring tile contains a tree
                    if (IsSurroundingAreaBlocked(tileLocation, location))
                    {
                        // Set the flag to true if neighboring trees are present
                        neighboringTreesPresent = true;
                        break;
                    }

                    float adjustedSpreadChance = ModEntry.fruitSpreadChance;

                    // Check if the tile is a suitable terrain type
                    string terrainType = location.doesTileHaveProperty((int)tileLocation.X, (int)tileLocation.Y, "Type", "Back");
                    if (terrainType == "Dirt" || terrainType == "Grass")
                    {
                        // Check if the tile location already exists in the list
                        if (!newFruitTreesInfo.Any(info => info.Item1 == tileLocation))
                        {
                            if (neighboringTreesPresent)
                            {
                                adjustedSpreadChance *= ModEntry.fruitTreeDensityModifier;
                                if (Game1.random.NextDouble() < adjustedSpreadChance)
                                {
                                    newFruitTreesInfo.Add(new Tuple<Vector2, string>(tileLocation, fruitTree.treeId.Value));
                                }
                            }
                            // Calculate chance of spreading
                            else if (!neighboringTreesPresent)
                            {
                                if (Game1.random.NextDouble() < ModEntry.fruitSpreadChance)
                                {
                                    newFruitTreesInfo.Add(new Tuple<Vector2, string>(tileLocation, fruitTree.treeId.Value));
                                }
                            }
                        }
                    }
                }
            }
        }

        // Check surrounding area for adding new wild trees
        public static void TrySpreadWildTree(GameLocation location, Vector2 treeTile, Tree wildTree, List<Tuple<Vector2, string>> newWildTreesInfo)
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
                        location.IsTileOccupiedBy(tileLocation) ||
                        newWildTreesInfo.Any(info => info.Item1 == tileLocation))
                        continue;

                    // Check if the neighboring tile contains a tree
                    if (IsSurroundingAreaBlocked(tileLocation, location))
                    {
                        // Set the flag to true if neighboring trees are present
                        neighboringTreesPresent = true;
                        break;
                    }

                    float adjustedSpreadChance = ModEntry.wildSpreadChance;

                    // Check if the tile is a suitable terrain type
                    string terrainType = location.doesTileHaveProperty((int)tileLocation.X, (int)tileLocation.Y, "Type", "Back");
                    if (terrainType == "Dirt" || terrainType == "Grass")
                    {
                        // Check if the tile location already exists in the list
                        if (!newWildTreesInfo.Any(info => info.Item1 == tileLocation))
                        {
                            if (neighboringTreesPresent)
                            {
                                adjustedSpreadChance *= ModEntry.wildTreeDensityModifier;
                                if (Game1.random.NextDouble() < adjustedSpreadChance)
                                {
                                    newWildTreesInfo.Add(new Tuple<Vector2, string>(tileLocation, wildTree.treeType.Value));
                                }
                            }
                            // Calculate chance of spreading
                            else if (!neighboringTreesPresent)
                            {
                                if (Game1.random.NextDouble() < ModEntry.wildSpreadChance)
                                {
                                    newWildTreesInfo.Add(new Tuple<Vector2, string>(tileLocation, wildTree.treeType.Value));
                                }
                            }
                        }
                    }
                }
            }
        }

        // Check for nearby larger trees
        public static bool IsSurroundingAreaBlocked(Vector2 treeTile, GameLocation location)
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
    }
}
