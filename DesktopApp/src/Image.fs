module DesktopApp.Image

open Avalonia.Controls
open Avalonia.FuncUI.Builder
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Types
open Avalonia.Media

[<RequireQualifiedAccess>]
module Image =
    let private source<'t when 't :> Image>(value: IImage) : IAttr<'t> =
        AttrBuilder<'t>.CreateProperty<IImage>(Image.SourceProperty, value, ValueNone)

    let private icon path attrs =
        let drawing = GeometryDrawing()
        drawing.Geometry <- PathGeometry.Parse(path)
        drawing.Brush <- Brush.Parse("#ffffff")

        let drawingImage = DrawingImage()
        drawingImage.Drawing <- drawing

        let defaultAttrs = [
            source drawingImage
            Image.width 24.0
            Image.height 24.0
        ]
        Image.create (defaultAttrs @ attrs) :> IView

    let warningIcon = icon "M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm1 15h-2v-2h2v2zm0-4h-2V7h2v6z"
