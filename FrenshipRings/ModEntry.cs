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
        internal static bool IsEnabled = true;

        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.TimeChanged += GameLoop_TimeChanged;
        }

        private void GameLoop_TimeChanged(object? sender, TimeChangedEventArgs e)
        {
            if (Game1.player.isWearingRing("juminos.FrenshipRings_Shadow"))
            {
                Shadow.DisableShadow(Monitor);
            }
        }
    }
}
