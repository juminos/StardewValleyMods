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

        public override void Entry(IModHelper helper)
        {
            SMonitor = Monitor;
            SHelper = helper;

            helper.Events.Player.Warped += OnWarp;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
        }

        private void OnWarp(object? sender, WarpedEventArgs e)
        {
            if (!e.IsLocalPlayer || e.NewLocation == null)
                return;

            var newLocation = e.NewLocation;
            var oldLocation = e.OldLocation;

            // Check for spiders in location
            var newSpiders = newLocation.characters.Any(c => c is Leaper);

            if (newSpiders)
            {
                // Check for players wearing spider ring
                var anySpiderRing = newLocation.farmers.Any(farmer => farmer.isWearingRing("juminos.MoreRings_Spider"));

                if (anySpiderRing)
                {
                    Monitor.Log($"Attempting to disable spider aggression at {newLocation}.");
                    Spider.DisableSpider(newLocation);
                }
            }

            // Check previous locationo for spiders
            var previousSpiders = oldLocation.characters.Any(c => c is Leaper);

            if (previousSpiders)
            {
                // Check for players wearing spider ring
                var anySpiderRing = oldLocation.farmers.Any(farmer => farmer.isWearingRing("juminos.MoreRings_Spider"));

                if (!anySpiderRing)
                {
                    Monitor.Log($"Attempting to enable spider aggression at {oldLocation}.");
                    Spider.EnableSpider(oldLocation);
                }
            }
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
                var anyShadowRing = currentLocation.farmers.Any(farmer => farmer.isWearingRing("juminos.MoreRings_Shadow"));

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
                var anySpiderRing = currentLocation.farmers.Any(farmer => farmer.isWearingRing("juminos.MoreRings_Spider"));

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
        }
    }
}
