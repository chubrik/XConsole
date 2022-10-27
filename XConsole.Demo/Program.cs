#if NET5_0
#pragma warning disable CA1416 // Validate platform compatibility
#endif

using Chubrik.XConsole.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Chubrik.XConsole.Demo;

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

        if (!Console.Utils.Confirm())
            return;

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
        var animationList = new List<ConsoleAnimation>();

        for (; fileIndex < Math.Min(100, files.Length); fileIndex++)
        {
            await Task.Delay(50);
            var endPos = Console.WriteLine(files[fileIndex].Name + ' ').End;
            endPosList.Add(endPos);

            if ((fileIndex + 1) % 10 == 0)
            {
                var animation = endPosList[fileIndex - 7].AnimateEllipsis();
                animationList.Add(animation);
            }
        }

        foreach (var animation in animationList)
        {
            await Task.Delay(300);
            animation.Stop().TryWrite("bC`ok");
        }
    }
}
