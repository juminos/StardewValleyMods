using HarmonyLib;
using StardewValley.Monsters;

namespace FrenshipRings.HarmonyPatches;

internal class IsInvinciblePatch
{
    internal static IMonitor? _monitor;
    internal static IModHelper? _helper;
    internal IsInvinciblePatch(IMonitor modMonitor, IModHelper modHelper)
    {
        _monitor = modMonitor;
        _helper = modHelper;
    }
    internal void ApplyPatch(Harmony harmony)
    {
        harmony.Patch(AccessTools.Method(typeof(Monster), nameof(Monster.isInvincible), null), postfix: new HarmonyMethod(GetType(), nameof(InvinciblePatch_Postfix)));
    }
    internal static void InvinciblePatch_Postfix(Monster __instance, ref bool __result)
    {
        if (
            (__instance is ShadowBrute || __instance is ShadowGirl || __instance is ShadowGuy || __instance is Shooter || __instance is ShadowShaman) &&
            Game1.player.isWearingRing("juminos.FrenshipRings.CP_Shadow") ||
            __instance is Leaper &&
            Game1.player.isWearingRing("juminos.FrenshipRings.CP_Spider") ||
            __instance is DustSpirit &&
            Game1.player.isWearingRing("juminos.FrenshipRings.CP_Dust")
            )
        {
            __result = true;
            return;
        }
    }
}
