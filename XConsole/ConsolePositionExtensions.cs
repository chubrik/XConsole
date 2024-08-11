#pragma warning disable CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)
#pragma warning disable CS1584 // XML comment has syntactically incorrect cref attribute
#pragma warning disable CS1658 // Warning is overriding an error
#if !NET
#pragma warning disable CS8604 // Possible null reference argument.
#endif

namespace Chubrik.XConsole;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
#if NET
using System.Runtime.Versioning;
#endif
#if NET7_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

/// <summary>
/// The most useful extensions for <see cref="ConsolePosition"/>.
/// </summary>
#if NET
[UnsupportedOSPlatform("android")]
[UnsupportedOSPlatform("browser")]
[UnsupportedOSPlatform("ios")]
[UnsupportedOSPlatform("tvos")]
#endif
public static class ConsolePositionExtensions
{
    #region Write

    /// <inheritdoc cref="TryWrite(ConsolePosition)"/>
    [Obsolete("At least one argument should be specified.", error: true)]
    public static void Write(this ConsolePosition position) => throw new InvalidOperationException();

    /// <returns>The new end <see cref="ConsolePosition"/> structure.</returns>
    /// <exception cref="ArgumentOutOfRangeException"/>
    /// <inheritdoc cref="TryWrite(ConsolePosition, string?)"/>
    public static ConsolePosition Write(this ConsolePosition position, string? value)
    {
        return XConsole.WriteToPosition(position, [ConsoleItem.Parse(value)]);
    }

    /// <returns>The new end <see cref="ConsolePosition"/> structure.</returns>
    /// <exception cref="ArgumentOutOfRangeException"/>
    /// <inheritdoc cref="TryWrite(ConsolePosition, string?[])"/>
    public static ConsolePosition Write(this ConsolePosition position, params string?[] values)
    {
        Debug.Assert(values.Length > 0);
        var items = new ConsoleItem[values.Length];

        for (var i = 0; i < values.Length; i++)
            items[i] = ConsoleItem.Parse(values[i]);

        return XConsole.WriteToPosition(position, items);
    }

    /// <inheritdoc cref="Write(ConsolePosition, string?[])"/>
    public static ConsolePosition Write(this ConsolePosition position, IReadOnlyList<string?> values)
    {
        var items = new ConsoleItem[values.Count];

        for (var i = 0; i < values.Count; i++)
            items[i] = ConsoleItem.Parse(values[i]);

        return XConsole.WriteToPosition(position, items);
    }

    /// <returns>The new end <see cref="ConsolePosition"/> structure.</returns>
    /// <exception cref="ArgumentOutOfRangeException"/>
    /// <inheritdoc cref="TryWrite(ConsolePosition, bool)"/>
    public static ConsolePosition Write(this ConsolePosition position, bool value)
    {
        return XConsole.WriteToPosition(position, [new ConsoleItem(value.ToString())]);
    }

    /// <returns>The new end <see cref="ConsolePosition"/> structure.</returns>
    /// <exception cref="ArgumentOutOfRangeException"/>
    /// <inheritdoc cref="TryWrite(ConsolePosition, char)"/>
    public static ConsolePosition Write(this ConsolePosition position, char value)
    {
        return XConsole.WriteToPosition(position, [new ConsoleItem(value.ToString())]);
    }

    /// <returns>The new end <see cref="ConsolePosition"/> structure.</returns>
    /// <exception cref="ArgumentOutOfRangeException"/>
    /// <inheritdoc cref="TryWrite(ConsolePosition, char[])"/>
    public static ConsolePosition Write(this ConsolePosition position, char[]? buffer)
    {
        if (buffer == null || buffer.Length == 0)
            return position;

        return XConsole.WriteToPosition(position, [new ConsoleItem(new string(buffer))]);
    }

    /// <returns>The new end <see cref="ConsolePosition"/> structure.</returns>
    /// <exception cref="ArgumentOutOfRangeException"/>
    /// <inheritdoc cref="TryWrite(ConsolePosition, char[], int, int)"/>
    public static ConsolePosition Write(this ConsolePosition position, char[] buffer, int index, int count)
    {
        if (count == 0)
            return position;

        return XConsole.WriteToPosition(position, [new ConsoleItem(new string(buffer, index, count))]);
    }

