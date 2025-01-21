using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley.Monsters;
using StardewValley;

namespace DinoForm2;

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
            __instance is DinoMonster &&
            Game1.player.hasBuff("juminos.DinoForm2_DinoForm")
            )
        {
            __result = true;
            return;
        }
    }
}
