using Console = System.XConsole;

Console.WriteLine("bC`Hello", "R`, ", "mY`World", "G`!");
Console.WriteLine();

//

Console.WriteLine("d`##########");
var (start, end) = Console.Write("d`##########");
Console.WriteLine(); 
start.Write("Start");
end.Write("End");

//

new XConsolePosition(5, 10).Write("Custom position");
