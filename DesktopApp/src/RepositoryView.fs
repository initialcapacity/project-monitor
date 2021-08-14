[<RequireQualifiedAccess>]
module DesktopApp.RepositoryView

open Avalonia.Controls
open Avalonia.FuncUI.DSL

open Avalonia.FuncUI.Types
open Avalonia.Layout
open DesktopApp.GithubApi
open DesktopApp.RemoteData

type Dispatch =
    { OnRepositoryChanged: (GithubRepository -> GithubRepository) -> unit
      OnRepositorySaved: unit -> unit
      OnEdit: unit -> unit }

let form (repository: GithubRepository) dispatch =
    let labelHeight = 32
    let fieldHeight = 32
    let buttonHeight = 48

    let onOwnerChanged text =
        dispatch.OnRepositoryChanged (fun r -> { r with Owner = text })

    let onNameChanged text =
        dispatch.OnRepositoryChanged (fun r -> { r with Name = text })

    let onTokenChanged text =
        dispatch.OnRepositoryChanged (fun r -> { r with Token = text })

    Grid.create [
        Grid.columnDefinitions "*, 400, *"
        Grid.rowDefinitions $"*, {labelHeight}, {fieldHeight}, {labelHeight}, {fieldHeight}, {labelHeight}, {fieldHeight}, {buttonHeight}, *"

        Grid.children [
            TextBlock.label "Owner" [ col 1; row 1 ]
            TextBox.field repository.Owner onOwnerChanged [ col 1; row 2 ]
            TextBlock.label "Repo" [ col 1; row 3 ]
            TextBox.field repository.Name onNameChanged [ col 1; row 4 ]
            TextBlock.label "Token" [ col 1; row 5 ]
            TextBox.field repository.Token onTokenChanged [ col 1; row 6 ]
            Button.primary "Save" dispatch.OnRepositorySaved [ col 1; row 7 ]
        ]
    ] :> IView

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

let status (data: RemoteData<ActionRun, ApiError>) dispatch =
    let color, childView =
        match data with
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

    DockPanel.create [
        DockPanel.background color
        DockPanel.children [
            Button.back "Edit" dispatch.OnEdit [
                DockPanel.dock Dock.Top
            ]
            childView
        ]
    ] :> IView
