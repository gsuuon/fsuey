#load "Flow.fs"

open FSUI.Flow

type MyWaitType = MyWaitType of string
type MyReturnType = MyReturnType of string

let foo : Flow<unit, MyWaitType> = 
    flow {
        let mutable x = 0
        printfn "\tfoo: start"
        yield (MyWaitType "wait one")
        while x < 3 do
            printfn "doing %i" x
            x <- x + 1
            yield (MyWaitType (sprintf "wait while %i" x))

        while x < 2 do
            printfn "doing %i" x
            x <- x + 1
            yield (MyWaitType (sprintf "wait while %i" x))
        printfn "\tfoo: end"
    }

let enumerate name (eventual: Flow<_,_>) =
    let mutable steps = 0
    printfn "\n\n----- Enumerating %s\n%A\n-----" name eventual

    for x in eventual do
        printfn "%i> %A" steps x
        steps <- steps + 1

    printfn "Took %i steps\n-----" steps

enumerate (nameof foo) foo

(*

:!dotnet fsi .\Flow\Flow.Test.fsx



----- Enumerating foo
Wait (None, Some <fun:foo@10>)
-----
0> None
	foo: start
1> None
doing 0
2> None
doing 1
3> None
doing 2
4> None
5> None
6> None
	foo: end
7> Some ()
Took 8 steps
-----
*)
