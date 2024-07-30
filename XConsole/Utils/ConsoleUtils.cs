#pragma warning disable CA1822 // Mark members as static

namespace Chubrik.XConsole;

using System;
using System.Runtime.InteropServices;
using System.Threading;
#if NET
using System.Runtime.Versioning;
#endif

/// <summary>
/// Instance for built-in and custom extensions.
/// </summary>
#if NET7_0_OR_GREATER
public sealed partial class ConsoleUtils
#else
public sealed class ConsoleUtils
#endif
{
    internal ConsoleUtils() { }

    #region Animations

    /// <summary>
    /// Starts an ellipsis animation at the current position.
    /// </summary>
#if NET
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
#endif
    public IConsoleAnimation AnimateEllipsis()
    {
        return new EllipsisAnimation(XConsole.CursorPosition, cancellationToken: null);
    }

    /// <inheritdoc cref="AnimateEllipsis()"/>
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

    /// <summary>
    /// Starts a spinner animation at the current position.
    /// </summary>
#if NET
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
#endif
    public IConsoleAnimation AnimateSpinner()
    {
        return new SpinnerAnimation(XConsole.CursorPosition, cancellationToken: null);
    }

    /// <inheritdoc cref="AnimateSpinner()"/>
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

    /// <summary>
    /// Displays the <paramref name="message"/> and waits until the user presses Y or N and then Enter.
    /// </summary>
    /// <returns>True or False according to the user’s decision.</returns>
#if NET
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
#endif
    public bool Confirm(string message = "Continue? [y/n]: ", string yes = "Yes", string no = "No")
    {
        var yesItem = ConsoleItem.Parse(yes);
        var noItem = ConsoleItem.Parse(no);
        var yesLength = yesItem.Value.Length;
        var noLength = noItem.Value.Length;

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
                                answerPosition.Write(new string(' ', noLength));

                            answer = true;
                            XConsole.CursorPosition = XConsole.WriteToPosition(answerPosition, [yesItem]);
                        }
                        continue;

                    case ConsoleKey.N:
                        if (answer != false)
                        {
                            if (answer == true)
                                answerPosition.Write(new string(' ', yesLength));

                            answer = false;
                            XConsole.CursorPosition = XConsole.WriteToPosition(answerPosition, [noItem]);
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
                            answerPosition.Write(new string(' ', Math.Max(yesLength, noLength)));
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

    /// <summary>
    /// Hides the console window.
    /// </summary>
    [SupportedOSPlatform("windows")]
    public void HideWindow() => ShowWindow(_consolePtr, SW_HIDE);

    /// <summary>
    /// Maximizes the console window.
    /// </summary>
    [SupportedOSPlatform("windows")]
    public void MaximizeWindow() => ShowWindow(_consolePtr, SW_MAXIMIZE);

    /// <summary>
    /// Minimizes the console window.
    /// </summary>
    [SupportedOSPlatform("windows")]
    public void MinimizeWindow() => ShowWindow(_consolePtr, SW_MINIMIZE);

    /// <summary>
    /// Restores the console window after minimization.
    /// </summary>
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
