#pragma warning disable CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)
#if NET
#pragma warning disable CA1416 // Validate platform compatibility
#endif

namespace Chubrik.XConsole;

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Text;
#if NET9_0_OR_GREATER
using System.Threading;
#endif
#if NET7_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

/// <summary>
/// Extended .NET console with coloring microsyntax, multiline pinning, write-to-position and most useful utils.
/// <para>
/// The main features:
/// <list type="bullet">
/// <item>Backward compatibility with the standard <see cref="Console"/>.</item>
/// <item>
/// A simple <see href="https://github.com/chubrik/XConsole#coloring">microsyntax</see> to colorize any text,
/// resistant to multithreading. Also supports the “NO_COLOR” convention to disable any colors.
/// </item>
/// <item>
/// <see cref="Pin(string?)"/> methods pin text below the regular log messages.
/// Text can be dynamic, multiline and colored. It’s resistant to console buffer overflow.
/// </item>
/// <item>
/// <see cref="CursorPosition"/> provides smart <see cref="ConsolePosition"/> structure
/// resistant to console buffer overflow and always points to the correct position within the console buffer area.
/// </item>
/// <item>
/// <see cref="Write(string?)"/> and <see cref="WriteLine(string?)"/> overloads
/// return begin and end <see cref="ConsolePosition"/> within the console buffer area.
/// </item>
/// <item><see cref="ReadLine()"/> has additional modes that mask or hides the typed characters on the screen.</item>
/// <item><see cref="Extras"/> is singleton instance for pre-built and custom extensions.</item>
/// </list>
/// </para>
/// <see href="https://github.com/chubrik/XConsole#xconsole">More info</see>
/// </summary>
public static class XConsole
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

    /// <summary>
    /// Disables all colors according to the "NO_COLOR" convention. See: <see href="https://no-color.org/"/>.
    /// </summary>
    public static bool NO_COLOR
    {
        get => !_coloringEnabled;
        set => _coloringEnabled = !value;
    }

    /// <inheritdoc cref="ConsoleExtras"/>
    public static ConsoleExtras Extras { get; } = new();

    /// <summary>
    /// Executes the <paramref name="action"/> callback synchronously.
    /// </summary>
    /// <param name="action">The action callback to execute synchronously.</param>
    public static void Sync(Action action)
    {
        lock (_syncLock)
            action();
    }

    /// <inheritdoc cref="Sync(Action)"/>
    public static T Sync<T>(Func<T> action)
    {
        lock (_syncLock)
            return action();
    }

    #endregion

    #region Pinning

    private static Func<IReadOnlyList<string?>>? _getPinValues = null;
    private static int _pinHeight = 0;
    private static int _pinClearWidth = 0;
    private static int _pinClearHeight = 0;
    private static string _pinClear = string.Empty;

    /// <summary>
    /// Pins the specified string value as a static text below the regular log messages.
    /// Text can be multiline and <see href="https://github.com/chubrik/XConsole#coloring">colored</see>.
    /// Pin is resistant to console buffer overflow.
    /// </summary>
    /// <param name="value">
    /// The value for pin below the regular log messages.
    /// Text can be colored using a simple <see href="https://github.com/chubrik/XConsole#coloring">microsyntax</see>.
    /// </param>
    /// <remarks>
    /// See also:<br/>• <seealso cref="Unpin()"/>
    /// </remarks>
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
    public static void Pin(string? value)
    {
        if (!_positioningEnabled)
            return;

        var values = new[] { value };
        _getPinValues = () => values;
        UpdatePin();
    }

    /// <summary>
    /// Pins the specified set of string values as a static text below the regular log messages.
    /// Text can be multiline and <see href="https://github.com/chubrik/XConsole#coloring">colored</see>.
    /// Pin is resistant to console buffer overflow.
    /// </summary>
    /// <param name="values">
    /// The set of values for pin below the regular log messages.
    /// Text can be colored using a simple <see href="https://github.com/chubrik/XConsole#coloring">microsyntax</see>.
    /// </param>
    /// <remarks>
    /// See also:<br/>• <seealso cref="Unpin()"/>
    /// </remarks>
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
    public static void Pin(IReadOnlyList<string?> values)
    {
        if (!_positioningEnabled)
            return;

        _getPinValues = () => values;
        UpdatePin();
    }

    /// <summary>
    /// Pins the value from <paramref name="getValue"/> callback as a dynamic text below the regular log messages.
    /// Text can be multiline and <see href="https://github.com/chubrik/XConsole#coloring">colored</see>,
    /// it updates every time <see cref="Write(string?)"/> or <see cref="WriteLine(string?)"/> method are called.
    /// Pin is resistant to console buffer overflow.
    /// </summary>
    /// <param name="getValue">
    /// The value callback to pin as a dynamic text below the regular log messages.
    /// Text can be colored using a simple <see href="https://github.com/chubrik/XConsole#coloring">microsyntax</see>.
    /// </param>
    /// <remarks>
    /// See also:
    /// <br/>• <seealso cref="UpdatePin()"/>
    /// <br/>• <seealso cref="Unpin()"/>
    /// </remarks>
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
    public static void Pin(Func<string?> getValue)
    {
        if (!_positioningEnabled)
            return;

        var values = new string?[1];

        _getPinValues = () =>
        {
            values[0] = getValue();
            return values;
        };

        UpdatePin();
    }

    /// <summary>
    /// Pins the values from <paramref name="getValues"/> callback as a dynamic text below the regular log messages.
    /// Text can be multiline and <see href="https://github.com/chubrik/XConsole#coloring">colored</see>,
    /// it updates every time <see cref="Write(string?)"/> or <see cref="WriteLine(string?)"/> method are called.
    /// Pin is resistant to console buffer overflow.
    /// </summary>
    /// <param name="getValues">
    /// The values callback to pin as a dynamic text below the regular log messages.
    /// Text can be colored using a simple <see href="https://github.com/chubrik/XConsole#coloring">microsyntax</see>.
    /// </param>
    /// <inheritdoc cref="Pin(Func{string?})"/>
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
    public static void Pin(Func<IReadOnlyList<string?>> getValues)
    {
        if (!_positioningEnabled)
            return;

        _getPinValues = getValues;
        UpdatePin();
    }

    /// <summary>
    /// Updates the pinned dynamic text below the regular log messages.
    /// </summary>
    /// <remarks>
    /// See also:
    /// <br/>• <seealso cref="Pin(Func{string?})"/>
    /// <br/>• <seealso cref="Pin(Func{IReadOnlyList{string?}})"/>
    /// <br/>• <seealso cref="Unpin()"/>
    /// </remarks>
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
    public static void UpdatePin()
    {
        if (!_positioningEnabled)
            return;

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
#if NET
            (logLeft, logTop) = Console.GetCursorPosition();
#else
            (logLeft, logTop) = (Console.CursorLeft, Console.CursorTop);
#endif
            WriteBaseWithPin(logItems: null, isWriteLine: false, pinItems,
                logBeginLeft: logLeft, logBeginTop: ref logTop, out _, out _);
        }
    }

    /// <summary>
    /// Removes the pinned text below the regular log messages.
    /// </summary>
    /// <remarks>
    /// See also:
    /// <br/>• <seealso cref="Pin(string?)"/>
    /// <br/>• <seealso cref="Pin(IReadOnlyList{string?})"/>
    /// <br/>• <seealso cref="Pin(Func{string?})"/>
    /// <br/>• <seealso cref="Pin(Func{IReadOnlyList{string?}})"/>
    /// <br/>• <seealso cref="UpdatePin()"/>
    /// </remarks>
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
    public static void Unpin()
    {
        if (!_positioningEnabled)
            return;

        int logLeft, logTop;

        lock (_syncLock)
        {
            if (_getPinValues == null)
                return;
#if NET
            (logLeft, logTop) = Console.GetCursorPosition();
#else
            (logLeft, logTop) = (Console.CursorLeft, Console.CursorTop);
#endif
            var pinClear = GetPinClear();
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

    /// <summary>
    /// Gets or sets the position of the cursor.
    /// </summary>
    /// <returns>
    /// Smart <see cref="ConsolePosition"/> structure, resistant to console buffer overflow
    /// and always points to the correct position within the console buffer area.
    /// </returns>
    /// <inheritdoc cref="Console.SetCursorPosition(int, int)"/>
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
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
#if NET
            (logLeft, logTop) = Console.GetCursorPosition();
#else
            (logLeft, logTop) = (Console.CursorLeft, Console.CursorTop);
#endif
            shiftTop = _shiftTop;
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
#if NET
            (endLeft, endTop) = Console.GetCursorPosition();
#else
            (endLeft, endTop) = (Console.CursorLeft, Console.CursorTop);
#endif
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

    /// <inheritdoc cref="Console.ReadLine()"/>
    /// <remarks>
    /// See also:
    /// <br/>• <seealso cref="ReadLine(ConsoleReadLineMode, char)"/> – reads the line in masked or hidden modes.
    /// </remarks>
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    public static string? ReadLine()
    {
        lock (_syncLock)
        {
#if NET
            var (logBeginLeft, logBeginTop) = Console.GetCursorPosition();
#else
            var (logBeginLeft, logBeginTop) = (Console.CursorLeft, Console.CursorTop);
#endif
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

    /// <summary>
    /// <inheritdoc cref="Console.ReadLine()"/>
    /// <para>
    /// There are three <paramref name="mode"/> variants:
    /// <br/>• <see cref="ConsoleReadLineMode.Default"/> – default behavior.
    /// <br/>• <see cref="ConsoleReadLineMode.Masked"/> –
    /// each typed character is displayed as default or custom <paramref name="maskChar"/>.
    /// <br/>• <see cref="ConsoleReadLineMode.Hidden"/> – typed characters are not displayed.
    /// </para>
    /// </summary>
    /// <param name="mode">
    /// There are three <paramref name="mode"/> variants:
    /// <br/>• <see cref="ConsoleReadLineMode.Default"/> – default behavior.
    /// <br/>• <see cref="ConsoleReadLineMode.Masked"/> –
    /// each typed character is displayed as default or custom <paramref name="maskChar"/>.
    /// <br/>• <see cref="ConsoleReadLineMode.Hidden"/> – typed characters are not displayed.
    /// </param>
    /// <param name="maskChar">Char used for mask each character in masked <paramref name="mode"/>.</param>
    /// <inheritdoc cref="Console.ReadLine()"/>
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    public static string? ReadLine(ConsoleReadLineMode mode, char maskChar = '\u2022')
    {
        switch (mode)
        {
            case ConsoleReadLineMode.Default:
                return ReadLine();

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
                                WriteLine();
                                return null;
                            }
#endif
                            if (isMaskedMode)
                                Console.Write(maskChar);

                            result += keyInfo.KeyChar;
                        }
                    }
                    while (keyInfo.Key != ConsoleKey.Enter);

                    WriteLine();
                }

                if (result.Length > 0 && result[0] == '\x1a')
                    return null;

                return result;

            default:
                throw new ArgumentOutOfRangeException(nameof(mode));
        }
    }

    #endregion

    #region Write common

    private static readonly ConsoleItem[] _singleItemArray = new ConsoleItem[1];

    /// <inheritdoc cref="Console.WriteLine()"/>
    /// <returns><inheritdoc cref="WriteLine(string?)"/></returns>
    public static (ConsolePosition Begin, ConsolePosition End) WriteLine()
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
#if NET
                (logLeft, logTop) = Console.GetCursorPosition();
