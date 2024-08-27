using System.Collections.Generic;

namespace BetterFruitTrees
{
    public class ModConfig
    {
        // Chance for fruit tree to spread to a given tile within its spread radius
        public float? FruitSpreadChance { get; set; } = 0.05f;
        public float? WildSpreadChance { get; set; } = 0.05f;
        // Radius for fruit tree spreading check
        public int? SpreadRadius { get; set; } = 3;
        // Whether to spread trees to adjacent tiles
        public bool? DenseTrees { get; set; } = false;
        // Days until fruit trees reach maturity
        public int? FruitDaysToFinalStage { get; set; } = 84;
        // Days until fruit trees reach maturity on farm or in greenhouse
        public int? FruitFarmDaysToFinalStage { get; set; } = 84;
        // Days until wild trees reach maturity
        public int? WildDaysToFinalStage { get; set; } = 28;
        // Days until wild trees reach maturity on farm or in greenhouse
        public int? WildFarmDaysToFinalStage { get; set; } = 28;
        // Minimum number of days for mature tree to grow into large tree
        public int? DaysToLargeTree { get; set; } = 56;
        // Chance for mature tree to grow into large tree once conditions are met
        public float? LargeTreeChance { get; set; } = 0.2f;
        // Toggle winter growth
        public bool? WinterGrowth { get; set; } = false;
        // Chance for a tree to be replaced by a fruit tree (0.0 to 1.0)
        public float FruitTreeChance { get; set; } = 0.03f;
        // Weighted lists of tree replacement options
        public Dictionary<string, Dictionary<string, List<TreeReplacement>>> TreeConfigurations { get; set; }

