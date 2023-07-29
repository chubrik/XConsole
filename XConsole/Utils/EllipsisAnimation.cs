namespace Chubrik.XConsole;

using System;
using System.Threading;
using System.Threading.Tasks;

internal sealed class EllipsisAnimation : ConsoleAnimation
{
    private readonly TimeSpan _delay = TimeSpan.FromMilliseconds(_random.Next(100, 150));
    protected override string Clear { get; } = "   ";

    public EllipsisAnimation(ConsolePosition position, CancellationToken? cancellationToken)
        : base(position, cancellationToken) { }

    protected override async Task LoopAsync(CancellationToken cancellationToken)
    {
        var delay = _delay;
        var delayX2 = delay + delay;
        var position = Position;

        for (; ; )
        {
            position.Write("d`.");
            await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            position.Write(".", "d`.");
            await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            position.Write("d`.", ".", "d`.");
            await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            position.Write("d` .", ".");
            await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            position.Write("d`  .");
            await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            position.Write("   ");
            await Task.Delay(delayX2, cancellationToken).ConfigureAwait(false);
        }
    }
}
