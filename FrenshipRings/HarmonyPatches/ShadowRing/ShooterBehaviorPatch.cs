using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley.Monsters;

namespace FrenshipRings.HarmonyPatches.ShadowRing;

internal class ShooterBehaviorPatch
{
    internal static IMonitor? _monitor;
    internal static IModHelper? _helper;
    internal ShooterBehaviorPatch(IMonitor modMonitor, IModHelper modHelper)
    {
        _monitor = modMonitor;
        _helper = modHelper;
    }
    internal void ApplyPatch(Harmony harmony)
    {
        if (!ModEntry.SHelper.ModRegistry.IsLoaded("juminos.MonsterHutchStarter"))
        {
            harmony.Patch(AccessTools.Method(typeof(Shooter), nameof(Shooter.behaviorAtGameTick), new[] { typeof(GameTime) }), prefix: new HarmonyMethod(GetType(), nameof(ShadowShooterPatch_Prefix)));
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
}