#else
                (logLeft, logTop) = (Console.CursorLeft, Console.CursorTop);
#endif
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
#if NET
                (logLeft, logTop) = Console.GetCursorPosition();
#else
                (logLeft, logTop) = (Console.CursorLeft, Console.CursorTop);
#endif
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
#if NET
                (logBeginLeft, logBeginTop) = Console.GetCursorPosition();
#else
                (logBeginLeft, logBeginTop) = (Console.CursorLeft, Console.CursorTop);
#endif
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
#if NET
                (logBeginLeft, logBeginTop) = Console.GetCursorPosition();
#else
                (logBeginLeft, logBeginTop) = (Console.CursorLeft, Console.CursorTop);
#endif
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
#if NET
        (logEndLeft, logEndTop) = Console.GetCursorPosition();
#else
        (logEndLeft, logEndTop) = (Console.CursorLeft, Console.CursorTop);
#endif
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
#if NET
            (logEndLeft, logEndTop) = Console.GetCursorPosition();
#else
            (logEndLeft, logEndTop) = (Console.CursorLeft, Console.CursorTop);
#endif
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

    #region Write API

    /// <summary>
    /// <inheritdoc cref="Console.Write(string?)"/>
    /// Text can be colored using a simple <see href="https://github.com/chubrik/XConsole#coloring">microsyntax</see>.
    /// </summary>
    /// <param name="value">
    /// The value to write.
    /// Text can be colored using a simple <see href="https://github.com/chubrik/XConsole#coloring">microsyntax</see>.
    /// </param>
    /// <returns>
    /// Begin and end <see cref="ConsolePosition"/> structures, resistant to console buffer overflow
    /// and always point to the correct positions within the console buffer area.
    /// </returns>
    /// <inheritdoc cref="Console.Write(string?)"/>
    public static (ConsolePosition Begin, ConsolePosition End) Write(string? value)
    {
        return WriteBase(ConsoleItem.Parse(value), [], isWriteLine: false);
    }

    /// <summary>
    /// Writes the specified set of string values to the standard output stream.
    /// Text can be colored using a simple <see href="https://github.com/chubrik/XConsole#coloring">microsyntax</see>.
    /// </summary>
    /// <param name="values">
    /// The set of values to write.
    /// Text can be colored using a simple <see href="https://github.com/chubrik/XConsole#coloring">microsyntax</see>.
    /// </param>
    /// <inheritdoc cref="Write(string?)"/>
    public static (ConsolePosition Begin, ConsolePosition End) Write(IReadOnlyList<string?> values)
    {
        var logItems = new ConsoleItem[values.Count];

        for (var i = 0; i < values.Count; i++)
            logItems[i] = ConsoleItem.Parse(values[i]);

        return WriteBase(null, logItems, isWriteLine: false);
    }

    /// <summary>
    /// <inheritdoc cref="Console.Write(string, object?)"/>
    /// Text can be colored using a simple <see href="https://github.com/chubrik/XConsole#coloring">microsyntax</see>.
    /// </summary>
    /// <param name="format">
    /// A composite format string.
    /// Text can be colored using a simple <see href="https://github.com/chubrik/XConsole#coloring">microsyntax</see>.
    /// </param>
    /// <returns><inheritdoc cref="Write(string?)"/></returns>
    /// <inheritdoc cref="Console.Write(string, object?)"/>
    public static (ConsolePosition Begin, ConsolePosition End) Write(
#if NET7_0_OR_GREATER
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
        string format, object? arg0)
    {
        if (format.Length == 0)
        {
            var position = CursorPosition;
            return (position, position);
        }

        return WriteBase(ConsoleItem.Parse(string.Format(format, arg0)), [], isWriteLine: false);
    }

    /// <summary>
    /// <inheritdoc cref="Console.Write(string, object?, object?)"/>
    /// Text can be colored using a simple <see href="https://github.com/chubrik/XConsole#coloring">microsyntax</see>.
    /// </summary>
    /// <param name="format">
    /// A composite format string.
    /// Text can be colored using a simple <see href="https://github.com/chubrik/XConsole#coloring">microsyntax</see>.
    /// </param>
    /// <returns><inheritdoc cref="Write(string?)"/></returns>
    /// <inheritdoc cref="Console.Write(string, object?, object?)"/>
    public static (ConsolePosition Begin, ConsolePosition End) Write(
#if NET7_0_OR_GREATER
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
        string format, object? arg0, object? arg1)
    {
        if (format.Length == 0)
        {
            var position = CursorPosition;
            return (position, position);
        }

        return WriteBase(ConsoleItem.Parse(string.Format(format, arg0, arg1)), [], isWriteLine: false);
    }

    /// <summary>
    /// <inheritdoc cref="Console.Write(string, object?, object?, object?)"/>
    /// Text can be colored using a simple <see href="https://github.com/chubrik/XConsole#coloring">microsyntax</see>.
    /// </summary>
    /// <param name="format">
    /// A composite format string.
    /// Text can be colored using a simple <see href="https://github.com/chubrik/XConsole#coloring">microsyntax</see>.
    /// </param>
    /// <returns><inheritdoc cref="Write(string?)"/></returns>
    /// <inheritdoc cref="Console.Write(string, object?, object?, object?)"/>
    public static (ConsolePosition Begin, ConsolePosition End) Write(
#if NET7_0_OR_GREATER
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
        string format, object? arg0, object? arg1, object? arg2)
    {
        if (format.Length == 0)
        {
            var position = CursorPosition;
            return (position, position);
        }

        return WriteBase(ConsoleItem.Parse(string.Format(format, arg0, arg1, arg2)), [], isWriteLine: false);
    }

    /// <summary>
    /// <inheritdoc cref="Console.Write(string, object?[])"/>
    /// Text can be colored using a simple <see href="https://github.com/chubrik/XConsole#coloring">microsyntax</see>.
    /// </summary>
    /// <param name="format">
    /// A composite format string.
    /// Text can be colored using a simple <see href="https://github.com/chubrik/XConsole#coloring">microsyntax</see>.
    /// </param>
    /// <returns><inheritdoc cref="Write(string?)"/></returns>
    /// <inheritdoc cref="Console.Write(string, object?[])"/>
    public static (ConsolePosition Begin, ConsolePosition End) Write(
#if NET7_0_OR_GREATER
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
        string format, params object?[]? arg)
    {
        if (format.Length == 0)
        {
            var position = CursorPosition;
            return (position, position);
        }

        return WriteBase(ConsoleItem.Parse(string.Format(format, arg ?? [])), [], isWriteLine: false);
    }

