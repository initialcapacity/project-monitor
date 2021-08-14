module DesktopApp.MainUI

open DesktopApp
open Elmish

open DesktopApp.GithubApi
open DesktopApp.RemoteData
open FSharp.Control

type Screen =
    | RepositoryForm
    | RepositoryStatus

type Model =
    { Repository: GithubRepository
      Screen: Screen
      ActionRunData: RemoteData<ActionRun, ApiError> }

type Msg =
    | RepositoryChanged of (GithubRepository -> GithubRepository)
    | SaveRepository of unit
    | EditRepository of unit
    | UpdateActionRunData of RemoteData<ActionRun, ApiError>
    | Refresh

let init _ =
    { Repository = { Owner = ""; Name = ""; Token = "" }
      Screen = RepositoryForm
      ActionRunData = NotLoaded }, Cmd.none

let view (model: Model) (dispatch: Dispatch<Msg>) =
    let repoDispatch: RepositoryView.Dispatch =
        { OnRepositoryChanged = RepositoryChanged >> dispatch
          OnRepositorySaved = SaveRepository >> dispatch
          OnEdit = EditRepository >> dispatch }

    match model.Screen with
    | RepositoryForm -> RepositoryView.form model.Repository repoDispatch
    | RepositoryStatus -> RepositoryView.status model.ActionRunData repoDispatch

let private fetchActionRunData model =
    async {
        let! result = fetchActionRuns model.Repository
        return result |> RemoteData.ofResult |> UpdateActionRunData
    }

let update (msg: Msg) (model: Model): Model * Cmd<Msg> =
    match msg with
    | RepositoryChanged func -> { model with Repository = func model.Repository }, Cmd.none
    | SaveRepository () -> { model with Screen = RepositoryStatus; ActionRunData = Loading }, Cmd.OfAsync.result (fetchActionRunData model)
    | EditRepository () -> { model with Screen = RepositoryForm }, Cmd.none
    | UpdateActionRunData data -> { model with ActionRunData = data }, Cmd.none
    | Refresh -> model, Cmd.OfAsync.result (fetchActionRunData model)

let subscribe (model: Model): Cmd<Msg> =
    let oneSecond = 1000
    let refreshFrequency = oneSecond * 10

    Cmd.ofSub (fun dispatch ->
        async {
            while true do
                do! Async.Sleep(refreshFrequency)
                dispatch Refresh
        }
        |> Async.Start
    )
