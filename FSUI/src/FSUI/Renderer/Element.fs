namespace FSUI.Renderer.Element

open FSUI.Renderer.Cache
open FSUI.Renderer.Provider

type Position =
    | Ordinal of parent: Position * int
    | Nominal of parent: Position * name: string * insertAfter: int
    | Root

[<AbstractClass>]
type Element<'data, 'props, 'visual, 'node>
    (
        [<InlineIfLambda>]
        wrap: 'visual -> 'node,
        cache: Swapper<Position,'data,'props,'visual>
    ) =
    let save pos data props el =
        cache.Fresh.Add(pos, (data, props, el))
        el

    member this.Render (data: 'data) (props: 'props) (pos: Position) : 'node =
        match cache.Stale.TryGetValue pos with
        | true, (cachedData, cachedProps, cachedVisual) ->
            this.Update cachedData cachedProps cachedVisual data props
        | _ ->
            this.Create data props
        |> save pos data props
        |> wrap

    abstract member Create: 'data -> 'props -> 'visual
    abstract member Update: 'data -> 'props -> 'visual -> 'data -> 'props -> 'visual
