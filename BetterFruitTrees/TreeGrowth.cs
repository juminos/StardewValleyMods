#define LOGGING

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WilderTrees
{
    public class TreeGrowth
    {
        private static Random random = new Random();

        public static void UpdateTreeGrowth(GameLocation location, IMonitor monitor, ModConfig config)
        {
            // Lookup valid large tree ids
            List<string> wildTreeBuildings = new List<string> { "maple", "oak", "mahogany" };
            Dictionary<string, string> treeBuildingIds = new Dictionary<string, string>();

            foreach (var tree in wildTreeBuildings)
            {
                foreach (var buildingkvp in Game1.buildingData)
                {
                    string lowerCaseBuildingId = buildingkvp.Key.ToLower();
                    if (lowerCaseBuildingId.Contains(tree) &&
                        buildingkvp.Value.BuildingToUpgrade == null &&
                        !treeBuildingIds.Keys.Contains(tree))
                    {
                        treeBuildingIds.Add(tree, buildingkvp.Key.ToString());
                    }
                }
            }
            Dictionary<string, string> fruitTreeBuildingIds = new Dictionary<string, string>();

            foreach (var treekvp in Game1.fruitTreeData)
            {
                string lowerCaseTreeKeyword = GetKeywordFromTreeId(treekvp.Key).ToLower();
                foreach (var buildingkvp in Game1.buildingData)
                {
                    string lowerCaseBuildingId = buildingkvp.Key.ToLower();

                    if (!string.IsNullOrEmpty(lowerCaseBuildingId) &&
                        !string.IsNullOrEmpty(lowerCaseTreeKeyword) &&
                        lowerCaseBuildingId.Contains(lowerCaseTreeKeyword) &&
                        buildingkvp.Value.BuildingToUpgrade == null &&
                        !fruitTreeBuildingIds.Keys.Contains(lowerCaseTreeKeyword))
                    {
                        fruitTreeBuildingIds.Add(lowerCaseTreeKeyword, buildingkvp.Key.ToString());
                    }
                }
            }
#if LOGGING
            monitor.Log($"Wild tree buildings: {treeBuildingIds.Keys.ToString()}, fruit tree buildings: {fruitTreeBuildingIds.Keys.ToString()}", LogLevel.Trace);
#endif

            foreach (KeyValuePair<Vector2, TerrainFeature> pair in location.terrainFeatures.Pairs)
            {
                // Check for growing fruit trees
                if (pair.Value is FruitTree fruitTree && fruitTree.growthStage.Value < 4)
                {
                    if (fruitTree.growthStage.Value < 4)
                    {
                    }
                    // Revert vanilla daily growth
                    if (fruitTree.modData.ContainsKey("GrowthCheck"))
                    {
                        fruitTree.daysUntilMature.Value = Convert.ToInt32(fruitTree.modData["GrowthCheck"]);
                    }
                    else
                    {
                        fruitTree.modData["GrowthCheck"] = fruitTree.daysUntilMature.Value.ToString();
                    }

#if LOGGING
                        monitor.Log($"Processing FruitTree at position {pair.Key} with growth stage {fruitTree.growthStage.Value} and daysUntilMature {fruitTree.daysUntilMature.Value}.", LogLevel.Trace);
#endif

                    // Check if growing conditions are met
                    if (!IsSurroundingFruitAreaOvershadowed(pair.Key, location, fruitTree))
                    {
                        float fruitGrowthChance = 28f / Math.Max(config.FruitDaysToFinalStage, 1);
                        float fruitFarmGrowthChance = 28f / Math.Max(config.FruitFarmDaysToFinalStage, 1);

                        if (fruitTree.modData.ContainsKey("Fertilized") && fruitTree.modData["Fertilized"].ToString() == "true")
                        {
                            fruitGrowthChance *= 2f;
                            fruitFarmGrowthChance *= 2f;
                        }

                        if (location.IsGreenhouse || location.IsFarm)
                        {
                            fruitTree.daysUntilMature.Value -= (int)fruitFarmGrowthChance;
                            if (Game1.random.NextDouble() < fruitFarmGrowthChance - (int)fruitFarmGrowthChance)
                            {
                                fruitTree.daysUntilMature.Value--;
                            }
                            fruitTree.modData["GrowthCheck"] = fruitTree.daysUntilMature.Value.ToString();
                        }
                        else
                        {
                            fruitTree.daysUntilMature.Value -= (int)fruitGrowthChance;
                            if (Game1.random.NextDouble() < fruitGrowthChance - (int)fruitGrowthChance)
                            {
                                fruitTree.daysUntilMature.Value--;
                            }
                            fruitTree.modData["GrowthCheck"] = fruitTree.daysUntilMature.Value.ToString();
                        }
                    }

                    // Force update growth stage
                    int startingTreeStage = fruitTree.growthStage.Value;
                    fruitTree.growthStage.Value = FruitTree.DaysUntilMatureToGrowthStage(fruitTree.daysUntilMature.Value);
                    int updatedTreeStage = fruitTree.growthStage.Value;
                    if (updatedTreeStage > startingTreeStage && fruitTree.modData.ContainsKey("Fertilized"))
                    {
                        fruitTree.modData.Remove("Fertilized");
                    }

#if LOGGING
                    monitor.Log($"Updated FruitTree at position {pair.Key} to growth stage {fruitTree.growthStage.Value} with daysUntilMature {fruitTree.daysUntilMature.Value}.", LogLevel.Trace);
#endif
                }
                // Check for growing wild trees
                else if (pair.Value is Tree wildTree && wildTree.growthStage.Value <= 5)
                {
                    if (wildTree.modData.ContainsKey("GrowthCheck"))
                    {
                        wildTree.growthStage.Value = Convert.ToInt32(wildTree.modData["GrowthCheck"]);
                    }
                    else if (!wildTree.modData.ContainsKey("GrowthCheck") && wildTree.growthStage.Value < 5)
                    {
                        wildTree.modData["GrowthCheck"] = wildTree.growthStage.Value.ToString();
                    }

#if LOGGING
                        monitor.Log($"Processing WildTree at position {pair.Key} with growth stage {wildTree.growthStage.Value}.", LogLevel.Trace);
#endif

                    if (!IsSurroundingWildAreaOvershadowed(pair.Key, location, wildTree))
                    {
                        float wildGrowthChance = 5f / Math.Max(config.WildDaysToFinalStage, 1);
                        float wildFarmGrowthChance = 5f / Math.Max(config.WildFarmDaysToFinalStage, 1);

                        if (wildTree.fertilized.Value == true)
                        {
                            wildGrowthChance *= 2;
                            wildFarmGrowthChance *= 2;
                        }

                        wildGrowthChance = Math.Min(wildGrowthChance, 1);
                        wildFarmGrowthChance = Math.Min(wildFarmGrowthChance, 1);

                        if (location.IsGreenhouse || location.IsFarm)
                        {
                            if (Game1.random.NextDouble() < wildFarmGrowthChance)
                            {
                                wildTree.growthStage.Value++;
                                if (wildTree.growthStage.Value >= 5)
                                {
                                    wildTree.modData.Remove("GrowthCheck");
                                }
                                else
                                {
                                    wildTree.modData["GrowthCheck"] = wildTree.growthStage.Value.ToString();
                                }
                            }
                        }
                        else
                        {
                            if (Game1.random.NextDouble() < wildGrowthChance)
                            {
                                wildTree.growthStage.Value++;
                                if (wildTree.growthStage.Value >= 5)
                                {
                                    wildTree.modData.Remove("GrowthCheck");
                                }
                                else
                                {
                                    wildTree.modData["GrowthCheck"] = wildTree.growthStage.Value.ToString();
                                }
                            }
                        }
                    }

#if LOGGING
                        monitor.Log($"Updated WildTree at position {pair.Key} to growth stage {wildTree.growthStage.Value}.", LogLevel.Trace);
#endif
                }

                // Large wild tree growth
                if (pair.Value is Tree matureWildTree && matureWildTree.growthStage.Value >= 5 && location.IsFarm)
                {
                    string wildTreeKeyword = GetKeywordFromTreeId(matureWildTree.treeType.Value);

#if LOGGING
                    monitor.Log($"Checking {wildTreeKeyword} for large version", LogLevel.Trace);
#endif

                    if (treeBuildingIds.ContainsKey(wildTreeKeyword))
                    {
                        if (!matureWildTree.modData.ContainsKey("LargeTreeCount"))
                        {
                            int largeTreeCount = 0;
                            matureWildTree.modData["LargeTreeCount"] = largeTreeCount.ToString();
#if LOGGING
                            monitor.Log($"Initialized LargeTreeCount for {matureWildTree.treeType.Name} at position {pair.Key}.", LogLevel.Trace);
#endif
                        }
                        else
                        {
                            int updateCounter = Convert.ToInt32(matureWildTree.modData["LargeTreeCount"]);
                            updateCounter++;
                            matureWildTree.modData["LargeTreeCount"] = updateCounter.ToString();
                        }

                        bool clearArea = true;
                        for (int x = -2; x <= 2 && clearArea; x++)
                        {
                            for (int y = -2; y <= 2 && clearArea; y++)
                            {
                                Vector2 tilelocation = new Vector2(pair.Key.X + x, pair.Key.Y + y);
                                if (!location.isTileOnMap(tilelocation) ||
                                    !location.isTilePassable(tilelocation) ||
                                    location.IsTileOccupiedBy(tilelocation))
                                {
                                    clearArea = false;
#if LOGGING
                                    monitor.Log($"Tile at position {tilelocation} prevents growth of tree at position {pair.Key}.", LogLevel.Trace);
#endif
                                }
                            }
                        }

                        if (clearArea &&
                            Convert.ToInt32(matureWildTree.modData["LargeTreeCount"]) >= config.DaysToLargeTree &&
                            Game1.random.NextDouble() < config.LargeTreeChance &&
                            matureWildTree.modData.ContainsKey("Fertilized"))
                        {
                            Vector2 newBuildingPosition = new Vector2(pair.Key.X - 1, pair.Key.Y);
                            Building largeTree = new Building(treeBuildingIds[wildTreeKeyword], newBuildingPosition);
                            location.terrainFeatures.Remove(pair.Key);
                            location.buildings.Add(largeTree);
                            largeTree.modData["IsLargeTree"] = "true";
                            largeTree.modData.Remove("Fertilized");
                        }
                    }
                    else
                    {
#if LOGGING
                        monitor.Log($"{wildTreeKeyword} not found in large tree dictionary", LogLevel.Trace);
#endif
                    }
                }

                // Large fruit tree growth
                if (pair.Value is FruitTree matureFruitTree && matureFruitTree.growthStage.Value >= 4 && location.IsFarm)
                {
                    string fruitTreeKeyword = GetKeywordFromTreeId(matureFruitTree.treeId.Value).ToLower();

#if LOGGING
                    monitor.Log($"Checking {fruitTreeKeyword} for large version", LogLevel.Trace);
#endif

                    if (fruitTreeBuildingIds.ContainsKey(fruitTreeKeyword))
                    {
                        if (!matureFruitTree.modData.ContainsKey("LargeTreeCount"))
                        {
                            int largeTreeCount = 0;
                            matureFruitTree.modData["LargeTreeCount"] = largeTreeCount.ToString();
#if LOGGING
                            monitor.Log($"Initialized LargeTreeCount for {matureFruitTree.treeId.Name} at position {pair.Key}.", LogLevel.Trace);
#endif
                        }
                        else
                        {
                            int updateCounter = Convert.ToInt32(matureFruitTree.modData["LargeTreeCount"]);
                            updateCounter++;
                            matureFruitTree.modData["LargeTreeCount"] = updateCounter.ToString();
                        }

                        bool clearArea = true;
                        for (int x = -2; x <= 2 && clearArea; x++)
                        {
                            for (int y = -2; y <= 2 && clearArea; y++)
                            {
                                Vector2 tilelocation = new Vector2(pair.Key.X + x, pair.Key.Y + y);
                                if (!location.isTileOnMap(tilelocation) ||
                                    !location.isTilePassable(tilelocation) ||
                                    location.IsTileOccupiedBy(tilelocation))
                                {
                                    clearArea = false;
#if LOGGING
                                    monitor.Log($"Tile at position {tilelocation} prevents growth of tree at position {pair.Key}.", LogLevel.Trace);
#endif
                                }
                            }
                        }

                        if (clearArea &&
                            Convert.ToInt32(matureFruitTree.modData["LargeTreeCount"]) >= config.DaysToLargeTree &&
                            Game1.random.NextDouble() < config.LargeTreeChance &&
                            matureFruitTree.modData.ContainsKey("Fertilized"))
                        {
                            Vector2 newBuildingPosition = new Vector2(pair.Key.X - 1, pair.Key.Y);
                            Building largeTree = new Building(fruitTreeBuildingIds[fruitTreeKeyword], newBuildingPosition);
                            location.terrainFeatures.Remove(pair.Key);
                            location.buildings.Add(largeTree);
                            largeTree.modData["IsLargeTree"] = "true";
                            largeTree.modData.Remove("Fertilized");
                        }
                    }
                    else
                    {
#if LOGGING
                        monitor.Log($"{fruitTreeKeyword} not found in large tree dictionary", LogLevel.Trace);
#endif
                    }
                }
            }
        }

        public static bool IsSurroundingFruitAreaOvershadowed(Vector2 treeTile, GameLocation location, FruitTree fruitTree)
        {
            Vector2[] surroundingTileLocationsArray = Utility.getSurroundingTileLocationsArray(treeTile);
            foreach (Vector2 tileLocation in surroundingTileLocationsArray)
            {
                if (location.terrainFeatures.TryGetValue(tileLocation, out TerrainFeature terrainFeature))
                {
                    if (terrainFeature is FruitTree otherFruitTree && otherFruitTree.growthStage.Value > fruitTree.growthStage.Value)
                    {
                        return true;
                    }
                    else if (terrainFeature is Tree otherWildTree && otherWildTree.growthStage.Value >= fruitTree.growthStage.Value)
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

        public static bool IsSurroundingWildAreaOvershadowed(Vector2 treeTile, GameLocation location, Tree wildTree)
        {
            Vector2[] surroundingTileLocationsArray = Utility.getSurroundingTileLocationsArray(treeTile);
            foreach (Vector2 tileLocation in surroundingTileLocationsArray)
            {
                if (location.terrainFeatures.TryGetValue(tileLocation, out TerrainFeature terrainFeature))
                {
                    if (terrainFeature is FruitTree otherFruitTree && otherFruitTree.growthStage.Value > 0 && otherFruitTree.growthStage.Value >= wildTree.growthStage.Value)
                    {
                        return true;
                    }
                    else if (terrainFeature is Tree otherWildTree && otherWildTree.growthStage.Value > 0 && otherWildTree.growthStage.Value > wildTree.growthStage.Value)
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

        private static string GetKeywordFromTreeId(string treeId)
        {
            switch (treeId)
            {
                case "1": return "oak";
                case "2": return "maple";
                case "3": return "pine";
                case "7": return "mushroom";
                case "8": return "mahogany";
                case "6": return "desertpalm";
                case "9": return "islandpalm";
                case "633": return "apple";
                case "629": return "apricot";
                case "69": return "banana";
                case "628": return "cherry";
                case "835": return "mango";
                case "630": return "orange";
                case "631": return "peach";
                case "632": return "pomegranate";
                default: return treeId;
            }
        }
    }
}
