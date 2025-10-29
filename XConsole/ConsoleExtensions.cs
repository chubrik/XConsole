﻿#pragma warning disable IDE0060 // Remove unused parameter

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
    /// <returns><see langword="True"/> or <see langword="false"/> according to the user’s decision.</returns>
    public static bool Confirm(
        this ConsoleExtras extras, string message = "Continue? [y/n]: ", string yes = "Yes", string no = "No")
    {
        var yesItem = ConsoleItem.Parse(yes);
        var noItem = ConsoleItem.Parse(no);
        var yesLength = yesItem.GetSingleLineLengthOrMinusOne();
        var noLength = noItem.GetSingleLineLengthOrMinusOne();

        if (yesLength < 0)
        {
            yesItem = new ConsoleItem("Yes");
            yesLength = 3;
        }

        if (noLength < 0)
        {
            noItem = new ConsoleItem("No");
            noLength = 2;
        }

        var yesClear =
            new ConsoleItem(new string('\b', yesLength) + new string(' ', yesLength) + new string('\b', yesLength));

        var noClear =
            new ConsoleItem(new string('\b', noLength) + new string(' ', noLength) + new string('\b', noLength));

        return XConsole.Sync(() =>
        {
            bool? result = null;
            XConsole.Write(message);

            for (; ; )
            {
                var key = Console.ReadKey(intercept: true).Key;

                switch (key)
                {
                    case ConsoleKey.Y:
                        if (result == null)
                            XConsole.WriteItemsIsolated([yesItem]);
                        else if (result == false)
                            XConsole.WriteItemsIsolated([noClear, yesItem]);

                        result = true;
                        continue;

                    case ConsoleKey.N:
                        if (result == null)
                            XConsole.WriteItemsIsolated([noItem]);
                        else if (result == true)
                            XConsole.WriteItemsIsolated([yesClear, noItem]);

                        result = false;
                        continue;

                    case ConsoleKey.Backspace:
                        if (result == true)
                            XConsole.WriteItemsIsolated([yesClear]);
                        else if (result == false)
                            XConsole.WriteItemsIsolated([noClear]);

                        result = null;
                        continue;

                    case ConsoleKey.Enter:
                        if (result != null)
                        {
                            XConsole.WriteLine();
                            return result.Value;
                        }
                        continue;

                    default:
                        continue;
                }
            }
        });
    }
}
