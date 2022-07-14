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
        swappers: Swappers
    ) =
    abstract member Create: 'data -> 'props -> 'visual
    abstract member Update: 'data -> 'props -> 'visual -> 'data -> 'props -> 'visual

    member val Cache = swappers.Create()

    member this.Render (data: 'data) (props: 'props) (pos: Position) : 'node =
        match this.Cache.Find pos with
        | Some (cachedData, cachedProps, cachedVisual) ->
            this.Update cachedData cachedProps cachedVisual data props
        | None -> this.Create data props
        |> wrap
