using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Buildings;
using xTile;

namespace MonsterHutchFramework.MonsterHutchFramework
{
    internal class MonsterHutch
    {
        public static void ExpandMonsterHutchInterior(AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Maps/SlimeHutch"))
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

                    var expandedHutch = editor["Slime Hutch"];

                    expandedHutch.Name = I18n.MonsterHutchExpanded_Name();
                    expandedHutch.Description = I18n.MonsterHutchExpanded_Description();
                    expandedHutch.BuildingToUpgrade = "SlimeHutch";
                    expandedHutch.IndoorMap = "MonsterHutchExpanded";

                    editor.Add($"{ModEntry.Mod.ModManifest.UniqueID}_MonsterHutchExpanded", expandedHutch);
                });
            }
        }
    }
}
