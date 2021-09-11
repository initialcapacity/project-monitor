module DesktopApp.Config

open System.IO
open FSharp.Data

open DesktopApp.GithubApi

type private ConfigProvider = JsonProvider<"res/ConfigSample.json">

type ConfigLoadError =
    | FileReadError of exn
    | ParseError of exn

[<RequireQualifiedAccess>]
module Config =

    let private tryReadFile path =
        Try.async FileReadError (fun () -> async { return File.ReadAllText(path) })

    let private tryParse json =
        Try.async ParseError (fun () -> async { return ConfigProvider.Parse(json) })

    let private createRepositories (root: ConfigProvider.Root): GithubRepoWorkflow list =
        root.Workflows
        |> Array.toList
        |> List.map (fun r -> { Owner = r.Owner; Repo = r.Repository; Workflow = r.Workflow; Token = r.Token })

    let tryLoad path =
        path
        |> tryReadFile
        |> AsyncResult.bind tryParse
        |> AsyncResult.map createRepositories
