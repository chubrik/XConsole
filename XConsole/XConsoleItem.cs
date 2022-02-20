namespace System
{
    internal struct XConsoleItem
    {
        public const ConsoleColor NoColor = (ConsoleColor)(-1);

        public string Value;
        public readonly ConsoleColor BgColor;
        public readonly ConsoleColor FgColor;

        private XConsoleItem(string value, ConsoleColor bgColor, ConsoleColor fgColor)
        {
            Value = value;
            BgColor = bgColor;
            FgColor = fgColor;
        }

        public static XConsoleItem Parse(string value)
        {
            if (value.Length == 0)
                return new(string.Empty, NoColor, NoColor);

            if (value[0] == '`')
                return new(value[1..], NoColor, NoColor);

            if (value.Length == 1)
                return new(value, NoColor, NoColor);

            if (value[1] == '`')
                return value[0] switch
                {
                    'W' => new(value[2..], NoColor, ConsoleColor.White),
                    'Y' => new(value[2..], NoColor, ConsoleColor.Yellow),
                    'C' => new(value[2..], NoColor, ConsoleColor.Cyan),
                    'G' => new(value[2..], NoColor, ConsoleColor.Green),
                    'M' => new(value[2..], NoColor, ConsoleColor.Magenta),
                    'R' => new(value[2..], NoColor, ConsoleColor.Red),
                    'B' => new(value[2..], NoColor, ConsoleColor.Blue),
                    'w' => new(value[2..], NoColor, ConsoleColor.Gray),
                    'y' => new(value[2..], NoColor, ConsoleColor.DarkYellow),
                    'c' => new(value[2..], NoColor, ConsoleColor.DarkCyan),
                    'g' => new(value[2..], NoColor, ConsoleColor.DarkGreen),
                    'm' => new(value[2..], NoColor, ConsoleColor.DarkMagenta),
                    'r' => new(value[2..], NoColor, ConsoleColor.DarkRed),
                    'b' => new(value[2..], NoColor, ConsoleColor.DarkBlue),
                    'd' => new(value[2..], NoColor, ConsoleColor.DarkGray),
                    'n' => new(value[2..], NoColor, ConsoleColor.Black),
                    _ => new(value, NoColor, NoColor),
                };

            if (value.Length == 2)
                return new(value, NoColor, NoColor);

            if (value[2] == '`')
            {
                ConsoleColor bgColor;

                switch (value[0])
                {
                    case 'W': bgColor = ConsoleColor.White; break;
                    case 'Y': bgColor = ConsoleColor.Yellow; break;
                    case 'C': bgColor = ConsoleColor.Cyan; break;
                    case 'G': bgColor = ConsoleColor.Green; break;
                    case 'M': bgColor = ConsoleColor.Magenta; break;
                    case 'R': bgColor = ConsoleColor.Red; break;
                    case 'B': bgColor = ConsoleColor.Blue; break;
                    case 'w': bgColor = ConsoleColor.Gray; break;
                    case 'y': bgColor = ConsoleColor.DarkYellow; break;
                    case 'c': bgColor = ConsoleColor.DarkCyan; break;
                    case 'g': bgColor = ConsoleColor.DarkGreen; break;
                    case 'm': bgColor = ConsoleColor.DarkMagenta; break;
                    case 'r': bgColor = ConsoleColor.DarkRed; break;
                    case 'b': bgColor = ConsoleColor.DarkBlue; break;
                    case 'd': bgColor = ConsoleColor.DarkGray; break;
                    case 'n': bgColor = ConsoleColor.Black; break;
                    default: return new(value, NoColor, NoColor);
                }

                return value[1] switch
                {
                    'W' => new(value[3..], bgColor, ConsoleColor.White),
                    'Y' => new(value[3..], bgColor, ConsoleColor.Yellow),
                    'C' => new(value[3..], bgColor, ConsoleColor.Cyan),
                    'G' => new(value[3..], bgColor, ConsoleColor.Green),
                    'M' => new(value[3..], bgColor, ConsoleColor.Magenta),
                    'R' => new(value[3..], bgColor, ConsoleColor.Red),
                    'B' => new(value[3..], bgColor, ConsoleColor.Blue),
                    'w' => new(value[3..], bgColor, ConsoleColor.Gray),
                    'y' => new(value[3..], bgColor, ConsoleColor.DarkYellow),
                    'c' => new(value[3..], bgColor, ConsoleColor.DarkCyan),
                    'g' => new(value[3..], bgColor, ConsoleColor.DarkGreen),
                    'm' => new(value[3..], bgColor, ConsoleColor.DarkMagenta),
                    'r' => new(value[3..], bgColor, ConsoleColor.DarkRed),
                    'b' => new(value[3..], bgColor, ConsoleColor.DarkBlue),
                    'd' => new(value[3..], bgColor, ConsoleColor.DarkGray),
                    'n' => new(value[3..], bgColor, ConsoleColor.Black),
                    ' ' => new(value[3..], bgColor, NoColor),
                    _ => new(value, NoColor, NoColor),
                };
            }

            return new(value, NoColor, NoColor);
        }
    }
}
