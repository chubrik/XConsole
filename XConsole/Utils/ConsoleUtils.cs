namespace Chubrik.XConsole;

using System;
using System.Runtime.InteropServices;
using System.Threading;

#if NET
using System.Runtime.Versioning;
[SupportedOSPlatform("windows")]
[UnsupportedOSPlatform("android")]
[UnsupportedOSPlatform("browser")]
[UnsupportedOSPlatform("ios")]
[UnsupportedOSPlatform("tvos")]
#endif
public sealed class ConsoleUtils
{
    internal ConsoleUtils() { }

    #region Animations

    public IConsoleAnimation AnimateEllipsis()
    {
        return new EllipsisAnimation(XConsole.CursorPosition);
    }

    public IConsoleAnimation AnimateEllipsis(CancellationToken cancellationToken)
    {
        return new EllipsisAnimation(XConsole.CursorPosition, cancellationToken);
    }

    public IConsoleAnimation AnimateSpinner()
    {
        return new SpinnerAnimation(XConsole.CursorPosition);
    }

    public IConsoleAnimation AnimateSpinner(CancellationToken cancellationToken)
    {
        return new SpinnerAnimation(XConsole.CursorPosition, cancellationToken);
    }

    #endregion

    public bool Confirm(string message = "W`Continue? [Y/n]: ", string yes = "G`Yes", string no = "R`No")
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

    public void HideWindow() => ShowWindow(_consolePtr, SW_HIDE);
    public void MaximizeWindow() => ShowWindow(_consolePtr, SW_MAXIMIZE);
    public void MinimizeWindow() => ShowWindow(_consolePtr, SW_MINIMIZE);
    public void RestoreWindow() => ShowWindow(_consolePtr, SW_RESTORE);

    [DllImport("kernel32.dll", ExactSpelling = true)]
    private static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
#endif

    #endregion
}
