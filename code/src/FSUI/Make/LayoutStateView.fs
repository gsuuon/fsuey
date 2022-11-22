module FSUI.Make.LayoutStoreView

open FSUI.Types

type Store<'state, 'msg> =
    abstract Dispatch : 'msg -> unit
    abstract State : 'state

type View<'layout, 'state, 'msg> =
    abstract Dispatch : 'msg -> unit
    abstract State : 'state
    abstract Layout : 'layout -> unit

type Runner<'layout, 'state, 'msg, 'node> =
    abstract View : View<'layout, 'state, 'msg>
    abstract Render : unit -> unit

let make
    (initialLayout : 'layout)
    (initialState : 'state)
    (ingest : 'msg -> 'state -> ( ('state -> 'state) -> unit) -> unit)
    (show          : View<'layout, 'state, 'msg> -> 'layout -> 'node)
    (render        : 'node -> unit)
                   : Runner<'layout, 'state, 'msg, 'node>
                   =
        let mutable layout = initialLayout
        let mutable state = initialState

        let rec view = 
            { new View<'layout, 'state, 'msg> with
                member _.Dispatch msg = ingest msg state set
                member _.State = state
                member _.Layout layout' =
                    layout <- layout'
                    doRender ()
            }

        and set update =
            state <- update state
            doRender()

        and doRender () =
            show view layout |> render

        { new Runner<'layout, 'state, 'msg, 'node> with
            member _.View = view
            member _.Render () = doRender ()
        }

module App =
    module Domain =
        type Msg =
            | Increment
            | Decrement

        type State =
            { x : int
            }

    open Domain

    let text _ = ()
    let button _ = ()

    type Layout =
        | A
        | B
        | C

    let showMain (vm: View<_,_,_>) =
        function
        | A -> text vm.State.x
        | B -> button (fun _ -> vm.Dispatch Increment)
        | C -> button (fun _ -> vm.Layout B)
