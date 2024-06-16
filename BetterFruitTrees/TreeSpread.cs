using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.TerrainFeatures;
using System.Threading;

namespace BetterFruitTrees
{
    public class TreeSpread
    {
        // Iterate through all mature trees
        public static void SpreadTrees(GameLocation location, IMonitor monitor)
        {
            // Check for spreading fruit trees
            var treesAndFruitTrees = location.terrainFeatures.Pairs
                .Where(pair => pair.Value is Tree || pair.Value is FruitTree)
                .ToArray();
            foreach (KeyValuePair<Vector2, TerrainFeature> pair in treesAndFruitTrees)
            {
                if (pair.Value is FruitTree fruitTree &&
                    fruitTree.growthStage.Value == 4)
                {
                    // Log spread attempt on farm
                    if (location is Farm)
                    {
                        //monitor.Log($"Attempting to spread fruit tree at position {pair.Key}.", LogLevel.Trace);
                    }
                    float spreadChance = ModEntry.fruitSpreadChance;
                    if (fruitTree.modData.ContainsKey("Fertilized"))
                    {
                        spreadChance = 0.8f;
                        fruitTree.modData.Remove("Fertilized");
                    }
                    if (Game1.random.NextDouble() < spreadChance)
                    {
                        // Log spread attempt
                        //monitor.Log($"Attempting to spread wild tree at position {pair.Key}.", LogLevel.Trace);
                        SpreadFruitTree(location, pair.Key, fruitTree, monitor);
                    }
                }
                if (pair.Value is Tree wildTree &&
                    wildTree.growthStage.Value == 5)
                {
                    float spreadChance = ModEntry.wildSpreadChance;
                    if (wildTree.modData.ContainsKey("Fertilized"))
                    {
                        spreadChance = 0.8f;
                        wildTree.modData.Remove("Fertilized");
                    }
                    if (Game1.random.NextDouble() < spreadChance)
                    {
                        // Log spread attempt
                        //monitor.Log($"Attempting to spread wild tree at position {pair.Key}.", LogLevel.Trace);
                        SpreadWildTree(location, pair.Key, wildTree, monitor);
                    }
                }
            }
        }

        // Add new fruit tree at random tile within spread radius
        public static void SpreadFruitTree(GameLocation location, Vector2 treeTile, FruitTree fruitTree, IMonitor monitor)
        {
            // List to store weighted tiles
            List<Vector2> weightedTiles = new List<Vector2>();
            int suitableTileCount = 0;

            // Iterate over surrounding tiles within spread radius
            for (int x = (int)treeTile.X - ModEntry.spreadRadius; x <= (int)treeTile.X + ModEntry.spreadRadius; x++)
            {
                for (int y = (int)treeTile.Y - ModEntry.spreadRadius; y <= (int)treeTile.Y + ModEntry.spreadRadius; y++)
                {
                    Vector2 tileLocation = new Vector2(x, y);

                    // Check tile is suitable for tree placement
                    string terrainType = location.doesTileHaveProperty((int)tileLocation.X, (int)tileLocation.Y, "Type", "Back");
                    if (terrainType != "Dirt" && terrainType != "Grass")
                        continue;
                    if (tileLocation == treeTile ||
                        !location.isTileLocationOpen(tileLocation) ||
                        !location.isTileOnMap(tileLocation) ||
                        location.isWaterTile((int)tileLocation.X, (int)tileLocation.Y) ||
                        location.IsTileOccupiedBy(tileLocation) ||
                        (!ModEntry.denseTrees &&
                        (Math.Abs(x - treeTile.X) <= 1 && Math.Abs(y - treeTile.Y) <= 1))) 
                        continue;
                    suitableTileCount++;

                    int surroundedCount = CountOccupiedSurroundingTiles(tileLocation, location);
                    // Add tile to weighted list based on surrounded tile count
                    for (int i = 0; i < 256 / ((int)Math.Pow(2, surroundedCount)); i++)
                    {
                        weightedTiles.Add(tileLocation);
                    }
                }
            }
            if (weightedTiles.Count == 0 || Game1.random.NextDouble() >= ((float)suitableTileCount / ((2 * ModEntry.spreadRadius + 1) * (2 * ModEntry.spreadRadius + 1) - 1)))
                return;

            Vector2 selectedTile = weightedTiles[Game1.random.Next(weightedTiles.Count)];

            string fruitTreeId = fruitTree.treeId.Value;
            FruitTree newFruitTree = new FruitTree(fruitTreeId, 0);
            location.terrainFeatures.Add(selectedTile, newFruitTree);
        }

        // Add new wild tree at random tile within spread radius
        public static void SpreadWildTree(GameLocation location, Vector2 treeTile, Tree wildTree, IMonitor monitor)
        {
            // List to store weighted tiles
            List<Vector2> weightedTiles = new List<Vector2>();
            int suitableTileCount = 0;

            // Iterate over surrounding tiles within spread radius
            for (int x = (int)treeTile.X - ModEntry.spreadRadius; x <= (int)treeTile.X + ModEntry.spreadRadius; x++)
            {
                for (int y = (int)treeTile.Y - ModEntry.spreadRadius; y <= (int)treeTile.Y + ModEntry.spreadRadius; y++)
                {
                    Vector2 tileLocation = new Vector2(x, y);

                    // Check tile is suitable for tree placement
                    string terrainType = location.doesTileHaveProperty((int)tileLocation.X, (int)tileLocation.Y, "Type", "Back");
                    if (terrainType != "Dirt" && terrainType != "Grass")
                        continue;
                    if (tileLocation == treeTile ||
                        !location.isTileLocationOpen(tileLocation) ||
                        !location.isTileOnMap(tileLocation) ||
                        location.isWaterTile((int)tileLocation.X, (int)tileLocation.Y) ||
                        location.IsTileOccupiedBy(tileLocation))
                        continue;
                    suitableTileCount++;

                    int surroundedCount = CountOccupiedSurroundingTiles(tileLocation, location);

                    // Add tile to weighted list based on surrounded tile count
                    for (int i = 0; i < 256 / ((int)Math.Pow(2, surroundedCount)); i++)
                    {
                        weightedTiles.Add(tileLocation);
                    }
                }
            }

            if (weightedTiles.Count == 0 || Game1.random.NextDouble() >= ((float)suitableTileCount / ((2 * ModEntry.spreadRadius + 1) * (2 * ModEntry.spreadRadius + 1) - 1)))
                return;

            Vector2 selectedTile = weightedTiles[Game1.random.Next(weightedTiles.Count)];

            string wildTreeId = wildTree.treeType.Value.ToString();
            Tree newWildTree = new Tree(wildTreeId, 0);
            location.terrainFeatures.Add(selectedTile, newWildTree);
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
