module Sketch.App.View

open Design.Common
open Design.Common.Interfaces

open Design.LayoutElements

type Model = {
    greet : string
}

let view model =
    div [
        text model.greet
        // button ("Hey", fun () -> ())
    ]

module Extension =
    type Design.HostA.Elements.IVisualElement with
        member _.Foo () = ()

module GenericNode =
    // open type Elements<Design.HostA.Environment.Node>
    
    let env = Design.HostA.Environment.EnvA()
    let pos = Ordinal 0

    let initialModel = {
        greet = "Hi"
    }
    
    // view initialModel env pos
        // 1. The type 'Design.HostA.Environment.EnvA' is not compatible with the type 'IText<Design.HostA.Elements.VisualButtonElement>'
            // val div:
            //    children: List<'a -> Position -> 'node> ->
            //    env     : 'a       (requires :> IContainer<'node,'b> ) ->
            //    pos     : Position
            //           -> unit

            // * `'a` is Design.HostA.Environment.`EnvA`
            // * `'b` is Design.HostA.Elements.`VisualButtonElement`
            // * `'c` is `obj`
