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

let mutable private configPath: string = "./config.json"

type MainWindow() as this =
    inherit HostWindow()
    do
        base.Title <- "Project Monitor"
        base.Width <- 800.0
        base.Height <- 400.0
        base.TransparencyLevelHint <- WindowTransparencyLevel.AcrylicBlur
        base.ExtendClientAreaChromeHints <- ExtendClientAreaChromeHints.PreferSystemChrome
        base.Background <- Brush.Parse("#6000")

        (MainUI.init configPath, MainUI.update, MainUI.view)
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

let private startApp path =
    configPath <- path

    AppBuilder
        .Configure<App>()
        .UsePlatformDetect()
        .UseSkia()
        .StartWithClassicDesktopLifetime([||])

let private argumentsError () =
    eprintfn "Usage error, please launch with path to config file as argument"
    1

[<EntryPoint>]
let main args =
    match args with
    | [| path |] -> startApp path
    | _ -> argumentsError ()
