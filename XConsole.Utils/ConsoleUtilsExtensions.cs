namespace Chubrik.XConsole.Utils;

using System;
using System.Threading;

#if NET
using System.Runtime.Versioning;
using System.Runtime.InteropServices;
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
        this ConsoleUtils _, string message = "W`Continue? [Y/n]: ", string yes = "G`Yes", string no = "R`No")
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

    #region Window resize

#if NET
    // https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-showwindow

    private const int SW_HIDE = 0;
    private const int SW_MAXIMIZE = 3;
    private const int SW_MINIMIZE = 6;
    private const int SW_RESTORE = 9;

    [DllImport("kernel32.dll", ExactSpelling = true)]
    private static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    private static readonly IntPtr _consolePtr = GetConsoleWindow();

    public static void HideWindow(this ConsoleUtils _) => ShowWindow(_consolePtr, SW_HIDE);
    public static void MaximizeWindow(this ConsoleUtils _) => ShowWindow(_consolePtr, SW_MAXIMIZE);
    public static void MinimizeWindow(this ConsoleUtils _) => ShowWindow(_consolePtr, SW_MINIMIZE);
    public static void RestoreWindow(this ConsoleUtils _) => ShowWindow(_consolePtr, SW_RESTORE);
#endif

    #endregion
}
