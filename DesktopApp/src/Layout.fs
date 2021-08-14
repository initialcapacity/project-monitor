module DesktopApp.Layout

open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Types

open DesktopApp.StatusView

[<RequireQualifiedAccess>]
module Layout =
    let private tooManyReposView =
        TextBlock.subTitle "Too many repos, max is 1." []

    let private one s1 =
        StatusView.create s1 []

    let private two s1 s2 =
        Grid.create [
            Grid.rowDefinitions "*"
            Grid.columnDefinitions " *, *"

            Grid.children [
                StatusView.create s1 [ Grid.column 0 ]
                StatusView.create s2 [ Grid.column 1 ]
            ]
        ] :> IView

    let private three s1 s2 s3 =
        Grid.create [
            Grid.rowDefinitions "*"
            Grid.columnDefinitions "2*, 3*"
            Grid.children [
                StatusView.create s1 [ Grid.column 0 ]
                Grid.create [
                    Grid.column 1
                    Grid.rowDefinitions "*, *"
                    Grid.columnDefinitions "*"
                    Grid.children [
                        StatusView.create s2 [ Grid.row 0 ]
                        StatusView.create s3 [ Grid.row 1 ]
                    ]
                ]
            ]
        ] :> IView

    let private four s1 s2 s3 s4 =
        Grid.create [
            Grid.rowDefinitions "*, *"
            Grid.columnDefinitions " *, *"

            Grid.children [
                StatusView.create s1 [ Grid.column 0; Grid.row 0 ]
                StatusView.create s2 [ Grid.column 1; Grid.row 0 ]
                StatusView.create s3 [ Grid.column 0; Grid.row 1 ]
                StatusView.create s4 [ Grid.column 1; Grid.row 1 ]
            ]
        ] :> IView

    let create (statuses: BuildStatus list) =
        match statuses with
        | [s1] -> one s1
        | [s1; s2] -> two s1 s2
        | [s1; s2; s3] -> three s1 s2 s3
        | [s1; s2; s3; s4] -> four s1 s2 s3 s4
        | _ -> tooManyReposView
