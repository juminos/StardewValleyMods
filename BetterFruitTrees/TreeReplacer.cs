//#define LOGGING

using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json.Linq;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.WildTrees;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using xTile.Dimensions;
using xTile.Layers;
using xTile;
using xTile.Tiles;

namespace BetterFruitTrees
{
    public class TreeReplacer
    {
        public static void ReplaceTree(Tree tree, GameLocation location, ModConfig config, IMonitor monitor)
        {
            if (config == null)
            {
                location.terrainFeatures[tree.Tile] = tree;
                return;
            }

            // Convert the tree type string to the corresponding key string
            string treeTypeKey;
            switch (tree.treeType.Value)
            {
                case "1":
                    treeTypeKey = "OakTreeReplacements";
                    break;
                case "2":
                    treeTypeKey = "MapleTreeReplacements";
                    break;
                case "3":
                    treeTypeKey = "PineTreeReplacements";
                    break;
                case "7":
                    treeTypeKey = "MushroomTreeReplacements";
                    break;
                case "8":
                    treeTypeKey = "MahoganyTreeReplacements";
                    break;
                case "6":
                    treeTypeKey = "DesertPalmTreeReplacements";
                    break;
                case "9":
                    treeTypeKey = "IslandPalmTreeReplacements";
                    break;
                case "Cornucopia_SapodillaSeed":
                    treeTypeKey = "SapodillaTreeReplacements";
                    break;
                default:
                    // Unknown tree types, return original tree to location
                    location.terrainFeatures[tree.Tile] = tree;
                    return;
            }
#if LOGGING
            Console.WriteLine($"treeTypeKey: {treeTypeKey}");
#endif
            // Determine whether to replace with fruit tree based on configurable weight value
            bool isFruitTree = Game1.random.NextDouble() < config.FruitTreeChance;

            TerrainFeature newTree = tree;
            // If the fruit tree check passes
            if (isFruitTree)
            {
                // Initialize dictionary
                Dictionary<string, int> validFruitTreeIds = new Dictionary<string, int>();

                // Define vanilla fruit tree keywords
                List<string> vanillaFruitTreeKeywords = new List<string> { "apple", "apricot", "banana", "cherry", "mango", "orange", "peach", "pomegranate" };
                // Iterate through all fruit tree keywords in the list for the current tree type
                foreach (var fruitTreeKeyword in config.TreeConfigurations[treeTypeKey]["FruitTrees"])
                {
                    // Get the keyword and weight for each fruit tree in the dictionary
                    string keyword = fruitTreeKeyword.Keyword;
                    int weight = fruitTreeKeyword.Weight;
                    if (config.RandomTreeReplace)
                    {
                        weight = 1;
                    }
#if LOGGING
                    Console.WriteLine($"Processing fruit tree keyword: {fruitTreeKeyword.Keyword}");
#endif
                    if (vanillaFruitTreeKeywords.Contains(keyword.ToLower()))
                    {
                        // Convert the keyword to the corresponding numerical id using a switch
                        string fruitTreeId = GetVanillaTreeId(keyword);

                        // Add matching fruit tree ids with the weight value
                        validFruitTreeIds.Add(fruitTreeId, weight);
                    }
                    else
                    {
                        // Iterate through all fruit tree ids found in the game data
                        foreach (var fruitTreeId in Game1.fruitTreeData.Keys)
                        {
                            // Case-insensitive comparison of fruit tree ids with keyword substring
                            if (fruitTreeId.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                // Add matching fruit tree ids with the weight value
                                validFruitTreeIds.Add(fruitTreeId, weight);
#if LOGGING
                            Console.WriteLine($"Found matching fruit tree id: {fruitTreeId}");
#endif
                            }
                        }
                    }
                }
#if LOGGING
                // Log the structure of weightedFruitTreeIds after populating
                Console.WriteLine("Weighted fruit tree IDs:");
                foreach (var kvp in validFruitTreeIds)
                {
                    Console.WriteLine($"Fruit tree ID: {kvp.Key}, Weight: {kvp.Value}");
                }
#endif

                // Initialize list of valid fruit tree id replacements with weights applied
                List<string> weightedFruitTreeList = new List<string>();
                // Add valid fruit tree ids by weight
                foreach (string fruitTreeId in validFruitTreeIds.Keys)
                {
#if LOGGING
                    Console.WriteLine($"Processing fruit tree ID: {fruitTreeId} with weight: {validFruitTreeIds[fruitTreeId]}");
#endif
                    int weight = validFruitTreeIds[fruitTreeId];
                    for (int i = 0; i < weight; i++)
                    {
                        weightedFruitTreeList.Add(fruitTreeId);
                    }
                }
#if LOGGING
                // Log the contents of weightedFruitTreeList
                Console.WriteLine("Contents of weightedFruitTreeList:");
                Console.WriteLine(string.Join(", ", weightedFruitTreeList));
                Console.WriteLine($"Weighted fruit tree list count: {weightedFruitTreeList.Count}");
#endif
                // Select a random fruit tree ID from the weighted list
                if (weightedFruitTreeList.Count > 0)
                {
                    string randomFruitTreeId = weightedFruitTreeList[Game1.random.Next(weightedFruitTreeList.Count)];
#if LOGGING
                    Console.WriteLine($"Selected fruit tree id: {randomFruitTreeId}");
#endif
                    // Create the new tree using selected tree ID
                    if (Game1.fruitTreeData.Keys.Contains(randomFruitTreeId))
                    {
#if LOGGING
                        Console.WriteLine($"Checked that fruit tree data contains key: {randomFruitTreeId}");
#endif
                        int originalGrowthStage = tree.growthStage.Value;
                        if (originalGrowthStage == 5)
                        {
                            originalGrowthStage = 4;
                        }
                        FruitTree fruitTree = new FruitTree(randomFruitTreeId, originalGrowthStage);
#if LOGGING
                        Console.WriteLine($"adding fruit tree: {fruitTree.treeId} at stage: {fruitTree.growthStage}");
#endif
                        newTree = fruitTree;
                    }
                }
            }
            else
            {
                // Initialize dictionary for valid wild tree keywords with their weight value
                Dictionary<string, int> validWildTreeIds = new Dictionary<string, int>();

                // Define vanilla wild tree keywords
                List<string> vanillaWildTreeKeywords = new List<string> { "oak", "maple", "pine", "mushroom", "mahogany", "desertpalm", "islandpalm" };

                // Iterate through all wild tree keywords in the list for the current tree type
                foreach (var wildTreeKeyword in config.TreeConfigurations[treeTypeKey]["WildTrees"])
                {
                    // Get the keyword and weight for each wild tree in the dictionary
                    string keyword = wildTreeKeyword.Keyword;
                    int weight = wildTreeKeyword.Weight;
#if LOGGING
                    Console.WriteLine($"Processing wild tree keyword: {wildTreeKeyword.Keyword}");
#endif
                    if (vanillaWildTreeKeywords.Contains(keyword.ToLower()))
                    {
                        // Convert the keyword to the corresponding numerical id using a switch
                        string wildTreeId = GetVanillaTreeId(keyword);

                        // Add matching wild tree ids with the weight value
                        validWildTreeIds.Add(wildTreeId, weight);
                    }
                    else
                    {
                        monitor.Log("Wild tree type not found.");
                        return;
                        // Iterate through all wild tree ids found in the game data
                        // foreach (var treeType in Enum.GetValues(typeof(Tree))) // broken
                        // {
                        //    string wildTreeId = treeType.ToString();

                        //    WildTreeData wildTreeData;
                        //    if (Tree.TryGetData(wildTreeId, out wildTreeData))
                        //    {
                        //        // Log the keyword and the current wild tree ID for debugging
                        //        monitor.Log ($"Comparing keyword '{keyword}' with wild tree ID '{wildTreeId}'");

                                // Case-insensitive comparison of wild tree ids with keyword substring
                        //      if (wildTreeId.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                        //        {
                        //            // Add matching wild tree ids with the weight value
                        //            validWildTreeIds.Add(wildTreeId, weight);

                        //            monitor.Log ($"Found matching wild tree ID: {wildTreeId}");
                        //        }
                        //    }
                        //}
                    }
#if LOGGING
                    Console.WriteLine($"Matching wild tree ids: {string.Join(", ", validWildTreeIds)}");
#endif
                }
#if LOGGING
                // Log the structure of weightedWildTreeIds after populating
                Console.WriteLine("Weighted wild tree IDs:");
                foreach (var kvp in validWildTreeIds)
                {
                    Console.WriteLine($"Wild tree ID: {kvp.Key}, Weight: {kvp.Value}");
                }
#endif
                // Initialize list of valid wild tree id replacements with weights applied
                List<string> weightedWildTreeList = new List<string>();
                // Add valid wild tree ids by weight
                foreach (string wildTreeId in validWildTreeIds.Keys)
                {
#if LOGGING
                    Console.WriteLine($"Processing wild tree ID: {wildTreeId} with weight: {validWildTreeIds[wildTreeId]}");
#endif
                    int weight = validWildTreeIds[wildTreeId];
                    for (int i = 0; i < weight; i++)
                    {
                        weightedWildTreeList.Add(wildTreeId);
                    }
                }
                // Select a random wild tree ID from the weighted list
                if (weightedWildTreeList.Count > 0)
                {
                    string randomWildTreeId = weightedWildTreeList[Game1.random.Next(weightedWildTreeList.Count)];

                    // Check if the tree data exists for selected ID
                    if (Tree.TryGetData(randomWildTreeId, out var wildTreeData))
                    {
#if LOGGING
                        monitor.Log ($"Checked that wild tree data contains key: {randomWildTreeId}");
#endif

                        // Create the new tree using selected tree ID and tree data
                        int originalGrowthStage = tree.growthStage.Value;
                        Tree wildTree = new Tree(randomWildTreeId, originalGrowthStage);

#if LOGGING
                        monitor.Log ($"adding wild tree: {randomWildTreeId} at stage: {wildTree.growthStage}");
#endif

                        newTree = wildTree;
                    }
                }
            }
            // Replace with selected tree
            location.terrainFeatures[tree.Tile] = newTree;
        }

