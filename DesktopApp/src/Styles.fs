[<AutoOpen>]
module DesktopApp.Styles

open Elmish
open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Types

let inline col v = Grid.column v
let inline row v = Grid.row v

[<RequireQualifiedAccess>]
module Color =
    let transparent = "#0000"
    let yellow = "#b884"
    let green = "#b484"
    let red = "#b844"
    let grey = "#b444"

[<RequireQualifiedAccess>]
module TextBlock =
    let private textBlock (classes: string list) text attrs: IView =
        let defaults = [
            TextBlock.text text
            TextBlock.classes classes
        ]
        TextBlock.create (defaults @ attrs) :> IView

    let label = textBlock ["label"]
    let title = textBlock ["title"]
    let subTitle = textBlock ["subTitle"]

[<RequireQualifiedAccess>]
module TextBox =
    let field value onTextChanged attrs: IView =
        let defaults = [
            TextBox.text value
            TextBox.onTextChanged onTextChanged
            TextBox.classes ["field"]
        ]
        TextBox.create (defaults @ attrs) :> IView

[<RequireQualifiedAccess>]
module Button =
    let private button (classes: string list) (text: string) onClick attrs: IView =
        let defaults = [
            Button.content text
            Button.onClick (fun _ -> onClick())
            Button.classes classes
        ]
        Button.create (defaults @ attrs) :> IView

    let primary = button ["primary"]
    let back = button ["back"]