#if NET9_0_OR_GREATER
    /// <summary>
    /// <inheritdoc cref="Console.Write(string, ReadOnlySpan{object?})"/>
    /// Text can be colored using a simple <see href="https://github.com/chubrik/XConsole#coloring">microsyntax</see>.
    /// </summary>
    /// <param name="format">
    /// A composite format string.
    /// Text can be colored using a simple <see href="https://github.com/chubrik/XConsole#coloring">microsyntax</see>.
    /// </param>
    /// <returns><inheritdoc cref="Write(string?)"/></returns>
    /// <inheritdoc cref="Console.Write(string, ReadOnlySpan{object?})"/>
    public static (ConsolePosition Begin, ConsolePosition End) Write(
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, params ReadOnlySpan<object?> arg)
    {
        if (format.Length == 0)
        {
            var position = CursorPosition;
            return (position, position);
        }

        return WriteBase(ConsoleItem.Parse(string.Format(format, arg)), [], isWriteLine: false);
    }
#endif

    /// <inheritdoc cref="Console.Write(bool)"/>
    /// <returns><inheritdoc cref="Write(string?)"/></returns>
    public static (ConsolePosition Begin, ConsolePosition End) Write(bool value)
    {
        return WriteBase(new ConsoleItem(value.ToString()), [], isWriteLine: false);
    }

    /// <inheritdoc cref="Console.Write(char)"/>
    /// <returns><inheritdoc cref="Write(string?)"/></returns>
    public static (ConsolePosition Begin, ConsolePosition End) Write(char value)
    {
        return WriteBase(new ConsoleItem(value.ToString()), [], isWriteLine: false);
    }

    /// <inheritdoc cref="Console.Write(char[])"/>
    /// <returns><inheritdoc cref="Write(string?)"/></returns>
    public static (ConsolePosition Begin, ConsolePosition End) Write(char[]? buffer)
    {
        if (buffer == null || buffer.Length == 0)
        {
            var position = CursorPosition;
            return (position, position);
        }

        return WriteBase(new ConsoleItem(new string(buffer)), [], isWriteLine: false);
    }

    /// <inheritdoc cref="Console.Write(char[], int, int)"/>
    /// <returns><inheritdoc cref="Write(string?)"/></returns>
    public static (ConsolePosition Begin, ConsolePosition End) Write(char[] buffer, int index, int count)
    {
        if (count == 0)
        {
            var position = CursorPosition;
            return (position, position);
        }

        return WriteBase(new ConsoleItem(new string(buffer, index, count)), [], isWriteLine: false);
    }

