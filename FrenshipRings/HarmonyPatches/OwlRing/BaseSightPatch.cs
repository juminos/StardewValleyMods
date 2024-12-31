using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using FrenshipRings.Toolkit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley.Monsters;

namespace FrenshipRings.HarmonyPatches.OwlRing;

/// <summary>
/// Patches so monsters have to be closer to you to see them.
/// </summary>
internal class BaseSightPatch
{
    internal static IMonitor _monitor;
    internal static IModHelper _modHelper;
    internal BaseSightPatch(IMonitor modMonitor, IModHelper modHelper)
    {
        _monitor = modMonitor;
        _modHelper = modHelper;
    }
    internal void ApplyPatch(Harmony harmony)
    {
        harmony.Patch(AccessTools.Method(typeof(NPC), nameof(NPC.withinPlayerThreshold), new[] { typeof(int) }), prefix: new HarmonyMethod(GetType(), nameof(BaseSightPrefix)));
    }

    internal static void BaseSightPrefix(NPC __instance, ref int threshold)
    {
        if (__instance is not Monster)
        {
            return;
        }
        if (Game1.player.isWearingRing("juminos.FrenshipRings.CP_Owl"))
        {
            threshold /= 2;
            threshold += threshold / 2;
        }
    }
}
