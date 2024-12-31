using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using StardewValley.Monsters;

namespace FrenshipRings.HarmonyPatches.DustRing;

internal class DustNerf
{
    internal static IMonitor? _monitor;
    internal static IModHelper? _helper;
    internal DustNerf(IMonitor modMonitor, IModHelper modHelper)
    {
        _monitor = modMonitor;
        _helper = modHelper;
    }
    internal void ApplyPatch(Harmony harmony)
    {
        harmony.Patch(AccessTools.Method(typeof(Monster), nameof(Monster.isInvincible), null), postfix: new HarmonyMethod(GetType(), nameof(DustInvinciblePatch_Postfix)));
        harmony.Patch(AccessTools.Method(typeof(Monster), nameof(Monster.OverlapsFarmerForDamage), null), postfix: new HarmonyMethod(GetType(), nameof(DustOverlapFarmer_Postfix)));
    }
    internal static void DustInvinciblePatch_Postfix(Monster __instance, ref bool __result)
    {
        if (__instance is DustSpirit &&
            Game1.player.isWearingRing("juminos.FrenshipRings.CP_Dust"))
        {
            __result = true;
            return;
        }
    }
    internal static void DustOverlapFarmer_Postfix(Monster __instance, Farmer who, ref bool __result)
    {
        if (__instance is DustSpirit &&
            who.isWearingRing("juminos.FrenshipRings.CP_Dust"))
        {
            __result = false;
            return;
        }
    }

}
