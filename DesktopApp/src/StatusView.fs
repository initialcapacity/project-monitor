module DesktopApp.StatusView

open Avalonia.Controls
open Avalonia.FuncUI.DSL

open Avalonia.FuncUI.Types
open Avalonia.Layout
open DesktopApp.GithubApi
open DesktopApp.RemoteData

type BuildStatus = RemoteData<ActionRun, ApiError>

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
        Grid.margin 16.0
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

module StatusView =
    let create (status: BuildStatus) attrs =
        let color, childView =
            match status with
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


        let defaultAttrs = [
            Border.borderThickness 16.0
            Border.cornerRadius 16.0
            Border.borderBrush Color.transparent
            Border.background color
            Border.child childView
        ]
        Border.create (defaultAttrs @ attrs) :> IView
