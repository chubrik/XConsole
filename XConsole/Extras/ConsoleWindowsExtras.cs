#pragma warning disable IDE0060 // Remove unused parameter

namespace Chubrik.XConsole.Extras;

using System;
using System.Runtime.InteropServices;
#if NET
using System.Runtime.Versioning;
#endif

/// <summary>
/// The most useful Windows extensions for <see cref="ConsoleExtras"/>.
/// </summary>
#if NET
[SupportedOSPlatform("windows")]
#endif
#if NET7_0_OR_GREATER
public static partial class ConsoleWindowsExtras
#else
public static class ConsoleWindowsExtras
#endif
{
    private static readonly IntPtr _consolePtr = GetConsoleWindow();

#if NET7_0_OR_GREATER
    [LibraryImport("kernel32.dll")]
    private static partial IntPtr GetConsoleWindow();
#else
    [DllImport("kernel32.dll", ExactSpelling = true)]
    private static extern IntPtr GetConsoleWindow();
#endif

    #region Window resize

    // https://stackoverflow.com/questions/22053112

    /// <summary>
    /// Hides the console window.
    /// </summary>
    public static void HideWindow(this ConsoleExtras extras) => ShowWindow(_consolePtr, WINDOW_HIDE);

    /// <summary>
    /// Maximizes the console window.
    /// </summary>
    public static void MaximizeWindow(this ConsoleExtras extras) => ShowWindow(_consolePtr, WINDOW_MAXIMIZE);

    /// <summary>
    /// Minimizes the console window.
    /// </summary>
    public static void MinimizeWindow(this ConsoleExtras extras) => ShowWindow(_consolePtr, WINDOW_MINIMIZE);

    /// <summary>
    /// Restores the console window after minimization.
    /// </summary>
    public static void RestoreWindow(this ConsoleExtras extras) => ShowWindow(_consolePtr, WINDOW_RESTORE);

    private const int WINDOW_HIDE = 0;
    private const int WINDOW_MAXIMIZE = 3;
    private const int WINDOW_MINIMIZE = 6;
    private const int WINDOW_RESTORE = 9;

#if NET7_0_OR_GREATER
    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool ShowWindow(IntPtr hWnd, int nCmdShow);
#else
    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
#endif

    #endregion
}
