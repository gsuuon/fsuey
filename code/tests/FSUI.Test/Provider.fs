namespace FSUI.Test.Provider

open FSUI.Test.Host

open FSUI.Types
open FSUI.Renderer
open FSUI.Renderer.Provider
open FSUI.Renderer.Cache
open FSUI.Elements.Interfaces

type Prop =
    | Class of string

type TestElement<'p, 'd, 'v> =
    { create : 'p collection -> 'd -> 'v
      update : 'd -> 'd -> 'v -> 'v
    }

type Env() =
    let swappers = Swappers()

    let mkElement (element: TestElement<_,_,_>) =
        Element.create
            (fun x -> x :> Visual)
            (swappers.Create ignore)
            { change = fun props visual -> visual // TODO
              update = element.update
              create = element.create
            }

    interface IProvider with
        member _.Cache = swappers

    interface IText<Prop, Visual> with
        member val Text =
            mkElement {
                create = fun _ data -> Text data
                update = fun _ data visual -> visual.Content <- data; visual
            }

    interface IContainer<Prop, Visual> with
        member val Container =
            mkElement {
                create = fun _ data -> Collection data
                update = fun _ data visual -> visual.Children <- data; visual
            }
