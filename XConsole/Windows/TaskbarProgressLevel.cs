namespace Chubrik.XConsole;

using System.Runtime.Versioning;

/// <summary>
/// The appearance type of the taskbar button when in progress.
/// </summary>
[SupportedOSPlatform("windows")]
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
