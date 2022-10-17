namespace XConsole.Utils;

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

public sealed class ProcessAnimation : IDisposable
{
    private static readonly Random _random = new();

    private readonly CancellationTokenSource _cts;
    private readonly ConsolePosition _position;
    private Task? _task;

    public ProcessAnimation(ConsolePosition position)
        : this(position, new CancellationTokenSource()) { }

    public ProcessAnimation(ConsolePosition position, CancellationToken cancellationToken)
        : this(position, CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)) { }

    private ProcessAnimation(ConsolePosition position, CancellationTokenSource cts)
    {
        _cts = cts;
        _position = position;
        _task = StartAsync();
    }

    private async Task StartAsync()
    {
        var delay = _random.Next(100, 150);
        var delayX2 = delay * 2;
        var position = _position;
        var ct = _cts.Token;

        try
        {
            for (; ; )
            {
                position.TryWrite("d`.");
                await Task.Delay(delay, ct);
                position.TryWrite(".", "d`.");
                await Task.Delay(delay, ct);
                position.TryWrite("d`.", ".", "d`.");
                await Task.Delay(delay, ct);
                position.TryWrite("d` .", ".");
                await Task.Delay(delay, ct);
                position.TryWrite("d`  .");
                await Task.Delay(delay, ct);
                position.TryWrite("   ");
                await Task.Delay(delayX2, ct);
            }
        }
        catch (TaskCanceledException)
        {
        }
        finally
        {
            position.TryWrite("   ");
        }
    }

    public void Stop()
    {
        lock (this)
            if (!_cts.IsCancellationRequested)
            {
                _cts.Cancel();
                _task?.Wait();
                _task = null;
            }
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
