#if !NET
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#endif

namespace Chubrik.XConsole;

using System;

internal readonly struct ConsoleItem(
    string value,
    ConsoleItemType type = ConsoleItemType.Plain,
    ConsoleColor foreColor = default,
    ConsoleColor backColor = default)
{
    public string Value { get; } = value;
    public ConsoleItemType Type { get; } = type;
    public ConsoleColor ForeColor { get; } = foreColor;
    public ConsoleColor BackColor { get; } = backColor;

    public static ConsoleItem Parse(string? value)
    {
        const int tick = '`';
        const int y = 'y';
        const int space = ' ';
        const int _x1b = '\x1b';

        if (string.IsNullOrEmpty(value))
            return new(string.Empty);

        var char0 = (int)value[0];

        if (char0 == tick)
            return new(value.Substring(1));

        if (value.Length == 1)
            return new(value);

        if (value[1] == tick)
        {
            if (char0 <= y)
            {
                var foreColor = _colorMap[char0];

                if (foreColor != -1)
                    return new(value.Substring(2), ConsoleItemType.ForeColor, foreColor: (ConsoleColor)foreColor);
            }

            return new(value);
        }

        if (value.Length > 2 && value[2] == tick && char0 <= y)
        {
            var backColor = _colorMap[value[1]];

            if (backColor != -1)
            {
                if (char0 <= y)
                {
                    var foreColor = _colorMap[char0];

                    if (foreColor != -1)
                        return new(
                            value.Substring(3), ConsoleItemType.BothColors,
                            foreColor: (ConsoleColor)foreColor, backColor: (ConsoleColor)backColor);

                    if (char0 == space)
                        return new(value.Substring(3), ConsoleItemType.BackColor, backColor: (ConsoleColor)backColor);
                }
            }

            return new(value);
        }

        if (char0 == _x1b)
            return new(value, ConsoleItemType.Ansi);

        return new(value);
    }

    private static readonly int[] _colorMap =
    [
        -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
        -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
        -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
        -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
        -1, -1,  9, 11, -1, -1, -1, 10, -1, -1, -1, -1, -1, 13, -1, -1,
        -1, -1, 12, -1, -1, -1, -1, 15, -1, 14, -1, -1, -1, -1, -1, -1,
        -1, -1,  1,  3,  8, -1, -1,  2, -1, -1, -1, -1, -1,  5,  0, -1,
        -1, -1,  4, -1, -1, -1, -1,  7, -1,  6
    ];
}
