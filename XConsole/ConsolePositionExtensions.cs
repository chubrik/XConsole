namespace Chubrik.XConsole;

using System;
using System.Collections.Generic;
using System.Diagnostics;

public static class ConsolePositionExtensions
{
    [Obsolete("At least one argument should be specified.", error: true)]
    public static void Write(this ConsolePosition _) => throw new InvalidOperationException();

    public static ConsolePosition Write(this ConsolePosition position, params string?[] values)
    {
        Debug.Assert(values.Length > 0);
        return XConsole.WriteToPosition(position, values);
    }

    public static ConsolePosition Write(this ConsolePosition position, IReadOnlyList<string?> values)
    {
        return XConsole.WriteToPosition(position, values);
    }

    [Obsolete("At least one argument should be specified.", error: true)]
    public static void TryWrite(this ConsolePosition _) => throw new InvalidOperationException();

    public static ConsolePosition? TryWrite(this ConsolePosition position, params string?[] values)
    {
        Debug.Assert(values.Length > 0);

        try
        {
            return XConsole.WriteToPosition(position, values);
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }
    }

    public static ConsolePosition? TryWrite(this ConsolePosition position, IReadOnlyList<string?> values)
    {
        try
        {
            return XConsole.WriteToPosition(position, values);
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }
    }
}