#if NET10_0_OR_GREATER
    /// <inheritdoc cref="Console.Write(ReadOnlySpan{char})"/>
    /// <returns><inheritdoc cref="Write(string?)"/></returns>
    public static (ConsolePosition Begin, ConsolePosition End) Write(ReadOnlySpan<char> value)
    {
        return WriteBase(new ConsoleItem(value.ToString()), [], isWriteLine: false);
    }
#endif

    /// <inheritdoc cref="Console.Write(decimal)"/>
    /// <returns><inheritdoc cref="Write(string?)"/></returns>
    public static (ConsolePosition Begin, ConsolePosition End) Write(decimal value)
    {
        return WriteBase(new ConsoleItem(value.ToString()), [], isWriteLine: false);
    }

    /// <inheritdoc cref="Console.Write(double)"/>
    /// <returns><inheritdoc cref="Write(string?)"/></returns>
    public static (ConsolePosition Begin, ConsolePosition End) Write(double value)
    {
        return WriteBase(new ConsoleItem(value.ToString()), [], isWriteLine: false);
    }

    /// <inheritdoc cref="Console.Write(float)"/>
    /// <returns><inheritdoc cref="Write(string?)"/></returns>
    public static (ConsolePosition Begin, ConsolePosition End) Write(float value)
    {
        return WriteBase(new ConsoleItem(value.ToString()), [], isWriteLine: false);
    }

    /// <inheritdoc cref="Console.Write(int)"/>
    /// <returns><inheritdoc cref="Write(string?)"/></returns>
    public static (ConsolePosition Begin, ConsolePosition End) Write(int value)
    {
        return WriteBase(new ConsoleItem(value.ToString()), [], isWriteLine: false);
    }

    /// <inheritdoc cref="Console.Write(uint)"/>
    /// <returns><inheritdoc cref="Write(string?)"/></returns>
    public static (ConsolePosition Begin, ConsolePosition End) Write(uint value)
    {
        return WriteBase(new ConsoleItem(value.ToString()), [], isWriteLine: false);
    }

    /// <inheritdoc cref="Console.Write(long)"/>
    /// <returns><inheritdoc cref="Write(string?)"/></returns>
    public static (ConsolePosition Begin, ConsolePosition End) Write(long value)
    {
        return WriteBase(new ConsoleItem(value.ToString()), [], isWriteLine: false);
    }

    /// <inheritdoc cref="Console.Write(ulong)"/>
    /// <returns><inheritdoc cref="Write(string?)"/></returns>
    public static (ConsolePosition Begin, ConsolePosition End) Write(ulong value)
    {
        return WriteBase(new ConsoleItem(value.ToString()), [], isWriteLine: false);
    }

    /// <inheritdoc cref="Console.Write(object?)"/>
    /// <returns><inheritdoc cref="Write(string?)"/></returns>
    public static (ConsolePosition Begin, ConsolePosition End) Write(object? value)
    {
        var valueStr = value?.ToString();

        if (string.IsNullOrEmpty(valueStr))
        {
            var position = CursorPosition;
            return (position, position);
        }
#if !NET
#pragma warning disable CS8604 // Possible null reference argument.
#endif
        return WriteBase(new ConsoleItem(valueStr), [], isWriteLine: false);
#if !NET
#pragma warning restore CS8604 // Possible null reference argument.
#endif
    }

    #endregion

    #region WriteLine API

    /// <summary>
    /// <inheritdoc cref="Console.WriteLine(string?)"/>
    /// Text can be colored using a simple <see href="https://github.com/chubrik/XConsole#coloring">microsyntax</see>.
    /// </summary>
    /// <inheritdoc cref="Write(string?)"/>
    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(string? value)
    {
        return WriteBase(ConsoleItem.Parse(value), [], isWriteLine: true);
    }

    /// <summary>
    /// Writes the specified set of string values, followed by the current line terminator,
    /// to the standard output stream.
    /// Text can be colored using a simple <see href="https://github.com/chubrik/XConsole#coloring">microsyntax</see>.
    /// </summary>
    /// <param name="values">
    /// The set of values to write.
    /// Text can be colored using a simple <see href="https://github.com/chubrik/XConsole#coloring">microsyntax</see>.
    /// </param>
    /// <inheritdoc cref="WriteLine(string?)"/>
    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(IReadOnlyList<string?> values)
    {
        var logItems = new ConsoleItem[values.Count];

        for (var i = 0; i < values.Count; i++)
            logItems[i] = ConsoleItem.Parse(values[i]);

        return WriteBase(null, logItems, isWriteLine: true);
    }

    /// <summary>
    /// <inheritdoc cref="Console.WriteLine(string, object?)"/>
    /// Text can be colored using a simple <see href="https://github.com/chubrik/XConsole#coloring">microsyntax</see>.
    /// </summary>
    /// <param name="format">
    /// A composite format string.
    /// Text can be colored using a simple <see href="https://github.com/chubrik/XConsole#coloring">microsyntax</see>.
    /// </param>
    /// <returns><inheritdoc cref="WriteLine(string?)"/></returns>
    /// <inheritdoc cref="Console.WriteLine(string, object?)"/>
    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(
#if NET7_0_OR_GREATER
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
        string format, object? arg0)
    {
        if (format.Length == 0)
            return WriteLine();

        return WriteBase(ConsoleItem.Parse(string.Format(format, arg0)), [], isWriteLine: true);
    }

    /// <summary>
    /// <inheritdoc cref="Console.WriteLine(string, object?, object?)"/>
    /// Text can be colored using a simple <see href="https://github.com/chubrik/XConsole#coloring">microsyntax</see>.
    /// </summary>
    /// <param name="format">
    /// A composite format string.
    /// Text can be colored using a simple <see href="https://github.com/chubrik/XConsole#coloring">microsyntax</see>.
    /// </param>
    /// <returns><inheritdoc cref="WriteLine(string?)"/></returns>
    /// <inheritdoc cref="Console.WriteLine(string, object?, object?)"/>
    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(
#if NET7_0_OR_GREATER
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
        string format, object? arg0, object? arg1)
    {
        if (format.Length == 0)
            return WriteLine();

        return WriteBase(ConsoleItem.Parse(string.Format(format, arg0, arg1)), [], isWriteLine: true);
    }

    /// <summary>
    /// <inheritdoc cref="Console.WriteLine(string, object?, object?, object?)"/>
    /// Text can be colored using a simple <see href="https://github.com/chubrik/XConsole#coloring">microsyntax</see>.
    /// </summary>
    /// <param name="format">
    /// A composite format string.
    /// Text can be colored using a simple <see href="https://github.com/chubrik/XConsole#coloring">microsyntax</see>.
    /// </param>
    /// <returns><inheritdoc cref="WriteLine(string?)"/></returns>
    /// <inheritdoc cref="Console.WriteLine(string, object?, object?, object?)"/>
    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(
#if NET7_0_OR_GREATER
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
        string format, object? arg0, object? arg1, object? arg2)
    {
        if (format.Length == 0)
            return WriteLine();

        return WriteBase(ConsoleItem.Parse(string.Format(format, arg0, arg1, arg2)), [], isWriteLine: true);
    }

    /// <summary>
    /// <inheritdoc cref="Console.WriteLine(string, object?[])"/>
    /// Text can be colored using a simple <see href="https://github.com/chubrik/XConsole#coloring">microsyntax</see>.
    /// </summary>
    /// <param name="format">
    /// A composite format string.
    /// Text can be colored using a simple <see href="https://github.com/chubrik/XConsole#coloring">microsyntax</see>.
    /// </param>
    /// <returns><inheritdoc cref="WriteLine(string?)"/></returns>
    /// <inheritdoc cref="Console.WriteLine(string, object?[])"/>
    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(
#if NET7_0_OR_GREATER
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
        string format, params object?[]? arg)
    {
        if (format.Length == 0)
            return WriteLine();

        return WriteBase(ConsoleItem.Parse(string.Format(format, arg ?? [])), [], isWriteLine: true);
    }

