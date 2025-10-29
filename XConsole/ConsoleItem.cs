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
        if (value == null || value.Length < 2)
            return new(value ?? string.Empty);

        var char0 = (int)value[0];
        var char1 = (int)value[1];

        if (char1 == '`')
        {
            if (char0 <= 'y')
            {
                var foreColor = _colorMap[char0];

                if (foreColor != -1)
                    return new(value.Substring(2), ConsoleItemType.ForeColor, foreColor: (ConsoleColor)foreColor);
            }

            return new(value);
        }

        if (value.Length >= 3 && value[2] == '`')
        {
            if (char0 <= 'y' && char1 <= 'y')
            {
                var backColor = _colorMap[char1];

                if (backColor != -1)
                {
                    var foreColor = _colorMap[char0];

                    if (foreColor != -1)
                        return new(value.Substring(3), ConsoleItemType.BothColors,
                            foreColor: (ConsoleColor)foreColor, backColor: (ConsoleColor)backColor);

                    if (char0 == ' ')
                        return new(value.Substring(3), ConsoleItemType.BackColor, backColor: (ConsoleColor)backColor);
                }
            }

            return new(value);
        }

        if (char0 == '\x1b')
            return new(value, ConsoleItemType.Ansi);

        return new(value);
    }

    public int GetSingleLineLengthOrZero()
    {
        var value = Value;
        var valueLength = value.Length;
        var result = 0;
        int @char;

        for (var i = 0; i < valueLength; i++)
        {
            @char = value[i];

            if (@char < ' ')
                return 0;
#if NET
            if (@char == '\x1b' && VirtualTerminal.IsEnabled)
            {
                i++;

                for (; i < valueLength; i++)
                {
                    @char = value[i];

                    if (@char < ' ')
                        return 0;

                    if ((@char >= 'A' && @char <= 'Z') || (@char >= 'a' && @char <= 'z'))
                        break;
                }

                continue;
            }
#endif
            result++;
        }

        return result;
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
