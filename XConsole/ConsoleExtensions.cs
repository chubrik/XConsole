#pragma warning disable IDE0060 // Remove unused parameter

namespace Chubrik.XConsole;

using System;

/// <summary>
/// Common extensions for <see cref="ConsoleExtras"/>.
/// </summary>
public static class ConsoleExtensions
{
    /// <summary>
    /// Displays the <paramref name="message"/> and waits until the user presses Y or N and then Enter.
    /// </summary>
    /// <returns>True or False according to the user’s decision.</returns>
    public static bool Confirm(
        this ConsoleExtras extras, string message = "Continue? [y/n]: ", string yes = "Yes", string no = "No")
    {
        var yesItem = ConsoleItem.Parse(yes);
        var noItem = ConsoleItem.Parse(no);
        var yesLength = yesItem.GetSingleLineLengthOrZero();
        var noLength = noItem.GetSingleLineLengthOrZero();

        if (yesLength == 0)
        {
            yes = "Yes";
            yesLength = yes.Length;
        }

        if (noLength == 0)
        {
            no = "No";
            noLength = no.Length;
        }

        var yesClear = new string('\b', yesLength) + new string(' ', yesLength) + new string('\b', yesLength);
        var noClear = new string('\b', noLength) + new string(' ', noLength) + new string('\b', noLength);

        return XConsole.Sync(() =>
        {
            bool? answer = null;
            XConsole.Write(message);

            for (; ; )
            {
                var key = Console.ReadKey(intercept: true).Key;

                switch (key)
                {
                    case ConsoleKey.Y:
                        if (answer == null)
                            XConsole.Write(yes);
                        else if (answer == false)
                            XConsole.Write([noClear, yes]);

                        answer = true;
                        continue;

                    case ConsoleKey.N:
                        if (answer == null)
                            XConsole.Write(no);
                        else if (answer == true)
                            XConsole.Write([yesClear, no]);

                        answer = false;
                        continue;

                    case ConsoleKey.Backspace:
                        if (answer == true)
                            XConsole.Write(yesClear);
                        else if (answer == false)
                            XConsole.Write(noClear);

                        answer = null;
                        continue;

                    case ConsoleKey.Enter:
                        if (answer != null)
                        {
                            XConsole.WriteLine();
                            return answer.Value;
                        }
                        continue;

                    default:
                        continue;
                }
            }
        });
    }
}