#if NET9_0_OR_GREATER
    /// <summary>
    /// <inheritdoc cref="Console.WriteLine(string, ReadOnlySpan{object?})"/>
    /// Text can be colored using a simple <see href="https://github.com/chubrik/XConsole#coloring">microsyntax</see>.
    /// </summary>
    /// <param name="format">
    /// A composite format string.
    /// Text can be colored using a simple <see href="https://github.com/chubrik/XConsole#coloring">microsyntax</see>.
    /// </param>
    /// <returns><inheritdoc cref="WriteLine(string?)"/></returns>
    /// <inheritdoc cref="Console.WriteLine(string, ReadOnlySpan{object?})"/>
    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, params ReadOnlySpan<object?> arg)
    {
        if (format.Length == 0)
            return WriteLine();

        return WriteBase(ConsoleItem.Parse(string.Format(format, arg)), [], isWriteLine: true);
    }
#endif

    /// <inheritdoc cref="Console.WriteLine(bool)"/>
    /// <returns><inheritdoc cref="WriteLine(string?)"/></returns>
    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(bool value)
    {
        return WriteBase(new ConsoleItem(value.ToString()), [], isWriteLine: true);
    }

    /// <inheritdoc cref="Console.WriteLine(char)"/>
    /// <returns><inheritdoc cref="WriteLine(string?)"/></returns>
    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(char value)
    {
        return WriteBase(new ConsoleItem(value.ToString()), [], isWriteLine: true);
    }

    /// <inheritdoc cref="Console.WriteLine(char[])"/>
    /// <returns><inheritdoc cref="WriteLine(string?)"/></returns>
    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(char[]? buffer)
    {
        if (buffer == null || buffer.Length == 0)
            return WriteLine();

        return WriteBase(new ConsoleItem(new string(buffer)), [], isWriteLine: true);
    }

    /// <inheritdoc cref="Console.WriteLine(char[], int, int)"/>
    /// <returns><inheritdoc cref="WriteLine(string?)"/></returns>
    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(char[] buffer, int index, int count)
    {
        if (count == 0)
            return WriteLine();

        return WriteBase(new ConsoleItem(new string(buffer, index, count)), [], isWriteLine: true);
    }

