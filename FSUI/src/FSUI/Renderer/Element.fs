namespace FSUI.Renderer.Element

open FSUI.Renderer.Cache

type Position =
    | Ordinal of parent: Position * int
    | Nominal of parent: Position * name: string * insertAfter: int
    | Root

[<AbstractClass>]
type Element<'data, 'props, 'visual, 'node>
    (
        [<InlineIfLambda>]
        wrap: 'visual -> 'node,
        [<InlineIfLambda>]
        remove: 'visual -> unit,
        swappers: Swappers
    ) =
    abstract member Create: 'data -> 'props -> 'visual
    abstract member Update: 'data -> 'props -> 'visual -> 'data -> 'props -> 'visual

    member val Cache = swappers.Create(remove)

    member inline private this.Save pos data props el =
        this.Cache.Fresh.Add(pos, (data, props, el))
        el

    member this.Render (data: 'data) (props: 'props) (pos: Position) : 'node =
        match this.Cache.Stale.TryGetValue pos with
        | true, (cachedData, cachedProps, cachedVisual) ->
            this.Update cachedData cachedProps cachedVisual data props
        | _ -> this.Create data props
        |> this.Save pos data props
        |> wrap
