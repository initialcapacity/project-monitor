module DesktopApp.RemoteData

type RemoteData<'success, 'error> =
    | NotLoaded
    | Loading
    | Loaded of 'success
    | Refreshing of 'success
    | Error of 'error

[<RequireQualifiedAccess>]
module RemoteData =

    let map f data =
        match data with
        | NotLoaded -> NotLoaded
        | Loading -> Loading
        | Loaded x -> Loaded (f x)
        | Refreshing x -> Refreshing (f x)
        | Error x -> Error x

    let ofResult result =
        result
        |> Result.map Loaded
        |> Result.defaultWith Error

    let defaultValue value data =
        match data with
        | Loaded x -> x
        | Refreshing x -> x
        | _ -> value
