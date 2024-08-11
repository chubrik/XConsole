namespace Chubrik.XConsole;

#if NET
using System.Runtime.Versioning;
#endif

/// <summary>
/// The appearance type of the taskbar button when in progress.
/// </summary>
#if NET
[SupportedOSPlatform("windows")]
#endif
public enum TaskbarProgressLevel
{
    /// <summary>
    /// Default appearance of the taskbar button. Usually green.
    /// </summary>
    Default,

    /// <summary>
    /// Warning appearance of the taskbar button. Usually yellow.
    /// </summary>
    Warning,

    /// <summary>
    /// Error appearance of the taskbar button. Usually red.
    /// </summary>
    Error
}
