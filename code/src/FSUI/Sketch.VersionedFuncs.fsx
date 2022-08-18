#load "../utilsTest.fsx"
open UtilsTest

/// Think of these as versioned functions which return their next version
type State<'T, 'R> = State of ('T -> 'R * State<'T, 'R>)

let pin (stateFn': State<_,_>) =
    let mutable stateFn = stateFn'

    fun x ->
        let (State fn) = stateFn
        let (v, next) = fn x
        stateFn <- next
        v

module NestedStateMachine =
    type IFinishes =
        abstract Finished : bool option

    type FooState =
        | Forward of int
        | Reverse of int
        | Success
        | Failure
        interface IFinishes with
            member this.Finished =
                match this with
                | Success -> Some true
                | Failure -> Some false
                | _ -> None

    type FooAction =
        | Increase
        | Decrease

    type BarState =
        | A of FooState
        | B of FooState

    module Without =
        let actionFoo =
            function
            | _, (Success as foo) | _, (Failure as foo) -> foo
            | _, Forward x | _, Reverse x when x = 10   -> Success
            | _, Forward x | _, Reverse x when x = 0    -> Failure

            | Increase, Forward x -> Forward (x + 1)
            | Increase, Reverse x -> Forward (x - 1)
            | Decrease, Forward x -> Forward (x - 1)
            | Decrease, Reverse x -> Forward (x + 1)
            
        let actionBar =
            let on (x: #IFinishes) success failure lift =
                match x.Finished with
                | Some true -> success x
                | Some false -> failure x
                | _ -> lift x

            function
            | action, BarState.A foo ->
                on (actionFoo (action, foo) )
                 <| fun _ -> BarState.B (Reverse 5)
                 <| fun _ -> BarState.A (Forward 5)
                 <| BarState.A
            | action, BarState.B foo ->
                on (actionFoo (action, foo) )
                 <| fun _ -> BarState.A (Forward 5)
                 <| fun _ -> BarState.B (Reverse 5)
                 <| BarState.B

        let runBar (bar: BarState) =
            let mutable state = bar
            fun action ->
                state <- actionBar (action, state)

    module With =
        // Is this interesting? We get to effectively skip defining a containing state machine type
        // but lose out on serialization and being able to talk about the containing state type
        // These are also not equivalently written
        let actFoo =
            function
            | _, (Success as foo) | _, (Failure as foo) -> foo
            | _, Forward x | _, Reverse x when x = 10   -> Success
            | _, Forward x | _, Reverse x when x = 0    -> Failure

            | Increase, Forward x -> Forward (x + 1)
            | Increase, Reverse x -> Forward (x - 1)
            | Decrease, Forward x -> Forward (x - 1)
            | Decrease, Reverse x -> Forward (x + 1)

        // TODO
        // I need to think about this more, I think this does allow writing
        // arbitrarily deep state machines much simpler. We don't need the containing state type if the
        // bottom type + others are able to fully describe the machine.
        // Would need an entry function which takes every state argument.
        let rec actBarA actFoo foo =
            State <| fun action ->
                let foo' = actFoo (action, foo)

                ( foo'
                , match foo' with
                  | Success -> actBarB actFoo (Reverse 5)
                  | Failure -> actBarA actFoo (Forward 5)
                  | x       -> actBarA actFoo x
                )
        and actBarB actFoo foo =
            State <| fun action ->
                let foo' = actFoo (action, foo)

                ( foo'
                , match foo' with
                  | Success -> actBarA actFoo (Forward 5)
                  | Failure -> actBarB actFoo (Reverse 5)
                  | x       -> actBarB actFoo x
                )
    
        let runBarA (foo: FooState) = actBarA actFoo foo |> pin

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
    sayFooOrBar "hi"     |> printfn "> %s"
    sayFooOrBar "world"  |> printfn "> %s"
    sayFooOrBar "swap"   |> printfn "> %s"
    sayFooOrBar "hey"    |> printfn "> %s"
    sayFooOrBar "planet" |> printfn "> %s"
    sayFooOrBar "swap"   |> printfn "> %s"
    sayFooOrBar "greets" |> printfn "> %s"
    sayFooOrBar "earth"  |> printfn "> %s"
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
    doOnce 2 |> printfn "> %i"
    doOnce 3 |> printfn "> %i"

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
    doOnce 2 |> printfn "> %i"
    doOnce 3 |> printfn "> %i"

    Did something with 1
    > 2
    > 2
    > 2
    > 2
    *)

module Stateful =
    let rec addMore n =
        State (fun x ->
            x + n, addMore (n + 1)
        )

    // Each call of addMore will add an increasing base number to the argument
    let add = addMore 0 |> pin

    add 0 |> printfn "> %i"
    add 0 |> printfn "> %i"
    add 0 |> printfn "> %i"
    add 5 |> printfn "> %i"
    (*
    > 0
    > 1
    > 2
    > 8
    *)
