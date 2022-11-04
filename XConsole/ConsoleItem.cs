namespace Chubrik.XConsole;

using System;

internal readonly struct ConsoleItem
{
    public ConsoleItemType Type { get; }
    public string Value { get; }
    public ConsoleColor BackColor { get; }
    public ConsoleColor ForeColor { get; }

    private ConsoleItem(ConsoleItemType type, string value, ConsoleColor backColor, ConsoleColor foreColor)
    {
        Type = type;
        Value = value;
        BackColor = backColor;
        ForeColor = foreColor;
    }

    public ConsoleItem(string value)
    {
        Type = ConsoleItemType.Plain;
        Value = value;
        BackColor = default;
        ForeColor = default;
    }

    public static ConsoleItem Parse(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return new(string.Empty);

        var char0 = value[0];

        if (char0 == '`')
            return new(value.Substring(1));

        if (value.Length == 1)
            return new(value);

        if (value[1] == '`')
        {
            if (char0 <= 'y')
            {
                var foreColor = _colorMap[char0];

                if (foreColor != -1)
                    return new(
                        ConsoleItemType.ForeColor, value.Substring(2),
                        backColor: default, (ConsoleColor)foreColor);
            }

            return new(value);
        }

        if (value.Length > 2 && value[2] == '`' && char0 <= 'y')
        {
            var backColor = _colorMap[char0];

            if (backColor != -1)
            {
                var char1 = value[1];

                if (char1 <= 'y')
                {
                    var foreColor = _colorMap[char1];

                    if (foreColor != -1)
                        return new(
                            ConsoleItemType.BothColors, value.Substring(3),
                            (ConsoleColor)backColor, (ConsoleColor)foreColor);

                    if (char1 == ' ')
                        return new(
                            ConsoleItemType.BackColor, value.Substring(3),
                            (ConsoleColor)backColor, foreColor: default);
                }
            }

            return new(value);
        }

        if (char0 == '\x1b')
            return new(ConsoleItemType.Ansi, value, backColor: default, foreColor: default);

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
