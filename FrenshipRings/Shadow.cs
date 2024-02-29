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
            foreach (ShadowGuy shadowGuy in Game1.player.currentLocation.characters)
            {
                shadowGuy.DamageToFarmer = 0;
                shadowGuy.Health = 999999;
                shadowGuy.MaxHealth = 999999;
                shadowGuy.missChance.Value = 999999;
                shadowGuy.moveTowardPlayerThreshold.Value = 0;
                shadowGuy.setInvincibleCountdown(10);
            }
            foreach (ShadowGirl shadowGirl in Game1.player.currentLocation.characters)
            {
                shadowGirl.DamageToFarmer = 0;
                shadowGirl.Health = 999999;
                shadowGirl.MaxHealth = 999999;
                shadowGirl.missChance.Value = 999999;
                shadowGirl.moveTowardPlayerThreshold.Value = 0;
                shadowGirl.setInvincibleCountdown(10);
            }
        }
    }
}
