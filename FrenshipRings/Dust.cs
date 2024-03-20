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
    public class Dust
    {
        public static void DisableDust(GameLocation currentLocation)
        {
            foreach (DustSpirit dust in currentLocation.characters)
            {
                dust.DamageToFarmer = 0;
                dust.Health = 999999;
                dust.MaxHealth = 999999;
                dust.missChance.Value = 999999;
                dust.moveTowardPlayerThreshold.Value = 0;
                dust.setInvincibleCountdown(10);
            }
        }
        public static void EnableDust(GameLocation oldLocation)
        {
            foreach (DustSpirit dust in oldLocation.characters)
            {
                dust.DamageToFarmer = 15;
                dust.Health = 200;
                dust.MaxHealth = 200;
                dust.missChance.Value = 999999;
                dust.moveTowardPlayerThreshold.Value = 0;
                dust.setInvincibleCountdown(10);
            }
        }
    }
}
