using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FrenshipRings.ReflectionManager;
using FrenshipRings.Utilities.HarmonyHelper;
using FrenshipRings.Toolkit.Extensions;
using HarmonyLib;
using StardewValley.Monsters;

namespace FrenshipRings.HarmonyPatches.OwlRing;

/// <summary>
/// A patch so the farmer is seen less by lava lurks.
/// </summary>
[HarmonyPatch(typeof(LavaLurk))]
internal static class LavaLurkNerf
{
    private static float AdjustLavaLurkDistance(float original, Farmer farmer)
        => (farmer.isWearingRing("juminos.FrenshipRings.CP_Owl")) ? original / 2 : original;

    [HarmonyPatch(nameof(LavaLurk.TargetInRange))]
    private static IEnumerable<CodeInstruction>? Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator gen, MethodBase original)
    {
        try
        {
            ILHelper helper = new(original, instructions, ModEntry.SMonitor, gen);
            helper.FindNext(new CodeInstructionWrapper[]
            {
                (OpCodes.Call, typeof(Math).GetCachedMethod<float>(nameof(Math.Abs), ReflectionCache.FlagTypes.StaticFlags)),
                (OpCodes.Ldc_R4, 640f),
            })
            .Advance(2)
            .Insert(new CodeInstruction[]
            {
                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldfld, typeof(LavaLurk).GetCachedField(nameof(LavaLurk.targettedFarmer), ReflectionCache.FlagTypes.InstanceFlags)),
                new(OpCodes.Call, typeof(LavaLurkNerf).GetCachedMethod(nameof(AdjustLavaLurkDistance), ReflectionCache.FlagTypes.StaticFlags)),
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
