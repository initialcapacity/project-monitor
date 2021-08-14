module DesktopApp.MainUI

open DesktopApp
open Elmish

open DesktopApp.GithubApi
open DesktopApp.RemoteData

type Screen =
    | RepositoryForm
    | RepositoryStatus

type Model =
    { Repository: GithubRepository
      Screen: Screen
      ActionRunData: RemoteData<ActionRun, ApiError> }

type Msg =
    | RepositoryChanged of GithubRepository
    | SaveRepository of unit
    | EditRepository of unit
    | UpdateActionRunData of RemoteData<ActionRun, ApiError>

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
    | RepositoryChanged value -> { model with Repository = value }, Cmd.none
    | SaveRepository () -> { model with Screen = RepositoryStatus; ActionRunData = Loading }, Cmd.OfAsync.result (fetchActionRunData model)
    | EditRepository () -> { model with Screen = RepositoryForm }, Cmd.none
    | UpdateActionRunData data -> { model with ActionRunData = data }, Cmd.none
