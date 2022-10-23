namespace XConsole;

using System;

internal readonly struct ConsoleItem
{
    public const ConsoleColor NoColor = (ConsoleColor)(-1);
    private static readonly ConsoleItem Empty = new(string.Empty);

    public string Value { get; }
    public ConsoleColor BackColor { get; }
    public ConsoleColor ForeColor { get; }

    private ConsoleItem(string value, ConsoleColor backColor, ConsoleColor foreColor)
    {
        Value = value;
        BackColor = backColor;
        ForeColor = foreColor;
    }

    public ConsoleItem(string value)
    {
        Value = value;
        BackColor = NoColor;
        ForeColor = NoColor;
    }

    public static ConsoleItem Parse(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return Empty;

        var char0 = value[0];

        if (char0 == '`')
            return new(value.Substring(1));

        if (value.Length == 1)
            return new(value);

        if (value[1] == '`')
        {
            if (char0 <= 'y')
            {
                var foreColor = (ConsoleColor)_colorMap[char0];

                if (foreColor != NoColor)
                    return new(value.Substring(2), NoColor, foreColor);
            }

            return new(value);
        }

        if (value.Length > 2 && value[2] == '`' && char0 <= 'y')
        {
            var backColor = (ConsoleColor)_colorMap[char0];

            if (backColor != NoColor)
            {
                var char1 = value[1];

                if (char1 <= 'y')
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
