namespace Chubrik.XConsole;

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
    public static IConsoleAnimation AnimateEllipsis(this ConsolePosition position)
    {
        return new EllipsisAnimation(position);
    }

    public static IConsoleAnimation AnimateEllipsis(this ConsolePosition position, CancellationToken cancellationToken)
    {
        return new EllipsisAnimation(position, cancellationToken);
    }

    public static IConsoleAnimation AnimateSpinner(this ConsolePosition position)
    {
        return new SpinnerAnimation(position);
    }

    public static IConsoleAnimation AnimateSpinner(this ConsolePosition position, CancellationToken cancellationToken)
    {
        return new SpinnerAnimation(position, cancellationToken);
    }
}
