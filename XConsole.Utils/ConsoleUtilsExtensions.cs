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
    #region Animations

    public static ConsoleAnimation AnimateEllipsis(this ConsoleUtils _)
    {
        return new EllipsisAnimation(XConsole.CursorPosition);
    }

    public static ConsoleAnimation AnimateEllipsis(this ConsoleUtils _, CancellationToken cancellationToken)
    {
        return new EllipsisAnimation(XConsole.CursorPosition, cancellationToken);
    }

    public static ConsoleAnimation AnimateSpinner(this ConsoleUtils _)
    {
        return new SpinnerAnimation(XConsole.CursorPosition);
    }

    public static ConsoleAnimation AnimateSpinner(this ConsoleUtils _, CancellationToken cancellationToken)
    {
        return new SpinnerAnimation(XConsole.CursorPosition, cancellationToken);
    }

    #endregion

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

    #region ReadLine

    public static string ReadLineHidden(this ConsoleUtils _) => ReadLineBase(maskChar: null);

    public static string ReadLineMasked(this ConsoleUtils _, char maskChar = '\u2022') => ReadLineBase(maskChar);

    private static string ReadLineBase(char? maskChar)
    {
        return XConsole.Sync(() =>
        {
            var text = string.Empty;
            ConsoleKeyInfo keyInfo;

            do
            {
                keyInfo = XConsole.ReadKey(intercept: true);

                if (keyInfo.Key == ConsoleKey.Backspace && text.Length > 0)
                {
                    if (maskChar != null)
                        XConsole.Write("\b \b");

                    text = text.Substring(0, text.Length - 1);
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    if (maskChar != null)
                        XConsole.Write(maskChar.Value);

                    text += keyInfo.KeyChar;
                }
            }
            while (keyInfo.Key != ConsoleKey.Enter);

            XConsole.WriteLine();
            return text;
        });
    }

    #endregion

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
