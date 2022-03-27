using System.Diagnostics;
using System.Text;

#if !NETSTANDARD
using System.Runtime.Versioning;
#endif

namespace System
{
    public static class XConsole
    {
        private static readonly object _lock = new();
        private static readonly string _newLine = Environment.NewLine;

#if !NETSTANDARD
        private static bool _cursorVisible = OperatingSystem.IsWindows() && Console.CursorVisible;
#else
        private static bool _cursorVisible = Console.CursorVisible;
#endif

        private static int _maxTop = Console.BufferHeight - 1;
        internal static long ShiftTop = 0;

        public static void Lock(Action action)
        {
            lock (_lock)
                action();
        }

        #region ReadLine

#if !NETSTANDARD
        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("browser")]
#endif
        public static string ReadLine()
        {
            lock (_lock)
                return Console.ReadLine() ?? string.Empty;
        }

#if !NETSTANDARD
        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("browser")]
#endif
        public static string ReadLine(XConsoleReadLineMode mode, char maskChar = '*')
        {
            switch (mode)
            {
                case XConsoleReadLineMode.Default:
                    return ReadLine();

                case XConsoleReadLineMode.Masked:
                case XConsoleReadLineMode.Hidden:
                    var isMaskedMode = mode == XConsoleReadLineMode.Masked;
                    var text = string.Empty;
                    ConsoleKeyInfo keyInfo;

                    lock (_lock)
                    {
                        do
                        {
                            keyInfo = Console.ReadKey(intercept: true);

                            if (keyInfo.Key == ConsoleKey.Backspace && text.Length > 0)
                            {
                                if (isMaskedMode)
                                    Console.Write("\b \b");

                                text = text.Substring(0, text.Length - 1);
                            }
                            else if (!char.IsControl(keyInfo.KeyChar))
                            {
                                if (isMaskedMode)
                                    Console.Write(maskChar);

                                text += keyInfo.KeyChar;
                            }
                        }
                        while (keyInfo.Key != ConsoleKey.Enter);

                        Console.WriteLine();
                    }

                    return text;

                default:
                    throw new ArgumentOutOfRangeException(nameof(mode));
            }
        }

        #endregion

        #region Write, WriteLine

        public static (XConsolePosition Begin, XConsolePosition End) Write(string? value)
        {
            return WriteBase(new[] { value }, isWriteLine: false);
        }

        public static (XConsolePosition Begin, XConsolePosition End) Write(params string?[] values)
        {
            return WriteBase(values, isWriteLine: false);
        }

        public static (XConsolePosition Begin, XConsolePosition End) WriteLine(string? value)
        {
            return WriteBase(new[] { value }, isWriteLine: true);
        }

        public static (XConsolePosition Begin, XConsolePosition End) WriteLine(params string?[] values)
        {
            return WriteBase(values, isWriteLine: true);
        }

        private static (XConsolePosition Begin, XConsolePosition End) WriteBase(IReadOnlyList<string?> logValues, bool isWriteLine)
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

                var spaces = _newLine + new string(' ', Console.BufferWidth - 1);

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

        internal static XConsolePosition WriteToPosition(IReadOnlyList<string?> values, XConsolePosition position)
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

            if (item.BackColor == XConsoleItem.NoColor)
            {
                if (item.ForeColor == XConsoleItem.NoColor)
                {
                    Console.Write(item.Value);
                }
                else
                {
                    var origForeColor = Console.ForegroundColor;
                    Console.ForegroundColor = item.ForeColor;
                    Console.Write(item.Value);
                    Console.ForegroundColor = origForeColor;
                }
            }
            else
            {
                if (item.ForeColor == XConsoleItem.NoColor)
                {
                    var origBackColor = Console.BackgroundColor;
                    Console.BackgroundColor = item.BackColor;
                    Console.Write(item.Value);
                    Console.BackgroundColor = origBackColor;
                }
                else
                {
                    var origBackColor = Console.BackgroundColor;
                    var origForeColor = Console.ForegroundColor;
                    Console.BackgroundColor = item.BackColor;
                    Console.ForegroundColor = item.ForeColor;
                    Console.Write(item.Value);
                    Console.BackgroundColor = origBackColor;
                    Console.ForegroundColor = origForeColor;
                }
            }
        }

        public static (XConsolePosition Begin, XConsolePosition End) Write(bool value) => Write(value.ToString());
        public static (XConsolePosition Begin, XConsolePosition End) Write(char value) => Write(value.ToString());
        public static (XConsolePosition Begin, XConsolePosition End) Write(char[]? buffer) => Write(buffer?.ToString());
        public static (XConsolePosition Begin, XConsolePosition End) Write(char[] buffer, int index, int count) => Write(buffer.ToString()?.Substring(index, count));
        public static (XConsolePosition Begin, XConsolePosition End) Write(decimal value) => Write(value.ToString());
        public static (XConsolePosition Begin, XConsolePosition End) Write(double value) => Write(value.ToString());
        public static (XConsolePosition Begin, XConsolePosition End) Write(int value) => Write(value.ToString());
        public static (XConsolePosition Begin, XConsolePosition End) Write(long value) => Write(value.ToString());
        public static (XConsolePosition Begin, XConsolePosition End) Write(object? value) => Write(value?.ToString());
        public static (XConsolePosition Begin, XConsolePosition End) Write(float value) => Write(value.ToString());
        public static (XConsolePosition Begin, XConsolePosition End) Write(string format, object? arg0) => Write(string.Format(format, arg0));
        public static (XConsolePosition Begin, XConsolePosition End) Write(string format, object? arg0, object? arg1) => Write(string.Format(format, arg0, arg1));
        public static (XConsolePosition Begin, XConsolePosition End) Write(string format, object? arg0, object? arg1, object? arg2) => Write(string.Format(format, arg0, arg1, arg2));
        public static (XConsolePosition Begin, XConsolePosition End) Write(string format, params object?[]? arg) => Write(string.Format(format, arg ?? Array.Empty<object?>()));
        //[CLSCompliant(false)]
        public static (XConsolePosition Begin, XConsolePosition End) Write(uint value) => Write(value.ToString());
        //[CLSCompliant(false)]
        public static (XConsolePosition Begin, XConsolePosition End) Write(ulong value) => Write(value.ToString());

        public static (XConsolePosition Begin, XConsolePosition End) WriteLine(bool value) => WriteLine(value.ToString());
        public static (XConsolePosition Begin, XConsolePosition End) WriteLine(char value) => WriteLine(value.ToString());
        public static (XConsolePosition Begin, XConsolePosition End) WriteLine(char[]? buffer) => WriteLine(buffer?.ToString());
        public static (XConsolePosition Begin, XConsolePosition End) WriteLine(char[] buffer, int index, int count) => WriteLine(buffer.ToString()?.Substring(index, count));
        public static (XConsolePosition Begin, XConsolePosition End) WriteLine(decimal value) => WriteLine(value.ToString());
        public static (XConsolePosition Begin, XConsolePosition End) WriteLine(double value) => WriteLine(value.ToString());
        public static (XConsolePosition Begin, XConsolePosition End) WriteLine(int value) => WriteLine(value.ToString());
        public static (XConsolePosition Begin, XConsolePosition End) WriteLine(long value) => WriteLine(value.ToString());
        public static (XConsolePosition Begin, XConsolePosition End) WriteLine(object? value) => WriteLine(value?.ToString());
        public static (XConsolePosition Begin, XConsolePosition End) WriteLine(float value) => WriteLine(value.ToString());
        public static (XConsolePosition Begin, XConsolePosition End) WriteLine(string format, object? arg0) => WriteLine(string.Format(format, arg0));
        public static (XConsolePosition Begin, XConsolePosition End) WriteLine(string format, object? arg0, object? arg1) => WriteLine(string.Format(format, arg0, arg1));
        public static (XConsolePosition Begin, XConsolePosition End) WriteLine(string format, object? arg0, object? arg1, object? arg2) => WriteLine(string.Format(format, arg0, arg1, arg2));
        public static (XConsolePosition Begin, XConsolePosition End) WriteLine(string format, params object?[]? arg) => WriteLine(string.Format(format, arg ?? Array.Empty<object?>()));
        //[CLSCompliant(false)]
        public static (XConsolePosition Begin, XConsolePosition End) WriteLine(uint value) => WriteLine(value.ToString());
        //[CLSCompliant(false)]
        public static (XConsolePosition Begin, XConsolePosition End) WriteLine(ulong value) => WriteLine(value.ToString());

        #endregion

        #region Pin

        private static int _pinHeight = 0;
        private static Func<IReadOnlyList<string?>>? _getPinValues = null;
        private static readonly IReadOnlyList<XConsoleItem> _noItems = Array.Empty<XConsoleItem>();

        public static void Pin(params string?[] values)
        {
            _getPinValues = () => values;
            UpdatePin();
        }

        public static void Pin(Func<string?> getValue)
        {
            _getPinValues = () => new[] { getValue() };
            UpdatePin();
        }

        public static void Pin(Func<IReadOnlyList<string?>> getValues)
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

            var spaces = _newLine + new string(' ', Console.BufferWidth - 1);

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
            var spaces = _newLine + new string(' ', Console.BufferWidth - 1);

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

        #region Proxy

