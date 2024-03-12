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

namespace FrenshipRings
{
    public class ModEntry : Mod
    {
        internal static bool shadowDisabled = false;

        public override void Entry(IModHelper helper)
        {
            helper.Events.Player.Warped += OnWarp;
        }

        private void OnWarp(object? sender, WarpedEventArgs e)
        {
            if (!e.IsLocalPlayer)
                return;

            if (shadowDisabled)
                Shadow.EnableShadow();

            if (Game1.player.isWearingRing("juminos.FrenshipRings_Shadow"))
            {
                Shadow.DisableShadow(Monitor);
            }
            if (Game1.player.isWearingRing("juminos.FrenshipRings_Spider"))
            {
                Spider.DisableSpider(Monitor);
            }
        }
    }
}
