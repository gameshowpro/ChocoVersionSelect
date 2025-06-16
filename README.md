# ChocoVersionSelect

<img src="./resources/icon/icon.svg" align="left" alt="ChocoVersionSelect icon" width="64px" />

ChocoVersionSelect is a Windows user interface for [Chocolatey](https://chocolatey.org) (the Machine Package Manager for Windows). Unlike [Chocolatey GUI](https://github.com/chocolatey/ChocolateyGUI), which has a broad set of features, ChocoVersionSelect is focused on managing the local installation of a *single package*. It does this by wrapping choco's `list`, `search`, and `upgrade` verbs.
## Installation
You can install ChocoVersionSelect from the [Chocolatey Community Respository](https://community.chocolatey.org/packages/chocoversionselect). If you already have it enabled as one of your packages sources, you can install it like this:
```
choco install ChocoVersionSelect -y
```
## Usage

ChocoVersionSelect requires commandline parameters. You can see a reminder of these by executing it with none.
```
ChocoVersionSelect <packageName> [--source=<sourceName>]
```
In the case where multiple sources are configured, including the source name parameter should speed up the `search` and `upgrade` operations.
To end-user scenarios are envisaged:
1. Clicking on a Windows shortcut that has been preconfigured with the appropriate parameters, name, and icon.
1. Clicking on a button in another application that will launch ChocoVersionSelect to allow the parent application to be upgraded.
## Features
- Launched with commandline parameters specifying package name and source.
- UI updates asynchronously as data is received from [choco.exe](https://github.com/chocolatey/choco).
- All available versions of the package are shown, with focus on the current, latest, and previous.
- Publish dates and ages are shown.
- Upgrades and downgrades can be requested from the UI and confirmed on completion.
- Runs without administrative access, requesting admin escalation for Chocolatey when required.
- User's dark mode preferences from Windows are observed.
## Possible future features
- Full (but optional) support for runnning with administrative, allowing the console window to remaining hidden and possibly better feedback to be provided during upgrades.
- Toast notifictions within the window to provide an additional layer of feedback, especially in an exception state.
- Access to console session for troubleshooting purposes, e.g. forwarding to support staff.
- Automatic refreshing and notification of new package availability using taskbar icon badge.
- Multi-language support.
## Why does this exist?
ChocoVersionSelect was created to address a specific use-case. Some users who are not comfortable using a command shell. Some users would be confused or endangered by the wide array of features in [Chocolatey GUI](https://github.com/chocolatey/ChocolateyGUI). ChocoVersionSelect lets them achieve all the package management they need with a minimal learning curve.
## Dependencies
ChocoVersionSelect does not have any build-time dependencies outside of the .NET 9.0 SDK. It won't do anything useful unless you already have [Chocolatey](https://chocolatey.org) installed and it can find choco.exe on the Windows [path](https://learn.microsoft.com/en-us/windows-server/administration/windows-commands/path).
## Source code notes
At the time of writing, this project was built for the lastest .NET Runtime (9.0) and C# (12.0). The UI is built with WPF using MVVM patterns.

Since this application is quite limited in size and scope, it could serve as a good example for developers looking for a working example of some recent .NET features:
- Immutability
	- [FrozenDictionary](https://learn.microsoft.com/en-us/dotnet/api/system.collections.frozen.frozendictionary)
	- [ImmutableList](https://learn.microsoft.com/en-us/dotnet/api/system.collections.immutable.immutablelist-1)
	- [record](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/record)
- [Channels](https://learn.microsoft.com/en-us/dotnet/api/system.threading.channels) for cross-thread producer consumer patterns.
- [Collection expressions](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-12#collection-expressions)
- [global using](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/using-directive#global-modifier)
- [File scoped namespace declarations](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/namespace)
- [Primary constructors](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/instance-constructors#primary-constructors)
- Dark mode in WPF
	- Getting Windows Dark mode setting
	- Detecting changes to Windows' Dark Mode setting.
	- Applying dark mode setting to title bar.
	- Switching WPF resource dictionary and icon.
## Credits
- Dark mode change detection was adapted from the code on [David Rickard's Tech Blog](https://engy.us/blog/2018/10/20/dark-theme-in-wpf/)
- Iconography is from [Pictogrammer's Material Design Icons](https://pictogrammers.com/library/mdi/) set.
## Contributing
If you have an idea to improve existing code, feel free to send a pull request. If you want to add a new feature, it's probably worth opening a ticket first so we can discuss the specification in advance, to prevent wasted effort.
