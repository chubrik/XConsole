namespace Chubrik.XConsole;

/// <summary>
/// The mode of <see cref="XConsole.ReadLine(ConsoleReadLineMode)"/> method.
/// <para>
/// There are three mode variants:
/// <br/>&#8226; <see cref="ConsoleReadLineMode.Default"/> &#8211; default behavior.
/// <br/>&#8226; <see cref="ConsoleReadLineMode.Masked"/> &#8211; typed characters are displayed as a mask.
/// <br/>&#8226; <see cref="ConsoleReadLineMode.Hidden"/> &#8211; typed characters are not displayed.
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
