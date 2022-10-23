namespace Chubrik.XConsole.Utils;

using System;

public interface IConsoleAnimation : IDisposable
{
    public void Stop();
    public ConsolePosition StopAndWrite(params string?[] values);
    public ConsolePosition? StopAndTryWrite(params string?[] values);
}
