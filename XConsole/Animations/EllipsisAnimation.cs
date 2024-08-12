namespace Chubrik.XConsole;

using System;
using System.Threading;
using System.Threading.Tasks;

internal sealed class EllipsisAnimation(ConsolePosition position, CancellationToken? cancellationToken)
    : ConsoleAnimation(position, cancellationToken)
{
    protected override string Clear { get; } = "   ";

    private static readonly ConsoleItem _darkDot = new(".", ConsoleItemType.ForeColor, foreColor: ConsoleColor.DarkGray);
    private static readonly ConsoleItem _grayDot = new(".", ConsoleItemType.ForeColor, foreColor: ConsoleColor.Gray);

    protected override async Task LoopAsync(CancellationToken cancellationToken)
    {
        var position = Position;
        var delay = TimeSpan.FromMilliseconds(Random.Next(100, 151));
        var delayX2 = delay + delay;
        var frame1 = new[] { _darkDot };
        var frame2 = new[] { _grayDot, _darkDot };
        var frame3 = new[] { _darkDot, _grayDot, _darkDot };
        var frame4 = new[] { new ConsoleItem(" .", ConsoleItemType.ForeColor, foreColor: ConsoleColor.DarkGray), _grayDot };
        var frame5 = new[] { new ConsoleItem("  .", ConsoleItemType.ForeColor, foreColor: ConsoleColor.DarkGray) };
        var frame6 = new[] { new ConsoleItem("   ") };

        for (; ; )
        {
            XConsole.WriteToPosition(position, frame1, viewportOnly: true);
            await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            XConsole.WriteToPosition(position, frame2, viewportOnly: true);
            await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            XConsole.WriteToPosition(position, frame3, viewportOnly: true);
            await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            XConsole.WriteToPosition(position, frame4, viewportOnly: true);
            await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            XConsole.WriteToPosition(position, frame5, viewportOnly: true);
            await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            XConsole.WriteToPosition(position, frame6, viewportOnly: true);
            await Task.Delay(delayX2, cancellationToken).ConfigureAwait(false);
        }
    }
}
