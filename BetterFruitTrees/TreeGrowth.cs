//#define LOGGING

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;

namespace BetterFruitTrees
{
    public class TreeGrowth
    {
        private static Random random = new Random();

        public static void UpdateTreeGrowth(GameLocation location, IMonitor monitor)
        {
                foreach (KeyValuePair<Vector2, TerrainFeature> pair in location.terrainFeatures.Pairs)
                {
                    // Check for growing fruit trees
                    if (pair.Value is FruitTree fruitTree && fruitTree.growthStage.Value < 4)
                    {
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
                        if (!IsSurroundingFruitAreaOvershadowed(pair.Key, location, fruitTree) && (!location.IsWinterHere() || ModEntry.winterGrowth))
                        {
                            float fruitGrowthChance = 28f / Math.Max(ModEntry.fruitDaysToFinalStage, 1);
                            float fruitFarmGrowthChance = 28f / Math.Max(ModEntry.fruitFarmDaysToFinalStage, 1);

                            if (fruitTree.modData.ContainsKey("Fertilized") && fruitTree.modData["Fertilized"].ToString() == "true")
                            {
                                fruitGrowthChance *= 2f;
                                fruitFarmGrowthChance *= 2f;
                                fruitTree.modData.Remove("Fertilized");
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
                        fruitTree.growthStage.Value = FruitTree.DaysUntilMatureToGrowthStage(fruitTree.daysUntilMature.Value);

#if LOGGING
                        monitor.Log($"Updated FruitTree at position {pair.Key} to growth stage {fruitTree.growthStage.Value} with daysUntilMature {fruitTree.daysUntilMature.Value}.", LogLevel.Trace);
#endif
                    }
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

                        if (!IsSurroundingWildAreaOvershadowed(pair.Key, location, wildTree) && (!location.IsWinterHere() || ModEntry.winterGrowth))
                        {
                            float wildGrowthChance = 5f / Math.Max(ModEntry.wildDaysToFinalStage, 1);
                            float wildFarmGrowthChance = 5f / Math.Max(ModEntry.wildFarmDaysToFinalStage, 1);

                            if (wildTree.modData.ContainsKey("Fertilized") && wildTree.modData["Fertilized"] == "true")
                            {
                                wildGrowthChance *= 2;
                                wildFarmGrowthChance *= 2;
                                wildTree.modData.Remove("Fertilized");
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
                    else if (pair.Value is Tree matureWildTree && matureWildTree.growthStage.Value >= 5 && location.IsFarm)
                    {
                        if (!matureWildTree.modData.ContainsKey("LargeTreeCount"))
                        {
                            int largeTreeCount = 0;
                            matureWildTree.modData["LargeTreeCount"] = largeTreeCount.ToString();
#if LOGGING
                            monitor.Log($"Initialized LargeTreeCount for tree at position {pair.Key}.", LogLevel.Trace);
#endif
                        }
                        else if (!location.IsWinterHere() || ModEntry.winterGrowth)
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
                            Convert.ToInt32(matureWildTree.modData["LargeTreeCount"]) >= ModEntry.daysToLargeTree &&
                            Game1.random.NextDouble() < ModEntry.largeTreeChance &&
                            (!location.IsWinterHere() || ModEntry.winterGrowth) &&
                            matureWildTree.modData.ContainsKey("Fertilized"))
                        {
                            string wildTreeKeyword = GetKeywordFromTreeId(matureWildTree.treeType.Value);

                            foreach (var kvp in Game1.buildingData)
                            {
                                string lowerCaseTreeKeyword = wildTreeKeyword.ToLower();
                                string lowerCaseBuildingId = kvp.Key.ToLower();

                                if (!string.IsNullOrEmpty(wildTreeKeyword) && lowerCaseBuildingId.Contains(lowerCaseTreeKeyword))
                                {
                                    Vector2 newBuildingPosition = new Vector2(pair.Key.X - 1, pair.Key.Y);
                                    Building largeTree = new Building(kvp.Key, newBuildingPosition);
                                    location.terrainFeatures.Remove(pair.Key);
                                    location.buildings.Add(largeTree);
                                    largeTree.modData["IsLargeTree"] = "true";
                                    break;
                                }
                                else
                                {
#if LOGGING
                                    monitor.Log($"Error: No matching building found for tree keyword '{wildTreeKeyword}'", LogLevel.Error);
#endif
                                }
                            }
                        }
                    }
                    else if (pair.Value is FruitTree matureFruitTree && matureFruitTree.growthStage.Value >= 4 && location is Farm)
                    {
                        if (!matureFruitTree.modData.ContainsKey("LargeTreeCount"))
                        {
                            int largeTreeCount = 0;
                            matureFruitTree.modData["LargeTreeCount"] = largeTreeCount.ToString();
#if LOGGING
                            monitor.Log($"Initialized LargeTreeCount for tree at position {pair.Key}.", LogLevel.Trace);
#endif
                        }
                        else if (!location.IsWinterHere() || ModEntry.winterGrowth)
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
                            Convert.ToInt32(matureFruitTree.modData["LargeTreeCount"]) >= ModEntry.daysToLargeTree &&
                            Game1.random.NextDouble() < ModEntry.largeTreeChance &&
                            (!location.IsWinterHere() || ModEntry.winterGrowth) &&
                            matureFruitTree.modData.ContainsKey("Fertilized"))
                        {
                            string fruitTreeKeyword = GetKeywordFromTreeId(matureFruitTree.treeId.Value);

                            foreach (var kvp in Game1.buildingData)
                            {
                                string lowerCaseTreeKeyword = fruitTreeKeyword.ToLower();
                                string lowerCaseBuildingId = kvp.Key.ToLower();

                                if (!string.IsNullOrEmpty(fruitTreeKeyword) && lowerCaseBuildingId.Contains(lowerCaseTreeKeyword))
                                {
                                    Vector2 newBuildingPosition = new Vector2(pair.Key.X - 1, pair.Key.Y);
                                    Building largeTree = new Building(kvp.Key, newBuildingPosition);
                                    location.terrainFeatures.Remove(pair.Key);
                                    location.buildings.Add(largeTree);
                                    largeTree.modData["IsLargeTree"] = "true";
                                    break;
                                }
                                else
                                {
#if LOGGING
                                    monitor.Log($"Error: No matching building found for tree keyword '{fruitTreeKeyword}'", LogLevel.Error);
#endif
                                }
                            }
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
                default: return null;
            }
        }
    }
}
