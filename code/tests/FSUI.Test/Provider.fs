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

type Env() as this =
    let swappers = Swappers()

    let mkElement (element: TestElement<_,_,_>) =
        Element.create
            (fun x -> x :> Visual)
            (swappers.Create ignore)
            { change = fun props visual -> visual // TODO
              update = element.update
              create = element.create
            }

    member val NewTexts = 0 with get, set
    member val NewButtons = 0 with get, set
    member val NewContainers = 0 with get, set

    interface IProvider with
        member _.Cache = swappers

    interface IText<Prop, Visual> with
        member val Text =
            mkElement {
                create = fun _ data ->
                    this.NewTexts <- this.NewTexts + 1
                    Text data
                update = fun _ data visual -> visual.Body <- data; visual
            }

    interface IButton<Prop, Visual * Keyed<string, (unit -> unit)>, Visual> with
        member val Button =
            mkElement {
                create = fun _ (child, Keyed (_, action) ) ->
                    this.NewButtons <- this.NewButtons + 1
                    Button (child,  action)

                update = fun last (child, keyedAction) visual ->
                    // Unfortunately since we shove everything into data, we don't know
                    // if action or child changed
                    let (child', keyedAction') = last

                    if child' <> child then
                        visual.Child <- child

                    if keyedAction' <> keyedAction then
                        let (Keyed (_, action) ) = keyedAction
                        visual.Action <- action

                    visual
            }

    interface IContainer<Prop, Visual> with
        member val Container =
            mkElement {
                create = fun _ data ->
                    this.NewContainers <- this.NewContainers + 1
                    Collection data
                update = fun _ data visual -> visual.Children <- data; visual
            }
