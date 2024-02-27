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
    public class Shadow
    {
        public static void DisableShadow(IMonitor monitor)
        {
            foreach (ShadowShaman shadowShaman in Game1.player.currentLocation.characters)
            {
                shadowShaman.DamageToFarmer = 0;
                shadowShaman.Health = 999999;
                shadowShaman.MaxHealth = 999999;
                shadowShaman.missChance.Value = 999999;
                shadowShaman.moveTowardPlayerThreshold.Value = 0;
                shadowShaman.setInvincibleCountdown(10);
            }
            foreach (ShadowBrute shadowBrute in Game1.player.currentLocation.characters)
            {
                shadowBrute.DamageToFarmer = 0;
                shadowBrute.Health = 999999;
                shadowBrute.MaxHealth = 999999;
                shadowBrute.missChance.Value = 999999;
                shadowBrute.moveTowardPlayerThreshold.Value = 0;
                shadowBrute.setInvincibleCountdown(10);
            }
        }
    }
}
