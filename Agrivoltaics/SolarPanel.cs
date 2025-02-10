using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI.Events;
using StardewValley.GameData.BigCraftables;
using StardewValley.GameData.Buildings;
using StardewValley.Objects;

namespace Agrivoltaics
{
    internal class SolarPanel
    {
        internal static void ConfigOptions(AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Buildings"))
            {
                e.Edit(asset =>
                {
                    var data = asset.AsDictionary<string, BuildingData>().Data;
                    var buildCost = ModEntry.Config.SolarPanelCost;
                    var refQAmount = ModEntry.Config.RefinedQuartzAmount;
                    var ironBAmount = ModEntry.Config.IronBarAmount;
                    var goldBAmount = ModEntry.Config.GoldBarAmount;
                    string condBuild = null;
                    if (ModEntry.Config.ConditionalBuild)
                    {
                        condBuild = ModEntry.Config.BuildCondition;
                    }

                    var solarPanelData = new BuildingData
                    {
                        Name = I18n.SolarPanel_Name(),
                        Description = I18n.SolarPanel_Description(),
                        Texture = ModEntry.Mod.SolarPanelAssetPath,
                        Size = new Microsoft.Xna.Framework.Point(3, 1),
                        SourceRect = new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 72),
                        DrawOffset = new Microsoft.Xna.Framework.Vector2(-8, -4),
                        CollisionMap = "OXO",
                        Builder = "Robin",
                        DrawShadow = false,
                        BuildCondition = condBuild,
                        BuildDays = 1,
                        BuildCost = buildCost
                    };

                    var AddtlPlaceTiles = new BuildingPlacementTile
                    {
                        TileArea = new Microsoft.Xna.Framework.Rectangle(-1, -4, 5, 4),
                        OnlyNeedsToBePassable = true
                    };
                    solarPanelData.AdditionalPlacementTiles = new List<BuildingPlacementTile>() { AddtlPlaceTiles };

                    var buildMat1 = new BuildingMaterial
                    {
                        ItemId = "(O)338",
                        Amount = refQAmount
                    };
                    var buildMat2 = new BuildingMaterial
                    {
                        ItemId = "(O)335",
                        Amount = ironBAmount
                    };
                    var buildMat3 = new BuildingMaterial
                    {
                        ItemId = "(O)336",
                        Amount = goldBAmount
                    };
                    solarPanelData.BuildMaterials = new List<BuildingMaterial>() { buildMat1, buildMat2, buildMat3 };

                    var chest = new BuildingChest
                    {
                        Id = "Output",
                        Type = BuildingChestType.Collect,
                        DisplayTile = new Microsoft.Xna.Framework.Vector2(1, 0),
                        DisplayHeight = 1.5f
                    };
                    solarPanelData.Chests = new List<BuildingChest> { chest };

                    var actionTile = new BuildingActionTile
                    {
                        Id = "Default_OpenOutputChest",
                        Tile = new Microsoft.Xna.Framework.Point(1, 0),
                        Action = "BuildingChest Output"
                    };
                    solarPanelData.ActionTiles = new List<BuildingActionTile> { actionTile };

                    data.Add($"{ModEntry.Mod.ModManifest.UniqueID}_SolarPanel", solarPanelData);
                });
            }
        }
    }
}
