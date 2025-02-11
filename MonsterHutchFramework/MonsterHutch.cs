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
            if (e.NameWithoutLocale.IsEquivalentTo("Maps/SlimeHutch") && ModEntry.Config.ReplaceHutchInterior)
            {
                e.Edit(asset =>
                {
                    var editor = asset.AsMap();

                    editor.ReplaceWith(ModEntry.SHelper.ModContent.Load<Map>("assets/MonsterHutch.tmx"));
                });
            }
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Buildings"))
            {
                e.Edit(asset =>
                {
                    var editor = asset.AsDictionary<string, BuildingData>().Data;

                    editor["Slime Hutch"].Name = I18n.MonsterHutch_Name();
                    editor["Slime Hutch"].Description = I18n.MonsterHutch_Description();
                    editor["Slime Hutch"].MaxOccupants = ModEntry.Config.HutchSlimeCapacity;

                    if (ModEntry.Config.HutchExpansion)
                    {
                        var expandedHutch = new BuildingData
                        {
                            Name = I18n.MonsterHutchExpanded_Name(),
                            Description = I18n.MonsterHutchExpanded_Description(),
                            Texture = "Buildings\\Slime Hutch",
                            DrawShadow = true,
                            UpgradeSignTile = new Microsoft.Xna.Framework.Vector2((float)2.5, 5),
                            UpgradeSignHeight = 12.0f,
                            Size = new Microsoft.Xna.Framework.Point(7, 4),
                            FadeWhenBehind = true,
                            SourceRect = new Microsoft.Xna.Framework.Rectangle(0, 0, 112, 112),
                            Builder = "Robin",
                            BuildDays = 2,
                            BuildCost = 10000,
                            BuildMaterials = null,
                            BuildingToUpgrade = "Slime Hutch",
                            HumanDoor = new Microsoft.Xna.Framework.Point(3, 3),
                            IndoorMap = "MonsterHutchExpanded",
                            IndoorMapType = "StardewValley.SlimeHutch",
                            MaxOccupants = ModEntry.Config.HutchSlimeCapacity,
                            ValidOccupantTypes = new List<string>() { "Slime" },
                        };

                        var bigHutchBuildMat1 = new BuildingMaterial
                        {
                            ItemId = "(O)390",
                            Amount = 500
                        };
                        var bigHutchBuildMat2 = new BuildingMaterial
                        {
                            ItemId = "(O)338",
                            Amount = 10
                        };
                        var bigHutchBuildMat3 = new BuildingMaterial
                        {
                            ItemId = "(O)337",
                            Amount = 1
                        };

                        expandedHutch.BuildMaterials = new List<BuildingMaterial>() { bigHutchBuildMat1, bigHutchBuildMat2, bigHutchBuildMat3 };

                        editor.Add($"{ModEntry.Mod.ModManifest.UniqueID}_MonsterHutchExpanded", expandedHutch);
                    }
                });
            }
            if (e.NameWithoutLocale.IsEquivalentTo("Maps/MonsterHutchExpanded"))
            {
                e.LoadFromModFile<Map>("assets/MonsterHutchExpanded.tmx", AssetLoadPriority.Medium);
            }
        }
    }
}
