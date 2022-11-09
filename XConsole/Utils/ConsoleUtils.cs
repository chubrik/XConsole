#pragma warning disable CA1822 // Mark members as static

namespace Chubrik.XConsole;

using System;
using System.Runtime.InteropServices;
using System.Threading;
#if NET
using System.Runtime.Versioning;
#endif

public sealed class ConsoleUtils
{
    internal ConsoleUtils() { }

    #region Animations

#if NET
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
#endif
    public IConsoleAnimation AnimateEllipsis()
    {
        return new EllipsisAnimation(XConsole.CursorPosition);
    }

#if NET
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
#endif
    public IConsoleAnimation AnimateEllipsis(CancellationToken cancellationToken)
    {
        return new EllipsisAnimation(XConsole.CursorPosition, cancellationToken);
    }

#if NET
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
#endif
    public IConsoleAnimation AnimateSpinner()
    {
        return new SpinnerAnimation(XConsole.CursorPosition);
    }

#if NET
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
#endif
    public IConsoleAnimation AnimateSpinner(CancellationToken cancellationToken)
    {
        return new SpinnerAnimation(XConsole.CursorPosition, cancellationToken);
    }

    #endregion

#if NET
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
#endif
    public bool Confirm(string message = "Continue? [Y/n]: ", string yes = "Yes", string no = "No")
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

    private static readonly IntPtr _consolePtr = GetConsoleWindow();

    [SupportedOSPlatform("windows")]
    public void HideWindow() => ShowWindow(_consolePtr, SW_HIDE);

    [SupportedOSPlatform("windows")]
    public void MaximizeWindow() => ShowWindow(_consolePtr, SW_MAXIMIZE);

    [SupportedOSPlatform("windows")]
    public void MinimizeWindow() => ShowWindow(_consolePtr, SW_MINIMIZE);

    [SupportedOSPlatform("windows")]
    public void RestoreWindow() => ShowWindow(_consolePtr, SW_RESTORE);

    [DllImport("kernel32.dll", ExactSpelling = true)]
    private static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

#endif
    #endregion
}
