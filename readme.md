![Icon](https://raw.githubusercontent.com/devlooped/SmallSharp/main/assets/img/icon-32.png) SmallSharp
============

[![Version](https://img.shields.io/nuget/v/SmallSharp.svg?color=royalblue)](https://www.nuget.org/packages/SmallSharp) 
[![Downloads](https://img.shields.io/nuget/dt/SmallSharp?color=darkmagenta)](https://www.nuget.org/packages/SmallSharp) 
[![License](https://img.shields.io/github/license/devlooped/SmallSharp.svg?color=blue)](https://github.com/devlooped/SmallSharp/blob/main/LICENSE) 
[![build](https://github.com/devlooped/SmallSharp/actions/workflows/build.yml/badge.svg?branch=main)](https://github.com/devlooped/SmallSharp/actions/workflows/build.yml)

<!-- #description -->
Create, edit and run multiple C# top-level programs in the same project, respecting per-file `#:package` 
references and `#:property` project values 😍
<!-- #description -->

## Open Source Maintenance Fee

To ensure the long-term sustainability of this project, use of SmallSharp requires an 
[Open Source Maintenance Fee](https://opensourcemaintenancefee.org). While the source 
code is freely available under the terms of the [MIT License](https://github.com/devlooped/SmallSharp/blob/main/license.txt), all other aspects of the 
project --including opening or commenting on issues, participating in discussions and 
downloading releases-- require [adherence to the Maintenance Fee](https://github.com/devlooped/SmallSharp/blob/main/osmfeula.txt).

In short, if you use this project to generate revenue, the [Maintenance Fee is required](https://github.com/devlooped/SmallSharp/blob/main/osmfeula.txt).

To pay the Maintenance Fee, [become a Sponsor](https://github.com/sponsors/devlooped).

<!-- #content -->
## Overview

C# [top-level programs](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/program-structure/top-level-statements) 
allow a very intuitive, simple and streamlined experience for quickly spiking or learning C#. 
The addition of [dotnet run app.cs](https://devblogs.microsoft.com/dotnet/announcing-dotnet-run-app/) in 
.NET 10 takes this further by allowing package references and even MSBuild properties to be 
specified per file:

```csharp
#:package Humanizer@2.14.1

using Humanizer;

var dotNet9Released = DateTimeOffset.Parse("2024-12-03");
var since = DateTimeOffset.Now - dotNet9Released;

Console.WriteLine($"It has been {since.Humanize()} since .NET 9 was released.");
```

Editing these standalone files in VSCode, however, is suboptimal compared with the full C# 
experience in Visual Studio. In Visual Studio, though, you can only have one top-level program 
in a project, and as of now, you cannot leverage the `#:package` and `#:property` directives 
at all.

**SmallSharp** allows dynamically selecting the file to run, right from the Start button/dropdown 
(for compilation and launch/debug). It also automatically restores the `#:package` references so 
the project system can resolve them, and even emits the `#:property` directives if present to customize 
the build as needed.

![start button](https://raw.githubusercontent.com/devlooped/SmallSharp/main/assets/img/launchSettings.png)

This list is automatically kept in sync as you add more `.cs` files to the project. When you select 
one target C# file, that becomes the only top-level program to be compiled, so you don't have to 
modify any of the others since they automatically become *None* items. 

> [!TIP]
> An initial build after selection change migh be needed to restore the packages and compile the 
> selected file

All compile files directly under the project directory root are considered top-level programs for 
selection and compilation purposes. If you need to share code among them, you can place additional 
files in subdirectories and those will behave like normal compile items.

## Usage

SmallSharp works by just installing the 
[SmallSharp](https://nuget.org/packages/SmallSharp) nuget package in a C# console project.

Recommended installation as an SDK:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <Sdk Name="SmallSharp" Version="2.0.0" />

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>

</Project>
```

Or as a regular package reference:
```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="SmallSharp" Version="*" />
  </ItemGroup>

</Project>
```

Keep adding as many top-level programs as you need, and switch between them easily by simply 
selecting the desired file from the Start button dropdown.

## How It Works

This nuget package leverages in concert the following standalone and otherwise 
unrelated features of the compiler, nuget and MSBuild:

1. The C# compiler only allows one top-level program per compilation.
2. Launch profiles (the entries in the Run dropdown) are populated from the Properties\launchSettings.json file
3. Whenever changed, the dropdown selection is persisted as the `$(ActiveDebugProfile)` MSBuild property in a file 
   named after the project with the `.user` extension
4. This file is imported before NuGet-provided MSBuild targets

Using the above features in concert, **SmallSharp** essentially does the following:

* Emit top-level files as a `launchSettings.json` profile and set the `$(ActiveDebugProfile)`.

* Exclude `.cs` files at the project level from being included as `<Compile>` by the default SDK 
  includes and include them explicitly as `<None>` instead so they show up in the solution explorer. 
  This prevents the compiler from causing an error for multiple top-level programs.

* Explicitly include as `<Compile>` only the `$(ActiveDebugProfile)` property value.

* Emit `#:package` and `#:property` directive to an automatically imported `obj\SmallSharp.targets` file

* SmallSharp MSBuild SDK automatically imports the `SmallSharp.targets` file, which causes a new 
  restore to automatically happen in Visual Studio, bringing all required dependencies automatically.

This basically mean that this it will also work consistently if you use `dotnet run` from the command-line, 
since the "Main" file selection is performed exclusively via MSBuild item manipulation.

> [!TIP]
> It is recommended to keep the project file to its bare minimum, usually having just the SmallSharp 
> SDK reference, and do all project/package references in the top-level files using the `#:package` and 
> `#:property` directives for improved isolation between the top-level programs.

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <Sdk Name="SmallSharp" Version="2.0.0" />

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>

</Project>
```

![run humanizer file](https://raw.githubusercontent.com/devlooped/SmallSharp/main/assets/img/runfile1.png)

![run mcp file](https://raw.githubusercontent.com/devlooped/SmallSharp/main/assets/img/runfile2.png)

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
[![Reuben Swartz](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/rbnswartz.png "Reuben Swartz")](https://github.com/rbnswartz)
[![Jacob Foshee](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/jfoshee.png "Jacob Foshee")](https://github.com/jfoshee)
[![](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/Mrxx99.png "")](https://github.com/Mrxx99)
[![Eric Johnson](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/eajhnsn1.png "Eric Johnson")](https://github.com/eajhnsn1)
[![David JENNI](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/davidjenni.png "David JENNI")](https://github.com/davidjenni)
[![Jonathan ](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/Jonathan-Hickey.png "Jonathan ")](https://github.com/Jonathan-Hickey)
[![Charley Wu](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/akunzai.png "Charley Wu")](https://github.com/akunzai)
[![Ken Bonny](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/KenBonny.png "Ken Bonny")](https://github.com/KenBonny)
[![Simon Cropp](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/SimonCropp.png "Simon Cropp")](https://github.com/SimonCropp)
[![agileworks-eu](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/agileworks-eu.png "agileworks-eu")](https://github.com/agileworks-eu)
[![Zheyu Shen](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/arsdragonfly.png "Zheyu Shen")](https://github.com/arsdragonfly)
[![Vezel](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/vezel-dev.png "Vezel")](https://github.com/vezel-dev)
[![ChilliCream](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/ChilliCream.png "ChilliCream")](https://github.com/ChilliCream)
[![4OTC](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/4OTC.png "4OTC")](https://github.com/4OTC)
[![Vincent Limo](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/v-limo.png "Vincent Limo")](https://github.com/v-limo)
[![Jordan S. Jones](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/jordansjones.png "Jordan S. Jones")](https://github.com/jordansjones)
[![domischell](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/DominicSchell.png "domischell")](https://github.com/DominicSchell)
[![Justin Wendlandt](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/jwendl.png "Justin Wendlandt")](https://github.com/jwendl)
[![Adrian Alonso](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/adalon.png "Adrian Alonso")](https://github.com/adalon)
[![Michael Hagedorn](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/Eule02.png "Michael Hagedorn")](https://github.com/Eule02)
[![torutek](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/torutek.png "torutek")](https://github.com/torutek)


<!-- sponsors.md -->

[![Sponsor this project](https://raw.githubusercontent.com/devlooped/sponsors/main/sponsor.png "Sponsor this project")](https://github.com/sponsors/devlooped)
&nbsp;

[Learn more about GitHub Sponsors](https://github.com/sponsors)

<!-- https://github.com/devlooped/sponsors/raw/main/footer.md -->
