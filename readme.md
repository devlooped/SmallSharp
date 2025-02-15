![Icon](https://raw.githubusercontent.com/devlooped/SmallSharp/main/assets/img/icon-32.png) SmallSharp
============

[![Version](https://img.shields.io/nuget/v/SmallSharp.svg?color=royalblue)](https://www.nuget.org/packages/SmallSharp) [![Downloads](https://img.shields.io/nuget/dt/SmallSharp?color=darkmagenta)](https://www.nuget.org/packages/SmallSharp) [![License](https://img.shields.io/github/license/devlooped/SmallSharp.svg?color=blue)](https://github.com/devlooped/SmallSharp/blob/main/LICENSE) [![GH CI Status](https://github.com/devlooped/SmallSharp/workflows/build/badge.svg?branch=main)](https://github.com/devlooped/avatar/actions?query=branch%3Amain+workflow%3Abuild+) 

![Icon](https://raw.githubusercontent.com/devlooped/SmallSharp/main/assets/img/SmallSharpDemo.gif)

<!-- #content -->
Create, edit and run multiple C# top-level programs in the same project 😍

## Why

The new-ish C# [top-level programs](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/program-structure/top-level-statements) 
allow a very intuitive, simple and streamlined experience for quickly spiking or learning C#. 

One missing thing since their introduction in Visual Studio is that you can only have one such 
top-level program in a project. This means that in order to prototype or learn a different area 
of .NET, you'd be forced to either replace your previous top-level program or change it to be a 
non-compile item somehow so you can keep it around (i.e. rename to a `.txt` or change its build action).

**SmallSharp** allows you to select which file should be the top-level program to run, right from 
the Start button/dropdown (for compilation and launch/debug). Moreover, it will monitor the active 
file you're editing, and automatically make it the startup file for you!

![start button](https://raw.githubusercontent.com/devlooped/SmallSharp/main/assets/img/launchSettings.png)

This list is automatically kept in sync as you add more `.cs` files to the project. When you select 
one target C# file, that becomes the only top-level program to be compiled, so you don't have to 
modify any of the others since they automatically become *None* items.

All compile files directly under the project directory root are considered top-level programs for 
selection and compilation purposes. If you need to share code among them, you can place them in 
subdirectories and those will behave like normal compile items.

## Usage

There is no need to install any Visual Studio extension. SmallSharp works by just installing the 
[SmallSharp](https://nuget.org/packages/SmallSharp) nuget package in a C# console project.

1. Create a new Console project:

![New Project Dialog](https://raw.githubusercontent.com/devlooped/SmallSharp/main/assets/img/NewConsoleProject.png)

   By default, this new console project may not be set up to target `net5.0` or use the latest C# version. 
   So click on the project node, and the project file will open in the editor. Make sure you either:

  * Target the recommended framework version (i.e. net6.0):

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net6.0</TargetFramework>
  </PropertyGroup>

</Project>
```

  * Or use latest C# language version if targeting another framework:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net472</TargetFramework>
    <LangVersion>latest</LangVersion>
  </PropertyGroup>

</Project>
```
 
1. Install the **SmallSharp** nuget package using your preferred method:

  * From the Dependencies node, Manage NuGet Packages dialog:

![New Project Dialog](https://raw.githubusercontent.com/devlooped/SmallSharp/main/assets/img/NuGetPackage.png)

   * By just adding it directly to the .csproj:

```xml
  <ItemGroup>
    <PackageReference Include="SmallSharp" Version="*" />
  </ItemGroup>
```

   * Via the dotnet CLI:

```
> dotnet add package SmallSharp
```

   * Via the Package Manager console:

```
PM> install-package SmallSharp
```

3. Now open that Program.cs and make changes to the new concise top-level program such as:

```csharp
using System;
using static System.Console;

WriteLine("Hello World!");
```

Keep adding as many top-level programs as you need, and switch between them easily by simply 
changing the active document.

![Demo](https://raw.githubusercontent.com/devlooped/SmallSharp/main/assets/img/TrackSelection.gif)


## How It Works

This nuget package leverages in concert the following standalone and otherwise 
unrelated features of the compiler, nuget, Visual Studio and MSBuild:

1. The C# compiler only allows one top-level program per compilation.
2. Launch profiles (the entries in the Run dropdown) are populated from the Properties\launchSettings.json file
3. Whenever changed, the dropdown selection is persisted as the `$(ActiveDebugProfile)` MSBuild property in a file 
   named after the project with the `.user` extension
4. This file is imported before NuGet-provided MSBuild targets
5. The `$(DefaultItemExcludesInProjectFolder)` MSBuild property allows excluding items at the project-level from 
   the automatically added items by the SDK.

Using the above features in concert, **SmallSharp** essentially does the following:

* Monitor the active document in VS and emit it as a `launchSettings.json` profile and set it as 
   the `$(ActiveDebugProfile)`.

* Exclude `.cs` files at the project level from being included as `<Compile>` by the default SDK 
  includes and include them explicitly as `<None>` instead so they show up in the solution explorer. 
  This prevents the compiler from causing an error for multiple top-level programs.

* Explicitly include as `<Compile>` only the `$(ActiveDebugProfile)` property value.

This basically mean that this it will also work consistently if you use `dotnet run` from the command-line, 
since the "Main" file selection is performed exclusively via MSBuild item manipulation.

Finally, there is some lovely COM-based magic to access the active Visual Studio IDE (via DTE) to monitor 
the currently opened source file to keep it in sync with the launch profile. This is done purely using 
public COM primitives and equally public VSSDK nuget packages their APIs. This enables some useful integration 
with the IDE without requiring installing a VS extension from the marketplace and deal with gracefully 
degrading functionality.

> NOTE: If active document tracking fails to initialize properly restarting Visual Studio will almost 
> always fix it. Once tracking starts, it will work consistently from that point on. The Start dropdown 
> is always available as a fallback in case of issues.

<!-- include https://github.com/devlooped/sponsors/raw/main/footer.md -->
# Sponsors 

<!-- sponsors.md -->
[![Clarius Org](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/clarius.png "Clarius Org")](https://github.com/clarius)
[![MFB Technologies, Inc.](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/MFB-Technologies-Inc.png "MFB Technologies, Inc.")](https://github.com/MFB-Technologies-Inc)
[![Torutek](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/torutek-gh.png "Torutek")](https://github.com/torutek-gh)
[![DRIVE.NET, Inc.](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/drivenet.png "DRIVE.NET, Inc.")](https://github.com/drivenet)
[![Keith Pickford](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/Keflon.png "Keith Pickford")](https://github.com/Keflon)
[![Thomas Bolon](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/tbolon.png "Thomas Bolon")](https://github.com/tbolon)
[![Kori Francis](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/kfrancis.png "Kori Francis")](https://github.com/kfrancis)
[![Toni Wenzel](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/twenzel.png "Toni Wenzel")](https://github.com/twenzel)
[![Uno Platform](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/unoplatform.png "Uno Platform")](https://github.com/unoplatform)
[![Dan Siegel](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/dansiegel.png "Dan Siegel")](https://github.com/dansiegel)
[![Reuben Swartz](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/rbnswartz.png "Reuben Swartz")](https://github.com/rbnswartz)
[![Jacob Foshee](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/jfoshee.png "Jacob Foshee")](https://github.com/jfoshee)
[![](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/Mrxx99.png "")](https://github.com/Mrxx99)
[![Eric Johnson](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/eajhnsn1.png "Eric Johnson")](https://github.com/eajhnsn1)
[![Ix Technologies B.V.](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/IxTechnologies.png "Ix Technologies B.V.")](https://github.com/IxTechnologies)
[![David JENNI](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/davidjenni.png "David JENNI")](https://github.com/davidjenni)
[![Jonathan ](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/Jonathan-Hickey.png "Jonathan ")](https://github.com/Jonathan-Hickey)
[![Charley Wu](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/akunzai.png "Charley Wu")](https://github.com/akunzai)
[![Jakob Tikjøb Andersen](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/jakobt.png "Jakob Tikjøb Andersen")](https://github.com/jakobt)
[![Tino Hager](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/tinohager.png "Tino Hager")](https://github.com/tinohager)
[![Ken Bonny](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/KenBonny.png "Ken Bonny")](https://github.com/KenBonny)
[![Simon Cropp](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/SimonCropp.png "Simon Cropp")](https://github.com/SimonCropp)
[![agileworks-eu](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/agileworks-eu.png "agileworks-eu")](https://github.com/agileworks-eu)
[![sorahex](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/sorahex.png "sorahex")](https://github.com/sorahex)
[![Zheyu Shen](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/arsdragonfly.png "Zheyu Shen")](https://github.com/arsdragonfly)
[![Vezel](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/vezel-dev.png "Vezel")](https://github.com/vezel-dev)
[![ChilliCream](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/ChilliCream.png "ChilliCream")](https://github.com/ChilliCream)
[![4OTC](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/4OTC.png "4OTC")](https://github.com/4OTC)
[![Vincent Limo](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/v-limo.png "Vincent Limo")](https://github.com/v-limo)
[![Jordan S. Jones](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/jordansjones.png "Jordan S. Jones")](https://github.com/jordansjones)
[![domischell](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/DominicSchell.png "domischell")](https://github.com/DominicSchell)


<!-- sponsors.md -->

[![Sponsor this project](https://raw.githubusercontent.com/devlooped/sponsors/main/sponsor.png "Sponsor this project")](https://github.com/sponsors/devlooped)
&nbsp;

[Learn more about GitHub Sponsors](https://github.com/sponsors)

<!-- https://github.com/devlooped/sponsors/raw/main/footer.md -->
