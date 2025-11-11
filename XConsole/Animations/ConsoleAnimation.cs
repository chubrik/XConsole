namespace Chubrik.XConsole;

using System;
using System.Threading;
using System.Threading.Tasks;

internal abstract class ConsoleAnimation : IConsoleAnimation
{
    protected static readonly Random Random = new();

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
            await LoopAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            if (exception is ArgumentOutOfRangeException || exception is TaskCanceledException)
                Position.TryWrite(Clear);
            else
                throw;
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
        _task.Dispose();
        _cts.Dispose();
    }
}
