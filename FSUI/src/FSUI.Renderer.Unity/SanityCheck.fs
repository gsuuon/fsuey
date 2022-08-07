module SanityCheck

open UnityEngine.UIElements

open FSUI.Renderer.Cache
open FSUI.Renderer.Element
open FSUI.Renderer.Unity.Hierarchy

open type UnityNode

type UnityProp =
    | Class of string

type UnityProps = List<UnityProp>

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

let create (wrap: 'visual -> 'node) (cache: Swapper<_,_,_,_>) (element: #IElement<_,_,_>) =
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

type RenderElement<'data, 'props, 'node> = 'data -> 'props -> Position -> 'node

module Interfaces =
    type IText<'props, 'visual, 'node> =
        abstract Text : RenderElement<string, 'props, 'node>

    type IContainer<'props, 'node> =
        abstract Container : RenderElement<List<'node>, 'props, 'node>

module Implementation =
    open Interfaces

    module Views =
        let text props data =
            fun (env: #IText<_,_,_>) pos ->
                env.Text data props pos

        let div props children =
            fun (env: #IContainer<'props, 'nodes>) pos ->
                let childNodes : List<'nodes> =
                    children
                     |> List.mapi
                        ( fun idx render -> render env (Ordinal (pos, idx)) )

                env.Container childNodes props pos

    module Layout =
        open Views

        let layout () =
            div [] [
                text [] "hey"
            ]

    type Provider () =
        let swappers = Swappers()
        let screen x = create (fun (x: #VisualElement) -> x :> VisualElement) (swappers.Create UnityNode.remove) x

        interface IText<UnityProps, Label, VisualElement> with
            member val Text =
                screen {
                    create = fun d p -> Label d
                    update = fun d' p' el d p -> el.text <- d; el
                }

        interface IContainer<UnityProps, VisualElement> with
            member val Container =
                screen {
                    create = fun d p ->
                        let el = VisualElement()
                        for child in d do
                            el.Add child
                        el
                    update = fun d' p' el d p -> el // so on
                }

    module App =
        open FSUI.Renderer.Element

        let env = Provider()
        let view () =
            (Layout.layout ()) env Root

