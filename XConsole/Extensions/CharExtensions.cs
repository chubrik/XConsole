#if NET

namespace Chubrik.XConsole;

using System;
using System.Drawing;
using System.Globalization;
using static StringExtensions;

public static class CharExtensions
{
    #region Foreground color

    /// <summary>XConsole extension for colorizing text in the console.</summary>
    public static string Color(this char value, Color color)
    {
        return XConsole.VirtualTerminalAndColoringEnabled
            ? string.Format(_foregroundColorFormat, color.R, color.G, color.B, value)
            : value.ToString();
    }

    /// <summary>XConsole extension for colorizing text in the console.</summary>
    public static string Color(this char value, KnownColor color)
    {
        return value.Color(System.Drawing.Color.FromKnownColor(color: color));
    }

    /// <summary>XConsole extension for colorizing text in the console.</summary>
    public static string Color(this char value, ConsoleColor color)
    {
        return XConsole.VirtualTerminalAndColoringEnabled
            ? string.Format(_foregroundConsoleColorFormat, _foregroundConsoleColorCodes[(int)color], value)
            : value.ToString();
    }

    /// <summary>XConsole extension for colorizing text in the console.</summary>
    public static string Color(this char value, int rgb)
    {
        return value.Color(System.Drawing.Color.FromArgb(argb: rgb));
    }

    /// <summary>XConsole extension for colorizing text in the console.</summary>
    public static string Color(this char value, int red, int green, int blue)
    {
        return value.Color(System.Drawing.Color.FromArgb(red: red, green: green, blue: blue));
    }

    /// <summary>XConsole extension for colorizing text in the console.</summary>
    public static string Color(this char value, string hexColor)
    {
        var rgb = int.Parse(hexColor[0] == '#' ? hexColor.Substring(1) : hexColor, NumberStyles.HexNumber);
        return value.Color(System.Drawing.Color.FromArgb(argb: rgb));
    }

    #endregion

    #region Background color

    /// <summary>XConsole extension for colorizing text in the console.</summary>
    public static string BgColor(this char value, Color color)
    {
        return XConsole.VirtualTerminalAndColoringEnabled
            ? string.Format(_backgroundColorFormat, color.R, color.G, color.B, value)
            : value.ToString();
    }

    /// <summary>XConsole extension for colorizing text in the console.</summary>
    public static string BgColor(this char value, KnownColor color)
    {
        return value.BgColor(System.Drawing.Color.FromKnownColor(color: color));
    }

    /// <summary>XConsole extension for colorizing text in the console.</summary>
    public static string BgColor(this char value, ConsoleColor color)
    {
        return XConsole.VirtualTerminalAndColoringEnabled
            ? string.Format(_backgroundConsoleColorFormat, _backgroundConsoleColorCodes[(int)color], value)
            : value.ToString();
    }

    /// <summary>XConsole extension for colorizing text in the console.</summary>
    public static string BgColor(this char value, int rgb)
    {
        return value.BgColor(System.Drawing.Color.FromArgb(argb: rgb));
    }

    /// <summary>XConsole extension for colorizing text in the console.</summary>
    public static string BgColor(this char value, int red, int green, int blue)
    {
        return value.BgColor(System.Drawing.Color.FromArgb(red: red, green: green, blue: blue));
    }

    /// <summary>XConsole extension for colorizing text in the console.</summary>
    public static string BgColor(this char value, string hexColor)
    {
        var rgb = int.Parse(hexColor[0] == '#' ? hexColor.Substring(1) : hexColor, NumberStyles.HexNumber);
        return value.BgColor(System.Drawing.Color.FromArgb(argb: rgb));
    }

    #endregion

    /// <summary>XConsole extension for underlining text in the console.</summary>
    public static string Underline(this char value)
    {
        return XConsole._virtualTerminalEnabled ? string.Format(_underlineFormat, value) : value.ToString();
    }
}

#endif
