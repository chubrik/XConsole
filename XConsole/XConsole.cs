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
public static partial class XConsole
{
    #region Common

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
        UpdatePinImpl();
    }

    /// <summary>
    /// Pins the specified array of string values as a static text below the regular log messages.
    /// Text can be multiline and <see href="https://github.com/chubrik/XConsole#coloring">colored</see>.
    /// Pin is resistant to console buffer overflow.
    /// </summary>
    /// <param name="values">
    /// An array of values for pin below the regular log messages.
    /// Text can be colored using a simple <see href="https://github.com/chubrik/XConsole#coloring">microsyntax</see>.
    /// </param>
    /// <remarks>
    /// See also:<br/>• <seealso cref="Unpin()"/>
    /// </remarks>
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
    public static void Pin(string?[]? values)
    {
        if (!_positioningEnabled)
            return;

        _getPinValues = () => values;
        UpdatePinImpl();
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
    [Obsolete("Use string?[]? overload instead.")]
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
    public static void Pin(IReadOnlyList<string?> values)
    {
        if (!_positioningEnabled)
            return;

        var array = new string?[values.Count];

        for (var i = 0; i < values.Count; i++)
            array[i] = values[i];

        _getPinValues = () => array;
        UpdatePinImpl();
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

        UpdatePinImpl();
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
    public static void Pin(Func<string?[]?> getValues)
    {
        if (!_positioningEnabled)
            return;

        _getPinValues = getValues;
        UpdatePinImpl();
    }

    /// <summary>
    /// Pins a set of string values from <paramref name="getValues"/> callback as a dynamic text below the regular
    /// log messages. Text can be multiline and <see href="https://github.com/chubrik/XConsole#coloring">colored</see>,
    /// it updates every time <see cref="Write(string?)"/> or <see cref="WriteLine(string?)"/> method are called.
    /// Pin is resistant to console buffer overflow.
    /// </summary>
    /// <param name="getValues">
    /// The set of values callback to pin as a dynamic text below the regular log messages.
    /// Text can be colored using a simple <see href="https://github.com/chubrik/XConsole#coloring">microsyntax</see>.
    /// </param>
    /// <inheritdoc cref="Pin(Func{string?})"/>
    [Obsolete("Use Func<string?[]?> overload instead.")]
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
    public static void Pin(Func<IReadOnlyList<string?>> getValues)
    {
        if (!_positioningEnabled)
            return;

        _getPinValues = () =>
        {
            var values = getValues();
            var array = new string?[values.Count];

            for (var i = 0; i < values.Count; i++)
                array[i] = values[i];

            return array;
        };

        UpdatePinImpl();
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
        UpdatePinImpl();
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
        UnpinImpl();
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => CursorPositionImpl;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => CursorPositionImpl = value;
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string? ReadLine()
    {
        return ReadLineImpl();
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string? ReadLine(ConsoleReadLineMode mode, char maskChar = '\u2022')
    {
        return ReadLineImpl(mode, maskChar);
    }

    #endregion

    #region Write

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
        return WriteParced(value, isWriteLine: false);
    }

    /// <summary>
    /// Writes the specified array of string values to the standard output stream.
    /// Text can be colored using a simple <see href="https://github.com/chubrik/XConsole#coloring">microsyntax</see>.
    /// </summary>
    /// <param name="values">
    /// An array of values to write.
    /// Text can be colored using a simple <see href="https://github.com/chubrik/XConsole#coloring">microsyntax</see>.
    /// </param>
    /// <inheritdoc cref="Write(string?)"/>
    public static (ConsolePosition Begin, ConsolePosition End) Write(string?[]? values)
    {
        return WriteParced(values ?? [], isWriteLine: false);
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
    [Obsolete("Use string?[]? overload instead.")]
    public static (ConsolePosition Begin, ConsolePosition End) Write(IReadOnlyList<string?> values)
    {
        var array = new string?[values.Count];

        for (var i = 0; i < values.Count; i++)
            array[i] = values[i];

        return WriteParced(array, isWriteLine: false);
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
        return WriteParced(string.Format(format, arg0), isWriteLine: false);
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
        return WriteParced(string.Format(format, arg0, arg1), isWriteLine: false);
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
        return WriteParced(string.Format(format, arg0, arg1, arg2), isWriteLine: false);
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
        return WriteParced(string.Format(format, arg ?? []), isWriteLine: false);
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
        return WriteParced(string.Format(format, arg), isWriteLine: false);
    }
#endif

    /// <inheritdoc cref="Console.Write(bool)"/>
    /// <returns><inheritdoc cref="Write(string?)"/></returns>
    public static (ConsolePosition Begin, ConsolePosition End) Write(bool value)
    {
        return WritePlain(value.ToString(), isWriteLine: false);
    }

    /// <inheritdoc cref="Console.Write(char)"/>
    /// <returns><inheritdoc cref="Write(string?)"/></returns>
    public static (ConsolePosition Begin, ConsolePosition End) Write(char value)
    {
        return WritePlain(value.ToString(), isWriteLine: false);
    }

    /// <inheritdoc cref="Console.Write(char[])"/>
    /// <returns><inheritdoc cref="Write(string?)"/></returns>
    public static (ConsolePosition Begin, ConsolePosition End) Write(char[]? buffer)
    {
        return WritePlain(new string(buffer), isWriteLine: false);
    }

    /// <inheritdoc cref="Console.Write(char[], int, int)"/>
    /// <returns><inheritdoc cref="Write(string?)"/></returns>
    public static (ConsolePosition Begin, ConsolePosition End) Write(char[] buffer, int index, int count)
    {
        return WritePlain(new string(buffer, index, count), isWriteLine: false);
    }

    /// <inheritdoc cref="Console.Write(decimal)"/>
    /// <returns><inheritdoc cref="Write(string?)"/></returns>
    public static (ConsolePosition Begin, ConsolePosition End) Write(decimal value)
    {
        return WritePlain(value.ToString(), isWriteLine: false);
    }

    /// <inheritdoc cref="Console.Write(double)"/>
    /// <returns><inheritdoc cref="Write(string?)"/></returns>
    public static (ConsolePosition Begin, ConsolePosition End) Write(double value)
    {
        return WritePlain(value.ToString(), isWriteLine: false);
    }

    /// <inheritdoc cref="Console.Write(float)"/>
    /// <returns><inheritdoc cref="Write(string?)"/></returns>
    public static (ConsolePosition Begin, ConsolePosition End) Write(float value)
    {
        return WritePlain(value.ToString(), isWriteLine: false);
    }

    /// <inheritdoc cref="Console.Write(int)"/>
    /// <returns><inheritdoc cref="Write(string?)"/></returns>
    public static (ConsolePosition Begin, ConsolePosition End) Write(int value)
    {
        return WritePlain(value.ToString(), isWriteLine: false);
    }

    /// <inheritdoc cref="Console.Write(uint)"/>
    /// <returns><inheritdoc cref="Write(string?)"/></returns>
    public static (ConsolePosition Begin, ConsolePosition End) Write(uint value)
    {
        return WritePlain(value.ToString(), isWriteLine: false);
    }

    /// <inheritdoc cref="Console.Write(long)"/>
    /// <returns><inheritdoc cref="Write(string?)"/></returns>
    public static (ConsolePosition Begin, ConsolePosition End) Write(long value)
    {
        return WritePlain(value.ToString(), isWriteLine: false);
    }

    /// <inheritdoc cref="Console.Write(ulong)"/>
    /// <returns><inheritdoc cref="Write(string?)"/></returns>
    public static (ConsolePosition Begin, ConsolePosition End) Write(ulong value)
    {
        return WritePlain(value.ToString(), isWriteLine: false);
    }

    /// <inheritdoc cref="Console.Write(object?)"/>
    /// <returns><inheritdoc cref="Write(string?)"/></returns>
    public static (ConsolePosition Begin, ConsolePosition End) Write(object? value)
    {
        return WritePlain(value?.ToString() ?? string.Empty, isWriteLine: false);
    }

#if NET10_0_OR_GREATER
    /// <inheritdoc cref="Console.Write(ReadOnlySpan{char})"/>
    /// <returns><inheritdoc cref="Write(string?)"/></returns>
    public static (ConsolePosition Begin, ConsolePosition End) Write(ReadOnlySpan<char> value)
    {
        return WritePlain(value.ToString(), isWriteLine: false);
    }
#endif

    #endregion

    #region WriteLine

    /// <inheritdoc cref="Console.WriteLine()"/>
    /// <returns><inheritdoc cref="WriteLine(string?)"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (ConsolePosition Begin, ConsolePosition End) WriteLine()
    {
        return WriteLineImpl();
    }

    /// <summary>
    /// <inheritdoc cref="Console.WriteLine(string?)"/>
    /// Text can be colored using a simple <see href="https://github.com/chubrik/XConsole#coloring">microsyntax</see>.
    /// </summary>
    /// <inheritdoc cref="Write(string?)"/>
    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(string? value)
    {
        return WriteParced(value, isWriteLine: true);
    }

    /// <summary>
    /// Writes the specified array of string values, followed by the current line terminator,
    /// to the standard output stream.
    /// Text can be colored using a simple <see href="https://github.com/chubrik/XConsole#coloring">microsyntax</see>.
    /// </summary>
    /// <param name="values">
    /// An array of values to write.
    /// Text can be colored using a simple <see href="https://github.com/chubrik/XConsole#coloring">microsyntax</see>.
    /// </param>
    /// <inheritdoc cref="WriteLine(string?)"/>
    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(string?[]? values)
    {
        return WriteParced(values ?? [], isWriteLine: true);
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
    [Obsolete("Use string?[]? overload instead.")]
    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(IReadOnlyList<string?> values)
    {
        var array = new string?[values.Count];

        for (var i = 0; i < values.Count; i++)
            array[i] = values[i];

        return WriteParced(array, isWriteLine: true);
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
        return WriteParced(string.Format(format, arg0), isWriteLine: true);
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
        return WriteParced(string.Format(format, arg0, arg1), isWriteLine: true);
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
        return WriteParced(string.Format(format, arg0, arg1, arg2), isWriteLine: true);
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
        return WriteParced(string.Format(format, arg ?? []), isWriteLine: true);
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
        return WriteParced(string.Format(format, arg), isWriteLine: true);
    }
#endif

    /// <inheritdoc cref="Console.WriteLine(bool)"/>
    /// <returns><inheritdoc cref="WriteLine(string?)"/></returns>
    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(bool value)
    {
        return WritePlain(value.ToString(), isWriteLine: true);
    }

    /// <inheritdoc cref="Console.WriteLine(char)"/>
    /// <returns><inheritdoc cref="WriteLine(string?)"/></returns>
    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(char value)
    {
        return WritePlain(value.ToString(), isWriteLine: true);
    }

    /// <inheritdoc cref="Console.WriteLine(char[])"/>
    /// <returns><inheritdoc cref="WriteLine(string?)"/></returns>
    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(char[]? buffer)
    {
        return WritePlain(new string(buffer), isWriteLine: true);
    }

    /// <inheritdoc cref="Console.WriteLine(char[], int, int)"/>
    /// <returns><inheritdoc cref="WriteLine(string?)"/></returns>
    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(char[] buffer, int index, int count)
    {
        return WritePlain(new string(buffer, index, count), isWriteLine: true);
    }

    /// <inheritdoc cref="Console.WriteLine(decimal)"/>
    /// <returns><inheritdoc cref="WriteLine(string?)"/></returns>
    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(decimal value)
    {
        return WritePlain(value.ToString(), isWriteLine: true);
    }

    /// <inheritdoc cref="Console.WriteLine(double)"/>
    /// <returns><inheritdoc cref="WriteLine(string?)"/></returns>
    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(double value)
    {
        return WritePlain(value.ToString(), isWriteLine: true);
    }

    /// <inheritdoc cref="Console.WriteLine(float)"/>
    /// <returns><inheritdoc cref="WriteLine(string?)"/></returns>
    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(float value)
    {
        return WritePlain(value.ToString(), isWriteLine: true);
    }

    /// <inheritdoc cref="Console.WriteLine(int)"/>
    /// <returns><inheritdoc cref="WriteLine(string?)"/></returns>
    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(int value)
    {
        return WritePlain(value.ToString(), isWriteLine: true);
    }

    /// <inheritdoc cref="Console.WriteLine(uint)"/>
    /// <returns><inheritdoc cref="WriteLine(string?)"/></returns>
    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(uint value)
    {
        return WritePlain(value.ToString(), isWriteLine: true);
    }

    /// <inheritdoc cref="Console.WriteLine(long)"/>
    /// <returns><inheritdoc cref="WriteLine(string?)"/></returns>
    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(long value)
    {
        return WritePlain(value.ToString(), isWriteLine: true);
    }

    /// <inheritdoc cref="Console.WriteLine(ulong)"/>
    /// <returns><inheritdoc cref="WriteLine(string?)"/></returns>
    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(ulong value)
    {
        return WritePlain(value.ToString(), isWriteLine: true);
    }

    /// <inheritdoc cref="Console.WriteLine(object?)"/>
    /// <returns><inheritdoc cref="WriteLine(string?)"/></returns>
    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(object? value)
    {
        return WritePlain(value?.ToString() ?? string.Empty, isWriteLine: true);
    }

#if NET10_0_OR_GREATER
    /// <inheritdoc cref="Console.WriteLine(ReadOnlySpan{char})"/>
    /// <returns><inheritdoc cref="WriteLine(string?)"/></returns>
    public static (ConsolePosition Begin, ConsolePosition End) WriteLine(ReadOnlySpan<char> value)
    {
        return WritePlain(value.ToString(), isWriteLine: true);
    }
#endif

    #endregion

    #region Remaining API wrappers

    /// <inheritdoc cref="Console.In"/>
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
    public static TextReader In
    {
        get => Console.In;
    }

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
    public static bool KeyAvailable
    {
        get => Console.KeyAvailable;
    }

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
    public static TextWriter Out
    {
        get => Console.Out;
    }

    /// <inheritdoc cref="Console.Error"/>
    public static TextWriter Error
    {
        get => Console.Error;
    }

    /// <inheritdoc cref="Console.IsInputRedirected"/>
    public static bool IsInputRedirected
    {
        get => Console.IsInputRedirected;
    }

    /// <inheritdoc cref="Console.IsOutputRedirected"/>
    public static bool IsOutputRedirected
    {
        get => Console.IsOutputRedirected;
    }

    /// <inheritdoc cref="Console.IsErrorRedirected"/>
    public static bool IsErrorRedirected
    {
        get => Console.IsErrorRedirected;
    }

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
    public static bool NumberLock
    {
        get => Console.NumberLock;
    }

    /// <inheritdoc cref="Console.CapsLock"/>
    [SupportedOSPlatform("windows")]
    public static bool CapsLock
    {
        get => Console.CapsLock;
    }

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
    public static int LargestWindowWidth
    {
        get => Console.LargestWindowWidth;
    }

    /// <inheritdoc cref="Console.LargestWindowHeight"/>
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
    public static int LargestWindowHeight
    {
        get => Console.LargestWindowHeight;
    }

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

#if NET
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
            return Console.GetCursorPosition();
    }
#endif

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
    public static void Beep()
    {
        Console.Beep();
    }

    /// <inheritdoc cref="Console.Beep(int, int)"/>
    [SupportedOSPlatform("windows")]
    public static void Beep(int frequency, int duration)
    {
        Console.Beep(frequency: frequency, duration: duration);
    }

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
        {
            _shiftTop = 0;
            Console.Clear();
            UpdatePinImpl();
        }
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
