using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace FrenshipRings.HarmonyPatches.BunnyRing;

/// <summary>
/// Changes the base chance of bunnies spawning if the bunny ring is equipped.
/// </summary>
[HarmonyPatch(typeof(GameLocation))]
internal static class BaseChanceModifier
{
    [HarmonyPatch(nameof(GameLocation.addBunnies))]
    private static void Prefix(ref double chance)
    {
        if (Game1.player.isWearingRing("juminos.FrenshipRings.CP_Bunny"))
        {
            chance = 1.1;
        }
    }
}
