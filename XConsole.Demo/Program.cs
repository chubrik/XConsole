using Console = System.XConsole;

Console.WriteLine("bC`Hello", "R`, ", "mY`World", "G`!");
Console.WriteLine();

//

Console.WriteLine("d`##########");
var (begin, end) = Console.Write("d`##########");
Console.WriteLine();
begin.Write("Begin");
end.Write("End");

//

new XConsolePosition(30, 1).Write("Y`Custom position");

//
Console.WriteLine();
//

Console.Write("W`Continue?", " [y/n]: ");
var yes = Console.ReadKey(true).Key == ConsoleKey.Y;
Console.WriteLine(yes ? "G`Yes" : "R`No");
if (!yes) return;

//
Console.WriteLine();
//

var sysFolder = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.System));
var files = sysFolder.GetFiles("*.exe");
var fileIndex = 0;
Console.Pin(() => new[] { "m`This is pin!\n", $"g`Number of files: ", $"W`{fileIndex}" });
var endPosList = new List<XConsolePosition>();

for (; fileIndex < Math.Min(100, files.Length); fileIndex++)
{
    await Task.Delay(50);
    var endPos = Console.WriteLine(files[fileIndex].Name).End;
    endPosList.Add(endPos);

    if ((fileIndex + 1) % 10 == 0)
        endPosList[fileIndex - 7].Write(" - ", "bC`Ok");
}
