#pragma warning disable IDE0060 // Remove unused parameter

namespace Chubrik.XConsole;

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
public static partial class ConsoleWindowsExtensions
#else
public static class ConsoleWindowsExtensions
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

    #region App highlight

    // https://stackoverflow.com/questions/11309827

    /// <summary>
    /// Reset current app highlighting.
    /// </summary>
    public static void AppHighlightReset(this ConsoleExtras extras)
    {
        if (!_isAppHighlightSupported)
            return;

        var fInfo = new FlashInfo();
        fInfo.cbSize = Convert.ToUInt32(Marshal.SizeOf(fInfo));
        fInfo.hwnd = _consolePtr;
        fInfo.dwFlags = APP_HIGHLIGHT_RESET;
        fInfo.uCount = 0;
        fInfo.dwTimeout = 0;
        FlashWindowEx(ref fInfo);
    }

    /// <summary>
    /// Set current app highlighting.
    /// </summary>
    /// <param name="kind">Sets what will be highlighted.</param>
    /// <param name="blinkCount">Sets the count of blinks. 0 for infinite blinking.</param>
    /// <param name="resetOnFocus">Automatically reset highlighting when a window comes into focus.</param>
    public static void AppHighlight(
#pragma warning disable CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)
        this ConsoleExtras extras,
#pragma warning restore CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)
        AppHighlightKind kind = AppHighlightKind.WindowAndTaskbar,
        int blinkCount = 3,
        bool resetOnFocus = true)
    {
        if (!_isAppHighlightSupported)
            return;

        var flags = kind switch
        {
            AppHighlightKind.Window => APP_HIGHLIGHT_WINDOW,
            AppHighlightKind.Taskbar => APP_HIGHLIGHT_TASKBAR,
            AppHighlightKind.WindowAndTaskbar => APP_HIGHLIGHT_WINDOW | APP_HIGHLIGHT_TASKBAR,
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };

        if (resetOnFocus)
            flags |= APP_HIGHLIGHT_RESET_ON_FOCUS;

        var fInfo = new FlashInfo();
        fInfo.cbSize = Convert.ToUInt32(Marshal.SizeOf(fInfo));
        fInfo.hwnd = _consolePtr;
        fInfo.dwFlags = flags;
        fInfo.uCount = (uint)Math.Max(0, blinkCount);
        fInfo.dwTimeout = 0;
        FlashWindowEx(ref fInfo);
    }

    private static readonly bool _isAppHighlightSupported = GetIsAppHighlightSupported();

    private static bool GetIsAppHighlightSupported()
    {
#if NETSTANDARD1_3
        return true; // todo
#else
        return Environment.OSVersion.Version.Major >= 5;
#endif
    }

    private const uint APP_HIGHLIGHT_RESET = 0;
    private const uint APP_HIGHLIGHT_WINDOW = 1;
    private const uint APP_HIGHLIGHT_TASKBAR = 2;
    private const uint APP_HIGHLIGHT_RESET_ON_FOCUS = 8;

    [StructLayout(LayoutKind.Sequential)]
    private struct FlashInfo
    {
        public uint cbSize;
        public IntPtr hwnd;
        public uint dwFlags;
        public uint uCount;
        public uint dwTimeout;
    }

#if NET7_0_OR_GREATER
    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool FlashWindowEx(ref FlashInfo pwfi);
#else
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool FlashWindowEx(ref FlashInfo pwfi);
#endif

    #endregion

    #region Taskbar progress

    // https://stackoverflow.com/questions/77404714

    /// <summary>
    /// Sets the taskbar button to the default appearance.
    /// </summary>
    public static void TaskbarProgressReset(this ConsoleExtras extras)
    {
        if (!_isTaskbarProgressSupported)
            return;

        _taskbarInstance.SetProgressState(_consolePtr, TASKBAR_PROGRESS_RESET);
    }

    /// <summary>
    /// Sets the taskbar button to animated progress appearance.
    /// </summary>
    public static void TaskbarProgressAnimate(this ConsoleExtras extras)
    {
        if (!_isTaskbarProgressSupported)
            return;

        _taskbarInstance.SetProgressState(_consolePtr, TASKBAR_PROGRESS_ANIMATE);
    }

    /// <summary>
    /// Sets the taskbar button to specified progress appearance.
    /// </summary>
    /// <param name="value">Current progress value on the taskbar button.</param>
    /// <param name="maxValue">Maximum progress value on the taskbar button.</param>
    /// <param name="level">The appearance type of the taskbar button.</param>
