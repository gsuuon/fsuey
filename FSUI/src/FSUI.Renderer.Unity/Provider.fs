namespace FSUI.Renderer.Unity

open UnityEngine
open UnityEngine.UIElements

open FSUI.Renderer.Provider
open FSUI.Renderer.Element
open FSUI.Elements.Interfaces

open FSUI.Renderer.Unity.Interfaces
open FSUI.Renderer.Unity.Hierarchy

open type UnityNode

type UnityProps =
    | Class of string

[<AbstractClass>]
type ScreenSpace<'data, 'element when 'element :> VisualElement>(provider: Provider) =
    inherit Element<'data, UnityProps, 'element, UnityNode>(
        (fun x -> Screen x),
        provider.Cache.Create UnityNode.remove
    )

// TODO worldspace
[<AbstractClass>]
type WorldSpace<'data>(provider: Provider) =
    inherit Element<'data, UnityProps, GameObjectNode, UnityNode>(
        (fun x -> World x),
        provider.Cache.Create UnityNode.remove
    )

type UnityProvider() as this =
    inherit Provider()

    interface IContainer<UnityProps, VisualElement, UnityNode> with
        member val Container =
            { new ScreenSpace<UnityNode list, VisualElement>(this) with
                member _.Create data props =
                    addChildren (data, new VisualElement())

                /// cache swap wipes out stale children, we don't need to remove them or compare here
                member _.Update data' props' el data props =
                    addChildren (data, el)
            }

    interface IText<UnityProps, Label, UnityNode> with
        member val Text =
            { new ScreenSpace<string, Label>(this) with
                    // TODO props
                member _.Create data props =
                    Label data
                member _.Update data' props' el' data props =
                    el'.text <- data
                    el'
            }

    interface ISpotlight<UnityProps> with
        member val Spotlight =
            { new WorldSpace<Vector3>(this) with
                member _.Create data props =
                    { gameObject = new GameObject()
                      visualElementParent = Unchecked.defaultof<_>
                        // NOTE this is just placeholder, I'm abandoning this interlaced visual/gameobject path for an explicit container element
                    }
                member _.Update data' props' gO data props = gO
            }

