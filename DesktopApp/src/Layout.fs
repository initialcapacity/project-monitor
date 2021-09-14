module DesktopApp.Layout

open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Types

open DesktopApp.StatusView

let private view s row col =
    StatusView.create s [ Grid.row row; Grid.column col ]

let private grid rows columns children =
    let def count = "*" |> List.replicate count |> String.concat ","

    Grid.create [
        Grid.rowDefinitions (def rows)
        Grid.columnDefinitions (def columns)
        Grid.children children
    ] :> IView

let private grid4 = grid 2 2

let private grid6 = grid 2 3

[<RequireQualifiedAccess>]
module Layout =
    let private tooManyReposView =
        TextBlock.subTitle "Too many repos, max is 6." []

    let private layout1 s1 =
        StatusView.create s1 []

    let private layout2 s1 s2 =
        grid 1 2 [
            view s1 0 0
            view s2 0 1
        ]

    let private layout3 s1 s2 s3 =
        grid4 [
            view s1 0 0
            view s2 0 1
            view s3 1 0
        ]

    let private layout4 s1 s2 s3 s4 =
        grid4 [
            view s1 0 0
            view s2 0 1
            view s3 1 0
            view s4 1 1
        ]

    let private layout5 s1 s2 s3 s4 s5 =
        grid6 [
            view s1 0 0
            view s2 0 1
            view s3 0 2
            view s4 1 0
            view s5 1 1
        ]

    let private layout6 s1 s2 s3 s4 s5 s6 =
        grid6 [
            view s1 0 0
            view s2 0 1
            view s3 0 2
            view s4 1 0
            view s5 1 1
            view s6 1 2
        ]

    let create (statuses: RepositoryAndStatus list) =
        let view =
            match statuses with
            | [s1] -> layout1 s1
            | [s1; s2] -> layout2 s1 s2
            | [s1; s2; s3] -> layout3 s1 s2 s3
            | [s1; s2; s3; s4] -> layout4 s1 s2 s3 s4
            | [s1; s2; s3; s4; s5] -> layout5 s1 s2 s3 s4 s5
            | [s1; s2; s3; s4; s5; s6] -> layout6 s1 s2 s3 s4 s5 s6
            | _ -> tooManyReposView

        DockPanel.create [
            DockPanel.margin 16.0
            DockPanel.children [ view ]
        ] :> IView
