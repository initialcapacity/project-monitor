[<AutoOpen>]
module DesktopApp.Prelude

open System

let curry f a b = f (a, b)

[<RequireQualifiedAccess>]
module Result =

    let defaultWith f result =
        match result with
        | Ok x -> x
        | Error x -> f x

    let toOption result =
        match result with
        | Ok x -> Some x
        | Error _ -> None

    let toBool result =
        match result with
        | Ok _ -> true
        | Error _ -> false

[<RequireQualifiedAccess>]
module Try =
    let async wrapper f =
        async {
            try
                let! value = f ()
                return Ok value
            with ex ->
                return Error (wrapper ex)
        }

[<RequireQualifiedAccess>]
module AsyncResult =

    let map f asyncRes =
        async {
            match! asyncRes with
            | Ok x -> return Ok (f x)
            | Error x -> return Error x
        }

    let bind f asyncRes =
        async {
            match! asyncRes with
            | Ok x -> return! f x
            | Error x -> return Error x
        }
