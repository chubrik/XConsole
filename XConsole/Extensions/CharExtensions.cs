#if NET

namespace Chubrik.XConsole;

using System;
using System.Drawing;

public static class CharExtensions
{
    #region Foreground color

    /// <summary>XConsole extension for colorizing text in the console.</summary>
    public static string Color(this char value, ConsoleColor color)
    {
        return value.ToString().Color(color: color);
    }

    /// <summary>XConsole extension for colorizing text in the console.</summary>
    public static string Color(this char value, KnownColor color)
    {
        return value.ToString().Color(color: System.Drawing.Color.FromKnownColor(color: color));
    }

    /// <summary>XConsole extension for colorizing text in the console.</summary>
    public static string Color(this char value, Color color)
    {
        return value.ToString().Color(color: color);
    }

    /// <summary>XConsole extension for colorizing text in the console.</summary>
    public static string Color(this char value, int rgb)
    {
        return value.ToString().Color(rgb: rgb);
    }

    /// <summary>XConsole extension for colorizing text in the console.</summary>
    public static string Color(this char value, string hexColor)
    {
        return value.ToString().Color(hexColor: hexColor);
    }

    /// <summary>XConsole extension for colorizing text in the console.</summary>
    public static string Color(this char value, int red, int green, int blue)
    {
        return value.ToString().Color(red: red, green: green, blue: blue);
    }

    #endregion

    #region Background color

    /// <summary>XConsole extension for colorizing text in the console.</summary>
    public static string BgColor(this char value, ConsoleColor color)
    {
        return value.ToString().BgColor(color: color);
    }

    /// <summary>XConsole extension for colorizing text in the console.</summary>
    public static string BgColor(this char value, KnownColor color)
    {
        return value.ToString().BgColor(color: System.Drawing.Color.FromKnownColor(color: color));
    }

    /// <summary>XConsole extension for colorizing text in the console.</summary>
    public static string BgColor(this char value, Color color)
    {
        return value.ToString().BgColor(color: color);
    }

    /// <summary>XConsole extension for colorizing text in the console.</summary>
    public static string BgColor(this char value, int rgb)
    {
        return value.ToString().BgColor(rgb: rgb);
    }

    /// <summary>XConsole extension for colorizing text in the console.</summary>
    public static string BgColor(this char value, string hexColor)
    {
        return value.ToString().BgColor(hexColor: hexColor);
    }

    /// <summary>XConsole extension for colorizing text in the console.</summary>
    public static string BgColor(this char value, int red, int green, int blue)
    {
        return value.ToString().BgColor(red: red, green: green, blue: blue);
    }

    #endregion

    /// <summary>XConsole extension for underlining text in the console.</summary>
    public static string Underline(this char value)
    {
        return value.ToString().Underline();
    }
}

#endif
