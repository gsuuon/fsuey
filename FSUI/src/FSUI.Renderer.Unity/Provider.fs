namespace FSUI.Renderer.Unity

open UnityEngine
open UnityEngine.UIElements

open FSUI.Renderer.Provider
open FSUI.Renderer.Element
open FSUI.Elements.Interfaces

open FSUI.Renderer.Unity.Interfaces

type UnityProps =
    | Class of string

[<AbstractClass>]
type ScreenSpace<'data, 'element when 'element :> VisualElement>(cache) =
    inherit Element<'data, UnityProps, 'element, WrappedElement>(
        (fun x -> Screen x),
        (fun x -> x.RemoveFromHierarchy()),
        cache
    )

// TODO worldspace
[<AbstractClass>]
type WorldSpace<'data>(cache) =
    inherit Element<'data, UnityProps, GameObject, WrappedElement>(
        (fun x -> World x),
        (fun x -> GameObject.Destroy x),
        cache
    )

type UnityUITKProvider() =
    inherit Provider()

    let addChildren (xs: WrappedElement list) (el: VisualElement) =
        xs |> List.iter
                (function
                 | Screen child -> el.Add child
                 | World x -> () // TODO
                 )
        el

    interface IContainer<UnityProps, VisualElement, WrappedElement> with
        member _.Container =
            { new ScreenSpace<WrappedElement list, VisualElement>(base.Cache) with
                member _.Create data props =
                    new VisualElement() |> addChildren data

                /// cache swap wipes out stale children, we don't need to remove them or compare here
                member _.Update data' props' el data props =
                    el |> addChildren data
            }

    interface IText<UnityProps, Label, WrappedElement> with
        member _.Text =
            { new ScreenSpace<string, Label>(base.Cache) with
                    // TODO props
                member _.Create data props =
                    Label data
                member _.Update data' props' el' data props =
                    el'.text <- data
                    el'
            }

    interface ISpotlight<UnityProps> with
        member _.Spotlight =
            { new WorldSpace<Vector3>(base.Cache) with
                member _.Create data props = new GameObject()
                member _.Update data' props' gO data props = gO
            }

