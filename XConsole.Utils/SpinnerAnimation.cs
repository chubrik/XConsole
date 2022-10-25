namespace Chubrik.XConsole.Utils;

using System;
using System.Threading;
using System.Threading.Tasks;

internal sealed class SpinnerAnimation : ConsoleAnimation
{
    public SpinnerAnimation(ConsolePosition position)
        : base(position) { }

    public SpinnerAnimation(ConsolePosition position, CancellationToken cancellationToken)
        : base(position, cancellationToken) { }

    protected override async Task StartAsync(CancellationToken cancellationToken)
    {
        var delay = _random.Next(80, 125);
        var position = Position;

        for (; ; )
        {
            try
            {
                for (; ; )
                {
                    position.Write("/");
                    await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                    position.Write("\u2014");
                    await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                    position.Write("\\");
                    await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                    position.Write("|");
                    await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (TaskCanceledException)
            {
                position.TryWrite(" ");
                return;
            }
            catch (ArgumentOutOfRangeException)
            {
            }
        }
    }
}
