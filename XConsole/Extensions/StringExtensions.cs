#if NET

namespace Chubrik.XConsole;

using System;
using System.Drawing;
using System.Globalization;
using System.Runtime.CompilerServices;

public static class StringExtensions
{
    private const string _foregroundConsoleColorFormat = "\x1b[{0}m{1}\x1b[39m";
    private const string _backgroundConsoleColorFormat = "\x1b[{0}m{1}\x1b[49m";
    private const string _foregroundRgbColorFormat = "\x1b[38;2;{0};{1};{2}m{3}\x1b[39m";
    private const string _backgroundRgbColorFormat = "\x1b[48;2;{0};{1};{2}m{3}\x1b[49m";
    private const string _underlineFormat = "\x1b[4m{0}\x1b[24m";

    internal static readonly int[] _foregroundConsoleColorCodes =
        [30, 34, 32, 36, 31, 35, 33, 37, 90, 94, 92, 96, 91, 95, 93, 97];

    internal static readonly int[] _backgroundConsoleColorCodes =
        [40, 44, 42, 46, 41, 45, 43, 47, 100, 104, 102, 106, 101, 105, 103, 107];

    #region Foreground color

    /// <summary>XConsole extension for colorizing text in the console.</summary>
    public static string Color(this string value, ConsoleColor color)
    {
        return XConsole.VirtualTerminalAndColoringEnabled
            ? string.Format(_foregroundConsoleColorFormat, _foregroundConsoleColorCodes[(int)color], value)
            : value;
    }

    /// <summary>XConsole extension for colorizing text in the console.</summary>
    public static string Color(this string value, KnownColor color)
    {
        return value.Color(color: System.Drawing.Color.FromKnownColor(color: color));
    }

    /// <summary>XConsole extension for colorizing text in the console.</summary>
    public static string Color(this string value, Color color)
    {
        return ColorBase(value: value, red: color.R, green: color.G, blue: color.B);
    }

    /// <summary>XConsole extension for colorizing text in the console.</summary>
    public static string Color(this string value, int rgb)
    {
        if (rgb < 0 || rgb > 16777215)
            throw new ArgumentOutOfRangeException(nameof(rgb));

        return ColorBase(value: value, red: rgb >> 16, green: (rgb >> 8) & 255, blue: rgb & 255);
    }

    /// <summary>XConsole extension for colorizing text in the console.</summary>
    public static string Color(this string value, string hexColor)
    {
        var hex = hexColor[0] == '#' ? hexColor[1..] : hexColor;

        if (!int.TryParse(hex, NumberStyles.HexNumber, provider: null, out var rgb))
            throw new FormatException();

        if (rgb > 16777215)
            throw new ArgumentOutOfRangeException(nameof(hexColor));

        return ColorBase(value: value, red: rgb >> 16, green: (rgb >> 8) & 255, blue: rgb & 255);
    }

    /// <summary>XConsole extension for colorizing text in the console.</summary>
    public static string Color(this string value, int red, int green, int blue)
    {
        if (red < 0 || red > 255)
            throw new ArgumentOutOfRangeException(nameof(red));

        if (green < 0 || green > 255)
            throw new ArgumentOutOfRangeException(nameof(green));

        if (blue < 0 || blue > 255)
            throw new ArgumentOutOfRangeException(nameof(blue));

        return ColorBase(value: value, red: red, green: green, blue: blue);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string ColorBase(string value, int red, int green, int blue)
    {
        return XConsole.VirtualTerminalAndColoringEnabled
            ? string.Format(_foregroundRgbColorFormat, red, green, blue, value)
            : value;
    }

    #endregion

    #region Background color

    /// <summary>XConsole extension for colorizing text in the console.</summary>
    public static string BgColor(this string value, ConsoleColor color)
    {
        return XConsole.VirtualTerminalAndColoringEnabled
            ? string.Format(_backgroundConsoleColorFormat, _backgroundConsoleColorCodes[(int)color], value)
            : value;
    }

    /// <summary>XConsole extension for colorizing text in the console.</summary>
    public static string BgColor(this string value, KnownColor color)
    {
        return value.BgColor(System.Drawing.Color.FromKnownColor(color: color));
    }

    /// <summary>XConsole extension for colorizing text in the console.</summary>
    public static string BgColor(this string value, Color color)
    {
        return BgColorBase(value: value, red: color.R, green: color.G, blue: color.B);
    }

    /// <summary>XConsole extension for colorizing text in the console.</summary>
    public static string BgColor(this string value, int rgb)
    {
        if (rgb < 0 || rgb > 16777215)
            throw new ArgumentOutOfRangeException(nameof(rgb));

        return BgColorBase(value: value, red: rgb >> 16, green: (rgb >> 8) & 255, blue: rgb & 255);
    }

    /// <summary>XConsole extension for colorizing text in the console.</summary>
    public static string BgColor(this string value, string hexColor)
    {
        var hex = hexColor[0] == '#' ? hexColor[1..] : hexColor;

        if (!int.TryParse(hex, NumberStyles.HexNumber, provider: null, out var rgb))
            throw new FormatException();

        if (rgb > 16777215)
            throw new ArgumentOutOfRangeException(nameof(hexColor));

        return BgColorBase(value: value, red: rgb >> 16, green: (rgb >> 8) & 255, blue: rgb & 255);
    }

    /// <summary>XConsole extension for colorizing text in the console.</summary>
    public static string BgColor(this string value, int red, int green, int blue)
    {
        if (red < 0 || red > 255)
            throw new ArgumentOutOfRangeException(nameof(red));

        if (green < 0 || green > 255)
            throw new ArgumentOutOfRangeException(nameof(green));

        if (blue < 0 || blue > 255)
            throw new ArgumentOutOfRangeException(nameof(blue));

        return BgColorBase(value: value, red: red, green: green, blue: blue);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string BgColorBase(string value, int red, int green, int blue)
    {
        return XConsole.VirtualTerminalAndColoringEnabled
            ? string.Format(_backgroundRgbColorFormat, red, green, blue, value)
            : value;
    }

    #endregion

    /// <summary>XConsole extension for underlining text in the console.</summary>
    public static string Underline(this string value)
    {
        return XConsole.VirtualTerminalEnabled ? string.Format(_underlineFormat, value) : value;
    }
}

#endif
