namespace Chubrik.XConsole.Utils;

using System;
using System.Collections.Generic;
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
    private readonly CancellationTokenSource _cts;
    private readonly Task _task;
    protected static readonly Random _random = new();

    public ConsolePosition Position { get; }

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

    protected abstract Task StartAsync(CancellationToken cancellationToken);

    public void Stop()
    {
        lock (this)
            if (!_cts.IsCancellationRequested)
                _cts.Cancel();

        _task.Wait();
    }

    [Obsolete("At least one argument should be specified.", error: true)]
    public void StopAndWrite() => throw new InvalidOperationException();

    public ConsolePosition StopAndWrite(params string?[] values)
    {
        Stop();
        return Position.Write(values);
    }

    public ConsolePosition StopAndWrite(IReadOnlyList<string?> values)
    {
        Stop();
        return Position.Write(values);
    }

    [Obsolete("At least one argument should be specified.", error: true)]
    public void StopAndTryWrite() => throw new InvalidOperationException();

    public ConsolePosition? StopAndTryWrite(params string?[] values)
    {
        Stop();
        return Position.TryWrite(values);
    }

    public ConsolePosition? StopAndTryWrite(IReadOnlyList<string?> values)
    {
        Stop();
        return Position.TryWrite(values);
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
