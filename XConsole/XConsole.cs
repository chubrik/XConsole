﻿#if NET
#pragma warning disable CA1416 // Validate platform compatibility
#else
#pragma warning disable CS8604 // Possible null reference argument.
#endif

namespace Chubrik.XConsole;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
#if NET
using System.Runtime.Versioning;
using System.Runtime.InteropServices;
#endif

public static class XConsole
{
    private static readonly string _newLine = Environment.NewLine;
    private static readonly bool _positioningEnabled;
    private static readonly object _syncLock = new();

    private static bool _coloringEnabled = Environment.GetEnvironmentVariable("NO_COLOR") == null;
    private static bool _cursorVisible;
    private static int _maxTop;
    private static long _shiftTop = 0;

#if NET
    internal static readonly bool VirtualTerminalEnabled;
#endif

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

#if NET
        if (OperatingSystem.IsWindows())
        {
            try
            {
                VirtualTerminalEnabled = EnableVirtualTerminal();
            }
            catch { }
        }
#endif
    }

    internal static long ShiftTop => _shiftTop;
#if NET
    internal static bool VirtualTerminalAndColoringEnabled => VirtualTerminalEnabled && _coloringEnabled;
#endif

    public static bool NO_COLOR
    {
        get => !_coloringEnabled;
        set => _coloringEnabled = !value;
    }

    public static ConsoleUtils Utils { get; } = new();

    public static void Sync(Action action)
    {
        lock (_syncLock)
            action();
    }

    public static T Sync<T>(Func<T> action)
    {
        lock (_syncLock)
            return action();
    }

    #region Pinning

    private static Func<IReadOnlyList<string?>>? _getPinValues = null;
    private static int _pinHeight = 0;

    [Obsolete("At least one argument should be specified.", error: true)]
    public static void Pin() => throw new InvalidOperationException();

#if NET
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
#endif
    public static void Pin(params string?[] values)
    {
        Debug.Assert(values.Length > 0);

        if (!_positioningEnabled)
            return;

        _getPinValues = () => values;
        UpdatePin();
    }

#if NET
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
#endif
    public static void Pin(IReadOnlyList<string?> values)
    {
        if (!_positioningEnabled)
            return;

        _getPinValues = () => values;
        UpdatePin();
    }

#if NET
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
#endif
    public static void Pin(Func<string?> getValue)
    {
        if (!_positioningEnabled)
            return;

        _getPinValues = () => new[] { getValue() };
        UpdatePin();
    }

#if NET
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
#endif
    public static void Pin(Func<IReadOnlyList<string?>> getValues)
    {
        if (!_positioningEnabled)
            return;

        _getPinValues = getValues;
        UpdatePin();
    }

#if NET
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
#endif
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
        var pinItems = new ConsoleItem[pinValues.Count];

        for (var i = 0; i < pinValues.Count; i++)
            pinItems[i] = ConsoleItem.Parse(pinValues[i]);

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
                _pinHeight = 1 + CalcLineWrapCount(pinItems, beginLeft: 0);
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

#if NET
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
#endif
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

    private const long _intMinValue = int.MinValue;