    /// <returns>The new end <see cref="ConsolePosition"/> structure.</returns>
    /// <exception cref="ArgumentOutOfRangeException"/>
    /// <inheritdoc cref="TryWrite(ConsolePosition, decimal)"/>
    public static ConsolePosition Write(this ConsolePosition position, decimal value)
    {
        return XConsole.WriteToPosition(position, [new ConsoleItem(value.ToString())]);
    }

    /// <returns>The new end <see cref="ConsolePosition"/> structure.</returns>
    /// <exception cref="ArgumentOutOfRangeException"/>
    /// <inheritdoc cref="TryWrite(ConsolePosition, double)"/>
    public static ConsolePosition Write(this ConsolePosition position, double value)
    {
        return XConsole.WriteToPosition(position, [new ConsoleItem(value.ToString())]);
    }

    /// <returns>The new end <see cref="ConsolePosition"/> structure.</returns>
    /// <exception cref="ArgumentOutOfRangeException"/>
    /// <inheritdoc cref="TryWrite(ConsolePosition, int)"/>
    public static ConsolePosition Write(this ConsolePosition position, int value)
    {
        return XConsole.WriteToPosition(position, [new ConsoleItem(value.ToString())]);
    }

    /// <returns>The new end <see cref="ConsolePosition"/> structure.</returns>
    /// <exception cref="ArgumentOutOfRangeException"/>
    /// <inheritdoc cref="TryWrite(ConsolePosition, long)"/>
    public static ConsolePosition Write(this ConsolePosition position, long value)
    {
        return XConsole.WriteToPosition(position, [new ConsoleItem(value.ToString())]);
    }

    /// <returns>The new end <see cref="ConsolePosition"/> structure.</returns>
    /// <exception cref="ArgumentOutOfRangeException"/>
    /// <inheritdoc cref="TryWrite(ConsolePosition, object?)"/>
    public static ConsolePosition Write(this ConsolePosition position, object? value)
    {
        var valueStr = value?.ToString();

        if (string.IsNullOrEmpty(valueStr))
            return position;

        return XConsole.WriteToPosition(position, [new ConsoleItem(valueStr)]);
    }

    /// <returns>The new end <see cref="ConsolePosition"/> structure.</returns>
    /// <exception cref="ArgumentOutOfRangeException"/>
    /// <inheritdoc cref="TryWrite(ConsolePosition, float)"/>
    public static ConsolePosition Write(this ConsolePosition position, float value)
    {
        return XConsole.WriteToPosition(position, [new ConsoleItem(value.ToString())]);
    }

    /// <returns>The new end <see cref="ConsolePosition"/> structure.</returns>
    /// <exception cref="ArgumentOutOfRangeException"/>
    /// <inheritdoc cref="TryWrite(ConsolePosition, string, object?)"/>
    public static ConsolePosition Write(this ConsolePosition position,
#if NET7_0_OR_GREATER
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
        string format, object? arg0)
    {
        if (format.Length == 0)
            return position;

        return XConsole.WriteToPosition(position, [ConsoleItem.Parse(string.Format(format, arg0))]);
    }

    /// <returns>The new end <see cref="ConsolePosition"/> structure.</returns>
    /// <exception cref="ArgumentOutOfRangeException"/>
    /// <inheritdoc cref="TryWrite(ConsolePosition, string, object?, object?)"/>
    public static ConsolePosition Write(this ConsolePosition position,
#if NET7_0_OR_GREATER
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
        string format, object? arg0, object? arg1)
    {
        if (format.Length == 0)
            return position;

        return XConsole.WriteToPosition(position, [ConsoleItem.Parse(string.Format(format, arg0, arg1))]);
    }

    /// <returns>The new end <see cref="ConsolePosition"/> structure.</returns>
    /// <exception cref="ArgumentOutOfRangeException"/>
    /// <inheritdoc cref="TryWrite(ConsolePosition, string, object?, object?, object?)"/>
    public static ConsolePosition Write(this ConsolePosition position,
#if NET7_0_OR_GREATER
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
        string format, object? arg0, object? arg1, object? arg2)
    {
        if (format.Length == 0)
            return position;

        return XConsole.WriteToPosition(position, [ConsoleItem.Parse(string.Format(format, arg0, arg1, arg2))]);
    }