#pragma warning disable IDE0079 // Remove unnecessary suppression

#if !NETSTANDARD
        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("browser")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
#endif
        public static ConsoleColor BackgroundColor
        {
            get
            {
                lock (_lock)
                    return Console.BackgroundColor;
            }
            set
            {
                lock (_lock)
                    Console.BackgroundColor = value;
            }
        }

        public static int BufferHeight
        {
#if !NETSTANDARD
            [UnsupportedOSPlatform("android")]
            [UnsupportedOSPlatform("browser")]
            [UnsupportedOSPlatform("ios")]
            [UnsupportedOSPlatform("tvos")]
#endif
            get
            {
                lock (_lock)
                    return Console.BufferHeight;
            }
#if !NETSTANDARD
            [SupportedOSPlatform("windows")]
#endif
            set
            {
                lock (_lock)
                {
#pragma warning disable CA1416 // Validate platform compatibility
                    Console.BufferHeight = value;
#pragma warning restore CA1416 // Validate platform compatibility
                    _maxTop = value - 1;
                }
            }
        }

        public static int BufferWidth
        {
#if !NETSTANDARD
            [UnsupportedOSPlatform("android")]
            [UnsupportedOSPlatform("browser")]
            [UnsupportedOSPlatform("ios")]
            [UnsupportedOSPlatform("tvos")]
#endif
            get
            {
                lock (_lock)
                    return Console.BufferWidth;
            }
#if !NETSTANDARD
            [SupportedOSPlatform("windows")]
#endif
            set
            {
                lock (_lock)
#pragma warning disable CA1416 // Validate platform compatibility
                    Console.BufferWidth = value;
#pragma warning restore CA1416 // Validate platform compatibility
            }
        }

