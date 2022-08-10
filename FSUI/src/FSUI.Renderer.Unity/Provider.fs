namespace FSUI.Renderer.Unity

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
    type ScreenProp =
        | Class of string

    type ScreenProps = ScreenProp list
    type ScreenElement = VisualElement

    type WorldElement = GameObject
    type WorldProp =
        | Start of System.Action
        | Update of System.Action
        | Child of child: GameObject
    type WorldProps = WorldProp list

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

[<AutoSerializable(false)>] // Unity will try to serialize this and hit depth limits
type UnityProvider() =
    let ulog x = Debug.Log x
    let swappers = Swappers()

    let screen (x: IElement<'props, 'data, 'visual>) =
        create (fun x -> x :> VisualElement) (swappers.Create Graph.remove) x

    let world (x: IElement<'props, 'data, WorldElement>) =
        create id (swappers.Create Graph.remove) x

    let polyString = // Example of multiple specializations of an interface (can't `member val`)
        screen {
            create = fun p d         -> Label d
            update = fun p' d' e p d -> e.text <- d; e
        }

    interface IProvider with
        member _.Cache = swappers

    interface IText<ScreenProps, ScreenElement> with
        member val Text =
            screen {
                create = fun p d         -> Label d
                update = fun p' d' e p d -> e.text <- d; e
            }

    interface IContainer<ScreenProps, ScreenElement> with
        member val Container =
            screen {
                create = fun p d         -> Graph.addChildren (d, new VisualElement())
                update = fun p' d' e p d -> Graph.addChildren (d, e)
                    // Assumes adding same element is no-op
                    // swapper's swap should take care of removing stale children
            }

    interface IGameObject<WorldElement.Hooks.Prop list, string, WorldElement> with
        member val GameObject =
            WorldElement.Element.create GameObject swappers

    member val Prefab : RendersElement<WorldElement.Hooks.Prop list, string, WorldElement> =
        WorldElement.Element.create
            (Resources.Load<GameObject> >> GameObject.Instantiate<GameObject>)
            swappers

    // TODO Can I avoid rendering an empty VisualElement to contain game objects?
    interface IJoinContain<ScreenProps, GameObject list, ScreenElement> with
        member val JoinContain =
            screen {
                create = fun p d         -> VisualGameObjectContainer d
                update = fun p' d' e p d -> e // TODO
            }
    
    interface IButton<ScreenProps, ScreenElement * (unit -> unit), ScreenElement> with
        member val Button =
            screen {
                create = fun p ((child, action)) ->
                    ScreenNode.addChild (Button (System.Action action)) child
                update = fun p' ((_, action')) e p ((child, action)) ->
                    if action'.GetType() <> action.GetType() then // TODO do this better
                        e.remove_clicked action'
                        e.add_clicked action

                    ScreenNode.addChild e child
            }

    // NOTE These are just examples of multiple specializations
    interface IPoly<ScreenProps, obj, VisualElement> with
        member val Poly =
            screen {
                create = fun p d         -> Label (string d)
                update = fun p' d' e p d -> e.text <- (string d); e
            }

    interface IPoly<ScreenProps, string, VisualElement> with
        member _.Poly = polyString
