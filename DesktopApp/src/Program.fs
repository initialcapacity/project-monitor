module DesktopApp.Program

open Avalonia.Controls
open Avalonia.Media
open Avalonia.Platform
open DesktopApp
open Elmish
open Avalonia
open Avalonia.Controls.ApplicationLifetimes
open Avalonia.FuncUI
open Avalonia.FuncUI.Elmish
open Avalonia.FuncUI.Components.Hosts

type MainWindow() as this =
    inherit HostWindow()
    do
        base.Title <- "Project Monitor"
        base.Width <- 800.0
        base.Height <- 400.0
        base.ExtendClientAreaToDecorationsHint <- true
        base.TransparencyLevelHint <- WindowTransparencyLevel.AcrylicBlur
        base.ExtendClientAreaChromeHints <- ExtendClientAreaChromeHints.PreferSystemChrome
        base.Background <- Brush.Parse(Color.grey)

        (MainUI.init, MainUI.update, MainUI.view)
        |||> Elmish.Program.mkProgram
        |> Program.withSubscription MainUI.subscribe
        |> Program.withHost this
        |> Program.run

type App() =
    inherit Application()

    override this.Initialize() =
        this.Styles.Load "avares://Avalonia.Themes.Fluent/FluentDark.xaml"
        this.Styles.Load "avares://DesktopApp/res/Styles.xaml"

    override this.OnFrameworkInitializationCompleted() =
        match this.ApplicationLifetime with
        | :? IClassicDesktopStyleApplicationLifetime as desktopLifetime ->
            desktopLifetime.MainWindow <- MainWindow()
        | _ -> ()

[<EntryPoint>]
let main args =
    AppBuilder
        .Configure<App>()
        .UsePlatformDetect()
        .UseSkia()
        .StartWithClassicDesktopLifetime(args)
