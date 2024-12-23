﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace FrenshipRings.Toolkit.Extensions;

/// <summary>
/// Extensions for Harmony.
/// </summary>
public static class HarmonyExtensions
{
    /// <summary>
    /// Snitch on all the functions patched.
    /// </summary>
    /// <param name="harmony">Harmony instance.</param>
    /// <param name="monitor">Logger.</param>
    /// <param name="filter">Filter to use. Leave null to not filter.</param>
    /// <param name="transpilersOnly">Whether or not to log transpilers only.</param>
    public static void Snitch(this Harmony harmony, IMonitor monitor, Func<Patch, bool>? filter = null, bool transpilersOnly = false)
    {
        filter ??= (_) => true;
        foreach (MethodBase? method in harmony.GetPatchedMethods())
        {
            method?.Snitch(monitor, filter, transpilersOnly);
        }
    }

    /// <summary>
    /// Snitch on all the functions patched.
    /// </summary>
    /// <param name="harmony">Harmony instance.</param>
    /// <param name="monitor">Logger.</param>
    /// <param name="uniqueID">Unique ID to look for.</param>
    /// <param name="transpilersOnly">Whether or not to log transpilers only.</param>
    public static void Snitch(this Harmony harmony, IMonitor monitor, string uniqueID, bool transpilersOnly = false)
        => harmony.Snitch(monitor, (p) => p.owner == uniqueID, transpilersOnly);

    /// <summary>
    /// Snitch on patches from a single function.
    /// </summary>
    /// <param name="method">Method to look at.</param>
    /// <param name="monitor">Logger.</param>
    /// <param name="filter">Filter. Leave null to not filter.</param>
    /// <param name="transpilersOnly">Whether or not to log transpilers only.</param>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static void Snitch(this MethodBase method, IMonitor monitor, Func<Patch, bool>? filter = null, bool transpilersOnly = false)
    {
        filter ??= (_) => true;
        Patches? patches = Harmony.GetPatchInfo(method);

        if (patches is null)
        {
            if (!transpilersOnly)
            {
                monitor.Log($"No patches found for {method.GetFullName()} when attempting to snitch");
            }
            return;
        }

        if (transpilersOnly && !patches.Transpilers.Any((patch) => filter(patch)))
        {
            return;
        }

        StringBuilder sb = StringBuilderCache.Acquire();
        sb.Append("Patched method ").Append(method.FullDescription());

        if (!transpilersOnly)
        {
            foreach (Patch patch in patches.Prefixes.Where(filter))
            {
                sb.AppendLine().Append("\tPrefixed with method: ").Append(patch.PatchMethod.GetFullName());
            }
            foreach (Patch patch in patches.Postfixes.Where(filter))
            {
                sb.AppendLine().Append("\tPostfixed with method: ").Append(patch.PatchMethod.GetFullName());
            }
        }

        foreach (Patch patch in patches.Transpilers.Where(filter))
        {
            sb.AppendLine().Append("\tTranspiled with method: ").Append(patch.PatchMethod.GetFullName());
        }

        if (!transpilersOnly)
        {
            foreach (Patch patch in patches.Finalizers.Where(filter))
            {
                sb.AppendLine().Append("\tFinalized with method: ").Append(patch.PatchMethod.GetFullName());
            }
        }

        monitor.Log(StringBuilderCache.GetStringAndRelease(sb), LogLevel.Trace);
    }

    /// <summary>
    /// Snitch on patches from a single function.
    /// </summary>
    /// <param name="method">Method to look at.</param>
    /// <param name="monitor">Logger.</param>
    /// <param name="uniqueID">UniqueID to filter for.</param>
    /// <param name="transpilersOnly">Whether or not to log transpilers only.</param>
    public static void Snitch(this MethodBase method, IMonitor monitor, string uniqueID, bool transpilersOnly = false)
        => method.Snitch(monitor, (Patch p) => p.owner == uniqueID, transpilersOnly);
}
