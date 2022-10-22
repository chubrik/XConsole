namespace Chubrik.Console;

using System;
using System.Collections.Generic;
using System.Diagnostics;

#if NET
using System.Runtime.Versioning;
[SupportedOSPlatform("windows")]
[UnsupportedOSPlatform("android")]
[UnsupportedOSPlatform("browser")]
[UnsupportedOSPlatform("ios")]
[UnsupportedOSPlatform("tvos")]
#endif
public readonly struct ConsolePosition
{
    public int Left { get; }
    public int InitialTop { get; }
    internal long ShiftTop { get; }

    internal ConsolePosition(int left, int top, long shiftTop)
    {
        Left = left;
        InitialTop = top;
        ShiftTop = shiftTop;
    }

    public ConsolePosition(int left, int top)
    {
        Left = left;
        InitialTop = top;
        ShiftTop = XConsole.ShiftTop;
    }

    [Obsolete("Arguments should be specified.", error: true)]
    public ConsolePosition() => throw new InvalidOperationException();

    public int? ActualTop => XConsole.GetPositionActualTop(this);

    [Obsolete("At least one argument should be specified.", error: true)]
    public void Write() => throw new InvalidOperationException();

    public ConsolePosition Write(params string?[] values)
    {
        Debug.Assert(values.Length > 0);
        return XConsole.WriteToPosition(this, values);
    }

    public ConsolePosition Write(IReadOnlyList<string?> values)
    {
        return XConsole.WriteToPosition(this, values);
    }

    [Obsolete("At least one argument should be specified.", error: true)]
    public void TryWrite() => throw new InvalidOperationException();

    public ConsolePosition? TryWrite(params string?[] values)
    {
        Debug.Assert(values.Length > 0);

        try
        {
            return XConsole.WriteToPosition(this, values);
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }
    }

    public ConsolePosition? TryWrite(IReadOnlyList<string?> values)
    {
        try
        {
            return XConsole.WriteToPosition(this, values);
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }
    }
}
