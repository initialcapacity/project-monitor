[<AutoOpen>]
module DesktopApp.Styles

open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Types

let inline col v = Grid.column v
let inline row v = Grid.row v

[<RequireQualifiedAccess>]
module Color =
    let transparent = "#0000"
    let yellow = "#884"
    let green = "#484"
    let red = "#844"
    let grey = "#444"
    let black = "#000"

[<RequireQualifiedAccess>]
module TextBlock =
    let private textBlock (classes: string list) text attrs: IView =
        let defaults = [
            TextBlock.text text
            TextBlock.classes classes
        ]
        TextBlock.create (defaults @ attrs) :> IView

    let title = textBlock ["title"]
    let subTitle = textBlock ["subTitle"]
    let note = textBlock ["note"]
