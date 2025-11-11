using Chubrik.XConsole.StringExtensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace Chubrik.XConsole.Demo;

public static class Program
{
    public static async Task Main()
    {
        Console.WriteLine(["Cb`Hello", "R`, ", "Ym`World", "G`!"]);

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

        if (!Console.Extras.Confirm())
            return;

        //
        Console.WriteLine();
        //

        Console.Extras.WindowMaximize();
        //Console.SetCursorPosition(0, 8950);

        var sysFolder = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.System));
        var files = sysFolder.GetFiles("*.exe");
        var fileIndex = 0;
        var flleMaxIndex = Math.Min(100, files.Length) - 1;
        Console.Pin(() => ["m`This is pin!\n", $"g`Number of files: ", $"W`{fileIndex + 1}"]);
        var endPosList = new List<ConsolePosition>();
        var animations = new List<IConsoleAnimation>();
        var random = new Random();

        for (; fileIndex <= flleMaxIndex; fileIndex++)
        {
            await Task.Delay(50);
            var color = Color.FromArgb(random.Next(255), random.Next(255), random.Next(255));
            var bgColor = Color.FromArgb(random.Next(64), random.Next(64), random.Next(64));
            var endPos = Console.WriteLine(files[fileIndex].Name.Color(color).BgColor(bgColor) + ' ').End;
            Console.Extras.TaskbarProgress(fileIndex, flleMaxIndex, TaskbarProgressLevel.Warning);
            endPosList.Add(endPos);

            if ((fileIndex - 2) % 10 == 0)
            {
                var animation1 = endPosList[fileIndex].AnimateSpinner();
                animations.Add(animation1);
            }

            if ((fileIndex - 7) % 10 == 0)
            {
                var animation2 = endPosList[fileIndex].AnimateEllipsis();
                animations.Add(animation2);
            }
        }

        Console.Extras.TaskbarProgressAnimate();

        foreach (var animation in animations)
        {
            await Task.Delay(150);
            animation.Stop().TryWrite("Cb`ok");
        }

        Console.Extras.TaskbarProgressReset();
        Console.Extras.AppHighlight();
    }
}
