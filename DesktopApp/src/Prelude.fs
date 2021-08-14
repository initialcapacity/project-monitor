[<AutoOpen>]
module DesktopApp.Prelude

[<RequireQualifiedAccess>]
module Result =

    let defaultWith f result =
        match result with
        | Ok x -> x
        | Error x -> f x

[<RequireQualifiedAccess>]
module Try =
    let async wrapper f =
        async {
            try
                let! value = f ()
                return Ok value
            with ex -> return Error (wrapper ex)
        }
