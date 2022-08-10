#load "Flow.fs"

open FSUI.Renderer.Unity.SampleApplication.Flow

type MyWaitType = MyWaitType of string
type MyReturnType = MyReturnType of string

let flow = FlowBuilder()

let foo : FlowState<unit, MyWaitType> = 
    flow {
        let mutable x = 0
        printfn "\tfoo: start"
        yield (MyWaitType "wait one")
        while x < 10 do
            printfn "doing %i" x
            x <- x + 1
            yield (MyWaitType (sprintf "wait while %i" x))

        while x < 12 do
            printfn "doing %i" x
            x <- x + 1
            yield (MyWaitType (sprintf "wait while %i" x))
        printfn "\tfoo: end"
    }

let enumerate name (flow: FlowState<_,_>) =
    let mutable steps = 0
    printfn "\n\n----- Enumerating %s\n%A\n-----" name flow

    for x in flow do
        printfn "%i> %A" steps x
        steps <- steps + 1

    printfn "Took %i steps\n-----" steps

enumerate (nameof foo) foo
