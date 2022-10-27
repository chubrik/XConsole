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
public readonly struct ConsolePosition
{
    public int Left { get; }
    public int InitialTop { get; }
    internal long ShiftTop { get; }

    public int? ActualTop => XConsole.GetPositionActualTop(this);

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
}
