﻿#if NET

namespace Chubrik.XConsole;

using System;
using System.Drawing;
using System.Globalization;

public static class StringExtensions
{
    internal const string ForegroundColorFormat = "\x1b[38;2;{0};{1};{2}m{3}\x1b[39m";
    internal const string BackgroundColorFormat = "\x1b[48;2;{0};{1};{2}m{3}\x1b[49m";
    internal const string ForegroundConsoleColorFormat = "\x1b[{0}m{1}\x1b[39m";
    internal const string BackgroundConsoleColorFormat = "\x1b[{0}m{1}\x1b[49m";
    internal const string UnderlineFormat = "\x1b[4m{0}\x1b[24m";

    internal static readonly int[] ForegroundConsoleColorCodes =
        new[] { 30, 34, 32, 36, 31, 35, 33, 37, 90, 94, 92, 96, 91, 95, 93, 97 };

    internal static readonly int[] BackgroundConsoleColorCodes =
        new[] { 40, 44, 42, 46, 41, 45, 43, 47, 100, 104, 102, 106, 101, 105, 103, 107 };

    #region Foreground color

    /// <summary>XConsole extension for colorizing text in the console.</summary>
    public static string Color(this string value, Color color)
    {
        return XConsole.VirtualTerminalAndColoringEnabled
            ? string.Format(ForegroundColorFormat, color.R, color.G, color.B, value)
            : value;
    }

    /// <summary>XConsole extension for colorizing text in the console.</summary>
    public static string Color(this string value, KnownColor color)
    {
        return value.Color(System.Drawing.Color.FromKnownColor(color: color));
    }

    /// <summary>XConsole extension for colorizing text in the console.</summary>
    public static string Color(this string value, ConsoleColor color)
    {
        return XConsole.VirtualTerminalAndColoringEnabled
            ? string.Format(ForegroundConsoleColorFormat, ForegroundConsoleColorCodes[(int)color], value)
            : value;
    }

    /// <summary>XConsole extension for colorizing text in the console.</summary>
    public static string Color(this string value, int rgb)
    {
        return value.Color(System.Drawing.Color.FromArgb(argb: rgb));
    }

    /// <summary>XConsole extension for colorizing text in the console.</summary>
    public static string Color(this string value, int red, int green, int blue)
    {
        return value.Color(System.Drawing.Color.FromArgb(red: red, green: green, blue: blue));
    }

    /// <summary>XConsole extension for colorizing text in the console.</summary>
    public static string Color(this string value, string hexColor)
    {
        var rgb = int.Parse(hexColor[0] == '#' ? hexColor.Substring(1) : hexColor, NumberStyles.HexNumber);
        return value.Color(System.Drawing.Color.FromArgb(argb: rgb));
    }

    #endregion

    #region Background color

    /// <summary>XConsole extension for colorizing text in the console.</summary>
    public static string BgColor(this string value, Color color)
    {
        return XConsole.VirtualTerminalAndColoringEnabled
            ? string.Format(BackgroundColorFormat, color.R, color.G, color.B, value)
            : value;
    }

    /// <summary>XConsole extension for colorizing text in the console.</summary>
    public static string BgColor(this string value, KnownColor color)
    {
        return value.BgColor(System.Drawing.Color.FromKnownColor(color: color));
    }

    /// <summary>XConsole extension for colorizing text in the console.</summary>
    public static string BgColor(this string value, ConsoleColor color)
    {
        return XConsole.VirtualTerminalAndColoringEnabled
            ? string.Format(BackgroundConsoleColorFormat, BackgroundConsoleColorCodes[(int)color], value)
            : value;
    }

    /// <summary>XConsole extension for colorizing text in the console.</summary>
    public static string BgColor(this string value, int rgb)
    {
        return value.BgColor(System.Drawing.Color.FromArgb(argb: rgb));
    }

    /// <summary>XConsole extension for colorizing text in the console.</summary>
    public static string BgColor(this string value, int red, int green, int blue)
    {
        return value.BgColor(System.Drawing.Color.FromArgb(red: red, green: green, blue: blue));
    }

    /// <summary>XConsole extension for colorizing text in the console.</summary>
    public static string BgColor(this string value, string hexColor)
    {
        var rgb = int.Parse(hexColor[0] == '#' ? hexColor.Substring(1) : hexColor, NumberStyles.HexNumber);
        return value.BgColor(System.Drawing.Color.FromArgb(argb: rgb));
    }

    #endregion

    /// <summary>XConsole extension for underlining text in the console.</summary>
    public static string Underline(this string value)
    {
        return XConsole.VirtualTerminalEnabled ? string.Format(UnderlineFormat, value) : value;
    }
}

#endif
