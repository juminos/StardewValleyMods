using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Buildings;
using xTile;

namespace MonsterHutchFramework
{
    internal class MonsterHutch
    {
        public static void ExpandMonsterHutchInterior(AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Buildings"))
            {
                e.Edit(asset =>
                {
                    var editor = asset.AsDictionary<string, BuildingData>().Data;

                    var monsterHutch = new BuildingData
                    {
                        Name = I18n.MonsterHutch_Name(),
                        Description = I18n.MonsterHutch_Description(),
                        Texture = ModEntry.Mod.MonsterHutchExteriorPath,
                        DrawShadow = true,
                        UpgradeSignTile = new Microsoft.Xna.Framework.Vector2((float)3.5, 5),
                        UpgradeSignHeight = 12.0f,
                        Size = new Microsoft.Xna.Framework.Point(11, 6),
                        FadeWhenBehind = true,
                        Builder = "Robin",
                        BuildDays = 2,
                        BuildCost = 20000,
                        BuildMaterials = null,
                        HumanDoor = new Microsoft.Xna.Framework.Point(5, 5),
                        IndoorMap = "LargeMonsterHutch",
                        IndoorMapType = "StardewValley.SlimeHutch",
                        MaxOccupants = ModEntry.Config.HutchSlimeCapacity,
                        ValidOccupantTypes = new List<string>() { "Slime" },
                    };

                    var monsterHutchBuildMat1 = new BuildingMaterial
                    {
                        ItemId = "(O)390",
                        Amount = 750
                    };
                    var monsterHutchBuildMat2 = new BuildingMaterial
                    {
                        ItemId = "(O)338",
                        Amount = 15
                    };
                    var monsterHutchBuildMat3 = new BuildingMaterial
                    {
                        ItemId = "(O)337",
                        Amount = 2
                    };

                    monsterHutch.BuildMaterials = new List<BuildingMaterial>() { monsterHutchBuildMat1, monsterHutchBuildMat2, monsterHutchBuildMat3 };

                    editor.Add($"{ModEntry.Mod.ModManifest.UniqueID}_MonsterHutch", monsterHutch);


                    if (ModEntry.Config.HutchExpansion)
                    {
                        var expandedHutch = new BuildingData
                        {
                            Name = I18n.MonsterHutchExpanded_Name(),
                            Description = I18n.MonsterHutchExpanded_Description(),
                            Texture = ModEntry.Mod.MonsterHutchExteriorPath,
                            DrawShadow = true,
                            UpgradeSignTile = new Microsoft.Xna.Framework.Vector2((float)3.5, 5),
                            UpgradeSignHeight = 12.0f,
                            Size = new Microsoft.Xna.Framework.Point(11, 6),
                            FadeWhenBehind = true,
                            Builder = "Robin",
                            BuildDays = 2,
                            BuildCost = 100000,
                            BuildMaterials = null,
                            BuildingToUpgrade = $"{ModEntry.Mod.ModManifest.UniqueID}_MonsterHutch",
                            HumanDoor = new Microsoft.Xna.Framework.Point(5, 5),
                            IndoorMap = "LargeMonsterHutchExpanded",
                            IndoorMapType = "StardewValley.SlimeHutch",
                            MaxOccupants = ModEntry.Config.HutchSlimeCapacity,
                            ValidOccupantTypes = new List<string>() { "Slime" },
                        };

                        var expandedHutchBuildMat1 = new BuildingMaterial
                        {
                            ItemId = "(O)390",
                            Amount = 500
                        };
                        var expandedHutchBuildMat2 = new BuildingMaterial
                        {
                            ItemId = "(O)338",
                            Amount = 10
                        };
                        var expandedHutchBuildMat3 = new BuildingMaterial
                        {
                            ItemId = "(O)337",
                            Amount = 1
                        };

                        expandedHutch.BuildMaterials = new List<BuildingMaterial>() { expandedHutchBuildMat1, expandedHutchBuildMat2, expandedHutchBuildMat3 };

                        editor.Add($"{ModEntry.Mod.ModManifest.UniqueID}_MonsterHutchExpanded", expandedHutch);
                    }
                });
            }
            if (e.NameWithoutLocale.IsEquivalentTo("Maps/LargeMonsterHutchExpanded"))
            {
                e.LoadFromModFile<Map>("assets/LargeMonsterHutchExpanded.tmx", AssetLoadPriority.Medium);
            }
            if (e.NameWithoutLocale.IsEquivalentTo("Maps/LargeMonsterHutch"))
            {
                e.LoadFromModFile<Map>("assets/LargeMonsterHutch.tmx", AssetLoadPriority.Medium);
            }
        }
    }
}
