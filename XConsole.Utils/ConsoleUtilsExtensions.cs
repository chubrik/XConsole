namespace Chubrik.XConsole.Utils;

using System;
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
        return new EllipsisAnimation(XConsole.CursorPosition);
    }

    public static IConsoleAnimation AnimateEllipsis(this ConsoleUtils _, CancellationToken cancellationToken)
    {
        return new EllipsisAnimation(XConsole.CursorPosition, cancellationToken);
    }

    public static IConsoleAnimation AnimateSpinner(this ConsoleUtils _)
    {
        return new SpinnerAnimation(XConsole.CursorPosition);
    }

    public static IConsoleAnimation AnimateSpinner(this ConsoleUtils _, CancellationToken cancellationToken)
    {
        return new SpinnerAnimation(XConsole.CursorPosition, cancellationToken);
    }

    public static bool Confirm(
        this ConsoleUtils _, string message = "Continue? [Y/n]: ", string yes = "Yes", string no = "No")
    {
        return XConsole.Sync(() =>
        {
            XConsole.Write(message);

            for (; ; )
            {
                var key = Console.ReadKey(intercept: true).Key;

                if (key == ConsoleKey.Y)
                {
                    XConsole.WriteLine(yes);
                    return true;
                }
                else if (key == ConsoleKey.N || key == ConsoleKey.Escape)
                {
                    XConsole.WriteLine(no);
                    return false;
                }
            }
        });
    }
}