    /// <returns>The new end <see cref="ConsolePosition"/> structure.</returns>
    /// <exception cref="ArgumentOutOfRangeException"/>
    /// <inheritdoc cref="TryWrite(ConsolePosition, string, object?[])"/>
    public static ConsolePosition Write(this ConsolePosition position,
#if NET7_0_OR_GREATER
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
        string format, params object?[]? arg)
    {
        if (format.Length == 0)
            return position;

        return XConsole.WriteToPosition(position, [ConsoleItem.Parse(string.Format(format, arg ?? []))]);
    }

    /// <returns>The new end <see cref="ConsolePosition"/> structure.</returns>
    /// <exception cref="ArgumentOutOfRangeException"/>
    /// <inheritdoc cref="TryWrite(ConsolePosition, uint)"/>
    public static ConsolePosition Write(this ConsolePosition position, uint value)
    {
        return XConsole.WriteToPosition(position, [new ConsoleItem(value.ToString())]);
    }

    /// <returns>The new end <see cref="ConsolePosition"/> structure.</returns>
    /// <exception cref="ArgumentOutOfRangeException"/>
    /// <inheritdoc cref="TryWrite(ConsolePosition, ulong)"/>
    public static ConsolePosition Write(this ConsolePosition position, ulong value)
    {
        return XConsole.WriteToPosition(position, [new ConsoleItem(value.ToString())]);
    }

    #endregion

    #region TryWrite

    /// <inheritdoc cref="XConsole.Write()"/>
    [Obsolete("At least one argument should be specified.", error: true)]
    public static void TryWrite(this ConsolePosition position) => throw new InvalidOperationException();

    /// <summary>
    /// Wtites the specified string directly to a position.
    /// Text can be colored using a simple <see href="https://github.com/chubrik/XConsole#coloring">microsyntax</see>.
    /// </summary>
    /// <param name="value">
    /// The value to write.
    /// Text can be colored using a simple <see href="https://github.com/chubrik/XConsole#coloring">microsyntax</see>.
    /// </param>
    /// <returns>
    /// The new end <see cref="ConsolePosition"/> structure,
    /// or <see cref="null"/> if the current position actually points outside the console buffer area.
    /// </returns>
    /// <exception cref="System.Security.SecurityException"/>
    /// <exception cref="System.IO.IOException"/>
    public static ConsolePosition? TryWrite(this ConsolePosition position, string? value)
    {
        return TryWriteBase(position, [ConsoleItem.Parse(value)]);
    }

    /// <summary>
    /// Wtites the specified set of string values directly to a position.
    /// Text can be colored using a simple <see href="https://github.com/chubrik/XConsole#coloring">microsyntax</see>.
    /// </summary>
    /// <param name="values">
    /// The set of values to write.
    /// Text can be colored using a simple <see href="https://github.com/chubrik/XConsole#coloring">microsyntax</see>.
    /// </param>
    /// <inheritdoc cref="TryWrite(ConsolePosition, string?)"/>
    public static ConsolePosition? TryWrite(this ConsolePosition position, params string?[] values)
    {
        Debug.Assert(values.Length > 0);
        var items = new ConsoleItem[values.Length];

        for (var i = 0; i < values.Length; i++)
            items[i] = ConsoleItem.Parse(values[i]);

        return TryWriteBase(position, items);
    }

    /// <inheritdoc cref="TryWrite(ConsolePosition, string?[])"/>
    public static ConsolePosition? TryWrite(this ConsolePosition position, IReadOnlyList<string?> values)
    {
        var items = new ConsoleItem[values.Count];

        for (var i = 0; i < values.Count; i++)
            items[i] = ConsoleItem.Parse(values[i]);

        return TryWriteBase(position, items);
    }

    /// <summary>
    /// Writes the text representation of the specified Boolean value directly to a position.
    /// </summary>
    /// <inheritdoc cref="Console.Write(bool)"/>
    /// <inheritdoc cref="TryWrite(ConsolePosition, string?)"/>
    public static ConsolePosition? TryWrite(this ConsolePosition position, bool value)
    {
        return TryWriteBase(position, [new ConsoleItem(value.ToString())]);
    }

    /// <summary>
    /// Writes the specified Unicode character value directly to a position.
    /// </summary>
    /// <inheritdoc cref="Console.Write(char)"/>
    /// <inheritdoc cref="TryWrite(ConsolePosition, string?)"/>
    public static ConsolePosition? TryWrite(this ConsolePosition position, char value)
    {
        return TryWriteBase(position, [new ConsoleItem(value.ToString())]);
    }

