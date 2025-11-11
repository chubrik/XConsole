namespace Chubrik.XConsole;

using System.Threading;
#if NET
using System.Runtime.Versioning;
#endif

/// <summary>
/// Animation extensions for <see cref="ConsoleExtras"/>.
/// </summary>
#if NET
[UnsupportedOSPlatform("android")]
[UnsupportedOSPlatform("browser")]
[UnsupportedOSPlatform("ios")]
[UnsupportedOSPlatform("tvos")]
#endif
public static class ConsoleAnimationExtensions
{
    /// <summary>
    /// Starts an ellipsis animation at the current position.
    /// </summary>
    public static IConsoleAnimation AnimateEllipsis(
        this ConsoleExtras _, CancellationToken? cancellationToken = null)
    {
        return XConsole.CursorPosition.AnimateEllipsis(cancellationToken);
    }

    /// <summary>
    /// Starts a spinner animation at the current position.
    /// </summary>
    public static IConsoleAnimation AnimateSpinner(
        this ConsoleExtras _, CancellationToken? cancellationToken = null)
    {
        return XConsole.CursorPosition.AnimateSpinner(cancellationToken);
    }
}
