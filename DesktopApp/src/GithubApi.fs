module DesktopApp.GithubApi

open FSharp.Data

type ActionStatus =
    | InProgress
    | Success
    | Failure
    | Unknown

type ActionRun =
    { Status: ActionStatus
      Message: string
      Project: string }

type ApiError =
    | ParseError of exn
    | ConnectionError of exn
    | NoWorkflow

type GithubInfo =
    { Owner: string
      Repo: string
      Token: string }

[<Literal>]
let private jsonSample = """
    {"workflow_runs": [
        {"status": "queued", "conclusion": null, "head_commit": {
            "message": "Initial commit"
        }},
        {"status": "queued", "conclusion": "completed", "head_commit": {
            "message": "Initial commit"
        }}
    ]}
"""

type WorkflowsJsonProvider = JsonProvider<jsonSample>

let private tryParse json =
    try Ok (WorkflowsJsonProvider.Parse json)
    with ex -> Error (ParseError ex)

let private fetchActionRunsJson info () =
    let headers = seq [
        "Authorization", $"token %s{info.Token}"
        "User-Agent", "ProjectMonitor/1.0"
    ]
    Http.AsyncRequestString(
        $"https://api.github.com/repos/%s{info.Owner}/%s{info.Repo}/actions/runs",
        [], headers
    )

let private jsonWorkflowToActionRun info (w: WorkflowsJsonProvider.WorkflowRun) =
    let status =
        match w.Status, w.Conclusion with
        | "completed", Some "success" -> Success
        | "completed", Some "failure" -> Failure
        | "completed", Some "timed_out" -> Failure
        | "completed", _ -> Unknown
        | _, _ -> InProgress

    { Status = status
      Message = w.HeadCommit.Message
      Project = $"%s{info.Owner}/%s{info.Repo}" }

let private jsonToActionRun info (json: WorkflowsJsonProvider.Root) =
    json.WorkflowRuns
    |> Array.tryHead
    |> Option.map ((jsonWorkflowToActionRun info) >> Ok)
    |> Option.defaultValue (Error NoWorkflow)

let fetchActionRuns info =
    async {
        let! result =
            fetchActionRunsJson info |> Try.async ConnectionError

        return result
        |> Result.bind tryParse
        |> Result.bind (jsonToActionRun info)
    }
