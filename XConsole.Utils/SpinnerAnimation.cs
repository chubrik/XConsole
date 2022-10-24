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
internal sealed class SpinnerAnimation : IConsoleAnimation
{
    private static readonly Random _random = new();

    private readonly CancellationTokenSource _cts;
    private readonly ConsolePosition _position;
    private readonly Task _task;

    public SpinnerAnimation(ConsolePosition position)
        : this(position, new CancellationTokenSource()) { }

    public SpinnerAnimation(ConsolePosition position, CancellationToken cancellationToken)
        : this(position, CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)) { }

    private SpinnerAnimation(ConsolePosition position, CancellationTokenSource cts)
    {
        _cts = cts;
        _position = position;
        _task = StartAsync(_cts.Token);
    }

    private async Task StartAsync(CancellationToken cancellationToken)
    {
        var delay = _random.Next(80, 125);
        var position = _position;

        for (; ; )
        {
            try
            {
                for (; ; )
                {
                    position.Write("/");
                    await Task.Delay(delay, cancellationToken);
                    position.Write("\u2014");
                    await Task.Delay(delay, cancellationToken);
                    position.Write("\\");
                    await Task.Delay(delay, cancellationToken);
                    position.Write("|");
                    await Task.Delay(delay, cancellationToken);
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
        return _position.Write(values);
    }

    public ConsolePosition StopAndWrite(IReadOnlyList<string?> values)
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

    public ConsolePosition? StopAndTryWrite(IReadOnlyList<string?> values)
    {
        Stop();
        return _position.TryWrite(values);
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
