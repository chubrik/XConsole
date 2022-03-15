using System.Diagnostics;

namespace System
{
    public static class XConsole
    {
        private static readonly object _lock = new();
        private static readonly string _newLine = Environment.NewLine;
        private static bool _cursorVisible = OperatingSystem.IsWindows() && Console.CursorVisible;
        private static int _maxTop = Console.BufferHeight - 1;
        internal static long ShiftTop = 0;

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

        public static (XConsolePosition Begin, XConsolePosition End) Write(params string[] values)
        {
            return WriteBase(values, isWriteLine: false);
        }

        public static (XConsolePosition Begin, XConsolePosition End) WriteLine(params string[] values)
        {
            return WriteBase(values, isWriteLine: true);
        }

        private static (XConsolePosition Begin, XConsolePosition End) WriteBase(IReadOnlyList<string> logValues, bool isWriteLine)
        {
            var logItems = logValues.Count > 0
                ? logValues.Select(XConsoleItem.Parse).Where(i => i.Value.Length > 0).ToList()
                : _noItems;

            int beginLeft, beginTop, endLeft, endTop;
            long beginShiftTop, endShiftTop;
            var getPinValues = _getPinValues;

            if (getPinValues == null)
            {
                lock (_lock)
                {
                    beginLeft = Console.CursorLeft;
                    beginTop = Console.CursorTop;
                    beginShiftTop = ShiftTop;
                    Console.CursorVisible = false;

                    foreach (var logItem in logItems)
                        WriteItem(logItem);

                    endLeft = Console.CursorLeft;
                    endTop = Console.CursorTop;

                    if (isWriteLine)
                        Console.WriteLine();

                    Console.CursorVisible = _cursorVisible;

                    if (endTop == _maxTop)
                    {
                        var logNewLineCount = logItems.SelectMany(i => i.Value).Count(i => i == '\n');

                        var shift = isWriteLine
                            ? beginTop + logNewLineCount + 1 - endTop
                            : beginTop + logNewLineCount - endTop;

                        ShiftTop += shift;
                        beginTop -= shift;
                        endTop -= shift;
                    }

                    endShiftTop = ShiftTop;
                }
            }
            else
            {
                var pinValues = getPinValues();

                var pinItems = pinValues.Count > 0
                    ? pinValues.Select(XConsoleItem.Parse).Where(i => i.Value.Length > 0).ToList()
                    : _noItems;

                var spaces = _newLine + new string(' ', Console.BufferWidth);

                lock (_lock)
                {
                    beginLeft = Console.CursorLeft;
                    beginTop = Console.CursorTop;
                    beginShiftTop = ShiftTop;
                    Console.CursorVisible = false;

                    if (_getPinValues == null)
                    {
                        foreach (var logItem in logItems)
                            WriteItem(logItem);

                        endLeft = Console.CursorLeft;
                        endTop = Console.CursorTop;

                        if (isWriteLine)
                            Console.WriteLine();

                        if (endTop == _maxTop)
                        {
                            var logNewLineCount = logItems.SelectMany(i => i.Value).Count(i => i == '\n');

                            var shift = isWriteLine
                                ? beginTop + logNewLineCount + 1 - endTop
                                : beginTop + logNewLineCount - endTop;

                            ShiftTop += shift;
                            beginTop -= shift;
                            endTop -= shift;
                        }
                    }
                    else
                    {
                        for (var i = 0; i < _pinHeight; i++)
                            Console.Write(spaces);

                        Console.SetCursorPosition(beginLeft, beginTop);

                        foreach (var logItem in logItems)
                            WriteItem(logItem);

                        endLeft = Console.CursorLeft;
                        endTop = Console.CursorTop;
                        Console.WriteLine();

                        if (isWriteLine)
                        {
                            Console.WriteLine();

                            foreach (var pinItem in pinItems)
                                WriteItem(pinItem);

                            if (Console.CursorTop == _maxTop)
                            {
                                var logNewLineCount = logItems.SelectMany(i => i.Value).Count(i => i == '\n');
                                _pinHeight = 1 + pinItems.SelectMany(i => i.Value).Count(i => i == '\n');
                                var shift = beginTop + logNewLineCount + 1 + _pinHeight - _maxTop;
                                ShiftTop += shift;
                                beginTop -= shift;
                                endTop -= shift;
                            }
                            else
                                _pinHeight = Console.CursorTop - (endTop + 1);

                            Console.SetCursorPosition(0, endTop + 1);
                        }
                        else
                        {
                            foreach (var pinItem in pinItems)
                                WriteItem(pinItem);

                            if (Console.CursorTop == _maxTop)
                            {
                                var logNewLineCount = logItems.SelectMany(i => i.Value).Count(i => i == '\n');
                                _pinHeight = 1 + pinItems.SelectMany(i => i.Value).Count(i => i == '\n');
                                var shift = beginTop + logNewLineCount + _pinHeight - _maxTop;
                                ShiftTop += shift;
                                beginTop -= shift;
                                endTop -= shift;
                            }
                            else
                                _pinHeight = Console.CursorTop - endTop;

                            Console.SetCursorPosition(endLeft, endTop);
                        }
                    }

                    Console.CursorVisible = _cursorVisible;
                    endShiftTop = ShiftTop;
                }
            }

            return (
                new(left: beginLeft, top: beginTop, shiftTop: beginShiftTop),
                new(left: endLeft, top: endTop, shiftTop: endShiftTop)
            );
        }

