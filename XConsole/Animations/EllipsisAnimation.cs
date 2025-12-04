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
        var frame6 = new ConsoleItem(Clear);

        if (VirtualTerminal.IsEnabled)
        {
            var frame1 = new ConsoleItem("\x1b[90m.\x1b[39m", ConsoleItemType.Ansi);
            var frame2 = new ConsoleItem("\x1b[37m.\x1b[90m.\x1b[39m", ConsoleItemType.Ansi);
            var frame3 = new ConsoleItem("\x1b[90m.\x1b[37m.\x1b[90m.\x1b[39m", ConsoleItemType.Ansi);
            var frame4 = new ConsoleItem(" \x1b[90m.\x1b[37m.\x1b[39m", ConsoleItemType.Ansi);
            var frame5 = new ConsoleItem("  \x1b[90m.\x1b[39m", ConsoleItemType.Ansi);

            for (; ; )
            {
                XConsole.WriteToPosition(position, frame1, [], viewportOnly: true);
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                XConsole.WriteToPosition(position, frame2, [], viewportOnly: true);
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                XConsole.WriteToPosition(position, frame3, [], viewportOnly: true);
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                XConsole.WriteToPosition(position, frame4, [], viewportOnly: true);
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                XConsole.WriteToPosition(position, frame5, [], viewportOnly: true);
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                XConsole.WriteToPosition(position, frame6, [], viewportOnly: true);
                await Task.Delay(delayX2, cancellationToken).ConfigureAwait(false);
            }
        }
        else
        {
            var darkDot = new ConsoleItem(".", ConsoleItemType.ForeColor, foreColor: ConsoleColor.DarkGray);
            var grayDot = new ConsoleItem(".", ConsoleItemType.ForeColor, foreColor: ConsoleColor.Gray);
            var frame1 = darkDot;
            var frame2 = new ConsoleItem[] { grayDot, darkDot };
            var frame3 = new ConsoleItem[] { darkDot, grayDot, darkDot };
            var frame4 =
                new ConsoleItem[] { new(" .", ConsoleItemType.ForeColor, foreColor: ConsoleColor.DarkGray), grayDot };
            var frame5 = new ConsoleItem("  .", ConsoleItemType.ForeColor, foreColor: ConsoleColor.DarkGray);

            for (; ; )
            {
                XConsole.WriteToPosition(position, frame1, [], viewportOnly: true);
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                XConsole.WriteToPosition(position, null, frame2, viewportOnly: true);
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                XConsole.WriteToPosition(position, null, frame3, viewportOnly: true);
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                XConsole.WriteToPosition(position, null, frame4, viewportOnly: true);
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                XConsole.WriteToPosition(position, frame5, [], viewportOnly: true);
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                XConsole.WriteToPosition(position, frame6, [], viewportOnly: true);
                await Task.Delay(delayX2, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
