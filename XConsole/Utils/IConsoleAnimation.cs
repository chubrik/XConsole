namespace Chubrik.XConsole;

using System;

/// <summary>
/// An instance of the animation running in the console.
/// </summary>
public interface IConsoleAnimation : IDisposable
{
    /// <summary>
    /// Stops the animation running in the console.
    /// </summary>
    /// <returns>Begin <see cref="ConsolePosition"/> of the animation.</returns>
    public ConsolePosition Stop();
}
