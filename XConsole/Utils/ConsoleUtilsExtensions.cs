#pragma warning disable IDE0060 // Remove unused parameter

namespace Chubrik.XConsole;

using System;
using System.Threading;
#if NET
using System.Runtime.Versioning;
#endif

/// <summary>
/// The most useful extensions for <see cref="ConsoleUtils"/>.
/// </summary>
public static class ConsoleUtilsExtensions
{
    /// <summary>
    /// Starts an ellipsis animation at the current position.
    /// </summary>
#if NET
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
#endif
    public static IConsoleAnimation AnimateEllipsis(this ConsoleUtils utils)
    {
        return XConsole.CursorPosition.AnimateEllipsis();
    }

    /// <summary>
    /// <inheritdoc cref="AnimateEllipsis(ConsoleUtils)"/>
    /// </summary>
#if NET
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
#endif
    public static IConsoleAnimation AnimateEllipsis(this ConsoleUtils utils, CancellationToken cancellationToken)
    {
        return XConsole.CursorPosition.AnimateEllipsis(cancellationToken);
    }

    /// <summary>
    /// Starts a spinner animation at the current position.
    /// </summary>
#if NET
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
#endif
    public static IConsoleAnimation AnimateSpinner(this ConsoleUtils utils)
    {
        return XConsole.CursorPosition.AnimateSpinner();
    }

    /// <summary>
    /// <inheritdoc cref="AnimateSpinner(ConsoleUtils)"/>
    /// </summary>
#if NET
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
#endif
    public static IConsoleAnimation AnimateSpinner(this ConsoleUtils utils, CancellationToken cancellationToken)
    {
        return XConsole.CursorPosition.AnimateSpinner(cancellationToken);
    }

    /// <summary>
    /// Displays the <paramref name="message"/> and waits until the user presses Y or N and then Enter.
    /// </summary>
    /// <returns>True or False according to the user’s decision.</returns>
#if NET
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
#endif
    public static bool Confirm(
        this ConsoleUtils utils, string message = "Continue? [y/n]: ", string yes = "Yes", string no = "No")
    {
        var yesItem = ConsoleItem.Parse(yes);
        var noItem = ConsoleItem.Parse(no);
        var yesLength = yesItem.Value.Length;
        var noLength = noItem.Value.Length;

        return XConsole.Sync(() =>
        {
            bool? answer = null;
            var answerPosition = XConsole.Write(message).End;

            for (; ; )
            {
                var key = Console.ReadKey(intercept: true).Key;

                switch (key)
                {
                    case ConsoleKey.Y:
                        if (answer != true)
                        {
                            if (answer == false)
                                answerPosition.Write(new string(' ', noLength));

                            answer = true;
                            XConsole.CursorPosition = XConsole.WriteToPosition(answerPosition, [yesItem]);
                        }
                        continue;

                    case ConsoleKey.N:
                        if (answer != false)
                        {
                            if (answer == true)
                                answerPosition.Write(new string(' ', yesLength));

                            answer = false;
                            XConsole.CursorPosition = XConsole.WriteToPosition(answerPosition, [noItem]);
                        }
                        continue;

                    case ConsoleKey.Enter:
                        if (answer != null)
                        {
                            XConsole.WriteLine();
                            return answer.Value;
                        }
                        continue;

                    case ConsoleKey.Backspace:
                        if (answer != null)
                        {
                            answer = null;
                            answerPosition.Write(new string(' ', Math.Max(yesLength, noLength)));
                            XConsole.CursorPosition = answerPosition;
                        }
                        continue;

                    default:
                        continue;
                }
            }
        });
    }
}
