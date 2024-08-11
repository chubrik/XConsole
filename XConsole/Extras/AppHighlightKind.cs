namespace Chubrik.XConsole.Extras;

#if NET
using System.Runtime.Versioning;
#endif

/// <summary>
/// Sets what will be highlighted.
/// </summary>
#if NET
[SupportedOSPlatform("windows")]
#endif
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
