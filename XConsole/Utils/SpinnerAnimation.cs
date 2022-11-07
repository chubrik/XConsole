namespace Chubrik.XConsole;

using System;
using System.Threading;
using System.Threading.Tasks;

#if NET
using System.Runtime.Versioning;
[SupportedOSPlatform("windows")]
[UnsupportedOSPlatform("android")]
[UnsupportedOSPlatform("browser")]
[UnsupportedOSPlatform("ios")]
[UnsupportedOSPlatform("tvos")]
#endif
internal sealed class SpinnerAnimation : ConsoleAnimation
{
    private readonly TimeSpan _delay = TimeSpan.FromMilliseconds(_random.Next(80, 125));
    protected override string Clear { get; } = " ";

    public SpinnerAnimation(ConsolePosition position)
        : base(position) { }

    public SpinnerAnimation(ConsolePosition position, CancellationToken cancellationToken)
        : base(position, cancellationToken) { }

    protected override async Task LoopAsync(CancellationToken cancellationToken)
    {
        var delay = _delay;
        var position = Position;

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
}
