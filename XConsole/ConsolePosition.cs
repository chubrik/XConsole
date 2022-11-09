#if !NET
#pragma warning disable CS8604 // Possible null reference argument.
#endif

namespace Chubrik.XConsole;

using System;
using System.Collections.Generic;
using System.Diagnostics;
#if NET
using System.Runtime.Versioning;
#endif
#if NET7_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

#if NET
[UnsupportedOSPlatform("android")]
[UnsupportedOSPlatform("browser")]
[UnsupportedOSPlatform("ios")]
[UnsupportedOSPlatform("tvos")]
#endif
public readonly struct ConsolePosition
{
    public int Left { get; }
    public int InitialTop { get; }
    internal long ShiftTop { get; }

    public int? ActualTop => XConsole.GetPositionActualTop(this);

    internal ConsolePosition(int left, int top, long shiftTop)
    {
        Left = left;
        InitialTop = top;
        ShiftTop = shiftTop;
    }

    public ConsolePosition(int left, int top)
    {
        Left = left;
        InitialTop = top;
        ShiftTop = XConsole.ShiftTop;
    }

    [Obsolete("Arguments should be specified.", error: true)]
    public ConsolePosition() => throw new InvalidOperationException();

    #region Write

    [Obsolete("At least one argument should be specified.", error: true)]
    public void Write() => throw new InvalidOperationException();

    public ConsolePosition Write(string? value)
    {
        return XConsole.WriteToPosition(this, new[] { ConsoleItem.Parse(value) });
    }

    public ConsolePosition Write(params string?[] values)
    {
        Debug.Assert(values.Length > 0);
        var items = new ConsoleItem[values.Length];

        for (var i = 0; i < values.Length; i++)
            items[i] = ConsoleItem.Parse(values[i]);

        return XConsole.WriteToPosition(this, items);
    }

    public ConsolePosition Write(IReadOnlyList<string?> values)
    {
        var items = new ConsoleItem[values.Count];

        for (var i = 0; i < values.Count; i++)
            items[i] = ConsoleItem.Parse(values[i]);

        return XConsole.WriteToPosition(this, items);
    }

    public ConsolePosition Write(bool value)
    {
        return XConsole.WriteToPosition(this, new[] { new ConsoleItem(value.ToString()) });
    }

    public ConsolePosition Write(char value)
    {
        return XConsole.WriteToPosition(this, new[] { new ConsoleItem(value.ToString()) });
    }

    public ConsolePosition Write(char[]? buffer)
    {
        var value = buffer?.ToString();

        return string.IsNullOrEmpty(value)
            ? this
            : XConsole.WriteToPosition(this, new[] { new ConsoleItem(value) });
    }

    public ConsolePosition Write(char[] buffer, int index, int count)
    {
        var value = buffer.ToString()?.Substring(index, count);

        return string.IsNullOrEmpty(value)
            ? this
            : XConsole.WriteToPosition(this, new[] { new ConsoleItem(value) });
    }

    public ConsolePosition Write(decimal value)
    {
        return XConsole.WriteToPosition(this, new[] { new ConsoleItem(value.ToString()) });
    }

    public ConsolePosition Write(double value)
    {
        return XConsole.WriteToPosition(this, new[] { new ConsoleItem(value.ToString()) });
    }

    public ConsolePosition Write(int value)
    {
        return XConsole.WriteToPosition(this, new[] { new ConsoleItem(value.ToString()) });
    }

    public ConsolePosition Write(long value)
    {
        return XConsole.WriteToPosition(this, new[] { new ConsoleItem(value.ToString()) });
    }

    public ConsolePosition Write(object? value)
    {
        var valueStr = value?.ToString();

        return string.IsNullOrEmpty(valueStr)
            ? this
            : XConsole.WriteToPosition(this, new[] { new ConsoleItem(valueStr) });
    }

    public ConsolePosition Write(float value)
    {
        return XConsole.WriteToPosition(this, new[] { new ConsoleItem(value.ToString()) });
    }

    public ConsolePosition Write(
#if NET7_0_OR_GREATER
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
        string format, object? arg0)
    {
        return string.IsNullOrEmpty(format)
            ? this
            : XConsole.WriteToPosition(this, new[] { ConsoleItem.Parse(string.Format(format, arg0)) });
    }

    public ConsolePosition Write(
#if NET7_0_OR_GREATER
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
        string format, object? arg0, object? arg1)
    {
        return string.IsNullOrEmpty(format)
            ? this
            : XConsole.WriteToPosition(this, new[] { ConsoleItem.Parse(string.Format(format, arg0, arg1)) });
    }

    public ConsolePosition Write(
#if NET7_0_OR_GREATER
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
        string format, object? arg0, object? arg1, object? arg2)
    {
        return string.IsNullOrEmpty(format)
            ? this
            : XConsole.WriteToPosition(this, new[] { ConsoleItem.Parse(string.Format(format, arg0, arg1, arg2)) });
    }

    public ConsolePosition Write(
#if NET7_0_OR_GREATER
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
        string format, params object?[]? arg)
    {
        return string.IsNullOrEmpty(format)
            ? this
            : XConsole.WriteToPosition(
                this, new[] { ConsoleItem.Parse(string.Format(format, arg ?? Array.Empty<object?>())) });
    }

    public ConsolePosition Write(uint value)
    {
        return XConsole.WriteToPosition(this, new[] { new ConsoleItem(value.ToString()) });
    }

    public ConsolePosition Write(ulong value)
    {
        return XConsole.WriteToPosition(this, new[] { new ConsoleItem(value.ToString()) });
    }

    #endregion

    #region TryWrite

    [Obsolete("At least one argument should be specified.", error: true)]
    public void TryWrite() => throw new InvalidOperationException();

    public ConsolePosition? TryWrite(string? value)
    {
        return TryWriteBase(new[] { ConsoleItem.Parse(value) });
    }

    public ConsolePosition? TryWrite(params string?[] values)
    {
        Debug.Assert(values.Length > 0);
        var items = new ConsoleItem[values.Length];

        for (var i = 0; i < values.Length; i++)
            items[i] = ConsoleItem.Parse(values[i]);

        return TryWriteBase(items);
    }

    public ConsolePosition? TryWrite(IReadOnlyList<string?> values)
    {
        var items = new ConsoleItem[values.Count];

        for (var i = 0; i < values.Count; i++)
            items[i] = ConsoleItem.Parse(values[i]);

        return TryWriteBase(items);
    }

    public ConsolePosition? TryWrite(bool value)
    {
        return TryWriteBase(new[] { new ConsoleItem(value.ToString()) });
    }

    public ConsolePosition? TryWrite(char value)
    {
        return TryWriteBase(new[] { new ConsoleItem(value.ToString()) });
    }

    public ConsolePosition? TryWrite(char[]? buffer)
    {
        var value = buffer?.ToString();

        return string.IsNullOrEmpty(value)
            ? this
            : TryWriteBase(new[] { new ConsoleItem(value) });
    }

    public ConsolePosition? TryWrite(char[] buffer, int index, int count)
    {
        var value = buffer.ToString()?.Substring(index, count);

        return string.IsNullOrEmpty(value)
            ? this
            : TryWriteBase(new[] { new ConsoleItem(value) });
    }

    public ConsolePosition? TryWrite(decimal value)
    {
        return TryWriteBase(new[] { new ConsoleItem(value.ToString()) });
    }

    public ConsolePosition? TryWrite(double value)
    {
        return TryWriteBase(new[] { new ConsoleItem(value.ToString()) });
    }

    public ConsolePosition? TryWrite(int value)
    {
        return TryWriteBase(new[] { new ConsoleItem(value.ToString()) });
    }

    public ConsolePosition? TryWrite(long value)
    {
        return TryWriteBase(new[] { new ConsoleItem(value.ToString()) });
    }

    public ConsolePosition? TryWrite(object? value)
    {
        var valueStr = value?.ToString();

        return string.IsNullOrEmpty(valueStr)
            ? this
            : TryWriteBase(new[] { new ConsoleItem(valueStr) });
    }

    public ConsolePosition? TryWrite(float value)
    {
        return TryWriteBase(new[] { new ConsoleItem(value.ToString()) });
    }

    public ConsolePosition? TryWrite(
#if NET7_0_OR_GREATER
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
        string format, object? arg0)
    {
        return string.IsNullOrEmpty(format)
            ? this
            : TryWriteBase(new[] { ConsoleItem.Parse(string.Format(format, arg0)) });
    }

    public ConsolePosition? TryWrite(
#if NET7_0_OR_GREATER
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
        string format, object? arg0, object? arg1)
    {
        return string.IsNullOrEmpty(format)
            ? this
            : TryWriteBase(new[] { ConsoleItem.Parse(string.Format(format, arg0, arg1)) });
    }

    public ConsolePosition? TryWrite(
#if NET7_0_OR_GREATER
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
        string format, object? arg0, object? arg1, object? arg2)
    {
        return string.IsNullOrEmpty(format)
            ? this
            : TryWriteBase(new[] { ConsoleItem.Parse(string.Format(format, arg0, arg1, arg2)) });
    }

    public ConsolePosition? TryWrite(
#if NET7_0_OR_GREATER
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
        string format, params object?[]? arg)
    {
        return string.IsNullOrEmpty(format)
            ? this
            : TryWriteBase(new[] { ConsoleItem.Parse(string.Format(format, arg ?? Array.Empty<object?>())) });
    }

    public ConsolePosition? TryWrite(uint value)
    {
        return TryWriteBase(new[] { new ConsoleItem(value.ToString()) });
    }

    public ConsolePosition? TryWrite(ulong value)
    {
        return TryWriteBase(new[] { new ConsoleItem(value.ToString()) });
    }

    private ConsolePosition? TryWriteBase(ConsoleItem[] items)
    {
        try
        {
            return XConsole.WriteToPosition(this, items);
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }
    }

    #endregion
}
