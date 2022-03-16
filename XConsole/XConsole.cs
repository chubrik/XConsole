using System.Diagnostics;
using System.Runtime.Versioning;
using System.Text;

namespace System
{
    public static class XConsole
    {
        private static readonly object _lock = new();
        private static readonly string _newLine = Environment.NewLine;
        private static bool _cursorVisible = OperatingSystem.IsWindows() && Console.CursorVisible;
        private static int _maxTop = Console.BufferHeight - 1;
        internal static long ShiftTop = 0;

        public static void Lock(Action action)
        {
            lock (_lock)
                action();
        }

        #region ReadLine

        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("browser")]
        public static string ReadLine()
        {
            lock (_lock)
                return Console.ReadLine() ?? string.Empty;
        }

        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("browser")]
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

                                text = text[0..^1];
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
        public static (XConsolePosition Begin, XConsolePosition End) Write(string? value) => Write(new[] { value ?? string.Empty });

        //public static void Write(string format, object? arg0) { }
        //public static void Write(string format, object? arg0, object? arg1) { }
        //public static void Write(string format, object? arg0, object? arg1, object? arg2) { }
        //public static void Write(string format, params object?[]? arg) { }

        //[CLSCompliant(false)]
        public static void Write(uint value) => Write(value.ToString());
        //[CLSCompliant(false)]
        public static void Write(ulong value) => Write(value.ToString());

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
        public static (XConsolePosition Begin, XConsolePosition End) WriteLine(string? value) => WriteLine(new[] { value ?? string.Empty });

        //public static void WriteLine(string format, object? arg0) { }
        //public static void WriteLine(string format, object? arg0, object? arg1) { }
        //public static void WriteLine(string format, object? arg0, object? arg1, object? arg2) { }
        //public static void WriteLine(string format, params object?[]? arg) { }

        //[CLSCompliant(false)]
        public static void WriteLine(uint value) => WriteLine(value.ToString());
        //[CLSCompliant(false)]
        public static void WriteLine(ulong value) => WriteLine(value.ToString());

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

        #region Proxy

        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("browser")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
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
            [UnsupportedOSPlatform("android")]
            [UnsupportedOSPlatform("browser")]
            [UnsupportedOSPlatform("ios")]
            [UnsupportedOSPlatform("tvos")]
            get
            {
                lock (_lock)
                    return Console.BufferHeight;
            }
            [SupportedOSPlatform("windows")]
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
            [UnsupportedOSPlatform("android")]
            [UnsupportedOSPlatform("browser")]
            [UnsupportedOSPlatform("ios")]
            [UnsupportedOSPlatform("tvos")]
            get
            {
                lock (_lock)
                    return Console.BufferWidth;
            }
            [SupportedOSPlatform("windows")]
            set
            {
                lock (_lock)
#pragma warning disable CA1416 // Validate platform compatibility
                    Console.BufferWidth = value;
#pragma warning restore CA1416 // Validate platform compatibility
            }
        }

        [SupportedOSPlatform("windows")]
        public static bool CapsLock => Console.CapsLock;

        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("browser")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
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
            [UnsupportedOSPlatform("android")]
            [UnsupportedOSPlatform("browser")]
            [UnsupportedOSPlatform("ios")]
            [UnsupportedOSPlatform("tvos")]
            get => Console.CursorSize;
            [SupportedOSPlatform("windows")]
#pragma warning disable CA1416 // Validate platform compatibility
            set => Console.CursorSize = value;
