module FSUI.Renderer.Element

open FSUI.Renderer.Cache

open FSUI.Difference
open FSUI.Types

type ElementRecord<'p, 'd, 'v> =
    { create : 'p collection -> 'd -> 'v
      change : Changes<'p> -> 'v -> 'v
      update : 'd -> 'd -> 'v -> 'v
    }
    interface IElementRenderer<'p, 'd, 'v> with
        member this.Create props data = this.create props data
        member this.Change changes visual = this.change changes visual
        member this.Update lastData thisData visual = this.update lastData thisData visual

[<AutoOpen>]
module private Util =
    let inline gate pred fn v =
        if pred then fn v else v

    let inline gateOpt opt fn v =
        match opt with
        | Some x -> fn x v
        | None -> v

    let inline save (cache: Swapper<_,_,_,_>) props data pos visual =
        try
            cache.Fresh.Add (pos, (props, data, visual) )
        with
        | :? System.ArgumentException -> // TODO Is an element existing in Fresh at this position an error?
            eprintfn "Fresh cache already contained an element at %s" (string pos)
        visual

let create<'prop, 'data, 'visual, 'node, 'element
                when 'element :> IElementRenderer<'prop, 'data, 'visual>
                 and 'prop : equality
                 and 'data : equality
                 >
    (asNode: 'visual -> 'node)
    (cache: Swapper<Position, 'prop collection, 'data, 'visual>)
    (element: 'element)
        : Applies<'prop, 'data, 'node>
    =
    fun (props: 'prop collection) (data: 'data) (pos: Position) ->
        let (exists, last) = cache.Stale.Remove pos // match .. with syntax doesn't get the correct overload

        match exists, last with
        | false, _ ->
            element.Create props data
        | true, (props', data', visual') ->
            visual'
             |> gate (data' <> data) (element.Update data' data)
             |> gateOpt (difference props' props) element.Change
        |>  save cache props data pos
        |>  asNode
