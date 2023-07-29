namespace Chubrik.XConsole;

using System;
using System.Threading;
using System.Threading.Tasks;

internal abstract class ConsoleAnimation : IConsoleAnimation
{
    protected static readonly Random _random = new();
    private static readonly TimeSpan _restartDelay = TimeSpan.FromSeconds(1);

    private readonly CancellationTokenSource _cts;
    private readonly Task _task;

    public ConsolePosition Position { get; }
    protected abstract string Clear { get; }

    public ConsoleAnimation(ConsolePosition position, CancellationToken? cancellationToken)
    {
        Position = position;

        _cts = cancellationToken != null
            ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken.Value)
            : new CancellationTokenSource();
        
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
                    await Task.Delay(_restartDelay, cancellationToken).ConfigureAwait(false);
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
