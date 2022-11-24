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
        mkStore       : (unit -> unit) -> Store<'state, 'msg>,
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
    | DoUpdate of 'state
    | NoUpdate

type Updater<'state> = 'state -> ShouldUpdate<'state>

let noop _ = ()

let mkStoreByIngest
    (initialize: (Updater<'state> -> unit) -> unit)
    (ingest: (Updater<'state> -> unit) -> 'msg -> unit)
    (initialState: 'state)
    (render: unit -> unit)
    =
    let mutable state = initialState

    let set updater =
        match updater state with
        | DoUpdate state' ->
            state <- state'
            render ()
        | NoUpdate ->
            ()
    
    initialize set

    { new Store<_,_> with
        member _.State = state
        member this.Dispatch msg = ingest set msg
    }

let make
    (initialLayout : 'layout)
    (mkStore       : (unit -> unit) -> Store<'state, 'msg>)
    (show          : View<_,_,_> -> 'layout -> 'node)
    (render        : 'node -> unit)
    =
    Component (initialLayout, mkStore, show, render)
