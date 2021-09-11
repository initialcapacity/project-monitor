module DesktopApp.MainUI

open System
open Elmish
open FSharp.Control
open Avalonia.FuncUI.DSL

open DesktopApp.RemoteData
open DesktopApp.Config
open DesktopApp.GithubApi
open DesktopApp.StatusView
open DesktopApp.Layout

type Model =
    { ConfigPath: string
      RepositoryStatuses: RemoteData<RepositoryAndStatus list, ConfigLoadError> }

type Msg =
    | ConfigLoaded of RemoteData<GithubRepoWorkflow list, ConfigLoadError>
    | UpdateStatus of RepositoryAndStatus
    | Refresh

let private loadConfig path =
    async {
        let! result = Config.tryLoad path
        let data = RemoteData.ofResult result
        return ConfigLoaded data
    }

let init configPath _ =
    { ConfigPath = configPath; RepositoryStatuses = RemoteData.Loading },
    Cmd.OfAsync.result (loadConfig configPath)

let private loadingView path =
    TextBlock.subTitle $"Loading config at {path}" []

let private errorView _ =
    TextBlock.subTitle "Failed to load config" []

let view (model: Model) (_: Dispatch<Msg>) =
    match model.RepositoryStatuses with
    | RemoteData.Loading -> loadingView model.ConfigPath
    | RemoteData.Loaded repoStatuses -> Layout.create repoStatuses
    | RemoteData.Error err -> errorView err

let private refreshRepo dispatch previousRepoAndStatus =
    let newBuildStatus s c =
        { Status = s; Commit = c; Fetched = DateTimeOffset() }

    async {
        let repo = previousRepoAndStatus.Repository
        let previousStatus = previousRepoAndStatus.Status

        let! result = fetchActionRuns "https://api.github.com" repo

        let remoteBuildStatus =
            match result, previousStatus with
            | Result.Ok (actionStatus, commit), _ ->
                Loaded (newBuildStatus actionStatus commit)
            | Result.Error apiError, Loaded buildStatus ->
                RefreshError (buildStatus, apiError)
            | Result.Error apiError, RefreshError (buildStatus, _) ->
                RefreshError (buildStatus, apiError)
            | Result.Error apiError, _ ->
                Error apiError

        let msg = UpdateStatus { Repository = repo; Status = remoteBuildStatus }

        dispatch msg
    }

let private refreshData model dispatch =
    model.RepositoryStatuses
    |> RemoteData.defaultValue []
    |> AsyncSeq.ofSeq
    |> AsyncSeq.iterAsyncParallel (refreshRepo dispatch)
    |> Async.RunSynchronously

let private updateRepoStatus repoStatus allRepoStatuses =
    allRepoStatuses
    |> List.map (fun r ->
        if r.Repository = repoStatus.Repository
            then repoStatus
            else r
    )

let private repositoriesToStatuses =
    List.map (fun r -> { Repository = r; Status = NotLoaded })

let update (msg: Msg) (model: Model): Model * Cmd<Msg> =
    match msg with
    | ConfigLoaded data ->
        let newStatuses = data |> RemoteData.map repositoriesToStatuses
        { model with RepositoryStatuses = newStatuses }, Cmd.ofMsg Refresh
    | UpdateStatus repoStatus ->
        let newStatuses = model.RepositoryStatuses |> RemoteData.map (updateRepoStatus repoStatus)
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
