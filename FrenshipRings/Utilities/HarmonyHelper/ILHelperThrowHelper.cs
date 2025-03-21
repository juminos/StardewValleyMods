﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FrenshipRings.Utilities.HarmonyHelper;

/// <summary>
/// Thrown when IL codes are expected but not matched.
/// </summary>
public class NoMatchFoundException : InvalidOperationException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NoMatchFoundException"/> class.
    /// </summary>
    public NoMatchFoundException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NoMatchFoundException"/> class.
    /// </summary>
    /// <param name="message">Message to include.</param>
    public NoMatchFoundException(string? message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NoMatchFoundException"/> class.
    /// </summary>
    /// <param name="instructions">Instructiosn searched for.</param>
    public NoMatchFoundException(IEnumerable<CodeInstructionWrapper> instructions)
        : base($"The desired pattern wasn't found:\n\n" + string.Join('\n', instructions.Select(i => i.ToString())))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NoMatchFoundException"/> class.
    /// </summary>
    /// <param name="message">Message to include.</param>
    /// <param name="innerException">Inner exception.</param>
    public NoMatchFoundException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// Throw helper for the ILHelper.
/// </summary>
public static partial class ILHelperThrowHelper
{
#if NET6_0_OR_GREATER
    // [StaticTraceHidden] // error suppressed; not sure what this does
#endif
    [DoesNotReturn]
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowNoMatchFoundException()
    {
        throw new NoMatchFoundException();
    }

#if NET6_0_OR_GREATER
    // [StaticTraceHidden]
#endif
    [DoesNotReturn]
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowNoMatchFoundException(string? message)
    {
        throw new NoMatchFoundException(message);
    }

#if NET6_0_OR_GREATER
    // [StaticTraceHidden]
#endif
    [DoesNotReturn]
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowNoMatchFoundException(string? message, Exception? inner)
    {
        throw new NoMatchFoundException(message, inner);
    }

#if NET6_0_OR_GREATER
    // [StaticTraceHidden]
#endif
    [DoesNotReturn]
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowNoMatchFoundException(IEnumerable<CodeInstructionWrapper> codeInstructions)
    {
        throw new NoMatchFoundException(codeInstructions);
    }

#if NET6_0_OR_GREATER
    // [StaticTraceHidden]
#endif
    [DoesNotReturn]
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static T ThrowNoMatchFoundException<T>()
    {
        throw new NoMatchFoundException();
    }

#if NET6_0_OR_GREATER
    // [StaticTraceHidden]
#endif
    [DoesNotReturn]
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static T ThrowNoMatchFoundException<T>(string? message)
    {
        throw new NoMatchFoundException(message);
    }

#if NET6_0_OR_GREATER
    // [StaticTraceHidden]
#endif
    [DoesNotReturn]
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static T ThrowNoMatchFoundException<T>(string? message, Exception? inner)
    {
        throw new NoMatchFoundException(message, inner);
    }

#if NET6_0_OR_GREATER
    // [StaticTraceHidden]
#endif
    [DoesNotReturn]
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static T ThrowNoMatchFoundException<T>(IEnumerable<CodeInstructionWrapper> codeInstructions)
    {
        throw new NoMatchFoundException(codeInstructions);
    }
}