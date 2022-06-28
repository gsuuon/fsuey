module Sketch.App.View

open Design.Common
open Design.Common.Interfaces
open Design.HostA
open Design.LayoutElements

type Model = {
    greet : string
}

let view model =
    div [
        text model.greet
        button ("Hey", fun () -> ())
    ]

view { greet = "hi" } (EnvA()) (Ordinal 0)
    // TODO-next
    // how do containers work?
    // how do parent-child attach?
    // container must specify that children return an attacher
    // container must return a attachee
    // let x = container result
    // x.AddChild (child result)
    // elements can't return Unit, but they don't need to return a collection type
    // or their underlying object. just something the parent can use to attach.

