# XConsole
[![NuGet package](https://img.shields.io/nuget/v/XConsole)](https://www.nuget.org/packages/XConsole/)
[![MIT license](https://img.shields.io/github/license/chubrik/XConsole)](https://github.com/chubrik/XConsole/blob/main/LICENSE)
[![NuGet downloads](https://img.shields.io/nuget/dt/XConsole)](https://www.nuget.org/packages/XConsole/)

Extended .NET console with coloring microsyntax, multiline pinning, write-to-position and most useful utils.
It’s designed with a focus on performance for professional use in complex tasks:
a huge number of asynchronous logs, with the need to highlight important and pin summary data.
XConsole is safe for multitasking, safe for console buffer overflow (9000+ lines), easy to use.

The main features are shown in the following image:

![XConsole summary](https://raw.githubusercontent.com/chubrik/XConsole/main/img/summary.png)
<br>

## Contents
- [Install](#install)
- [Setup](#setup)
- [Coloring](#coloring)
- [Pinning](#pinning)
- [Positioning](#positioning)
- [ReadLine](#readline)
- [License](#license)
<br><br><br>

## <a name="install"></a>Install
By Package Manager or by adding package reference to \*.csproj file.
More ways see on [NuGet Gallery](https://www.nuget.org/packages/XConsole/).
```
PM> Install-Package XConsole
```
```csproj
<PackageReference Include="XConsole" Version="1.6.*" />
```
<br><br>

## <a name="setup"></a>Setup
Simple way to start using the XConsole is by adding `using` to your code.
After that you can write `Console` as usual and get all the features of XConsole.
Alternatively, you can upgrade an entire project to XConsole at once
by adding the following lines to the \*.csproj file.
```csharp
// Safely upgrade a single code file to XConsole
using Console = Chubrik.XConsole.XConsole;
```
```csproj
<ItemGroup>
  <!-- Safely upgrade an entire project to XConsole -->
  <Using Include="Chubrik.XConsole" />
  <Using Include="Chubrik.XConsole.XConsole" Alias="Console" />
</ItemGroup>
```
This trick is great for upgrading an existing regular console application
because XConsole is backwards compatible with the standard Console.
After the upgrade, the application will not change how it works
unless you want to start using the XConsole features.
<br><br><br><br>

## <a name="coloring"></a>Coloring

### Standard console colors
To colorize the text with standard console colors you need to add a prefix to a string.
The prefix consists of one or two letters denoting colors,
followed by a trailing `` ` `` character, after which the text immediately begins.
Let’s denote the text color letter as *T* and the background color letter as *B*.
In this case, there are three prefix patterns: `` T` `` to change only text color,
`` TB` `` to change text and background colors, ``  B` `` (space at the beginning) to change only background color.
To make a multicolor message split it to parts with individual color prefixes
and pass them to single `WriteLine` method.
Be sure that your messages will not be broken by other threads.
```csharp
Console.WriteLine("G`This line is colored using simple microsyntax");                     // Single color
Console.WriteLine(["C`It is easy ", "Wb`to use many", "R` colors in ", "Y`one message"]); // Multicolor
```
![XConsole single color](https://raw.githubusercontent.com/chubrik/XConsole/main/img/colors-standard.png)

The following table shows all standard console colors and their letter designations:

![XConsole colors table](https://raw.githubusercontent.com/chubrik/XConsole/main/img/colors-table.png)
<br>

### 24-bit colors
If you’re running your application on Windows 10+ with .NET 5+, extended colors are available to you:
```csharp
using Chubrik.XConsole.StringExtensions;

// Different ways to the same 24-bit color
Console.WriteLine("Orange text".Color(Color.Orange)); // Color structure
Console.WriteLine("Orange text".Color(255, 165, 0));  // Red, green & blue
Console.WriteLine("Orange text".Color(0xFFA500));     // Hexadecimal number
Console.WriteLine("Orange text".Color("#FFA500"));    // Hexadecimal string

// Combinations of text and background colors
Console.WriteLine("Orange with an indigo background".Color(Color.Orange).BgColor(Color.Indigo));
Console.WriteLine(("Lime with " + "a brown".BgColor(Color.Brown) + " background").Color(Color.Lime));
Console.WriteLine($"Aqua with {"a navy".BgColor(Color.Navy)} background".Color(Color.Aqua));
```
![XConsole colors extended](https://raw.githubusercontent.com/chubrik/XConsole/main/img/colors-extended.png)

#### NO_COLOR
XConsole supports the [`NO_COLOR`](https://no-color.org/) standard with an environment variable and with a property.
```csharp
Console.NO_COLOR = true; // Disable all colors
```
<br><br>

## <a name="pinning"></a>Pinning
You can pin some text below regular log messages with the `Pin` method.
Pinned text can be static or dynamic, contain one or more lines, and of course can be colored.
The dynamic pin will be automatically updated every time the `Write` or `WriteLine` methods are called.
Pin is resistant to line wrapping and to console buffer overflow (9000+ log lines).

```csharp
Console.Pin("Simple static pin");             // Simple static pin
Console.Pin(["Y`Multicolor", " static pin"]); // Multicolor static pin
Console.Pin("Multiline\nstatic pin");         // Multiline static pin

Console.Pin(() => "Simple pin, value=" + value);                   // Simple dynamic pin
Console.Pin(() => ["Y`Multicolor", " pin, value=", "C`" + value]); // Multicolor dynamic pin
Console.Pin(() => ["Multiline pin,\nvalue=", "C`" + value]);       // Multiline dynamic pin
```
![XConsole pin 1](https://raw.githubusercontent.com/chubrik/XConsole/main/img/pin-1.png)
![XConsole pin 2](https://raw.githubusercontent.com/chubrik/XConsole/main/img/pin-2.png)
![XConsole pin 3](https://raw.githubusercontent.com/chubrik/XConsole/main/img/pin-3.png)

You can also update a dynamic pin manually using the `UpdatePin` method.
To remove a pin, call the `Unpin` method.
```csharp
Console.UpdatePin(); // Update dynamic pin manually
Console.Unpin();     // Remove pin
```
<br><br>

## <a name="positioning"></a>Positioning
XConsole provides a `ConsolePosition` structure, which is a position within the console buffer area.
This is an important feature if you have a lot of log messages.
This structure is resistant to console buffer overflow (9000+ log lines)
and always points to the correct position within the console buffer area.
Each `Write` and `WriteLine` method returns a begin and end position.
You can save them and use later to rewrite the text or add text to the end of the message.
To do this, `ConsolePosition` has its own `Write` method, which returns a new end position.

**NB!** The end position of `WriteLine` means the position
after the last character *before* the line break.
```csharp
var (begin, end) = Console.WriteLine("Hello, World!"); // Write message and save positions

begin.Write("C`HELLO");       // Rewrite part of text
end = end.Write(" I'm here"); // Add text to the end and update end position
end.Write("Y`!");
```
![XConsole positioning](https://raw.githubusercontent.com/chubrik/XConsole/main/img/positioning.png)
<br><br>
You can also get or set the cursor position using the `CursorPosition` property:
```csharp
ConsolePosition position = Console.CursorPosition;              // Get cursor position
Console.CursorPosition = new ConsolePosition(left: 15, top: 5); // Set cursor position
```
<br><br>

## <a name="readline"></a>ReadLine
ReadLine has two additional modes for secure input: masked and hidden. Mask symbol is customizable.
```csharp
Console.ReadLine(ConsoleReadLineMode.Masked);                // Masked ReadLine
Console.ReadLine(ConsoleReadLineMode.Masked, maskChar: '#'); // Masked ReadLine with custom mask
Console.ReadLine(ConsoleReadLineMode.Hidden);                // Hidden ReadLine
```
![XConsole positioning](https://raw.githubusercontent.com/chubrik/XConsole/main/img/readline.png)
<br><br><br>

## <a name="license"></a>License
The XConsole is licensed under the [MIT license](https://github.com/chubrik/XConsole/blob/main/LICENSE).
<br><br>
