using Chubrik.XConsole.Extras;
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
        Console.WriteLine("Cb`Hello", "R`, ", "Ym`World", "G`!");

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

        Console.Extras.MaximizeWindow();
        //Console.SetCursorPosition(0, 8950);

        var sysFolder = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.System));
        var files = sysFolder.GetFiles("*.exe");
        var fileIndex = 0;
        var flleMaxIndex = Math.Min(100, files.Length) - 1;
        Console.Pin(() => ["m`This is pin!\n", $"g`Number of files: ", $"W`{fileIndex + 1}"]);
        var endPosList = new List<ConsolePosition>();
        var animationList = new List<IConsoleAnimation>();
        var random = new Random();

        for (; fileIndex <= flleMaxIndex; fileIndex++)
        {
            await Task.Delay(50);
            var color = Color.FromArgb(random.Next(255), random.Next(255), random.Next(255));
            var bgColor = Color.FromArgb(random.Next(64), random.Next(64), random.Next(64));
            var endPos = Console.WriteLine(files[fileIndex].Name.Color(color).BgColor(bgColor) + ' ').End;
            Console.Extras.TaskbarProgress(fileIndex, flleMaxIndex, TaskbarProgressLevel.Warning);
            endPosList.Add(endPos);

            if ((fileIndex + 1) % 10 == 0)
            {
                var animation1 = endPosList[fileIndex - 7].AnimateEllipsis();
                var animation2 = endPosList[fileIndex - 2].AnimateSpinner();
                animationList.Add(animation1);
                animationList.Add(animation2);
            }
        }

        Console.Extras.TaskbarProgressAnimate();

        foreach (var animation in animationList)
        {
            await Task.Delay(150);
            animation.Stop().TryWrite("Cb`ok");
        }

        Console.Extras.TaskbarProgressReset();
        Console.Extras.AppHighlight();
    }
}
