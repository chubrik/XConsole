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
[Obsolete("This struct is deprecated. Use ConsolePosition instead.")]
public readonly struct XConsolePosition
{
    private readonly ConsolePosition _position;

    public int Left => _position.Left;
    public int InitialTop => _position.InitialTop;
    internal long ShiftTop => _position.ShiftTop;

    internal XConsolePosition(int left, int top, long shiftTop)
    {
        _position = new ConsolePosition(left: left, top: top, shiftTop: shiftTop);
    }

    public XConsolePosition(int left, int top)
    {
        _position = new ConsolePosition(left: left, top: top);
    }

    [Obsolete("Arguments should be specified.", error: true)]
    public XConsolePosition() => throw new InvalidOperationException();

    public int? ActualTop => _position.ActualTop;

    [Obsolete("At least one argument should be specified.", error: true)]
    public void Write() => throw new InvalidOperationException();

    public XConsolePosition Write(params string?[] values)
    {
        return _position.Write(values);
    }

    [Obsolete("At least one argument should be specified.", error: true)]
    public void TryWrite() => throw new InvalidOperationException();

    public XConsolePosition? TryWrite(params string?[] values)
    {
        return _position.TryWrite(values);
    }

    public static implicit operator ConsolePosition(XConsolePosition position) =>
        new(left: position.Left, top: position.InitialTop, shiftTop: position.ShiftTop);

    public static implicit operator XConsolePosition(ConsolePosition position) =>
        new(left: position.Left, top: position.InitialTop, shiftTop: position.ShiftTop);
}