#if !NETSTANDARD
        [SupportedOSPlatform("windows")]
#endif
        public static bool CapsLock => Console.CapsLock;

#if !NETSTANDARD
        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("browser")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
#endif
        public static int CursorLeft
        {
            get
            {
                lock (_lock)
                    return Console.CursorLeft;
            }
            set
            {
                lock (_lock)
                    Console.CursorLeft = value;
            }
        }

        public static int CursorSize
        {
#if !NETSTANDARD
            [UnsupportedOSPlatform("android")]
            [UnsupportedOSPlatform("browser")]
            [UnsupportedOSPlatform("ios")]
            [UnsupportedOSPlatform("tvos")]
#endif
            get => Console.CursorSize;
#if !NETSTANDARD
            [SupportedOSPlatform("windows")]
#endif
#pragma warning disable CA1416 // Validate platform compatibility
            set => Console.CursorSize = value;
#pragma warning restore CA1416 // Validate platform compatibility
        }

#if !NETSTANDARD
        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("browser")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
#endif
        public static int CursorTop
        {
            get
            {
                lock (_lock)
                    return Console.CursorTop;
            }
            set
            {
                lock (_lock)
                    Console.CursorTop = value;
            }
        }

        public static bool CursorVisible
        {
#if !NETSTANDARD
            [SupportedOSPlatform("windows")]
#endif
            get
            {
                lock (_lock)
                    return _cursorVisible;
            }
#if !NETSTANDARD
            [UnsupportedOSPlatform("android")]
            [UnsupportedOSPlatform("browser")]
            [UnsupportedOSPlatform("ios")]
            [UnsupportedOSPlatform("tvos")]
#endif
            set
            {
                lock (_lock)
                {
                    Console.CursorVisible = value;
                    _cursorVisible = value;
                }
            }
        }

        public static TextWriter Error => Console.Error;

#if !NETSTANDARD
        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("browser")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
#endif
        public static ConsoleColor ForegroundColor
        {
            get
            {
                lock (_lock)
                    return Console.ForegroundColor;
            }
            set
            {
                lock (_lock)
                    Console.ForegroundColor = value;
            }
        }

#if !NETSTANDARD
        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("browser")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
