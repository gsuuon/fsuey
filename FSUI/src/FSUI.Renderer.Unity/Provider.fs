namespace FSUI.Renderer.Unity

open UnityEngine
open UnityEngine.UIElements

open FSUI.Renderer.Provider
open FSUI.Renderer.Element
open FSUI.Elements.Interfaces

open FSUI.Renderer.Unity.Interfaces
open FSUI.Renderer.Unity.Hierarchy

open type UnityNode

type UnityProp =
    | Class of string

type UnityProps = List<UnityProp>

[<AbstractClass>]
type ScreenSpace<'data, 'element when 'element :> VisualElement>(provider: Provider) =
    inherit Element<'data, UnityProps, 'element, VisualElement>(
        (fun (x: 'element) -> x :> VisualElement),
        provider.Cache.Create UnityNode.remove
    )

type UnityBehavior =
    | Start of System.Action
    | Update of System.Action
    | Child of child: GameObject

type UnityGameObject =
    | New of name: string
    | Prefab of resourcePath: string
    with
    member this.Create () =
        match this with
        | New name -> GameObject(name)
        | Prefab path ->
            let prefab = Resources.Load<GameObject>(path)
            GameObject.Instantiate<GameObject>(prefab, Vector3.zero, Quaternion.identity)

[<AbstractClass>]
type WorldSpace<'data>(provider: Provider) =
    inherit Element<'data, UnityBehavior list, GameObject, GameObject>(
        id,
        provider.Cache.Create GameObject.Destroy
    )


// TODO can I just use this somehow without inheriting visualelement?
// can I do that without a DU wrapping gameobject + visualelement and switching for every element?
// visual element isn't based on interfaces
type GameObjectCollection = GameObjectCollection of children: GameObject list
    with 
    static member Destroy (GameObjectCollection children) =
        children
         |> List.iter (GameObject.Destroy)

// TODO move this somewhere
type VisualGameObjectContainer(children: GameObject list) =
    inherit VisualElement()

    member this.RemoveFromHierarchy () =
        children
         |> List.iter (GameObject.Destroy)

        base.RemoveFromHierarchy()


module Element =
    open FSUI.Renderer.Cache


    type IElement<'data, 'props, 'visual> =
        abstract member Create: 'data -> 'props -> 'visual
        abstract member Update: 'data -> 'props -> 'visual -> 'data -> 'props -> 'visual

    type ElementRecord<'d, 'p, 'v> =
        {
            create : 'd -> 'p -> 'v
            update : 'd -> 'p -> 'v -> 'd -> 'p -> 'v
        }
        interface IElement<'d, 'p, 'v> with
            member this.Create a b = this.create a b
            member this.Update a b c d e = this.update a b c d e

    let create wrap (cache: Swapper<_,_,_,_>) (element: #IElement<_,_,_>) =
        fun (data: 'data) (props: 'props) (pos: Position) ->
            let (exists, stale) = cache.Stale.Remove pos
                // for some reason normal match with syntax doesn't work here
            
            match exists, stale with
            | true, (cachedData, cachedProps, cachedVisual) ->
                element.Update cachedData cachedProps cachedVisual data props
            | _ ->
                element.Create data props
            |> fun el -> cache.Fresh.Add(pos, (data, props, el)); el
            |> wrap

// module Foo =
//     type Provider () =
//         let screenSpace x = element base.mkCache x
//         interface IText<..> with
//             member val Text =
//                 screenSpace {
//                     create = ..
//                     update = ..
//                 }


//     module Views =
//         let text props data =
//             fun (env: #IText) pos ->
//                 env.Text props data pos


open FSUI.Renderer.Cache
open Element

type UnityProvider() as this =
    inherit Provider() // TODO all this really does is create a swapper - lets just remove it for now

    let swappers = Swappers()

    let ulog x = Debug.Log x
    let screen x = Element.create (fun (x: #VisualElement) -> x :> VisualElement) (swappers.Create UnityNode.remove) x

    let intScreenElement =
        screen
            { new IElement<_,_,_> with
                member _.Create (x: int) (y: UnityProps) = Label (string x)
                member _.Update  x y el x2 y2 = el
            }

    let intScreenElement2 =
        screen {
            create = fun (x: int) (y: UnityProps) -> Label (string x)
            update = fun x y el x2 y2 -> el
        }

    let intElement =
        { new Element<int, unit, Label, VisualElement>((fun x -> x :> VisualElement), base.Cache.Create (UnityNode.remove)) with
            member _.Create x () = Label <| string x
            member _.Update a b el d e =
                el.text <- string d
                el
        }

    // let text2 =
    //     UnityProvider.screenSpace this {
    //         create = fun data props ->
    //             let x = Button()
    //             x.text <- data
    //             x
    //         update = fun d' p' el d p ->
    //             el.text <- d; el
    //     }

    // static member inline screenSpace this spec =
    //     { new ScreenSpace<_,_>(this) with
    //         member _.Create d p = spec.create d p
    //         member _.Update d' p' el d p = spec.update d' p' el d p
    //     }

    interface IContainer<UnityProps, VisualElement, VisualElement> with
        member val Container =
            { new ScreenSpace<VisualElement list, VisualElement>(this) with
                member _.Create data props =
                    addChildren (data, new VisualElement())

                /// cache swap wipes out stale children, we don't need to remove them or compare here
                member _.Update data' props' el data props =
                    // FIXME once again, assuming adding the same element is a noop
                    // should just not do this
                    // unfortunately i have no way to diff the children except for a physical compare
                    // likely what the underlying Add already does?
                    addChildren (data, el)
            }

    // interface IText<UnityProps, Button, VisualElement> with
    //     member _.Text = text2
            

    interface IText<UnityProps, Label, VisualElement> with
        member val Text =
            { new ScreenSpace<_, _>(this) with
                    // TODO props
                member _.Create data props =
                    ulog (sprintf "Text create: %s" data)
                    Label data
                member _.Update data' props' el' data props =
                    el'.text <- data
                    el'
            }
    
    interface ISpotlight<UnityBehavior list> with // TODO
        member val Spotlight =
            { new WorldSpace<Vector3>(this) with
                member _.Create data props =
                    GameObject()
                member _.Update data' props' gO data props = gO
            }

    interface IGameObject<UnityGameObject, UnityBehavior list, GameObject, GameObject> with
        member val GameObject =
            { new WorldSpace<UnityGameObject>(this) with
                member _.Create uGameObject behaviors =
                    // TODO behaviors
                    // Child are in behaviors
                    // rename?
                    uGameObject.Create()

                member _.Update uGameObject' behaviors' gameObject uGameObject behaviors =
                    // TODO reconsider this
                    if uGameObject' <> uGameObject then
                        GameObject.Destroy gameObject
                        uGameObject.Create()
                    else
                        gameObject
            }

    interface IJoinContain<GameObject list, UnityProps, VisualGameObjectContainer, VisualElement> with
        member val JoinContain =
            { new ScreenSpace<_, _>(this) with
                member _.Create children props = VisualGameObjectContainer children
                member _.Update a b c d e = c
                    // TODO update
            }

    interface IButton<VisualElement * (unit -> unit), UnityProps, Button, VisualElement> with
        member val Button =
            { new ScreenSpace<VisualElement * (unit -> unit), Button>(this) with
                member _.Create ((child, action)) props =
                    ulog "Button create"
                    let b = Button(action)
                    b.Add child
                    b

                member _.Update ((_, lastAction)) props el ((newChild, newAction)) e =
                    el.Add newChild
                    if lastAction.GetType() <> newAction.GetType() then // TODO do this better
                            // Could easily get the same closure with different values for example
                            // probably better just to always require an explicit key
                            // or always attach/re-attach
                            // could skip if we're physical equal
                        el.remove_clicked lastAction
                        el.add_clicked newAction
                    el
            }

    // TODO These are just examples of multiple specializations
    interface IElement<string, unit, Label, VisualElement> with
        member val Element =
            { new Element<string, unit, Label, VisualElement>((fun x -> x :> VisualElement), base.Cache.Create (UnityNode.remove)) with
                member _.Create x () = Label x
                member _.Update a b el data e =
                    el.text <- data
                    el
            }

    interface IElement<int, unit, Label, VisualElement> with
        member _.Element = intElement

