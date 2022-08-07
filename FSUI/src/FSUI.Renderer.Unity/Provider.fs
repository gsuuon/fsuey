﻿namespace FSUI.Renderer.Unity

open UnityEngine
open UnityEngine.UIElements

open FSUI.Elements.Interfaces

open FSUI.Renderer.Cache
open FSUI.Renderer.Provider
open FSUI.Renderer.Element
open FSUI.Renderer.Unity


[<AutoOpen>]
module Types =
    type ScreenProp =
        | Class of string

    type ScreenProps = ScreenProp list
    type ScreenElement = VisualElement


    type WorldElement = GameObject
    type WorldProps =
        | Start of System.Action
        | Update of System.Action
        | Child of child: GameObject

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

    interface IGameObject<WorldProps, WorldElementType, WorldElement> with
        member val GameObject =
            world {
                create = fun p (d: WorldElementType) -> d.Create()
                update = fun p' d' e p d             ->
                    if d' <> d then
                        GameObject.Destroy e
                        d.Create()
                    else
                        e
            }

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
                    ScreenElement.addChild (Button (System.Action action)) child
                update = fun p' ((_, action')) e p ((child, action)) ->
                    if action'.GetType() <> action.GetType() then // TODO do this better
                        e.remove_clicked action'
                        e.add_clicked action

                    ScreenElement.addChild e child
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
