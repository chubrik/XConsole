using System.Diagnostics;

internal class Program
{
    private const int BufferWidth = 100;
    private const int BufferHeight = 100;
    private const int WindowHeight = 30;
    private const int MaxTop = BufferHeight - 1;

    private static void Main()
    {
        Console.SetWindowSize(BufferWidth, WindowHeight);
        Console.SetBufferSize(BufferWidth, BufferHeight);

        Simple();
        PinOneLine();
        PinTwoLines();

        Reset();
        Console.WriteLine("G`All tests passed");
    }

    private static void Simple()
    {
        Reset();
        var line = 0;

        for (; line < MaxTop - 2; line++)
            Console.WriteLine($"Line {line}");

        var (begin1, _) = Console.WriteLine($"Line {line++}");
        Debug.Assert(begin1.InitialTop == MaxTop - 2);
        Debug.Assert(begin1.ActualTop == MaxTop - 2);
        Debug.Assert(begin1.ShiftTop == 0);

        var (begin2, _) = Console.WriteLine($"Line {line++}");
        Debug.Assert(begin1.ActualTop == MaxTop - 2);
        Debug.Assert(begin2.InitialTop == MaxTop - 1);
        Debug.Assert(begin2.ActualTop == MaxTop - 1);
        Debug.Assert(begin2.ShiftTop == 0);

        var (begin3, _) = Console.WriteLine($"Line {line++}");
        Debug.Assert(begin1.ActualTop == MaxTop - 3);
        Debug.Assert(begin2.ActualTop == MaxTop - 2);
        Debug.Assert(begin3.InitialTop == MaxTop - 1);
        Debug.Assert(begin3.ActualTop == MaxTop - 1);
        Debug.Assert(begin3.ShiftTop == 1);

        var (begin4, _) = Console.WriteLine($"Line {line++}");
        Debug.Assert(begin1.ActualTop == MaxTop - 4);
        Debug.Assert(begin2.ActualTop == MaxTop - 3);
        Debug.Assert(begin3.ActualTop == MaxTop - 2);
        Debug.Assert(begin4.InitialTop == MaxTop - 1);
        Debug.Assert(begin4.ActualTop == MaxTop - 1);
        Debug.Assert(begin4.ShiftTop == 2);
    }

    private static void PinOneLine()
    {
        Reset();
        Console.Pin("Line Pinned");
        var line = 0;

        for (; line < MaxTop - 3; line++)
            Console.WriteLine($"Line {line}");

        var (begin1, _) = Console.WriteLine($"Line {line++}");
        Debug.Assert(begin1.InitialTop == MaxTop - 3);
        Debug.Assert(begin1.ActualTop == MaxTop - 3);
        Debug.Assert(begin1.ShiftTop == 0);

        var (begin2, _) = Console.WriteLine($"Line {line++}");
        Debug.Assert(begin1.ActualTop == MaxTop - 3);
        Debug.Assert(begin2.InitialTop == MaxTop - 2);
        Debug.Assert(begin2.ActualTop == MaxTop - 2);
        Debug.Assert(begin2.ShiftTop == 0);

        var (begin3, _) = Console.WriteLine($"Line {line++}");
        Debug.Assert(begin1.ActualTop == MaxTop - 4);
        Debug.Assert(begin2.ActualTop == MaxTop - 3);
        Debug.Assert(begin3.InitialTop == MaxTop - 2);
        Debug.Assert(begin3.ActualTop == MaxTop - 2);
        Debug.Assert(begin3.ShiftTop == 1);

        var (begin4, _) = Console.WriteLine($"Line {line++}");
        Debug.Assert(begin1.ActualTop == MaxTop - 5);
        Debug.Assert(begin2.ActualTop == MaxTop - 4);
        Debug.Assert(begin3.ActualTop == MaxTop - 3);
        Debug.Assert(begin4.InitialTop == MaxTop - 2);
        Debug.Assert(begin4.ActualTop == MaxTop - 2);
        Debug.Assert(begin4.ShiftTop == 2);
    }

    private static void PinTwoLines()
    {
        Reset();
        Console.Pin("Line Pinned 1\nLine Pinned 2");
        var line = 0;

        for (; line < MaxTop - 4; line++)
            Console.WriteLine($"Line {line}");

        var (begin1, _) = Console.WriteLine($"Line {line++}");
        Debug.Assert(begin1.InitialTop == MaxTop - 4);
        Debug.Assert(begin1.ActualTop == MaxTop - 4);
        Debug.Assert(begin1.ShiftTop == 0);

        var (begin2, _) = Console.WriteLine($"Line {line++}");
        Debug.Assert(begin1.ActualTop == MaxTop - 4);
        Debug.Assert(begin2.InitialTop == MaxTop - 3);
        Debug.Assert(begin2.ActualTop == MaxTop - 3);
        Debug.Assert(begin2.ShiftTop == 0);

        var (begin3, _) = Console.WriteLine($"Line {line++}");
        Debug.Assert(begin1.ActualTop == MaxTop - 5);
        Debug.Assert(begin2.ActualTop == MaxTop - 4);
        Debug.Assert(begin3.InitialTop == MaxTop - 3);
        Debug.Assert(begin3.ActualTop == MaxTop - 3);
        Debug.Assert(begin3.ShiftTop == 1);

        var (begin4, _) = Console.WriteLine($"Line {line++}");
        Debug.Assert(begin1.ActualTop == MaxTop - 6);
        Debug.Assert(begin2.ActualTop == MaxTop - 5);
        Debug.Assert(begin3.ActualTop == MaxTop - 4);
        Debug.Assert(begin4.InitialTop == MaxTop - 3);
        Debug.Assert(begin4.ActualTop == MaxTop - 3);
        Debug.Assert(begin4.ShiftTop == 2);
    }

    private static void Reset()
    {
        Console.ResetColor();
        Console.Unpin();
        Console.Clear();
    }
}
