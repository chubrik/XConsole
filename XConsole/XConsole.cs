using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        private static readonly object _syncLock = new();
        private static readonly string _newLine = Environment.NewLine;
        private static bool _cursorVisible = Console.CursorVisible;
        private static int _maxTop = Console.BufferHeight - 1;
        internal static long ShiftTop = 0;

        public static void Sync(Action action)
        {
            lock (_syncLock)
                action();
        }

        [Obsolete("Method is deprecated, use Sync method instead.")]
        public static void Lock(Action action) => Sync(action);

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

            var pinHeight = _pinHeight;

            var pinClear = pinHeight > 0
                ? _newLine + new string(' ', Console.BufferWidth * pinHeight - 1)
                : string.Empty;

            var pinValues = getPinValues();
            var pinItems = new List<XConsoleItem>(pinValues.Count);

            foreach (var pinValue in pinValues)
                if (!string.IsNullOrEmpty(pinValue))
                    pinItems.Add(XConsoleItem.Parse(pinValue));

            int origLeft, origTop, pinEndTop;

            lock (_syncLock)
            {
                if (_getPinValues == null)
                    return;

                if (_pinHeight > 0)
                {
                    if (pinHeight != _pinHeight)
                        pinClear = _newLine + new string(' ', Console.BufferWidth * _pinHeight - 1);

#if !NETSTANDARD
                    (origLeft, origTop) = Console.GetCursorPosition();
#else
                    origLeft = Console.CursorLeft;
                    origTop = Console.CursorTop;
#endif
                    Console.CursorVisible = false;
                    Console.Write(pinClear);
                    Console.SetCursorPosition(origLeft, origTop);
                }
                else
                {
#if !NETSTANDARD
                    (origLeft, origTop) = Console.GetCursorPosition();
#else
                    origLeft = Console.CursorLeft;
                    origTop = Console.CursorTop;
#endif
                    Console.CursorVisible = false;
                }

                Console.WriteLine();
                WriteItems(pinItems);
                pinEndTop = Console.CursorTop;

                if (pinEndTop == _maxTop)
                {
                    _pinHeight = 1 + GetLineWrapCount(pinItems, beginLeft: 0);
                    var shift = origTop + _pinHeight - _maxTop;
                    ShiftTop += shift;
                    origTop -= shift;
                }
                else
                    _pinHeight = pinEndTop - origTop;

                Console.SetCursorPosition(origLeft, origTop);
                Console.CursorVisible = _cursorVisible;
            }
        }

        public static void Unpin()
        {
            var pinHeight = _pinHeight;

            var pinClear = pinHeight > 0
                ? _newLine + new string(' ', Console.BufferWidth * pinHeight - 1)
                : string.Empty;

            int origLeft, origTop;

            lock (_syncLock)
            {
                if (_getPinValues == null)
                    return;

                if (_pinHeight > 0)
                {
                    if (pinHeight != _pinHeight)
                        pinClear = _newLine + new string(' ', Console.BufferWidth + _pinHeight - 1);

#if !NETSTANDARD
                    (origLeft, origTop) = Console.GetCursorPosition();
#else
                    origLeft = Console.CursorLeft;
                    origTop = Console.CursorTop;
#endif
                    Console.CursorVisible = false;
                    Console.Write(pinClear);
                    Console.SetCursorPosition(origLeft, origTop);
                    Console.CursorVisible = _cursorVisible;
                }

                _getPinValues = null;
                _pinHeight = 0;
            }
        }

        #endregion

        #region Position

        public static XConsolePosition CursorPosition
        {
            get
            {
                lock (_syncLock)
                {
#if !NETSTANDARD
                    var (left, top) = Console.GetCursorPosition();
                    return new(left: left, top: top, shiftTop: ShiftTop);
#else
                    return new(left: Console.CursorLeft, top: Console.CursorTop, shiftTop: ShiftTop);
#endif
                }
            }
            set
            {
                lock (_syncLock)
                {
                    var shiftedTop = checked((int)(value.Top + value.ShiftTop - ShiftTop));
                    Console.SetCursorPosition(value.Left, shiftedTop);
                }
            }
        }

        internal static bool TryGetPositionShiftedTop(XConsolePosition position, out int shiftedTop)
        {
            long shiftTop;
            int bufferHeight;

            lock (_syncLock)
            {
                shiftTop = ShiftTop;
                bufferHeight = Console.BufferHeight;
            }

            var rawShiftedTop = position.Top + position.ShiftTop - shiftTop;

            if (rawShiftedTop >= 0 && rawShiftedTop < bufferHeight)
            {
                shiftedTop = (int)rawShiftedTop;
                return true;
            }
            else
            {
                shiftedTop = default;
                return false;
            }
        }

        internal static XConsolePosition WriteToPosition(XConsolePosition position, string?[] values)
        {
            Debug.Assert(values.Length > 0);
            var items = new List<XConsoleItem>(values.Length);

            foreach (var value in values)
                if (!string.IsNullOrEmpty(value))
                    items.Add(XConsoleItem.Parse(value));

            int origLeft, origTop, shiftedTop, endLeft, endTop;
            long shiftTop;

            lock (_syncLock)
            {
#if !NETSTANDARD
                (origLeft, origTop) = Console.GetCursorPosition();
#else
                origLeft = Console.CursorLeft;
                origTop = Console.CursorTop;
#endif
                shiftTop = ShiftTop;
                shiftedTop = checked((int)(position.Top + position.ShiftTop - shiftTop));
                Console.CursorVisible = false;
                Console.SetCursorPosition(position.Left, shiftedTop);
                WriteItems(items);
#if !NETSTANDARD
                (endLeft, endTop) = Console.GetCursorPosition();
#else
                endLeft = Console.CursorLeft;
                endTop = Console.CursorTop;
#endif

                if (endTop == _maxTop)
                {
                    var lineWrapCount = GetLineWrapCount(items, origLeft);
                    var shift = origTop + lineWrapCount - endTop;
                    shiftTop += shift;
                    origTop -= shift;
                    ShiftTop = shiftTop;
                }

                Console.SetCursorPosition(origLeft, origTop);
                Console.CursorVisible = _cursorVisible;
            }

            return new(left: endLeft, top: endTop, shiftTop: shiftTop);
        }

        #endregion

        #region ReadLine

        public static string ReadLine()
        {
            lock (_syncLock)
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

                    lock (_syncLock)
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
            var items = string.IsNullOrEmpty(value)
                ? Array.Empty<XConsoleItem>()
                : new[] { XConsoleItem.Parse(value) };

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
            var items = string.IsNullOrEmpty(value)
                ? Array.Empty<XConsoleItem>()
                : new[] { XConsoleItem.Parse(value) };

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

        private static (XConsolePosition Begin, XConsolePosition End) WriteBase(
            IReadOnlyList<XConsoleItem> logItems, bool isWriteLine)
        {
            int beginLeft, beginTop, endLeft, endTop;
            long shiftTop;
            var getPinValues = _getPinValues;

            if (getPinValues == null)
            {
                lock (_syncLock)
                {
#if !NETSTANDARD
                    (beginLeft, beginTop) = Console.GetCursorPosition();
#else
                    beginLeft = Console.CursorLeft;
                    beginTop = Console.CursorTop;
#endif
                    Console.CursorVisible = false;
                    WriteItems(logItems);
#if !NETSTANDARD
                    (endLeft, endTop) = Console.GetCursorPosition();
#else
                    endLeft = Console.CursorLeft;
                    endTop = Console.CursorTop;
#endif

                    if (isWriteLine)
                    {
                        Console.WriteLine();
                        Console.CursorVisible = _cursorVisible;

                        if (endTop == _maxTop)
                        {
                            var logLineWrapCount = GetLineWrapCount(logItems, beginLeft);
                            var shift = beginTop + logLineWrapCount + 1 - endTop;
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
                            var logLineWrapCount = GetLineWrapCount(logItems, beginLeft);
                            var shift = beginTop + logLineWrapCount - endTop;
                            ShiftTop += shift;
                            beginTop -= shift;
                        }
                    }

                    shiftTop = ShiftTop;
                }
            }
            else
            {
                var pinHeight = _pinHeight;

                var pinClear = pinHeight > 0
                    ? _newLine + new string(' ', Console.BufferWidth * pinHeight - 1)
                    : string.Empty;

                var pinValues = getPinValues();
                var pinItems = new List<XConsoleItem>(pinValues.Count);

                foreach (var pinValue in pinValues)
                    if (!string.IsNullOrEmpty(pinValue))
                        pinItems.Add(XConsoleItem.Parse(pinValue));

                int pinEndTop;

                lock (_syncLock)
                {
                    if (_getPinValues == null)
                    {
#if !NETSTANDARD
                        (beginLeft, beginTop) = Console.GetCursorPosition();
#else
                        beginLeft = Console.CursorLeft;
                        beginTop = Console.CursorTop;
#endif
                        Console.CursorVisible = false;
                        WriteItems(logItems);
#if !NETSTANDARD
                        (endLeft, endTop) = Console.GetCursorPosition();
#else
                        endLeft = Console.CursorLeft;
                        endTop = Console.CursorTop;
#endif

                        if (isWriteLine)
                        {
                            Console.WriteLine();
                            Console.CursorVisible = _cursorVisible;

                            if (endTop == _maxTop)
                            {
                                var logLineWrapCount = GetLineWrapCount(logItems, beginLeft);
                                var shift = beginTop + logLineWrapCount + 1 - endTop;
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
                                var logLineWrapCount = GetLineWrapCount(logItems, beginLeft);
                                var shift = beginTop + logLineWrapCount - endTop;
                                ShiftTop += shift;
                                beginTop -= shift;
                            }
                        }
                    }
                    else
                    {
                        if (_pinHeight > 0)
                        {
                            if (pinHeight != _pinHeight)
                                pinClear = _newLine + new string(' ', Console.BufferWidth + _pinHeight - 1);

#if !NETSTANDARD
                            (beginLeft, beginTop) = Console.GetCursorPosition();
#else
                            beginLeft = Console.CursorLeft;
                            beginTop = Console.CursorTop;
#endif
                            Console.CursorVisible = false;
                            Console.Write(pinClear);
                            Console.SetCursorPosition(beginLeft, beginTop);
                        }
                        else
                        {
#if !NETSTANDARD
                            (beginLeft, beginTop) = Console.GetCursorPosition();
#else
                            beginLeft = Console.CursorLeft;
                            beginTop = Console.CursorTop;
#endif
                            Console.CursorVisible = false;
                        }

                        WriteItems(logItems);
#if !NETSTANDARD
                        (endLeft, endTop) = Console.GetCursorPosition();
#else
                        endLeft = Console.CursorLeft;
                        endTop = Console.CursorTop;
#endif
                        Console.WriteLine();

                        if (isWriteLine)
                        {
                            Console.WriteLine();
                            WriteItems(pinItems);
                            pinEndTop = Console.CursorTop;

                            if (pinEndTop == _maxTop)
                            {
                                var logLineWrapCount = GetLineWrapCount(logItems, beginLeft);
                                _pinHeight = 1 + GetLineWrapCount(pinItems, beginLeft: 0);
                                var shift = beginTop + logLineWrapCount + 1 + _pinHeight - _maxTop;
                                ShiftTop += shift;
                                beginTop -= shift;
                                endTop = _maxTop - _pinHeight - 1;
                            }
                            else
                                _pinHeight = pinEndTop - (endTop + 1);

                            Console.SetCursorPosition(0, endTop + 1);
                        }
                        else
                        {
                            WriteItems(pinItems);
                            pinEndTop = Console.CursorTop;

                            if (pinEndTop == _maxTop)
                            {
                                var logLineWrapCount = GetLineWrapCount(logItems, beginLeft);
                                _pinHeight = 1 + GetLineWrapCount(pinItems, beginLeft: 0);
                                var shift = beginTop + logLineWrapCount + _pinHeight - _maxTop;
                                ShiftTop += shift;
                                beginTop -= shift;
                                endTop = _maxTop - _pinHeight;
                            }
                            else
                                _pinHeight = pinEndTop - endTop;

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

        #region Overloads

        public static (XConsolePosition Begin, XConsolePosition End) Write(bool value) =>
            WriteBase(new[] { new XConsoleItem(value.ToString()) }, isWriteLine: false);

        public static (XConsolePosition Begin, XConsolePosition End) Write(char value) =>
            WriteBase(new[] { new XConsoleItem(value.ToString()) }, isWriteLine: false);

        public static (XConsolePosition Begin, XConsolePosition End) Write(char[]? buffer)
        {
            var value = buffer?.ToString();

            var items = string.IsNullOrEmpty(value)
                ? Array.Empty<XConsoleItem>()
                : new[] { new XConsoleItem(value) };

            return WriteBase(items, isWriteLine: false);
        }

        public static (XConsolePosition Begin, XConsolePosition End) Write(char[] buffer, int index, int count)
        {
            var value = buffer.ToString()?.Substring(index, count);

            var items = string.IsNullOrEmpty(value)
                ? Array.Empty<XConsoleItem>()
                : new[] { new XConsoleItem(value) };

            return WriteBase(items, isWriteLine: false);
        }

        public static (XConsolePosition Begin, XConsolePosition End) Write(decimal value) =>
            WriteBase(new[] { new XConsoleItem(value.ToString()) }, isWriteLine: false);

        public static (XConsolePosition Begin, XConsolePosition End) Write(double value) =>
            WriteBase(new[] { new XConsoleItem(value.ToString()) }, isWriteLine: false);

        public static (XConsolePosition Begin, XConsolePosition End) Write(int value) =>
            WriteBase(new[] { new XConsoleItem(value.ToString()) }, isWriteLine: false);

        public static (XConsolePosition Begin, XConsolePosition End) Write(long value) =>
            WriteBase(new[] { new XConsoleItem(value.ToString()) }, isWriteLine: false);

        public static (XConsolePosition Begin, XConsolePosition End) Write(object? value)
        {
            var valueStr = value?.ToString();

            var items = string.IsNullOrEmpty(valueStr)
                ? Array.Empty<XConsoleItem>()
                : new[] { new XConsoleItem(valueStr) };

            return WriteBase(items, isWriteLine: false);
        }

        public static (XConsolePosition Begin, XConsolePosition End) Write(float value) =>
            WriteBase(new[] { new XConsoleItem(value.ToString()) }, isWriteLine: false);

        public static (XConsolePosition Begin, XConsolePosition End) Write(
            string format, object? arg0) =>
            WriteBase(new[] { XConsoleItem.Parse(string.Format(format, arg0)) }, isWriteLine: false);

        public static (XConsolePosition Begin, XConsolePosition End) Write(
            string format, object? arg0, object? arg1) =>
            WriteBase(new[] { XConsoleItem.Parse(string.Format(format, arg0, arg1)) }, isWriteLine: false);

        public static (XConsolePosition Begin, XConsolePosition End) Write(
            string format, object? arg0, object? arg1, object? arg2) =>
            WriteBase(new[] { XConsoleItem.Parse(string.Format(format, arg0, arg1, arg2)) }, isWriteLine: false);

        public static (XConsolePosition Begin, XConsolePosition End) Write(
            string format, params object?[]? arg) =>
            WriteBase(
                new[] { XConsoleItem.Parse(string.Format(format, arg ?? Array.Empty<object?>())) },
                isWriteLine: false);

        public static (XConsolePosition Begin, XConsolePosition End) Write(uint value) =>
            WriteBase(new[] { new XConsoleItem(value.ToString()) }, isWriteLine: false);

        public static (XConsolePosition Begin, XConsolePosition End) Write(ulong value) =>
            WriteBase(new[] { new XConsoleItem(value.ToString()) }, isWriteLine: false);

        public static (XConsolePosition Begin, XConsolePosition End) WriteLine(bool value) =>
            WriteBase(new[] { new XConsoleItem(value.ToString()) }, isWriteLine: true);

        public static (XConsolePosition Begin, XConsolePosition End) WriteLine(char value) =>
            WriteBase(new[] { new XConsoleItem(value.ToString()) }, isWriteLine: true);

        public static (XConsolePosition Begin, XConsolePosition End) WriteLine(char[]? buffer)
        {
            var value = buffer?.ToString();

            var items = string.IsNullOrEmpty(value)
                ? Array.Empty<XConsoleItem>()
                : new[] { new XConsoleItem(value) };

            return WriteBase(items, isWriteLine: true);
        }

        public static (XConsolePosition Begin, XConsolePosition End) WriteLine(char[] buffer, int index, int count)
        {
            var value = buffer.ToString()?.Substring(index, count);

            var items = string.IsNullOrEmpty(value)
                ? Array.Empty<XConsoleItem>()
                : new[] { new XConsoleItem(value) };

            return WriteBase(items, isWriteLine: true);
        }

        public static (XConsolePosition Begin, XConsolePosition End) WriteLine(decimal value) =>
            WriteBase(new[] { new XConsoleItem(value.ToString()) }, isWriteLine: true);

        public static (XConsolePosition Begin, XConsolePosition End) WriteLine(double value) =>
            WriteBase(new[] { new XConsoleItem(value.ToString()) }, isWriteLine: true);

        public static (XConsolePosition Begin, XConsolePosition End) WriteLine(int value) =>
            WriteBase(new[] { new XConsoleItem(value.ToString()) }, isWriteLine: true);

        public static (XConsolePosition Begin, XConsolePosition End) WriteLine(long value) =>
            WriteBase(new[] { new XConsoleItem(value.ToString()) }, isWriteLine: true);

        public static (XConsolePosition Begin, XConsolePosition End) WriteLine(object? value)
        {
            var valueStr = value?.ToString();

            var items = string.IsNullOrEmpty(valueStr)
                ? Array.Empty<XConsoleItem>()
                : new[] { new XConsoleItem(valueStr) };

            return WriteBase(items, isWriteLine: true);
        }

        public static (XConsolePosition Begin, XConsolePosition End) WriteLine(float value) =>
            WriteBase(new[] { new XConsoleItem(value.ToString()) }, isWriteLine: true);

        public static (XConsolePosition Begin, XConsolePosition End) WriteLine(
            string format, object? arg0) =>
            WriteBase(new[] { XConsoleItem.Parse(string.Format(format, arg0)) }, isWriteLine: true);

        public static (XConsolePosition Begin, XConsolePosition End) WriteLine(
            string format, object? arg0, object? arg1) =>
            WriteBase(new[] { XConsoleItem.Parse(string.Format(format, arg0, arg1)) }, isWriteLine: true);

        public static (XConsolePosition Begin, XConsolePosition End) WriteLine(
            string format, object? arg0, object? arg1, object? arg2) =>
            WriteBase(new[] { XConsoleItem.Parse(string.Format(format, arg0, arg1, arg2)) }, isWriteLine: true);

        public static (XConsolePosition Begin, XConsolePosition End) WriteLine(
            string format, params object?[]? arg) =>
            WriteBase(
                new[] { XConsoleItem.Parse(string.Format(format, arg ?? Array.Empty<object?>())) },
                isWriteLine: true);

        public static (XConsolePosition Begin, XConsolePosition End) WriteLine(uint value) =>
            WriteBase(new[] { new XConsoleItem(value.ToString()) }, isWriteLine: true);

        public static (XConsolePosition Begin, XConsolePosition End) WriteLine(ulong value) =>
            WriteBase(new[] { new XConsoleItem(value.ToString()) }, isWriteLine: true);

        #endregion

        #endregion

        #region Private utils

        private static void WriteItems(IReadOnlyList<XConsoleItem> items)
        {
            foreach (var item in items)
            {
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
        }

        private static int GetLineWrapCount(IReadOnlyList<XConsoleItem> items, int beginLeft)
        {
            var lineWrapCount = 0;
            var left = beginLeft;
            var bufferWidth = Console.BufferWidth;
            string chars;
            int charCount, charIndex;
            char chr;

            for (var itemIndex = 0; itemIndex < items.Count; itemIndex++)
            {
                chars = items[itemIndex].Value;
                charCount = chars.Length;

                for (charIndex = 0; charIndex < charCount; charIndex++)
                {
                    chr = chars[charIndex];

                    if (chr >= 32)
                        left++;
                    else
                        switch (chr)
                        {
                            case '\n':
                                left = 0;
                                lineWrapCount++;
                                continue;

                            case '\r':
                                left = 0;
                                continue;

                            case '\t':
                                left = (left + 8) / 8 * 8;
                                break;

                            case '\b':

                                if (left > 0)
                                    left--;

                                continue;

                            default:
                                continue;
                        }

                    if (left >= bufferWidth)
                    {
                        lineWrapCount++;
                        left -= bufferWidth;
                    }
                }
            }

            return lineWrapCount;
        }

        #endregion

        #region Remaining API coverage

        public static ConsoleColor BackgroundColor
        {
            get
            {
                lock (_syncLock)
                    return Console.BackgroundColor;
            }
            set
            {
                lock (_syncLock)
                    Console.BackgroundColor = value;
            }
        }

        public static int BufferHeight
        {
            get
            {
                lock (_syncLock)
                    return Console.BufferHeight;
            }
            set
            {
                lock (_syncLock)
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
                lock (_syncLock)
                    return Console.BufferWidth;
            }
            set
            {
                lock (_syncLock)
                    Console.BufferWidth = value;
            }
        }

        public static bool CapsLock => Console.CapsLock;

        public static int CursorLeft
        {
            get
            {
                lock (_syncLock)
                    return Console.CursorLeft;
            }
            set
            {
                lock (_syncLock)
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
                lock (_syncLock)
                    return Console.CursorTop;
            }
            set
            {
                lock (_syncLock)
                    Console.CursorTop = value;
            }
        }

        public static bool CursorVisible
        {
            get
            {
                lock (_syncLock)
                    return Console.CursorVisible;
            }
            set
            {
                lock (_syncLock)
                    Console.CursorVisible = _cursorVisible = value;
            }
        }

        public static TextWriter Error => Console.Error;

        public static ConsoleColor ForegroundColor
        {
            get
            {
                lock (_syncLock)
                    return Console.ForegroundColor;
            }
            set
            {
                lock (_syncLock)
                    Console.ForegroundColor = value;
            }
        }

        public static TextReader In => Console.In;

        public static Encoding InputEncoding
        {
            get
            {
                lock (_syncLock)
                    return Console.InputEncoding;
            }
            set
            {
                lock (_syncLock)
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
                lock (_syncLock)
                    return Console.OutputEncoding;
            }
            set
            {
                lock (_syncLock)
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
            lock (_syncLock)
                Console.Clear();
        }

        public static (int Left, int Top) GetCursorPosition()
        {
            lock (_syncLock)
#if !NETSTANDARD
                return Console.GetCursorPosition();
#else
                return (Console.CursorLeft, Console.CursorTop);
#endif
        }

        public static void MoveBufferArea(
            int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight, int targetLeft, int targetTop)
        {
            lock (_syncLock)
                Console.MoveBufferArea(
                    sourceLeft: sourceLeft, sourceTop: sourceTop,
                    sourceWidth: sourceWidth, sourceHeight: sourceHeight,
                    targetLeft: targetLeft, targetTop: targetTop);
        }

        public static void MoveBufferArea(
            int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight, int targetLeft, int targetTop,
            char sourceChar, ConsoleColor sourceForeColor, ConsoleColor sourceBackColor)
        {
            lock (_syncLock)
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
            lock (_syncLock)
                return Console.Read();
        }

        public static ConsoleKeyInfo ReadKey()
        {
            lock (_syncLock)
                return Console.ReadKey();
        }

        public static ConsoleKeyInfo ReadKey(bool intercept)
        {
            lock (_syncLock)
                return Console.ReadKey(intercept: intercept);
        }

        public static void ResetColor()
        {
            lock (_syncLock)
                Console.ResetColor();
        }

        public static void SetBufferSize(int width, int height)
        {
            lock (_syncLock)
                Console.SetBufferSize(width: width, height: height);
        }

        public static void SetCursorPosition(int left, int top)
        {
            lock (_syncLock)
                Console.SetCursorPosition(left: left, top: top);
        }

        public static void SetError(TextWriter newError) => Console.SetError(newError: newError);

        public static void SetIn(TextReader newIn) => Console.SetIn(newIn: newIn);

        public static void SetOut(TextWriter newOut) => Console.SetOut(newOut: newOut);

        public static void SetWindowPosition(int left, int top) => Console.SetWindowPosition(left: left, top: top);

        public static void SetWindowSize(int width, int height)
        {
            lock (_syncLock)
                Console.SetWindowSize(width: width, height: height);
        }

        #endregion
    }
}
