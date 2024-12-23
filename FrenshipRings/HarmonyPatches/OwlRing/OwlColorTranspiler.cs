using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using FrenshipRings.ReflectionManager;
using FrenshipRings.Toolkit;
using FrenshipRings.Utilities.HarmonyHelper;
using HarmonyLib;
using StardewValley.BellsAndWhistles;
using Microsoft.Xna.Framework;
using FrenshipRings.Toolkit.Extensions;

namespace FrenshipRings.HarmonyPatches.OwlRing;

/// <summary>
/// Patches owls to try to see their pretty sprite in the day.
/// </summary>
[HarmonyPatch(typeof(Owl))]
internal static class OwlColorTranspiler
{
    [MethodImpl(TKConstants.Hot)]
    private static Color GetColorForTime(Color prevcolor)
    {
        if (Game1.isDarkOut(Game1.currentLocation))
        {
            return prevcolor;
        }
        if (Game1.isStartingToGetDarkOut(Game1.currentLocation))
        {
            return Color.LightSteelBlue;
        }
        return Color.White;
    }

    [HarmonyPatch(nameof(Owl.drawAboveFrontLayer))]
    private static IEnumerable<CodeInstruction>? Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator gen, MethodBase original)
    {
        try
        {
            ILHelper helper = new(original, instructions, ModEntry.SMonitor, gen);
            helper.FindNext(new CodeInstructionWrapper[]
            {
                (OpCodes.Call, typeof(Color).GetCachedProperty(nameof(Color.MediumBlue), ReflectionCache.FlagTypes.StaticFlags).GetGetMethod()),
            })
            .Advance(1)
            .Insert(new CodeInstruction[]
            {
                new (OpCodes.Call, typeof(OwlColorTranspiler).GetCachedMethod(nameof(GetColorForTime), ReflectionCache.FlagTypes.StaticFlags)),
            });

            // helper.Print();
            return helper.Render();
        }
        catch (Exception ex)
        {
            ModEntry.SMonitor.Log($"Ran into error transpiling {original.Name}\n\n{ex}", LogLevel.Error);
            original.Snitch(ModEntry.SMonitor);
        }
        return null;
    }
}
