using System;
using System.Drawing;

namespace Chubrik.XConsole.Demo;

public static class Screenshots
{
    public static void Summary()
    {
        Console.WriteLine("Regular log message: 12 apples");
        Console.WriteLine("Regular log message: 8 bananas");
        Console.WriteLine("Regular log message: 14 limes");
        Console.WriteLine("G`This line is colorized using simple microsyntax");
        Console.WriteLine("Regular log message: 7 apples");
        Console.WriteLine("Regular log message: 21 bananas");
        Console.WriteLine("Regular log message: 3 limes");
        Console.WriteLine("C`It is easy ", "Wb`to use many", "R` colors in ", "Y`one message", "d`  (safe for multitasking)");
        Console.WriteLine("Regular log message: 2 apples");
        Console.WriteLine("Regular log message: 11 bananas");
        Console.WriteLine("Regular log message: 5 limes");

        var ansiText = "Extended colors and underlining are possible too";
        var part = Math.Ceiling(ansiText.Length / (float)6);

        for (var step = 1; step <= 2; step++)
        {
            var maxRed = step == 1 ? 255 : 120;
            var maxGreen = step == 1 ? 255 : 80;
            var maxBlue = step == 1 ? 255 : 160;

            for (var i = 0; i < ansiText.Length; i++)
            {
                var red = i <= part ? maxRed : i < part * 2 ? (int)(maxRed * (1 - (i - part) / (float)part)) : i >= part * 5 ? maxRed : i > part * 4 ? (int)(maxRed * (i - part * 4) / (float)part) : 0;
                var green = i < part ? (int)(maxGreen * i / (float)part) : i <= part * 3 ? maxGreen : i < part * 4 ? (int)(maxGreen * (1 - (i - part * 3) / (float)part)) : 0;
                var blue = i <= part * 2 ? 0 : i < part * 3 ? (int)(maxBlue * (i - part * 2) / (float)part) : i <= part * 5 ? maxBlue : i < part * 6 ? (int)(maxBlue * (1 - (i - part * 5) / (float)part)) : 0;
                var letter = step == 1 ? ansiText[i].Color(red, green, blue) : ansiText[i].Color(ConsoleColor.White).BgColor(red, green, blue);
                Console.Write(i >= 20 && i <= 30 ? letter.Underline() : letter);
            }

            Console.WriteLine();
        }

        Console.WriteLine("Regular log message: 9 apples");
        Console.WriteLine("Regular log message: 10 bananas");
        Console.WriteLine("Regular log message: 1 limes", "M`  This text was written later using saved position");
        Console.WriteLine("Regular log message: 18 apples", "M`  (positioning is safe for 9000+ log lines)");
        Console.WriteLine("Regular log message: 6 bananas");
        Console.WriteLine("Regular log message: 10 limes");

        Console.Pin(() => new[]
        {
            "W`Pinned multiline results:\n",
            "Apples=", "R`154", ", bananas=", "Y`241", ", limes=", "G`87", ", tital=", "nc`482", ", time=", "g`12:48\n",
            "d`(Pin will not be broken after 9000 log lines)"
        });

        for (; ; )
            if (Console.ReadKey(true).Key == ConsoleKey.Escape)
                return;
    }

    public static void ColorsStandard()
    {
        Console.WriteLine("G`This line is colorized using simple microsyntax");
        Console.WriteLine("C`It is easy ", "Wb`to use many", "R` colors in ", "Y`one message");
    }

