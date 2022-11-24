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

let createBase<'prop, 'data, 'dataRaw, 'visual, 'node, 'element
                when 'element :> IElementRenderer<'prop, 'data, 'visual>
                 and 'prop : equality
                 >
    (wrapData: 'dataRaw -> 'data)
    (diffData: 'data -> 'data -> bool)
    (asNode: 'visual -> 'node)
    (cache: Swapper<Position, 'prop collection, 'data, 'visual>)
    (element: 'element)
        : Applies<'prop, 'dataRaw, 'node>
    =
    fun (props: 'prop collection) (dataRaw: 'dataRaw) (pos: Position) ->
        let data = wrapData dataRaw

        match cache.Stale.Remove pos : bool * _ with
        | false, _ ->
            element.Create props data
             |> save cache props data pos
             |> asNode

        | true, (props', data', visual') ->
            // TODO probably want to refactor this
            let mutable dataChanged = false
            let mutable propsChanged = false

            visual'
             |> gate (diffData data' data) (fun el ->
                    dataChanged <- true
                    element.Update data' data el
                )
             |> gateOpt (difference props' props) (fun el ->
                    propsChanged <- true
                    element.Change el
                )
             |> save cache
                    (if propsChanged then props else props')
                    (if dataChanged then data else data')
                    pos
             |> asNode

let inline create a = createBase id (<>) a
