namespace Chubrik.XConsole;

/// <summary>
/// The mode of <see cref="XConsole.ReadLine(ConsoleReadLineMode, char)"/> method.
/// <para>
/// There are three mode variants:
/// <br/>&#8226; <see cref="ConsoleReadLineMode.Default"/> &#8212; default behavior.
/// <br/>&#8226; <see cref="ConsoleReadLineMode.Masked"/> &#8212;
/// all characters are shown as a specified mask character.
/// <br/>&#8226; <see cref="ConsoleReadLineMode.Hidden"/> &#8212; no characters are shown on the screen.
/// </para>
/// </summary>
public enum ConsoleReadLineMode
{
    /// <summary>
    /// Default behavior.
    /// </summary>
    Default,

    /// <summary>
    /// All characters are shown as a specified character.
    /// </summary>
    Masked,

    /// <summary>
    /// No characters are shown on the screen.
    /// </summary>
    Hidden
}
