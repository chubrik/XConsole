#pragma warning disable IDE0060 // Remove unused parameter

namespace Chubrik.XConsole;

using System;
#if NET
using System.Runtime.Versioning;
#endif

/// <summary>
/// Common extensions for <see cref="ConsoleExtras"/>.
/// </summary>
public static class ConsoleExtensions
{
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
        this ConsoleExtras extras, string message = "Continue? [y/n]: ", string yes = "Yes", string no = "No")
    {
        // todo Without positioning and without UnsupportedOSPlatform

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
