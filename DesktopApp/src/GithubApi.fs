module DesktopApp.GithubApi

open FSharp.Data

type ActionStatus =
    | InProgress
    | Success
    | Failure
    | Unknown

type BuildRun =
    { Status: ActionStatus
      Message: string
      Project: string
      Workflow: string }

type ApiError =
    | ParseError of exn
    | ConnectionError of exn
    | NoWorkflow

type GithubRepoWorkflow =
    { Owner: string
      Repo: string
      Workflow: string
      Token: string }

type WorkflowsJsonProvider = JsonProvider<"../DesktopApp/res/WorkflowRuns.json">

let private tryParse json =
    try Ok (WorkflowsJsonProvider.Parse json)
    with ex -> Error (ParseError ex)

let private jsonWorkflowToActionRun workflow (w: WorkflowsJsonProvider.WorkflowRun) =
    let status =
        match w.Status, w.Conclusion with
        | "completed", Some "success" -> Success
        | "completed", Some "failure" -> Failure
        | "completed", Some "timed_out" -> Failure
        | "completed", _ -> Unknown
        | _, _ -> InProgress

    { Status = status
      Message = w.HeadCommit.Message
      Project = $"%s{workflow.Owner}/%s{workflow.Repo}"
      Workflow = workflow.Workflow }

let private jsonToActionRun (workflow: GithubRepoWorkflow) (json: WorkflowsJsonProvider.Root) =
    json.WorkflowRuns
    |> Array.toSeq
    |> Seq.filter (fun run -> run.Name = workflow.Workflow)
    |> Seq.tryHead
    |> Option.map ((jsonWorkflowToActionRun workflow) >> Ok)
    |> Option.defaultValue (Error NoWorkflow)

let fetchActionRuns baseUrl workflow =
    let headers = seq [
        "Authorization", $"token %s{workflow.Token}"
        "User-Agent", "ProjectMonitor/1.0"
    ]

    let doRequest () =
        Http.AsyncRequestString(
            $"%s{baseUrl}/repos/%s{workflow.Owner}/%s{workflow.Repo}/actions/runs",
            [], headers
        )

    async {
        let! result =
            doRequest |> Try.async ConnectionError

        return result
        |> Result.bind tryParse
        |> Result.bind (jsonToActionRun workflow)
    }
