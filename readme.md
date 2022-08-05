![Icon](https://raw.githubusercontent.com/devlooped/SmallSharp/main/img/icon-32.png) SmallSharp
============

Create, edit and run multiple C# 9.0 top-level programs in the same project 😍

![Icon](https://raw.githubusercontent.com/devlooped/SmallSharp/main/img/SmallSharpDemo.gif)



[![Version](https://img.shields.io/nuget/v/SmallSharp.svg?color=royalblue)](https://www.nuget.org/packages/SmallSharp) [![Downloads](https://img.shields.io/nuget/dt/SmallSharp?color=darkmagenta)](https://www.nuget.org/packages/SmallSharp) [![License](https://img.shields.io/github/license/devlooped/SmallSharp.svg?color=blue)](https://github.com/devlooped/SmallSharp/blob/main/LICENSE) [![GitHub](https://img.shields.io/badge/-source-181717.svg?logo=GitHub)](https://github.com/devlooped/SmallSharp) [![CI Version](https://img.shields.io/endpoint?label=nuget.ci&color=brightgreen&url=https://shields.kzu.io/vpre/SmallSharp/main)](https://pkg.kzu.io/index.json) [![GH CI Status](https://github.com/devlooped/SmallSharp/workflows/build/badge.svg?branch=main)](https://github.com/devlooped/avatar/actions?query=branch%3Amain+workflow%3Abuild+) 


## Why

The new C# 9 [top-level programs](https://devblogs.microsoft.com/dotnet/welcome-to-c-9-0/#top-level-programs) allow a very intuitive, simple and streamlined experience for quickly spiking or learning C#. 

One missing thing since its introduction in Visual Studio 16.8 is that you can only have one such top-level programs in a project. This means that in order to prototype or learn a different area of .NET, you'd be forced to either replace your previous top-level program or change it to be a non-compile item somehow so you can keep it around (i.e. rename to a `.txt` or change its build action).

**SmallSharp** brings that very feature by automatically generating a launch profile for each `.cs` file at the project level so you can just select which top-level program should be the startup item (for compilation and launch/debug).

This list is automatically kept in sync as you add more `.cs` files to the project. When you select one target C# file, that becomes the only top-level program to be compiled, so you don't have to modify any of the others since they automatically become *None* items.

All compile files directly under the project directory root are considered top-level programs for selection and compilation purposes. If you need to share code among them, you can place them in subdirectories and those will behave like normal compile items.

## Usage

There is no need to install any Visual Studio extension. SmallSharp works by just installing the `SmallSharp` nuget package in a C# 9.0 project (such as a .NET 5.0 project).

1. Create a new Console (.NET Core):


![New Project Dialog](https://raw.githubusercontent.com/devlooped/SmallSharp/main/img/NewConsoleProject.png)

   By default, this new console project may not be set up to target `net5.0` or use the latest C# version. 
   So click on the project node, and the project file will open in the editor. Make sure you either:

  * Target `net5.0` framework:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net5.0</TargetFramework>
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
 
2. Install the **SmallSharp** nuget package using your preferred method:

  * From the Dependencies node, Manage NuGet Packages dialog:

![New Project Dialog](https://raw.githubusercontent.com/devlooped/SmallSharp/main/img/NuGetPackage.png)

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

3. Now open that Program.cs, and replace it all with a nice concise top-level program like:

```csharp
using System;
using static System.Console;

WriteLine("Hello World!");
```

Keep adding as many top-level programs as you need, and switch between them easily by simply changing the active document.

![Demo](https://raw.githubusercontent.com/devlooped/SmallSharp/main/img/TrackSelection.gif)

## How It Works

This nuget package leverages in concert the following standalone and otherwise 
unrelated features of Roslyn, Visual Studio and MSBuild:

1. The C# compiler only allows one top-level program per compilation.
2. Launch profiles (the entries in the Run dropdown) are populated from the Properties\launchSettings.json file
3. Whenever changed, the dropdown selection is persisted as the `$(ActiveDebugProfile)` MSBuild property in a file named after the project with the `.user` extension
4. This file is imported before NuGet-provided MSBuild targets
5. The `$(DefaultItemExcludesInProjectFolder)` MSBuild property allows excluding items at the project-level from the automatically added items by the SDK.

Using the above features in concert, **SmallSharp** essentially does the following:

a. Monitor the active document in VS and emit it as a `launchSettings.json` profile and set it as the `$(ActiveDebugProfile)`.

b. Exclude `.cs` files at the project level from being included as `<Compile>` by the default SDK includes and include them explicitly as `<None>` instead so they show up in the solution explorer. This prevents the compiler from causing an error for multiple top-level programs.

b. Explicitly include as `<Compile>` only the `$(ActiveDebugProfile)` property value.

This basically mean that this it will also work consistently if you use `dotnet run` from the command-line, since the "Main" file selection is performed exclusively via MSBuild item manipulation.

Finally, there is some lovely COM-based magic to access the active Visual Studio IDE (via DTE) to monitor the currently opened source file to keep it in sync with the launch profile. This is done purely using public COM primitives and equally public VSSDK nuget packages their APIs. This enables some useful integration with the IDE without requiring installing a VS extension from the marketplace and deal with gracefully degrading functionality.

<!-- include docs/footer.md -->
# Sponsors 

<!-- sponsors.md -->
<!-- sponsors -->

<a href='https://github.com/KirillOsenkov'>
  <img src='https://github.com/devlooped/sponsors/raw/main/.github/avatars/KirillOsenkov.svg' alt='Kirill Osenkov' title='Kirill Osenkov'>
</a>
<a href='https://github.com/augustoproiete'>
  <img src='https://github.com/devlooped/sponsors/raw/main/.github/avatars/augustoproiete.svg' alt='C. Augusto Proiete' title='C. Augusto Proiete'>
</a>
<a href='https://github.com/sandrock'>
  <img src='https://github.com/devlooped/sponsors/raw/main/.github/avatars/sandrock.svg' alt='SandRock' title='SandRock'>
</a>
<a href='https://github.com/aws'>
  <img src='https://github.com/devlooped/sponsors/raw/main/.github/avatars/aws.svg' alt='Amazon Web Services' title='Amazon Web Services'>
</a>
<a href='https://github.com/MelbourneDeveloper'>
  <img src='https://github.com/devlooped/sponsors/raw/main/.github/avatars/MelbourneDeveloper.svg' alt='Christian Findlay' title='Christian Findlay'>
</a>
<a href='https://github.com/clarius'>
  <img src='https://github.com/devlooped/sponsors/raw/main/.github/avatars/clarius.svg' alt='Clarius Org' title='Clarius Org'>
</a>
<a href='https://github.com/MFB-Technologies-Inc'>
  <img src='https://github.com/devlooped/sponsors/raw/main/.github/avatars/MFB-Technologies-Inc.svg' alt='MFB Technologies, Inc.' title='MFB Technologies, Inc.'>
</a>

<!-- sponsors -->

<!-- sponsors.md -->

<br>&nbsp;
<a href="https://github.com/sponsors/devlooped" title="Sponsor this project">
  <img src="https://github.com/devlooped/sponsors/blob/main/sponsor.png" />
</a>
<br>

[Learn more about GitHub Sponsors](https://github.com/sponsors)

<!-- docs/footer.md -->
