#r "nuget: Unquote"

open System
open Swensen.Unquote
open Swensen.Unquote.Operators

module private State =
    let mutable testCount = 0
    let mutable hasErrored = false

let private errored msg (e: exn) =
    State.hasErrored <- true

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
    
let that msg expr =
    try
        test expr
        State.testCount <- State.testCount + 1
    with
    | :? Swensen.Unquote.AssertionFailedException as e ->
        errored msg e

let show msg expr =
    try
        test expr
        State.testCount <- State.testCount + 1
        let reduction =
            expr
             |> reduceFully
             |> List.map decompile
             |> String.concat "\n"
        printf "Passes that: "
        Console.ForegroundColor <- ConsoleColor.Green
        printfn "%s" msg
        Console.ResetColor()
        printfn "%s" reduction
    with
    | :? Swensen.Unquote.AssertionFailedException as e ->
        errored msg e

let exitCode () =
    if State.hasErrored then
        exit 1
    else
        printfn "Passed %i tests" State.testCount
        exit 0
