module DesktopApp.RemoteData

type RemoteData<'success, 'error> =
    | NotLoaded
    | Loading
    | Loaded of 'success
    | Refreshing of 'success
    | Error of 'error

[<RequireQualifiedAccess>]
module RemoteData =
    let ofResult result =
        result
        |> Result.map Loaded
        |> Result.defaultWith Error
