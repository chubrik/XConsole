namespace System
{
    internal struct XConsoleItem
    {
        public const ConsoleColor NoColor = (ConsoleColor)(-1);

        public string Value;
        public readonly ConsoleColor BackColor;
        public readonly ConsoleColor ForeColor;

        private XConsoleItem(string value, ConsoleColor backColor, ConsoleColor foreColor)
        {
            Value = value;
            BackColor = backColor;
            ForeColor = foreColor;
        }

        public static XConsoleItem Parse(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return new(string.Empty, NoColor, NoColor);

            if (value[0] == '`')
                return new(value.Substring(1), NoColor, NoColor);

            if (value.Length == 1)
                return new(value, NoColor, NoColor);

            if (value[1] == '`')
                return value[0] switch
                {
                    'W' => new(value.Substring(2), NoColor, ConsoleColor.White),
                    'Y' => new(value.Substring(2), NoColor, ConsoleColor.Yellow),
                    'C' => new(value.Substring(2), NoColor, ConsoleColor.Cyan),
                    'G' => new(value.Substring(2), NoColor, ConsoleColor.Green),
                    'M' => new(value.Substring(2), NoColor, ConsoleColor.Magenta),
                    'R' => new(value.Substring(2), NoColor, ConsoleColor.Red),
                    'B' => new(value.Substring(2), NoColor, ConsoleColor.Blue),
                    'w' => new(value.Substring(2), NoColor, ConsoleColor.Gray),
                    'y' => new(value.Substring(2), NoColor, ConsoleColor.DarkYellow),
                    'c' => new(value.Substring(2), NoColor, ConsoleColor.DarkCyan),
                    'g' => new(value.Substring(2), NoColor, ConsoleColor.DarkGreen),
                    'm' => new(value.Substring(2), NoColor, ConsoleColor.DarkMagenta),
                    'r' => new(value.Substring(2), NoColor, ConsoleColor.DarkRed),
                    'b' => new(value.Substring(2), NoColor, ConsoleColor.DarkBlue),
                    'd' => new(value.Substring(2), NoColor, ConsoleColor.DarkGray),
                    'n' => new(value.Substring(2), NoColor, ConsoleColor.Black),
                    _ => new(value, NoColor, NoColor),
                };

            if (value.Length == 2)
                return new(value, NoColor, NoColor);

            if (value[2] == '`')
            {
                ConsoleColor backColor;

                switch (value[0])
                {
                    case 'W': backColor = ConsoleColor.White; break;
                    case 'Y': backColor = ConsoleColor.Yellow; break;
                    case 'C': backColor = ConsoleColor.Cyan; break;
                    case 'G': backColor = ConsoleColor.Green; break;
                    case 'M': backColor = ConsoleColor.Magenta; break;
                    case 'R': backColor = ConsoleColor.Red; break;
                    case 'B': backColor = ConsoleColor.Blue; break;
                    case 'w': backColor = ConsoleColor.Gray; break;
                    case 'y': backColor = ConsoleColor.DarkYellow; break;
                    case 'c': backColor = ConsoleColor.DarkCyan; break;
                    case 'g': backColor = ConsoleColor.DarkGreen; break;
                    case 'm': backColor = ConsoleColor.DarkMagenta; break;
                    case 'r': backColor = ConsoleColor.DarkRed; break;
                    case 'b': backColor = ConsoleColor.DarkBlue; break;
                    case 'd': backColor = ConsoleColor.DarkGray; break;
                    case 'n': backColor = ConsoleColor.Black; break;
                    default: return new(value, NoColor, NoColor);
                }

                return value[1] switch
                {
                    'W' => new(value.Substring(3), backColor, ConsoleColor.White),
                    'Y' => new(value.Substring(3), backColor, ConsoleColor.Yellow),
                    'C' => new(value.Substring(3), backColor, ConsoleColor.Cyan),
                    'G' => new(value.Substring(3), backColor, ConsoleColor.Green),
                    'M' => new(value.Substring(3), backColor, ConsoleColor.Magenta),
                    'R' => new(value.Substring(3), backColor, ConsoleColor.Red),
                    'B' => new(value.Substring(3), backColor, ConsoleColor.Blue),
                    'w' => new(value.Substring(3), backColor, ConsoleColor.Gray),
                    'y' => new(value.Substring(3), backColor, ConsoleColor.DarkYellow),
                    'c' => new(value.Substring(3), backColor, ConsoleColor.DarkCyan),
                    'g' => new(value.Substring(3), backColor, ConsoleColor.DarkGreen),
                    'm' => new(value.Substring(3), backColor, ConsoleColor.DarkMagenta),
                    'r' => new(value.Substring(3), backColor, ConsoleColor.DarkRed),
                    'b' => new(value.Substring(3), backColor, ConsoleColor.DarkBlue),
                    'd' => new(value.Substring(3), backColor, ConsoleColor.DarkGray),
                    'n' => new(value.Substring(3), backColor, ConsoleColor.Black),
                    ' ' => new(value.Substring(3), backColor, NoColor),
                    _ => new(value, NoColor, NoColor),
                };
            }

            return new(value, NoColor, NoColor);
        }
    }
}
