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
            ((__instance is ShadowBrute || __instance is ShadowGirl || __instance is ShadowGuy || __instance is Shooter || __instance is ShadowShaman) &&
            !ModEntry.Config.LethalRings &&
            Game1.player.isWearingRing("juminos.FrenshipRings.CP_Shadow")) ||
            (__instance is Leaper &&
            !ModEntry.Config.LethalRings &&
            Game1.player.isWearingRing("juminos.FrenshipRings.CP_Spider")) ||
            (__instance is Bat bat && !bat.magmaSprite.Value && !bat.hauntedSkull.Value &&
            !ModEntry.Config.LethalRings &&
            Game1.player.isWearingRing("juminos.FrenshipRings.CP_Bat")) ||
            (__instance is Bat magmaSprite && magmaSprite.magmaSprite.Value &&
            !ModEntry.Config.LethalRings &&
            Game1.player.isWearingRing("juminos.FrenshipRings.CP_MagmaSprite")) ||
            ((__instance is GreenSlime || __instance is BigSlime) &&
            ModEntry.Config.FriendlySlimeRing &&
            Game1.player.isWearingRing("520")) ||
            (__instance is DustSpirit dust && !dust.isHardModeMonster.Value &&
            !ModEntry.Config.LethalRings &&
            Game1.player.isWearingRing("juminos.FrenshipRings.CP_Dust")) ||
            (__instance is RockCrab crab &&
            !ModEntry.Config.LethalRings &&
            Game1.player.isWearingRing("810"))
            )
        {
            __result = true;
            return;
        }
    }
}
