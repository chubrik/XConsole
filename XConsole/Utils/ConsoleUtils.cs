#pragma warning disable CA1822 // Mark members as static

namespace Chubrik.XConsole;

using System;
using System.Runtime.InteropServices;
using System.Threading;
#if NET
using System.Runtime.Versioning;
#endif

#if NET7_0_OR_GREATER
public sealed partial class ConsoleUtils
#else
public sealed class ConsoleUtils
#endif
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
    public bool Confirm(string message = "Continue? [y/n]: ", string yes = "Yes", string no = "No")
    {
        return XConsole.Sync(() =>
        {
            bool? answer = null;
            var answerPosition = XConsole.Write(message).End;

            for (; ; )
            {
                var key = Console.ReadKey(intercept: true).Key;

                switch (key)
                {
                    case ConsoleKey.Y:
                        if (answer != true)
                        {
                            if (answer == false)
                                answerPosition.Write(new string(' ', no.Length));

                            answer = true;
                            XConsole.CursorPosition = answerPosition.Write(yes);
                        }
                        continue;

                    case ConsoleKey.N:
                        if (answer != false)
                        {
                            if (answer == true)
                                answerPosition.Write(new string(' ', yes.Length));

                            answer = false;
                            XConsole.CursorPosition = answerPosition.Write(no);
                        }
                        continue;

                    case ConsoleKey.Enter:
                        if (answer != null)
                        {
                            XConsole.WriteLine();
                            return answer.Value;
                        }
                        continue;

                    case ConsoleKey.Backspace:
                    case ConsoleKey.Escape:
                        if (answer != null)
                        {
                            answer = null;
                            answerPosition.Write(new string(' ', Math.Max(yes.Length, no.Length)));
                            XConsole.CursorPosition = answerPosition;
                        }
                        continue;

                    default:
                        continue;
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

#if NET7_0_OR_GREATER
    [LibraryImport("kernel32.dll")]
    private static partial IntPtr GetConsoleWindow();

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool ShowWindow(IntPtr hWnd, int nCmdShow);
#else
    [DllImport("kernel32.dll", ExactSpelling = true)]
    private static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
#endif

#endif
    #endregion
}
