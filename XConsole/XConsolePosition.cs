namespace Chubrik.XConsole;

using System;

#if NET
using System.Runtime.Versioning;
[SupportedOSPlatform("windows")]
[UnsupportedOSPlatform("android")]
[UnsupportedOSPlatform("browser")]
[UnsupportedOSPlatform("ios")]
[UnsupportedOSPlatform("tvos")]
#endif
public readonly struct XConsolePosition
{
    public int Left { get; }
    public int InitialTop { get; }
    internal long ShiftTop { get; }

    internal XConsolePosition(int left, int top, long shiftTop)
    {
        Left = left;
        InitialTop = top;
        ShiftTop = shiftTop;
    }

    public XConsolePosition(int left, int top)
    {
        Left = left;
        InitialTop = top;
        ShiftTop = XConsole.ShiftTop;
    }

    [Obsolete("Position arguments should be specified", error: true)]
    public XConsolePosition() => throw new InvalidOperationException();

    public int? ActualTop => XConsole.GetPositionActualTop(this);

    [Obsolete("At least one argument should be specified", error: true)]
    public void Write() => throw new InvalidOperationException();

    public XConsolePosition Write(params string?[] values)
    {
        return XConsole.WriteToPosition(this, values);
    }

    [Obsolete("At least one argument should be specified", error: true)]
    public void TryWrite() => throw new InvalidOperationException();

    public XConsolePosition? TryWrite(params string?[] values)
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
