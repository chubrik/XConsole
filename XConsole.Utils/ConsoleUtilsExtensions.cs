namespace Chubrik.XConsole.Utils;

using System.Threading;

#if NET
using System.Runtime.Versioning;
[SupportedOSPlatform("windows")]
[UnsupportedOSPlatform("android")]
[UnsupportedOSPlatform("browser")]
[UnsupportedOSPlatform("ios")]
[UnsupportedOSPlatform("tvos")]
#endif
public static class ConsoleUtilsExtensions
{
    public static IConsoleAnimation AnimateEllipsis(this ConsoleUtils _)
    {
        return XConsole.CursorPosition.AnimateEllipsis();
    }

    public static IConsoleAnimation AnimateEllipsis(this ConsoleUtils _, CancellationToken cancellationToken)
    {
        return XConsole.CursorPosition.AnimateEllipsis(cancellationToken);
    }

    public static IConsoleAnimation AnimateSpinner(this ConsoleUtils _)
    {
        return XConsole.CursorPosition.AnimateSpinner();
    }

    public static IConsoleAnimation AnimateSpinner(this ConsoleUtils _, CancellationToken cancellationToken)
    {
        return XConsole.CursorPosition.AnimateSpinner(cancellationToken);
    }
}
