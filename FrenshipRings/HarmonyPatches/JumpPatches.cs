using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using FrenshipRings;
using FrenshipRings.Toolkit;
using HarmonyLib;
using StardewValley.Tools;

namespace FrenshipRings.HarmonyPatches;

/// <summary>
/// Patches to make sure the player doesn't move in certain times.
/// </summary>
[HarmonyPatch]
[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Named for Harmony.")]
internal static class JumpPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(MeleeWeapon), nameof(MeleeWeapon.leftClick))]
    private static bool PrefixSwordSwing(Farmer who)
    {
        return ModEntry.CurrentJumper?.IsValid(out Farmer? farmer) != true || !ReferenceEquals(who, farmer);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Game1), nameof(Game1.pressUseToolButton))]
    private static bool PrefixUseTool()
    {
        return ModEntry.CurrentJumper?.IsValid(out Farmer? farmer) != true;
    }

    [HarmonyPostfix]
    [MethodImpl(TKConstants.Hot)]
    [HarmonyPatch(typeof(Farmer), nameof(Farmer.getDrawLayer))]
    private static void PostfixGetDrawLayer(Farmer __instance, ref float __result)
    {
        switch (MathF.Sign(__instance.yJumpVelocity))
        {
            // player rising.
            case 1:

                // and moving forward
                if (MathF.Sign(__instance.Position.Y - __instance.lastPosition.Y) == 1)
                {
                    __result -= 0.0035f;
                    return;
                }

                __result += 0.0035f;
                return;

            // player falling
            case -1:

                // and moving backwards
                if (MathF.Sign(__instance.Position.Y - __instance.lastPosition.Y) == -1)
                {
                    __result -= 0.0035f;
                    return;
                }

                __result += 0.0035f;
                return;
                break;
        }
    }
}