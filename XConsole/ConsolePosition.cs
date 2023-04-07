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
/// Smart structure, resistant to console buffer overflow
/// and always points to the correct position within the console buffer area.
/// It has own set of <see cref="Write()"/> and <see cref="TryWrite()"/> methods
/// that write text directly to a position and return the new <see cref="ConsolePosition"/> structure.
/// <para>
/// Provides three properties:
/// <br/>&#8226; <see cref="Left"/> &#8212; the column position within the console buffer area.
/// <br/>&#8226; <see cref="InitialTop"/> &#8212; the row position within the console buffer area at the initial moment.
/// <br/>&#8226; <see cref="ActualTop"/> &#8212; the current row position within the console buffer area,
/// considering the buffer overflow.
/// </para>
/// </summary>
#if NET
[UnsupportedOSPlatform("android")]
[UnsupportedOSPlatform("browser")]
[UnsupportedOSPlatform("ios")]
[UnsupportedOSPlatform("tvos")]
#endif
public readonly struct ConsolePosition
{
    /// <summary>
    /// The column position within the console buffer area.
    /// </summary>
    public int Left { get; }

    /// <summary>
    /// The row position within the console buffer area at the initial moment.
    /// </summary>
    public int InitialTop { get; }

    internal long ShiftTop { get; }

    /// <summary>
    /// The current row position within the console buffer area, considering the buffer overflow.
    /// </summary>
    public int? ActualTop => XConsole.GetPositionActualTop(this);

    internal ConsolePosition(int left, int top, long shiftTop)
    {
        Left = left;
        InitialTop = top;
        ShiftTop = shiftTop;
    }

    /// <summary>
    /// Constructs the position within the console buffer area.
    /// </summary>
    /// <param name="left">The column position. Columns are numbered from left to right starting at 0.</param>
    /// <param name="top">The row position. Rows are numbered from top to bottom starting at 0.</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public ConsolePosition(int left, int top)
    {
        if (left < 0)
            throw new ArgumentOutOfRangeException(nameof(left), "Non-negative number required.");

        if (top < 0)
            throw new ArgumentOutOfRangeException(nameof(top), "Non-negative number required.");

        Left = left;
        InitialTop = top;
        ShiftTop = XConsole.ShiftTop;
    }

    /// <summary>
    /// <paramref name="Left"/> and <paramref name="top"/> arguments should be specified.
    /// </summary>
    /// <remarks>
    /// Please, use constructor overload:
    /// <br/>&#8226; <see cref="ConsolePosition(int, int)"/>
    /// </remarks>
    /// <exception cref="InvalidOperationException"></exception>
    [Obsolete("Arguments should be specified.", error: true)]
    public ConsolePosition() => throw new InvalidOperationException();

    #region Write

    /// <inheritdoc cref="TryWrite()"/>
    [Obsolete("At least one argument should be specified.", error: true)]
    public void Write() => throw new InvalidOperationException();

    /// <returns>The new end <see cref="ConsolePosition"/> structure.</returns>
    /// <exception cref="ArgumentOutOfRangeException"/>
    /// <inheritdoc cref="TryWrite(string?)"/>
    public ConsolePosition Write(string? value)
    {
        return XConsole.WriteToPosition(this, new[] { ConsoleItem.Parse(value) });
    }

    /// <returns>The new end <see cref="ConsolePosition"/> structure.</returns>
    /// <exception cref="ArgumentOutOfRangeException"/>
    /// <inheritdoc cref="TryWrite(string?[])"/>
    public ConsolePosition Write(params string?[] values)
    {
        Debug.Assert(values.Length > 0);
        var items = new ConsoleItem[values.Length];

        for (var i = 0; i < values.Length; i++)
            items[i] = ConsoleItem.Parse(values[i]);

        return XConsole.WriteToPosition(this, items);
    }

    /// <inheritdoc cref="Write(string?[])"/>
    public ConsolePosition Write(IReadOnlyList<string?> values)
    {
        var items = new ConsoleItem[values.Count];

        for (var i = 0; i < values.Count; i++)
            items[i] = ConsoleItem.Parse(values[i]);

        return XConsole.WriteToPosition(this, items);
    }

    /// <returns>The new end <see cref="ConsolePosition"/> structure.</returns>
    /// <exception cref="ArgumentOutOfRangeException"/>
    /// <inheritdoc cref="TryWrite(bool)"/>
    public ConsolePosition Write(bool value)
    {
        return XConsole.WriteToPosition(this, new[] { new ConsoleItem(value.ToString()) });
    }

    /// <returns>The new end <see cref="ConsolePosition"/> structure.</returns>
    /// <exception cref="ArgumentOutOfRangeException"/>
    /// <inheritdoc cref="TryWrite(char)"/>
    public ConsolePosition Write(char value)
    {
        return XConsole.WriteToPosition(this, new[] { new ConsoleItem(value.ToString()) });
    }

    /// <returns>The new end <see cref="ConsolePosition"/> structure.</returns>
    /// <exception cref="ArgumentOutOfRangeException"/>
    /// <inheritdoc cref="TryWrite(char[]?)"/>
    public ConsolePosition Write(char[]? buffer)
    {
        if (buffer == null || buffer.Length == 0)
            return this;

        return XConsole.WriteToPosition(this, new[] { new ConsoleItem(new string(buffer)) });
    }

    /// <returns>The new end <see cref="ConsolePosition"/> structure.</returns>
    /// <exception cref="ArgumentOutOfRangeException"/>
    /// <inheritdoc cref="TryWrite(char[], int, int)"/>
    public ConsolePosition Write(char[] buffer, int index, int count)
    {
        if (count == 0)
            return this;

        return XConsole.WriteToPosition(this, new[] { new ConsoleItem(new string(buffer, index, count)) });
    }

    /// <returns>The new end <see cref="ConsolePosition"/> structure.</returns>
    /// <exception cref="ArgumentOutOfRangeException"/>
    /// <inheritdoc cref="TryWrite(decimal)"/>
    public ConsolePosition Write(decimal value)
    {
        return XConsole.WriteToPosition(this, new[] { new ConsoleItem(value.ToString()) });
    }

    /// <returns>The new end <see cref="ConsolePosition"/> structure.</returns>
    /// <exception cref="ArgumentOutOfRangeException"/>
    /// <inheritdoc cref="TryWrite(double)"/>
    public ConsolePosition Write(double value)
    {
        return XConsole.WriteToPosition(this, new[] { new ConsoleItem(value.ToString()) });
    }

    /// <returns>The new end <see cref="ConsolePosition"/> structure.</returns>
    /// <exception cref="ArgumentOutOfRangeException"/>
    /// <inheritdoc cref="TryWrite(int)"/>
    public ConsolePosition Write(int value)
    {
        return XConsole.WriteToPosition(this, new[] { new ConsoleItem(value.ToString()) });
    }

    /// <returns>The new end <see cref="ConsolePosition"/> structure.</returns>
    /// <exception cref="ArgumentOutOfRangeException"/>
    /// <inheritdoc cref="TryWrite(long)"/>
    public ConsolePosition Write(long value)
    {
        return XConsole.WriteToPosition(this, new[] { new ConsoleItem(value.ToString()) });
    }

    /// <returns>The new end <see cref="ConsolePosition"/> structure.</returns>
    /// <exception cref="ArgumentOutOfRangeException"/>
    /// <inheritdoc cref="TryWrite(object?)"/>
    public ConsolePosition Write(object? value)
    {
        var valueStr = value?.ToString();

        if (string.IsNullOrEmpty(valueStr))
            return this;

        return XConsole.WriteToPosition(this, new[] { new ConsoleItem(valueStr) });
    }

    /// <returns>The new end <see cref="ConsolePosition"/> structure.</returns>
    /// <exception cref="ArgumentOutOfRangeException"/>
    /// <inheritdoc cref="TryWrite(float)"/>
    public ConsolePosition Write(float value)
    {
        return XConsole.WriteToPosition(this, new[] { new ConsoleItem(value.ToString()) });
    }

    /// <returns>The new end <see cref="ConsolePosition"/> structure.</returns>
    /// <exception cref="ArgumentOutOfRangeException"/>
    /// <inheritdoc cref="TryWrite(string, object?)"/>
    public ConsolePosition Write(
#if NET7_0_OR_GREATER
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
        string format, object? arg0)
    {
        if (format.Length == 0)
            return this;

        return XConsole.WriteToPosition(this, new[] { ConsoleItem.Parse(string.Format(format, arg0)) });
    }

    /// <returns>The new end <see cref="ConsolePosition"/> structure.</returns>
    /// <exception cref="ArgumentOutOfRangeException"/>
    /// <inheritdoc cref="TryWrite(string, object?, object?)"/>
    public ConsolePosition Write(
#if NET7_0_OR_GREATER
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
        string format, object? arg0, object? arg1)
    {
        if (format.Length == 0)
            return this;

        return XConsole.WriteToPosition(this, new[] { ConsoleItem.Parse(string.Format(format, arg0, arg1)) });
    }

    /// <returns>The new end <see cref="ConsolePosition"/> structure.</returns>
    /// <exception cref="ArgumentOutOfRangeException"/>
    /// <inheritdoc cref="TryWrite(string, object?, object?, object?)"/>
    public ConsolePosition Write(
#if NET7_0_OR_GREATER
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
        string format, object? arg0, object? arg1, object? arg2)
    {
        if (format.Length == 0)
            return this;

        return XConsole.WriteToPosition(this, new[] { ConsoleItem.Parse(string.Format(format, arg0, arg1, arg2)) });
    }

    /// <returns>The new end <see cref="ConsolePosition"/> structure.</returns>
    /// <exception cref="ArgumentOutOfRangeException"/>
    /// <inheritdoc cref="TryWrite(string, object?[]?)"/>
    public ConsolePosition Write(
#if NET7_0_OR_GREATER
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
        string format, params object?[]? arg)
    {
        if (format.Length == 0)
            return this;

        return XConsole.WriteToPosition(
            this, new[] { ConsoleItem.Parse(string.Format(format, arg ?? Array.Empty<object?>())) });
    }

    /// <returns>The new end <see cref="ConsolePosition"/> structure.</returns>
    /// <exception cref="ArgumentOutOfRangeException"/>
    /// <inheritdoc cref="TryWrite(uint)"/>
    public ConsolePosition Write(uint value)
    {
        return XConsole.WriteToPosition(this, new[] { new ConsoleItem(value.ToString()) });
    }

    /// <returns>The new end <see cref="ConsolePosition"/> structure.</returns>
    /// <exception cref="ArgumentOutOfRangeException"/>
    /// <inheritdoc cref="TryWrite(ulong)"/>
    public ConsolePosition Write(ulong value)
    {
        return XConsole.WriteToPosition(this, new[] { new ConsoleItem(value.ToString()) });
    }

    #endregion

    #region TryWrite

    /// <inheritdoc cref="XConsole.Write()"/>
    [Obsolete("At least one argument should be specified.", error: true)]
    public void TryWrite() => throw new InvalidOperationException();

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
    public ConsolePosition? TryWrite(string? value)
    {
        return TryWriteBase(new[] { ConsoleItem.Parse(value) });
    }

    /// <summary>
    /// Wtites the specified set of string values directly to a position.
    /// Text can be colored using a simple <see href="https://github.com/chubrik/XConsole#coloring">microsyntax</see>.
    /// </summary>
    /// <param name="values">
    /// The set of values to write.
    /// Text can be colored using a simple <see href="https://github.com/chubrik/XConsole#coloring">microsyntax</see>.
    /// </param>
    /// <inheritdoc cref="TryWrite(string?)"/>
    public ConsolePosition? TryWrite(params string?[] values)
    {
        Debug.Assert(values.Length > 0);
        var items = new ConsoleItem[values.Length];

        for (var i = 0; i < values.Length; i++)
            items[i] = ConsoleItem.Parse(values[i]);

        return TryWriteBase(items);
    }

    /// <inheritdoc cref="TryWrite(string?[])"/>
    public ConsolePosition? TryWrite(IReadOnlyList<string?> values)
    {
        var items = new ConsoleItem[values.Count];

        for (var i = 0; i < values.Count; i++)
            items[i] = ConsoleItem.Parse(values[i]);

        return TryWriteBase(items);
    }

    /// <summary>
    /// Writes the text representation of the specified Boolean value directly to a position.
    /// </summary>
    /// <inheritdoc cref="Console.Write(bool)"/>
    /// <inheritdoc cref="TryWrite(string?)"/>
    public ConsolePosition? TryWrite(bool value)
    {
        return TryWriteBase(new[] { new ConsoleItem(value.ToString()) });
    }

    /// <summary>
    /// Writes the specified Unicode character value directly to a position.
    /// </summary>
    /// <inheritdoc cref="Console.Write(char)"/>
    /// <inheritdoc cref="TryWrite(string?)"/>
    public ConsolePosition? TryWrite(char value)
    {
        return TryWriteBase(new[] { new ConsoleItem(value.ToString()) });
    }

    /// <summary>
    /// Writes the specified array of Unicode characters directly to a position.
    /// </summary>
    /// <inheritdoc cref="Console.Write(char[]?)"/>
    /// <inheritdoc cref="TryWrite(string?)"/>
    public ConsolePosition? TryWrite(char[]? buffer)
    {
        if (buffer == null || buffer.Length == 0)
            return this;

        return TryWriteBase(new[] { new ConsoleItem(new string(buffer)) });
    }

    /// <summary>
    /// Writes the specified subarray of Unicode characters directly to a position.
    /// </summary>
    /// <inheritdoc cref="Console.Write(char[], int, int)"/>
    /// <inheritdoc cref="TryWrite(string?)"/>
    public ConsolePosition? TryWrite(char[] buffer, int index, int count)
    {
        if (count == 0)
            return this;

        return TryWriteBase(new[] { new ConsoleItem(new string(buffer, index, count)) });
    }

    /// <summary>
    /// Writes the text representation of the specified <see cref="decimal"/> value directly to a position.
    /// </summary>
    /// <inheritdoc cref="Console.Write(decimal)"/>
    /// <inheritdoc cref="TryWrite(string?)"/>
    public ConsolePosition? TryWrite(decimal value)
    {
        return TryWriteBase(new[] { new ConsoleItem(value.ToString()) });
    }

    /// <summary>
    /// Writes the text representation of the specified double-precision floating-point value directly to a position.
    /// </summary>
    /// <inheritdoc cref="Console.Write(double)"/>
    /// <inheritdoc cref="TryWrite(string?)"/>
    public ConsolePosition? TryWrite(double value)
    {
        return TryWriteBase(new[] { new ConsoleItem(value.ToString()) });
    }

    /// <summary>
    /// Writes the text representation of the specified 32-bit signed integer value directly to a position.
    /// </summary>
    /// <inheritdoc cref="Console.Write(int)"/>
    /// <inheritdoc cref="TryWrite(string?)"/>
    public ConsolePosition? TryWrite(int value)
    {
        return TryWriteBase(new[] { new ConsoleItem(value.ToString()) });
    }

    /// <summary>
    /// Writes the text representation of the specified 64-bit signed integer value directly to a position.
    /// </summary>
    /// <inheritdoc cref="Console.Write(long)"/>
    /// <inheritdoc cref="TryWrite(string?)"/>
    public ConsolePosition? TryWrite(long value)
    {
        return TryWriteBase(new[] { new ConsoleItem(value.ToString()) });
    }

    /// <summary>
    /// Writes the text representation of the specified object directly to a position.
    /// </summary>
    /// <inheritdoc cref="Console.Write(object?)"/>
    /// <inheritdoc cref="TryWrite(string?)"/>
    public ConsolePosition? TryWrite(object? value)
    {
        var valueStr = value?.ToString();

        if (string.IsNullOrEmpty(valueStr))
            return this;

        return TryWriteBase(new[] { new ConsoleItem(valueStr) });
    }

    /// <summary>
    /// Writes the text representation of the specified single-precision floating-point value directly to a position.
    /// </summary>
    /// <inheritdoc cref="Console.Write(float)"/>
    /// <inheritdoc cref="TryWrite(string?)"/>
    public ConsolePosition? TryWrite(float value)
    {
        return TryWriteBase(new[] { new ConsoleItem(value.ToString()) });
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
    /// <inheritdoc cref="TryWrite(string?)"/>
    public ConsolePosition? TryWrite(
#if NET7_0_OR_GREATER
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
        string format, object? arg0)
    {
        if (format.Length == 0)
            return this;

        return TryWriteBase(new[] { ConsoleItem.Parse(string.Format(format, arg0)) });
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
    /// <inheritdoc cref="TryWrite(string?)"/>
    public ConsolePosition? TryWrite(
#if NET7_0_OR_GREATER
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
        string format, object? arg0, object? arg1)
    {
        if (format.Length == 0)
            return this;

        return TryWriteBase(new[] { ConsoleItem.Parse(string.Format(format, arg0, arg1)) });
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
    /// <inheritdoc cref="TryWrite(string?)"/>
    public ConsolePosition? TryWrite(
#if NET7_0_OR_GREATER
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
        string format, object? arg0, object? arg1, object? arg2)
    {
        if (format.Length == 0)
            return this;

        return TryWriteBase(new[] { ConsoleItem.Parse(string.Format(format, arg0, arg1, arg2)) });
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
    /// <inheritdoc cref="Console.Write(string, object?[]?)"/>
    /// <inheritdoc cref="TryWrite(string?)"/>
    public ConsolePosition? TryWrite(
#if NET7_0_OR_GREATER
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
        string format, params object?[]? arg)
    {
        if (format.Length == 0)
            return this;

        return TryWriteBase(new[] { ConsoleItem.Parse(string.Format(format, arg ?? Array.Empty<object?>())) });
    }

    /// <summary>
    /// Writes the text representation of the specified 32-bit unsigned integer value directly to a position.
    /// </summary>
    /// <inheritdoc cref="Console.Write(uint)"/>
    /// <inheritdoc cref="TryWrite(string?)"/>
    public ConsolePosition? TryWrite(uint value)
    {
        return TryWriteBase(new[] { new ConsoleItem(value.ToString()) });
    }

    /// <summary>
    /// Writes the text representation of the specified 64-bit unsigned integer value directly to a position.
    /// </summary>
    /// <inheritdoc cref="Console.Write(ulong)"/>
    /// <inheritdoc cref="TryWrite(string?)"/>
    public ConsolePosition? TryWrite(ulong value)
    {
        return TryWriteBase(new[] { new ConsoleItem(value.ToString()) });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ConsolePosition? TryWriteBase(ConsoleItem[] items)
    {
        try
        {
            return XConsole.WriteToPosition(this, items);
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }
    }

    #endregion
}
