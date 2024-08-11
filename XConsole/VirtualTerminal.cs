#if NET

namespace Chubrik.XConsole;

using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

[SupportedOSPlatform("windows")]
#if NET7_0_OR_GREATER
internal static partial class VirtualTerminal
#else
internal static class VirtualTerminal
#endif
{
    public static readonly bool IsEnabled = TryEnable();

    // https://learn.microsoft.com/en-us/windows/console/console-virtual-terminal-sequences

    private const int STD_OUTPUT_HANDLE = -11;
    private const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 4;

    private static readonly IntPtr INVALID_HANDLE_VALUE = new(-1);

    private static bool TryEnable()
    {
        if (!OperatingSystem.IsWindows())
            return false;

        try
        {
            var hOut = GetStdHandle(STD_OUTPUT_HANDLE);

            return hOut != INVALID_HANDLE_VALUE &&
                GetConsoleMode(hOut, out var dwMode) &&
                SetConsoleMode(hOut, dwMode | ENABLE_VIRTUAL_TERMINAL_PROCESSING);
        }
        catch
        {
            return false;
        }
    }

#if NET7_0_OR_GREATER
    [LibraryImport("kernel32.dll", SetLastError = true)]
    private static partial IntPtr GetStdHandle(int nStdHandle);

    [LibraryImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

    [LibraryImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);
#else
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll")]
    private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

    [DllImport("kernel32.dll")]
    private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);
#endif
}

#endif
