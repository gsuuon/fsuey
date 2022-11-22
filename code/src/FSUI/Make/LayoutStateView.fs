module FSUI.Make.LayoutStoreView

open FSUI.Types

type Store<'state, 'msg> =
    abstract Dispatch : 'msg -> unit
    abstract State : 'state

type View<'layout, 'state, 'msg> =
    abstract Layout : 'layout -> unit
    abstract Dispatch : 'msg -> unit
    abstract State : 'state

type Runner<'layout, 'state, 'msg, 'node> =
    abstract View : View<'layout, 'state, 'msg>
    abstract Render : unit -> unit


type Component<'layout, 'state, 'msg, 'node>(
    // Class to contain the recursive references between view store and render
        initialLayout : 'layout,
        mkStore : (unit -> unit) -> Store<'state, 'msg>,
        show          : View<_,_,_> -> 'layout -> 'node,
        render        : 'node -> unit
    ) as this
    =

    let mutable layout = initialLayout
    let store : Store<'state, 'msg> = mkStore this.doRender
    let view =
        { new View<_,_,_> with
            member _.Layout layout' =
                layout <- layout'
                this.doRender ()
            member _.Dispatch x = store.Dispatch x
            member _.State = store.State
        }

    member _.doRender () = show view layout |> render

    interface Runner<'layout,'state,'msg,'node> with
        member _.View = view
        member _.Render () = this.doRender ()
    
let make initialLayout mkStore show render =
    Component(initialLayout, mkStore, show, render) :> Runner<_,_,_,_>
