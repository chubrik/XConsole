namespace Chubrik.XConsole;

/// <summary>
/// The mode of <see cref="XConsole.ReadLine(ConsoleReadLineMode, char)"/> method.
/// <para>
/// There are three mode variants:
/// <br/>• <see cref="ConsoleReadLineMode.Default"/> – default behavior.
/// <br/>• <see cref="ConsoleReadLineMode.Masked"/> – each typed character is displayed as default or custom mask.
/// <br/>• <see cref="ConsoleReadLineMode.Hidden"/> – typed characters are not displayed.
/// </para>
/// </summary>
public enum ConsoleReadLineMode
{
    /// <summary>
    /// Default behavior.
    /// </summary>
    Default,

    /// <summary>
    /// Each typed character is displayed as default or custom mask.
    /// </summary>
    Masked,

    /// <summary>
    /// Typed characters are not displayed.
    /// </summary>
    Hidden
}
