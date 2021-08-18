﻿module DesktopApp.GithubApi

open FSharp.Data

type ActionStatus =
    | InProgress
    | Success
    | Failure
    | Unknown

type BuildRun =
    { Status: ActionStatus
      Message: string
      Project: string }

type ApiError =
    | ParseError of exn
    | ConnectionError of exn
    | NoWorkflow

type GithubRepository =
    { Owner: string
      Name: string
      Token: string }

type WorkflowsJsonProvider = JsonProvider<"../DesktopApp/res/WorkflowRuns.json">

let private tryParse json =
    try Ok (WorkflowsJsonProvider.Parse json)
    with ex -> Error (ParseError ex)

let private jsonWorkflowToActionRun repo (w: WorkflowsJsonProvider.WorkflowRun) =
    let status =
        match w.Status, w.Conclusion with
        | "completed", Some "success" -> Success
        | "completed", Some "failure" -> Failure
        | "completed", Some "timed_out" -> Failure
        | "completed", _ -> Unknown
        | _, _ -> InProgress

    { Status = status
      Message = w.HeadCommit.Message
      Project = $"%s{repo.Owner}/%s{repo.Name}" }

let private jsonToActionRun repo (json: WorkflowsJsonProvider.Root) =
    json.WorkflowRuns
    |> Array.tryHead
    |> Option.map ((jsonWorkflowToActionRun repo) >> Ok)
    |> Option.defaultValue (Error NoWorkflow)

let fetchActionRuns baseUrl repo =
    let headers = seq [
        "Authorization", $"token %s{repo.Token}"
        "User-Agent", "ProjectMonitor/1.0"
    ]

    let doRequest () =
        Http.AsyncRequestString(
            $"%s{baseUrl}/repos/%s{repo.Owner}/%s{repo.Name}/actions/runs",
            [], headers
        )

    async {
        let! result =
            doRequest |> Try.async ConnectionError

        return result
        |> Result.bind tryParse
        |> Result.bind (jsonToActionRun repo)
    }