    public static void ColorsTable()
    {
        Console.WriteLine("d`code                        result  comment");
        Console.WriteLine("Console.Write(", "y`\"W`text\"", ");    ", "W`text    [W]", "hite");
        Console.WriteLine("Console.Write(", "y`\"Y`text\"", ");    ", "Y`text    [Y]", "ellow");
        Console.WriteLine("Console.Write(", "y`\"C`text\"", ");    ", "C`text    [C]", "yan");
        Console.WriteLine("Console.Write(", "y`\"G`text\"", ");    ", "G`text    [G]", "reen");
        Console.WriteLine("Console.Write(", "y`\"M`text\"", ");    ", "M`text    [M]", "agenta");
        Console.WriteLine("Console.Write(", "y`\"R`text\"", ");    ", "R`text    [R]", "ed");
        Console.WriteLine("Console.Write(", "y`\"B`text\"", ");    ", "B`text    [B]", "lue");
        Console.WriteLine("Console.Write(", "y`\"w`text\"", ");    ", "w`text    ", "dark [w]hite (gray)");
        Console.WriteLine("Console.Write(", "y`\"y`text\"", ");    ", "y`text    ", "dark ", "y`[y]", "ellow");
        Console.WriteLine("Console.Write(", "y`\"c`text\"", ");    ", "c`text    ", "dark ", "c`[c]", "yan");
        Console.WriteLine("Console.Write(", "y`\"g`text\"", ");    ", "g`text    ", "dark ", "g`[g]", "reen");
        Console.WriteLine("Console.Write(", "y`\"m`text\"", ");    ", "m`text    ", "dark ", "m`[m]", "agenta");
        Console.WriteLine("Console.Write(", "y`\"r`text\"", ");    ", "r`text    ", "dark ", "r`[r]", "ed");
        Console.WriteLine("Console.Write(", "y`\"b`text\"", ");    ", "b`text    ", "dark ", "b`[b]", "lue");
        Console.WriteLine("Console.Write(", "y`\"d`text\"", ");    ", "d`text    [d]", "ark gray");
        Console.WriteLine("Console.Write(", "y`\"n`text\"", ");    ", "n`text    ", "d`[n]", "o color (black)");
    }

    public static void ColorsExtended()
    {
        Console.WriteLine("Orange text".Color(Color.Orange));
        Console.WriteLine("Оrange with an indigo background".Color(Color.Orange).BgColor(Color.Indigo));
        Console.WriteLine(("Lime with " + "a brown".BgColor(Color.Brown) + " background").Color(Color.Lime));
        Console.WriteLine($"Aqua with {"a navy".BgColor(Color.Navy)} background".Color(Color.Aqua));
    }

    public static void DynamicPin1()
    {
        var value = 25;
        Console.WriteLine("Regular log message");
        Console.WriteLine("Regular log message");
        Console.WriteLine("Regular log message");
        Console.WriteLine("Regular log message");
        Console.Pin(() => "Simple pin, value=" + value);

        for (; ; )
            if (Console.ReadKey(true).Key == ConsoleKey.Escape)
                return;
    }

    public static void DynamicPin2()
    {
        var value = 25;
        Console.WriteLine("Regular log message");
        Console.WriteLine("Regular log message");
        Console.WriteLine("Regular log message");
        Console.WriteLine("Regular log message");
        Console.Pin(() => new[] { "Y`Multicolor", " pin, value=", "C`" + value });

        for (; ; )
            if (Console.ReadKey(true).Key == ConsoleKey.Escape)
                return;
    }

    public static void DynamicPin3()
    {
        var value = 25;
        Console.WriteLine("Regular log message");
        Console.WriteLine("Regular log message");
        Console.WriteLine("Regular log message");
        Console.Pin(() => new[] { "Multiline pin,\nvalue=", "C`" + value });

        for (; ; )
            if (Console.ReadKey(true).Key == ConsoleKey.Escape)
                return;
    }

    public static void Positioning()
    {
        var (begin, end) = Console.WriteLine("Hello, World!");
        begin.Write("C`HELLO");
        end = end.Write(" I'm here");
        end.Write("Y`!");
    }

    public static void ReadLine()
    {
        var (_, end) = Console.WriteLine("Please type password in masked mode: ••••••••");
        Console.WriteLine("Please type password with custom mask: ########");
        Console.WriteLine("Please type password in hidden mode:");
        Console.CursorPosition = end;

        for (; ; )
            if (Console.ReadKey(true).Key == ConsoleKey.Escape)
                return;
    }
}