    /// <summary>
    /// Writes the specified array of Unicode characters directly to a position.
    /// </summary>
    /// <inheritdoc cref="Console.Write(char[])"/>
    /// <inheritdoc cref="TryWrite(ConsolePosition, string?)"/>
    public static ConsolePosition? TryWrite(this ConsolePosition position, char[]? buffer)
    {
        if (buffer == null || buffer.Length == 0)
            return position;

        return TryWriteBase(position, [new ConsoleItem(new string(buffer))]);
    }

    /// <summary>
    /// Writes the specified subarray of Unicode characters directly to a position.
    /// </summary>
    /// <inheritdoc cref="Console.Write(char[], int, int)"/>
    /// <inheritdoc cref="TryWrite(ConsolePosition, string?)"/>
    public static ConsolePosition? TryWrite(this ConsolePosition position, char[] buffer, int index, int count)
    {
        if (count == 0)
            return position;

        return TryWriteBase(position, [new ConsoleItem(new string(buffer, index, count))]);
    }

    /// <summary>
    /// Writes the text representation of the specified <see cref="decimal"/> value directly to a position.
    /// </summary>
    /// <inheritdoc cref="Console.Write(decimal)"/>
    /// <inheritdoc cref="TryWrite(ConsolePosition, string?)"/>
    public static ConsolePosition? TryWrite(this ConsolePosition position, decimal value)
    {
        return TryWriteBase(position, [new ConsoleItem(value.ToString())]);
    }

    /// <summary>
    /// Writes the text representation of the specified double-precision floating-point value directly to a position.
    /// </summary>
    /// <inheritdoc cref="Console.Write(double)"/>
    /// <inheritdoc cref="TryWrite(ConsolePosition, string?)"/>
    public static ConsolePosition? TryWrite(this ConsolePosition position, double value)
    {
        return TryWriteBase(position, [new ConsoleItem(value.ToString())]);
    }

    /// <summary>
    /// Writes the text representation of the specified 32-bit signed integer value directly to a position.
    /// </summary>
    /// <inheritdoc cref="Console.Write(int)"/>
    /// <inheritdoc cref="TryWrite(ConsolePosition, string?)"/>
    public static ConsolePosition? TryWrite(this ConsolePosition position, int value)
    {
        return TryWriteBase(position, [new ConsoleItem(value.ToString())]);
    }

    /// <summary>
    /// Writes the text representation of the specified 64-bit signed integer value directly to a position.
    /// </summary>
    /// <inheritdoc cref="Console.Write(long)"/>
    /// <inheritdoc cref="TryWrite(ConsolePosition, string?)"/>
    public static ConsolePosition? TryWrite(this ConsolePosition position, long value)
    {
        return TryWriteBase(position, [new ConsoleItem(value.ToString())]);
    }

    /// <summary>
    /// Writes the text representation of the specified object directly to a position.
    /// </summary>
    /// <inheritdoc cref="Console.Write(object?)"/>
    /// <inheritdoc cref="TryWrite(ConsolePosition, string?)"/>
    public static ConsolePosition? TryWrite(this ConsolePosition position, object? value)
    {
        var valueStr = value?.ToString();

        if (string.IsNullOrEmpty(valueStr))
            return position;

        return TryWriteBase(position, [new ConsoleItem(valueStr)]);
    }

    /// <summary>
    /// Writes the text representation of the specified single-precision floating-point value directly to a position.
    /// </summary>
    /// <inheritdoc cref="Console.Write(float)"/>
    /// <inheritdoc cref="TryWrite(ConsolePosition, string?)"/>
    public static ConsolePosition? TryWrite(this ConsolePosition position, float value)
    {
        return TryWriteBase(position, [new ConsoleItem(value.ToString())]);
    }

