#define LOGGING

using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json.Linq;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.WildTrees;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;

namespace BetterFruitTrees
{
    public class TreeReplacer
    {
        public static void ReplaceTree(Tree tree, GameLocation location, ModConfig config)
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
                Dictionary<string, Dictionary<string, int>> validFruitTreeIds = new Dictionary<string, Dictionary<string, int>>();

                // Define vanilla fruit tree keywords
                List<string> vanillaFruitTreeKeywords = new List<string> { "apple", "apricot", "banana", "cherry", "mango", "orange", "peach", "pomegranate" };
                // Iterate through all fruit tree keywords in the list for the current tree type
                foreach (var fruitTreeKeyword in config.TreeConfigurations[treeTypeKey]["FruitTrees"])
                {
                    // Get the keyword and weight for each fruit tree in the dictionary
                    string keyword = fruitTreeKeyword.Keyword;
                    int weight = fruitTreeKeyword.Weight;
#if LOGGING
                    Console.WriteLine($"Processing fruit tree keyword: {fruitTreeKeyword.Keyword}");
#endif
                    // Initialize dictionary for all fruit tree ids that match and the weight value
                    Dictionary<string, int> fruitTreeIdMatches = new Dictionary<string, int>();

                    if (vanillaFruitTreeKeywords.Contains(keyword.ToLower()))
                    {
                        // Convert the keyword to the corresponding numerical id using a switch
                        string fruitTreeId = GetVanillaTreeId(keyword);

                        // Add matching fruit tree ids with the weight value
                        fruitTreeIdMatches.Add(fruitTreeId, weight);
                    }
                    else
                    {
                        // Iterate through all fruit tree ids found in the game data
                        foreach (var fruitTreeId in Game1.fruitTreeData.Keys)
                        {
                            // Remove spaces from keyword
                            string keywordToCompare = keyword.Replace(" ", string.Empty);
                            // Case-insensitive comparison of fruit tree ids with keyword substring
                            if (fruitTreeId.IndexOf(keywordToCompare, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                // Add matching fruit tree ids with the weight value
                                fruitTreeIdMatches.Add(fruitTreeId, weight);
#if LOGGING
                            Console.WriteLine($"Found matching fruit tree id: {fruitTreeId}");
#endif
                            }
                        }
                    }

                    // add each keyword along with it's matching fruit tree ids and weights to valid fruit tree dictionary
                    validFruitTreeIds[keyword] = fruitTreeIdMatches;
#if LOGGING
                    Console.WriteLine($"Matching fruit tree ids: {string.Join(", ", fruitTreeIdMatches)}");
#endif
                }

                // Initialize dictionary for list of all valid fruit tree replacement options and their weight values
                Dictionary<string, int> weightedFruitTreeIds = new Dictionary<string, int>();

                // iterate through the dictionary starting with keywords that had the fewest matching fruit tree ids (to prioritize unique matches)
                foreach (var entry in validFruitTreeIds.OrderBy(kvp => kvp.Value.Count))
                {
                    Dictionary<string, int> matchingFruitTreeIds = entry.Value;

#if LOGGING
                    Console.WriteLine($"Keyword: {entry.Key}, Matching fruit tree IDs: {string.Join(", ", matchingFruitTreeIds.Keys)}");
#endif

                    //Add the matching tree ids if they haven't already been added
                    foreach (string treeId in matchingFruitTreeIds.Keys)
                    {
                        if (!weightedFruitTreeIds.ContainsKey(treeId))
                        {
                            // add the valid fruit tree id and weight pair to matchingTreeIds dictionary
                            weightedFruitTreeIds.Add(treeId, matchingFruitTreeIds[treeId]);
                        }
                    }
                }

#if LOGGING
                // Log the structure of weightedFruitTreeIds after populating
                Console.WriteLine("Weighted fruit tree IDs:");
                foreach (var kvp in weightedFruitTreeIds)
                {
                    Console.WriteLine($"Fruit tree ID: {kvp.Key}, Weight: {kvp.Value}");
                }
#endif

                // Initialize list of valid fruit tree id replacements with weights applied
                List<string> weightedFruitTreeList = new List<string>();
                // Add valid fruit tree ids by weight
                foreach (string fruitTreeId in weightedFruitTreeIds.Keys)
                {
#if LOGGING
                    Console.WriteLine($"Processing fruit tree ID: {fruitTreeId} with weight: {weightedFruitTreeIds[fruitTreeId]}");
#endif
                    int weight = weightedFruitTreeIds[fruitTreeId];
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
                        FruitTree fruitTree = new FruitTree(randomFruitTreeId, FruitTree.seedStage);
                        fruitTree.growthStage.Value = Game1.random.Next(3,5);
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
                Dictionary<string, Dictionary<string, int>> validWildTreeIds = new Dictionary<string, Dictionary<string, int>>();

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
                    // Initialize dictionary for all wild tree ids that match and the weight value
                    Dictionary<string, int> wildTreeIdMatches = new Dictionary<string, int>();
                    if (vanillaWildTreeKeywords.Contains(keyword.ToLower()))
                    {
                        // Convert the keyword to the corresponding numerical id using a switch
                        string wildTreeId = GetVanillaTreeId(keyword);

                        // Add matching wild tree ids with the weight value
                        wildTreeIdMatches.Add(wildTreeId, weight);
                    }
                    else
                    {
                        // Iterate through all wild tree ids found in the game data
                        foreach (var wildTreeId in Tree.GetWildTreeDataDictionary().Keys)
                        {
                            // Remove spaces from keyword
                            string keywordToCompare = keyword.Replace(" ", string.Empty);
#if LOGGING
                            // Log the keyword and the current wild tree ID for debugging
                            Console.WriteLine($"Comparing keyword '{keywordToCompare}' with wild tree ID '{wildTreeId}'");
#endif
                            // Case-insensitive comparison of wild tree ids with keyword substring
                            if (wildTreeId.IndexOf(keywordToCompare, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                // Add matching wild tree ids with the weight value
                                wildTreeIdMatches.Add(wildTreeId, weight);
#if LOGGING
                                Console.WriteLine($"Found matching wild tree id: {wildTreeId}");
#endif
                            }
                        }
                    }
                    // add each keyword along with it's matching wild tree ids and weights to valid wild tree dictionary
                    validWildTreeIds[keyword] = wildTreeIdMatches;
#if LOGGING
                    Console.WriteLine($"Matching wild tree ids: {string.Join(", ", wildTreeIdMatches)}");
#endif
                }
                // Initialize dictionary for list of all valid wild tree replacement options and their weight values
                Dictionary<string, int> weightedWildTreeIds = new Dictionary<string, int>();

                // iterate through the dictionary starting with keywords that had the fewest matching wild tree ids (to prioritize unique matches)
                foreach (var entry in validWildTreeIds.OrderBy(kvp => kvp.Value.Count))
                {
                    Dictionary<string, int> matchingWildTreeIds = entry.Value;

#if LOGGING
                    Console.WriteLine($"Keyword: {entry.Key}, Matching wild tree IDs: {string.Join(", ", matchingWildTreeIds.Keys)}");
#endif
                    //Add the matching tree ids if they haven't already been added
                    foreach (string treeId in matchingWildTreeIds.Keys)
                    {
                        if (!weightedWildTreeIds.ContainsKey(treeId))
                        {
                            // add the valid wild tree id and weight pair to matchingTreeIds dictionary
                            weightedWildTreeIds.Add(treeId, matchingWildTreeIds[treeId]);
                        }
                    }
                }

#if LOGGING
                // Log the structure of weightedWildTreeIds after populating
                Console.WriteLine("Weighted wild tree IDs:");
                foreach (var kvp in weightedWildTreeIds)
                {
                    Console.WriteLine($"Wild tree ID: {kvp.Key}, Weight: {kvp.Value}");
                }
#endif
                // Initialize list of valid wild tree id replacements with weights applied
                List<string> weightedWildTreeList = new List<string>();
                // Add valid wild tree ids by weight
                foreach (string wildTreeId in weightedWildTreeIds.Keys)
                {
#if LOGGING
                    Console.WriteLine($"Processing wild tree ID: {wildTreeId} with weight: {weightedWildTreeIds[wildTreeId]}");
#endif
                    int weight = weightedWildTreeIds[wildTreeId];
                    for (int i = 0; i < weight; i++)
                    {
                        weightedWildTreeList.Add(wildTreeId);
                    }
                }
#if LOGGING
                // Log the contents of weightedWildTreeList
                Console.WriteLine("Contents of weightedWildTreeList:");
                Console.WriteLine(string.Join(", ", weightedWildTreeList));
                Console.WriteLine($"Weighted wild tree list count: {weightedWildTreeList.Count}");
#endif
                // Select a random wild tree ID from the weighted list
                if (weightedWildTreeList.Count > 0)
                {
                    string randomWildTreeId = weightedWildTreeList[Game1.random.Next(weightedWildTreeList.Count)];
#if LOGGING
                    Console.WriteLine($"Selected wild tree id: {randomWildTreeId}");
#endif
                    // Create the new tree using selected tree ID
                    if (Tree.GetWildTreeDataDictionary().Keys.Contains(randomWildTreeId))
                    {
#if LOGGING
                        Console.WriteLine($"Checked that wild tree data contains key: {randomWildTreeId}");
#endif
                        Tree wildTree = new Tree(randomWildTreeId, Tree.seedStage);
                        wildTree.growthStage.Value = Game1.random.Next(4, 6);
#if LOGGING
                        Console.WriteLine($"adding wild tree: {randomWildTreeId} at stage: {wildTree.growthStage}");
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
    }
}