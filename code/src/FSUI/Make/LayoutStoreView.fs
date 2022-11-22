module FSUI.Make.LayoutStoreView

open FSUI.Types

type Store<'state, 'msg> =
    abstract Dispatch : 'msg -> unit
    abstract State : 'state

type View<'layout, 'state, 'msg> =
    abstract Layout : 'layout -> unit
    abstract Dispatch : 'msg -> unit
    abstract State : 'state

type Component<'layout, 'state, 'msg, 'node>(
    // Class to contain the recursive references between view store and render
        initialLayout : 'layout,
        mkStore : (unit -> unit) -> Store<'state, 'msg>,
        show          : View<_,_,_> -> 'layout -> 'node,
        render        : 'node -> unit
    ) as this
    =

    let mutable layout = initialLayout
    let store : Store<'state, 'msg> = mkStore this.Render
    let view =
        { new View<_,_,_> with
            member _.Layout layout' =
                layout <- layout'
                this.Render ()
            member _.Dispatch x = store.Dispatch x
            member _.State = store.State
        }

    member _.View = view
    member _.Render () = show view layout |> render

type ShouldUpdate<'state> =
    | Update of 'state
    | NoUpdate

let mkStoreByIngest ingest initialState render =
    let mutable state = initialState

    let set updater =
        match updater state with
        | Update state' ->
            state <- state'
            render()
        | NoUpdate ->
            ()

    { new Store<_,_> with
        member _.State = state
        member this.Dispatch msg = ingest msg set
    }

let make initialLayout mkStore show render =
    Component(initialLayout, mkStore, show, render)

module App =
    module Domain =
        type Msg =
            | Increment
            | Decrement

        type State =
            { x : int }

    open Domain

    let text _ = ()
    let button _ = ()

    type Layout =
        | A
        | B
        | C

    let viewMain (view: View<_,_,_>) =
        function
        | A -> text view.State.x
        | B -> button (fun _ -> view.Dispatch Increment)
        | C -> button (fun _ -> view.Layout B)

    let myStore init render =
        let mutable state = init

        async {
            while true do
                do! Async.Sleep 1000
                state <- { state with x = state.x + 1 }
                render ()
        } |> Async.Start

        { new Store<_,_> with
            member _.State = state
            member this.Dispatch msg = 
                match msg with
                | Increment ->
                    async {
                        if this.State.x < 10 then
                            state <- { state with x = this.State.x + 1 }
                            render ()
                        else
                            ()
                    } |> Async.Start
                | Decrement ->
                    ()
        }

    let myIngestStore =
        mkStoreByIngest
         <| fun msg update ->
                match msg with
                | Increment ->
                    async {
                        update
                         <| function
                            | { x = x } when x < 10 ->
                                Update { x = x + 1 }
                            | _ ->
                                NoUpdate
                    } |> Async.Start
                | Decrement ->
                    ()

    let runner renderer = make A (myStore { x = 0 }) viewMain renderer
