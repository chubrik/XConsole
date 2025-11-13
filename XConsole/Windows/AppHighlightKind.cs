namespace Chubrik.XConsole;

using System.Runtime.Versioning;

/// <summary>
/// Sets what will be highlighted.
/// </summary>
[SupportedOSPlatform("windows")]
public enum AppHighlightKind
{
    /// <summary>
    /// The app window will be highlighted.
    /// </summary>
    Window = 1,

    /// <summary>
    /// The taskbar button will be highlighted.
    /// </summary>
    Taskbar,

    /// <summary>
    /// The app window and taskbar button will be highlighted.
    /// </summary>
    WindowAndTaskbar
}
