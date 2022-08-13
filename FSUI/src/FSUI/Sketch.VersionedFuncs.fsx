#load "./TestReferences.fsx"
open TestReferences

/// Think of these as versioned functions which return their next version
type State<'T, 'R> = State of ('T -> 'R * State<'T, 'R>)

let pin (stateFn': State<_,_>) =
    let mutable stateFn = stateFn'

    fun x ->
        let (State fn) = stateFn
        let (v, next) = fn x
        stateFn <- next
        v

module DataChanges =
    let reverse =
        Seq.rev >> Seq.map string >> String.concat ""

    let rec sayFoo name =
        State (fun msg ->
            sprintf "%s says %s as foo" name msg
            , if msg = "swap" then
                sayBar (name |> reverse)
              else
                sayFoo name
            )
    and sayBar name =
        State (fun msg ->
            sprintf "%s says %s as bar" name msg
            , if msg = "swap" then
                sayFoo (name |> reverse)
              else
                sayBar name
        )

    let sayFooOrBar = sayFoo "inti" |> pin
    (*
    sayFooOrBar "hi" |> printfn "> %s"
    sayFooOrBar "world" |> printfn "> %s"
    sayFooOrBar "swap" |> printfn "> %s"
    sayFooOrBar "hey" |> printfn "> %s"
    sayFooOrBar "planet" |> printfn "> %s"
    sayFooOrBar "swap" |> printfn "> %s"
    sayFooOrBar "greets" |> printfn "> %s"
    sayFooOrBar "earth" |> printfn "> %s"
    > inti says hi as foo
    > inti says world as foo
    > inti says swap as foo
    > itni says hey as bar
    > itni says planet as bar
    > itni says swap as bar
    > inti says greets as foo
    > inti says earth as foo
    *)

    module Test =
        // Since state is an argument, testing is trivial -- nothing special needs to be done to mock
        that "it says a name and saying"
            <@ sayFoo "inti" |> pin <| "thing" = "inti says thing as foo" @>

        that "it says a name and saying with bar"
            <@ sayBar "inti" |> pin <| "thing" = "inti says thing as bar" @>

        // To test that we get to the correct next state:
        let sayer = sayFoo "inti" |> pin

        that "swap works like any other message"
            <@ sayer "swap" = "inti says swap as foo" @>

        that "after 'swap' it says bar with name reversed"
            <@ sayer "hi" = "itni says hi as bar" @>

module Memoize1 =
    let rec value x =
        State (fun _ -> x, value x)

    let once fn =
        State
            (fun x ->
                let y = fn x
                y, value y
            )

    let doThing x =
        printfn "Did something with %A" x
        x + 1

    let doOnce = once doThing |> pin

    (*
    doOnce 1 |> printfn "> %i"
    doOnce 1 |> printfn "> %i"
    doOnce 1 |> printfn "> %i"
    doOnce 1 |> printfn "> %i"

    Did something with 1
    > 2
    > 2
    > 2
    > 2
    *)

module Memoize2 =
    let rec once fn =
        State
            (fun x ->
                let value = fn x
                value, once (fun _ -> value)
            )

    let doThing x =
        printfn "Did something with %A" x
        x + 1

    let doOnce = once doThing |> pin

    (*
    doOnce 1 |> printfn "> %i"
    doOnce 1 |> printfn "> %i"
    doOnce 1 |> printfn "> %i"
    doOnce 1 |> printfn "> %i"


    Did something with 1
    > 2
    > 2
    > 2
    > 2
    *)

module Stateful =
    let rec addMore n =
        State (fun x -> x + n, addMore (n + 1) )

    let add = addMore 1 |> pin

    (*
    add 1 |> printfn "> %i"
    add 1 |> printfn "> %i"
    add 1 |> printfn "> %i"
    add 1 |> printfn "> %i"
    add 1 |> printfn "> %i"

    > 2
    > 3
    > 4
    > 5
    > 6
    *)