        internal static XConsolePosition WriteToPosition(IReadOnlyList<string> values, XConsolePosition position)
        {
            Debug.Assert(values.Count > 0);

            var items = values.Select(XConsoleItem.Parse).Where(i => i.Value.Length > 0).ToList();
            int endLeft, endTop;
            long endShiftTop;

            lock (_lock)
            {
                var origLeft = Console.CursorLeft;
                var origTop = Console.CursorTop;
                Console.CursorVisible = false;
                var shiftedTop = (int)(position.Top + position.ShiftTop - ShiftTop);

                try
                {
                    Console.SetCursorPosition(position.Left, shiftedTop);
                }
                catch (ArgumentOutOfRangeException)
                {
                    // do nothing
                    Console.CursorVisible = _cursorVisible;
                    return position;
                }

                foreach (var item in items)
                    WriteItem(item);

                endLeft = Console.CursorLeft;
                endTop = Console.CursorTop;

                if (endTop == _maxTop)
                {
                    var newLineCount = items.SelectMany(i => i.Value).Count(i => i == '\n');
                    var shift = origTop + newLineCount - endTop;
                    ShiftTop += shift;
                    origTop -= shift;
                    endTop -= shift;
                }

                Console.SetCursorPosition(origLeft, origTop);
                Console.CursorVisible = _cursorVisible;
                endShiftTop = ShiftTop;
            }

            return new(left: endLeft, top: endTop, shiftTop: endShiftTop);
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

        #region Pin

        private static int _pinHeight = 0;
        private static Func<IReadOnlyList<string>>? _getPinValues = null;
        private static readonly IReadOnlyList<XConsoleItem> _noItems = Array.Empty<XConsoleItem>();

        public static void Pin(params string[] values)
        {
            _getPinValues = () => values;
            UpdatePin();
        }

        public static void Pin(Func<string> getValue)
        {
            _getPinValues = () => new[] { getValue() };
            UpdatePin();
        }

        public static void Pin(Func<IReadOnlyList<string>> getValues)
        {
            _getPinValues = getValues;
            UpdatePin();
        }

        public static void UpdatePin()
        {
            var getPinValues = _getPinValues;

            if (getPinValues == null)
                return;

            var pinValues = getPinValues();

            var pinItems = pinValues.Count > 0
                ? pinValues.Select(XConsoleItem.Parse).Where(i => i.Value.Length > 0).ToList()
                : _noItems;

            var spaces = _newLine + new string(' ', Console.BufferWidth);

            lock (_lock)
            {
                if (_getPinValues == null)
                    return;

                var origLeft = Console.CursorLeft;
                var origTop = Console.CursorTop;
                Console.CursorVisible = false;

                for (var i = 0; i < _pinHeight; i++)
                    Console.Write(spaces);

                Console.SetCursorPosition(origLeft, origTop);
                Console.WriteLine();

                foreach (var pinItem in pinItems)
                    WriteItem(pinItem);

                if (Console.CursorTop == _maxTop)
                {
                    _pinHeight = 1 + pinItems.SelectMany(i => i.Value).Count(i => i == '\n');
                    var shift = origTop + _pinHeight - _maxTop;
                    ShiftTop += shift;
                    origTop -= shift;
                }
                else
                    _pinHeight = Console.CursorTop - origTop;

                Console.SetCursorPosition(origLeft, origTop);
                Console.CursorVisible = _cursorVisible;
            }
        }

        public static void Unpin()
        {
            var spaces = _newLine + new string(' ', Console.BufferWidth);

            lock (_lock)
            {
                if (_getPinValues == null)
                    return;

                _getPinValues = null;
                var origLeft = Console.CursorLeft;
                var origTop = Console.CursorTop;
                Console.CursorVisible = false;

                for (var i = 0; i < _pinHeight; i++)
                    Console.Write(spaces);

                Console.SetCursorPosition(origLeft, origTop);
                Console.CursorVisible = _cursorVisible;
                _pinHeight = 0;
            }
        }

        #endregion
    }
}