        // Constructor to initialize TreeConfigurations
        public ModConfig()
        {
            TreeConfigurations = new Dictionary<string, Dictionary<string, List<TreeReplacement>>>
            {
                {
                    "OakTreeReplacements", new Dictionary<string, List<TreeReplacement>>
                    {
                        { "FruitTrees", new List<TreeReplacement>
                            {
                                new TreeReplacement { Keyword = "apple", Weight = 1 },
                                new TreeReplacement { Keyword = "apricot", Weight = 0 },
                                new TreeReplacement { Keyword = "banana", Weight = 0 },
                                new TreeReplacement { Keyword = "cherry", Weight = 1 },
                                new TreeReplacement { Keyword = "mango", Weight = 0 },
                                new TreeReplacement { Keyword = "orange", Weight = 0 },
                                new TreeReplacement { Keyword = "peach", Weight = 1 },
                                new TreeReplacement { Keyword = "pomegranate", Weight = 0 }
                            }
                        },
                        { "WildTrees", new List<TreeReplacement>
                            {
                                new TreeReplacement { Keyword = "oak", Weight = 20 },
                                new TreeReplacement { Keyword = "maple", Weight = 0 },
                                new TreeReplacement { Keyword = "pine", Weight = 0 },
                                new TreeReplacement { Keyword = "mushroom", Weight = 1 },
                                new TreeReplacement { Keyword = "mahogany", Weight = 0 },
                                new TreeReplacement { Keyword = "desertpalm", Weight = 0 },
                                new TreeReplacement { Keyword = "islandpalm", Weight = 0 }
                            }
                        }
                    }
                },
                {
                    "MapleTreeReplacements", new Dictionary<string, List<TreeReplacement>>
                    {
                        { "FruitTrees", new List<TreeReplacement>
                            {
                                new TreeReplacement { Keyword = "apple", Weight = 1 },
                                new TreeReplacement { Keyword = "apricot", Weight = 0 },
                                new TreeReplacement { Keyword = "banana", Weight = 0 },
                                new TreeReplacement { Keyword = "cherry", Weight = 1 },
                                new TreeReplacement { Keyword = "mango", Weight = 0 },
                                new TreeReplacement { Keyword = "orange", Weight = 0 },
                                new TreeReplacement { Keyword = "peach", Weight = 1 },
                                new TreeReplacement { Keyword = "pomegranate", Weight = 0 }
                            }
                        },
                        { "WildTrees", new List<TreeReplacement>
                            {
                                new TreeReplacement { Keyword = "oak", Weight = 0 },
                                new TreeReplacement { Keyword = "maple", Weight = 20 },
                                new TreeReplacement { Keyword = "pine", Weight = 0 },
                                new TreeReplacement { Keyword = "mushroom", Weight = 1 },
                                new TreeReplacement { Keyword = "mahogany", Weight = 0 },
                                new TreeReplacement { Keyword = "desertpalm", Weight = 0 },
                                new TreeReplacement { Keyword = "islandpalm", Weight = 0 }
                            }
                        }
                    }
                },
                {
                    "PineTreeReplacements", new Dictionary<string, List<TreeReplacement>>
                    {
                        { "FruitTrees", new List<TreeReplacement>
                            {
                                new TreeReplacement { Keyword = "apple", Weight = 1 },
                                new TreeReplacement { Keyword = "apricot", Weight = 0 },
                                new TreeReplacement { Keyword = "banana", Weight = 0 },
                                new TreeReplacement { Keyword = "cherry", Weight = 1 },
                                new TreeReplacement { Keyword = "mango", Weight = 0 },
                                new TreeReplacement { Keyword = "orange", Weight = 0 },
                                new TreeReplacement { Keyword = "peach", Weight = 1 },
                                new TreeReplacement { Keyword = "pomegranate", Weight = 0 }
                            }
                        },
                        { "WildTrees", new List<TreeReplacement>
                            {
                                new TreeReplacement { Keyword = "oak", Weight = 0 },
                                new TreeReplacement { Keyword = "maple", Weight = 0 },
                                new TreeReplacement { Keyword = "pine", Weight = 20 },
                                new TreeReplacement { Keyword = "mushroom", Weight = 0 },
                                new TreeReplacement { Keyword = "mahogany", Weight = 0 },
                                new TreeReplacement { Keyword = "desertpalm", Weight = 0 },
                                new TreeReplacement { Keyword = "islandpalm", Weight = 0 }
                            }
                        }
                    }
                },
                {
                    "MushroomTreeReplacements", new Dictionary<string, List<TreeReplacement>>
                    {
                        { "FruitTrees", new List<TreeReplacement>
                            {
                                new TreeReplacement { Keyword = "apple", Weight = 0 },
                                new TreeReplacement { Keyword = "apricot", Weight = 0 },
                                new TreeReplacement { Keyword = "banana", Weight = 0 },
                                new TreeReplacement { Keyword = "cherry", Weight = 0 },
                                new TreeReplacement { Keyword = "mango", Weight = 0 },
                                new TreeReplacement { Keyword = "orange", Weight = 0 },
                                new TreeReplacement { Keyword = "peach", Weight = 0 },
                                new TreeReplacement { Keyword = "pomegranate", Weight = 0 }
                            }
                        },
                        { "WildTrees", new List<TreeReplacement>
                            {
                                new TreeReplacement { Keyword = "oak", Weight = 0 },
                                new TreeReplacement { Keyword = "maple", Weight = 0 },
                                new TreeReplacement { Keyword = "pine", Weight = 0 },
                                new TreeReplacement { Keyword = "mushroom", Weight = 1 },
                                new TreeReplacement { Keyword = "mahogany", Weight = 0 },
                                new TreeReplacement { Keyword = "desertpalm", Weight = 0 },
                                new TreeReplacement { Keyword = "islandpalm", Weight = 0 }
                            }
                        }
                    }
                },
                {
                    "MahoganyTreeReplacements", new Dictionary<string, List<TreeReplacement>>
                    {
                        { "FruitTrees", new List<TreeReplacement>
                            {
                                new TreeReplacement { Keyword = "apple", Weight = 0 },
                                new TreeReplacement { Keyword = "apricot", Weight = 0 },
                                new TreeReplacement { Keyword = "banana", Weight = 1 },
                                new TreeReplacement { Keyword = "cherry", Weight = 0 },
                                new TreeReplacement { Keyword = "mango", Weight = 1 },
                                new TreeReplacement { Keyword = "orange", Weight = 0 },
                                new TreeReplacement { Keyword = "peach", Weight = 0 },
                                new TreeReplacement { Keyword = "pomegranate", Weight = 0 }
                            }
                        },
                        { "WildTrees", new List<TreeReplacement>
                            {
                                new TreeReplacement { Keyword = "oak", Weight = 0 },
                                new TreeReplacement { Keyword = "maple", Weight = 0 },
                                new TreeReplacement { Keyword = "pine", Weight = 0 },
                                new TreeReplacement { Keyword = "mushroom", Weight = 1 },
                                new TreeReplacement { Keyword = "mahogany", Weight = 20 },
                                new TreeReplacement { Keyword = "desertpalm", Weight = 0 },
                                new TreeReplacement { Keyword = "islandpalm", Weight = 0 }
                            }
                        }
                    }
                },
                {
                    "DesertPalmTreeReplacements", new Dictionary<string, List<TreeReplacement>>
                    {
                        { "FruitTrees", new List<TreeReplacement>
                            {
                                new TreeReplacement { Keyword = "apple", Weight = 0 },
                                new TreeReplacement { Keyword = "apricot", Weight = 1 },
                                new TreeReplacement { Keyword = "banana", Weight = 0 },
                                new TreeReplacement { Keyword = "cherry", Weight = 0 },
                                new TreeReplacement { Keyword = "mango", Weight = 0 },
                                new TreeReplacement { Keyword = "orange", Weight = 1 },
                                new TreeReplacement { Keyword = "peach", Weight = 0 },
                                new TreeReplacement { Keyword = "pomegranate", Weight = 1 }
                            }
                        },
                        { "WildTrees", new List<TreeReplacement>
                            {
                                new TreeReplacement { Keyword = "oak", Weight = 0 },
                                new TreeReplacement { Keyword = "maple", Weight = 0 },
                                new TreeReplacement { Keyword = "pine", Weight = 0 },
                                new TreeReplacement { Keyword = "mushroom", Weight = 0 },
                                new TreeReplacement { Keyword = "mahogany", Weight = 0 },
                                new TreeReplacement { Keyword = "desertpalm", Weight = 1 },
                                new TreeReplacement { Keyword = "islandpalm", Weight = 0 }
                            }
                        }
                    }
                },
                {
                    "IslandPalmTreeReplacements", new Dictionary<string, List<TreeReplacement>>
                    {
                        { "FruitTrees", new List<TreeReplacement>
                            {
                                new TreeReplacement { Keyword = "apple", Weight = 0 },
                                new TreeReplacement { Keyword = "apricot", Weight = 0 },
                                new TreeReplacement { Keyword = "banana", Weight = 2 },
                                new TreeReplacement { Keyword = "cherry", Weight = 0 },
                                new TreeReplacement { Keyword = "mango", Weight = 2 },
                                new TreeReplacement { Keyword = "orange", Weight = 0 },
                                new TreeReplacement { Keyword = "peach", Weight = 0 },
                                new TreeReplacement { Keyword = "pomegranate", Weight = 1 }
                            }
                        },
                        { "WildTrees", new List<TreeReplacement>
                            {
                                new TreeReplacement { Keyword = "oak", Weight = 0 },
                                new TreeReplacement { Keyword = "maple", Weight = 0 },
                                new TreeReplacement { Keyword = "pine", Weight = 0 },
                                new TreeReplacement { Keyword = "mushroom", Weight = 0 },
                                new TreeReplacement { Keyword = "mahogany", Weight = 0 },
                                new TreeReplacement { Keyword = "desertpalm", Weight = 0 },
                                new TreeReplacement { Keyword = "islandpalm", Weight = 1 }
                            }
                        }
                    }
                },
            };
        }
    }

    public class TreeReplacement
    {
        public string? Keyword { get; set; }
        public int Weight { get; set; }
    }
}
