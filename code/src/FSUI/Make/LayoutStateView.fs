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


type Program<'layout, 'state, 'msg, 'node>(initialLayout, mkStore, show, render) as this =
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

    member _.doRender () =
        show view store layout |> render

    interface Runner<'layout,'state,'msg,'node> with
        member _.View = view
        member _.Render () = this.doRender ()
    
// let make
//     (initialLayout : 'layout)
//     (mkStore : (unit -> unit) -> Store<'state, 'msg>)
//     (show          : View<_,_,_> -> Store<'state, 'msg> -> 'layout -> 'node)
//     (render        : 'node -> unit)
//                    : Runner<'layout, 'state, 'msg, 'node>
//                    =
//         let mutable layout = initialLayout

//         let rec view = 
//             { new View<_,_,_> with
//                 member _.Layout layout' =
//                     layout <- layout'
//                     doRender ()
//             }

//         and store = mkStore doRender
//         and doRender () =
//             show view store layout |> render

//         { new Runner<'layout, 'state, 'msg, 'node> with
//             member _.View = view
//             member _.Render () = doRender ()
//         }

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
