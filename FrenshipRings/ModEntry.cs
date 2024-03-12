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
        internal static IMonitor SMonitor;
        internal static IModHelper SHelper;

        internal static bool shadowDisabled = false;

        public override void Entry(IModHelper helper)
        {
            SMonitor = Monitor;
            SHelper = helper;

            helper.Events.Player.Warped += OnWarp;
        }

        private void OnWarp(object? sender, WarpedEventArgs e)
        {
            if (!e.IsLocalPlayer || e.NewLocation == null)
                return;

            var newLocation = e.NewLocation;
            var newShadowPeople = newLocation.characters.Any(c => c is ShadowShaman || c is ShadowBrute || c is ShadowGuy || c is ShadowGirl);

            if (newShadowPeople)
            {
                // Check if any player in the new location is wearing a shadow ring
                var anyShadowRing = newLocation.farmers.Any(farmer => farmer.isWearingRing("juminos.MoreRings_Shadow"));

                if (anyShadowRing)
                {
                    Monitor.Log($"Attempting to disable shadow aggression at {newLocation}.");
                    Shadow.DisableShadow();
                }
            }

            var oldLocation = e.OldLocation;
            var previousShadowPeople = oldLocation.characters.Any(c => c is ShadowShaman || c is ShadowBrute || c is ShadowGuy || c is ShadowGirl);

            if (previousShadowPeople)
            {
                var oldLocationPlayers = oldLocation.farmers.ToList();

                if (!oldLocationPlayers.Any() || oldLocationPlayers.All(player => player != Game1.player && player.isWearingRing("juminos.MoreRings_Shadow")))
                {
                    Monitor.Log($"Attempting to enable shadow aggression at {oldLocation}.");
                    Shadow.EnableShadow(oldLocation);
                }
            }
            if (Game1.player.isWearingRing("juminos.MoreRings_Spider"))
            {
                Spider.DisableSpider(Monitor);
            }
        }
    }
}
