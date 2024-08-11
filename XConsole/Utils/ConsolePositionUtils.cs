namespace Chubrik.XConsole;

using System.Threading;
#if NET
using System.Runtime.Versioning;
#endif

/// <summary>
/// The most useful extensions for <see cref="ConsolePosition"/>.
/// </summary>
#if NET
[UnsupportedOSPlatform("android")]
[UnsupportedOSPlatform("browser")]
[UnsupportedOSPlatform("ios")]
[UnsupportedOSPlatform("tvos")]
#endif
public static class ConsolePositionUtils
{
    /// <summary>
    /// Starts an ellipsis animation at the cpecified position.
    /// </summary>
    public static IConsoleAnimation AnimateEllipsis(this ConsolePosition position)
    {
        return new EllipsisAnimation(position, cancellationToken: null);
    }

    /// <inheritdoc cref="AnimateEllipsis(ConsolePosition)"/>
    public static IConsoleAnimation AnimateEllipsis(this ConsolePosition position, CancellationToken cancellationToken)
    {
        return new EllipsisAnimation(position, cancellationToken);
    }

    /// <summary>
    /// Starts a spinner animation at the cpecified position.
    /// </summary>
    public static IConsoleAnimation AnimateSpinner(this ConsolePosition position)
    {
        return new SpinnerAnimation(position, cancellationToken: null);
    }

    /// <inheritdoc cref="AnimateSpinner(ConsolePosition)"/>
    public static IConsoleAnimation AnimateSpinner(this ConsolePosition position, CancellationToken cancellationToken)
    {
        return new SpinnerAnimation(position, cancellationToken);
    }
}
