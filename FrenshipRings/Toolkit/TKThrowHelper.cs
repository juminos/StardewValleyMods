using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FrenshipRings.Toolkit;

/// <summary>
/// Thrown when I get an unexpected enum value.
/// </summary>
/// <typeparam name="T">The enum type that recieved an unexpected value.</typeparam>
public class UnexpectedEnumValueException<T> : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnexpectedEnumValueException{T}"/> class.
    /// </summary>
    /// <param name="value">The unexpected enum value.</param>
    public UnexpectedEnumValueException(T value)
        : base($"Enum {typeof(T).Name} recieved unexpected value {value}")
    {
    }
}

/// <summary>
/// Throw helper for these exceptions.
/// </summary>
public static class TKThrowHelper
{
#if NET6_0_OR_GREATER
    // [StaticTraceHidden] // suppressing error; not sure what this does
#endif
    [DoesNotReturn]
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowUnexpectedEnumValueException<TEnum>(TEnum value)
    {
        throw new UnexpectedEnumValueException<TEnum>(value);
    }

#if NET6_0_OR_GREATER
    // [StaticTraceHidden]
#endif
    [DoesNotReturn]
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static TReturn ThrowUnexpectedEnumValueException<TEnum, TReturn>(TEnum value)
    {
        throw new UnexpectedEnumValueException<TEnum>(value);
    }

#if NET6_0_OR_GREATER
    // [StaticTraceHidden]
#endif
    [DoesNotReturn]
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowIndexOutOfRangeException()
    {
        throw new IndexOutOfRangeException();
    }

#if NET6_0_OR_GREATER
    // [StaticTraceHidden]
#endif
    [DoesNotReturn]
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static TReturn ThrowIndexOutOfRangeException<TReturn>()
    {
        throw new IndexOutOfRangeException();
    }
}
