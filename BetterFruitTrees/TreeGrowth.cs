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
        public static void UpdateFruitTrees(IMonitor monitor)
        {
            foreach (GameLocation location in Game1.locations)
            {
                foreach (KeyValuePair<Vector2, TerrainFeature> pair in location.terrainFeatures.Pairs)
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

                    if (pair.Value is FruitTree fruitTree && fruitTree.growthStage.Value < 4)
                    {
                        // Logging current custom countdown
                        monitor.Log($"UpdateFruitTrees: Current custom countdown for tree at {pair.Key}: {(fruitTree.modData.ContainsKey("CustomCountdown") ? fruitTree.modData["CustomCountdown"] : "Not set")}", LogLevel.Trace);

                        // Initialize customCountdown and countdownFallback if not already set
                        if (!fruitTree.modData.ContainsKey("CustomCountdown"))
                        {
                            fruitTree.modData["CustomCountdown"] = customFruitCountdown.ToString();
                            int countdownFallback = 0;
                            fruitTree.modData["CountdownFallback"] = countdownFallback.ToString();
                            // Logging initialized custom countdown
                            monitor.Log($"UpdateFruitTrees: Initialized custom countdown {customFruitCountdown} and fallback {countdownFallback} for tree at {pair.Key}", LogLevel.Trace);
                        }

                        // Check surrounding area for bigger trees, update customCountdown, fallback
                        int remainingCustomCountdown = Convert.ToInt32(fruitTree.modData["CustomCountdown"]);
                        int currentCountdownFallback = Convert.ToInt32(fruitTree.modData["CountdownFallback"]);

                        // Determine whether fruit tree will grow based on configurable weight value
                        bool willGrow = Game1.random.NextDouble() < ModEntry.fruitDailyGrowthChance;

                        if (!IsSurroundingFruitAreaOvershadowed(pair.Key, location, fruitTree) && !Game1.IsWinter && remainingCustomCountdown < customFruitCountdown && willGrow)
                        {
                            remainingCustomCountdown = Math.Max(remainingCustomCountdown - 1, 0);
                            currentCountdownFallback++;
                        }
                        else if (currentCountdownFallback >= 1 / ModEntry.fruitDailyGrowthChance)
                        {
                            remainingCustomCountdown = Math.Max(remainingCustomCountdown - 1, 0);
                            currentCountdownFallback = 0;
                        }
                        else
                        {
                            currentCountdownFallback++;
                        }
                        fruitTree.modData["CustomCountdown"] = remainingCustomCountdown.ToString();
                        fruitTree.modData["CountdownFallback"] = currentCountdownFallback.ToString();

                        // Logging updated custom countdown
                        monitor.Log($"UpdateFruitTrees: Updated custom countdown for tree at {pair.Key} to {remainingCustomCountdown}", LogLevel.Trace);

                        // Update growth stage
                        if (remainingCustomCountdown < customFruitCountdown)
                        {
                            int newGrowthStage = CalculateFruitTreeGrowthStage(remainingCustomCountdown, customFruitCountdown);
                            fruitTree.growthStage.Value = newGrowthStage;
                            // Logging updated growth stage
                            monitor.Log($"UpdateFruitTrees: Updated growth stage for tree at {pair.Key} to {newGrowthStage}", LogLevel.Trace);
                        }

                        // Remove unneeded countdown for mature trees
                        if (fruitTree.growthStage.Value == 4)
                        {
                            fruitTree.daysUntilMature.Value = 0;
                            fruitTree.modData.Remove("CustomCountdown");
                            // Logging removed custom countdown
                            monitor.Log($"UpdateFruitTrees: Removed custom countdown for mature tree at {pair.Key}", LogLevel.Trace);
                        }
                    }
                    else if (pair.Value is Tree wildTree && wildTree.growthStage.Value < 5)
                    {
                        // Logging current custom countdown
                        monitor.Log($"UpdateWildTrees: Current custom countdown for tree at {pair.Key}: {(wildTree.modData.ContainsKey("CustomCountdown") ? wildTree.modData["CustomCountdown"] : "Not set")}", LogLevel.Trace);

                        // Initialize customCountdown if not already set
                        if (!wildTree.modData.ContainsKey("CustomCountdown"))
                        {
                            wildTree.modData["CustomCountdown"] = customWildCountdown.ToString();
                            int countdownFallback = 0;
                            wildTree.modData["CountdownFallback"] = countdownFallback.ToString();
                            // Logging initialized custom countdown
                            monitor.Log($"UpdateWildTrees: Initialized custom countdown for tree at {pair.Key} to {customWildCountdown}", LogLevel.Trace);
                        }

                        // Check surrounding area for bigger trees, decrement customCountdown
                        int remainingCustomCountdown = Convert.ToInt32(wildTree.modData["CustomCountdown"]);
                        int currentCountdownFallback = Convert.ToInt32(wildTree.modData["CountdownFallback"]);
                        
                        // Determine whether wild tree will grow based on configurable weight value
                        bool willGrow = Game1.random.NextDouble() < ModEntry.wildDailyGrowthChance;

                        if (!IsSurroundingWildAreaOvershadowed(pair.Key, location, wildTree) && !Game1.IsWinter && remainingCustomCountdown < customWildCountdown && willGrow)
                        {
                            remainingCustomCountdown = Math.Max(remainingCustomCountdown - 1, 0);
                            currentCountdownFallback++;
                        }
                        else if (currentCountdownFallback >= 1 / ModEntry.wildDailyGrowthChance)
                        {
                            remainingCustomCountdown = Math.Max(remainingCustomCountdown - 1, 0);
                            currentCountdownFallback = 0;
                        }
                        else
                        {
                            currentCountdownFallback++;
                        }

                        wildTree.modData["CustomCountdown"] = remainingCustomCountdown.ToString();
                        wildTree.modData["CountdownFallback"] = currentCountdownFallback.ToString();

                        // Logging updated custom countdown
                        monitor.Log($"UpdateFruitTrees: Updated custom countdown for tree at {pair.Key} to {remainingCustomCountdown}", LogLevel.Trace);

                        // Update growth stage
                        if (remainingCustomCountdown < customWildCountdown)
                        {
                            int newGrowthStage = CalculateWildTreeGrowthStage(remainingCustomCountdown, customWildCountdown);
                            wildTree.growthStage.Value = newGrowthStage;
                            // Logging updated growth stage
                            monitor.Log($"UpdateFruitTrees: Updated growth stage for tree at {pair.Key} to {newGrowthStage}", LogLevel.Trace);
                        }

                        // Remove unneeded countdown for mature trees
                        if (wildTree.growthStage.Value == 5)
                        {
                            wildTree.modData.Remove("CustomCountdown");
                            // Logging removed custom countdown
                            monitor.Log($"UpdateFruitTrees: Removed custom countdown for mature tree at {pair.Key}", LogLevel.Trace);
                        }
                    }
                    // Large wild tree growth
                    else if (pair.Value is Tree matureWildTree && matureWildTree.growthStage.Value >= 5 && location is Farm)
                    {
                        // initialize custom counter for days to large tree growth
                        if (!matureWildTree.modData.ContainsKey("LargeTreeCount"))
                        {
                            int largeTreeCount = 0;
                            matureWildTree.modData["LargeTreeCount"] = largeTreeCount.ToString();
                            // Logging to check if the counter is initialized
                            monitor.Log($"Initialized LargeTreeCount for tree at position {pair.Key}.", LogLevel.Trace);
                        }
                        // check and update counter
                        int currentLargeTreeCount = Convert.ToInt32(matureWildTree.modData["LargeTreeCount"]);
                        bool willGrow = Game1.random.NextDouble() < ModEntry.wildDailyGrowthChance;
                        if (willGrow)
                        {
                            currentLargeTreeCount++;
                            // Logging to check if the tree will grow this day
                            monitor.Log($"Tree at position {pair.Key} will grow into a large tree.", LogLevel.Trace);
                        }
                        else
                        {
                            // Logging to check if the tree will not grow this day
                            monitor.Log($"Tree at position {pair.Key} will not grow into a large tree.", LogLevel.Trace);
                        }
                        matureWildTree.modData["LargeTreeCount"] = currentLargeTreeCount.ToString();
                        // check if surrounding area is clear
                        Vector2[] surroundingTileLocationsArray = Utility.getSurroundingTileLocationsArray(pair.Key);
                        bool clearArea = true;
                        foreach (Vector2 tileLocation in surroundingTileLocationsArray)
                        {
                            if (!location.isTileOnMap(tileLocation) ||
                                !location.isTilePassable(tileLocation) ||
                                location.IsTileOccupiedBy(tileLocation))
                            {
                                clearArea = false;
                                // Logging to check which tile prevents growth
                                monitor.Log($"Tile at position {tileLocation} prevents growth of tree at position {pair.Key}.", LogLevel.Trace);
                                break;
                            }
                        }
                        if (clearArea && currentLargeTreeCount > 2)
                        {
                            // replace tree with large tree building
                            string wildTreeKeyword = GetKeywordFromTreeId(matureWildTree.treeType.Value);

                            monitor.Log($"Looking for keyword match for treeId: {matureWildTree.treeType.Value}", LogLevel.Trace);
                            monitor.Log($"Matching wild tree keyword found: {wildTreeKeyword}", LogLevel.Trace);

                            // Iterate through building data to find the matching building ID
                            foreach (var kvp in Game1.buildingData)
                            {
                                // Log the building ID being checked
                                monitor.Log($"Checking building ID: {kvp.Key}", LogLevel.Trace);

                                // Convert both the tree keyword and building ID to lowercase for case-insensitive comparison
                                string lowerCaseTreeKeyword = wildTreeKeyword.ToLower();
                                string lowerCaseBuildingId = kvp.Key.ToLower();

                                if (!string.IsNullOrEmpty(wildTreeKeyword) && lowerCaseBuildingId.Contains(lowerCaseTreeKeyword))
                                {
                                    // Log the matching building ID
                                    monitor.Log($"Matching building ID found: {kvp.Key}", LogLevel.Trace);

                                    // Create the large tree building at the same tile position
                                    Building largeTree = new Building(kvp.Key, pair.Key);

                                    // Log the creation of the large tree building
                                    monitor.Log($"Large tree building created at position {pair.Key}", LogLevel.Trace);

                                    // Remove the tree from terrain features
                                    location.terrainFeatures.Remove(pair.Key);

                                    // Log the removal of the tree from terrain features
                                    monitor.Log($"Tree removed from terrain features at position {pair.Key}", LogLevel.Trace);

                                    // Add large tree to map
                                    location.buildings.Add(largeTree);

                                    // Remove unneeded counter
                                    matureWildTree.modData.Remove("LargeTreeCount");

                                    // Logging to confirm texture expansion
                                    monitor.Log($"Texture expanded for tree at position {pair.Key}.", LogLevel.Trace);

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
                        // initialize custom counter for days to large tree growth
                        if (!matureFruitTree.modData.ContainsKey("LargeTreeCount"))
                        {
                            int largeTreeCount = 0;
                            matureFruitTree.modData["LargeTreeCount"] = largeTreeCount.ToString();
                            // Logging to check if the counter is initialized
                            monitor.Log($"Initialized LargeTreeCount for tree at position {pair.Key}.", LogLevel.Trace);
                        }
                        // check and update counter
                        int currentLargeTreeCount = Convert.ToInt32(matureFruitTree.modData["LargeTreeCount"]);
                        bool willGrow = Game1.random.NextDouble() < ModEntry.fruitDailyGrowthChance;
                        if (willGrow)
                        {
                            currentLargeTreeCount++;
                            // Logging to check if the tree will grow this day
                            monitor.Log($"Tree at position {pair.Key} will grow into a large tree.", LogLevel.Trace);
                        }
                        else
                        {
                            // Logging to check if the tree will not grow this day
                            monitor.Log($"Tree at position {pair.Key} will not grow into a large tree.", LogLevel.Trace);
                        }
                        matureFruitTree.modData["LargeTreeCount"] = currentLargeTreeCount.ToString();
                        // check if surrounding area is clear
                        Vector2[] surroundingTileLocationsArray = Utility.getSurroundingTileLocationsArray(pair.Key);
                        bool clearArea = true;
                        foreach (Vector2 tileLocation in surroundingTileLocationsArray)
                        {
                            if (!location.isTileOnMap(tileLocation) ||
                                !location.isTilePassable(tileLocation) ||
                                location.IsTileOccupiedBy(tileLocation))
                            {
                                clearArea = false;
                                // Logging to check which tile prevents growth
                                monitor.Log($"Tile at position {tileLocation} prevents growth of tree at position {pair.Key}.", LogLevel.Trace);
                                break;
                            }
                        }
                        if (clearArea && currentLargeTreeCount > 2)
                        {
                            // replace tree with large tree building
                            string fruitTreeKeyword = GetKeywordFromTreeId(matureFruitTree.treeId.Value);

                            monitor.Log($"Looking for keyword match for treeId: {matureFruitTree.treeId.Value}", LogLevel.Trace);
                            monitor.Log($"Matching wild tree keyword found: {fruitTreeKeyword}", LogLevel.Trace);

                            // Iterate through building data to find the matching building ID
                            foreach (var kvp in Game1.buildingData)
                            {
                                // Log the building ID being checked
                                monitor.Log($"Checking building ID: {kvp.Key}", LogLevel.Trace);

                                // Convert both the tree keyword and building ID to lowercase for case-insensitive comparison
                                string lowerCaseTreeKeyword = fruitTreeKeyword.ToLower();
                                string lowerCaseBuildingId = kvp.Key.ToLower();

                                if (!string.IsNullOrEmpty(fruitTreeKeyword) && lowerCaseBuildingId.Contains(lowerCaseTreeKeyword))
                                {
                                    // Log the matching building ID
                                    monitor.Log($"Matching building ID found: {kvp.Key}", LogLevel.Trace);

                                    // Create the large tree building at the same tile position
                                    Building largeTree = new Building(kvp.Key, pair.Key);

                                    // Log the creation of the large tree building
                                    monitor.Log($"Large tree building created at position {pair.Key}", LogLevel.Trace);

                                    // Remove the tree from terrain features
                                    location.terrainFeatures.Remove(pair.Key);

                                    // Log the removal of the tree from terrain features
                                    monitor.Log($"Tree removed from terrain features at position {pair.Key}", LogLevel.Trace);

                                    // Add large tree to map
                                    location.buildings.Add(largeTree);

                                    // Remove unneeded counter
                                    matureFruitTree.modData.Remove("LargeTreeCount");

                                    // Logging to confirm texture expansion
                                    monitor.Log($"Texture expanded for tree at position {pair.Key}.", LogLevel.Trace);

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

        private static int CalculateFruitTreeGrowthStage(int remainingCustomCountdown, int customFruitCountdown)
        {
            int daysPerStage = Math.Max(customFruitCountdown / 4, 1);

            // Calculate the current growth stage based on the number of days until maturity
            if (remainingCustomCountdown > (customFruitCountdown - daysPerStage))
            {
                return 0;
            }
            else if (remainingCustomCountdown > (customFruitCountdown - (daysPerStage * 2)))
            {
                return 1;
            }
            else if (remainingCustomCountdown > (customFruitCountdown - (daysPerStage * 3)))
            {
                return 2;
            }
            else if (remainingCustomCountdown > 0)
            {
                return 3;
            }
            else
            {
                return 4;
            }
        }

        private static int CalculateWildTreeGrowthStage(int remainingCustomCountdown, int customWildCountdown)
        {
            int daysPerStage = Math.Max(customWildCountdown / 5, 1);

            // Calculate the current growth stage based on the number of days until maturity
            if (remainingCustomCountdown > (customWildCountdown - daysPerStage))
            {
                return 0;
            }
            else if (remainingCustomCountdown > (customWildCountdown - (daysPerStage * 2)))
            {
                return 1;
            }
            else if (remainingCustomCountdown > (customWildCountdown - (daysPerStage * 3)))
            {
                return 2;
            }
            else if (remainingCustomCountdown > (customWildCountdown - (daysPerStage * 4)))
            {
                return 3;
            }
            else if (remainingCustomCountdown > 0)
            {
                return 4;
            }
            else
            {
                return 5;
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
