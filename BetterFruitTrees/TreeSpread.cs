#define LOGGING

using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
using System.Collections.Generic;
using System.Linq;

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
                if (pair.Value is FruitTree fruitTree && fruitTree.growthStage.Value == 4)
                {
#if LOGGING
                    // Log spread attempt on farm
                    if (location is Farm)
                    {
                        monitor.Log($"Attempting to spread fruit tree at position {pair.Key}.", LogLevel.Trace);
                    }
#endif
                    float spreadChance = ModEntry.fruitSpreadChance;
                    if (fruitTree.modData.ContainsKey("Fertilized"))
                    {
                        spreadChance = 0.8f;
                        fruitTree.modData.Remove("Fertilized");
                    }
                    if (Game1.random.NextDouble() < spreadChance)
                    {
#if LOGGING
                        monitor.Log($"Spreading fruit tree from position {pair.Key} with spread chance {spreadChance}.", LogLevel.Trace);
#endif
                        SpreadFruitTree(location, pair.Key, fruitTree, monitor);
                    }
                }
                if (pair.Value is Tree wildTree && wildTree.growthStage.Value == 5)
                {
#if LOGGING
                    // Log spread attempt on farm
                    if (location is Farm)
                    {
                        monitor.Log($"Attempting to spread wild tree at position {pair.Key}.", LogLevel.Trace);
                    }
#endif
                    float spreadChance = ModEntry.wildSpreadChance;
                    if (wildTree.modData.ContainsKey("Fertilized"))
                    {
                        spreadChance = 0.8f;
                        wildTree.modData.Remove("Fertilized");
                    }
                    if (Game1.random.NextDouble() < spreadChance)
                    {
#if LOGGING
                        monitor.Log($"Spreading wild tree from position {pair.Key} with spread chance {spreadChance}.", LogLevel.Trace);
#endif
                        SpreadWildTree(location, pair.Key, wildTree, monitor);
                    }
                }
            }
        }

        // Add new fruit tree at random tile within spread radius
        public static void SpreadFruitTree(GameLocation location, Vector2 treeTile, FruitTree fruitTree, IMonitor monitor)
        {
            List<Vector2> weightedTiles = new List<Vector2>();
            int suitableTileCount = 0;

#if LOGGING
            monitor.Log($"Evaluating spread tiles for fruit tree at position {treeTile}.", LogLevel.Trace);
#endif

            for (int x = (int)treeTile.X - ModEntry.spreadRadius; x <= (int)treeTile.X + ModEntry.spreadRadius; x++)
            {
                for (int y = (int)treeTile.Y - ModEntry.spreadRadius; y <= (int)treeTile.Y + ModEntry.spreadRadius; y++)
                {
                    Vector2 tileLocation = new Vector2(x, y);
                    string terrainType = location.doesTileHaveProperty((int)tileLocation.X, (int)tileLocation.Y, "Type", "Back");

                    if (terrainType != "Dirt" && terrainType != "Grass" ||
                        tileLocation == treeTile ||
                        !location.isTileLocationOpen(tileLocation) ||
                        !location.isTileOnMap(tileLocation) ||
                        location.isWaterTile((int)tileLocation.X, (int)tileLocation.Y) ||
                        location.IsTileOccupiedBy(tileLocation) ||
                        (!ModEntry.denseTrees && (Math.Abs(x - treeTile.X) <= 1 && Math.Abs(y - treeTile.Y) <= 1)))
                    {
                        continue;
                    }

                    suitableTileCount++;
                    int surroundedCount = CountOccupiedSurroundingTiles(tileLocation, location);
                    for (int i = 0; i < 256 / ((int)Math.Pow(2, surroundedCount)); i++)
                    {
                        weightedTiles.Add(tileLocation);
                    }
                }
            }

            if (weightedTiles.Count == 0)
            {
#if LOGGING
                monitor.Log($"No suitable tiles found for spreading fruit tree from position {treeTile}.", LogLevel.Trace);
#endif
                return;
            }

            float spreadProbability = (float)suitableTileCount / ((2 * ModEntry.spreadRadius + 1) * (2 * ModEntry.spreadRadius + 1) - 1);
            if (Game1.random.NextDouble() >= spreadProbability)
            {
#if LOGGING
                monitor.Log($"Spread probability check failed for fruit tree at position {treeTile}. Probability: {spreadProbability}.", LogLevel.Trace);
#endif
                return;
            }

            Vector2 selectedTile = weightedTiles[Game1.random.Next(weightedTiles.Count)];
#if LOGGING
            monitor.Log($"Selected tile {selectedTile} for spreading fruit tree from position {treeTile}.", LogLevel.Trace);
#endif

            string fruitTreeId = fruitTree.treeId.Value;
            FruitTree newFruitTree = new FruitTree(fruitTreeId, 0);
            location.terrainFeatures.Add(selectedTile, newFruitTree);
        }

        // Add new wild tree at random tile within spread radius
        public static void SpreadWildTree(GameLocation location, Vector2 treeTile, Tree wildTree, IMonitor monitor)
        {
            List<Vector2> weightedTiles = new List<Vector2>();
            int suitableTileCount = 0;

#if LOGGING
            monitor.Log($"Evaluating spread tiles for wild tree at position {treeTile}.", LogLevel.Trace);
#endif

            for (int x = (int)treeTile.X - ModEntry.spreadRadius; x <= (int)treeTile.X + ModEntry.spreadRadius; x++)
            {
                for (int y = (int)treeTile.Y - ModEntry.spreadRadius; y <= (int)treeTile.Y + ModEntry.spreadRadius; y++)
                {
                    Vector2 tileLocation = new Vector2(x, y);
                    string terrainType = location.doesTileHaveProperty((int)tileLocation.X, (int)tileLocation.Y, "Type", "Back");

                    if (terrainType != "Dirt" && terrainType != "Grass" ||
                        tileLocation == treeTile ||
                        !location.isTileLocationOpen(tileLocation) ||
                        !location.isTileOnMap(tileLocation) ||
                        location.isWaterTile((int)tileLocation.X, (int)tileLocation.Y) ||
                        location.IsTileOccupiedBy(tileLocation))
                    {
                        continue;
                    }

                    suitableTileCount++;
                    int surroundedCount = CountOccupiedSurroundingTiles(tileLocation, location);
                    for (int i = 0; i < 256 / ((int)Math.Pow(2, surroundedCount)); i++)
                    {
                        weightedTiles.Add(tileLocation);
                    }
                }
            }

            if (weightedTiles.Count == 0)
            {
#if LOGGING
                monitor.Log($"No suitable tiles found for spreading wild tree from position {treeTile}.", LogLevel.Trace);
#endif
                return;
            }

            float spreadProbability = (float)suitableTileCount / ((2 * ModEntry.spreadRadius + 1) * (2 * ModEntry.spreadRadius + 1) - 1);
            if (Game1.random.NextDouble() >= spreadProbability)
            {
#if LOGGING
                monitor.Log($"Spread probability check failed for wild tree at position {treeTile}. Probability: {spreadProbability}.", LogLevel.Trace);
#endif
                return;
            }

            Vector2 selectedTile = weightedTiles[Game1.random.Next(weightedTiles.Count)];
#if LOGGING
            monitor.Log($"Selected tile {selectedTile} for spreading wild tree from position {treeTile}.", LogLevel.Trace);
#endif

            string wildTreeId = wildTree.treeType.Value.ToString();
            Tree newWildTree = new Tree(wildTreeId, 0);
            location.terrainFeatures.Add(selectedTile, newWildTree);
        }

        // Count occupied surrounding tiles
        public static int CountOccupiedSurroundingTiles(Vector2 treeTile, GameLocation location)
        {
            int occupiedCount = 0;
            Vector2[] surroundingTileLocationsArray = Utility.getSurroundingTileLocationsArray(treeTile);
            foreach (Vector2 tileLocation in surroundingTileLocationsArray)
            {
                if (!location.isTileOnMap(tileLocation))
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
