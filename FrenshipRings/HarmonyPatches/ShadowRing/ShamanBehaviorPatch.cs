using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley.Monsters;

namespace FrenshipRings.HarmonyPatches.ShadowRing;

internal class ShamanBehaviorPatch
{
    internal static IMonitor? _monitor;
    internal static IModHelper? _helper;
    internal ShamanBehaviorPatch(IMonitor modMonitor, IModHelper modHelper)
    {
        _monitor = modMonitor;
        _helper = modHelper;
    }
    internal void ApplyPatch(Harmony harmony)
    {
        if (!ModEntry.SHelper.ModRegistry.IsLoaded("juminos.MonsterHutchStarter"))
        {
            harmony.Patch(AccessTools.Method(typeof(ShadowShaman), nameof(ShadowShaman.behaviorAtGameTick), new[] { typeof(GameTime) }), prefix: new HarmonyMethod(GetType(), nameof(ShadowShamanCastPatch_Prefix)));
        }
    }
    internal static void ShadowShamanCastPatch_Prefix(ShadowShaman __instance, GameTime time)
    {
        if (__instance.coolDown <= 1500 && Game1.player.isWearingRing("juminos.FrenshipRings.CP_Shadow"))
        {
            __instance.coolDown = 1500;
        }
    }
}
