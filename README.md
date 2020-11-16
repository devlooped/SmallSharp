![Icon](img/icon-32.png) SmallSharp
============

Create, edit and run multiple C# 9.0 top-level programs in the same project by just selecting the startup one right from the start button!

![Icon](img/launchSettings.png)

[![Version](https://img.shields.io/nuget/v/SmallSharp.svg?color=royalblue)](https://www.nuget.org/packages/SmallSharp)
[![Downloads](https://img.shields.io/nuget/dt/SmallSharp.svg?color=green)](https://www.nuget.org/packages/SmallSharp)
[![License](https://img.shields.io/github/license/kzu/SmallSharp.svg?color=blue)](https://github.com//kzu/SmallSharp/blob/main/LICENSE)
[![Build](https://github.com/kzu/SmallSharp/workflows/build/badge.svg?branch=main)](https://github.com/kzu/SmallSharp/actions)
[![CI Version](https://img.shields.io/endpoint?url=https://shields.kzu.io/vpre/SmallSharp/main&label=nuget.ci&color=brightgreen)](https://pkg.kzu.io/index.json)

The new C# 9 [top-level programs](https://devblogs.microsoft.com/dotnet/welcome-to-c-9-0/#top-level-programs) allow a very intuitive, simple and streamlined experience for quickly spiking or learning C#. 

One missing thing since its introduction in Visual Studio 16.8 is that you can only have one such top-level programs in a project. This means that in order to prototype or learn a different area of .NET, you'd be forced to either replace your previous top-level program or change it to be a non-compile item somehow so you can keep it around (i.e. rename to a `.txt` or change its build action).

**SmallSharp** brings that very feature by automatically generating a launch profile for each `.cs` file at the project level so you can just select which top-level program should be the startup item (for compilation and launch/debug).

This list is automatically kept in sync as you add more `.cs` files to the project. When you select one target C# file, that becomes the only top-level program to be compiled, so you don't have to modify any of the others since they automatically become *None* items.

All compile files directly under the project directory root are considered top-level programs for selection and compilation purposes. If you need to share code among them, you can place them in subdirectories and those will behave like normal compile items.

![Demo](img/SmallSharp.gif)
