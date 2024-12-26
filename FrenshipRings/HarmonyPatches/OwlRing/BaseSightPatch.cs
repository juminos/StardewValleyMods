using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using FrenshipRings.Toolkit;
using HarmonyLib;
using StardewValley.Monsters;

namespace FrenshipRings.HarmonyPatches.OwlRing;

/// <summary>
/// Patches so monsters have to be closer to you to see them.
/// </summary>
[HarmonyPatch(typeof(NPC))]
[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Harmony convention.")]
internal static class BaseSightPatch
{
    [MethodImpl(TKConstants.Hot)]
    [HarmonyPatch(nameof(NPC.withinPlayerThreshold), new[] { typeof(int) })]
    private static void Prefix(NPC __instance, ref int threshold)
    {
        if (__instance is not Monster)
        {
            return;
        }

        if (Game1.player.isWearingRing("juminos.FrenshipRings.CP_Owl"))
        {
            threshold /= 2;
            threshold += threshold / 2;
            __instance.moveTowardPlayerThreshold.Value = threshold;
        }
    }
}