#pragma warning disable CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)
    public static void TaskbarProgress(this ConsoleExtras extras,
#pragma warning restore CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)
        double value, double maxValue, TaskbarProgressLevel level = TaskbarProgressLevel.Default)
    {
        if (!_isTaskbarProgressSupported)
            return;

        if (value > 0 && !double.IsPositiveInfinity(value) &&
            maxValue > 0 && !double.IsPositiveInfinity(maxValue))
        {
            var state = level switch
            {
                TaskbarProgressLevel.Default => TASKBAR_PROGRESS_DEFAULT,
                TaskbarProgressLevel.Warning => TASKBAR_PROGRESS_WARNING,
                TaskbarProgressLevel.Error => TASKBAR_PROGRESS_ERROR,
                _ => TASKBAR_PROGRESS_RESET
            };

            _taskbarInstance.SetProgressState(_consolePtr, state);

            if (state != TASKBAR_PROGRESS_RESET)
                _taskbarInstance.SetProgressValue(_consolePtr, (ulong)Math.Min(value, maxValue), (ulong)maxValue);
        }
        else
            extras.TaskbarProgressReset();
    }

    private static readonly bool _isTaskbarProgressSupported = GetIsTaskbarProgressSupported();

    private static bool GetIsTaskbarProgressSupported()
    {
#if NET
        var isWindows = OperatingSystem.IsWindows();
#elif NETSTANDARD
        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
#else
        var isWindows = true; // todo .NET Framework
#endif
#if NETSTANDARD1_3
        var isSupportedOsVersion = true; // todo
#else
        var isSupportedOsVersion = Environment.OSVersion.Version >= new Version(6, 1);
#endif
        return isWindows && isSupportedOsVersion;
    }

    private static readonly ITaskbarInstance _taskbarInstance = (ITaskbarInstance)new TaskbarInstance();

    private const int TASKBAR_PROGRESS_RESET = 0;
    private const int TASKBAR_PROGRESS_ANIMATE = 1;
    private const int TASKBAR_PROGRESS_DEFAULT = 2;
    private const int TASKBAR_PROGRESS_ERROR = 4;
    private const int TASKBAR_PROGRESS_WARNING = 8;

    [ComImport]
    [Guid("56fdf344-fd6d-11d0-958a-006097c9a090")]
    [ClassInterface(ClassInterfaceType.None)]
    private class TaskbarInstance { }

    [ComImport]
    [Guid("ea1afb91-9e28-4b86-90e9-9e9f8a5eefaf")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface ITaskbarInstance
    {
        [PreserveSig]
        void HrInit();

        [PreserveSig]
        void AddTab(IntPtr hwnd);

        [PreserveSig]
        void DeleteTab(IntPtr hwnd);

        [PreserveSig]
        void ActivateTab(IntPtr hwnd);

        [PreserveSig]
        void SetActiveAlt(IntPtr hwnd);

        [PreserveSig]
        void MarkFullscreenWindow(IntPtr hwnd, [MarshalAs(UnmanagedType.Bool)] bool fFullscreen);

        [PreserveSig]
        void SetProgressValue(IntPtr hwnd, ulong ullCompleted, ulong ullTotal);

        [PreserveSig]
        void SetProgressState(IntPtr hwnd, int state);
    }

    #endregion

    #region Window resize

    // https://stackoverflow.com/questions/22053112

    /// <summary>
    /// Hides the console window.
    /// </summary>
    public static void WindowHide(this ConsoleExtras extras) => ShowWindow(_consolePtr, WINDOW_HIDE);

    /// <summary>
    /// Maximizes the console window.
    /// </summary>
    public static void WindowMaximize(this ConsoleExtras extras) => ShowWindow(_consolePtr, WINDOW_MAXIMIZE);

    /// <summary>
    /// Minimizes the console window.
    /// </summary>
    public static void WindowMinimize(this ConsoleExtras extras) => ShowWindow(_consolePtr, WINDOW_MINIMIZE);

    /// <summary>
    /// Restores the console window after minimization.
    /// </summary>
    public static void WindowRestore(this ConsoleExtras extras) => ShowWindow(_consolePtr, WINDOW_RESTORE);

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