#if NET
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
#endif
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
                var actualTop = Math.Max(_intMinValue, value.InitialTop + value.ShiftTop - _shiftTop);
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

    internal static ConsolePosition WriteToPosition(ConsolePosition position, ConsoleItem[] items)
    {
        if (!_positioningEnabled)
            return default;

        int origLeft, origTop, endLeft, endTop;
        long shiftTop, positionActualTop;

        lock (_syncLock)
        {
            shiftTop = _shiftTop;
            positionActualTop = Math.Max(_intMinValue, position.InitialTop + position.ShiftTop - shiftTop);
#if NET
            (origLeft, origTop) = Console.GetCursorPosition();
#else
            (origLeft, origTop) = (Console.CursorLeft, Console.CursorTop);
#endif
            Console.CursorVisible = false;

            try
            {
                Console.SetCursorPosition(position.Left, unchecked((int)positionActualTop));
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
                var lineWrapCount = CalcLineWrapCount(items, origLeft);
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

#if NET
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
#endif
    public static string ReadLine()
    {
        lock (_syncLock)
            return Console.ReadLine() ?? string.Empty;
    }

#if NET
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
#endif
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
        return WriteBase(new[] { ConsoleItem.Parse(value) }, isWriteLine: false);
    }

    public static (ConsolePosition Begin, ConsolePosition End) Write(params string?[] values)
    {
        Debug.Assert(values.Length > 0);
        var items = new ConsoleItem[values.Length];

        for (var i = 0; i < values.Length; i++)
            items[i] = ConsoleItem.Parse(values[i]);

        return WriteBase(items, isWriteLine: false);
    }

    public static (ConsolePosition Begin, ConsolePosition End) Write(IReadOnlyList<string?> values)
    {
        var items = new ConsoleItem[values.Count];

        for (var i = 0; i < values.Count; i++)
            items[i] = ConsoleItem.Parse(values[i]);

        return WriteBase(items, isWriteLine: false);
    }

    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(string? value)
    {
        return WriteBase(new[] { ConsoleItem.Parse(value) }, isWriteLine: true);
    }

    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(params string?[] values)
    {
        Debug.Assert(values.Length > 0);
        var items = new ConsoleItem[values.Length];

        for (var i = 0; i < values.Length; i++)
            items[i] = ConsoleItem.Parse(values[i]);

        return WriteBase(items, isWriteLine: true);
    }

    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(IReadOnlyList<string?> values)
    {
        var items = new ConsoleItem[values.Count];

        for (var i = 0; i < values.Count; i++)
            items[i] = ConsoleItem.Parse(values[i]);

        return WriteBase(items, isWriteLine: true);
    }

    public static (ConsolePosition Begin, ConsolePosition End) WriteLine()
    {
        if (!_positioningEnabled)
        {
            lock (_syncLock)
                Console.WriteLine();

            return (default, default);
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
            var pinItems = new ConsoleItem[pinValues.Count];

            for (var i = 0; i < pinValues.Count; i++)
                pinItems[i] = ConsoleItem.Parse(pinValues[i]);

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
                        _pinHeight = 1 + CalcLineWrapCount(pinItems, beginLeft: 0);
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

    private static (ConsolePosition Begin, ConsolePosition End) WriteBase(ConsoleItem[] logItems, bool isWriteLine)
    {
        if (!_positioningEnabled)
        {
            lock (_syncLock)
            {
                WriteItems(logItems);

                if (isWriteLine)
                    Console.WriteLine();
            }

            return (default, default);
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
                        var logLineWrapCount = CalcLineWrapCount(logItems, beginLeft);
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
                        var logLineWrapCount = CalcLineWrapCount(logItems, beginLeft);
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
            var pinItems = new ConsoleItem[pinValues.Count];

            for (var i = 0; i < pinValues.Count; i++)
                pinItems[i] = ConsoleItem.Parse(pinValues[i]);

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
                            var logLineWrapCount = CalcLineWrapCount(logItems, beginLeft);
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
                            var logLineWrapCount = CalcLineWrapCount(logItems, beginLeft);
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
                            var logLineWrapCount = CalcLineWrapCount(logItems, beginLeft);
                            _pinHeight = 1 + CalcLineWrapCount(pinItems, beginLeft: 0);
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
                            var logLineWrapCount = CalcLineWrapCount(logItems, beginLeft);
                            _pinHeight = 1 + CalcLineWrapCount(pinItems, beginLeft: 0);
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

    #region Write overloads

    public static (ConsolePosition Begin, ConsolePosition End) Write(bool value)
    {
        return WriteBase(new[] { new ConsoleItem(value.ToString()) }, isWriteLine: false);
    }

    public static (ConsolePosition Begin, ConsolePosition End) Write(char value)
    {
        return WriteBase(new[] { new ConsoleItem(value.ToString()) }, isWriteLine: false);
    }

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

    public static (ConsolePosition Begin, ConsolePosition End) Write(decimal value)
    {
        return WriteBase(new[] { new ConsoleItem(value.ToString()) }, isWriteLine: false);
    }

    public static (ConsolePosition Begin, ConsolePosition End) Write(double value)
    {
        return WriteBase(new[] { new ConsoleItem(value.ToString()) }, isWriteLine: false);
    }

    public static (ConsolePosition Begin, ConsolePosition End) Write(int value)
    {
        return WriteBase(new[] { new ConsoleItem(value.ToString()) }, isWriteLine: false);
    }

    public static (ConsolePosition Begin, ConsolePosition End) Write(long value)
    {
        return WriteBase(new[] { new ConsoleItem(value.ToString()) }, isWriteLine: false);
    }

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

    public static (ConsolePosition Begin, ConsolePosition End) Write(float value)
    {
        return WriteBase(new[] { new ConsoleItem(value.ToString()) }, isWriteLine: false);
    }

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

    public static (ConsolePosition Begin, ConsolePosition End) Write(uint value)
    {
        return WriteBase(new[] { new ConsoleItem(value.ToString()) }, isWriteLine: false);
    }

    public static (ConsolePosition Begin, ConsolePosition End) Write(ulong value)
    {
        return WriteBase(new[] { new ConsoleItem(value.ToString()) }, isWriteLine: false);
    }

    #endregion

    #region WriteLine overloads

    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(bool value)
    {
        return WriteBase(new[] { new ConsoleItem(value.ToString()) }, isWriteLine: true);
    }

    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(char value)
    {
        return WriteBase(new[] { new ConsoleItem(value.ToString()) }, isWriteLine: true);
    }

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

    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(decimal value)
    {
        return WriteBase(new[] { new ConsoleItem(value.ToString()) }, isWriteLine: true);
    }

    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(double value)
    {
        return WriteBase(new[] { new ConsoleItem(value.ToString()) }, isWriteLine: true);
    }

    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(int value)
    {
        return WriteBase(new[] { new ConsoleItem(value.ToString()) }, isWriteLine: true);
    }

    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(long value)
    {
        return WriteBase(new[] { new ConsoleItem(value.ToString()) }, isWriteLine: true);
    }

    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(object? value)
    {
        var valueStr = value?.ToString();

        if (string.IsNullOrEmpty(valueStr))
            return WriteLine();

        return WriteBase(new[] { new ConsoleItem(valueStr) }, isWriteLine: true);
    }

    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(float value)
    {
        return WriteBase(new[] { new ConsoleItem(value.ToString()) }, isWriteLine: true);
    }

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

    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(uint value)
    {
        return WriteBase(new[] { new ConsoleItem(value.ToString()) }, isWriteLine: true);
    }

    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(ulong value)
    {
        return WriteBase(new[] { new ConsoleItem(value.ToString()) }, isWriteLine: true);
    }

    #endregion

    #endregion

    #region Private utils

    private static void WriteItems(ConsoleItem[] items)
    {
        if (_coloringEnabled)
        {
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
        else
            for (var i = 0; i < items.Length; i++)
                Console.Write(items[i].Value);
    }

    private static int CalcLineWrapCount(ConsoleItem[] items, int beginLeft)
    {
        var lineWrapCount = 0;
        var left = beginLeft;
        var bufferWidth = Console.BufferWidth;
        string chars;
        int charCount, charIndex;
        char chr;

        for (var itemIndex = 0; itemIndex < items.Length; itemIndex++)
        {
            chars = items[itemIndex].Value;
            charCount = chars.Length;

            for (charIndex = 0; charIndex < charCount; charIndex++)
            {
                chr = chars[charIndex];

                if (chr >= ' ')
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

                        case '\x1b':

#if NET
                            if (VirtualTerminalEnabled)
                            {
                                for (charIndex++; charIndex < charCount; charIndex++)
                                {
                                    chr = chars[charIndex];

                                    if ((chr >= 'A' && chr <= 'Z') || (chr >= 'a' && chr <= 'z'))
                                        break;
                                }

                                continue;
                            }
                            else
                            {
#endif
                                left++;
                                break;
#if NET
                            }
#endif

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

    #region Virtual terminal

#if NET
    // https://learn.microsoft.com/en-us/windows/console/console-virtual-terminal-sequences

    private const int STD_OUTPUT_HANDLE = -11;
    private const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;

    private static readonly IntPtr INVALID_HANDLE_VALUE = new(-1);

    private static bool EnableVirtualTerminal()
    {
        var hOut = GetStdHandle(STD_OUTPUT_HANDLE);

        return hOut != INVALID_HANDLE_VALUE &&
               GetConsoleMode(hOut, out var dwMode) &&
               SetConsoleMode(hOut, dwMode | ENABLE_VIRTUAL_TERMINAL_PROCESSING);
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll")]
    private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

    [DllImport("kernel32.dll")]
    private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);
#endif

    #endregion

    #endregion

    #region Remaining API

#if NET
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
#endif
    public static TextReader In => Console.In;

#if NET
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
#endif
    public static Encoding InputEncoding
    {
        get => Console.InputEncoding;
        set
        {
            lock (_syncLock)
                Console.InputEncoding = value;
        }
    }

    public static Encoding OutputEncoding
    {
        get => Console.OutputEncoding;
#if NET
        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
#endif
        set
        {
            lock (_syncLock)
                Console.OutputEncoding = value;
        }
    }

    public static bool KeyAvailable => Console.KeyAvailable;

#if NET
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
#endif
    public static ConsoleKeyInfo ReadKey()
    {
        lock (_syncLock)
            return Console.ReadKey();
    }

#if NET
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
#endif
    public static ConsoleKeyInfo ReadKey(bool intercept)
    {
        lock (_syncLock)
            return Console.ReadKey(intercept: intercept);
    }

    public static TextWriter Out => Console.Out;

    public static TextWriter Error => Console.Error;

    public static bool IsInputRedirected => Console.IsInputRedirected;

    public static bool IsOutputRedirected => Console.IsOutputRedirected;

    public static bool IsErrorRedirected => Console.IsErrorRedirected;

    public static int CursorSize
    {
#if NET
        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("browser")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
#endif
        get => Console.CursorSize;
#if NET
        [SupportedOSPlatform("windows")]
#endif
        set => Console.CursorSize = value;
    }

#if NET
    [SupportedOSPlatform("windows")]
#endif
    public static bool NumberLock => Console.NumberLock;

#if NET
    [SupportedOSPlatform("windows")]
#endif
    public static bool CapsLock => Console.CapsLock;

#if NET
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
#endif
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

#if NET
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
#endif
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

#if NET
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
#endif
    public static void ResetColor()
    {
        lock (_syncLock)
            Console.ResetColor();
    }

    public static int BufferWidth
    {
#if NET
        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("browser")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
#endif
        get => Console.BufferWidth;
#if NET
        [SupportedOSPlatform("windows")]
#endif
        set
        {
            lock (_syncLock)
                Console.BufferWidth = value;
        }
    }

    public static int BufferHeight
    {
#if NET
        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("browser")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
#endif
        get => Console.BufferHeight;
#if NET
        [SupportedOSPlatform("windows")]
#endif
        set
        {
            lock (_syncLock)
            {
                Console.BufferHeight = value;
                _maxTop = value - 1;
            }
        }
    }

#if NET
    [SupportedOSPlatform("windows")]
#endif
    public static void SetBufferSize(int width, int height)
    {
        lock (_syncLock)
            Console.SetBufferSize(width: width, height: height);
    }

    public static int WindowLeft
    {
        get => Console.WindowLeft;
#if NET
        [SupportedOSPlatform("windows")]
#endif
        set => Console.WindowLeft = value;
    }

    public static int WindowTop
    {
        get => Console.WindowTop;
#if NET
        [SupportedOSPlatform("windows")]
#endif
        set => Console.WindowTop = value;
    }

    public static int WindowWidth
    {
#if NET
        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("browser")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
#endif
        get => Console.WindowWidth;
#if NET
        [SupportedOSPlatform("windows")]
#endif
        set
        {
            lock (_syncLock)
                Console.WindowWidth = value;
        }
    }

    public static int WindowHeight
    {
#if NET
        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("browser")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
#endif
        get => Console.WindowHeight;
#if NET
        [SupportedOSPlatform("windows")]
#endif
        set
        {
            lock (_syncLock)
                Console.WindowHeight = value;
        }
    }

#if NET
    [SupportedOSPlatform("windows")]
#endif
    public static void SetWindowPosition(int left, int top)
    {
        Console.SetWindowPosition(left: left, top: top);
    }

#if NET
    [SupportedOSPlatform("windows")]
#endif
    public static void SetWindowSize(int width, int height)
    {
        lock (_syncLock)
            Console.SetWindowSize(width: width, height: height);
    }

#if NET
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
#endif
    public static int LargestWindowWidth => Console.LargestWindowWidth;

#if NET
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
#endif
    public static int LargestWindowHeight => Console.LargestWindowHeight;

    public static bool CursorVisible
    {
#if NET
        [SupportedOSPlatform("windows")]
#endif
        get
        {
            lock (_syncLock)
                return Console.CursorVisible;
        }
#if NET
        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("browser")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
#endif
        set
        {
            lock (_syncLock)
                Console.CursorVisible = _cursorVisible = value;
        }
    }

#if NET
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
#endif
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

#if NET
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
#endif
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

#if NET
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
#endif
    public static (int Left, int Top) GetCursorPosition()
    {
        lock (_syncLock)
#if NET
            return Console.GetCursorPosition();
#else
            return (Console.CursorLeft, Console.CursorTop);
#endif
    }

    public static string Title
    {
#if NET
        [SupportedOSPlatform("windows")]
#endif
        get => Console.Title;
#if NET
        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("browser")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
#endif
        set => Console.Title = value;
    }

#if NET
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
#endif
    public static void Beep() => Console.Beep();

#if NET
    [SupportedOSPlatform("windows")]
#endif
    public static void Beep(int frequency, int duration) => Console.Beep(frequency: frequency, duration: duration);

#if NET
    [SupportedOSPlatform("windows")]
#endif
    public static void MoveBufferArea(
        int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight, int targetLeft, int targetTop)
    {
        lock (_syncLock)
            Console.MoveBufferArea(
                sourceLeft: sourceLeft, sourceTop: sourceTop,
                sourceWidth: sourceWidth, sourceHeight: sourceHeight,
                targetLeft: targetLeft, targetTop: targetTop);
    }

#if NET
    [SupportedOSPlatform("windows")]
#endif
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

#if NET
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
#endif
    public static void Clear()
    {
        lock (_syncLock)
            Console.Clear();
    }

#if NET
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
#endif
    public static void SetCursorPosition(int left, int top)
    {
        lock (_syncLock)
            Console.SetCursorPosition(left: left, top: top);
    }

#if NET
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

#if NET
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

#if NET
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
#endif
    public static Stream OpenStandardInput()
    {
        lock (_syncLock)
            return Console.OpenStandardInput();
    }

#if !NETSTANDARD1_3
#if NET
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
#endif
    public static Stream OpenStandardInput(int bufferSize)
    {
        lock (_syncLock)
            return Console.OpenStandardInput(bufferSize: bufferSize);
    }
#endif

    public static Stream OpenStandardOutput()
    {
        lock (_syncLock)
            return Console.OpenStandardOutput();
    }

#if !NETSTANDARD1_3
    public static Stream OpenStandardOutput(int bufferSize)
    {
        lock (_syncLock)
            return Console.OpenStandardOutput(bufferSize: bufferSize);
    }
#endif

    public static Stream OpenStandardError()
    {
        return Console.OpenStandardError();
    }

#if !NETSTANDARD1_3
    public static Stream OpenStandardError(int bufferSize)
    {
        return Console.OpenStandardError(bufferSize: bufferSize);
    }
#endif

#if NET
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
#endif
    public static void SetIn(TextReader newIn)
    {
        lock (_syncLock)
            Console.SetIn(newIn: newIn);
    }

    public static void SetOut(TextWriter newOut)
    {
        lock (_syncLock)
            Console.SetOut(newOut: newOut);
    }

    public static void SetError(TextWriter newError)
    {
        Console.SetError(newError: newError);
    }

#if NET
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
#endif
    public static int Read()
    {
        lock (_syncLock)
            return Console.Read();
    }

    #endregion
}
