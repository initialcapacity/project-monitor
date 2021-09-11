module DesktopApp.RemoteData

type RemoteData<'success, 'error> =
    | Loading
    | Loaded of 'success
    | Error of 'error

[<RequireQualifiedAccess>]
module RemoteData =

    let map f data =
        match data with
        | Loading -> Loading
        | Loaded x -> Loaded (f x)
        | Error x -> Error x

    let ofResult result =
        result
        |> Result.map Loaded
        |> Result.defaultWith Error

    let defaultValue value data =
        match data with
        | Loaded x -> x
        | _ -> value
