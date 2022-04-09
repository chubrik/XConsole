using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace System
{
#if !NETSTANDARD
    using System.Runtime.Versioning;
    [SupportedOSPlatform("windows")]
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
#endif
    public static class XConsole
    {
        private static readonly object _lock = new();
        private static readonly string _newLine = Environment.NewLine;
        private static bool _cursorVisible = Console.CursorVisible;
        private static int _maxTop = Console.BufferHeight - 1;
        internal static long ShiftTop = 0;

        public static void Lock(Action action)
        {
            lock (_lock)
                action();
        }

        public static XConsolePosition CursorPosition
        {
            get
            {
                lock (_lock)
                    return new(left: Console.CursorLeft, top: Console.CursorTop, shiftTop: ShiftTop);
            }
            set
            {
                lock (_lock)
                {
                    var shiftedTop = checked((int)(value.Top + value.ShiftTop - ShiftTop));
                    Console.SetCursorPosition(value.Left, shiftedTop);
                }
            }
        }

        #region ReadLine

        public static string ReadLine()
        {
            lock (_lock)
                return Console.ReadLine() ?? string.Empty;
        }

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

        [Obsolete("At least one argument should be specified", error: true)]
        public static void Write() => throw new NotSupportedException("At least one argument should be specified");

        public static (XConsolePosition Begin, XConsolePosition End) Write(string? value)
        {
            var items = string.IsNullOrEmpty(value) ? Array.Empty<XConsoleItem>() : new[] { XConsoleItem.Parse(value) };
            return WriteBase(items, isWriteLine: false);
        }

        public static (XConsolePosition Begin, XConsolePosition End) Write(params string?[] values)
        {
            var items = new List<XConsoleItem>(values.Length);

            foreach (var value in values)
                if (!string.IsNullOrEmpty(value))
                    items.Add(XConsoleItem.Parse(value));

            return WriteBase(items, isWriteLine: false);
        }

        public static (XConsolePosition Begin, XConsolePosition End) WriteLine(string? value)
        {
            var items = string.IsNullOrEmpty(value) ? Array.Empty<XConsoleItem>() : new[] { XConsoleItem.Parse(value) };
            return WriteBase(items, isWriteLine: true);
        }

        public static (XConsolePosition Begin, XConsolePosition End) WriteLine(params string?[] values)
        {
            var items = new List<XConsoleItem>(values.Length);

            foreach (var value in values)
                if (!string.IsNullOrEmpty(value))
                    items.Add(XConsoleItem.Parse(value));

            return WriteBase(items, isWriteLine: true);
        }

        private static (XConsolePosition Begin, XConsolePosition End) WriteBase(IReadOnlyList<XConsoleItem> logItems, bool isWriteLine)
        {
            int beginLeft, beginTop, endLeft, endTop;
            long shiftTop;
            var getPinValues = _getPinValues;

            if (getPinValues == null)
            {
                lock (_lock)
                {
                    beginLeft = Console.CursorLeft;
                    beginTop = Console.CursorTop;
                    Console.CursorVisible = false;

                    foreach (var logItem in logItems)
                        WriteItem(logItem);

                    endLeft = Console.CursorLeft;
                    endTop = Console.CursorTop;

                    if (isWriteLine)
                    {
                        Console.WriteLine();
                        Console.CursorVisible = _cursorVisible;

                        if (endTop == _maxTop)
                        {
                            var logNewLineCount = logItems.SelectMany(i => i.Value).Count(i => i == '\n');
                            var shift = beginTop + logNewLineCount + 1 - endTop;
                            ShiftTop += shift;
                            beginTop -= shift;
                            endTop--;
                        }
                    }
                    else
                    {
                        Console.CursorVisible = _cursorVisible;

                        if (endTop == _maxTop)
                        {
                            var logNewLineCount = logItems.SelectMany(i => i.Value).Count(i => i == '\n');
                            var shift = beginTop + logNewLineCount - endTop;
                            ShiftTop += shift;
                            beginTop -= shift;
                        }
                    }

                    shiftTop = ShiftTop;
                }
            }
            else
            {
                var pinValues = getPinValues();
                var pinItems = new List<XConsoleItem>(pinValues.Count);

                foreach (var pinValue in pinValues)
                    if (!string.IsNullOrEmpty(pinValue))
                        pinItems.Add(XConsoleItem.Parse(pinValue));

                var spaces = _newLine + new string(' ', Console.BufferWidth - 1);

                lock (_lock)
                {
                    beginLeft = Console.CursorLeft;
                    beginTop = Console.CursorTop;
                    Console.CursorVisible = false;

                    if (_getPinValues == null)
                    {
                        foreach (var logItem in logItems)
                            WriteItem(logItem);

                        endLeft = Console.CursorLeft;
                        endTop = Console.CursorTop;

                        if (isWriteLine)
                        {
                            Console.WriteLine();
                            Console.CursorVisible = _cursorVisible;

                            if (endTop == _maxTop)
                            {
                                var logNewLineCount = logItems.SelectMany(i => i.Value).Count(i => i == '\n');
                                var shift = beginTop + logNewLineCount + 1 - endTop;
                                ShiftTop += shift;
                                beginTop -= shift;
                                endTop--;
                            }
                        }
                        else
                        {
                            Console.CursorVisible = _cursorVisible;

                            if (endTop == _maxTop)
                            {
                                var logNewLineCount = logItems.SelectMany(i => i.Value).Count(i => i == '\n');
                                var shift = beginTop + logNewLineCount - endTop;
                                ShiftTop += shift;
                                beginTop -= shift;
                            }
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
                                endTop = _maxTop - _pinHeight - 1;
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
                                endTop = _maxTop - _pinHeight;
                            }
                            else
                                _pinHeight = Console.CursorTop - endTop;

                            Console.SetCursorPosition(endLeft, endTop);
                        }

                        Console.CursorVisible = _cursorVisible;
                    }

                    shiftTop = ShiftTop;
                }
            }

            return (
                new(left: beginLeft, top: beginTop, shiftTop: shiftTop),
                new(left: endLeft, top: endTop, shiftTop: shiftTop)
            );
        }

        internal static XConsolePosition WriteToPosition(IReadOnlyList<string?> values, XConsolePosition position)
        {
            Debug.Assert(values.Count > 0);
            var items = new List<XConsoleItem>(values.Count);

            foreach (var value in values)
                if (!string.IsNullOrEmpty(value))
                    items.Add(XConsoleItem.Parse(value));

            int endLeft, endTop;
            long shiftTop;

            lock (_lock)
            {
                var origLeft = Console.CursorLeft;
                var origTop = Console.CursorTop;
                var shiftedTop = checked((int)(position.Top + position.ShiftTop - ShiftTop));
                Console.CursorVisible = false;
                Console.SetCursorPosition(position.Left, shiftedTop);

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
                }

                Console.SetCursorPosition(origLeft, origTop);
                Console.CursorVisible = _cursorVisible;
                shiftTop = ShiftTop;
            }

            return new(left: endLeft, top: endTop, shiftTop: shiftTop);
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

        public static (XConsolePosition Begin, XConsolePosition End) Write(bool value) => WriteNoParse(value.ToString());
        public static (XConsolePosition Begin, XConsolePosition End) Write(char value) => WriteNoParse(value.ToString());
        public static (XConsolePosition Begin, XConsolePosition End) Write(char[]? buffer) => WriteNoParse(buffer?.ToString());
        public static (XConsolePosition Begin, XConsolePosition End) Write(char[] buffer, int index, int count) => WriteNoParse(buffer.ToString()?.Substring(index, count));
        public static (XConsolePosition Begin, XConsolePosition End) Write(decimal value) => WriteNoParse(value.ToString());
        public static (XConsolePosition Begin, XConsolePosition End) Write(double value) => WriteNoParse(value.ToString());
        public static (XConsolePosition Begin, XConsolePosition End) Write(int value) => WriteNoParse(value.ToString());
        public static (XConsolePosition Begin, XConsolePosition End) Write(long value) => WriteNoParse(value.ToString());
        public static (XConsolePosition Begin, XConsolePosition End) Write(object? value) => WriteNoParse(value?.ToString());
        public static (XConsolePosition Begin, XConsolePosition End) Write(float value) => WriteNoParse(value.ToString());
        public static (XConsolePosition Begin, XConsolePosition End) Write(string format, object? arg0) => Write(string.Format(format, arg0));
        public static (XConsolePosition Begin, XConsolePosition End) Write(string format, object? arg0, object? arg1) => Write(string.Format(format, arg0, arg1));
        public static (XConsolePosition Begin, XConsolePosition End) Write(string format, object? arg0, object? arg1, object? arg2) => Write(string.Format(format, arg0, arg1, arg2));
        public static (XConsolePosition Begin, XConsolePosition End) Write(string format, params object?[]? arg) => Write(string.Format(format, arg ?? Array.Empty<object?>()));
        public static (XConsolePosition Begin, XConsolePosition End) Write(uint value) => WriteNoParse(value.ToString());
        public static (XConsolePosition Begin, XConsolePosition End) Write(ulong value) => WriteNoParse(value.ToString());

        public static (XConsolePosition Begin, XConsolePosition End) WriteLine(bool value) => WriteLineNoParse(value.ToString());
        public static (XConsolePosition Begin, XConsolePosition End) WriteLine(char value) => WriteLineNoParse(value.ToString());
        public static (XConsolePosition Begin, XConsolePosition End) WriteLine(char[]? buffer) => WriteLineNoParse(buffer?.ToString());
        public static (XConsolePosition Begin, XConsolePosition End) WriteLine(char[] buffer, int index, int count) => WriteLineNoParse(buffer.ToString()?.Substring(index, count));
        public static (XConsolePosition Begin, XConsolePosition End) WriteLine(decimal value) => WriteLineNoParse(value.ToString());
        public static (XConsolePosition Begin, XConsolePosition End) WriteLine(double value) => WriteLineNoParse(value.ToString());
        public static (XConsolePosition Begin, XConsolePosition End) WriteLine(int value) => WriteLineNoParse(value.ToString());
        public static (XConsolePosition Begin, XConsolePosition End) WriteLine(long value) => WriteLineNoParse(value.ToString());
        public static (XConsolePosition Begin, XConsolePosition End) WriteLine(object? value) => WriteLineNoParse(value?.ToString());
        public static (XConsolePosition Begin, XConsolePosition End) WriteLine(float value) => WriteLineNoParse(value.ToString());
        public static (XConsolePosition Begin, XConsolePosition End) WriteLine(string format, object? arg0) => WriteLine(string.Format(format, arg0));
        public static (XConsolePosition Begin, XConsolePosition End) WriteLine(string format, object? arg0, object? arg1) => WriteLine(string.Format(format, arg0, arg1));
        public static (XConsolePosition Begin, XConsolePosition End) WriteLine(string format, object? arg0, object? arg1, object? arg2) => WriteLine(string.Format(format, arg0, arg1, arg2));
        public static (XConsolePosition Begin, XConsolePosition End) WriteLine(string format, params object?[]? arg) => WriteLine(string.Format(format, arg ?? Array.Empty<object?>()));
        public static (XConsolePosition Begin, XConsolePosition End) WriteLine(uint value) => WriteLineNoParse(value.ToString());
        public static (XConsolePosition Begin, XConsolePosition End) WriteLine(ulong value) => WriteLineNoParse(value.ToString());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static (XConsolePosition Begin, XConsolePosition End) WriteNoParse(string? value)
        {
            var items = string.IsNullOrEmpty(value) ? Array.Empty<XConsoleItem>() : new[] { new XConsoleItem(value) };
            return WriteBase(items, isWriteLine: false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static (XConsolePosition Begin, XConsolePosition End) WriteLineNoParse(string? value)
        {
            var items = string.IsNullOrEmpty(value) ? Array.Empty<XConsoleItem>() : new[] { new XConsoleItem(value) };
            return WriteBase(items, isWriteLine: true);
        }

        #endregion

        #region Pin

        private static int _pinHeight = 0;
        private static Func<IReadOnlyList<string?>>? _getPinValues = null;

        [Obsolete("At least one argument should be specified", error: true)]
        public static void Pin() => throw new NotSupportedException("At least one argument should be specified");

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
            var pinItems = new List<XConsoleItem>(pinValues.Count);

            foreach (var pinValue in pinValues)
                if (!string.IsNullOrEmpty(pinValue))
                    pinItems.Add(XConsoleItem.Parse(pinValue));

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
            get
            {
                lock (_lock)
                    return Console.BufferHeight;
            }
            set
            {
                lock (_lock)
                {
                    Console.BufferHeight = value;
                    _maxTop = value - 1;
                }
            }
        }

        public static int BufferWidth
        {
            get
            {
                lock (_lock)
                    return Console.BufferWidth;
            }
            set
            {
                lock (_lock)
                    Console.BufferWidth = value;
            }
        }

        public static bool CapsLock => Console.CapsLock;

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
            get => Console.CursorSize;
            set => Console.CursorSize = value;
        }

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
            get
            {
                lock (_lock)
                    return Console.CursorVisible;
            }
            set
            {
                lock (_lock)
                    Console.CursorVisible = _cursorVisible = value;
            }
        }

        public static TextWriter Error => Console.Error;

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

        public static TextReader In => Console.In;

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

        public static int LargestWindowHeight => Console.LargestWindowHeight;

        public static int LargestWindowWidth => Console.LargestWindowWidth;

        public static bool NumberLock => Console.NumberLock;

        public static TextWriter Out => Console.Out;

        public static Encoding OutputEncoding
        {
            get
            {
                lock (_lock)
                    return Console.OutputEncoding;
            }
            set
            {
                lock (_lock)
                    Console.OutputEncoding = value;
            }
        }

        public static string Title
        {
            get => Console.Title;
            set => Console.Title = value;
        }

        public static bool TreatControlCAsInput
        {
            get => Console.TreatControlCAsInput;
            set => Console.TreatControlCAsInput = value;
        }

        public static int WindowHeight
        {
            get => Console.WindowHeight;
            set => Console.WindowHeight = value;
        }

        public static int WindowLeft
        {
            get => Console.WindowLeft;
            set => Console.WindowLeft = value;
        }

        public static int WindowTop
        {
            get => Console.WindowTop;
            set => Console.WindowTop = value;
        }

        public static int WindowWidth
        {
            get => Console.WindowWidth;
            set => Console.WindowWidth = value;
        }

        public static event ConsoleCancelEventHandler? CancelKeyPress
        {
            //[System.Runtime.CompilerServices.NullableContext(2)]
            add => Console.CancelKeyPress += value;
            //[System.Runtime.CompilerServices.NullableContext(2)]
            remove => Console.CancelKeyPress -= value;
        }

        public static void Beep() => Console.Beep();

        public static void Beep(int frequency, int duration) => Console.Beep(frequency: frequency, duration: duration);

        public static void Clear()
        {
            lock (_lock)
                Console.Clear();
        }

        public static (int Left, int Top) GetCursorPosition()
        {
            lock (_lock)
#if !NETSTANDARD
                return Console.GetCursorPosition();
#else
                return (Console.CursorLeft, Console.CursorTop);
#endif
        }

        public static void MoveBufferArea(
            int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight, int targetLeft, int targetTop)
        {
            lock (_lock)
                Console.MoveBufferArea(
                    sourceLeft: sourceLeft, sourceTop: sourceTop,
                    sourceWidth: sourceWidth, sourceHeight: sourceHeight,
                    targetLeft: targetLeft, targetTop: targetTop);
        }

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

        public static Stream OpenStandardInput() => Console.OpenStandardInput();

        public static Stream OpenStandardInput(int bufferSize) => Console.OpenStandardInput(bufferSize: bufferSize);

        public static Stream OpenStandardOutput() => Console.OpenStandardOutput();

        public static Stream OpenStandardOutput(int bufferSize) => Console.OpenStandardOutput(bufferSize: bufferSize);

        public static int Read()
        {
            lock (_lock)
                return Console.Read();
        }

        public static ConsoleKeyInfo ReadKey()
        {
            lock (_lock)
                return Console.ReadKey();
        }

        public static ConsoleKeyInfo ReadKey(bool intercept)
        {
            lock (_lock)
                return Console.ReadKey(intercept: intercept);
        }

        public static void ResetColor()
        {
            lock (_lock)
                Console.ResetColor();
        }

        public static void SetBufferSize(int width, int height)
        {
            lock (_lock)
                Console.SetBufferSize(width: width, height: height);
        }

        public static void SetCursorPosition(int left, int top)
        {
            lock (_lock)
                Console.SetCursorPosition(left: left, top: top);
        }

        public static void SetError(TextWriter newError) => Console.SetError(newError: newError);

        public static void SetIn(TextReader newIn) => Console.SetIn(newIn: newIn);

        public static void SetOut(TextWriter newOut) => Console.SetOut(newOut: newOut);

        public static void SetWindowPosition(int left, int top) => Console.SetWindowPosition(left: left, top: top);

        public static void SetWindowSize(int width, int height)
        {
            lock (_lock)
                Console.SetWindowSize(width: width, height: height);
        }

        #endregion
    }
}
