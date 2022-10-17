#if NET5_0
#pragma warning disable CA1416 // Validate platform compatibility
#endif

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using XConsole.Utils;

namespace XConsole.Demo;

public static class Program
{
    public static async Task Main()
    {
        Console.WriteLine("bC`Hello", "R`, ", "mY`World", "G`!");

        //
        Console.WriteLine();
        //

        Console.WriteLine("d`##########");
        var (begin, end) = Console.Write("d`##########");
        Console.WriteLine();
        begin.Write("Begin");
        end.Write("End");

        //

        new ConsolePosition(30, 1).Write("Y`Custom position");

        //
        Console.WriteLine();
        //

        Console.Write("W`Continue? [Y/n]: ");

        while (true)
        {
            var key = Console.ReadKey(true).Key;

            if (key == ConsoleKey.Y)
            {
                Console.WriteLine("G`Yes");
                break;
            }
            else if (key == ConsoleKey.N || key == ConsoleKey.Escape)
            {
                Console.WriteLine("R`No");
                return;
            }
        }

        //
        Console.WriteLine();
        //

        //Console.SetCursorPosition(0, 8950);

#if !NETSTANDARD1_3
        var sysFolder = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.System));
#else
        var sysFolder = new DirectoryInfo(@"C:\Windows\System32");
#endif
        var files = sysFolder.GetFiles("*.exe");
        var fileIndex = 0;
        Console.Pin(() => new[] { "m`This is pin!\n", $"g`Number of files: ", $"W`{fileIndex}" });
        var endPosList = new List<ConsolePosition>();
        var processList = new List<ProcessAnimation>();

        for (; fileIndex < Math.Min(100, files.Length); fileIndex++)
        {
            await Task.Delay(50);
            var endPos = Console.WriteLine(files[fileIndex].Name + ' ').End;
            endPosList.Add(endPos);

            if ((fileIndex + 1) % 10 == 0)
                processList.Add(endPosList[fileIndex - 7].StartProcessAnimation());
        }

        foreach (var process in processList)
        {
            await Task.Delay(300);
            process.StopAndTryWrite("bC`ok");
        }
    }
}
