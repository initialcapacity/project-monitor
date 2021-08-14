module DesktopApp.StatusView

open Avalonia.Controls
open Avalonia.FuncUI.DSL

open Avalonia.FuncUI.Types
open Avalonia.Layout
open DesktopApp.GithubApi
open DesktopApp.RemoteData

type BuildStatus = RemoteData<BuildRun, ApiError>

let private loadingView attrs: IView =
    TextBlock.subTitle "Loading..." attrs

let private loadedView buildRun attrs =
    let message =
        buildRun.Message.Split("\n")
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
            TextBlock.subTitle buildRun.Project [
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

let private buildClasses status =
    match status with
    | InProgress -> ["buildInProgress"]
    | Success -> ["buildSuccess"]
    | Failure -> ["buildFailure"]
    | Unknown -> ["buildUnknown"]

module StatusView =
    let create (status: BuildStatus) attrs =
        let classes, childView =
            match status with
            | NotLoaded -> ["statusNotLoaded"], loadingView [ col 1; row 1 ]
            | Loading -> ["statusNotLoaded"], loadingView [ col 1; row 1 ]
            | Loaded buildRun -> buildClasses buildRun.Status, loadedView buildRun [ col 1; row 1 ]
            | Refreshing buildRun -> buildClasses buildRun.Status, loadedView buildRun [ col 1; row 1 ]
            | Error err -> ["statusNotLoaded"], errorView err [ col 1; row 1 ]

        let defaultAttrs = [
            Border.classes (["statusView"] @ classes)
            Border.child childView
        ]
        Border.create (defaultAttrs @ attrs) :> IView
