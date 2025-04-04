﻿using System.Reflection;
using CommunityToolkit.Diagnostics;
using FastExpressionCompiler.LightExpression;
using System.Diagnostics.CodeAnalysis;

namespace FrenshipRings.Toolkit.Reflection;

/// <summary>
/// Makes delegates from reflection stuff.
/// </summary>
/// <remarks>Inspired by https://github.com/ameisen/SV-SpriteMaster/blob/master/SpriteMaster/Extensions/ReflectionExtDelegates.cs .</remarks>
public static class FastReflection
{
    /// <summary>
    /// Gets a getter for an instance field.
    /// </summary>
    /// <typeparam name="TObject">Effective type of the object.</typeparam>
    /// <typeparam name="TField">Effective type of the field.</typeparam>
    /// <param name="field">The fieldinfo.</param>
    /// <returns>Delegate that gets the field's value.</returns>
    /// <exception cref="ArgumentException">Type mismatch.</exception>
    [return: NotNullIfNotNull("field")]
    public static Func<TObject, TField>? GetInstanceFieldGetter<TObject, TField>(this FieldInfo? field)
    {
        if (field is null)
        {
            return null;
        }
        if (!typeof(TObject).IsAssignableTo(field.DeclaringType))
        {
            ThrowHelper.ThrowArgumentException($"{typeof(TObject).FullName} is not assignable to {field.DeclaringType?.FullName}");
        }
        if (!typeof(TField).IsAssignableFrom(field.FieldType))
        {
            ThrowHelper.ThrowArgumentException($"{typeof(TField).FullName} is not assignable from {field.FieldType.FullName}");
        }
        if (field.IsStatic)
        {
            ThrowHelper.ThrowArgumentException($"Expected a non-static field");
        }

        ParameterExpression? objparam = Expression.ParameterOf<TObject>("obj");
        MemberExpression? fieldgetter = Expression.Field(objparam, field);
        return Expression.Lambda<Func<TObject, TField>>(fieldgetter, objparam).CompileFast();
    }

    /// <summary>
    /// Gets a setter for an instance field.
    /// </summary>
    /// <typeparam name="TObject">Effective type of the object.</typeparam>
    /// <typeparam name="TField">Effective type of the field.</typeparam>
    /// <param name="field">The fieldinfo.</param>
    /// <returns>Delegate that allows setting the field's value.</returns>
    /// <exception cref="ArgumentException">Type mismatch.</exception>
    [return: NotNullIfNotNull("field")]
    public static Action<TObject, TField>? GetInstanceFieldSetter<TObject, TField>(this FieldInfo? field)
    {
        if (field is null)
        {
            return null;
        }
        if (!typeof(TObject).IsAssignableTo(field.DeclaringType))
        {
            ThrowHelper.ThrowArgumentException($"{typeof(TObject).FullName} is not assignable to {field.DeclaringType?.FullName}");
        }
        if (!typeof(TField).IsAssignableTo(field.FieldType))
        {
            ThrowHelper.ThrowArgumentException($"{typeof(TField).FullName} is not assignable to {field.FieldType.FullName}");
        }
        if (field.IsStatic)
        {
            ThrowHelper.ThrowArgumentException($"Expected a non-static field");
        }

        ParameterExpression? objparam = Expression.ParameterOf<TObject>("obj");
        ParameterExpression? fieldval = Expression.ParameterOf<TField>("fieldval");
        UnaryExpression? convertfield = Expression.Convert(fieldval, field.FieldType);
        MemberExpression? fieldsetter = Expression.Field(objparam, field);
        BinaryExpression? assignexpress = Expression.Assign(fieldsetter, convertfield);

        return Expression.Lambda<Action<TObject, TField>>(assignexpress, objparam, fieldval).CompileFast();
    }

    /// <summary>
    /// Gets a getter for a static field.
    /// </summary>
    /// <typeparam name="TField">Effective type of the field.</typeparam>
    /// <param name="field">Fieldinfo.</param>
    /// <returns>A delegate that allows getting the value from a static field.</returns>
    /// <exception cref="ArgumentException">Type mismatch.</exception>
    [return: NotNullIfNotNull("field")]
    public static Func<TField>? GetStaticFieldGetter<TField>(this FieldInfo? field)
    {
        if (field is null)
        {
            return null;
        }
        if (!typeof(TField).IsAssignableFrom(field.FieldType))
        {
            ThrowHelper.ThrowArgumentException($"{typeof(TField).FullName} is not assignable from {field.FieldType.FullName}");
        }
        if (!field.IsStatic)
        {
            ThrowHelper.ThrowArgumentException($"Expected a static field");
        }

        MemberExpression? fieldgetter = Expression.Field(null, field);
        return Expression.Lambda<Func<TField>>(fieldgetter).CompileFast();
    }

    /// <summary>
    /// Gets a setter for a static field.
    /// </summary>
    /// <typeparam name="TField">Effective type.</typeparam>
    /// <param name="field">Fieldinfo.</param>
    /// <returns>Delegate that allows setting of a static field.</returns>
    /// <exception cref="ArgumentException">Type mismatch.</exception>
    [return: NotNullIfNotNull("field")]
    public static Action<TField>? GetStaticFieldSetter<TField>(this FieldInfo? field)
    {
        if (field is null)
        {
            return null;
        }
        if (!typeof(TField).IsAssignableTo(field.FieldType))
        {
            ThrowHelper.ThrowArgumentException($"{typeof(TField).FullName} is not assignable to {field.FieldType.FullName}");
        }
        if (!field.IsStatic)
        {
            ThrowHelper.ThrowArgumentException($"Expected a static field");
        }

        ParameterExpression? fieldval = Expression.ParameterOf<TField>("fieldval");
        UnaryExpression? convertfield = Expression.Convert(fieldval, field.FieldType);
        MemberExpression? fieldsetter = Expression.Field(null, field);
        BinaryExpression? assignexpress = Expression.Assign(fieldsetter, convertfield);
        return Expression.Lambda<Action<TField>>(assignexpress, fieldval).CompileFast();
    }

    /// <summary>
    /// Gets an isinst for this particular type.
    /// </summary>
    /// <param name="type">Type.</param>
    /// <returns>IsInit method.</returns>
    [return: NotNullIfNotNull("type")]
    public static Func<object, bool>? GetTypeIs(this Type? type)
    {
        if (type is null)
        {
            return null;
        }
        ParameterExpression? obj = Expression.ParameterOf<object>("obj");
        TypeBinaryExpression? express = Expression.TypeIs(obj, type);
        return Expression.Lambda<Func<object, bool>>(express, obj).CompileFast();
    }
}