namespace Chubrik.XConsole;

using System;
using System.Collections.Generic;
#if NET
using System.Runtime.Versioning;
#endif

/// <summary>
/// Smart structure, resistant to console buffer overflow
/// and always points to the correct position within the console buffer area.
/// <para>
/// Provides three properties:
/// <br/>• <see cref="Left"/> – the column position within the console buffer area.
/// <br/>• <see cref="InitialTop"/> – the row position within the console buffer area at the initial moment.
/// <br/>• <see cref="ActualTop"/> – the current row position within the console buffer area,
/// considering the buffer overflow.
/// </para>
/// </summary>
/// <remarks>
/// See also these methods that write text directly to a position
/// and return the new <see cref="ConsolePosition"/> structure:
/// <br/>• <seealso cref="Write(string?)"/>
/// <br/>• <seealso cref="TryWrite(string?)"/>
/// </remarks>
#if NET
[UnsupportedOSPlatform("android")]
[UnsupportedOSPlatform("browser")]
[UnsupportedOSPlatform("ios")]
[UnsupportedOSPlatform("tvos")]
#endif
public readonly struct ConsolePosition
{
    #region Common

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

    /// <summary>
    /// Constructs the position within the console buffer area.
    /// </summary>
    /// <param name="left">The column position. Columns are numbered from left to right starting at 0.</param>
    /// <param name="top">The row position. Rows are numbered from top to bottom starting at 0.</param>
    /// <exception cref="ArgumentOutOfRangeException"/>
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

    internal ConsolePosition(int left, int top, long shiftTop)
    {
        Left = left;
        InitialTop = top;
        ShiftTop = shiftTop;
    }

#pragma warning disable CS1734 // XML comment has a paramref tag, but there is no parameter by that name
    /// <summary>
    /// <paramref name="Left"/> and <paramref name="top"/> arguments should be specified.
    /// </summary>
    /// <remarks>
    /// Please, use constructor overload:
    /// <br/>• <see cref="ConsolePosition(int, int)"/>
    /// </remarks>
    /// <exception cref="InvalidOperationException"/>
#pragma warning restore CS1734 // XML comment has a paramref tag, but there is no parameter by that name
    [Obsolete("Arguments should be specified.", error: true)]
    public ConsolePosition() => throw new InvalidOperationException();

    #endregion

    #region White methods

    /// <returns>The new end <see cref="ConsolePosition"/> structure.</returns>
    /// <exception cref="ArgumentOutOfRangeException"/>
    /// <inheritdoc cref="TryWrite(string?)"/>
    public ConsolePosition Write(string? value)
    {
        return XConsole.WriteToPosition(this, ConsoleItem.Parse(value), []);
    }

    /// <returns>The new end <see cref="ConsolePosition"/> structure.</returns>
    /// <exception cref="ArgumentOutOfRangeException"/>
    /// <inheritdoc cref="TryWrite(IReadOnlyList{string?})"/>
    public ConsolePosition Write(IReadOnlyList<string?> values)
    {
        var items = new ConsoleItem[values.Count];

        for (var i = 0; i < values.Count; i++)
            items[i] = ConsoleItem.Parse(values[i]);

        return XConsole.WriteToPosition(this, null, items);
    }

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
    /// or <see langword="null"/> if the current position actually points outside the console buffer area.
    /// </returns>
    /// <exception cref="System.Security.SecurityException"/>
    /// <exception cref="System.IO.IOException"/>
    public ConsolePosition? TryWrite(string? value)
    {
        try
        {
            return XConsole.WriteToPosition(this, ConsoleItem.Parse(value), []);
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }
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
    public ConsolePosition? TryWrite(IReadOnlyList<string?> values)
    {
        var items = new ConsoleItem[values.Count];

        for (var i = 0; i < values.Count; i++)
            items[i] = ConsoleItem.Parse(values[i]);

        try
        {
            return XConsole.WriteToPosition(this, null, items);
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }
    }

    #endregion
}
