﻿#if !NETSTANDARD1_3

namespace Chubrik.XConsole.Extensions;

using System;
using System.Drawing;
using System.Globalization;

public static class StringExtensions
{
    private const string _foregroundColorFormat = "\x1b[38;2;{0};{1};{2}m{3}\x1b[39m";
    private const string _backgroundColorFormat = "\x1b[48;2;{0};{1};{2}m{3}\x1b[49m";
    private const string _foregroundConsoleColorFormat = "\x1b[{0}m{1}\x1b[39m";
    private const string _backgroundConsoleColorFormat = "\x1b[{0}m{1}\x1b[49m";
    private const string _underlineFormat = "\x1b[4m{0}\x1b[24m";

    private static readonly int[] _foregroundConsoleColorCodes =
        new[] { 30, 34, 32, 36, 31, 35, 33, 37, 90, 94, 92, 96, 91, 95, 93, 97 };

    private static readonly int[] _backgroundConsoleColorCodes =
        new[] { 40, 44, 42, 46, 41, 45, 43, 47, 100, 104, 102, 106, 101, 105, 103, 107 };

    #region Foreground color

    public static string Color(this string value, Color color)
    {
        return string.Format(_foregroundColorFormat, color.R, color.G, color.B, value);
    }

#if !NETSTANDARD2_0
    public static string Color(this string value, KnownColor color)
    {
        return value.Color(System.Drawing.Color.FromKnownColor(color: color));
    }
#endif

    public static string Color(this string value, ConsoleColor color)
    {
        return string.Format(_foregroundConsoleColorFormat, _foregroundConsoleColorCodes[(int)color], value);
    }

    public static string Color(this string value, int rgb)
    {
        return value.Color(System.Drawing.Color.FromArgb(argb: rgb));
    }

    public static string Color(this string value, int red, int green, int blue)
    {
        return value.Color(System.Drawing.Color.FromArgb(red: red, green: green, blue: blue));
    }

    public static string Color(this string value, string hexColor)
    {
        var rgb = int.Parse(hexColor[0] == '#' ? hexColor.Substring(1) : hexColor, NumberStyles.HexNumber);
        return value.Color(System.Drawing.Color.FromArgb(argb: rgb));
    }

    #endregion

    #region Background color

    public static string BgColor(this string value, Color color)
    {
        return string.Format(_backgroundColorFormat, color.R, color.G, color.B, value);
    }

#if !NETSTANDARD2_0
    public static string BgColor(this string value, KnownColor color)
    {
        return value.BgColor(System.Drawing.Color.FromKnownColor(color: color));
    }
#endif

    public static string BgColor(this string value, ConsoleColor color)
    {
        return string.Format(_backgroundConsoleColorFormat, _backgroundConsoleColorCodes[(int)color], value);
    }

    public static string BgColor(this string value, int rgb)
    {
        return value.BgColor(System.Drawing.Color.FromArgb(argb: rgb));
    }

    public static string BgColor(this string value, int red, int green, int blue)
    {
        return value.BgColor(System.Drawing.Color.FromArgb(red: red, green: green, blue: blue));
    }

    public static string BgColor(this string value, string hexColor)
    {
        var rgb = int.Parse(hexColor[0] == '#' ? hexColor.Substring(1) : hexColor, NumberStyles.HexNumber);
        return value.BgColor(System.Drawing.Color.FromArgb(argb: rgb));
    }

    #endregion

    public static string Underline(this string value)
    {
        return string.Format(_underlineFormat, value);
    }
}

#endif
