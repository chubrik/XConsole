#if !NET
#pragma warning disable CS8604 // Possible null reference argument.
#endif

namespace Chubrik.Console;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

#if NET
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
    private static readonly bool _positioningEnabled = true;
    private static bool _coloringEnabled = Environment.GetEnvironmentVariable("NO_COLOR") == null;
    private static long _shiftTop = 0;
    private static bool _cursorVisible;
    private static int _maxTop;

    static XConsole()
    {
        Console.OutputEncoding = Encoding.UTF8;

        try
        {
            _cursorVisible = Console.CursorVisible;
            _maxTop = Console.BufferHeight - 1;
        }
        catch
        {
            _positioningEnabled = false;
        }
    }

    internal static long ShiftTop => _shiftTop;

    public static bool NO_COLOR
    {
        get => !_coloringEnabled;
        set => _coloringEnabled = !value;
    }

    public static void Sync(Action action)
    {
        lock (_syncLock)
            action();
    }

    #region Pinning

    private static Func<IReadOnlyList<string?>>? _getPinValues = null;
    private static int _pinHeight = 0;

    [Obsolete("At least one argument should be specified.", error: true)]
    public static void Pin() => throw new InvalidOperationException();

    public static void Pin(params string?[] values)
    {
        if (!_positioningEnabled)
            return;

        _getPinValues = () => values;
        UpdatePin();
    }

    public static void Pin(Func<string?> getValue)
    {
        if (!_positioningEnabled)
            return;

        _getPinValues = () => new[] { getValue() };
        UpdatePin();
    }

    public static void Pin(Func<IReadOnlyList<string?>> getValues)
    {
        if (!_positioningEnabled)
            return;

        _getPinValues = getValues;
        UpdatePin();
    }

    public static void UpdatePin()
    {
        if (!_positioningEnabled)
            return;

        var getPinValues = _getPinValues;

        if (getPinValues == null)
            return;

        var prePinHeight = _pinHeight;

        var pinClear = prePinHeight > 0
            ? _newLine + new string(' ', Console.BufferWidth * prePinHeight - 1)
            : string.Empty;

        var pinValues = getPinValues();
        var pinItems = new List<ConsoleItem>(pinValues.Count);

        foreach (var pinValue in pinValues)
            if (!string.IsNullOrEmpty(pinValue))
                pinItems.Add(ConsoleItem.Parse(pinValue));

        int pinHeight, origLeft, origTop, pinEndTop;

        lock (_syncLock)
        {
            if (_getPinValues == null)
                return;

            pinHeight = _pinHeight;

            if (pinHeight > 0)
            {
                if (pinHeight != prePinHeight)
                    pinClear = _newLine + new string(' ', Console.BufferWidth * pinHeight - 1);

#if NET
                (origLeft, origTop) = Console.GetCursorPosition();
#else
                (origLeft, origTop) = (Console.CursorLeft, Console.CursorTop);
#endif
                Console.CursorVisible = false;
                Console.Write(pinClear);
                Console.SetCursorPosition(0, origTop);
            }
            else
            {
#if NET
                (origLeft, origTop) = Console.GetCursorPosition();
#else
                (origLeft, origTop) = (Console.CursorLeft, Console.CursorTop);
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
                _shiftTop += shift;
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
        if (!_positioningEnabled)
            return;

        var prePinHeight = _pinHeight;

        var pinClear = prePinHeight > 0
            ? _newLine + new string(' ', Console.BufferWidth * prePinHeight - 1)
            : string.Empty;

        int pinHeight, origLeft, origTop;

        lock (_syncLock)
        {
            if (_getPinValues == null)
                return;

            pinHeight = _pinHeight;

            if (pinHeight > 0)
            {
                if (pinHeight != prePinHeight)
                    pinClear = _newLine + new string(' ', Console.BufferWidth * pinHeight - 1);

#if NET
                (origLeft, origTop) = Console.GetCursorPosition();
#else
                (origLeft, origTop) = (Console.CursorLeft, Console.CursorTop);
#endif
                Console.CursorVisible = false;
                Console.Write(pinClear);
                Console.SetCursorPosition(origLeft, origTop);
                Console.CursorVisible = _cursorVisible;
                _pinHeight = 0;
            }

            _getPinValues = null;
        }
    }

    #endregion

    #region Positioning

    public static ConsolePosition CursorPosition
    {
        get
        {
            lock (_syncLock)
            {
#if NET
                var (left, top) = Console.GetCursorPosition();
                return new(left: left, top: top, shiftTop: _shiftTop);
#else
                return new(left: Console.CursorLeft, top: Console.CursorTop, shiftTop: _shiftTop);
#endif
            }
        }
        set
        {
            lock (_syncLock)
            {
                var actualTop = value.InitialTop + value.ShiftTop - _shiftTop;

                if (actualTop < int.MinValue)
                    actualTop = int.MinValue;

                if (actualTop > int.MaxValue)
                    actualTop = int.MaxValue;

                Console.SetCursorPosition(value.Left, (int)actualTop);
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
        return actualTop >= 0 && actualTop < bufferHeight ? (int)actualTop : null;
    }

    internal static ConsolePosition WriteToPosition(ConsolePosition position, string?[] values)
    {
        if (!_positioningEnabled)
            return new(0, 0, 0);

        var items = new List<ConsoleItem>(values.Length);

        foreach (var value in values)
            if (!string.IsNullOrEmpty(value))
                items.Add(ConsoleItem.Parse(value));

        if (items.Count == 0)
            return position;

        int origLeft, origTop, endLeft, endTop;
        long shiftTop, positionActualTop;

        lock (_syncLock)
        {
            shiftTop = _shiftTop;
            positionActualTop = position.InitialTop + position.ShiftTop - shiftTop;

            if (positionActualTop < int.MinValue)
                positionActualTop = int.MinValue;

            if (positionActualTop > int.MaxValue)
                positionActualTop = int.MaxValue;

#if NET
            (origLeft, origTop) = Console.GetCursorPosition();
#else
            (origLeft, origTop) = (Console.CursorLeft, Console.CursorTop);
#endif
            Console.CursorVisible = false;

            try
            {
                Console.SetCursorPosition(position.Left, (int)positionActualTop);
            }
            catch
            {
                Console.CursorVisible = _cursorVisible;
                throw;
            }

            WriteItems(items);
#if NET
            (endLeft, endTop) = Console.GetCursorPosition();
#else
            (endLeft, endTop) = (Console.CursorLeft, Console.CursorTop);
#endif

            if (endTop == _maxTop)
            {
                var lineWrapCount = GetLineWrapCount(items, origLeft);
                var shift = origTop + lineWrapCount - endTop;
                shiftTop += shift;
                origTop -= shift;
                _shiftTop = shiftTop;
            }
            else if (_pinHeight > 0)
                Console.SetCursorPosition(0, origTop + _pinHeight);

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

    public static string ReadLine(ConsoleReadLineMode mode, char maskChar = '\u2022')
    {
        switch (mode)
        {
            case ConsoleReadLineMode.Default:
                return ReadLine();

            case ConsoleReadLineMode.Masked:
            case ConsoleReadLineMode.Hidden:
                var isMaskedMode = mode == ConsoleReadLineMode.Masked;
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

    [Obsolete("At least one argument should be specified.", error: true)]
    public static void Write() => throw new InvalidOperationException();

    public static (ConsolePosition Begin, ConsolePosition End) Write(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            var position = CursorPosition;
            return (position, position);
        }

        return WriteBase(new[] { ConsoleItem.Parse(value) }, isWriteLine: false);
    }

    public static (ConsolePosition Begin, ConsolePosition End) Write(params string?[] values)
    {
        var items = new List<ConsoleItem>(values.Length);

        foreach (var value in values)
            if (!string.IsNullOrEmpty(value))
                items.Add(ConsoleItem.Parse(value));

        if (items.Count == 0)
        {
            var position = CursorPosition;
            return (position, position);
        }

        return WriteBase(items, isWriteLine: false);
    }

    public static (ConsolePosition Begin, ConsolePosition End) Write(IEnumerable<string?> values)
    {
        var items = new List<ConsoleItem>();

        foreach (var value in values)
            if (!string.IsNullOrEmpty(value))
                items.Add(ConsoleItem.Parse(value));

        if (items.Count == 0)
        {
            var position = CursorPosition;
            return (position, position);
        }

        return WriteBase(items, isWriteLine: false);
    }

    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return WriteLine();

        return WriteBase(new[] { ConsoleItem.Parse(value) }, isWriteLine: true);
    }

    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(params string?[] values)
    {
        var items = new List<ConsoleItem>(values.Length);

        foreach (var value in values)
            if (!string.IsNullOrEmpty(value))
                items.Add(ConsoleItem.Parse(value));

        if (items.Count == 0)
            return WriteLine();

        return WriteBase(items, isWriteLine: true);
    }

    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(IEnumerable<string?> values)
    {
        var items = new List<ConsoleItem>();

        foreach (var value in values)
            if (!string.IsNullOrEmpty(value))
                items.Add(ConsoleItem.Parse(value));

        if (items.Count == 0)
            return WriteLine();

        return WriteBase(items, isWriteLine: true);
    }

    public static (ConsolePosition Begin, ConsolePosition End) WriteLine()
    {
        if (!_positioningEnabled)
        {
            lock (_syncLock)
                Console.WriteLine();

            return (new(0, 0, 0), new(0, 0, 0));
        }

        var getPinValues = _getPinValues;
        int origLeft, origTop;
        long shiftTop;

        if (getPinValues == null)
        {
            lock (_syncLock)
            {
#if NET
                (origLeft, origTop) = Console.GetCursorPosition();
#else
                (origLeft, origTop) = (Console.CursorLeft, Console.CursorTop);
#endif
                Console.WriteLine();

                if (origTop == _maxTop)
                {
                    _shiftTop++;
                    origTop--;
                }

                shiftTop = _shiftTop;
            }
        }
        else
        {
            var prePinHeight = _pinHeight;

            var pinClear = prePinHeight > 0
                ? _newLine + new string(' ', Console.BufferWidth * prePinHeight - 1)
                : string.Empty;

            var pinValues = getPinValues();
            var pinItems = new List<ConsoleItem>(pinValues.Count);

            foreach (var pinValue in pinValues)
                if (!string.IsNullOrEmpty(pinValue))
                    pinItems.Add(ConsoleItem.Parse(pinValue));

            int pinHeight, pinEndTop;

            lock (_syncLock)
            {
                if (_getPinValues == null)
                {
#if NET
                    (origLeft, origTop) = Console.GetCursorPosition();
#else
                    (origLeft, origTop) = (Console.CursorLeft, Console.CursorTop);
#endif
                    Console.WriteLine();

                    if (origTop == _maxTop)
                    {
                        _shiftTop++;
                        origTop--;
                    }
                }
                else
                {
                    pinHeight = _pinHeight;

                    if (pinHeight > 0)
                    {
                        if (pinHeight != prePinHeight)
                            pinClear = _newLine + new string(' ', Console.BufferWidth * pinHeight - 1);

#if NET
                        (origLeft, origTop) = Console.GetCursorPosition();
#else
                        (origLeft, origTop) = (Console.CursorLeft, Console.CursorTop);
#endif
                        Console.CursorVisible = false;
                        Console.Write(pinClear);
                        Console.SetCursorPosition(origLeft, origTop);
                    }
                    else
                    {
#if NET
                        (origLeft, origTop) = Console.GetCursorPosition();
#else
                        (origLeft, origTop) = (Console.CursorLeft, Console.CursorTop);
#endif
                        Console.CursorVisible = false;
                    }

                    Console.WriteLine(_newLine);
                    WriteItems(pinItems);
                    pinEndTop = Console.CursorTop;

                    if (pinEndTop == _maxTop)
                    {
                        _pinHeight = 1 + GetLineWrapCount(pinItems, beginLeft: 0);
                        var shift = origTop + 1 + _pinHeight - _maxTop;
                        _shiftTop += shift;
                        origTop -= shift;
                    }
                    else
                        _pinHeight = pinEndTop - (origTop + 1);

                    Console.SetCursorPosition(0, origTop);
                    Console.WriteLine();
                    Console.CursorVisible = _cursorVisible;
                }

                shiftTop = _shiftTop;
            }
        }

        return (
            new(left: origLeft, top: origTop, shiftTop: shiftTop),
            new(left: origLeft, top: origTop, shiftTop: shiftTop)
        );
    }

    private static (ConsolePosition Begin, ConsolePosition End) WriteBase(
        IReadOnlyList<ConsoleItem> logItems, bool isWriteLine)
    {
        Debug.Assert(logItems.Count > 0);

        if (!_positioningEnabled)
        {
            lock (_syncLock)
            {
                WriteItems(logItems);

                if (isWriteLine)
                    Console.WriteLine();
            }

            return (new(0, 0, 0), new(0, 0, 0));
        }

        var getPinValues = _getPinValues;
        int beginLeft, beginTop, endLeft, endTop;
        long shiftTop;

        if (getPinValues == null)
        {
            lock (_syncLock)
            {
#if NET
                (beginLeft, beginTop) = Console.GetCursorPosition();
                Console.CursorVisible = false;
                WriteItems(logItems);
                (endLeft, endTop) = Console.GetCursorPosition();
#else
                (beginLeft, beginTop) = (Console.CursorLeft, Console.CursorTop);
                Console.CursorVisible = false;
                WriteItems(logItems);
                (endLeft, endTop) = (Console.CursorLeft, Console.CursorTop);
#endif

                if (isWriteLine)
                {
                    Console.WriteLine();
                    Console.CursorVisible = _cursorVisible;

                    if (endTop == _maxTop)
                    {
                        var logLineWrapCount = GetLineWrapCount(logItems, beginLeft);
                        var shift = beginTop + logLineWrapCount + 1 - endTop;
                        _shiftTop += shift;
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
                        _shiftTop += shift;
                        beginTop -= shift;
                    }
                }

                shiftTop = _shiftTop;
            }
        }
        else
        {
            var prePinHeight = _pinHeight;

            var pinClear = prePinHeight > 0
                ? _newLine + new string(' ', Console.BufferWidth * prePinHeight - 1)
                : string.Empty;

            var pinValues = getPinValues();
            var pinItems = new List<ConsoleItem>(pinValues.Count);

            foreach (var pinValue in pinValues)
                if (!string.IsNullOrEmpty(pinValue))
                    pinItems.Add(ConsoleItem.Parse(pinValue));

            int pinHeight, pinEndTop;

            lock (_syncLock)
            {
                if (_getPinValues == null)
                {
#if NET
                    (beginLeft, beginTop) = Console.GetCursorPosition();
                    Console.CursorVisible = false;
                    WriteItems(logItems);
                    (endLeft, endTop) = Console.GetCursorPosition();
#else
                    (beginLeft, beginTop) = (Console.CursorLeft, Console.CursorTop);
                    Console.CursorVisible = false;
                    WriteItems(logItems);
                    (endLeft, endTop) = (Console.CursorLeft, Console.CursorTop);
#endif

                    if (isWriteLine)
                    {
                        Console.WriteLine();
                        Console.CursorVisible = _cursorVisible;

                        if (endTop == _maxTop)
                        {
                            var logLineWrapCount = GetLineWrapCount(logItems, beginLeft);
                            var shift = beginTop + logLineWrapCount + 1 - endTop;
                            _shiftTop += shift;
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
                            _shiftTop += shift;
                            beginTop -= shift;
                        }
                    }
                }
                else
                {
                    pinHeight = _pinHeight;

                    if (pinHeight > 0)
                    {
                        if (pinHeight != prePinHeight)
                            pinClear = _newLine + new string(' ', Console.BufferWidth * pinHeight - 1);

#if NET
                        (beginLeft, beginTop) = Console.GetCursorPosition();
#else
                        (beginLeft, beginTop) = (Console.CursorLeft, Console.CursorTop);
#endif
                        Console.CursorVisible = false;
                        Console.Write(pinClear);
                        Console.SetCursorPosition(beginLeft, beginTop);
                    }
                    else
                    {
#if NET
                        (beginLeft, beginTop) = Console.GetCursorPosition();
#else
                        (beginLeft, beginTop) = (Console.CursorLeft, Console.CursorTop);
#endif
                        Console.CursorVisible = false;
                    }

                    WriteItems(logItems);
#if NET
                    (endLeft, endTop) = Console.GetCursorPosition();
#else
                    (endLeft, endTop) = (Console.CursorLeft, Console.CursorTop);
#endif

                    if (isWriteLine)
                    {
                        Console.WriteLine(_newLine);
                        WriteItems(pinItems);
                        pinEndTop = Console.CursorTop;

                        if (pinEndTop == _maxTop)
                        {
                            var logLineWrapCount = GetLineWrapCount(logItems, beginLeft);
                            _pinHeight = 1 + GetLineWrapCount(pinItems, beginLeft: 0);
                            var shift = beginTop + logLineWrapCount + 1 + _pinHeight - _maxTop;
                            _shiftTop += shift;
                            beginTop -= shift;
                            endTop = _maxTop - _pinHeight - 1;
                        }
                        else
                            _pinHeight = pinEndTop - (endTop + 1);

                        Console.SetCursorPosition(0, endTop);
                        Console.WriteLine();
                    }
                    else
                    {
                        Console.WriteLine();
                        WriteItems(pinItems);
                        pinEndTop = Console.CursorTop;

                        if (pinEndTop == _maxTop)
                        {
                            var logLineWrapCount = GetLineWrapCount(logItems, beginLeft);
                            _pinHeight = 1 + GetLineWrapCount(pinItems, beginLeft: 0);
                            var shift = beginTop + logLineWrapCount + _pinHeight - _maxTop;
                            _shiftTop += shift;
                            beginTop -= shift;
                            endTop = _maxTop - _pinHeight;
                        }
                        else
                            _pinHeight = pinEndTop - endTop;

                        Console.SetCursorPosition(endLeft, endTop);
                    }

                    Console.CursorVisible = _cursorVisible;
                }

                shiftTop = _shiftTop;
            }
        }

        return (
            new(left: beginLeft, top: beginTop, shiftTop: shiftTop),
            new(left: endLeft, top: endTop, shiftTop: shiftTop)
        );
    }

    #region Overloads

    public static (ConsolePosition Begin, ConsolePosition End) Write(bool value) =>
        WriteBase(new[] { new ConsoleItem(value.ToString()) }, isWriteLine: false);

    public static (ConsolePosition Begin, ConsolePosition End) Write(char value) =>
        WriteBase(new[] { new ConsoleItem(value.ToString()) }, isWriteLine: false);

    public static (ConsolePosition Begin, ConsolePosition End) Write(char[]? buffer)
    {
        var value = buffer?.ToString();

        if (string.IsNullOrEmpty(value))
        {
            var position = CursorPosition;
            return (position, position);
        }

        return WriteBase(new[] { new ConsoleItem(value) }, isWriteLine: false);
    }

    public static (ConsolePosition Begin, ConsolePosition End) Write(char[] buffer, int index, int count)
    {
        var value = buffer.ToString()?.Substring(index, count);

        if (string.IsNullOrEmpty(value))
        {
            var position = CursorPosition;
            return (position, position);
        }

        return WriteBase(new[] { new ConsoleItem(value) }, isWriteLine: false);
    }

    public static (ConsolePosition Begin, ConsolePosition End) Write(decimal value) =>
        WriteBase(new[] { new ConsoleItem(value.ToString()) }, isWriteLine: false);

    public static (ConsolePosition Begin, ConsolePosition End) Write(double value) =>
        WriteBase(new[] { new ConsoleItem(value.ToString()) }, isWriteLine: false);

    public static (ConsolePosition Begin, ConsolePosition End) Write(int value) =>
        WriteBase(new[] { new ConsoleItem(value.ToString()) }, isWriteLine: false);

    public static (ConsolePosition Begin, ConsolePosition End) Write(long value) =>
        WriteBase(new[] { new ConsoleItem(value.ToString()) }, isWriteLine: false);

    public static (ConsolePosition Begin, ConsolePosition End) Write(object? value)
    {
        var valueStr = value?.ToString();

        if (string.IsNullOrEmpty(valueStr))
        {
            var position = CursorPosition;
            return (position, position);
        }

        return WriteBase(new[] { new ConsoleItem(valueStr) }, isWriteLine: false);
    }

    public static (ConsolePosition Begin, ConsolePosition End) Write(float value) =>
        WriteBase(new[] { new ConsoleItem(value.ToString()) }, isWriteLine: false);

    public static (ConsolePosition Begin, ConsolePosition End) Write(
        string format, object? arg0)
    {
        if (string.IsNullOrEmpty(format))
        {
            var position = CursorPosition;
            return (position, position);
        }

        return WriteBase(new[] { ConsoleItem.Parse(string.Format(format, arg0)) }, isWriteLine: false);
    }

    public static (ConsolePosition Begin, ConsolePosition End) Write(
        string format, object? arg0, object? arg1)
    {
        if (string.IsNullOrEmpty(format))
        {
            var position = CursorPosition;
            return (position, position);
        }

        return WriteBase(new[] { ConsoleItem.Parse(string.Format(format, arg0, arg1)) }, isWriteLine: false);
    }

    public static (ConsolePosition Begin, ConsolePosition End) Write(
        string format, object? arg0, object? arg1, object? arg2)
    {
        if (string.IsNullOrEmpty(format))
        {
            var position = CursorPosition;
            return (position, position);
        }

        return WriteBase(new[] { ConsoleItem.Parse(string.Format(format, arg0, arg1, arg2)) }, isWriteLine: false);
    }

    public static (ConsolePosition Begin, ConsolePosition End) Write(
        string format, params object?[]? arg)
    {
        if (string.IsNullOrEmpty(format))
        {
            var position = CursorPosition;
            return (position, position);
        }

        return WriteBase(
            new[] { ConsoleItem.Parse(string.Format(format, arg ?? Array.Empty<object?>())) },
            isWriteLine: false);
    }

    public static (ConsolePosition Begin, ConsolePosition End) Write(uint value) =>
        WriteBase(new[] { new ConsoleItem(value.ToString()) }, isWriteLine: false);

    public static (ConsolePosition Begin, ConsolePosition End) Write(ulong value) =>
        WriteBase(new[] { new ConsoleItem(value.ToString()) }, isWriteLine: false);

    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(bool value) =>
        WriteBase(new[] { new ConsoleItem(value.ToString()) }, isWriteLine: true);

    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(char value) =>
        WriteBase(new[] { new ConsoleItem(value.ToString()) }, isWriteLine: true);

    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(char[]? buffer)
    {
        var value = buffer?.ToString();

        if (string.IsNullOrEmpty(value))
            return WriteLine();

        return WriteBase(new[] { new ConsoleItem(value) }, isWriteLine: true);
    }

    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(char[] buffer, int index, int count)
    {
        var value = buffer.ToString()?.Substring(index, count);

        if (string.IsNullOrEmpty(value))
            return WriteLine();

        return WriteBase(new[] { new ConsoleItem(value) }, isWriteLine: true);
    }

    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(decimal value) =>
        WriteBase(new[] { new ConsoleItem(value.ToString()) }, isWriteLine: true);

    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(double value) =>
        WriteBase(new[] { new ConsoleItem(value.ToString()) }, isWriteLine: true);

    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(int value) =>
        WriteBase(new[] { new ConsoleItem(value.ToString()) }, isWriteLine: true);

    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(long value) =>
        WriteBase(new[] { new ConsoleItem(value.ToString()) }, isWriteLine: true);

    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(object? value)
    {
        var valueStr = value?.ToString();

        if (string.IsNullOrEmpty(valueStr))
            return WriteLine();

        return WriteBase(new[] { new ConsoleItem(valueStr) }, isWriteLine: true);
    }

    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(float value) =>
        WriteBase(new[] { new ConsoleItem(value.ToString()) }, isWriteLine: true);

    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(
        string format, object? arg0)
    {
        if (string.IsNullOrEmpty(format))
            return WriteLine();

        return WriteBase(new[] { ConsoleItem.Parse(string.Format(format, arg0)) }, isWriteLine: true);
    }

    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(
        string format, object? arg0, object? arg1)
    {
        if (string.IsNullOrEmpty(format))
            return WriteLine();

        return WriteBase(new[] { ConsoleItem.Parse(string.Format(format, arg0, arg1)) }, isWriteLine: true);
    }

    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(
        string format, object? arg0, object? arg1, object? arg2)
    {
        if (string.IsNullOrEmpty(format))
            return WriteLine();

        return WriteBase(new[] { ConsoleItem.Parse(string.Format(format, arg0, arg1, arg2)) }, isWriteLine: true);
    }

    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(
        string format, params object?[]? arg)
    {
        if (string.IsNullOrEmpty(format))
            return WriteLine();

        return WriteBase(
            new[] { ConsoleItem.Parse(string.Format(format, arg ?? Array.Empty<object?>())) },
            isWriteLine: true);
    }

    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(uint value) =>
        WriteBase(new[] { new ConsoleItem(value.ToString()) }, isWriteLine: true);

    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(ulong value) =>
        WriteBase(new[] { new ConsoleItem(value.ToString()) }, isWriteLine: true);

    #endregion

    #endregion

    #region Private utils

    private static void WriteItems(IReadOnlyList<ConsoleItem> items)
    {
        if (_coloringEnabled)
        {
            foreach (var item in items)
            {
                if (item.BackColor == ConsoleItem.NoColor)
                {
                    if (item.ForeColor == ConsoleItem.NoColor)
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
                    if (item.ForeColor == ConsoleItem.NoColor)
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
        else
            foreach (var item in items)
                Console.Write(item.Value);
    }

    private static int GetLineWrapCount(IReadOnlyList<ConsoleItem> items, int beginLeft)
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

    #region Remaining API

    public static ConsoleColor BackgroundColor
    {
        get
        {
            lock (_syncLock)
                return Console.BackgroundColor;
        }
        set
        {
            if (_coloringEnabled)
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
            if (_coloringEnabled)
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
#if NET
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

#if !NETSTANDARD1_3
    public static Stream OpenStandardError(int bufferSize) => Console.OpenStandardError(bufferSize: bufferSize);
#endif

    public static Stream OpenStandardInput() => Console.OpenStandardInput();

#if !NETSTANDARD1_3
    public static Stream OpenStandardInput(int bufferSize) => Console.OpenStandardInput(bufferSize: bufferSize);
#endif

    public static Stream OpenStandardOutput() => Console.OpenStandardOutput();

#if !NETSTANDARD1_3
    public static Stream OpenStandardOutput(int bufferSize) => Console.OpenStandardOutput(bufferSize: bufferSize);
#endif

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
