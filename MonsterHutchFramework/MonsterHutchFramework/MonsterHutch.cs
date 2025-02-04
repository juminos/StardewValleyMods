using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI.Events;
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
                    var editor = asset.AsDictionary<string, BuildingData>;


                });
            }
                
        }
    }
}
