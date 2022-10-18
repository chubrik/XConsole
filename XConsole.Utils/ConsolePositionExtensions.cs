namespace XConsole.Utils;

using System.Threading;

#if NET
using System.Runtime.Versioning;
[SupportedOSPlatform("windows")]
[UnsupportedOSPlatform("android")]
[UnsupportedOSPlatform("browser")]
[UnsupportedOSPlatform("ios")]
[UnsupportedOSPlatform("tvos")]
#endif

public static class ConsolePositionExtensions
{
    public static EllipsisAnimation StartEllipsisAnimation(
        this ConsolePosition position)
    {
        return new(position);
    }

    public static EllipsisAnimation StartEllipsisAnimation(
        this ConsolePosition position, CancellationToken cancellationToken)
    {
        return new(position, cancellationToken);
    }

    public static SpinnerAnimation StartSpinnerAnimation(
        this ConsolePosition position)
    {
        return new(position);
    }

    public static SpinnerAnimation StartSpinnerAnimation(
        this ConsolePosition position, CancellationToken cancellationToken)
    {
        return new(position, cancellationToken);
    }
}
