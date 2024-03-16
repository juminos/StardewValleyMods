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
        public static void DisableShadow(GameLocation currentLocation)
        {
            foreach (var character in currentLocation.characters)
            {
                if (character is ShadowShaman shadowShaman)
                {
                    shadowShaman.DamageToFarmer = 0;
                    shadowShaman.Health = 999999;
                    shadowShaman.MaxHealth = 999999;
                    shadowShaman.missChance.Value = 999999;
                    shadowShaman.moveTowardPlayerThreshold.Value = 0;
                    shadowShaman.setInvincibleCountdown(10);
                }
                else if (character is ShadowBrute shadowBrute)
                {
                    shadowBrute.DamageToFarmer = 0;
                    shadowBrute.Health = 999999;
                    shadowBrute.MaxHealth = 999999;
                    shadowBrute.missChance.Value = 999999;
                    shadowBrute.moveTowardPlayerThreshold.Value = 0;
                    shadowBrute.setInvincibleCountdown(10);
                }
                else if (character is ShadowGirl shadowGirl)
                {
                    shadowGirl.DamageToFarmer = 0;
                    shadowGirl.Health = 999999;
                    shadowGirl.MaxHealth = 999999;
                    shadowGirl.missChance.Value = 999999;
                    shadowGirl.moveTowardPlayerThreshold.Value = 0;
                    shadowGirl.setInvincibleCountdown(10);
                }
                else if (character is ShadowGuy shadowGuy)
                {
                    shadowGuy.DamageToFarmer = 0;
                    shadowGuy.Health = 999999;
                    shadowGuy.MaxHealth = 999999;
                    shadowGuy.missChance.Value = 999999;
                    shadowGuy.moveTowardPlayerThreshold.Value = 0;
                    shadowGuy.setInvincibleCountdown(10);
                }
            }
        }

        public static void EnableShadow(GameLocation oldLocation)
        {
            foreach (var character in oldLocation.characters)
            {
                if (character is ShadowShaman shadowShaman)
                {
                    shadowShaman.DamageToFarmer = 17;
                    shadowShaman.Health = 80;
                    shadowShaman.MaxHealth = 80;
                    shadowShaman.missChance.Value = 0.0;
                    shadowShaman.moveTowardPlayerThreshold.Value = 8;
                    shadowShaman.setInvincibleCountdown(0);
                }
                else if (character is ShadowBrute shadowBrute)
                {
                    shadowBrute.DamageToFarmer = 18;
                    shadowBrute.Health = 160;
                    shadowBrute.MaxHealth = 160;
                    shadowBrute.missChance.Value = 0.0;
                    shadowBrute.moveTowardPlayerThreshold.Value = 8;
                    shadowBrute.setInvincibleCountdown(0);
                }
                else if (character is ShadowGirl shadowGirl)
                {
                    shadowGirl.DamageToFarmer = 18;
                    shadowGirl.Health = 300;
                    shadowGirl.MaxHealth = 300;
                    shadowGirl.missChance.Value = 0.0;
                    shadowGirl.moveTowardPlayerThreshold.Value = 8;
                    shadowGirl.setInvincibleCountdown(0);
                }
                else if (character is ShadowGuy shadowGuy)
                {
                    shadowGuy.DamageToFarmer = 20;
                    shadowGuy.Health = 125;
                    shadowGuy.MaxHealth = 125;
                    shadowGuy.missChance.Value = 0.0;
                    shadowGuy.moveTowardPlayerThreshold.Value = -1;
                    shadowGuy.setInvincibleCountdown(0);
                }
            }
        }
    }
}
