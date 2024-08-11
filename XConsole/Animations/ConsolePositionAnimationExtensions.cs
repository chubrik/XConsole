namespace Chubrik.XConsole;

using System.Threading;
#if NET
using System.Runtime.Versioning;
#endif

/// <summary>
/// Animation extensions for <see cref="ConsolePosition"/>.
/// </summary>
#if NET
[UnsupportedOSPlatform("android")]
[UnsupportedOSPlatform("browser")]
[UnsupportedOSPlatform("ios")]
[UnsupportedOSPlatform("tvos")]
#endif
public static class ConsolePositionAnimationExtensions
{
    /// <summary>
    /// Starts an ellipsis animation at the specified position.
    /// </summary>
    public static IConsoleAnimation AnimateEllipsis(
        this ConsolePosition position, CancellationToken? cancellationToken = null)
    {
        return new EllipsisAnimation(position, cancellationToken);
    }

    /// <summary>
    /// Starts a spinner animation at the specified position.
    /// </summary>
    public static IConsoleAnimation AnimateSpinner(
        this ConsolePosition position, CancellationToken? cancellationToken = null)
    {
        return new SpinnerAnimation(position, cancellationToken);
    }
}