    /// <summary>
    /// Writes the text representation of the specified object directly to a position
    /// using the specified format information.
    /// Text can be colored using a simple <see href="https://github.com/chubrik/XConsole#coloring">microsyntax</see>.
    /// </summary>
    /// <param name="format">
    /// A composite format string.
    /// Text can be colored using a simple <see href="https://github.com/chubrik/XConsole#coloring">microsyntax</see>.
    /// </param>
    /// <inheritdoc cref="Console.Write(string, object?)"/>
    /// <inheritdoc cref="TryWrite(ConsolePosition, string?)"/>
    public static ConsolePosition? TryWrite(this ConsolePosition position,
#if NET7_0_OR_GREATER
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
        string format, object? arg0)
    {
        if (format.Length == 0)
            return position;

        return TryWriteBase(position, [ConsoleItem.Parse(string.Format(format, arg0))]);
    }

    /// <summary>
    /// Writes the text representation of the specified object directly to a position
    /// using the specified format information.
    /// Text can be colored using a simple <see href="https://github.com/chubrik/XConsole#coloring">microsyntax</see>.
    /// </summary>
    /// <param name="format">
    /// A composite format string.
    /// Text can be colored using a simple <see href="https://github.com/chubrik/XConsole#coloring">microsyntax</see>.
    /// </param>
    /// <inheritdoc cref="Console.Write(string, object?, object?)"/>
    /// <inheritdoc cref="TryWrite(ConsolePosition, string?)"/>
    public static ConsolePosition? TryWrite(this ConsolePosition position,
#if NET7_0_OR_GREATER
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
        string format, object? arg0, object? arg1)
    {
        if (format.Length == 0)
            return position;

        return TryWriteBase(position, [ConsoleItem.Parse(string.Format(format, arg0, arg1))]);
    }

    /// <summary>
    /// Writes the text representation of the specified object directly to a position
    /// using the specified format information.
    /// Text can be colored using a simple <see href="https://github.com/chubrik/XConsole#coloring">microsyntax</see>.
    /// </summary>
    /// <param name="format">
    /// A composite format string.
    /// Text can be colored using a simple <see href="https://github.com/chubrik/XConsole#coloring">microsyntax</see>.
    /// </param>
    /// <inheritdoc cref="Console.Write(string, object?, object?, object?)"/>
    /// <inheritdoc cref="TryWrite(ConsolePosition, string?)"/>
    public static ConsolePosition? TryWrite(this ConsolePosition position,
#if NET7_0_OR_GREATER
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
        string format, object? arg0, object? arg1, object? arg2)
    {
        if (format.Length == 0)
            return position;

        return TryWriteBase(position, [ConsoleItem.Parse(string.Format(format, arg0, arg1, arg2))]);
    }

    /// <summary>
    /// Writes the text representation of the specified object directly to a position
    /// using the specified format information.
    /// Text can be colored using a simple <see href="https://github.com/chubrik/XConsole#coloring">microsyntax</see>.
    /// </summary>
    /// <param name="format">
    /// A composite format string.
    /// Text can be colored using a simple <see href="https://github.com/chubrik/XConsole#coloring">microsyntax</see>.
    /// </param>
    /// <inheritdoc cref="Console.Write(string, object?[])"/>
    /// <inheritdoc cref="TryWrite(ConsolePosition, string?)"/>
    public static ConsolePosition? TryWrite(this ConsolePosition position,
#if NET7_0_OR_GREATER
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
        string format, params object?[]? arg)
    {
        if (format.Length == 0)
            return position;

        return TryWriteBase(position, [ConsoleItem.Parse(string.Format(format, arg ?? []))]);
    }

    /// <summary>
    /// Writes the text representation of the specified 32-bit unsigned integer value directly to a position.
    /// </summary>
    /// <inheritdoc cref="Console.Write(uint)"/>
    /// <inheritdoc cref="TryWrite(ConsolePosition, string?)"/>
    public static ConsolePosition? TryWrite(this ConsolePosition position, uint value)
    {
        return TryWriteBase(position, [new ConsoleItem(value.ToString())]);
    }

    /// <summary>
    /// Writes the text representation of the specified 64-bit unsigned integer value directly to a position.
    /// </summary>
    /// <inheritdoc cref="Console.Write(ulong)"/>
    /// <inheritdoc cref="TryWrite(ConsolePosition, string?)"/>
    public static ConsolePosition? TryWrite(this ConsolePosition position, ulong value)
    {
        return TryWriteBase(position, [new ConsoleItem(value.ToString())]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ConsolePosition? TryWriteBase(ConsolePosition position, ConsoleItem[] items)
    {
        try
        {
            return XConsole.WriteToPosition(position, items);
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }
    }

    #endregion
}
