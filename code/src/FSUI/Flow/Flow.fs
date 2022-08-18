module FSUI.Flow

open System.Collections
open System.Collections.Generic

/// Represents potentially infinite work in progress with potential intermediate values and a final value
type Flow<'T, 'W> =
    | Done of 'T
    | Wait of Option<'W> * (Option<unit -> Flow<'T, 'W>>)
    interface IEnumerable<Option<'T>> with
        member this.GetEnumerator () =
            let mutable copy =
                match this with
                | Wait (None, _) ->
                    this
                | _ ->
                    Wait (None, Some (fun () -> this) ) // IEnumerator.Current needs to start _before_ the first element

            {
                new IEnumerator<Option<'T>> with
                    member _.Current =
                        match copy with
                        | Done x -> Some x
                        | _      -> None

                    member _.Dispose () = ()

                interface IEnumerator with
                    member _.Current =
                        match copy with
                        | Done x           -> x :> obj
                        | Wait (Some x, _) -> x :> obj
                        | Wait _           -> null

                    member _.MoveNext () =
                        match copy with
                        | Wait (_, Some work) ->
                            copy <- work()
                            true
                        | _ ->
                            false

                    member _.Reset () =
                        copy <- this

            }

    interface IEnumerable with
        member this.GetEnumerator () = (this :> IEnumerable<_>).GetEnumerator()

type FlowBuilder() =
    member _.Zero ()  = Done ()
    member _.Return x = Done x
    member _.Yield x  = Wait (Some x, None)
    member _.Delay fn = Wait (None, Some fn) // Change this.While if Delay changes

    member this.While (pred, mBody) =
        if pred() then
            match mBody with
            | Wait (_, Some body) ->
                this.Combine
                    ( match body() with
                      | Done _ -> Wait (None, None)
                          // a side-effect only body will be this.Zero()
                          // if we were to Combine a Done here, we would short circuit
                      | x      -> x
                          // we can still yield or return in while
                    , this.Delay(fun () -> this.While(pred, mBody) )
                    )
            | _ ->
                // While syntax sugar is:
                //   { while expr do cexpr } ==> builder.While(fun () -> expr, builder.Delay({ cexpr }))
                failwith "While called with something other than Delayed<'T> or Delay value changed"
        else
            Wait (None, None)

    member this.Combine (flow1: Flow<'T, 'W>, flow2: Flow<'T, 'W>) =
        match flow1, flow2 with
        | Wait (Some x, None), Wait(None, Some work)
                                 -> Wait (Some x, Some work) // yield
        | Done _, _              -> flow1              // return
        | Wait (None, None), _   -> flow2
        | Wait (x, None), _      -> Wait (x, Some (fun () -> flow2) )
        | Wait (x, Some work), _ -> Wait (x, Some (fun () -> this.Combine(work(), flow2) ) )

    member this.Bind (flow: Flow<'T,_>, next: 'T -> Flow<'T2,_>) =
        match flow with
        | Done x              -> next x
        | Wait (w, Some work) -> Wait (w, Some (fun () -> this.Bind(work(), next) ) )
        | Wait (w, None)      -> Wait (w, None) // Prevent constraining 'T2 to 'T

    /// Binds to a unit function that takes a resolve function, eventually continuing with the value resolve is called witkh
    member this.Bind (continuation: ('T -> unit) -> unit, next: 'T -> Flow<'T2, 'W>) =
        let mutable value = None
        let resolve x     = value <- Some x

        continuation resolve

        let rec waitForValue () =
            match value with
            | Some x -> next x
            | None   -> Wait(None, Some waitForValue)

        waitForValue()

module Flow =
    let asEnumerator (x: #IEnumerable) = x.GetEnumerator()
    let asSeq (flow: Flow<_,_>) = // TODO?
        let xs = asEnumerator flow
        seq {
            while xs.MoveNext() do
                yield xs.Current
        }

    let step =
        function
        | Wait (_, Some work) -> work()
        | flow          -> flow

let flow = FlowBuilder()
