namespace Chubrik.XConsole.Utils;

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
internal sealed class EllipsisAnimation : IConsoleAnimation
{
    private static readonly Random _random = new();

    private readonly ConsolePosition _position;
    private readonly CancellationTokenSource _cts;

    public EllipsisAnimation(ConsolePosition position)
        : this(position, new CancellationTokenSource()) { }

    public EllipsisAnimation(ConsolePosition position, CancellationToken cancellationToken)
        : this(position, CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)) { }

    private EllipsisAnimation(ConsolePosition position, CancellationTokenSource cts)
    {
        _position = position;
        _cts = cts;
        _ = StartAsync();
    }

    private async Task StartAsync()
    {
        var delay = _random.Next(100, 150);
        var delayX2 = delay * 2;
        var position = _position;
        var ct = _cts.Token;

        for (; ; )
        {
            try
            {
                for (; ; )
                {
                    position.Write("d`.");
                    await Task.Delay(delay, ct);
                    position.Write(".", "d`.");
                    await Task.Delay(delay, ct);
                    position.Write("d`.", ".", "d`.");
                    await Task.Delay(delay, ct);
                    position.Write("d` .", ".");
                    await Task.Delay(delay, ct);
                    position.Write("d`  .");
                    await Task.Delay(delay, ct);
                    position.Write("   ");
                    await Task.Delay(delayX2, ct);
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

    public void Stop()
    {
        lock (this)
            if (!_cts.IsCancellationRequested)
                _cts.Cancel();
    }

    [Obsolete("At least one argument should be specified.", error: true)]
    public void StopAndWrite() => throw new InvalidOperationException();

    public ConsolePosition StopAndWrite(params string?[] values)
    {
        Stop();
        return _position.Write(values);
    }

    [Obsolete("At least one argument should be specified.", error: true)]
    public void StopAndTryWrite() => throw new InvalidOperationException();

    public ConsolePosition? StopAndTryWrite(params string?[] values)
    {
        Stop();
        return _position.TryWrite(values);
    }

    public void Dispose() => Stop();
}
