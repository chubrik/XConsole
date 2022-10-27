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
public abstract class ConsoleAnimation : IDisposable
{
    protected static readonly Random _random = new();

    private readonly CancellationTokenSource _cts;
    private readonly Task _task;

    public ConsolePosition Position { get; }
    protected abstract string Clear { get; }

    public ConsoleAnimation(ConsolePosition position)
        : this(position, new CancellationTokenSource()) { }

    public ConsoleAnimation(ConsolePosition position, CancellationToken cancellationToken)
        : this(position, CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)) { }

    private ConsoleAnimation(ConsolePosition position, CancellationTokenSource cts)
    {
        Position = position;
        _cts = cts;
        _task = StartAsync(_cts.Token);
    }

    private async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            for (; ; )
                try
                {
                    await LoopAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (ArgumentOutOfRangeException)
                {
                    await Task.Delay(1000, cancellationToken).ConfigureAwait(false);
                }
        }
        catch (TaskCanceledException)
        {
            Position.TryWrite(Clear);
        }
    }

    protected abstract Task LoopAsync(CancellationToken cancellationToken);

    public ConsolePosition Stop()
    {
        lock (this)
            if (!_cts.IsCancellationRequested)
                _cts.Cancel();

        _task.Wait();
        return Position;
    }

    public void Dispose()
    {
        Stop();
#if !NETSTANDARD1_3
        _task.Dispose();
#endif
        _cts.Dispose();
    }
}
