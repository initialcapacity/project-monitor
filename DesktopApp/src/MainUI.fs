module DesktopApp.MainUI

open DesktopApp.GithubApi
open DesktopApp.RemoteData
open Elmish
open Avalonia.Controls
open Avalonia.FuncUI.DSL

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

let private statusView model dispatch =
    let padding = 80
    let status =
        match model.ActionRunData with
        | NotLoaded -> ""
        | Loading -> "Loading"
        | Loaded status -> $"%A{status}"
        | Refreshing status -> $"%A{status}"
        | Error x -> $"Error %A{x}"

    Grid.create [
        Grid.rowDefinitions $"{padding}, *, {padding}"
        Grid.columnDefinitions $"{padding}, *, {padding}"

        Grid.children [
            Button.back "Back" dispatch BackToForm [ Grid.column 0; Grid.row 0 ]
            TextBlock.label status [ Grid.column 1; Grid.row 1 ]
        ]
    ]

let view (model: Model) (dispatch: Dispatch<Msg>) =
    match model.Screen with
    | RepoForm -> formView model dispatch
    | RepoStatus -> statusView model dispatch

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
