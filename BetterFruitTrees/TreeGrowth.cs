using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.TerrainFeatures;
using System.Threading;

namespace BetterFruitTrees
{
    public class TreeGrowth
    {
        private static Random random = new Random();
        public static void UpdateTreeGrowth(IMonitor monitor)
        {
            foreach (GameLocation location in Game1.locations)
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
                        // Check if growing conditions are met
                        if (!IsSurroundingFruitAreaOvershadowed(pair.Key, location, fruitTree) && (!location.IsWinterHere() || ModEntry.winterGrowth))
                        {
                            // Calculate daily growth chance
                            float fruitGrowthChance = 28f / Math.Max(ModEntry.fruitDaysToFinalStage, 1);
                            float fruitFarmGrowthChance = 28f / Math.Max(ModEntry.fruitFarmDaysToFinalStage, 1);

                            if (fruitTree.modData.ContainsKey("Fertilized") && fruitTree.modData["Fertilized"].ToString() == "true")
                            {
                                fruitGrowthChance *= 2f;
                                fruitFarmGrowthChance *= 2f;
                                fruitTree.modData.Remove("Fertilized");
                            }
                            // Use farm growth rate
                            if (location.IsGreenhouse || location.IsFarm)
                            {
                                // Decrement integer part of growth chance
                                fruitTree.daysUntilMature.Value -= (int)fruitFarmGrowthChance;
                                // Randomly decrement an extra day based on the fractional part of growth chance
                                if (Game1.random.NextDouble() < fruitFarmGrowthChance - (int)fruitFarmGrowthChance)
                                {
                                    fruitTree.daysUntilMature.Value--;
                                }
                                fruitTree.modData["GrowthCheck"] = fruitTree.daysUntilMature.Value.ToString();
                            }
                            // Universal growth for all other locations
                            else
                            {
                                // Decrement integer part of growth chance
                                fruitTree.daysUntilMature.Value -= (int)fruitGrowthChance;
                                // Randomly decrement an extra day based on the fractional part of growth chance
                                if (Game1.random.NextDouble() < fruitGrowthChance - (int)fruitGrowthChance)
                                {
                                    fruitTree.daysUntilMature.Value--;
                                }
                                fruitTree.modData["GrowthCheck"] = fruitTree.daysUntilMature.Value.ToString();
                            }
                        }
                        // Force update growth stage
                        fruitTree.growthStage.Value = FruitTree.DaysUntilMatureToGrowthStage(fruitTree.daysUntilMature.Value);
                    }
                    else if (pair.Value is Tree wildTree && wildTree.growthStage.Value <= 5)
                    {
                        // Revert to previous day growth stage
                        if (wildTree.modData.ContainsKey("GrowthCheck"))
                        {
                            wildTree.growthStage.Value = Convert.ToInt32(wildTree.modData["GrowthCheck"]);
                        }
                        // Initialize growth check for growing trees
                        else if (!wildTree.modData.ContainsKey("GrowthCheck") && wildTree.growthStage.Value < 5)
                        {
                            wildTree.modData["GrowthCheck"] = wildTree.growthStage.Value.ToString();
                        }
                        // Check if growing conditions are met
                        if (!IsSurroundingWildAreaOvershadowed(pair.Key, location, wildTree) && (!location.IsWinterHere() || ModEntry.winterGrowth))
                        {
                            // Calculate growth chance
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
                            // Check for growth on farm or greenhouse
                            if (location.IsGreenhouse || location.IsFarm)
                            {
                                if (Game1.random.NextDouble() < wildFarmGrowthChance)
                                {
                                    // Increment growth stage
                                    wildTree.growthStage.Value++;
                                    // Remove growth check from fully grown trees
                                    if (wildTree.growthStage.Value >= 5)
                                    {
                                        wildTree.modData.Remove("GrowthCheck");
                                    }
                                    // Update growth check value for all others
                                    else
                                    {
                                        wildTree.modData["GrowthCheck"] = wildTree.growthStage.Value.ToString();
                                    }
                                }
                            }
                            // Universal growth for all other locations
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
                    }
                    // Large wild tree growth
                    else if (pair.Value is Tree matureWildTree && matureWildTree.growthStage.Value >= 5 && location.IsFarm)
                    {
                        // initialize custom counter for days to large tree growth
                        if (!matureWildTree.modData.ContainsKey("LargeTreeCount"))
                        {
                            int largeTreeCount = 0;
                            matureWildTree.modData["LargeTreeCount"] = largeTreeCount.ToString();
                            // Logging to check if the counter is initialized
                            //monitor.Log($"Initialized LargeTreeCount for tree at position {pair.Key}.", LogLevel.Trace);
                        }
                        // Increment large tree counter if allowed
                        else if (!location.IsWinterHere() || ModEntry.winterGrowth)
                        {
                            int updateCounter = Convert.ToInt32(matureWildTree.modData["LargeTreeCount"]);
                            updateCounter++;
                            matureWildTree.modData["LargeTreeCount"] = updateCounter.ToString();
                        }
                        // check if surrounding area is clear in 2 tile radius
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
                                    // Logging to check which tile prevents growth
                                    //monitor.Log($"Tile at position {tileLocation} prevents growth of tree at position {pair.Key}.", LogLevel.Trace);
                                }
                            }
                        }
                        // Check growing conditions and chance
                        if (clearArea && 
                            Convert.ToInt32(matureWildTree.modData["LargeTreeCount"]) >= ModEntry.daysToLargeTree && 
                            Game1.random.NextDouble() < ModEntry.largeTreeChance && 
                            (!location.IsWinterHere() || ModEntry.winterGrowth) &&
                            matureWildTree.modData.ContainsKey("Fertilized"))
                        {
                            // replace tree with large tree building
                            string wildTreeKeyword = GetKeywordFromTreeId(matureWildTree.treeType.Value);

                            // Iterate through building data to find the matching building ID
                            foreach (var kvp in Game1.buildingData)
                            {
                                // Convert both the tree keyword and building ID to lowercase for case-insensitive comparison
                                string lowerCaseTreeKeyword = wildTreeKeyword.ToLower();
                                string lowerCaseBuildingId = kvp.Key.ToLower();

                                if (!string.IsNullOrEmpty(wildTreeKeyword) && lowerCaseBuildingId.Contains(lowerCaseTreeKeyword))
                                {
                                    // Create the large tree building 1 tile to the left of the current position
                                    Vector2 newBuildingPosition = new Vector2(pair.Key.X - 1, pair.Key.Y);
                                    Building largeTree = new Building(kvp.Key, newBuildingPosition);

                                    // Remove the tree from terrain features
                                    location.terrainFeatures.Remove(pair.Key);

                                    // Add large tree to map
                                    location.buildings.Add(largeTree);

                                    // Add tree marker
                                    largeTree.modData["IsLargeTree"] = "true";

                                    // Exit the loop once a match is found
                                    break;
                                }
                                else
                                {
                                    // Log an error if no matching building ID is found
                                    monitor.Log($"Error: No matching building found for tree keyword '{wildTreeKeyword}'", LogLevel.Error);
                                }
                            }
                        }
                    }
                    else if (pair.Value is FruitTree matureFruitTree && matureFruitTree.growthStage.Value >= 4 && location is Farm)
                    {
                        // Initialize custom counter for days to large tree growth
                        if (!matureFruitTree.modData.ContainsKey("LargeTreeCount"))
                        {
                            int largeTreeCount = 0;
                            matureFruitTree.modData["LargeTreeCount"] = largeTreeCount.ToString();
                        }
                        // Increment large tree counter if allowed
                        else if (!location.IsWinterHere() || ModEntry.winterGrowth)
                        {
                            int updateCounter = Convert.ToInt32(matureFruitTree.modData["LargeTreeCount"]);
                            updateCounter++;
                            matureFruitTree.modData["LargeTreeCount"] = updateCounter.ToString();
                        }
                        // check if surrounding area is clear in 2 tile radius
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
                                }
                            }
                        }
                        // Check growing conditions and chance
                        if (clearArea && 
                            Convert.ToInt32(matureFruitTree.modData["LargeTreeCount"]) >= ModEntry.daysToLargeTree && 
                            Game1.random.NextDouble() < ModEntry.largeTreeChance && 
                            (!location.IsWinterHere() || ModEntry.winterGrowth) &&
                            matureFruitTree.modData.ContainsKey("Fertilized"))
                        {
                            // replace tree with large tree building
                            string fruitTreeKeyword = GetKeywordFromTreeId(matureFruitTree.treeId.Value);

                            // Iterate through building data to find the matching building ID
                            foreach (var kvp in Game1.buildingData)
                            {
                                // Convert both the tree keyword and building ID to lowercase for case-insensitive comparison
                                string lowerCaseTreeKeyword = fruitTreeKeyword.ToLower();
                                string lowerCaseBuildingId = kvp.Key.ToLower();

                                if (!string.IsNullOrEmpty(fruitTreeKeyword) && lowerCaseBuildingId.Contains(lowerCaseTreeKeyword))
                                {
                                    // Create the large tree building 1 tile to the left of the current position
                                    Vector2 newBuildingPosition = new Vector2(pair.Key.X - 1, pair.Key.Y);
                                    Building largeTree = new Building(kvp.Key, newBuildingPosition);

                                    // Remove the tree from terrain features
                                    location.terrainFeatures.Remove(pair.Key);

                                    // Add large tree to map
                                    location.buildings.Add(largeTree);

                                    // Add tree marker
                                    largeTree.modData["IsLargeTree"] = "true";

                                    // Exit the loop once a match is found
                                    break;
                                }
                                else
                                {
                                    // Log an error if no matching building ID is found
                                    monitor.Log($"Error: No matching building found for tree keyword '{fruitTreeKeyword}'", LogLevel.Error);
                                }
                            }
                        }
                    }
                }
            }
        }

        // Check for nearby larger trees
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
            // Map tree IDs to their corresponding keywords
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
                default:
                    // Return null or an empty string to indicate that the tree ID is not handled
                    return null;
            }
        }
    }
}
