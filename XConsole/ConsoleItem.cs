namespace Chubrik.XConsole;

using System;

internal readonly struct ConsoleItem(
    ReadOnlyMemory<char> value,
    ConsoleItemType type = ConsoleItemType.Plain,
    ConsoleColor foreColor = (ConsoleColor)(-1),
    ConsoleColor backColor = (ConsoleColor)(-1))
{
    public ReadOnlyMemory<char> Value { get; } = value;
    public ConsoleItemType Type { get; } = type;
    public ConsoleColor ForeColor { get; } = foreColor;
    public ConsoleColor BackColor { get; } = backColor;

    public ConsoleItem(
        string value,
        ConsoleItemType type = ConsoleItemType.Plain,
        ConsoleColor foreColor = (ConsoleColor)(-1),
        ConsoleColor backColor = (ConsoleColor)(-1)
    )
        : this(
            value: value.AsMemory(),
            type: type,
            foreColor: foreColor,
            backColor: backColor)
    { }

    private static readonly ConsoleItem _empty = new(ReadOnlyMemory<char>.Empty);

    public static ConsoleItem Parse(string? value)
    {
        if (value == null)
            return _empty;

        var memory = value.AsMemory();

        if (value.Length < 2)
            return new(memory);

        var char0 = value[0];
        var char1 = value[1];

        if (char1 == '`')
        {
            if (char0 <= 'y')
            {
                var foreColor = _colorMap[char0];

                if (foreColor != -1)
                    return new(memory[2..], ConsoleItemType.ForeColor, foreColor: (ConsoleColor)foreColor);
            }

            return new(memory);
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
                        return new(memory[3..], ConsoleItemType.BothColors,
                            foreColor: (ConsoleColor)foreColor, backColor: (ConsoleColor)backColor);

                    if (char0 == ' ')
                        return new(memory[3..], ConsoleItemType.BackColor, backColor: (ConsoleColor)backColor);
                }
            }

            return new(memory);
        }

        if (char0 == '\x1b')
            return new(memory, ConsoleItemType.Ansi);

        return new(memory);
    }

    public int GetSingleLineLengthOrMinusOne()
    {
        var span = Value.Span;
        var result = 0;
        int @char;

        for (var i = 0; i < span.Length; i++)
        {
            @char = span[i];

            if (@char < ' ')
            {
                if (@char == '\x1b' && VirtualTerminal.IsEnabled)
                {
                    i++;

                    for (; i < span.Length; i++)
                    {
                        @char = span[i];

                        if (@char < ' ')
                            return -1;

                        if ((@char >= 'A' && @char <= 'Z') || (@char >= 'a' && @char <= 'z'))
                            break;
                    }

                    continue;
                }
                else
                {
                    return -1;
                }
            }

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
