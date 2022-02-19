using System.Diagnostics;

namespace System
{
    public static class XConsole
    {
        private static readonly object _lock = new();
        private static readonly ConsoleColor _noColor = (ConsoleColor)(-1);

        public static ConsoleColor BackgroundColor
        {
            get { lock (_lock) return Console.BackgroundColor; }
            set { lock (_lock) Console.BackgroundColor = value; }
        }

        public static ConsoleColor ForegroundColor
        {
            get { lock (_lock) return Console.ForegroundColor; }
            set { lock (_lock) Console.ForegroundColor = value; }
        }

        public static int WindowWidth => Console.WindowWidth;

        #region ReadKey, ReadLine

        public static ConsoleKeyInfo ReadKey() { lock (_lock) return Console.ReadKey(); }
        public static ConsoleKeyInfo ReadKey(bool intercept) { lock (_lock) return Console.ReadKey(intercept); }

        public static string ReadLine() { lock (_lock) return Console.ReadLine() ?? string.Empty; }

        public static string ReadLine(bool secure)
        {
            if (secure)
            {
                var pass = string.Empty;
                ConsoleKeyInfo keyInfo;
                ConsoleKey key;

                lock (_lock)
                    do
                    {
                        keyInfo = Console.ReadKey(intercept: true);
                        key = keyInfo.Key;

                        if (key == ConsoleKey.Backspace && pass.Length > 0)
                        {
                            Console.Write("\b \b");
                            pass = pass.Substring(0, pass.Length - 1);
                        }
                        else if (!char.IsControl(keyInfo.KeyChar))
                        {
                            Console.Write('*');
                            pass += keyInfo.KeyChar;
                        }
                    }
                    while (key != ConsoleKey.Enter);

                return pass;
            }
            else
                return ReadLine();
        }

        #endregion

        #region Write, WriteLine

        public static void Write(params string[] values) => Write(values, isNewLineOnEnd: false);

        public static void WriteLine(params string[] values) => Write(values, isNewLineOnEnd: true);

        private static void Write(IReadOnlyList<string> values, bool isNewLineOnEnd)
        {
            if (values.Count > 0)
            {
                var logItems = values.Select(Parse).Where(i => i.Value.Length > 0).ToList();

                lock (_lock)
                {
                    if (logItems.Count > 0)
                        foreach (var logItem in logItems)
                            WriteCore(logItem);

                    if (isNewLineOnEnd)
                        Console.WriteLine();
                }
            }
            else if (isNewLineOnEnd)
                lock (_lock)
                    Console.WriteLine();
        }

        private static void WriteCore(Item item)
        {
            Debug.Assert(item.Value.Length > 0);

            if (item.BgColor != _noColor)
            {
                if (item.FgColor != _noColor)
                {
                    var origBgColor = Console.BackgroundColor;
                    var origFgColor = Console.ForegroundColor;
                    Console.BackgroundColor = item.BgColor;
                    Console.ForegroundColor = item.FgColor;
                    Console.Write(item.Value);
                    Console.BackgroundColor = origBgColor;
                    Console.ForegroundColor = origFgColor;
                }
                else
                {
                    var origBgColor = Console.BackgroundColor;
                    Console.BackgroundColor = item.BgColor;
                    Console.Write(item.Value);
                    Console.BackgroundColor = origBgColor;
                }
            }
            else
            if (item.FgColor != _noColor)
            {
                var origFgColor = Console.ForegroundColor;
                Console.ForegroundColor = item.FgColor;
                Console.Write(item.Value);
                Console.ForegroundColor = origFgColor;
            }
            else
                Console.Write(item.Value);
        }

        private static Item Parse(string value)
        {
            if (value.Length == 0)
                return new(string.Empty, _noColor, _noColor);

            if (value[0] == '`')
                return new(value.Substring(1), _noColor, _noColor);

            if (value.Length == 1)
                return new(value, _noColor, _noColor);

            if (value[1] == '`')
                return value[0] switch
                {
                    'W' => new(value.Substring(2), _noColor, ConsoleColor.White),
                    'Y' => new(value.Substring(2), _noColor, ConsoleColor.Yellow),
                    'C' => new(value.Substring(2), _noColor, ConsoleColor.Cyan),
                    'G' => new(value.Substring(2), _noColor, ConsoleColor.Green),
                    'M' => new(value.Substring(2), _noColor, ConsoleColor.Magenta),
                    'R' => new(value.Substring(2), _noColor, ConsoleColor.Red),
                    'B' => new(value.Substring(2), _noColor, ConsoleColor.Blue),
                    'w' => new(value.Substring(2), _noColor, ConsoleColor.Gray),
                    'y' => new(value.Substring(2), _noColor, ConsoleColor.DarkYellow),
                    'c' => new(value.Substring(2), _noColor, ConsoleColor.DarkCyan),
                    'g' => new(value.Substring(2), _noColor, ConsoleColor.DarkGreen),
                    'm' => new(value.Substring(2), _noColor, ConsoleColor.DarkMagenta),
                    'r' => new(value.Substring(2), _noColor, ConsoleColor.DarkRed),
                    'b' => new(value.Substring(2), _noColor, ConsoleColor.DarkBlue),
                    'd' => new(value.Substring(2), _noColor, ConsoleColor.DarkGray),
                    'n' => new(value.Substring(2), _noColor, ConsoleColor.Black),
                    _ => new(value, _noColor, _noColor),
                };

            if (value.Length == 2)
                return new(value, _noColor, _noColor);

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
                    default: return new(value, _noColor, _noColor);
                }

                return value[1] switch
                {
                    'W' => new(value.Substring(3), bgColor, ConsoleColor.White),
                    'Y' => new(value.Substring(3), bgColor, ConsoleColor.Yellow),
                    'C' => new(value.Substring(3), bgColor, ConsoleColor.Cyan),
                    'G' => new(value.Substring(3), bgColor, ConsoleColor.Green),
                    'M' => new(value.Substring(3), bgColor, ConsoleColor.Magenta),
                    'R' => new(value.Substring(3), bgColor, ConsoleColor.Red),
                    'B' => new(value.Substring(3), bgColor, ConsoleColor.Blue),
                    'w' => new(value.Substring(3), bgColor, ConsoleColor.Gray),
                    'y' => new(value.Substring(3), bgColor, ConsoleColor.DarkYellow),
                    'c' => new(value.Substring(3), bgColor, ConsoleColor.DarkCyan),
                    'g' => new(value.Substring(3), bgColor, ConsoleColor.DarkGreen),
                    'm' => new(value.Substring(3), bgColor, ConsoleColor.DarkMagenta),
                    'r' => new(value.Substring(3), bgColor, ConsoleColor.DarkRed),
                    'b' => new(value.Substring(3), bgColor, ConsoleColor.DarkBlue),
                    'd' => new(value.Substring(3), bgColor, ConsoleColor.DarkGray),
                    'n' => new(value.Substring(3), bgColor, ConsoleColor.Black),
                    ' ' => new(value.Substring(3), bgColor, _noColor),
                    _ => new(value, _noColor, _noColor),
                };
            }

            return new(value, _noColor, _noColor);
        }

        private struct Item
        {
            public string Value;
            public readonly ConsoleColor BgColor;
            public readonly ConsoleColor FgColor;

            public Item(string value, ConsoleColor bgColor, ConsoleColor fgColor)
            {
                Value = value;
                BgColor = bgColor;
                FgColor = fgColor;
            }
        }

        #endregion
    }
}
