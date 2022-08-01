namespace FSUI.Renderer.Element

open FSUI.Renderer.Cache
open FSUI.Renderer.Provider

type Position =
    | Ordinal of parent: Position * int
    | Nominal of parent: Position * name: string * insertAfter: int
    | Root

/// Semantics determined by application
/// 'data is the input type
/// 'props is differenced items, properties. TODO
/// 'visual is the cached type. Typically the actual item rendered, so that update is type-safe.
/// 'node is the collection type to be used in containers. Typically a root type.
///    'node is not cached, but is re-wrapped each render to provide to container.
///     Generally best to be statically castable.
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
        let (exists, stale) = cache.Stale.Remove pos
            // for some reason normal match with syntax doesn't work here
        
        match exists, stale with
        | true, (cachedData, cachedProps, cachedVisual) ->
            this.Update cachedData cachedProps cachedVisual data props
        | _ ->
            this.Create data props
        |> save pos data props
        |> wrap

    abstract member Create: 'data -> 'props -> 'visual
    abstract member Update: 'data -> 'props -> 'visual -> 'data -> 'props -> 'visual
