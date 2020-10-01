![Icon](img/icon-32.png) SmallSharp
============

Create, edit and run multiple C# 9.0 top-level programs 
in the same project by just selecting the startup program from the start 
button!

![Icon](img/launchSettings.png)

[![Version](https://img.shields.io/nuget/v/SmallSharp.svg?color=royalblue)](https://www.nuget.org/packages/SmallSharp)
[![Downloads](https://img.shields.io/nuget/dt/SmallSharp.svg?color=green)](https://www.nuget.org/packages/SmallSharp)
[![License](https://img.shields.io/github/license/kzu/SmallSharp.svg?color=blue)](https://github.com//kzu/SmallSharp/blob/main/LICENSE)
[![Build](https://github.com/kzu/SmallSharp/workflows/build/badge.svg?branch=main)](https://github.com/kzu/SmallSharp/actions)

The new C# 9 [top-level programs](https://devblogs.microsoft.com/dotnet/welcome-to-c-9-0/#top-level-programs) allow a very intuitive, simple and streamlined experience for quickly spiking or learning C#. 

> As of Visual Studio 16.8 Preview, you need to turn on this feature by adding this property to your project file: 

```xml
<LangVersion>Preview</LangVersion>
```


One missing thing in its current incarnation in Visual Studio 16.8 is that you can only have one such top-level programs in a project. This means that in order to prototype or learn a different area of .NET, you'd be forced to either replace your previous top-level program or change it to be a non-compile item somehow so you can keep it around (i.e. rename to a `.txt`).

**SmallSharp** brings that very feature by automatically generating a launch profile for each `.cs` file at the project level:


This list is automatically kept in sync as you add more `.cs` files to the project. When you select one target C# file, that becomes the only top-level program to be compiled, so you don't have to modify any of the others since they automatically become *None* items.

All compile files directly under the project directory root are considered top-level programs for selection and compilation purposes. If you need to share code among them, you can place them in subdirectories and those will behave like normal compile items.

![Demo](img/SmallSharp.gif)
