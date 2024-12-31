using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley.Monsters;

namespace FrenshipRings.HarmonyPatches.SpiderRing;

internal class SpiderNerf
{
    internal static IMonitor? _monitor;
    internal static IModHelper? _helper;
    internal SpiderNerf(IMonitor modMonitor, IModHelper modHelper)
    {
        _monitor = modMonitor;
        _helper = modHelper;
    }
    internal void ApplyPatch(Harmony harmony)
    {
        harmony.Patch(AccessTools.Method(typeof(Monster), nameof(Monster.isInvincible), null), postfix: new HarmonyMethod(GetType(), nameof(SpiderInvinciblePatch_Postfix)));
        harmony.Patch(AccessTools.Method(typeof(Monster), nameof(Monster.OverlapsFarmerForDamage), null), postfix: new HarmonyMethod(GetType(), nameof(SpiderOverlapFarmer_Postfix)));
    }
    internal static void SpiderInvinciblePatch_Postfix(Monster __instance, ref bool __result)
    {
        if (__instance is Leaper &&
            Game1.player.isWearingRing("juminos.FrenshipRings.CP_Spider"))
        {
            __result = true;
            return;
        }
    }
    internal static void SpiderOverlapFarmer_Postfix(Monster __instance, Farmer who, ref bool __result)
    {
        if (__instance is Leaper &&
            who.isWearingRing("juminos.FrenshipRings.CP_Spider"))
        {
            __result = false;
            return;
        }
    }
}
