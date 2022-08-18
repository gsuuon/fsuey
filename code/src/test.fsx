open System
open System.Threading.Tasks
open System.Diagnostics
open System.IO

type ProcessOutput =
    { out  : string array
      err  : string array
      exit : int
      cmd  : string
      args : string list
      dir  : string
    }

type TestPassFail =
    | Passed
    | Failed of errors: string array

type TestResult =
    { passFail : TestPassFail
      name     : string
    }

let wait (t: Task<_>) =
    t.Wait()
    t.Result

let readStreamLines (sr: StreamReader) =
    let removeEmptyLast (xs: string array) =
        if xs[xs.Length - 1] = "" then
            xs[0..xs.Length - 2]
        else
            xs

    sr.ReadToEnd().Split Environment.NewLine |> removeEmptyLast

let runProcess dir (cmd: string) (args: string list) =
    let startInfo = new ProcessStartInfo()

    startInfo.RedirectStandardOutput <- true
    startInfo.RedirectStandardError <- true
    startInfo.UseShellExecute <- false
    startInfo.WorkingDirectory <- dir 
    startInfo.FileName <- cmd

    for arg in args do
        startInfo.ArgumentList.Add arg

    let proc = Process.Start startInfo

    task {
        do! proc.WaitForExitAsync()
        return
            { out = proc.StandardOutput |> readStreamLines
              err = proc.StandardError |> readStreamLines
              exit = proc.ExitCode
              cmd = cmd
              dir = dir
              args = args
            }
    }
    
let testName (testFullPath: string) = testFullPath.Replace(__SOURCE_DIRECTORY__, "")

let runTest testPath =
    printfn "Testing: %s" (testName testPath)
    runProcess __SOURCE_DIRECTORY__ "dotnet" ["fsi"; testPath]

let parseTestOutput (output: ProcessOutput) =
    let pathArg = output.args[1]
    let passFail =
        if output.exit <> 0 then
            Failed output.err
        else
            Passed

    { name = testName pathArg
      passFail = passFail
    }

let printTestResult result =
    let printColor color (msg: string) =
        Console.ForegroundColor <- color
        Console.Write msg
        Console.ResetColor()

    match result with 
    | { passFail = Passed
        name     = name }
        ->
        printColor ConsoleColor.Green "Passed: "
        Console.WriteLine name

    | { passFail = Failed errs
        name     = name }
        ->
        printColor ConsoleColor.Red "Failed: "
        Console.WriteLine name

        for er in errs do
            Console.WriteLine er

    result

let findAllTestScripts () =
    Directory.GetFiles (__SOURCE_DIRECTORY__, "*.Test.fsx", SearchOption.AllDirectories)

let testFailed x = Seq.exists (fun result -> result.passFail <> Passed) x

/// Run unit test scripts
/// Finds and runs scripts matching *.Test.fsx
/// Test scripts return an exit code 0 for success
let runTests () =
    findAllTestScripts ()
     |> Array.map runTest
     |> Array.map (wait >> parseTestOutput >> printTestResult)

if runTests() |> testFailed then
    exit 1
else
    exit 0
