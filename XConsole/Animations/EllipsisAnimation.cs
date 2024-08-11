namespace Chubrik.XConsole;

using System;
using System.Threading;
using System.Threading.Tasks;

internal sealed class EllipsisAnimation(ConsolePosition position, CancellationToken? cancellationToken)
    : ConsoleAnimation(position, cancellationToken)
{
    private readonly TimeSpan _delay = TimeSpan.FromMilliseconds(_random.Next(100, 151));
    protected override string Clear { get; } = "   ";

    protected override async Task LoopAsync(CancellationToken cancellationToken)
    {
        var position = Position;
        var delay = _delay;
        var delayX2 = delay + delay;
        var frame1 = new[] { ConsoleItem.Parse("d`.") };
        var frame2 = new[] { new ConsoleItem("."), ConsoleItem.Parse("d`.") };
        var frame3 = new[] { ConsoleItem.Parse("d`."), new ConsoleItem("."), ConsoleItem.Parse("d`.") };
        var frame4 = new[] { ConsoleItem.Parse("d` ."), new ConsoleItem(".") };
        var frame5 = new[] { ConsoleItem.Parse("d`  .") };
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
