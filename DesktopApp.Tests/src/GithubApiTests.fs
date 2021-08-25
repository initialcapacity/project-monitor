module DesktopApp.Tests.GithubApiTests

open DesktopApp.Tests.Support.ApiTesting
open DesktopApp.GithubApi
open Expecto
open Suave

let validResponse = """
{
  "workflow_runs": [
    {
      "status": "queued",
      "conclusion": null,
      "head_commit": {
        "message": "Initial commit"
      }
    },
    {
      "status": "queued",
      "conclusion": "completed",
      "head_commit": {
        "message": "Initial commit"
      }
    }
  ]
}
"""

[<Tests>]
let tests =
    testList "GithubApi Tests" [
        testCase "fetching status" <| fun _ ->
            use api = new TestApi(WebPart.json HTTP_200 validResponse)

            let asyncResult =
                fetchActionRuns
                    "http://localhost:8080"
                    { Owner = "dam5s"
                      Repo = "foobar"
                      Token = "the-secret-token-0023" }

            let result = Async.RunSynchronously asyncResult

            let expectedBuild =
                { Status = InProgress
                  Message = "Initial commit"
                  Project = "dam5s/foobar" }

            let expectedRequest =
                { Method = HttpMethod.GET
                  Path = "/repos/dam5s/foobar/actions/runs" }

            let expectedAuthHeader =
                ("authorization", "token the-secret-token-0023")

            Expect.equal result (Ok expectedBuild) "Expected result"
            Expect.equal (api.LastRequest()) (Some expectedRequest) "Expected request"
            Expect.contains (api.LastHeaders()) expectedAuthHeader "Expected authorization"
    ]
