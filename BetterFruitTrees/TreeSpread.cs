//#define LOGGING

using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
using System.Collections.Generic;
using System.Linq;

namespace WilderTrees
{
    public class TreeSpread
    {
        // Iterate through all mature trees
        public static void SpreadTrees(IMonitor monitor, ModConfig config)
        {
            foreach (GameLocation location in Game1.locations)
            {
                // Get all trees
                var treesAndFruitTrees = location.terrainFeatures.Pairs
                    .Where(pair => pair.Value is Tree || pair.Value is FruitTree)
                    .ToArray();

                // Count trees on map
                var treeCount = treesAndFruitTrees.Length;

                if (location.IsOutdoors && treeCount < config.TreeLimit)
                {
                    foreach (KeyValuePair<Vector2, TerrainFeature> pair in treesAndFruitTrees)
                    {
                        if (pair.Value is FruitTree fruitTree && fruitTree.growthStage.Value >= 4 && fruitTree.IsInSeasonHere())
                        {
#if LOGGING
                    // Log spread attempt on farm
                    if (location is Farm)
                    {
                        monitor.Log($"Attempting to spread fruit tree at position {pair.Key}.", LogLevel.Trace);
                    }
#endif
                            float spreadChance = config.FruitSpreadChance;
                            if (Game1.random.NextDouble() < spreadChance)
                            {
#if LOGGING
                        monitor.Log($"Spreading fruit tree from position {pair.Key} with spread chance {spreadChance} at {location}.", LogLevel.Trace);
#endif
                                SpreadFruitTree(location, pair.Key, fruitTree, monitor, config);
                            }
                        }
                        if (pair.Value is Tree wildTree && wildTree.growthStage.Value >= 5 && !location.IsWinterHere())
                        {
#if LOGGING
                    // Log spread attempt on farm
                    if (location is Farm)
                    {
                        monitor.Log($"Attempting to spread wild tree at position {pair.Key}.", LogLevel.Trace);
                    }
#endif
                            float spreadChance = config.WildSpreadChance;
                            if (Game1.random.NextDouble() < spreadChance)
                            {
#if LOGGING
                        monitor.Log($"Spreading wild tree from position {pair.Key} with spread chance {spreadChance} at {location}.", LogLevel.Trace);
#endif
                                SpreadWildTree(location, pair.Key, wildTree, monitor, config);
                            }
                        }
                    }
                }
            }
        }

        // Add new fruit tree at random tile within spread radius
        public static void SpreadFruitTree(GameLocation location, Vector2 treeTile, FruitTree fruitTree, IMonitor monitor, ModConfig config)
        {
            List<Vector2> weightedTiles = new List<Vector2>();
            int suitableTileCount = 0;

#if LOGGING
            monitor.Log($"Evaluating spread tiles for fruit tree at position {treeTile}.", LogLevel.Trace);
#endif

            for (int x = (int)treeTile.X - config.SpreadRadius; x <= (int)treeTile.X + config.SpreadRadius; x++)
            {
                for (int y = (int)treeTile.Y - config.SpreadRadius; y <= (int)treeTile.Y + config.SpreadRadius; y++)
                {
                    Vector2 tileLocation = new Vector2(x, y);
                    string terrainType = location.doesTileHaveProperty((int)tileLocation.X, (int)tileLocation.Y, "Type", "Back");

                    if (terrainType != "Dirt" && terrainType != "Grass" ||
                        tileLocation == treeTile ||
                        !location.isTileLocationOpen(tileLocation) ||
                        !location.isTileOnMap(tileLocation) ||
                        location.isWaterTile((int)tileLocation.X, (int)tileLocation.Y) ||
                        location.IsTileOccupiedBy(tileLocation) ||
                        (!config.DenseTrees && CheckTreeAdjacent(tileLocation, location)) ||
                        (!config.DenserTrees && (Math.Abs(x - treeTile.X) <= 1 && Math.Abs(y - treeTile.Y) <= 1)))
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

            float spreadProbability = (float)suitableTileCount / ((2 * config.SpreadRadius + 1) * (2 * config.SpreadRadius + 1) - 1);
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
        public static void SpreadWildTree(GameLocation location, Vector2 treeTile, Tree wildTree, IMonitor monitor, ModConfig config)
        {
            List<Vector2> weightedTiles = new List<Vector2>();
            int suitableTileCount = 0;

#if LOGGING
            monitor.Log($"Evaluating spread tiles for wild tree at position {treeTile}.", LogLevel.Trace);
#endif

            for (int x = (int)treeTile.X - config.SpreadRadius; x <= (int)treeTile.X + config.SpreadRadius; x++)
            {
                for (int y = (int)treeTile.Y - config.SpreadRadius; y <= (int)treeTile.Y + config.SpreadRadius; y++)
                {
                    Vector2 tileLocation = new Vector2(x, y);
                    string terrainType = location.doesTileHaveProperty((int)tileLocation.X, (int)tileLocation.Y, "Type", "Back");

                    if (terrainType != "Dirt" && terrainType != "Grass" ||
                        tileLocation == treeTile ||
                        !location.isTileLocationOpen(tileLocation) ||
                        !location.isTileOnMap(tileLocation) ||
                        location.isWaterTile((int)tileLocation.X, (int)tileLocation.Y) ||
                        location.IsTileOccupiedBy(tileLocation) ||
                        (!config.DenseTrees && CheckTreeAdjacent(tileLocation, location)) ||
                        (!config.DenserTrees && (Math.Abs(x - treeTile.X) <= 1 && Math.Abs(y - treeTile.Y) <= 1)))

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

            float spreadProbability = (float)suitableTileCount / ((2 * config.SpreadRadius + 1) * (2 * config.SpreadRadius + 1) - 1);
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

        // Check for trees in surrounding tiles
        public static bool CheckTreeAdjacent(Vector2 treeTile, GameLocation location)
        {
            bool isTreeAdjacent = false;
            Vector2[] surroundingTileLocationsArray = Utility.getSurroundingTileLocationsArray(treeTile);
            foreach (Vector2 tileLocation in surroundingTileLocationsArray)
            {
                if (!location.isTileOnMap(tileLocation))
                {
                    continue;
                }

                if (location.terrainFeatures.TryGetValue(tileLocation, out TerrainFeature terrainFeature))
                {
                    if (terrainFeature is FruitTree || terrainFeature is Tree)
                    {
                        isTreeAdjacent = true;
                    }
                }
            }
            return isTreeAdjacent;
        }
    }
}
