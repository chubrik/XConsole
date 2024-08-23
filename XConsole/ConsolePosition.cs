namespace Chubrik.XConsole;

using System;
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
/// See also these extension methods that write text directly to a position
/// and return the new <see cref="ConsolePosition"/> structure:
/// <br/>• <seealso cref="ConsolePositionExtensions.Write(ConsolePosition, string?)"/>
/// <br/>• <seealso cref="ConsolePositionExtensions.TryWrite(ConsolePosition, string?)"/>
/// </remarks>
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
}
