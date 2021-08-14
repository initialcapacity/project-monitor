[<AutoOpen>]
module DesktopApp.Styles

open Elmish
open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Types

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

module TextBox =
    let field value (dispatch: Dispatch<'a>) (msg: string -> 'a) attrs: IView =
        let defaults = [
            TextBox.text value
            TextBox.onTextChanged (msg >> dispatch)
            TextBox.classes ["field"]
        ]
        TextBox.create (defaults @ attrs) :> IView

module Button =
    let private button<'a> (classes: string list) (text: string) (dispatch: Dispatch<'a>) (msg: 'a) attrs: IView =
        let defaults = [
            Button.content text
            Button.onClick (fun _ -> dispatch msg)
            Button.classes classes
        ]
        Button.create (defaults @ attrs) :> IView

    let primary<'a> = button<'a> ["primary"]
    let back<'a> = button<'a> ["back"]
