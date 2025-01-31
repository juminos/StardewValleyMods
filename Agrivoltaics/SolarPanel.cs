using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI.Events;
using StardewValley.GameData.BigCraftables;
using StardewValley.GameData.Buildings;

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
                    if (data.ContainsKey("juminos.Agrivoltaics.CP_SolarPanel"))
                    {
                        data["juminos.Agrivoltaics.CP_SolarPanel"].BuildCost = ModEntry.Config.SolarPanelCost;
                        data["juminos.Agrivoltaics.CP_SolarPanel"].BuildMaterials[0].Amount = ModEntry.Config.RefinedQuartzAmount;
                        data["juminos.Agrivoltaics.CP_SolarPanel"].BuildMaterials[1].Amount = ModEntry.Config.IronBarAmount;
                        data["juminos.Agrivoltaics.CP_SolarPanel"].BuildMaterials[2].Amount = ModEntry.Config.GoldBarAmount;
                        if (ModEntry.Config.ConditionalBuild)
                        {
                            data["juminos.Agrivoltaics.CP_SolarPanel"].BuildCondition = ModEntry.Config.BuildCondition;
                        }
                    }
                });
            }
        }
    }
}
