namespace Chubrik.XConsole;

using System;
using System.Threading;
using System.Threading.Tasks;

internal sealed class SpinnerAnimation(ConsolePosition position, CancellationToken? cancellationToken)
    : ConsoleAnimation(position, cancellationToken)
{
    protected override string Clear { get; } = " ";

    protected override async Task LoopAsync(CancellationToken cancellationToken)
    {
        var position = Position;
        var delay = TimeSpan.FromMilliseconds(Random.Next(80, 121));
        var frame1 = new[] { new ConsoleItem("/") };
        var frame2 = new[] { new ConsoleItem("\u2014") };
        var frame3 = new[] { new ConsoleItem("\\") };
        var frame4 = new[] { new ConsoleItem("|") };

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
        }
    }
}
