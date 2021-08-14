module DesktopApp.MainUI

open Elmish
open FSharp.Control
open Avalonia.FuncUI.DSL

open DesktopApp.Config
open DesktopApp.GithubApi
open DesktopApp.RemoteData
open DesktopApp.StatusView
open DesktopApp.Layout

type RepositoryStatus =
    { Repository: GithubRepository
      Status: BuildStatus }

type Model =
    { ConfigPath: string
      RepositoryStatuses: RemoteData<RepositoryStatus list, ConfigLoadError> }

type Msg =
    | ConfigLoaded of RemoteData<GithubRepository list, ConfigLoadError>
    | UpdateStatus of GithubRepository * RemoteData<BuildRun, ApiError>
    | Refresh

let private loadConfig path =
    async {
        let! result = Config.tryLoad path
        let data = RemoteData.ofResult result
        return ConfigLoaded data
    }

let init configPath _ =
    { ConfigPath = configPath; RepositoryStatuses = NotLoaded },
    Cmd.OfAsync.result (loadConfig configPath)

let private loadingView path =
    TextBlock.subTitle $"Loading config at {path}" []

let private errorView _ =
    TextBlock.subTitle "Failed to load config" []

let view (model: Model) (_: Dispatch<Msg>) =
    match model.RepositoryStatuses with
    | NotLoaded | Loading -> loadingView model.ConfigPath
    | Loaded repoStatuses | Refreshing repoStatuses -> Layout.create (repoStatuses |> List.map (fun r -> r.Status))
    | Error err -> errorView err

let private refreshData model dispatch =
    async {
        let repoStatuses =
            model.RepositoryStatuses
            |> RemoteData.defaultValue []

        for repoStatus in repoStatuses do
            let repo = repoStatus.Repository
            let! result = fetchActionRuns repo

            let msg =
                result
                |> RemoteData.ofResult
                |> curry UpdateStatus repoStatus.Repository

            dispatch msg
    }
    |> Async.Start

let private updateRepoStatus repo status allRepoStatuses =
    allRepoStatuses
    |> List.map (fun repoStatus ->
        if repoStatus.Repository = repo
            then { repoStatus with Status = status }
            else repoStatus
    )

let private repositoriesToStatuses =
    List.map (fun r -> { Repository = r; Status = NotLoaded })

let update (msg: Msg) (model: Model): Model * Cmd<Msg> =
    match msg with
    | ConfigLoaded data ->
        let newStatuses = data |> RemoteData.map repositoriesToStatuses
        { model with RepositoryStatuses = newStatuses }, Cmd.ofMsg Refresh
    | UpdateStatus (repo, status) ->
        let newStatuses = model.RepositoryStatuses |> RemoteData.map (updateRepoStatus repo status)
        { model with RepositoryStatuses = newStatuses }, Cmd.none
    | Refresh -> model, Cmd.ofSub (refreshData model)

let subscribe (_: Model): Cmd<Msg> =
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
