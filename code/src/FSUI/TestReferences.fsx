#r "nuget: Unquote"

open System
open Swensen.Unquote

module _State =
    let mutable testCount = 0
    let mutable hasErrored = false

let that msg expr =
    try
        test expr
        _State.testCount <- _State.testCount + 1
    with
    | :? Swensen.Unquote.AssertionFailedException as e ->
        _State.hasErrored <- true
        let lines = e.Message.Split '\n'
        let last = lines.Length - 2 // last is empty after \n

        lines
         |> Array.iteri
            ( fun idx line ->
                if idx = 0 then
                    eprintf "Fails that: "
                    Console.ForegroundColor <- ConsoleColor.Red
                    eprintfn "%s" msg
                    Console.ResetColor()
                elif idx = last then
                    if not <| line.StartsWith "false" then
                        eprintfn "%s" line
                else
                    eprintfn "%s" line
            )

let exitCode () =
    if _State.hasErrored then
        exit 1
    else
        printfn "Passed %i tests" _State.testCount
        exit 0