#pragma warning restore CA1416 // Validate platform compatibility
        }

        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("browser")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
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
            [SupportedOSPlatform("windows")]
            get
            {
                lock (_lock)
                    return _cursorVisible;
            }
            [UnsupportedOSPlatform("android")]
            [UnsupportedOSPlatform("browser")]
            [UnsupportedOSPlatform("ios")]
            [UnsupportedOSPlatform("tvos")]
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

        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("browser")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
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

        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("browser")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
        public static TextReader In => Console.In;

        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("browser")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
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

        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("browser")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
        public static int LargestWindowHeight => Console.LargestWindowHeight;

        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("browser")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
        public static int LargestWindowWidth => Console.LargestWindowWidth;

        [SupportedOSPlatform("windows")]
        public static bool NumberLock => Console.NumberLock;

        public static TextWriter Out => Console.Out;

        public static Encoding OutputEncoding
        {
            get
            {
                lock (_lock)
                    return Console.OutputEncoding;
            }
            [UnsupportedOSPlatform("android")]
            [UnsupportedOSPlatform("ios")]
            [UnsupportedOSPlatform("tvos")]
            set
            {
                lock (_lock)
                    Console.OutputEncoding = value;
            }
        }

        public static string Title
        {
            [SupportedOSPlatform("windows")]
#pragma warning disable CA1416 // Validate platform compatibility
            get => Console.Title;
#pragma warning restore CA1416 // Validate platform compatibility
            [UnsupportedOSPlatform("android")]
            [UnsupportedOSPlatform("browser")]
            [UnsupportedOSPlatform("ios")]
            [UnsupportedOSPlatform("tvos")]
            set => Console.Title = value;
        }
        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("browser")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]

        public static bool TreatControlCAsInput
        {
            get => Console.TreatControlCAsInput;
            set => Console.TreatControlCAsInput = value;
        }

        public static int WindowHeight
        {
            [UnsupportedOSPlatform("android")]
            [UnsupportedOSPlatform("browser")]
            [UnsupportedOSPlatform("ios")]
            [UnsupportedOSPlatform("tvos")]
            get => Console.WindowHeight;
            [SupportedOSPlatform("windows")]
#pragma warning disable CA1416 // Validate platform compatibility
            set => Console.WindowHeight = value;
#pragma warning restore CA1416 // Validate platform compatibility
        }

        public static int WindowLeft
        {
            get => Console.WindowLeft;
            [SupportedOSPlatform("windows")]
#pragma warning disable CA1416 // Validate platform compatibility
            set => Console.WindowLeft = value;
#pragma warning restore CA1416 // Validate platform compatibility
        }

        public static int WindowTop
        {
            get => Console.WindowTop;
            [SupportedOSPlatform("windows")]
#pragma warning disable CA1416 // Validate platform compatibility
            set => Console.WindowTop = value;
#pragma warning restore CA1416 // Validate platform compatibility
        }

        public static int WindowWidth
        {
            [UnsupportedOSPlatform("android")]
            [UnsupportedOSPlatform("browser")]
            [UnsupportedOSPlatform("ios")]
            [UnsupportedOSPlatform("tvos")]
            get => Console.WindowWidth;
            [SupportedOSPlatform("windows")]
#pragma warning disable CA1416 // Validate platform compatibility
            set => Console.WindowWidth = value;
#pragma warning restore CA1416 // Validate platform compatibility
        }

        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("browser")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
        public static event ConsoleCancelEventHandler? CancelKeyPress
        {
            //[System.Runtime.CompilerServices.NullableContext(2)]
            add => Console.CancelKeyPress += value;
            //[System.Runtime.CompilerServices.NullableContext(2)]
            remove => Console.CancelKeyPress -= value;
        }

        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("browser")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
        public static void Beep() => Console.Beep();

        [SupportedOSPlatform("windows")]
        public static void Beep(int frequency, int duration) => Console.Beep(frequency: frequency, duration: duration);

        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
        public static void Clear()
        {
            lock (_lock)
                Console.Clear();
        }

        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("browser")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
        public static (int Left, int Top) GetCursorPosition()
        {
            lock (_lock)
                return Console.GetCursorPosition();
        }

        [SupportedOSPlatform("windows")]
        public static void MoveBufferArea(
            int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight, int targetLeft, int targetTop)
        {
            lock (_lock)
                Console.MoveBufferArea(
                    sourceLeft: sourceLeft, sourceTop: sourceTop,
                    sourceWidth: sourceWidth, sourceHeight: sourceHeight,
                    targetLeft: targetLeft, targetTop: targetTop);
        }

        [SupportedOSPlatform("windows")]
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

        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("browser")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
        public static Stream OpenStandardInput() => Console.OpenStandardInput();

        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("browser")]
        public static Stream OpenStandardInput(int bufferSize) => Console.OpenStandardInput(bufferSize: bufferSize);

        public static Stream OpenStandardOutput() => Console.OpenStandardOutput();

        public static Stream OpenStandardOutput(int bufferSize) => Console.OpenStandardOutput(bufferSize: bufferSize);

        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("browser")]
        public static int Read()
        {
            lock (_lock)
                return Console.Read();
        }

        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("browser")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
        public static ConsoleKeyInfo ReadKey()
        {
            lock (_lock)
                return Console.ReadKey();
        }

        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("browser")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
        public static ConsoleKeyInfo ReadKey(bool intercept)
        {
            lock (_lock)
                return Console.ReadKey(intercept);
        }

        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("browser")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
        public static void ResetColor()
        {
            lock (_lock)
                Console.ResetColor();
        }

        [SupportedOSPlatform("windows")]
        public static void SetBufferSize(int width, int height)
        {
            lock (_lock)
                Console.SetBufferSize(width: width, height: height);
        }

        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("browser")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
        public static void SetCursorPosition(int left, int top)
        {
            lock (_lock)
                Console.SetCursorPosition(left: left, top: top);
        }

        public static void SetError(TextWriter newError) => Console.SetError(newError: newError);

        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("browser")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
        public static void SetIn(TextReader newIn) => Console.SetIn(newIn: newIn);

        public static void SetOut(TextWriter newOut) => Console.SetOut(newOut: newOut);

        [SupportedOSPlatform("windows")]
        public static void SetWindowPosition(int left, int top) => Console.SetWindowPosition(left: left, top: top);

        [SupportedOSPlatform("windows")]
        public static void SetWindowSize(int width, int height)
        {
            lock (_lock)
                Console.SetWindowSize(width: width, height: height);
        }

        #endregion
    }
}
