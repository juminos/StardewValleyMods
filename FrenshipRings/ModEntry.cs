using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Monsters;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;

namespace FrenshipRings
{
    public class ModEntry : Mod
    {
        internal static IMonitor SMonitor;
        internal static IModHelper SHelper;

        internal static bool shadowDisabled = false;
        internal static bool spiderDisabled = false;
        internal static bool dustDisabled = false;

        public override void Entry(IModHelper helper)
        {
            SMonitor = Monitor;
            SHelper = helper;

            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
        }
        private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            if (!e.IsMultipleOf(20) || !Context.IsWorldReady || Game1.player == null)
                return;

            var currentLocation = Game1.player.currentLocation;

            // Check for shadow people in current location
            var anyShadowPeople = currentLocation.characters.Any(c => c is ShadowShaman || c is ShadowBrute || c is ShadowGuy || c is ShadowGirl);

            if (anyShadowPeople)
            {
                // Check for players wearing shadow ring
                var anyShadowRing = currentLocation.farmers.Any(farmer => farmer.isWearingRing("juminos.FrenshipRings.CP_Shadow"));

                if (anyShadowRing && !shadowDisabled)
                {
                    Monitor.Log($"Attempting to disable shadow aggression at {currentLocation}.");
                    Shadow.DisableShadow(currentLocation);
                    shadowDisabled = true;
                }
                else if (!anyShadowRing && shadowDisabled)
                {
                    Monitor.Log($"Attempting to enable shadow aggression at {currentLocation}.");
                    Shadow.EnableShadow(currentLocation);
                    shadowDisabled = false;
                }
            }

            // Check for spiders in current location
            var anySpiders = currentLocation.characters.Any(c => c is Leaper);

            if (anySpiders)
            {
                // Check for players wearing spider ring
                var anySpiderRing = currentLocation.farmers.Any(farmer => farmer.isWearingRing("juminos.FrenshipRings.CP_Spider"));

                if (anySpiderRing && !spiderDisabled)
                {
                    Monitor.Log($"Attempting to disable spider aggression at {currentLocation}.");
                    Spider.DisableSpider(currentLocation);
                    spiderDisabled = true;
                }
                else if (!anySpiderRing && spiderDisabled)
                {
                    Monitor.Log($"Attempting to enable spider aggression at {currentLocation}.");
                    Spider.EnableSpider(currentLocation);
                    spiderDisabled = false;
                }
            }

            // Check for dust spirit in current location
            var anyDust = currentLocation.characters.Any(c => c is DustSpirit);

            if (anyDust)
            {
                // Check for players wearing dust ring
                var anyDustRing = currentLocation.farmers.Any(farmer => farmer.isWearingRing("juminos.FrenshipRings.CP_Dust"));

                if (anyDustRing && !dustDisabled)
                {
                    Monitor.Log($"Attempting to disable dust spirit aggression at {currentLocation}.");
                    Dust.DisableDust(currentLocation);
                    dustDisabled = true;
                }
                else if (!anyDustRing && dustDisabled)
                {
                    Monitor.Log($"Attempting to enable dust spirit aggression at {currentLocation}.");
                    Dust.EnableDust(currentLocation);
                    dustDisabled = false;
                }
            }
        }
    }
}
