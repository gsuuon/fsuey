namespace FSUI.Test.Provider

open FSUI.Test.Host

open FSUI.Renderer.Element
open FSUI.Renderer.Provider
open FSUI.Renderer.Cache
open FSUI.Elements.Interfaces

type VisualProp =
    | Class of string

type Props = VisualProp list

[<AbstractClass>]
type VisualElement<'data, 'visual when 'visual :> Visual>(cache) =
    inherit Element<'data, Props, 'visual, Visual>((fun x -> x :> Visual), cache)

type Env() =
    inherit Provider()

    interface IText<Props, VisualText, Visual> with
        member val Text =
            { new VisualElement<string, VisualText>(base.Cache) with
                member _.Create data props = VisualText data
                member _.Update cachedContent cachedProps cachedVisual data props =
                    cachedVisual.Content <- data
                    cachedVisual
            }

    interface IImage<Props, VisualImage, Visual> with
        member val Image =
            { new VisualElement<string, VisualImage>(base.Cache) with
                member _.Create path props = VisualImage path
                member _.Update cachedContent cachedProps cachedVisual path props =
                    cachedVisual.Path <- path
                    cachedVisual
            }

    interface IContainer<Props, VisualCollection, Visual> with
        member val Container =
            { new VisualElement<Visual list, VisualCollection>(base.Cache) with
                member _.Create children props = VisualCollection children
                member _.Update cachedChildren cachedProps cachedVisual children props =
                    cachedVisual.Children <- children
                    cachedVisual
            }

