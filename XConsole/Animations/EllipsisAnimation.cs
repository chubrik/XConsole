namespace Chubrik.XConsole;

using System;
using System.Threading;
using System.Threading.Tasks;

internal sealed class EllipsisAnimation(ConsolePosition position, CancellationToken? cancellationToken)
    : ConsoleAnimation(position, cancellationToken)
{
    protected override string Clear { get; } = "   ";

    protected override async Task LoopAsync(CancellationToken cancellationToken)
    {
        var position = Position;
        var delay = TimeSpan.FromMilliseconds(Random.Next(100, 151));
        var delayX2 = delay + delay;

        ConsoleItem[] frame1;
        ConsoleItem[] frame2;
        ConsoleItem[] frame3;
        ConsoleItem[] frame4;
        ConsoleItem[] frame5;
        ConsoleItem[] frame6 = [new("   ")];

#if NET
        if (VirtualTerminal.IsEnabled)
        {
            frame1 = [new("\x1b[90m.\x1b[39m", type: ConsoleItemType.Ansi)];
            frame2 = [new("\x1b[37m.\x1b[90m.\x1b[39m", type: ConsoleItemType.Ansi)];
            frame3 = [new("\x1b[90m.\x1b[37m.\x1b[90m.\x1b[39m", type: ConsoleItemType.Ansi)];
            frame4 = [new(" \x1b[90m.\x1b[37m.\x1b[39m", type: ConsoleItemType.Ansi)];
            frame5 = [new("  \x1b[90m.\x1b[39m", type: ConsoleItemType.Ansi)];
        }
        else
#endif
        {
            var darkDot = new ConsoleItem(".", ConsoleItemType.ForeColor, foreColor: ConsoleColor.DarkGray);
            var grayDot = new ConsoleItem(".", ConsoleItemType.ForeColor, foreColor: ConsoleColor.Gray);

            frame1 = [darkDot];
            frame2 = [grayDot, darkDot];
            frame3 = [darkDot, grayDot, darkDot];
            frame4 = [new(" .", ConsoleItemType.ForeColor, foreColor: ConsoleColor.DarkGray), grayDot];
            frame5 = [new("  .", ConsoleItemType.ForeColor, foreColor: ConsoleColor.DarkGray)];
        }

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
