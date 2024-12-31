using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using FrenshipRings.Toolkit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Monsters;

namespace FrenshipRings.HarmonyPatches.ShadowRing;

internal class ShadowNerf
{
    internal static IMonitor? _monitor;
    internal static IModHelper? _helper;
    internal ShadowNerf(IMonitor modMonitor, IModHelper modHelper)
    {
        _monitor = modMonitor;
        _helper = modHelper;
    }
    internal void ApplyPatch(Harmony harmony)
    {
        harmony.Patch(AccessTools.Method(typeof(Monster), nameof(Monster.OverlapsFarmerForDamage), null), postfix: new HarmonyMethod(GetType(), nameof(ShadowOverlapFarmer_Postfix)));
        harmony.Patch(AccessTools.Method(typeof(ShadowShaman), nameof(ShadowShaman.behaviorAtGameTick), new[] { typeof(GameTime) }), prefix: new HarmonyMethod(GetType(), nameof(ShadowShamanCastPatch_Prefix)));
        harmony.Patch(AccessTools.Method(typeof(Monster), nameof(Monster.isInvincible), null), postfix: new HarmonyMethod(GetType(), nameof(ShadowInvinciblePatch_Postfix)));
        harmony.Patch(AccessTools.Method(typeof(Shooter), nameof(Shooter.behaviorAtGameTick), new[] { typeof(GameTime) }), prefix: new HarmonyMethod(GetType(), nameof(ShadowShooterPatch_Prefix)));


        // breaks monster behavior
        //harmony.Patch(AccessTools.Method(typeof(NPC), nameof(NPC.withinPlayerThreshold), new[] { typeof(int) }), prefix: new HarmonyMethod(GetType(), nameof(ShadowSightPatch_Prefix)));
    }



    internal static void ShadowInvinciblePatch_Postfix(Monster __instance, ref bool __result)
    {
        if ((__instance is ShadowBrute ||
            __instance is ShadowGirl ||
            __instance is ShadowGuy ||
            __instance is Shooter ||
            __instance is ShadowShaman) &&
            Game1.player.isWearingRing("juminos.FrenshipRings.CP_Shadow"))
        {
            __result = true;
            return;
        }
    }
    internal static void ShadowOverlapFarmer_Postfix(Monster __instance, Farmer who, ref bool __result)
    {
        if ((__instance is ShadowBrute ||
            __instance is ShadowGirl ||
            __instance is ShadowGuy ||
            __instance is Shooter ||
            __instance is ShadowShaman) &&
            who.isWearingRing("juminos.FrenshipRings.CP_Shadow"))
        {
            __result = false;
            return;
        }
    }
    internal static void ShadowShamanCastPatch_Prefix(ShadowShaman __instance, GameTime time)
    {
        if (__instance.coolDown <= 1500 && Game1.player.isWearingRing("juminos.FrenshipRings.CP_Shadow"))
        {
            __instance.coolDown = 1500;
        }
    }
    internal static void ShadowShooterPatch_Prefix(Shooter __instance, GameTime time)
    {
        if (Game1.player.isWearingRing("juminos.FrenshipRings.CP_Shadow") && (__instance.shooting.Value == true || (__instance.shooting.Value == false && __instance.nextShot <= 0f)))
        {
            __instance.shooting.Value = false;
            __instance.nextShot = 2f;
        }
    }

    // breaks monster behavior

    //internal static void ShadowSightPatch_Prefix(NPC __instance, ref int threshold)
    //{
    //    if (__instance is not Monster monster)
    //    {
    //        return;
    //    }
    //    if (monster.Name.ToLower().Contains("shadow"))
    //    {
    //        if (Game1.player.isWearingRing("juminos.FrenshipRings.CP_Shadow"))
    //        {
    //            _monitor.Log("removing shadow threshold", LogLevel.Trace);
    //            threshold = 0;
    //        }
    //    }
    //}
}
