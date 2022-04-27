# XConsole
[![NuGet package](https://img.shields.io/nuget/v/XConsole)](https://www.nuget.org/packages/XConsole/)
[![MIT license](https://img.shields.io/github/license/chubrik/XConsole)](https://github.com/chubrik/XConsole/blob/main/LICENSE)

Extended .NET console with coloring microsyntax, multiline pinning, write-to-position, etc.
Safe for multitasking, safe for 9000+ lines, easy to use.

The main features are shown in the following image:

![XConsole summary](https://raw.githubusercontent.com/chubrik/XConsole/main/img/summary.png)
<br><br>

- [Install](#install)
- [Setup](#setup)
- [Coloring](#coloring)
- [Pinning](#pinning)
- [Positioning](#positioning)
- [License](#license)
<br>

## <a name="install"></a>Install
By Package Manager:
```
PM> Install-Package XConsole
```
Or by adding to \*.csproj file:
```
<PackageReference Include="XConsole" Version="1.1.*" />
```
More ways to install see on [NuGet Gallery](https://www.nuget.org/packages/XConsole/).
<br><br><br>

## <a name="setup"></a>Setup
Simple way to start using the XConsole is by adding `using` to your code.
After that you can write `Console` as usual and get all the features of XConsole:
```csharp
// Safely upgrade a single code file to XConsole
using Console = Chubrik.XConsole.XConsole;
```
Alternatively, you can upgrade an entire project to XConsole at once
by adding the following lines to the \*.csproj file:
```csproj
<ItemGroup>
  <!-- Safely upgrade an entire project to XConsole -->
  <Using Include="Chubrik.XConsole.XConsole" Alias="Console" />
</ItemGroup>
```
This trick is great for upgrading an existing regular console application
because XConsole is backwards compatible with the standard Console.
After the upgrade, the application will not change how it works
unless you want to start using the XConsole features.
<br><br><br>

## <a name="coloring"></a>Coloring
To colorize the text you need to add a prefix to a string.
The prefix consists of one or two letters denoting colors,
followed by a trailing `` ` `` character, after which the text immediately begins.

Let’s denote the text color letter as `X` and the background color letter as `Z`.
In this case, there are three prefix patterns: `` X` `` to change text color,
`` Z ` `` to change background color, `` ZX` `` to change both colors.

For example, to colorize text to green just add `` G` `` prefix (*G* means green):
```csharp
Console.WriteLine("G`This log message is colorized using microsyntax");
```
![XConsole single color](https://raw.githubusercontent.com/chubrik/XConsole/main/img/colors-single.png)

To make a multicolor message split it to parts with individual color prefixes
and pass them to single `WriteLine` method.
Be sure that your messages will not be broken by other threads.
```csharp
Console.WriteLine("C`It is easy ", "bW`to use many", "R` colors in ", "Y`one message");
```
![XConsole multicolor](https://raw.githubusercontent.com/chubrik/XConsole/main/img/colors-multi.png)

The following table shows all possible colors and their letter designations:

![XConsole color table](https://raw.githubusercontent.com/chubrik/XConsole/main/img/colors-table.png)
<br>

### NO_COLOR
XConsole supports the [`NO_COLOR`](https://no-color.org/) standard with an appropriate environment variable and property.
<br><br><br>

## <a name="pinning"></a>Pinning
You can pin some text below regular log messages with the `Pin` method.
Pinned text can be static or dynamic, contain one or more lines, and of course can be colorized.
Pin is resistant to line wrapping and to console buffer overflows (9000+ log lines).

### Static pin
```csharp
Console.Pin("Simple static pin");           // Simple static pin
Console.Pin("Y`Multicolor", " static pin"); // Multicolor static pin
Console.Pin("Multiline\nstatic pin");       // Multiline static pin
```
![XConsole pin 1](https://raw.githubusercontent.com/chubrik/XConsole/main/img/pin-1.png)
![XConsole pin 2](https://raw.githubusercontent.com/chubrik/XConsole/main/img/pin-2.png)
![XConsole pin 3](https://raw.githubusercontent.com/chubrik/XConsole/main/img/pin-3.png)

### Dynamic pin
```csharp
Console.Pin(() => "Simple pin, value=" + value);                           // Simple dynamic pin
Console.Pin(() => new[] { "Y`Multicolor", " pin, value=", "C`" + value }); // Multicolor dynamic pin
Console.Pin(() => new[] { "Multiline pin,\nvalue=", "C`" + value });       // Multiline dynamic pin
```
![XConsole pin 4](https://raw.githubusercontent.com/chubrik/XConsole/main/img/pin-4.png)
![XConsole pin 5](https://raw.githubusercontent.com/chubrik/XConsole/main/img/pin-5.png)
![XConsole pin 6](https://raw.githubusercontent.com/chubrik/XConsole/main/img/pin-6.png)

The dynamic pin will be automatically updated every time the `Write` or `WriteLine` methods are called.
You can also update a dynamic pin manually using the `UpdatePin` method:
```csharp
Console.UpdatePin(); // Update dynamic pin manually
```
To remove a pin, call the `Unpin` method:
```csharp
Console.Unpin(); // Remove pin
```
<br>

## <a name="positioning"></a>Positioning
XConsole provides an `XConsolePosition` structure, which is a position in the console area.
This is an important feature if you have a lot of log messages.
This structure is resistant to console buffer overflows (9000+ log lines)
and always points to the correct position in the console area.

Each `Write` and `WriteLine` method returns a start and end position.
You can save them and use later to rewrite the text or add text to the end of the message.
To do this, `XConsolePosition` has its own `Write` method, which returns a new end position.

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
You can also get or set `XConsolePosition` of the cursor using the `CursorPosition` property:
```csharp
XConsolePosition position = Console.CursorPosition;              // Get cursor position
Console.CursorPosition = new XConsolePosition(left: 15, top: 5); // Set cursor position
```
<br>

## <a name="license"></a>License
The XConsole is licensed under the [MIT license](https://github.com/chubrik/XConsole/blob/main/LICENSE).
<br><br>
