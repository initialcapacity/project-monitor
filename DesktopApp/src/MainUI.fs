module DesktopApp.MainUI

open Elmish
open Avalonia.Layout
open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Types

open DesktopApp.GithubApi
open DesktopApp.RemoteData

type Screen =
    | RepoForm
    | RepoStatus

type Model =
    { GithubInfo: GithubInfo
      Screen: Screen
      ActionRunData: RemoteData<ActionRun, ApiError> }

type Msg =
    | OwnerChanged of string
    | RepoChanged of string
    | TokenChanged of string
    | FormSubmit
    | BackToForm
    | UpdateActionRunData of RemoteData<ActionRun, ApiError>

let init _ =
    { GithubInfo = { Owner = ""; Repo = ""; Token = "" }
      Screen = RepoForm
      ActionRunData = NotLoaded }, Cmd.none

let inline private col v = Grid.column v
let inline private row v = Grid.row v

let private formView model dispatch =
    let labelHeight = 32
    let fieldHeight = 32
    let buttonHeight = 48
    let githubInfo = model.GithubInfo

    Grid.create [
        Grid.columnDefinitions "*, 400, *"
        Grid.rowDefinitions $"*, {labelHeight}, {fieldHeight}, {labelHeight}, {fieldHeight}, {labelHeight}, {fieldHeight}, {buttonHeight}, *"

        Grid.children [
            TextBlock.label "Owner" [ col 1; row 1 ]
            TextBox.field githubInfo.Owner dispatch OwnerChanged [ col 1; row 2 ]

            TextBlock.label "Repo" [ col 1; row 3 ]
            TextBox.field githubInfo.Repo dispatch RepoChanged [ col 1; row 4 ]

            TextBlock.label "Token" [ col 1; row 5 ]
            TextBox.field githubInfo.Token dispatch TokenChanged [ col 1; row 6 ]

            Button.primary "Submit" dispatch FormSubmit [ col 1; row 7 ]
        ]
    ]

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

    TextBlock.label message attrs

let private statusView model dispatch =
    let color, childView =
        match model.ActionRunData with
        | NotLoaded -> "#0000", loadingView [ col 1; row 1 ]
        | Loading -> "#0000", loadingView [ col 1; row 1 ]
        | Loaded actionRun ->
            match actionRun.Status with
            | InProgress -> "#a884", loadedView actionRun [ col 1; row 1 ]
            | Success -> "#a484", loadedView actionRun [ col 1; row 1 ]
            | Failure -> "#a844", loadedView actionRun [ col 1; row 1 ]
            | Unknown -> "#a444", loadedView actionRun [ col 1; row 1 ]
        | Refreshing actionRun -> "#0000", loadedView actionRun [ col 1; row 1 ]
        | Error err -> "#0000", errorView err [ col 1; row 1 ]

    DockPanel.create [
        DockPanel.background color
        DockPanel.children [
            Button.back "Back" dispatch BackToForm [ DockPanel.dock Dock.Top ]
            childView
        ]
    ]

let view (model: Model) (dispatch: Dispatch<Msg>) =
    match model.Screen with
    | RepoForm -> formView model dispatch :> IView
    | RepoStatus -> statusView model dispatch :> IView

let private fetchActionRunData model =
    async {
        let! result = fetchActionRuns model.GithubInfo
        return result |> RemoteData.ofResult |> UpdateActionRunData
    }

let update (msg: Msg) (model: Model): Model * Cmd<Msg> =
    match msg with
    | OwnerChanged value -> { model with GithubInfo = { model.GithubInfo with Owner = value }}, Cmd.none
    | RepoChanged value -> { model with GithubInfo = { model.GithubInfo with Repo = value }}, Cmd.none
    | TokenChanged value -> { model with GithubInfo = { model.GithubInfo with Token = value }}, Cmd.none
    | FormSubmit -> { model with Screen = RepoStatus; ActionRunData = Loading }, Cmd.OfAsync.result (fetchActionRunData model)
    | BackToForm -> { model with Screen = RepoForm }, Cmd.none
    | UpdateActionRunData data -> { model with ActionRunData = data }, Cmd.none
