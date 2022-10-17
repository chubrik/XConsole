namespace XConsole.Utils;

using System.Threading;

#if NET
using System.Runtime.Versioning;
[SupportedOSPlatform("windows")]
[UnsupportedOSPlatform("android")]
[UnsupportedOSPlatform("browser")]
[UnsupportedOSPlatform("ios")]
[UnsupportedOSPlatform("tvos")]
#endif

public static class ConsolePositionExtensions
{
    public static ProcessAnimation StartProcessAnimation(
        this ConsolePosition position)
    {
        return new ProcessAnimation(position);
    }

    public static ProcessAnimation StartProcessAnimation(
        this ConsolePosition position, CancellationToken cancellationToken)
    {
        return new ProcessAnimation(position, cancellationToken);
    }
}
