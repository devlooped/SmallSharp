![Icon](https://raw.githubusercontent.com/devlooped/SmallSharp/main/assets/img/icon-32.png) SmallSharp
============

[![Version](https://img.shields.io/nuget/v/SmallSharp.svg?color=royalblue)](https://www.nuget.org/packages/SmallSharp) 
[![Downloads](https://img.shields.io/nuget/dt/SmallSharp?color=darkmagenta)](https://www.nuget.org/packages/SmallSharp) 
[![EULA](https://img.shields.io/badge/EULA-OSMF-blue?labelColor=black&color=C9FF30)](https://github.com/devlooped/SmallSharp/blob/main/osmfeula.txt)
[![OSS](https://img.shields.io/github/license/devlooped/SmallSharp.svg?color=blue)](https://github.com/devlooped/SmallSharp/blob/main/license.txt) 

<!-- #description -->
Create, edit and run multiple C# file-based apps in the same project in the IDE, honoring per-file `#:package` 
references, `#:property` project values and even `#:sdk` 😍
<!-- #description -->

<!-- include https://github.com/devlooped/.github/raw/main/osmf.md -->
## Open Source Maintenance Fee

To ensure the long-term sustainability of this project, users of this package who generate 
revenue must pay an [Open Source Maintenance Fee](https://opensourcemaintenancefee.org). 
While the source code is freely available under the terms of the [License](license.txt), 
this package and other aspects of the project require [adherence to the Maintenance Fee](osmfeula.txt).

To pay the Maintenance Fee, [become a Sponsor](https://github.com/sponsors/devlooped) at the proper 
OSMF tier. A single fee covers all of [Devlooped packages](https://www.nuget.org/profiles/Devlooped).

<!-- https://github.com/devlooped/.github/raw/main/osmf.md -->

<!-- #content -->
## Overview

C# [top-level programs](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/program-structure/top-level-statements) 
allow a very intuitive, simple and streamlined experience for quickly spiking or learning C#. 
The addition of [file-based apps](https://devblogs.microsoft.com/dotnet/announcing-dotnet-run-app/) in 
.NET 10 [takes this further](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/preprocessor-directives#file-based-apps) by allowing package references and even MSBuild properties to be 
specified per file:

```csharp
#:package Humanizer@2.14.1
#:property ImplicitUsings=true
#:property Nullable=enable

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
> selected file, unless you're using the SDK mode for SmallSharp (see below).

All compile files directly under the project directory root are considered top-level programs for 
selection and compilation purposes. If you need to share code among them, you can place additional 
files in subdirectories and those will behave like normal compile items.

## Usage

SmallSharp works by just installing the 
[SmallSharp](https://nuget.org/packages/SmallSharp) nuget package in a C# console project 
and adding a couple extra properties to the project file:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>

    <!-- 👇 additional properties required in package mode -->
    <ImportProjectExtensionProps>true</ImportProjectExtensionProps>
    <ImportProjectExtensionTargets>true</ImportProjectExtensionTargets>   
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="SmallSharp" Version="*" PrivateAssets="all" />
  </ItemGroup>

</Project>
```

There are some limitations with this mode, however: 
* You cannot use the `#:sdk` [directive](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/preprocessor-directives#file-based-apps) 
  to specify a different SDK per file, since the project file already specifies one.
* CLI-based builds may require multiple passes to restore and build the selected file, since 
  the package is only restored after the first build.
* You must add ImportProjectExtensionProps/ImportProjectExtensionTargets manually, polluting the 
  project file.

So the recommended way to use SmallSharp is via the SDK mode, which results in a more streamlined 
and seamless experience across IDE and CLI builds:

```xml
<Project Sdk="SmallSharp/2.2.3">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>

</Project>
```

The SDK mode will always produce a successful build in a single `dotnet build` pass even if you 
change the `ActiveFile` between builds.

> [!IMPORTANT]
> If no `#:sdk` directive is provided by a specific C# file-based app, the `Microsoft.NET.SDK` will be 
> used by default in this SDK mode.

Keep adding as many top-level programs as you need, and switch between them easily by simply 
selecting the desired file from the Start button dropdown.

When running from the command-line, you can select the file to run by passing it as an argument to `dotnet run`:

```bash
dotnet run -p:start=program1.cs
```

You can also use the shortcut `-p:s=[FILE]`.

## How It Works

This nuget package leverages in concert the following standalone and otherwise 
unrelated features of the compiler, nuget and MSBuild:

1. The C# compiler only allows one top-level program per compilation.
2. Launch profiles (the entries in the Run dropdown) are populated from the Properties\launchSettings.json file
3. Whenever changed, the dropdown selection is persisted as the `$(ActiveDebugProfile)` MSBuild property in a file 
   named after the project with the `.user` extension
4. This file is imported before NuGet-provided MSBuild targets
5. VS ignores `#:` directives when adding the flag `FileBasedProgram` to the `$(Features)` project property.

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
> `#:property` directives for improved isolation between the different file-based apps.

![run humanizer file](https://raw.githubusercontent.com/devlooped/SmallSharp/main/assets/img/runfile1.png)

![run mcp file](https://raw.githubusercontent.com/devlooped/SmallSharp/main/assets/img/runfile2.png)
<!-- #content -->

<!-- include https://github.com/devlooped/sponsors/raw/main/footer.md -->
# Sponsors 

<!-- sponsors.md -->
[![Clarius Org](https://avatars.githubusercontent.com/u/71888636?v=4&s=39 "Clarius Org")](https://github.com/clarius)
[![MFB Technologies, Inc.](https://avatars.githubusercontent.com/u/87181630?v=4&s=39 "MFB Technologies, Inc.")](https://github.com/MFB-Technologies-Inc)
[![SandRock](https://avatars.githubusercontent.com/u/321868?u=99e50a714276c43ae820632f1da88cb71632ec97&v=4&s=39 "SandRock")](https://github.com/sandrock)
[![DRIVE.NET, Inc.](https://avatars.githubusercontent.com/u/15047123?v=4&s=39 "DRIVE.NET, Inc.")](https://github.com/drivenet)
[![Keith Pickford](https://avatars.githubusercontent.com/u/16598898?u=64416b80caf7092a885f60bb31612270bffc9598&v=4&s=39 "Keith Pickford")](https://github.com/Keflon)
[![Thomas Bolon](https://avatars.githubusercontent.com/u/127185?u=7f50babfc888675e37feb80851a4e9708f573386&v=4&s=39 "Thomas Bolon")](https://github.com/tbolon)
[![Kori Francis](https://avatars.githubusercontent.com/u/67574?u=3991fb983e1c399edf39aebc00a9f9cd425703bd&v=4&s=39 "Kori Francis")](https://github.com/kfrancis)
[![Uno Platform](https://avatars.githubusercontent.com/u/52228309?v=4&s=39 "Uno Platform")](https://github.com/unoplatform)
[![Reuben Swartz](https://avatars.githubusercontent.com/u/724704?u=2076fe336f9f6ad678009f1595cbea434b0c5a41&v=4&s=39 "Reuben Swartz")](https://github.com/rbnswartz)
[![Jacob Foshee](https://avatars.githubusercontent.com/u/480334?v=4&s=39 "Jacob Foshee")](https://github.com/jfoshee)
[![](https://avatars.githubusercontent.com/u/33566379?u=bf62e2b46435a267fa246a64537870fd2449410f&v=4&s=39 "")](https://github.com/Mrxx99)
[![Eric Johnson](https://avatars.githubusercontent.com/u/26369281?u=41b560c2bc493149b32d384b960e0948c78767ab&v=4&s=39 "Eric Johnson")](https://github.com/eajhnsn1)
[![David JENNI](https://avatars.githubusercontent.com/u/3200210?v=4&s=39 "David JENNI")](https://github.com/davidjenni)
[![Jonathan ](https://avatars.githubusercontent.com/u/5510103?u=98dcfbef3f32de629d30f1f418a095bf09e14891&v=4&s=39 "Jonathan ")](https://github.com/Jonathan-Hickey)
[![Charley Wu](https://avatars.githubusercontent.com/u/574719?u=ea7c743490c83e8e4b36af76000f2c71f75d636e&v=4&s=39 "Charley Wu")](https://github.com/akunzai)
[![Ken Bonny](https://avatars.githubusercontent.com/u/6417376?u=569af445b6f387917029ffb5129e9cf9f6f68421&v=4&s=39 "Ken Bonny")](https://github.com/KenBonny)
[![Simon Cropp](https://avatars.githubusercontent.com/u/122666?v=4&s=39 "Simon Cropp")](https://github.com/SimonCropp)
[![agileworks-eu](https://avatars.githubusercontent.com/u/5989304?v=4&s=39 "agileworks-eu")](https://github.com/agileworks-eu)
[![Zheyu Shen](https://avatars.githubusercontent.com/u/4067473?v=4&s=39 "Zheyu Shen")](https://github.com/arsdragonfly)
[![Vezel](https://avatars.githubusercontent.com/u/87844133?v=4&s=39 "Vezel")](https://github.com/vezel-dev)
[![ChilliCream](https://avatars.githubusercontent.com/u/16239022?v=4&s=39 "ChilliCream")](https://github.com/ChilliCream)
[![4OTC](https://avatars.githubusercontent.com/u/68428092?v=4&s=39 "4OTC")](https://github.com/4OTC)
[![Vincent Limo](https://avatars.githubusercontent.com/devlooped-user?s=39 "Vincent Limo")](https://github.com/v-limo)
[![domischell](https://avatars.githubusercontent.com/u/66068846?u=0a5c5e2e7d90f15ea657bc660f175605935c5bea&v=4&s=39 "domischell")](https://github.com/DominicSchell)
[![Justin Wendlandt](https://avatars.githubusercontent.com/u/1068431?u=f7715ed6a8bf926d96ec286f0f1c65f94bf86928&v=4&s=39 "Justin Wendlandt")](https://github.com/jwendl)
[![Adrian Alonso](https://avatars.githubusercontent.com/u/2027083?u=129cf516d99f5cb2fd0f4a0787a069f3446b7522&v=4&s=39 "Adrian Alonso")](https://github.com/adalon)
[![Michael Hagedorn](https://avatars.githubusercontent.com/u/61711586?u=8f653dfcb641e8c18cc5f78692ebc6bb3a0c92be&v=4&s=39 "Michael Hagedorn")](https://github.com/Eule02)
[![torutek](https://avatars.githubusercontent.com/u/33917059?v=4&s=39 "torutek")](https://github.com/torutek)
[![mccaffers](https://avatars.githubusercontent.com/u/16667079?u=739e110e62a75870c981640447efa5eb2cb3bc8f&v=4&s=39 "mccaffers")](https://github.com/mccaffers)
[![Christoph Hochstätter](https://avatars.githubusercontent.com/u/17645550?u=01bbdcb84d03cac26260f1c951e046d24a324591&v=4&s=39 "Christoph Hochstätter")](https://github.com/christoh)
[![ADS Fund](https://avatars.githubusercontent.com/u/202042116?v=4&s=39 "ADS Fund")](https://github.com/ADS-Fund)


<!-- sponsors.md -->
[![Sponsor this project](https://avatars.githubusercontent.com/devlooped-sponsor?s=118 "Sponsor this project")](https://github.com/sponsors/devlooped)

[Learn more about GitHub Sponsors](https://github.com/sponsors)

<!-- https://github.com/devlooped/sponsors/raw/main/footer.md -->
