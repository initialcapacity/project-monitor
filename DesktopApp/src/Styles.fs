[<AutoOpen>]
module DesktopApp.Styles

open Elmish
open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Types

module TextBlock =
    let label text attrs: IView =
        let defaults = [
            TextBlock.text text
            TextBlock.classes ["label"]
        ]
        TextBlock.create (defaults @ attrs) :> IView

module TextBox =
    let field value (dispatch: Dispatch<'a>) (msg: string -> 'a) attrs: IView =
        let defaults = [
            TextBox.text value
            TextBox.onTextChanged (msg >> dispatch)
            TextBox.classes ["field"]
        ]
        TextBox.create (defaults @ attrs) :> IView

module Button =
    let private button (classes: string list) (text: string) (dispatch: Dispatch<'a>) (msg: 'a) attrs: IView =
        let defaults = [
            Button.content text
            Button.onClick (fun _ -> dispatch msg)
            Button.classes classes
        ]
        Button.create (defaults @ attrs) :> IView

    let primary (text: string) (dispatch: Dispatch<'a>) (msg: 'a) attrs: IView =
        button ["primary"] text dispatch msg attrs

    let back (text: string) (dispatch: Dispatch<'a>) (msg: 'a) attrs: IView =
        button ["back"] text dispatch msg attrs
