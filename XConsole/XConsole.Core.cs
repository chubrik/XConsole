namespace Chubrik.XConsole;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
#if NET9_0_OR_GREATER
using System.Threading;
#endif

public static partial class XConsole
{
    #region Common

    private static readonly string _newLine = Environment.NewLine;
    private static readonly bool _positioningEnabled;

#if NET9_0_OR_GREATER
    private static readonly Lock _syncLock = new();
#else
    private static readonly object _syncLock = new();
#endif

    private static bool _coloringEnabled = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("NO_COLOR"));
    private static bool _cursorVisible;
    private static int _maxTop;
    private static long _shiftTop = 0;

    static XConsole()
    {
        Console.OutputEncoding = Encoding.UTF8;

        try
        {
            _cursorVisible = Console.CursorVisible;
            _maxTop = Console.BufferHeight - 1;
            _positioningEnabled = _maxTop > 0;
        }
        catch { }
    }

    internal static long ShiftTop => _shiftTop;
#if NET
    internal static bool IsColoringEnabled => _coloringEnabled;
#endif

    #endregion

    #region Pinning

    private static Func<IReadOnlyList<string?>>? _getPinValues = null;
    private static int _pinHeight = 0;
    private static int _pinClearWidth = 0;
    private static int _pinClearHeight = 0;
    private static string _pinClear = string.Empty;

    private static void UpdatePinImpl()
    {
        var getPinValues = _getPinValues;

        if (getPinValues == null)
            return;

        var pinValues = getPinValues();
        var pinItems = new ConsoleItem[pinValues.Count];

        for (var i = 0; i < pinValues.Count; i++)
            pinItems[i] = ConsoleItem.Parse(pinValues[i]);

        int logLeft, logTop;

        lock (_syncLock)
        {
            if (_getPinValues == null)
                return;

            if (_pinHeight == 0)
                _pinHeight = 1;

            (logLeft, logTop) = GetCursorPositionNoLock();

            WriteBaseWithPin(logItems: null, isWriteLine: false, pinItems,
                logBeginLeft: logLeft, logBeginTop: ref logTop, out _, out _);
        }
    }

    private static void UnpinImpl()
    {
        int logLeft, logTop;

        lock (_syncLock)
        {
            if (_getPinValues == null)
                return;

            var pinClear = GetPinClear();
            (logLeft, logTop) = GetCursorPositionNoLock();
            Console.CursorVisible = false;
            Console.Write(pinClear);
            Console.SetCursorPosition(logLeft, logTop);
            Console.CursorVisible = _cursorVisible;
            _getPinValues = null;
            _pinHeight = 0;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string GetPinClear()
    {
        var pinClearWidth = Console.BufferWidth;
        var pinClearHeight = _pinHeight;

        if (_pinClearWidth != pinClearWidth || _pinClearHeight != pinClearHeight)
        {
            _pinClearWidth = pinClearWidth;
            _pinClearHeight = pinClearHeight;
            _pinClear = _newLine + new string(' ', pinClearWidth * pinClearHeight - 1);
        }

        return _pinClear;
    }

    #endregion

    #region Positioning

    internal static ConsolePosition CursorPositionImpl
    {
        get
        {
            lock (_syncLock)
            {
                var (left, top) = GetCursorPositionNoLock();
                return new(left: left, top: top, shiftTop: _shiftTop);
            }
        }
        set
        {
            lock (_syncLock)
            {
                var actualTop = Math.Max(int.MinValue, value.InitialTop + value.ShiftTop - _shiftTop);
                Console.SetCursorPosition(value.Left, unchecked((int)actualTop));
            }
        }
    }

    internal static int? GetPositionActualTop(ConsolePosition position)
    {
        long shiftTop;
        int bufferHeight;

        lock (_syncLock)
        {
            shiftTop = _shiftTop;
            bufferHeight = Console.BufferHeight;
        }

        var actualTop = position.InitialTop + position.ShiftTop - shiftTop;
        return actualTop >= 0 && actualTop < bufferHeight ? unchecked((int)actualTop) : null;
    }

    internal static ConsolePosition WriteToPosition(
        ConsolePosition position, ConsoleItem? singleItem, ConsoleItem[] manyItems, bool viewportOnly = false)
    {
        if (!_positioningEnabled)
            return default;

        bool isOnViewport;
        int logLeft, logTop, beginTop, endLeft, endTop;
        long shiftTop;
        ConsoleItem[] items;

        lock (_syncLock)
        {
            shiftTop = _shiftTop;
            (logLeft, logTop) = GetCursorPositionNoLock();
            beginTop = unchecked((int)Math.Max(int.MinValue, position.InitialTop + position.ShiftTop - shiftTop));
            isOnViewport = logTop + _pinHeight - beginTop < Console.WindowHeight;

            if (viewportOnly && !isOnViewport)
                throw new ArgumentOutOfRangeException(nameof(position));

            Console.CursorVisible = false;

            try
            {
                Console.SetCursorPosition(position.Left, beginTop);
            }
            catch
            {
                Console.CursorVisible = _cursorVisible;
                throw;
            }

            items = GetItems(singleItem, manyItems);
            WriteItems(items);
            (endLeft, endTop) = GetCursorPositionNoLock();

            if (endTop == _maxTop)
            {
                var lineWrapCount = GetLineWrapCount(items, position.Left);
                var shift = beginTop - endTop + lineWrapCount;
                shiftTop += shift;
                logTop -= shift;
                _shiftTop = shiftTop;
            }
            else if (_pinHeight > 0 && !isOnViewport)
                Console.SetCursorPosition(left: 0, logTop + _pinHeight);

            Console.SetCursorPosition(logLeft, logTop);
            Console.CursorVisible = _cursorVisible;
        }

        return new(left: endLeft, top: endTop, shiftTop: shiftTop);
    }

    #endregion

    #region ReadLine

    private static string? ReadLineImpl()
    {
        lock (_syncLock)
        {
            var (logBeginLeft, logBeginTop) = GetCursorPositionNoLock();
            var result = Console.ReadLine();

            if (_getPinValues != null)
            {
                Console.CursorVisible = false;
                Console.CursorTop--; //todo calculate
                var pinValues = _getPinValues();
                var pinItems = new ConsoleItem[pinValues.Count];

                for (var i = 0; i < pinValues.Count; i++)
                    pinItems[i] = ConsoleItem.Parse(pinValues[i]);

                WriteBaseWithPin(logItems: null, isWriteLine: false, pinItems,
                    logBeginLeft, ref logBeginTop, out _, out _);
            }

            return result;
        }
    }

    private static string? ReadLineImpl(ConsoleReadLineMode mode, char maskChar)
    {
        switch (mode)
        {
            case ConsoleReadLineMode.Default:
                return ReadLineImpl();

            case ConsoleReadLineMode.Masked:
            case ConsoleReadLineMode.Hidden:
                var isMaskedMode = mode == ConsoleReadLineMode.Masked;
                var result = string.Empty;
                ConsoleKeyInfo keyInfo;

                lock (_syncLock)
                {
                    do
                    {
                        keyInfo = Console.ReadKey(intercept: true);

                        if (keyInfo.Key == ConsoleKey.Backspace && result.Length > 0)
                        {
                            if (isMaskedMode)
                                Console.Write("\b \b");

                            result = result[..^1];
                        }
                        else if (
                            !char.IsControl(keyInfo.KeyChar) ||
                            keyInfo.Key == ConsoleKey.Tab ||
                            keyInfo.KeyChar == '\x1a')
                        {
#if NET
                            if (!OperatingSystem.IsWindows() && keyInfo.KeyChar == '\x1a')
                            {
                                WriteLineImpl();
                                return null;
                            }
#endif
                            if (isMaskedMode)
                                Console.Write(maskChar);

                            result += keyInfo.KeyChar;
                        }
                    }
                    while (keyInfo.Key != ConsoleKey.Enter);

                    WriteLineImpl();
                }

                if (result.Length > 0 && result[0] == '\x1a')
                    return null;

                return result;

            default:
                throw new ArgumentOutOfRangeException(nameof(mode));
        }
    }

    #endregion

    #region Writing

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static (ConsolePosition Begin, ConsolePosition End) WriteParced(
        string? value, bool isWriteLine)
    {
        return WriteBase(singleLogItem: ConsoleItem.Parse(value), manyLogItems: [], isWriteLine);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static (ConsolePosition Begin, ConsolePosition End) WriteParced(
        IReadOnlyList<string?> values, bool isWriteLine)
    {
        var logItems = new ConsoleItem[values.Count];

        for (var i = 0; i < values.Count; i++)
            logItems[i] = ConsoleItem.Parse(values[i]);

        return WriteBase(singleLogItem: null, manyLogItems: logItems, isWriteLine);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static (ConsolePosition Begin, ConsolePosition End) WritePlain(
        string value, bool isWriteLine)
    {
        return WriteBase(singleLogItem: new ConsoleItem(value), manyLogItems: [], isWriteLine);
    }

    internal static (ConsolePosition Begin, ConsolePosition End) WriteLineImpl()
    {
        if (!_positioningEnabled)
        {
            lock (_syncLock)
                Console.WriteLine();

            return (default, default);
        }

        var getPinValues = _getPinValues;
        int logLeft, logTop;
        long shiftTop;

        if (getPinValues == null)
        {
            lock (_syncLock)
            {
                (logLeft, logTop) = GetCursorPositionNoLock();
                Console.WriteLine();

                if (logTop == _maxTop)
                {
                    _shiftTop++;
                    logTop--;
                }

                shiftTop = _shiftTop;
            }
        }
        else
        {
            var pinValues = getPinValues();
            var pinItems = new ConsoleItem[pinValues.Count];

            for (var i = 0; i < pinValues.Count; i++)
                pinItems[i] = ConsoleItem.Parse(pinValues[i]);

            lock (_syncLock)
            {
                (logLeft, logTop) = GetCursorPositionNoLock();

                if (_getPinValues == null)
                {
                    Console.WriteLine();

                    if (logTop == _maxTop)
                    {
                        _shiftTop++;
                        logTop--;
                    }
                }
                else
                {
                    WriteBaseWithPin(logItems: null, isWriteLine: true, pinItems,
                        logBeginLeft: logLeft, logBeginTop: ref logTop, out _, out _);
                }

                shiftTop = _shiftTop;
            }
        }

        return (
            new(left: logLeft, top: logTop, shiftTop: shiftTop),
            new(left: logLeft, top: logTop, shiftTop: shiftTop)
        );
    }

    private static (ConsolePosition Begin, ConsolePosition End) WriteBase(
        ConsoleItem? singleLogItem, ConsoleItem[] manyLogItems, bool isWriteLine)
    {
        ConsoleItem[] logItems;

        if (!_positioningEnabled)
        {
            lock (_syncLock)
            {
                logItems = GetItems(singleLogItem, manyLogItems);
                WriteItems(logItems);

                if (isWriteLine)
                    Console.WriteLine();
            }

            return (default, default);
        }

        var getPinValues = _getPinValues;
        int logBeginLeft, logBeginTop, logEndLeft, logEndTop;
        long shiftTop;

        if (getPinValues == null)
        {
            lock (_syncLock)
            {
                (logBeginLeft, logBeginTop) = GetCursorPositionNoLock();
                logItems = GetItems(singleLogItem, manyLogItems);

                WriteBaseNoPin(logItems, isWriteLine,
                    logBeginLeft, logBeginTop, out logEndLeft, out logEndTop);

                shiftTop = _shiftTop;
            }
        }
        else
        {
            var pinValues = getPinValues();
            var pinItems = new ConsoleItem[pinValues.Count];

            for (var i = 0; i < pinValues.Count; i++)
                pinItems[i] = ConsoleItem.Parse(pinValues[i]);

            lock (_syncLock)
            {
                (logBeginLeft, logBeginTop) = GetCursorPositionNoLock();
                logItems = GetItems(singleLogItem, manyLogItems);

                if (_getPinValues == null)
                {
                    WriteBaseNoPin(logItems, isWriteLine,
                        logBeginLeft, logBeginTop, out logEndLeft, out logEndTop);
                }
                else
                {
                    WriteBaseWithPin(logItems, isWriteLine, pinItems,
                        logBeginLeft, ref logBeginTop, out logEndLeft, out logEndTop);
                }

                shiftTop = _shiftTop;
            }
        }

        return (
            new(left: logBeginLeft, top: logBeginTop, shiftTop: shiftTop),
            new(left: logEndLeft, top: logEndTop, shiftTop: shiftTop)
        );
    }

    private static void WriteBaseNoPin(
        ConsoleItem[] logItems, bool isWriteLine,
        int logBeginLeft, int logBeginTop, out int logEndLeft, out int logEndTop)
    {
        if (isWriteLine || logItems.Length > 1)
            Console.CursorVisible = false;

        WriteItems(logItems);
        (logEndLeft, logEndTop) = GetCursorPositionNoLock();

        if (isWriteLine)
            Console.WriteLine();

        Console.CursorVisible = _cursorVisible;

        if (logEndTop == _maxTop)
        {
            var logEndLineWrap = isWriteLine ? 1 : 0;
            var logLineWrapCount = GetLineWrapCount(logItems, logBeginLeft);
            var shift = logBeginTop - logEndTop + logLineWrapCount + logEndLineWrap;
            _shiftTop += shift;
            logBeginTop -= shift;
            logEndTop = logBeginTop + logLineWrapCount;
        }
    }

    private static void WriteBaseWithPin(
        ConsoleItem[]? logItems, bool isWriteLine, ConsoleItem[] pinItems,
        int logBeginLeft, ref int logBeginTop, out int logEndLeft, out int logEndTop)
    {
        var pinClear = GetPinClear();
        Console.CursorVisible = false;
        Console.Write(pinClear);
        Console.SetCursorPosition(logBeginLeft, logBeginTop);

        if (logItems != null)
        {
            WriteItems(logItems);
            (logEndLeft, logEndTop) = GetCursorPositionNoLock();
        }
        else
            (logEndLeft, logEndTop) = (logBeginLeft, logBeginTop);

        Console.WriteLine(isWriteLine ? _newLine : string.Empty);
        WriteItems(pinItems);
        var pinEndTop = Console.CursorTop;
        var logEndLineWrap = isWriteLine ? 1 : 0;

        if (pinEndTop == _maxTop)
        {
            var logLineWrapCount = logItems != null ? GetLineWrapCount(logItems, logBeginLeft) : 0;
            _pinHeight = 1 + GetLineWrapCount(pinItems, beginLeft: 0);
            var shift = logBeginTop - pinEndTop + logLineWrapCount + logEndLineWrap + _pinHeight;
            _shiftTop += shift;
            logBeginTop -= shift;
            logEndTop = logBeginTop + logLineWrapCount;
        }
        else
            _pinHeight = pinEndTop - logEndTop - logEndLineWrap;

        Console.SetCursorPosition(logEndLeft, logEndTop);

        if (isWriteLine)
            Console.WriteLine();

        Console.CursorVisible = _cursorVisible;
    }

    #endregion

    #region Internal utils

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static (int Left, int Top) GetCursorPositionNoLock()
    {
#if NET
        return Console.GetCursorPosition();
#else
        return (Console.CursorLeft, Console.CursorTop);
#endif
    }

    private static readonly ConsoleItem[] _singleItemArray = new ConsoleItem[1];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ConsoleItem[] GetItems(ConsoleItem? singleItem, ConsoleItem[] manyItems)
    {
        if (singleItem != null)
        {
            _singleItemArray[0] = singleItem.Value;
            return _singleItemArray;
        }

        return manyItems;
    }

    internal static void WriteItemsIsolated(ConsoleItem[] items)
    {
        if (items.Length > 1)
        {
            Console.CursorVisible = false;
            WriteItems(items);
            Console.CursorVisible = _cursorVisible;
        }
        else
            WriteItems(items);
    }

    private static void WriteItems(ConsoleItem[] items)
    {
        if (!_coloringEnabled)
        {
            for (var i = 0; i < items.Length; i++)
                Console.Write(items[i].Value);

            return;
        }

        ConsoleItem item;

        for (var i = 0; i < items.Length; i++)
        {
            item = items[i];

            switch (item.Type)
            {
                case ConsoleItemType.Plain:
                {
                    Console.Write(item.Value);
                    continue;
                }
                case ConsoleItemType.ForeColor:
                {
                    var origForeColor = Console.ForegroundColor;
                    Console.ForegroundColor = item.ForeColor;
                    Console.Write(item.Value);
                    Console.ForegroundColor = origForeColor;
                    continue;
                }
                case ConsoleItemType.BackColor:
                {
                    var origBackColor = Console.BackgroundColor;
                    Console.BackgroundColor = item.BackColor;
                    Console.Write(item.Value);
                    Console.BackgroundColor = origBackColor;
                    continue;
                }
                case ConsoleItemType.BothColors:
                {
                    var origForeColor = Console.ForegroundColor;
                    var origBackColor = Console.BackgroundColor;
                    Console.ForegroundColor = item.ForeColor;
                    Console.BackgroundColor = item.BackColor;
                    Console.Write(item.Value);
                    Console.ForegroundColor = origForeColor;
                    Console.BackgroundColor = origBackColor;
                    continue;
                }
                case ConsoleItemType.Ansi:
                {
                    var origForeColor = Console.ForegroundColor;
                    var origBackColor = Console.BackgroundColor;
                    Console.Write(item.Value);
                    Console.ForegroundColor = origForeColor;
                    Console.BackgroundColor = origBackColor;
                    continue;
                }
                default:
                    throw new InvalidOperationException();
            }
        }
    }

    private static int GetLineWrapCount(ConsoleItem[] items, int beginLeft)
    {
        var left = beginLeft;
        var bufferWidth = Console.BufferWidth;
        var result = 0;
        string chars;
        int charCount, charIndex, @char;

        for (var itemIndex = 0; itemIndex < items.Length; itemIndex++)
        {
            chars = items[itemIndex].Value;
            charCount = chars.Length;

            for (charIndex = 0; charIndex < charCount; charIndex++)
            {
                @char = chars[charIndex];

                if (@char >= ' ')
                    left++;
                else
                    switch (@char)
                    {
                        case '\n':
                            left = 0;
                            result++;
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

                        case '\x1b':
#if NET
                            if (VirtualTerminal.IsEnabled)
                            {
                                for (charIndex++; charIndex < charCount; charIndex++)
                                {
                                    @char = chars[charIndex];

                                    if ((@char >= 'A' && @char <= 'Z') || (@char >= 'a' && @char <= 'z'))
                                        break;
                                }

                                continue;
                            }
                            else
#endif
                            {
                                left++;
                                break;
                            }

                        default:
                            continue;
                    }

                if (left >= bufferWidth)
                {
                    result++;
                    left -= bufferWidth;
                }
            }
        }

        return result;
    }

    #endregion
}
