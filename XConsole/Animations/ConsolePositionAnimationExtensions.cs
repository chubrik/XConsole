namespace Chubrik.XConsole;

using System.Threading;
using System.Runtime.Versioning;

/// <summary>
/// Animation extensions for <see cref="ConsolePosition"/>.
/// </summary>
[UnsupportedOSPlatform("android")]
[UnsupportedOSPlatform("browser")]
[UnsupportedOSPlatform("ios")]
[UnsupportedOSPlatform("tvos")]
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
