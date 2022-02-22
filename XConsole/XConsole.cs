using System.Diagnostics;

namespace System
{
    public static class XConsole
    {
        private static readonly object _lock = new();
        private static readonly bool _isWindows = OperatingSystem.IsWindows();
        private static readonly string _newLine = Environment.NewLine;

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

        #region Read

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
                            pass = pass[0..^1];
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

            return Console.ReadLine() ?? string.Empty;
        }

        #endregion

        #region Write

        public static (XConsolePosition Start, XConsolePosition End) Write(params string[] values)
        {
            if (values.Length == 0)
            {
                int left, top;

                lock (_lock)
                {
                    left = Console.CursorLeft;
                    top = Console.WindowTop;
                }

                return (new(left: left, top: top), new(left: left, top: top));
            }

            return WriteBase(values);
        }

        public static (XConsolePosition Start, XConsolePosition End) WriteLine(params string[] values)
        {
            if (values.Length == 0)
            {
                int left, top;

                lock (_lock)
                {
                    left = Console.CursorLeft;
                    top = Console.WindowTop;
                    Console.WriteLine();
                }

                return (new(left: left, top: top), new(left: 0, top: top + 1));
            }

            values[^1] += _newLine;
            return WriteBase(values);
        }

        private static (XConsolePosition Start, XConsolePosition End) WriteBase(IReadOnlyList<string> values)
        {
            Debug.Assert(values.Count > 0);

            var items = values.Select(XConsoleItem.Parse).Where(i => i.Value.Length > 0).ToList();

            lock (_lock)
            {
#pragma warning disable CA1416 // Validate platform compatibility
                var origVisible = _isWindows && Console.CursorVisible;
#pragma warning restore CA1416 // Validate platform compatibility

                var startLeft = Console.CursorLeft;
                var startTop = Console.CursorTop;
                Console.CursorVisible = false;

                foreach (var item in items)
                    WriteItem(item);

                Console.CursorVisible = origVisible;
                var endLeft = Console.CursorLeft;
                var endTop = Console.CursorTop;
                return (new(left: startLeft, top: startTop), new(left: endLeft, top: endTop));
            }
        }

        internal static XConsolePosition WriteToPosition(IReadOnlyList<string> values, int left, int top)
        {
            Debug.Assert(values.Count > 0);

            var items = values.Select(XConsoleItem.Parse).Where(i => i.Value.Length > 0).ToList();

            lock (_lock)
            {
#pragma warning disable CA1416 // Validate platform compatibility
                var origVisible = _isWindows && Console.CursorVisible;
#pragma warning restore CA1416 // Validate platform compatibility

                var origLeft = Console.CursorLeft;
                var origTop = Console.CursorTop;
                Console.CursorVisible = false;

                try
                {
                    Console.SetCursorPosition(left, top);
                }
                catch (ArgumentOutOfRangeException)
                {
                    // do nothing
                }

                foreach (var item in items)
                    WriteItem(item);

                var endLeft = Console.CursorLeft;
                var endTop = Console.CursorTop;
                Console.SetCursorPosition(origLeft, origTop);
                Console.CursorVisible = origVisible;
                return new(left: endLeft, top: endTop);
            }
        }

        private static void WriteItem(XConsoleItem item)
        {
            Debug.Assert(item.Value.Length > 0);

            if (item.BgColor == XConsoleItem.NoColor)
            {
                if (item.FgColor == XConsoleItem.NoColor)
                {
                    Console.Write(item.Value);
                }
                else
                {
                    var origFgColor = Console.ForegroundColor;
                    Console.ForegroundColor = item.FgColor;
                    Console.Write(item.Value);
                    Console.ForegroundColor = origFgColor;
                }
            }
            else
            {
                if (item.FgColor == XConsoleItem.NoColor)
                {
                    var origBgColor = Console.BackgroundColor;
                    Console.BackgroundColor = item.BgColor;
                    Console.Write(item.Value);
                    Console.BackgroundColor = origBgColor;
                }
                else
                {
                    var origBgColor = Console.BackgroundColor;
                    var origFgColor = Console.ForegroundColor;
                    Console.BackgroundColor = item.BgColor;
                    Console.ForegroundColor = item.FgColor;
                    Console.Write(item.Value);
                    Console.BackgroundColor = origBgColor;
                    Console.ForegroundColor = origFgColor;
                }
            }
        }

        #endregion
    }
}
