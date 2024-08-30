using System.Collections.Generic;

namespace BetterFruitTrees
{
    public class ModConfig
    {
        // Chance for fruit tree to spread to a given tile within its spread radius
        public float FruitSpreadChance { get; set; } = 0.05f;
        // Chance for wild tree to spread to a given tile within its spread radius
        public float WildSpreadChance { get; set; } = 0.05f;
        // Radius for fruit tree spreading check
        public int SpreadRadius { get; set; } = 3;
        // Whether to spread trees to adjacent tiles
        public bool DenseTrees { get; set; } = false;
        // Days until fruit trees reach maturity
        public int FruitDaysToFinalStage { get; set; } = 84;
        // Days until fruit trees reach maturity on farm or in greenhouse
        public int FruitFarmDaysToFinalStage { get; set; } = 84;
        // Days until wild trees reach maturity
        public int WildDaysToFinalStage { get; set; } = 28;
        // Days until wild trees reach maturity on farm or in greenhouse
        public int WildFarmDaysToFinalStage { get; set; } = 28;
        // Minimum number of days for mature tree to grow into large tree
        public int DaysToLargeTree { get; set; } = 56;
        // Chance for mature tree to grow into large tree once conditions are met
        public float LargeTreeChance { get; set; } = 0.2f;
        // Toggle winter growth
        public bool WinterGrowth { get; set; } = false;
        // Chance for a tree to be replaced by a fruit tree (0.0 to 1.0)
        public float FruitTreeChance { get; set; } = 0.03f;
        // Toggle equally balanced weights for tree replacement
        public bool RandomTreeReplace { get; set; } = false;
        // Remove all trees not on respawn tiles (ignores farm)
        public string ResetTrees { get; set; } = "none";
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
                                new TreeReplacement { Keyword = "apricot", Weight = 1 },
                                new TreeReplacement { Keyword = "banana", Weight = 0 },
                                new TreeReplacement { Keyword = "cherry", Weight = 1 },
                                new TreeReplacement { Keyword = "mango", Weight = 0 },
                                new TreeReplacement { Keyword = "orange", Weight = 0 },
                                new TreeReplacement { Keyword = "peach", Weight = 1 },
                                new TreeReplacement { Keyword = "pomegranate", Weight = 0 },
                                new TreeReplacement { Keyword = "juminos.MoreFruitTrees_CrystalFruitSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "juminos.MoreFruitTrees_GoldenCoconutSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "juminos.MoreFruitTrees_GoldenWalnutSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "juminos.MoreFruitTrees_QiGemSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "juminos.MoreFruitTrees_SpiceBerrySapling", Weight = 0 },
                                new TreeReplacement { Keyword = "juminos.MoreFruitTrees_WildPlumSapling", Weight = 1 },
                                new TreeReplacement { Keyword = "Cornucopia_AlmondSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_AvocadoSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_BreadfruitSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_CamphorLeavesSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_CashewSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_CinnamonSticksSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_CocoaPodSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_CoconutSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_DragonFruitSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_DurianSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_EucalyptusLeavesSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_FigSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_GrapefruitSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_HazelnutSapling", Weight = 1 },
                                new TreeReplacement { Keyword = "Cornucopia_HibiscusSapling", Weight = 1 },
                                new TreeReplacement { Keyword = "Cornucopia_JasmineSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_LemonSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_LimeSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_LycheeSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_MagnoliaSapling", Weight = 1 },
                                new TreeReplacement { Keyword = "Cornucopia_MelaleucaLeavesSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_NectarineSapling", Weight = 1 },
                                new TreeReplacement { Keyword = "Cornucopia_NutmegSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_PapayaSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_PearSapling", Weight = 1 },
                                new TreeReplacement { Keyword = "Cornucopia_PecanSapling", Weight = 1 },
                                new TreeReplacement { Keyword = "Cornucopia_PersimmonSapling", Weight = 1 },
                                new TreeReplacement { Keyword = "Cornucopia_PistachioSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_PlantainSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_PomeloSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_UmeSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_WalnutSapling", Weight = 1 },
                                new TreeReplacement { Keyword = "Cornucopia_WisteriaSapling", Weight = 1 },
                                new TreeReplacement { Keyword = "Cornucopia_YlangYlangSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_YuzuSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Lumisteria.SereneMeadow_Lilac", Weight = 1 }

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
                                new TreeReplacement { Keyword = "islandpalm", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_SapodillaSeed", Weight = 0 },
                                new TreeReplacement { Keyword = "juminos.MoreWildTrees_Chestnut", Weight = 7 },
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
                                new TreeReplacement { Keyword = "apricot", Weight = 1 },
                                new TreeReplacement { Keyword = "banana", Weight = 0 },
                                new TreeReplacement { Keyword = "cherry", Weight = 1 },
                                new TreeReplacement { Keyword = "mango", Weight = 0 },
                                new TreeReplacement { Keyword = "orange", Weight = 0 },
                                new TreeReplacement { Keyword = "peach", Weight = 1 },
                                new TreeReplacement { Keyword = "pomegranate", Weight = 0 },
                                new TreeReplacement { Keyword = "juminos.MoreFruitTrees_CrystalFruitSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "juminos.MoreFruitTrees_GoldenCoconutSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "juminos.MoreFruitTrees_GoldenWalnutSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "juminos.MoreFruitTrees_QiGemSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "juminos.MoreFruitTrees_SpiceBerrySapling", Weight = 0 },
                                new TreeReplacement { Keyword = "juminos.MoreFruitTrees_WildPlumSapling", Weight = 1 },
                                new TreeReplacement { Keyword = "Cornucopia_AlmondSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_AvocadoSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_BreadfruitSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_CamphorLeavesSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_CashewSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_CinnamonSticksSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_CocoaPodSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_CoconutSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_DragonFruitSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_DurianSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_EucalyptusLeavesSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_FigSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_GrapefruitSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_HazelnutSapling", Weight = 1 },
                                new TreeReplacement { Keyword = "Cornucopia_HibiscusSapling", Weight = 1 },
                                new TreeReplacement { Keyword = "Cornucopia_JasmineSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_LemonSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_LimeSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_LycheeSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_MagnoliaSapling", Weight = 1 },
                                new TreeReplacement { Keyword = "Cornucopia_MelaleucaLeavesSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_NectarineSapling", Weight = 1 },
                                new TreeReplacement { Keyword = "Cornucopia_NutmegSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_PapayaSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_PearSapling", Weight = 1 },
                                new TreeReplacement { Keyword = "Cornucopia_PecanSapling", Weight = 1 },
                                new TreeReplacement { Keyword = "Cornucopia_PersimmonSapling", Weight = 1 },
                                new TreeReplacement { Keyword = "Cornucopia_PistachioSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_PlantainSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_PomeloSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_UmeSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_WalnutSapling", Weight = 1 },
                                new TreeReplacement { Keyword = "Cornucopia_WisteriaSapling", Weight = 1 },
                                new TreeReplacement { Keyword = "Cornucopia_YlangYlangSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_YuzuSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Lumisteria.SereneMeadow_Lilac", Weight = 1 }

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
                                new TreeReplacement { Keyword = "islandpalm", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_SapodillaSeed", Weight = 0 },
                                new TreeReplacement { Keyword = "juminos.MoreWildTrees_Chestnut", Weight = 7 },
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
                                new TreeReplacement { Keyword = "apricot", Weight = 1 },
                                new TreeReplacement { Keyword = "banana", Weight = 0 },
                                new TreeReplacement { Keyword = "cherry", Weight = 1 },
                                new TreeReplacement { Keyword = "mango", Weight = 0 },
                                new TreeReplacement { Keyword = "orange", Weight = 0 },
                                new TreeReplacement { Keyword = "peach", Weight = 1 },
                                new TreeReplacement { Keyword = "pomegranate", Weight = 0 },
                                new TreeReplacement { Keyword = "juminos.MoreFruitTrees_CrystalFruitSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "juminos.MoreFruitTrees_GoldenCoconutSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "juminos.MoreFruitTrees_GoldenWalnutSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "juminos.MoreFruitTrees_QiGemSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "juminos.MoreFruitTrees_SpiceBerrySapling", Weight = 0 },
                                new TreeReplacement { Keyword = "juminos.MoreFruitTrees_WildPlumSapling", Weight = 1 },
                                new TreeReplacement { Keyword = "Cornucopia_AlmondSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_AvocadoSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_BreadfruitSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_CamphorLeavesSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_CashewSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_CinnamonSticksSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_CocoaPodSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_CoconutSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_DragonFruitSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_DurianSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_EucalyptusLeavesSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_FigSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_GrapefruitSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_HazelnutSapling", Weight = 1 },
                                new TreeReplacement { Keyword = "Cornucopia_HibiscusSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_JasmineSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_LemonSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_LimeSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_LycheeSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_MagnoliaSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_MelaleucaLeavesSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_NectarineSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_NutmegSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_PapayaSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_PearSapling", Weight = 1 },
                                new TreeReplacement { Keyword = "Cornucopia_PecanSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_PersimmonSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_PistachioSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_PlantainSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_PomeloSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_UmeSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_WalnutSapling", Weight = 1 },
                                new TreeReplacement { Keyword = "Cornucopia_WisteriaSapling", Weight = 1 },
                                new TreeReplacement { Keyword = "Cornucopia_YlangYlangSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_YuzuSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Lumisteria.SereneMeadow_Lilac", Weight = 1 }

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
                                new TreeReplacement { Keyword = "islandpalm", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_SapodillaSeed", Weight = 0 },
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
                                new TreeReplacement { Keyword = "pomegranate", Weight = 0 },
                                new TreeReplacement { Keyword = "juminos.MoreFruitTrees_CrystalFruitSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "juminos.MoreFruitTrees_GoldenCoconutSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "juminos.MoreFruitTrees_GoldenWalnutSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "juminos.MoreFruitTrees_QiGemSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "juminos.MoreFruitTrees_SpiceBerrySapling", Weight = 0 },
                                new TreeReplacement { Keyword = "juminos.MoreFruitTrees_WildPlumSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_AlmondSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_AvocadoSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_BreadfruitSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_CamphorLeavesSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_CashewSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_CinnamonSticksSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_CocoaPodSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_CoconutSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_DragonFruitSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_DurianSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_EucalyptusLeavesSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_FigSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_GrapefruitSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_HazelnutSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_HibiscusSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_JasmineSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_LemonSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_LimeSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_LycheeSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_MagnoliaSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_MelaleucaLeavesSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_NectarineSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_NutmegSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_PapayaSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_PearSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_PecanSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_PersimmonSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_PistachioSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_PlantainSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_PomeloSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_UmeSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_WalnutSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_WisteriaSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_YlangYlangSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_YuzuSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Lumisteria.SereneMeadow_Lilac", Weight = 0 }

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
                                new TreeReplacement { Keyword = "islandpalm", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_SapodillaSeed", Weight = 0 },
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
                                new TreeReplacement { Keyword = "pomegranate", Weight = 0 },
                                new TreeReplacement { Keyword = "juminos.MoreFruitTrees_CrystalFruitSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "juminos.MoreFruitTrees_GoldenCoconutSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "juminos.MoreFruitTrees_GoldenWalnutSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "juminos.MoreFruitTrees_QiGemSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "juminos.MoreFruitTrees_SpiceBerrySapling", Weight = 0 },
                                new TreeReplacement { Keyword = "juminos.MoreFruitTrees_WildPlumSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_AlmondSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_AvocadoSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_BreadfruitSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_CamphorLeavesSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_CashewSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_CinnamonSticksSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_CocoaPodSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_CoconutSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_DragonFruitSapling", Weight = 1 },
                                new TreeReplacement { Keyword = "Cornucopia_DurianSapling", Weight = 1 },
                                new TreeReplacement { Keyword = "Cornucopia_EucalyptusLeavesSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_FigSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_GrapefruitSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_HazelnutSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_HibiscusSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_JasmineSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_LemonSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_LimeSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_LycheeSapling", Weight = 1 },
                                new TreeReplacement { Keyword = "Cornucopia_MagnoliaSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_MelaleucaLeavesSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_NectarineSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_NutmegSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_PapayaSapling", Weight = 1 },
                                new TreeReplacement { Keyword = "Cornucopia_PearSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_PecanSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_PersimmonSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_PistachioSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_PlantainSapling", Weight = 1 },
                                new TreeReplacement { Keyword = "Cornucopia_PomeloSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_UmeSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_WalnutSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_WisteriaSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_YlangYlangSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_YuzuSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Lumisteria.SereneMeadow_Lilac", Weight = 0 }

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
                                new TreeReplacement { Keyword = "islandpalm", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_SapodillaSeed", Weight = 0 },
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
                                new TreeReplacement { Keyword = "apricot", Weight = 0 },
                                new TreeReplacement { Keyword = "banana", Weight = 0 },
                                new TreeReplacement { Keyword = "cherry", Weight = 0 },
                                new TreeReplacement { Keyword = "mango", Weight = 0 },
                                new TreeReplacement { Keyword = "orange", Weight = 5 },
                                new TreeReplacement { Keyword = "peach", Weight = 0 },
                                new TreeReplacement { Keyword = "pomegranate", Weight = 5 },
                                new TreeReplacement { Keyword = "juminos.MoreFruitTrees_CrystalFruitSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "juminos.MoreFruitTrees_GoldenCoconutSapling", Weight = 1 },
                                new TreeReplacement { Keyword = "juminos.MoreFruitTrees_GoldenWalnutSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "juminos.MoreFruitTrees_QiGemSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "juminos.MoreFruitTrees_SpiceBerrySapling", Weight = 0 },
                                new TreeReplacement { Keyword = "juminos.MoreFruitTrees_WildPlumSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_AlmondSapling", Weight = 5 },
                                new TreeReplacement { Keyword = "Cornucopia_AvocadoSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_BreadfruitSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_CamphorLeavesSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_CashewSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_CinnamonSticksSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_CocoaPodSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_CoconutSapling", Weight = 10 },
                                new TreeReplacement { Keyword = "Cornucopia_DragonFruitSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_DurianSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_EucalyptusLeavesSapling", Weight = 5 },
                                new TreeReplacement { Keyword = "Cornucopia_FigSapling", Weight = 5 },
                                new TreeReplacement { Keyword = "Cornucopia_GrapefruitSapling", Weight = 5 },
                                new TreeReplacement { Keyword = "Cornucopia_HazelnutSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_HibiscusSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_JasmineSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_LemonSapling", Weight = 5 },
                                new TreeReplacement { Keyword = "Cornucopia_LimeSapling", Weight = 5 },
                                new TreeReplacement { Keyword = "Cornucopia_LycheeSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_MagnoliaSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_MelaleucaLeavesSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_NectarineSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_NutmegSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_PapayaSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_PearSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_PecanSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_PersimmonSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_PistachioSapling", Weight = 5 },
                                new TreeReplacement { Keyword = "Cornucopia_PlantainSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_PomeloSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_UmeSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_WalnutSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_WisteriaSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_YlangYlangSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_YuzuSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Lumisteria.SereneMeadow_Lilac", Weight = 0 }

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
                                new TreeReplacement { Keyword = "islandpalm", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_SapodillaSeed", Weight = 0 },
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
                                new TreeReplacement { Keyword = "banana", Weight = 5 },
                                new TreeReplacement { Keyword = "cherry", Weight = 0 },
                                new TreeReplacement { Keyword = "mango", Weight = 5 },
                                new TreeReplacement { Keyword = "orange", Weight = 0 },
                                new TreeReplacement { Keyword = "peach", Weight = 0 },
                                new TreeReplacement { Keyword = "pomegranate", Weight = 0 },
                                new TreeReplacement { Keyword = "juminos.MoreFruitTrees_CrystalFruitSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "juminos.MoreFruitTrees_GoldenCoconutSapling", Weight = 1 },
                                new TreeReplacement { Keyword = "juminos.MoreFruitTrees_GoldenWalnutSapling", Weight = 1 },
                                new TreeReplacement { Keyword = "juminos.MoreFruitTrees_QiGemSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "juminos.MoreFruitTrees_SpiceBerrySapling", Weight = 0 },
                                new TreeReplacement { Keyword = "juminos.MoreFruitTrees_WildPlumSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_AlmondSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_AvocadoSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_BreadfruitSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_CamphorLeavesSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_CashewSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_CinnamonSticksSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_CocoaPodSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_CoconutSapling", Weight = 10 },
                                new TreeReplacement { Keyword = "Cornucopia_DragonFruitSapling", Weight = 5 },
                                new TreeReplacement { Keyword = "Cornucopia_DurianSapling", Weight = 5 },
                                new TreeReplacement { Keyword = "Cornucopia_EucalyptusLeavesSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_FigSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_GrapefruitSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_HazelnutSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_HibiscusSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_JasmineSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_LemonSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_LimeSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_LycheeSapling", Weight = 5 },
                                new TreeReplacement { Keyword = "Cornucopia_MagnoliaSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_MelaleucaLeavesSapling", Weight = 5 },
                                new TreeReplacement { Keyword = "Cornucopia_NectarineSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_NutmegSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_PapayaSapling", Weight = 5 },
                                new TreeReplacement { Keyword = "Cornucopia_PearSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_PecanSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_PersimmonSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_PistachioSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_PlantainSapling", Weight = 5 },
                                new TreeReplacement { Keyword = "Cornucopia_PomeloSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_UmeSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_WalnutSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_WisteriaSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_YlangYlangSapling", Weight = 5 },
                                new TreeReplacement { Keyword = "Cornucopia_YuzuSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Lumisteria.SereneMeadow_Lilac", Weight = 0 }

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
                                new TreeReplacement { Keyword = "islandpalm", Weight = 20 },
                                new TreeReplacement { Keyword = "Cornucopia_SapodillaSeed", Weight = 1 },
                            }
                        }
                    }
                },
                {
                    "SapodillaTreeReplacements", new Dictionary<string, List<TreeReplacement>>
                    {
                        { "FruitTrees", new List<TreeReplacement>
                            {
                                new TreeReplacement { Keyword = "apple", Weight = 0 },
                                new TreeReplacement { Keyword = "apricot", Weight = 0 },
                                new TreeReplacement { Keyword = "banana", Weight = 5 },
                                new TreeReplacement { Keyword = "cherry", Weight = 0 },
                                new TreeReplacement { Keyword = "mango", Weight = 5 },
                                new TreeReplacement { Keyword = "orange", Weight = 0 },
                                new TreeReplacement { Keyword = "peach", Weight = 0 },
                                new TreeReplacement { Keyword = "pomegranate", Weight = 0 },
                                new TreeReplacement { Keyword = "juminos.MoreFruitTrees_CrystalFruitSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "juminos.MoreFruitTrees_GoldenCoconutSapling", Weight = 1 },
                                new TreeReplacement { Keyword = "juminos.MoreFruitTrees_GoldenWalnutSapling", Weight = 1 },
                                new TreeReplacement { Keyword = "juminos.MoreFruitTrees_QiGemSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "juminos.MoreFruitTrees_SpiceBerrySapling", Weight = 0 },
                                new TreeReplacement { Keyword = "juminos.MoreFruitTrees_WildPlumSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_AlmondSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_AvocadoSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_BreadfruitSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_CamphorLeavesSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_CashewSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_CinnamonSticksSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_CocoaPodSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_CoconutSapling", Weight = 10 },
                                new TreeReplacement { Keyword = "Cornucopia_DragonFruitSapling", Weight = 5 },
                                new TreeReplacement { Keyword = "Cornucopia_DurianSapling", Weight = 5 },
                                new TreeReplacement { Keyword = "Cornucopia_EucalyptusLeavesSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_FigSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_GrapefruitSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_HazelnutSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_HibiscusSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_JasmineSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_LemonSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_LimeSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_LycheeSapling", Weight = 5 },
                                new TreeReplacement { Keyword = "Cornucopia_MagnoliaSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_MelaleucaLeavesSapling", Weight = 5 },
                                new TreeReplacement { Keyword = "Cornucopia_NectarineSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_NutmegSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_PapayaSapling", Weight = 5 },
                                new TreeReplacement { Keyword = "Cornucopia_PearSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_PecanSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_PersimmonSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_PistachioSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_PlantainSapling", Weight = 5 },
                                new TreeReplacement { Keyword = "Cornucopia_PomeloSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_UmeSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_WalnutSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_WisteriaSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Cornucopia_YlangYlangSapling", Weight = 5 },
                                new TreeReplacement { Keyword = "Cornucopia_YuzuSapling", Weight = 0 },
                                new TreeReplacement { Keyword = "Lumisteria.SereneMeadow_Lilac", Weight = 0 }

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
                                new TreeReplacement { Keyword = "islandpalm", Weight = 1 },
                                new TreeReplacement { Keyword = "Cornucopia_SapodillaSeed", Weight = 20 },
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
