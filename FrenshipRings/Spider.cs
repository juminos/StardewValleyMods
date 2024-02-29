using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Monsters;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using xTile.Dimensions;

namespace FrenshipRings
{
    public class Spider
    {
        public static void DisableSpider(IMonitor monitor)
        {
            foreach (Leaper spider in Game1.player.currentLocation.characters)
            {
                spider.DamageToFarmer = 0;
                spider.Health = 999999;
                spider.MaxHealth = 999999;
                spider.missChance.Value = 999999;
                spider.moveTowardPlayerThreshold.Value = 0;
                spider.setInvincibleCountdown(10);
            }
        }
    }
}
