namespace Chubrik.XConsole.Utils;

using System;
using System.Threading;
using System.Threading.Tasks;

internal sealed class EllipsisAnimation : ConsoleAnimation
{
    public EllipsisAnimation(ConsolePosition position)
        : base(position) { }

    public EllipsisAnimation(ConsolePosition position, CancellationToken cancellationToken)
        : base(position, cancellationToken) { }

    protected override async Task StartAsync(CancellationToken cancellationToken)
    {
        var delay = _random.Next(100, 150);
        var delayX2 = delay * 2;
        var position = Position;

        for (; ; )
        {
            try
            {
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
            catch (TaskCanceledException)
            {
                position.TryWrite("   ");
                return;
            }
            catch (ArgumentOutOfRangeException)
            {
            }
        }
    }
}
