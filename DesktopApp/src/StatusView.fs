module DesktopApp.StatusView

open Avalonia.Controls
open Avalonia.FuncUI.DSL

open Avalonia.FuncUI.Types
open Avalonia.Layout
open DesktopApp.GithubApi
open DesktopApp.RemoteData

type BuildStatus = RemoteData<BuildRun, ApiError>

type RepositoryStatus =
    { Repository: GithubRepoWorkflow
      Status: BuildStatus }

let private statusBox repo message attrs =
    let viewAttrs = [
        Grid.rowDefinitions "*,*,*"
        Grid.columnDefinitions "*"
        Grid.margin 16.0
        Grid.children [
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
        ]
    ]

    Grid.create (viewAttrs @ attrs) :> IView
    

let private loadingView repo =
    statusBox repo "Loading..."

let private loadedView repo buildRun =
    let message =
        buildRun.Message.Split("\n")
        |> Array.tryHead
        |> Option.defaultValue "-"
        
    statusBox repo message

let private errorView repo error =
    let message =
        match error with
        | ParseError _ -> "JSON Parse Error"
        | ConnectionError _ -> "Connection Error"
        | NoWorkflow -> "No Workflow Available"

    statusBox repo message

let private buildClasses status =
    match status with
    | InProgress -> ["buildInProgress"]
    | Success -> ["buildSuccess"]
    | Failure -> ["buildFailure"]
    | Unknown -> ["buildUnknown"]

module StatusView =
    let create (repoStatus: RepositoryStatus) attrs =
        let repo = repoStatus.Repository

        let classes, childView =
            match repoStatus.Status with
            | NotLoaded -> ["statusNotLoaded"], loadingView repo [ col 1; row 1 ]
            | Loading -> ["statusNotLoaded"], loadingView repo [ col 1; row 1 ]
            | Loaded buildRun -> buildClasses buildRun.Status, loadedView repo buildRun [ col 1; row 1 ]
            | Refreshing buildRun -> buildClasses buildRun.Status, loadedView repo buildRun [ col 1; row 1 ]
            | Error err -> ["statusNotLoaded"], errorView repo err [ col 1; row 1 ]

        let defaultAttrs = [
            Border.classes (["statusView"] @ classes)
            Border.child childView
        ]
        Border.create (defaultAttrs @ attrs) :> IView
