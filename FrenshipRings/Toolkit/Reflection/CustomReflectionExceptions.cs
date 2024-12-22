using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FrenshipRings.Toolkit.Reflection;

/// <summary>
/// Thrown when a method accessed by reflection/Harmony isn't found.
/// </summary>
public class MethodNotFoundException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MethodNotFoundException"/> class.
    /// </summary>
    /// <param name="methodname">Name of the method.</param>
    public MethodNotFoundException(string methodname)
        : base($"{methodname} not found!")
    {
    }
}

public static class ReflectionThrowHelper
{
#if NET6_0_OR_GREATER
    // [StaticTraceHidden] // couldn't resolve error, not sure what this does
#endif
    [DoesNotReturn]
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowMethodNotFoundException(string methodName)
    {
        throw new MethodNotFoundException(methodName);
    }

#if NET6_0_OR_GREATER
    // [StaticTraceHidden] // couldn't resolve error, not sure what this does
#endif
    [DoesNotReturn]
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static T ThrowMethodNotFoundException<T>(string methodName)
    {
        throw new MethodNotFoundException(methodName);
    }
}