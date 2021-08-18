module DesktopApp.Tests.Support.ApiTesting

open System
open System.Threading
open Suave
open Suave.Operators
open Suave.Response
open Suave.Writers

module WebPart =
    let json status string =
        response status (UTF8.bytes string)
            >=> setHeader "Content-Type" "application/json"

let always a _ = a

let binding =
    HttpBinding.createSimple HTTP "0.0.0.0" 8080

type RecordedRequest =
    { Method: HttpMethod
      Path: string }

type TestApi(webPart: WebPart) as this =

    let mutable requests: HttpRequest list = []

    let cts = new CancellationTokenSource()
    let conf =
        { defaultConfig with
            bindings = [binding]
            cancellationToken = cts.Token }

    let recordingWebPart webPart: WebPart =
        fun ctx ->
            requests <- requests @ [ ctx.request ]
            webPart ctx

    do this.Start()

    member this.Start () =
        let ready, server = startWebServerAsync conf (recordingWebPart webPart)

        Async.Start(server, cts.Token)

        ready
        |> Async.map (always ())
        |> Async.RunSynchronously

    member this.Stop () =
        cts.Cancel ()

    member this.LastRequest(): RecordedRequest option =
        requests
        |> List.tryHead
        |> Option.map (fun r -> { Method = r.method; Path = r.path })

    member this.LastHeaders(): (string * string) seq =
        requests
        |> List.tryHead
        |> Option.map (fun r -> r.headers)
        |> Option.defaultValue []
        |> List.toSeq

    interface IDisposable with
        member this.Dispose() =
            this.Stop()
