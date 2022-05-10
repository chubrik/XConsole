namespace Chubrik.XConsole;

using System;
using System.Diagnostics;

internal struct XConsoleItem
{
    public const ConsoleColor NoColor = (ConsoleColor)(-1);

    public readonly string Value;
    public readonly ConsoleColor BackColor;
    public readonly ConsoleColor ForeColor;

    private XConsoleItem(string value, ConsoleColor backColor, ConsoleColor foreColor)
    {
        Value = value;
        BackColor = backColor;
        ForeColor = foreColor;
    }

    public XConsoleItem(string value)
    {
        Value = value;
        BackColor = NoColor;
        ForeColor = NoColor;
    }

    public static XConsoleItem Parse(string value)
    {
        Debug.Assert(!string.IsNullOrEmpty(value));
        var char0 = value[0];

        if (char0 == '`')
            return new(value.Substring(1));

        if (value.Length == 1)
            return new(value);

        if (value[1] == '`')
        {
            if (char0 <= 121)
            {
                var foreColor = (ConsoleColor)_colorMap[char0];

                if (foreColor != NoColor)
                    return new(value.Substring(2), NoColor, foreColor);
            }

            return new(value);
        }

        if (value.Length > 2 && value[2] == '`' && char0 <= 121)
        {
            var backColor = (ConsoleColor)_colorMap[char0];

            if (backColor != NoColor)
            {
                var char1 = value[1];

                if (char1 <= 121)
                {
                    var foreColor = (ConsoleColor)_colorMap[char1];

                    if (foreColor != NoColor || char1 == ' ')
                        return new(value.Substring(3), backColor, foreColor);
                }
            }
        }

        return new(value);
    }

    private static readonly int[] _colorMap = new[]
    {
        -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
        -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
        -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
        -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
        -1, -1,  9, 11, -1, -1, -1, 10, -1, -1, -1, -1, -1, 13, -1, -1,
        -1, -1, 12, -1, -1, -1, -1, 15, -1, 14, -1, -1, -1, -1, -1, -1,
        -1, -1,  1,  3,  8, -1, -1,  2, -1, -1, -1, -1, -1,  5,  0, -1,
        -1, -1,  4, -1, -1, -1, -1,  7, -1,  6
    };
}
