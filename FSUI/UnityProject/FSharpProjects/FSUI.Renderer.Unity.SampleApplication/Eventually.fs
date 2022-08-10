namespace FSUI.Renderer.Unity.SampleApplication

open System.Collections
open System.Collections.Generic

// TODO rename this from Eventually
module Eventually =
    /// Represents potentially infinite work in progress with potential intermediate values and a final value
    type Eventually<'T, 'W> =
        | Done of 'T
        | Wait of Option<'W> * (Option<unit -> Eventually<'T, 'W>>)
        interface IEnumerable<Option<'T>> with
            member this.GetEnumerator () =
                let mutable copy = Wait (None, Some (fun () -> this) ) // IEnumerator.Current needs to start _before_ the first element
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

    let asEnumerator (x: #IEnumerable) = x.GetEnumerator()

    let step =
        function
        | Wait (_, Some work) -> work()
        | eventually          -> eventually

    module Computation =
        type EventuallyBuilder() =
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

            member this.Combine (eventually1: Eventually<'T, 'W>, eventually2: Eventually<'T, 'W>) =
                match eventually1, eventually2 with
                | Wait (Some x, None), Wait(None, Some work)
                                         -> Wait (Some x, Some work) // yield
                | Done x, _              -> eventually1              // return
                | Wait (None, None), _   -> eventually2
                | Wait (x, None), _      -> Wait (x, Some (fun () -> eventually2) )
                | Wait (x, Some work), _ -> Wait (x, Some (fun () -> this.Combine(work(), eventually2) ) )

            member this.Bind (eventually: Eventually<'T,_>, next: 'T -> Eventually<'T2,_>) =
                match eventually with
                | Done x              -> next x
                | Wait (w, Some work) -> Wait (w, Some (fun () -> this.Bind(work(), next) ) )
                | Wait (w, None)      -> Wait (w, None) // Prevent constraining 'T2 to 'T

            /// Binds to a unit function that takes a resolve function, eventually continuing with the value resolve is called witkh
            member this.Bind (continuation: ('T -> unit) -> unit, next: 'T -> Eventually<'T2, 'W>) =
                let mutable value = None
                let resolve x     = value <- Some x

                continuation resolve

                let rec waitForValue () =
                    match value with
                    | Some x -> next x
                    | None   -> Wait(None, Some waitForValue)

                waitForValue()
            
    let eventually = Computation.EventuallyBuilder()
