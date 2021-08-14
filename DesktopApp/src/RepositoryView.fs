[<RequireQualifiedAccess>]
module DesktopApp.RepositoryView

open Avalonia.Controls
open Avalonia.FuncUI.DSL

open Avalonia.FuncUI.Types
open Avalonia.Layout
open DesktopApp.GithubApi
open DesktopApp.RemoteData

let private loadingView attrs: IView =
    TextBlock.subTitle "Loading..." attrs

let private loadedView actionRun attrs =
    let message =
        actionRun.Message.Split("\n")
        |> Array.tryHead
        |> Option.defaultValue "-"

    let viewAttrs = [
        Grid.rowDefinitions "*,*"
        Grid.columnDefinitions "*"
        Grid.children [
            TextBlock.title message [
                Grid.row 0
                TextBlock.verticalAlignment VerticalAlignment.Bottom
            ]
            TextBlock.subTitle actionRun.Project [
                Grid.row 1
                TextBlock.verticalAlignment VerticalAlignment.Top
            ]
        ]
    ]

    Grid.create (viewAttrs @ attrs) :> IView

let private errorView error attrs: IView =
    let message =
        match error with
        | ParseError _ -> "JSON Parse Error"
        | ConnectionError _ -> "Connection Error"
        | NoWorkflow -> "No Workflow Available"

    TextBlock.subTitle message attrs

let status (data: RemoteData<ActionRun, ApiError>) =
    let color, childView =
        match data with
        | NotLoaded -> Color.transparent, loadingView [ col 1; row 1 ]
        | Loading -> Color.transparent, loadingView [ col 1; row 1 ]
        | Loaded actionRun ->
            match actionRun.Status with
            | InProgress -> Color.yellow, loadedView actionRun [ col 1; row 1 ]
            | Success -> Color.green, loadedView actionRun [ col 1; row 1 ]
            | Failure -> Color.red, loadedView actionRun [ col 1; row 1 ]
            | Unknown -> Color.grey, loadedView actionRun [ col 1; row 1 ]
        | Refreshing actionRun -> Color.transparent, loadedView actionRun [ col 1; row 1 ]
        | Error err -> Color.transparent, errorView err [ col 1; row 1 ]

    DockPanel.create [
        DockPanel.background color
        DockPanel.children [ childView ]
    ] :> IView