#endif
        public static TextReader In => Console.In;

#if !NETSTANDARD
        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("browser")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
#endif
        public static Encoding InputEncoding
        {
            get
            {
                lock (_lock)
                    return Console.InputEncoding;
            }
            set
            {
                lock (_lock)
                    Console.InputEncoding = value;
            }
        }

        public static bool IsErrorRedirected => Console.IsErrorRedirected;

        public static bool IsInputRedirected => Console.IsInputRedirected;

        public static bool IsOutputRedirected => Console.IsOutputRedirected;

        public static bool KeyAvailable => Console.KeyAvailable;

#if !NETSTANDARD
        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("browser")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
#endif
        public static int LargestWindowHeight => Console.LargestWindowHeight;

#if !NETSTANDARD
        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("browser")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
#endif
        public static int LargestWindowWidth => Console.LargestWindowWidth;

#if !NETSTANDARD
        [SupportedOSPlatform("windows")]
#endif
        public static bool NumberLock => Console.NumberLock;

        public static TextWriter Out => Console.Out;

        public static Encoding OutputEncoding
        {
            get
            {
                lock (_lock)
                    return Console.OutputEncoding;
            }
#if !NETSTANDARD
            [UnsupportedOSPlatform("android")]
            [UnsupportedOSPlatform("ios")]
            [UnsupportedOSPlatform("tvos")]
#endif
            set
            {
                lock (_lock)
                    Console.OutputEncoding = value;
            }
        }

        public static string Title
        {
#if !NETSTANDARD
            [SupportedOSPlatform("windows")]
#endif
#pragma warning disable CA1416 // Validate platform compatibility
            get => Console.Title;
#pragma warning restore CA1416 // Validate platform compatibility
#if !NETSTANDARD
            [UnsupportedOSPlatform("android")]
            [UnsupportedOSPlatform("browser")]
            [UnsupportedOSPlatform("ios")]
            [UnsupportedOSPlatform("tvos")]
#endif
            set => Console.Title = value;
        }

#if !NETSTANDARD
        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("browser")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
#endif
        public static bool TreatControlCAsInput
        {
            get => Console.TreatControlCAsInput;
            set => Console.TreatControlCAsInput = value;
        }

        public static int WindowHeight
        {
#if !NETSTANDARD
            [UnsupportedOSPlatform("android")]
            [UnsupportedOSPlatform("browser")]
            [UnsupportedOSPlatform("ios")]
            [UnsupportedOSPlatform("tvos")]
#endif
            get => Console.WindowHeight;
#if !NETSTANDARD
            [SupportedOSPlatform("windows")]
#endif
#pragma warning disable CA1416 // Validate platform compatibility
            set => Console.WindowHeight = value;
#pragma warning restore CA1416 // Validate platform compatibility
        }

        public static int WindowLeft
        {
            get => Console.WindowLeft;
#if !NETSTANDARD
            [SupportedOSPlatform("windows")]
#endif
#pragma warning disable CA1416 // Validate platform compatibility
            set => Console.WindowLeft = value;
#pragma warning restore CA1416 // Validate platform compatibility
        }

        public static int WindowTop
        {
            get => Console.WindowTop;
#if !NETSTANDARD
            [SupportedOSPlatform("windows")]
#endif
#pragma warning disable CA1416 // Validate platform compatibility
            set => Console.WindowTop = value;
#pragma warning restore CA1416 // Validate platform compatibility
        }

        public static int WindowWidth
        {
#if !NETSTANDARD
            [UnsupportedOSPlatform("android")]
            [UnsupportedOSPlatform("browser")]
            [UnsupportedOSPlatform("ios")]
            [UnsupportedOSPlatform("tvos")]
#endif
            get => Console.WindowWidth;
#if !NETSTANDARD
            [SupportedOSPlatform("windows")]
#endif
#pragma warning disable CA1416 // Validate platform compatibility
            set => Console.WindowWidth = value;
#pragma warning restore CA1416 // Validate platform compatibility
        }

#if !NETSTANDARD
        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("browser")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
#endif
        public static event ConsoleCancelEventHandler? CancelKeyPress
        {
            //[System.Runtime.CompilerServices.NullableContext(2)]
            add => Console.CancelKeyPress += value;
            //[System.Runtime.CompilerServices.NullableContext(2)]
            remove => Console.CancelKeyPress -= value;
        }

#if !NETSTANDARD
        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("browser")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
