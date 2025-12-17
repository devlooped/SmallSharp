# Changelog

## [v2.3.0](https://github.com/devlooped/SmallSharp/tree/v2.3.0) (2025-12-17)

[Full Changelog](https://github.com/devlooped/SmallSharp/compare/v2.2.5...v2.3.0)

:sparkles: Implemented enhancements:

- Ensure properties are always set in props+targets [\#171](https://github.com/devlooped/SmallSharp/pull/171) (@kzu)

## [v2.2.5](https://github.com/devlooped/SmallSharp/tree/v2.2.5) (2025-10-16)

[Full Changelog](https://github.com/devlooped/SmallSharp/compare/v2.2.4...v2.2.5)

## [v2.2.4](https://github.com/devlooped/SmallSharp/tree/v2.2.4) (2025-09-24)

[Full Changelog](https://github.com/devlooped/SmallSharp/compare/v2.2.3...v2.2.4)

:sparkles: Implemented enhancements:

- Ensure when running from VS, output supports UTF-8 [\#161](https://github.com/devlooped/SmallSharp/pull/161) (@kzu)

## [v2.2.3](https://github.com/devlooped/SmallSharp/tree/v2.2.3) (2025-09-24)

[Full Changelog](https://github.com/devlooped/SmallSharp/compare/v2.2.2...v2.2.3)

:sparkles: Implemented enhancements:

- Switch to $\(Start\) to select startup file [\#160](https://github.com/devlooped/SmallSharp/pull/160) (@kzu)

:bug: Fixed bugs:

- Don't warn during restore about duplicate package refs [\#159](https://github.com/devlooped/SmallSharp/pull/159) (@kzu)

:twisted_rightwards_arrows: Merged:

- Upgrade to latest SLNX format [\#158](https://github.com/devlooped/SmallSharp/pull/158) (@kzu)

## [v2.2.2](https://github.com/devlooped/SmallSharp/tree/v2.2.2) (2025-09-24)

[Full Changelog](https://github.com/devlooped/SmallSharp/compare/v2.2.1...v2.2.2)

:twisted_rightwards_arrows: Merged:

- Emit package references in SDK mode too [\#157](https://github.com/devlooped/SmallSharp/pull/157) (@kzu)

## [v2.2.1](https://github.com/devlooped/SmallSharp/tree/v2.2.1) (2025-09-10)

[Full Changelog](https://github.com/devlooped/SmallSharp/compare/v2.2.0...v2.2.1)

:bug: Fixed bugs:

- Fix error when file has no package references [\#153](https://github.com/devlooped/SmallSharp/pull/153) (@kzu)

:twisted_rightwards_arrows: Merged:

- Improve directive matching by using named groups [\#154](https://github.com/devlooped/SmallSharp/pull/154) (@kzu)

## [v2.2.0](https://github.com/devlooped/SmallSharp/tree/v2.2.0) (2025-09-10)

[Full Changelog](https://github.com/devlooped/SmallSharp/compare/v2.1.0...v2.2.0)

:sparkles: Implemented enhancements:

- Align \#:property Name=Value with RC syntax [\#152](https://github.com/devlooped/SmallSharp/pull/152) (@kzu)

:bug: Fixed bugs:

- We only ever support C\#, so use .cs extension [\#151](https://github.com/devlooped/SmallSharp/pull/151) (@kzu)

## [v2.1.0](https://github.com/devlooped/SmallSharp/tree/v2.1.0) (2025-09-04)

[Full Changelog](https://github.com/devlooped/SmallSharp/compare/v2.1.0-beta...v2.1.0)

## [v2.1.0-beta](https://github.com/devlooped/SmallSharp/tree/v2.1.0-beta) (2025-09-04)

[Full Changelog](https://github.com/devlooped/SmallSharp/compare/v2.1.0-alpha...v2.1.0-beta)

:sparkles: Implemented enhancements:

- Improve automatic restore support in SDK mode [\#147](https://github.com/devlooped/SmallSharp/pull/147) (@kzu)

:twisted_rightwards_arrows: Merged:

- Demo can now rely on plain build working [\#145](https://github.com/devlooped/SmallSharp/pull/145) (@kzu)

## [v2.1.0-alpha](https://github.com/devlooped/SmallSharp/tree/v2.1.0-alpha) (2025-09-01)

[Full Changelog](https://github.com/devlooped/SmallSharp/compare/v2.0.0...v2.1.0-alpha)

:sparkles: Implemented enhancements:

- Update package references instead of emitting duplicates [\#140](https://github.com/devlooped/SmallSharp/issues/140)
- SmallSharp generated targets are not imported from dotnet build/run [\#139](https://github.com/devlooped/SmallSharp/issues/139)
- Add support for \#:sdk directive and no restore failures [\#144](https://github.com/devlooped/SmallSharp/pull/144) (@kzu)
- Ensure dotnet build works too [\#141](https://github.com/devlooped/SmallSharp/pull/141) (@kzu)

:hammer: Other:

- Add support for \#:sdk directive [\#143](https://github.com/devlooped/SmallSharp/issues/143)

## [v2.0.0](https://github.com/devlooped/SmallSharp/tree/v2.0.0) (2025-07-22)

[Full Changelog](https://github.com/devlooped/SmallSharp/compare/v2.0.0-rc...v2.0.0)

## [v2.0.0-rc](https://github.com/devlooped/SmallSharp/tree/v2.0.0-rc) (2025-07-22)

[Full Changelog](https://github.com/devlooped/SmallSharp/compare/v2.0.0-beta...v2.0.0-rc)

## [v2.0.0-beta](https://github.com/devlooped/SmallSharp/tree/v2.0.0-beta) (2025-07-22)

[Full Changelog](https://github.com/devlooped/SmallSharp/compare/v1.3.0...v2.0.0-beta)

:sparkles: Implemented enhancements:

- Fix issue with properties containing spaces [\#130](https://github.com/devlooped/SmallSharp/pull/130) (@kzu)
- Add support for \#:property directive [\#129](https://github.com/devlooped/SmallSharp/pull/129) (@kzu)
- Clean launchsettings.json if explicitly cleaned [\#128](https://github.com/devlooped/SmallSharp/pull/128) (@kzu)
- Add run file \#:package support [\#125](https://github.com/devlooped/SmallSharp/pull/125) (@kzu)

## [v1.3.0](https://github.com/devlooped/SmallSharp/tree/v1.3.0) (2025-07-19)

[Full Changelog](https://github.com/devlooped/SmallSharp/compare/v1.2.0...v1.3.0)

:sparkles: Implemented enhancements:

- Remove brittle active document tracking [\#122](https://github.com/devlooped/SmallSharp/pull/122) (@kzu)

## [v1.2.0](https://github.com/devlooped/SmallSharp/tree/v1.2.0) (2024-07-17)

[Full Changelog](https://github.com/devlooped/SmallSharp/compare/v1.1.7...v1.2.0)

:sparkles: Implemented enhancements:

- Make sure startup file is an up-to-date check input [\#115](https://github.com/devlooped/SmallSharp/pull/115) (@kzu)
- Make sure file names are sorted alphabetically [\#112](https://github.com/devlooped/SmallSharp/pull/112) (@kzu)

## [v1.1.7](https://github.com/devlooped/SmallSharp/tree/v1.1.7) (2022-11-16)

[Full Changelog](https://github.com/devlooped/SmallSharp/compare/v1.1.6...v1.1.7)

:sparkles: Implemented enhancements:

- Make SmallSharp a development dependency [\#95](https://github.com/devlooped/SmallSharp/issues/95)

## [v1.1.6](https://github.com/devlooped/SmallSharp/tree/v1.1.6) (2022-11-16)

[Full Changelog](https://github.com/devlooped/SmallSharp/compare/v1.1.5...v1.1.6)

:twisted_rightwards_arrows: Merged:

- +M▼ includes [\#79](https://github.com/devlooped/SmallSharp/pull/79) (@github-actions[bot])
- +M▼ includes [\#76](https://github.com/devlooped/SmallSharp/pull/76) (@github-actions[bot])

## [v1.1.5](https://github.com/devlooped/SmallSharp/tree/v1.1.5) (2022-02-03)

[Full Changelog](https://github.com/devlooped/SmallSharp/compare/v1.1.4...v1.1.5)

:sparkles: Implemented enhancements:

- Simplify editing of launchSettings.json by using Devlooped.JsonPoke [\#64](https://github.com/devlooped/SmallSharp/issues/64)

## [v1.1.4](https://github.com/devlooped/SmallSharp/tree/v1.1.4) (2021-07-05)

[Full Changelog](https://github.com/devlooped/SmallSharp/compare/v1.1.2...v1.1.4)

:hammer: Other:

- Include package readme for better discoverability in gallery [\#49](https://github.com/devlooped/SmallSharp/issues/49)

## [v1.1.2](https://github.com/devlooped/SmallSharp/tree/v1.1.2) (2021-05-17)

[Full Changelog](https://github.com/devlooped/SmallSharp/compare/v1.1.1...v1.1.2)

:sparkles: Implemented enhancements:

- When trying to attach to the IDE ActiveDocument, increase wait between retries [\#41](https://github.com/devlooped/SmallSharp/issues/41)
- Populate launchSettings with candidate top-level compile files [\#40](https://github.com/devlooped/SmallSharp/issues/40)

## [v1.1.1](https://github.com/devlooped/SmallSharp/tree/v1.1.1) (2021-04-08)

[Full Changelog](https://github.com/devlooped/SmallSharp/compare/v1.1.0...v1.1.1)

:bug: Fixed bugs:

- Failure with VS 16.10 preview [\#36](https://github.com/devlooped/SmallSharp/issues/36)

:hammer: Other:

- Ensure Properties folder exists before updating the launchSettings.json [\#33](https://github.com/devlooped/SmallSharp/issues/33)

:twisted_rightwards_arrows: Merged:

- Ensure the directory for launchSettings.json exists [\#34](https://github.com/devlooped/SmallSharp/pull/34) (@kzu)

## [v1.1.0](https://github.com/devlooped/SmallSharp/tree/v1.1.0) (2021-02-15)

[Full Changelog](https://github.com/devlooped/SmallSharp/compare/v1.0.3...v1.1.0)

:sparkles: Implemented enhancements:

- Adding new C\# file should result in a new startup file [\#9](https://github.com/devlooped/SmallSharp/issues/9)

:twisted_rightwards_arrows: Merged:

- 🗁 When adding new file, set as startup file [\#23](https://github.com/devlooped/SmallSharp/pull/23) (@kzu)

## [v1.0.3](https://github.com/devlooped/SmallSharp/tree/v1.0.3) (2021-02-12)

[Full Changelog](https://github.com/devlooped/SmallSharp/compare/v1.0.2...v1.0.3)

:sparkles: Implemented enhancements:

- Set up Discussions for the project [\#21](https://github.com/devlooped/SmallSharp/issues/21)

:bug: Fixed bugs:

- Fix icon that was replaced with clarius.org one [\#22](https://github.com/devlooped/SmallSharp/issues/22)

## [v1.0.2](https://github.com/devlooped/SmallSharp/tree/v1.0.2) (2021-02-12)

[Full Changelog](https://github.com/devlooped/SmallSharp/compare/v1.0.1...v1.0.2)

:sparkles: Implemented enhancements:

- Allow consuming CI/main package from the sleet feed [\#19](https://github.com/devlooped/SmallSharp/issues/19)

:bug: Fixed bugs:

- When changing active files quickly, opening startup file may fail [\#18](https://github.com/devlooped/SmallSharp/issues/18)
- Debug.Fail dialogs should not be shown on publicly released packages [\#17](https://github.com/devlooped/SmallSharp/issues/17)
- Renaming file crashes Visual Studio [\#15](https://github.com/devlooped/SmallSharp/issues/15)

:twisted_rightwards_arrows: Merged:

- Improve resiliency when invoking DTE [\#20](https://github.com/devlooped/SmallSharp/pull/20) (@kzu)
- typo: fix roslyn [\#14](https://github.com/devlooped/SmallSharp/pull/14) (@alastairtree)

## [v1.0.1](https://github.com/devlooped/SmallSharp/tree/v1.0.1) (2020-11-20)

[Full Changelog](https://github.com/devlooped/SmallSharp/compare/v1.0.0...v1.0.1)

:sparkles: Implemented enhancements:

- How to install?  [\#10](https://github.com/devlooped/SmallSharp/issues/10)

## [v1.0.0](https://github.com/devlooped/SmallSharp/tree/v1.0.0) (2020-11-18)

[Full Changelog](https://github.com/devlooped/SmallSharp/compare/v0.3.0...v1.0.0)

:twisted_rightwards_arrows: Merged:

- Open startup file upon selection [\#8](https://github.com/devlooped/SmallSharp/pull/8) (@kzu)
- Version improvements [\#6](https://github.com/devlooped/SmallSharp/pull/6) (@kzu)
- Push and add CI package version for dogfooding purposes. [\#5](https://github.com/devlooped/SmallSharp/pull/5) (@kzu)

## [v0.3.0](https://github.com/devlooped/SmallSharp/tree/v0.3.0) (2020-10-07)

[Full Changelog](https://github.com/devlooped/SmallSharp/compare/v0.2.0...v0.3.0)

## [v0.2.0](https://github.com/devlooped/SmallSharp/tree/v0.2.0) (2020-10-01)

[Full Changelog](https://github.com/devlooped/SmallSharp/compare/b42f339e41771204a132fda34b061236b78c8511...v0.2.0)



\* *This Changelog was automatically generated by [github_changelog_generator](https://github.com/github-changelog-generator/github-changelog-generator)*
