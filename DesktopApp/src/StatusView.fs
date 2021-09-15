module DesktopApp.StatusView

open System
open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Types
open Avalonia.Layout

open DesktopApp.GithubApi
open DesktopApp
open DesktopApp.Image

type BuildStatus =
    { Status: ActionStatus
      Commit: Commit
      Fetched: DateTimeOffset }

type RemoteBuildStatus =
    | NotLoaded
    | Loaded of BuildStatus
    | Error of ApiError
    | RefreshError of BuildStatus * ApiError

type RepositoryAndStatus =
    { Repository: GithubRepoWorkflow
      Status: RemoteBuildStatus }

let private warningView warning =
    let children =
        warning
        |> Option.map (fun msg -> [ TextBlock.note msg []; Image.warningIcon [] ])
        |> Option.defaultValue []

    StackPanel.create [
        Grid.row 2
        StackPanel.spacing 12.0
        StackPanel.orientation Orientation.Horizontal
        StackPanel.children children
        StackPanel.verticalAlignment VerticalAlignment.Bottom
        StackPanel.horizontalAlignment HorizontalAlignment.Right
    ] :> IView

let private statusBox repo message (warning: string option) attrs =
    let children = [
        TextBlock.title repo.Repo [
            Grid.row 0
            TextBlock.verticalAlignment VerticalAlignment.Bottom
        ]
        TextBlock.title repo.Workflow [
            Grid.row 1
        ]
        TextBlock.subTitle message [
            Grid.row 2
            TextBlock.verticalAlignment VerticalAlignment.Top
        ]
        warningView warning
    ]

    let viewAttrs = [
        Grid.rowDefinitions "*,*,*"
        Grid.columnDefinitions "*"
        Grid.margin 16.0
        Grid.children children
    ]

    Grid.create (viewAttrs @ attrs) :> IView

let private loadingView repo =
    statusBox repo "Loading..." None

let private buildMessage buildRun =
    buildRun.Commit.Message.Split("\n")
    |> Array.tryHead
    |> Option.defaultValue "-"

let private errorMessage error =
    match error with
    | ParseError _ -> "JSON Parse Error"
    | ConnectionError _ -> "Connection Error"
    | NoWorkflow -> "No Workflow Available"

let private loadedView repo buildRun  =
    let message = buildMessage buildRun
    statusBox repo message None

let private errorView repo err =
    let message = errorMessage err
    statusBox repo message None

let private loadedViewWithError repo buildRun error =
    let message = buildMessage buildRun
    let warning = errorMessage error
    statusBox repo message (Some warning)

let private buildClasses status =
    match status with
    | InProgress -> ["buildInProgress"]
    | Success -> ["buildSuccess"]
    | Failure -> ["buildFailure"]
    | Unknown -> ["buildUnknown"]

module StatusView =
    let create (repoStatus: RepositoryAndStatus) attrs =
        let repo = repoStatus.Repository

        let classes, childView =
            match repoStatus.Status with
            | NotLoaded -> ["statusNotLoaded"], loadingView repo [ col 1; row 1 ]
            | Loaded buildRun -> buildClasses buildRun.Status, loadedView repo buildRun [ col 1; row 1 ]
            | Error err -> ["statusNotLoaded"], errorView repo err [ col 1; row 1 ]
            | RefreshError (buildRun, err) -> buildClasses buildRun.Status, loadedViewWithError repo buildRun err [ col 1; row 1 ]

        let defaultAttrs = [
            Border.classes (["statusView"] @ classes)
            Border.child childView
        ]
        Border.create (defaultAttrs @ attrs) :> IView
