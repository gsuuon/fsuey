namespace FSUI.Renderer.Unity

open System.Collections.Generic

open UnityEngine
open UnityEngine.UIElements

open FSUI.Elements.Interfaces

open FSUI.Renderer.Cache
open FSUI.Renderer.Provider
open FSUI.Renderer.Element
open FSUI.Renderer.Unity

module Flow =
    open FSUI.Flow

    type UnityFlow() =
        inherit FlowBuilder() 
        member _.Run x = Flow.asEnumerator x

    let flow = UnityFlow()

module Renderer =
    let mount<'T when 'T : (new : unit -> 'T) and 'T :> IProvider > (document: UIDocument) =
        let env = new 'T()

        fun view ->
            view env Root |> document.rootVisualElement.Add
            env.Cache.Swap()

    
[<AutoOpen>]
module Types =
    [<CustomEquality; NoComparison>]
    type Keyed<'K, 'T when 'K : equality> = Keyed of 'K * 'T
        with
        interface System.IEquatable<Keyed<'K, 'T>> with
            member this.Equals other =
                let (Keyed (key, _)) = this
                let (Keyed (key', _)) = other
                key = key'

    type ScreenProp = ScreenElement.Props.Prop
    type ScreenElement = VisualElement

    type WorldElement = GameObject

    type WorldElementType =
        | Empty of name: string
        | Prefab of resourcePath: string
        with
        member this.Create () =
            match this with
            | Empty name -> GameObject(name)
            | Prefab path ->
                let prefab = Resources.Load<GameObject>(path)
                GameObject.Instantiate<GameObject>(prefab, Vector3.zero, Quaternion.identity)

    // TODO move this somewhere
    type VisualGameObjectContainer(children: GameObject list) =
        inherit VisualElement()

        member this.RemoveFromHierarchy () =
            children |> List.iter (Graph.remove)

            base.RemoveFromHierarchy()

type ScreenElementRecord<'d, 'v> = {
    create : 'd -> 'v
    update : 'd -> 'd -> 'v -> 'v
}

[<AutoSerializable(false)>] // Unity will try to serialize this and hit depth limits
type UnityProvider() =
    let ulog x = Debug.Log x
    let swappers = Swappers()

    let changeScreenProps (changes: FSUI.Difference.Changes<ScreenProp>) (el: #VisualElement) =
        changes.created |> ScreenElement.Props.applyProps el
        changes.removed |> ScreenElement.Props.unapplyProps el
        el

    let screen (el: ScreenElementRecord<'data, 'visual>) =
        create
            (fun x -> x :> VisualElement)
            (swappers.Create Graph.remove) 
            { create =
                fun props data ->
                    let visual = el.create data
                    props |> ScreenElement.Props.applyProps visual
                    visual
              update = el.update
              change = changeScreenProps
            }

    let world (x: IElement<'props, 'data, WorldElement>) =
        create id (swappers.Create Graph.remove) x

    let polyString = // Example of multiple specializations of an interface (can't `member val`)
        screen {
            create = fun d    -> Label d
            update = fun d' d e -> e.text <- d; e
        }

    interface IProvider with
        member _.Cache = swappers

    interface IText<ScreenProp, ScreenElement> with
        member val Text =
            screen {
                create = fun d    -> Label d
                update = fun d' d e -> e.text <- d ;e
            }

    interface IContainer<ScreenProp, ScreenElement> with
        member val Container =
            screen {
                create = fun d    -> Graph.addChildren (d, new VisualElement())
                update = fun d' d e -> Graph.addChildren (d, e)
                    // Assumes adding same element is no-op
                    // swapper's swap should take care of removing stale children
            }

    interface IGameObject<WorldElement.Hooks.Prop, string, WorldElement> with
        member val GameObject =
            WorldElement.Element.create GameObject swappers

    member val Prefab : RendersElement<WorldElement.Hooks.Prop, string, WorldElement> =
        WorldElement.Element.create
            (Resources.Load<GameObject> >> GameObject.Instantiate<GameObject>)
            swappers

    // TODO Can I avoid rendering an empty VisualElement to contain game objects?
    interface IJoinContain<ScreenProp, GameObject list, ScreenElement> with
        member val JoinContain =
            screen {
                create = fun d      -> VisualGameObjectContainer d
                update = fun d' d e -> e // TODO
            }
    
    interface IButton<ScreenProp, ScreenElement * Keyed<string, unit -> unit>, ScreenElement> with
        member val Button =
            screen {
                create = fun ((child, Keyed (_, action))) ->
                    ScreenNode.addChild (Button (System.Action action)) child
                update = fun ((_, Keyed (_, action'))) ((child, Keyed (_, action))) e ->
                    e.remove_clicked action'
                    e.add_clicked action

                    ScreenNode.addChild e child
            }

    // NOTE These are just examples of multiple specializations
    interface IPoly<ScreenProp, obj, VisualElement> with
        member val Poly =
            screen {
                create = fun d      -> Label (string d)
                update = fun d' d e -> e.text <- (string d); e
            }

    interface IPoly<ScreenProp, string, VisualElement> with
        member _.Poly = polyString
