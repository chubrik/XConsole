#if NET

namespace Chubrik.XConsole.Extras;

using System;
using System.Drawing;
using System.Runtime.Versioning;

/// <summary>
/// Char extensions for coloring and underlining text in the console.
/// </summary>
[SupportedOSPlatform("windows")]
public static class ConsoleCharExtras
{
    #region Foreground color

    /// <summary>
    /// Creates the string for coloring char in the console.
    /// </summary>
    public static string Color(this char value, ConsoleColor color)
    {
        return value.ToString().Color(color: color);
    }

    /// <inheritdoc cref="Color(char, ConsoleColor)"/>
    public static string Color(this char value, KnownColor color)
    {
        return value.ToString().Color(color: System.Drawing.Color.FromKnownColor(color: color));
    }

    /// <inheritdoc cref="Color(char, ConsoleColor)"/>
    public static string Color(this char value, Color color)
    {
        return value.ToString().Color(color: color);
    }

    /// <inheritdoc cref="Color(char, ConsoleColor)"/>
    public static string Color(this char value, int rgb)
    {
        return value.ToString().Color(rgb: rgb);
    }

    /// <inheritdoc cref="Color(char, ConsoleColor)"/>
    public static string Color(this char value, string hexColor)
    {
        return value.ToString().Color(hexColor: hexColor);
    }

    /// <inheritdoc cref="Color(char, ConsoleColor)"/>
    public static string Color(this char value, int red, int green, int blue)
    {
        return value.ToString().Color(red: red, green: green, blue: blue);
    }

    #endregion

    #region Background color

    /// <summary>
    /// Creates the string for coloring char background in the console.
    /// </summary>
    public static string BgColor(this char value, ConsoleColor color)
    {
        return value.ToString().BgColor(color: color);
    }

    /// <inheritdoc cref="BgColor(char, ConsoleColor)"/>
    public static string BgColor(this char value, KnownColor color)
    {
        return value.ToString().BgColor(color: System.Drawing.Color.FromKnownColor(color: color));
    }

    /// <inheritdoc cref="BgColor(char, ConsoleColor)"/>
    public static string BgColor(this char value, Color color)
    {
        return value.ToString().BgColor(color: color);
    }

    /// <inheritdoc cref="BgColor(char, ConsoleColor)"/>
    public static string BgColor(this char value, int rgb)
    {
        return value.ToString().BgColor(rgb: rgb);
    }

    /// <inheritdoc cref="BgColor(char, ConsoleColor)"/>
    public static string BgColor(this char value, string hexColor)
    {
        return value.ToString().BgColor(hexColor: hexColor);
    }

    /// <inheritdoc cref="BgColor(char, ConsoleColor)"/>
    public static string BgColor(this char value, int red, int green, int blue)
    {
        return value.ToString().BgColor(red: red, green: green, blue: blue);
    }

    #endregion

    /// <summary>
    /// Creates the string for underlining char in the console.
    /// </summary>
    public static string Underline(this char value)
    {
        return value.ToString().Underline();
    }
}

#endif