        // Method for retrieving the wild tree id from the keyword
        private static string GetVanillaTreeId(string keyword)
        {
            // Map wild tree keywords to their corresponding IDs
            switch (keyword)
            {
                case "oak": return "1";
                case "maple": return "2";
                case "pine": return "3";
                case "mushroom": return "7";
                case "mahogany": return "8";
                case "desertpalm": return "6";
                case "islandpalm": return "9";
                case "apple": return "633";
                case "apricot": return "629";
                case "banana": return "69";
                case "cherry": return "628";
                case "mango": return "835";
                case "orange": return "630";
                case "peach": return "631";
                case "pomegranate": return "632";
                default: throw new ArgumentOutOfRangeException(nameof(keyword), "Invalid tree keyword");
            }
        }

        public static void ResetTrees(GameLocation location, ModConfig config, IMonitor monitor)
        {
            if ((config.ResetTrees == "remove" && !location.IsFarm) || config.ResetTrees == "remove (farm)")
            {
                foreach (TerrainFeature feature in location.terrainFeatures.Values)
                {
                    if (feature is Tree tree)
                    {            
                        location.terrainFeatures.Remove(tree.Tile);
                    }
                    else if (feature is FruitTree fruitTree)
                    {
                        location.terrainFeatures.Remove(fruitTree.Tile);
                    }
                }
            }
        }
    }
}