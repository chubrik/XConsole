namespace Chubrik.XConsole;

using System;

public interface IConsoleAnimation : IDisposable
{
    public ConsolePosition Stop();
}