#endif
        public static void Beep() => Console.Beep();

#if !NETSTANDARD
        [SupportedOSPlatform("windows")]
#endif
        public static void Beep(int frequency, int duration) => Console.Beep(frequency: frequency, duration: duration);

#if !NETSTANDARD
        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
#endif
        public static void Clear()
        {
            lock (_lock)
                Console.Clear();
        }

#if !NETSTANDARD
        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("browser")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
#endif
        public static (int Left, int Top) GetCursorPosition()
        {
            lock (_lock)
#if !NETSTANDARD
                return Console.GetCursorPosition();
#else
                return (Console.CursorLeft, Console.CursorTop);
#endif
        }

#if !NETSTANDARD
        [SupportedOSPlatform("windows")]
#endif
        public static void MoveBufferArea(
            int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight, int targetLeft, int targetTop)
        {
            lock (_lock)
                Console.MoveBufferArea(
                    sourceLeft: sourceLeft, sourceTop: sourceTop,
                    sourceWidth: sourceWidth, sourceHeight: sourceHeight,
                    targetLeft: targetLeft, targetTop: targetTop);
        }

#if !NETSTANDARD
        [SupportedOSPlatform("windows")]
#endif
        public static void MoveBufferArea(
            int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight, int targetLeft, int targetTop,
            char sourceChar, ConsoleColor sourceForeColor, ConsoleColor sourceBackColor)
        {
            lock (_lock)
                Console.MoveBufferArea(
                    sourceLeft: sourceLeft, sourceTop: sourceTop,
                    sourceWidth: sourceWidth, sourceHeight: sourceHeight,
                    targetLeft: targetLeft, targetTop: targetTop,
                    sourceChar: sourceChar, sourceForeColor: sourceForeColor, sourceBackColor: sourceBackColor);
        }

        public static Stream OpenStandardError() => Console.OpenStandardError();

        public static Stream OpenStandardError(int bufferSize) => Console.OpenStandardError(bufferSize: bufferSize);

#if !NETSTANDARD
        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("browser")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
#endif
        public static Stream OpenStandardInput() => Console.OpenStandardInput();

#if !NETSTANDARD
        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("browser")]
#endif
        public static Stream OpenStandardInput(int bufferSize) => Console.OpenStandardInput(bufferSize: bufferSize);

        public static Stream OpenStandardOutput() => Console.OpenStandardOutput();

        public static Stream OpenStandardOutput(int bufferSize) => Console.OpenStandardOutput(bufferSize: bufferSize);

#if !NETSTANDARD
        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("browser")]
#endif
        public static int Read()
        {
            lock (_lock)
                return Console.Read();
        }

#if !NETSTANDARD
        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("browser")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
#endif
        public static ConsoleKeyInfo ReadKey()
        {
            lock (_lock)
                return Console.ReadKey();
        }

#if !NETSTANDARD
        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("browser")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
#endif
        public static ConsoleKeyInfo ReadKey(bool intercept)
        {
            lock (_lock)
                return Console.ReadKey(intercept);
        }

#if !NETSTANDARD
        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("browser")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
#endif
        public static void ResetColor()
        {
            lock (_lock)
                Console.ResetColor();
        }

#if !NETSTANDARD
        [SupportedOSPlatform("windows")]
#endif
        public static void SetBufferSize(int width, int height)
        {
            lock (_lock)
                Console.SetBufferSize(width: width, height: height);
        }

#if !NETSTANDARD
        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("browser")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
#endif
        public static void SetCursorPosition(int left, int top)
        {
            lock (_lock)
                Console.SetCursorPosition(left: left, top: top);
        }

        public static void SetError(TextWriter newError) => Console.SetError(newError: newError);

#if !NETSTANDARD
        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("browser")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
#endif
        public static void SetIn(TextReader newIn) => Console.SetIn(newIn: newIn);

        public static void SetOut(TextWriter newOut) => Console.SetOut(newOut: newOut);

#if !NETSTANDARD
        [SupportedOSPlatform("windows")]
#endif
        public static void SetWindowPosition(int left, int top) => Console.SetWindowPosition(left: left, top: top);

#if !NETSTANDARD
        [SupportedOSPlatform("windows")]
#endif
        public static void SetWindowSize(int width, int height)
        {
            lock (_lock)
                Console.SetWindowSize(width: width, height: height);
        }

#pragma warning restore IDE0079 // Remove unnecessary suppression

        #endregion
    }
}
