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
        harmony.Patch(AccessTools.Method(typeof(NPC), nameof(NPC.withinPlayerThreshold), new[] { typeof(int) }), prefix: new HarmonyMethod(GetType(), nameof(ShadowSightPatch_Prefix)));
        harmony.Patch(AccessTools.Constructor(typeof(Character),new[] { typeof(AnimatedSprite), typeof(Vector2), typeof(int), typeof(string) }), postfix: new HarmonyMethod(GetType(), nameof(ShadowCollisionPatch_Postfix)));
    }

    internal static void ShadowSightPatch_Prefix(NPC __instance, ref int threshold)
    {
        if (__instance is not ShadowBrute ||
            __instance is not ShadowGirl ||
            __instance is not ShadowGuy ||
            __instance is not ShadowShaman)
        {
            return;
        }

        if (Game1.player.isWearingRing("juminos.FrenshipRings.CP_Shadow"))
        {
            threshold = -1;
            //__instance.moveTowardPlayerThreshold.Value = threshold;
        }
    }

    public static void ShadowCollisionPatch_Postfix(Character __instance, AnimatedSprite sprite, Vector2 position, int speed, string name)
    {
        if (__instance is not ShadowBrute ||
            __instance is not ShadowGirl ||
            __instance is not ShadowGuy ||
            __instance is not ShadowShaman)
        {
            return;
        }
        if (Game1.player.isWearingRing("juminos.FrenshipRings.CP_Shadow"))
        {
            __instance.farmerPassesThrough = true;
        }
    }
}
