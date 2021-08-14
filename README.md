# Project Monitor

This is a simple project monitor that observes
the last workflow run of a set of Github repositories.

## Configuration

Running the application requires you to pass in the path to a configuration file.
It should follow the structure [from the provided sample](DesktopApp/res/ConfigSample.json).

## Dev Dependencies

 * [.NET 5](https://dotnet.microsoft.com/download/dotnet/5.0)
 * [JetBrains Rider](https://www.jetbrains.com/rider/)
 * or [Visual Studio Code](https://code.visualstudio.com/) + [Ionide](https://ionide.io/)

## Software Design

The application is developed with the cross-platform
desktop application framework [AvaloniaUI](https://avaloniaui.net/)
and its F# companion library [Avalonia.FuncUI](https://avaloniacommunity.github.io/Avalonia.FuncUI.Docs/).

It launches a single window which runs a small [Elmish](https://elmish.github.io/elmish/) application,
implementing a **Unidirectional Data Flow** architecture originally introduced
by the [Elm programming suite](https://guide.elm-lang.org/architecture/).

## Adding layouts for more projects

As of the time of writing this README the app supports 1, 2, 3 or 4 projects to be displayed.
If you need to support more projects, you only need change the [Layout module](DesktopApp/src/Layout.fs).

Understanding [AvaloniaUI's Grid](https://docs.avaloniaui.net/docs/controls/grid) should be enough to figure it out.

## Publishing

```sh
dotnet publish -c release -r <release-identifier>
```

Example release identifiers:
 * `osx-x64`
 * `linux-x64`
 * `win10-x64`
