namespace Chubrik.XConsole.Utils;

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
internal sealed class EllipsisAnimation : ConsoleAnimation
{
    private readonly int _delay = _random.Next(100, 150);
    protected override string Clear { get; } = "   ";

    public EllipsisAnimation(ConsolePosition position)
        : base(position) { }

    public EllipsisAnimation(ConsolePosition position, CancellationToken cancellationToken)
        : base(position, cancellationToken) { }

    protected override async Task LoopAsync(CancellationToken cancellationToken)
    {
        var delay = _delay;
        var delayX2 = delay * 2;
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
