namespace Chubrik.XConsole;

/// <summary>
/// The mode of <see cref="XConsole.ReadLine(ConsoleReadLineMode)"/> method.
/// <para>
/// There are three mode variants:
/// <br/>• <see cref="ConsoleReadLineMode.Default"/> – default behavior.
/// <br/>• <see cref="ConsoleReadLineMode.Masked"/> – typed characters are displayed as a mask.
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
    /// Typed characters are displayed as a mask.
    /// </summary>
    Masked,

    /// <summary>
    /// Typed characters are not displayed.
    /// </summary>
    Hidden
}