#if NET10_0_OR_GREATER
    /// <inheritdoc cref="Console.WriteLine(ReadOnlySpan{char})"/>
    /// <returns><inheritdoc cref="WriteLine(string?)"/></returns>
    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(ReadOnlySpan<char> value)
    {
        return WriteBase(new ConsoleItem(value.ToString()), [], isWriteLine: true);
    }
#endif

    /// <inheritdoc cref="Console.WriteLine(decimal)"/>
    /// <returns><inheritdoc cref="WriteLine(string?)"/></returns>
    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(decimal value)
    {
        return WriteBase(new ConsoleItem(value.ToString()), [], isWriteLine: true);
    }

    /// <inheritdoc cref="Console.WriteLine(double)"/>
    /// <returns><inheritdoc cref="WriteLine(string?)"/></returns>
    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(double value)
    {
        return WriteBase(new ConsoleItem(value.ToString()), [], isWriteLine: true);
    }

    /// <inheritdoc cref="Console.WriteLine(float)"/>
    /// <returns><inheritdoc cref="WriteLine(string?)"/></returns>
    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(float value)
    {
        return WriteBase(new ConsoleItem(value.ToString()), [], isWriteLine: true);
    }

    /// <inheritdoc cref="Console.WriteLine(int)"/>
    /// <returns><inheritdoc cref="WriteLine(string?)"/></returns>
    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(int value)
    {
        return WriteBase(new ConsoleItem(value.ToString()), [], isWriteLine: true);
    }

    /// <inheritdoc cref="Console.WriteLine(uint)"/>
    /// <returns><inheritdoc cref="WriteLine(string?)"/></returns>
    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(uint value)
    {
        return WriteBase(new ConsoleItem(value.ToString()), [], isWriteLine: true);
    }

    /// <inheritdoc cref="Console.WriteLine(long)"/>
    /// <returns><inheritdoc cref="WriteLine(string?)"/></returns>
    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(long value)
    {
        return WriteBase(new ConsoleItem(value.ToString()), [], isWriteLine: true);
    }

    /// <inheritdoc cref="Console.WriteLine(ulong)"/>
    /// <returns><inheritdoc cref="WriteLine(string?)"/></returns>
    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(ulong value)
    {
        return WriteBase(new ConsoleItem(value.ToString()), [], isWriteLine: true);
    }

    /// <inheritdoc cref="Console.WriteLine(object?)"/>
    /// <returns><inheritdoc cref="WriteLine(string?)"/></returns>
    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(object? value)
    {
        var valueStr = value?.ToString();

        if (string.IsNullOrEmpty(valueStr))
            return WriteLine();
#if !NET
#pragma warning disable CS8604 // Possible null reference argument.
#endif
        return WriteBase(new ConsoleItem(valueStr), [], isWriteLine: true);
#if !NET
#pragma warning restore CS8604 // Possible null reference argument.
#endif
    }

    #endregion

    #region Internal utils

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

    #region Remaining API

    /// <inheritdoc cref="Console.In"/>
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
    public static TextReader In => Console.In;

    /// <inheritdoc cref="Console.InputEncoding"/>
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
    public static Encoding InputEncoding
    {
        get => Console.InputEncoding;
        set
        {
            lock (_syncLock)
                Console.InputEncoding = value;
        }
    }

    /// <inheritdoc cref="Console.OutputEncoding"/>
    public static Encoding OutputEncoding
    {
        get => Console.OutputEncoding;
        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
        set
        {
            lock (_syncLock)
                Console.OutputEncoding = value;
        }
    }

    /// <inheritdoc cref="Console.KeyAvailable"/>
    public static bool KeyAvailable => Console.KeyAvailable;

    /// <inheritdoc cref="Console.ReadKey()"/>
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
    public static ConsoleKeyInfo ReadKey()
    {
        return ReadKey(intercept: false);
    }

    /// <inheritdoc cref="Console.ReadKey(bool)"/>
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
    public static ConsoleKeyInfo ReadKey(bool intercept)
    {
        var key = Console.ReadKey(intercept: true);

        if (!intercept)
            Write(key.KeyChar);

        return key;
    }

    /// <inheritdoc cref="Console.Out"/>
    public static TextWriter Out => Console.Out;

    /// <inheritdoc cref="Console.Error"/>
    public static TextWriter Error => Console.Error;

    /// <inheritdoc cref="Console.IsInputRedirected"/>
    public static bool IsInputRedirected => Console.IsInputRedirected;

    /// <inheritdoc cref="Console.IsOutputRedirected"/>
    public static bool IsOutputRedirected => Console.IsOutputRedirected;

    /// <inheritdoc cref="Console.IsErrorRedirected"/>
    public static bool IsErrorRedirected => Console.IsErrorRedirected;

    /// <inheritdoc cref="Console.CursorSize"/>
    public static int CursorSize
    {
        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("browser")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
        get => Console.CursorSize;
        [SupportedOSPlatform("windows")]
        set => Console.CursorSize = value;
    }

    /// <inheritdoc cref="Console.NumberLock"/>
    [SupportedOSPlatform("windows")]
    public static bool NumberLock => Console.NumberLock;

    /// <inheritdoc cref="Console.CapsLock"/>
    [SupportedOSPlatform("windows")]
    public static bool CapsLock => Console.CapsLock;

    /// <inheritdoc cref="Console.BackgroundColor"/>
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
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

    /// <inheritdoc cref="Console.ForegroundColor"/>
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
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

    /// <inheritdoc cref="Console.ResetColor()"/>
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
    public static void ResetColor()
    {
        lock (_syncLock)
            Console.ResetColor();
    }

    /// <inheritdoc cref="Console.BufferWidth"/>
    public static int BufferWidth
    {
        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("browser")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
        get => Console.BufferWidth;
        [SupportedOSPlatform("windows")]
        set
        {
            lock (_syncLock)
                Console.BufferWidth = value;
        }
    }

    /// <inheritdoc cref="Console.BufferHeight"/>
    public static int BufferHeight
    {
        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("browser")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
        get => Console.BufferHeight;
        [SupportedOSPlatform("windows")]
        set
        {
            lock (_syncLock)
            {
                Console.BufferHeight = value;
                _maxTop = value - 1;
            }
        }
    }

    /// <inheritdoc cref="Console.SetBufferSize(int, int)"/>
    [SupportedOSPlatform("windows")]
    public static void SetBufferSize(int width, int height)
    {
        lock (_syncLock)
            Console.SetBufferSize(width: width, height: height);
    }

    /// <inheritdoc cref="Console.WindowLeft"/>
    public static int WindowLeft
    {
        get => Console.WindowLeft;
        [SupportedOSPlatform("windows")]
        set => Console.WindowLeft = value;
    }

    /// <inheritdoc cref="Console.WindowTop"/>
    public static int WindowTop
    {
        get => Console.WindowTop;
        [SupportedOSPlatform("windows")]
        set => Console.WindowTop = value;
    }

    /// <inheritdoc cref="Console.WindowWidth"/>
    public static int WindowWidth
    {
        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("browser")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
        get => Console.WindowWidth;
        [SupportedOSPlatform("windows")]
        set
        {
            lock (_syncLock)
                Console.WindowWidth = value;
        }
    }

    /// <inheritdoc cref="Console.WindowHeight"/>
    public static int WindowHeight
    {
        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("browser")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
        get => Console.WindowHeight;
        [SupportedOSPlatform("windows")]
        set
        {
            lock (_syncLock)
                Console.WindowHeight = value;
        }
    }

    /// <inheritdoc cref="Console.SetWindowPosition(int, int)"/>
    [SupportedOSPlatform("windows")]
    public static void SetWindowPosition(int left, int top)
    {
        Console.SetWindowPosition(left: left, top: top);
    }

    /// <inheritdoc cref="Console.SetWindowSize(int, int)"/>
    [SupportedOSPlatform("windows")]
    public static void SetWindowSize(int width, int height)
    {
        lock (_syncLock)
            Console.SetWindowSize(width: width, height: height);
    }

    /// <inheritdoc cref="Console.LargestWindowWidth"/>
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
    public static int LargestWindowWidth => Console.LargestWindowWidth;

    /// <inheritdoc cref="Console.LargestWindowHeight"/>
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
    public static int LargestWindowHeight => Console.LargestWindowHeight;

    /// <inheritdoc cref="Console.CursorVisible"/>
    public static bool CursorVisible
    {
        [SupportedOSPlatform("windows")]
        get
        {
            lock (_syncLock)
                return Console.CursorVisible;
        }
        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("browser")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
        set
        {
            lock (_syncLock)
                Console.CursorVisible = _cursorVisible = value;
        }
    }

    /// <inheritdoc cref="Console.CursorLeft"/>
    /// <remarks>
    /// See also:<br/>
    /// • <seealso cref="CursorPosition"/> – smart <seealso cref="ConsolePosition"/> structure,
    /// resistant to console buffer overflow and always points to the correct position within the console buffer area.
    /// </remarks>
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
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

    /// <inheritdoc cref="Console.CursorTop"/>
    /// <remarks>
    /// See also:<br/>
    /// • <seealso cref="CursorPosition"/> – smart <seealso cref="ConsolePosition"/> structure,
    /// resistant to console buffer overflow and always points to the correct position within the console buffer area.
    /// </remarks>
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
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

    /// <summary>
    /// Gets the position of the cursor.
    /// </summary>
    /// <remarks>
    /// See also:<br/>
    /// • <seealso cref="CursorPosition"/> – smart <seealso cref="ConsolePosition"/> structure,
    /// resistant to console buffer overflow and always points to the correct position within the console buffer area.
    /// </remarks>
    /// <returns>The column and row position of the cursor.</returns>
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
    public static (int Left, int Top) GetCursorPosition()
    {
        lock (_syncLock)
#if NET
            return Console.GetCursorPosition();
#else
            return (Console.CursorLeft, Console.CursorTop);
#endif
    }

    /// <inheritdoc cref="Console.Title"/>
    public static string Title
    {
        [SupportedOSPlatform("windows")]
        get => Console.Title;
        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("browser")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
        set => Console.Title = value;
    }

    /// <inheritdoc cref="Console.Beep()"/>
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
    public static void Beep() => Console.Beep();

    /// <inheritdoc cref="Console.Beep(int, int)"/>
    [SupportedOSPlatform("windows")]
    public static void Beep(int frequency, int duration) => Console.Beep(frequency: frequency, duration: duration);

    /// <inheritdoc cref="Console.MoveBufferArea(int, int, int, int, int, int)"/>
    [SupportedOSPlatform("windows")]
    public static void MoveBufferArea(
        int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight, int targetLeft, int targetTop)
    {
        lock (_syncLock)
            Console.MoveBufferArea(
                sourceLeft: sourceLeft, sourceTop: sourceTop,
                sourceWidth: sourceWidth, sourceHeight: sourceHeight,
                targetLeft: targetLeft, targetTop: targetTop);
    }

    /// <inheritdoc cref="Console.MoveBufferArea(int, int, int, int, int, int, char, ConsoleColor, ConsoleColor)"/>
    [SupportedOSPlatform("windows")]
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

    /// <inheritdoc cref="Console.Clear()"/>
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
    public static void Clear()
    {
        lock (_syncLock)
            Console.Clear();
    }

    /// <inheritdoc cref="Console.SetCursorPosition(int, int)"/>
    /// <remarks>
    /// See also:<br/>
    /// • <seealso cref="CursorPosition"/> – smart <seealso cref="ConsolePosition"/> structure,
    /// resistant to console buffer overflow and always points to the correct position within the console buffer area.
    /// </remarks>
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
    public static void SetCursorPosition(int left, int top)
    {
        lock (_syncLock)
            Console.SetCursorPosition(left: left, top: top);
    }

    /// <inheritdoc cref="Console.CancelKeyPress"/>
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
    public static event ConsoleCancelEventHandler? CancelKeyPress
    {
        add => Console.CancelKeyPress += value;
        remove => Console.CancelKeyPress -= value;
    }

    /// <inheritdoc cref="Console.TreatControlCAsInput"/>
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
    public static bool TreatControlCAsInput
    {
        get => Console.TreatControlCAsInput;
        set => Console.TreatControlCAsInput = value;
    }

    /// <inheritdoc cref="Console.OpenStandardInput()"/>
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
    public static Stream OpenStandardInput()
    {
        lock (_syncLock)
            return Console.OpenStandardInput();
    }

    /// <inheritdoc cref="Console.OpenStandardInput(int)"/>
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    public static Stream OpenStandardInput(int bufferSize)
    {
        lock (_syncLock)
            return Console.OpenStandardInput(bufferSize: bufferSize);
    }

    /// <inheritdoc cref="Console.OpenStandardOutput()"/>
    public static Stream OpenStandardOutput()
    {
        lock (_syncLock)
            return Console.OpenStandardOutput();
    }

    /// <inheritdoc cref="Console.OpenStandardOutput(int)"/>
    public static Stream OpenStandardOutput(int bufferSize)
    {
        lock (_syncLock)
            return Console.OpenStandardOutput(bufferSize: bufferSize);
    }

    /// <inheritdoc cref="Console.OpenStandardError()"/>
    public static Stream OpenStandardError()
    {
        return Console.OpenStandardError();
    }

    /// <inheritdoc cref="Console.OpenStandardError(int)"/>
    public static Stream OpenStandardError(int bufferSize)
    {
        return Console.OpenStandardError(bufferSize: bufferSize);
    }

    /// <inheritdoc cref="Console.SetIn(TextReader)"/>
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
    public static void SetIn(TextReader newIn)
    {
        lock (_syncLock)
            Console.SetIn(newIn: newIn);
    }

    /// <inheritdoc cref="Console.SetOut(TextWriter)"/>
    public static void SetOut(TextWriter newOut)
    {
        lock (_syncLock)
            Console.SetOut(newOut: newOut);
    }

    /// <inheritdoc cref="Console.SetError(TextWriter)"/>
    public static void SetError(TextWriter newError)
    {
        Console.SetError(newError: newError);
    }

    /// <inheritdoc cref="Console.Read()"/>
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    public static int Read()
    {
        lock (_syncLock)
            return Console.Read();
    }

    #endregion
}
