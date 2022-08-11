module FSUI.Renderer.Element

open FSUI.Renderer.Cache
open FSUI.Renderer.Provider

open FSUI.Difference

type Position =
    | Ordinal of parent: Position * int
    | Nominal of parent: Position * name: string * insertAfter: int
    | Root

type IElement<'prop, 'data, 'visual> =
    abstract Create: seq<'prop> -> 'data -> 'visual
    abstract Change: Changes<'prop> -> 'visual -> 'visual
    abstract Update: 'data -> 'data -> 'visual -> 'visual

type ElementRecord<'p, 'd, 'v> =
    { create : seq<'p> -> 'd -> 'v
      change : Changes<'p> -> 'v -> 'v
      update : 'd -> 'd -> 'v -> 'v
    }
    interface IElement<'p, 'd, 'v> with
        member this.Create props data = this.create props data
        member this.Change changes visual = this.change changes visual
        member this.Update lastData thisData visual = this.update lastData thisData visual

type RendersElement<'prop, 'data, 'node> = seq<'prop> -> 'data -> Position -> 'node

let create<'prop, 'data, 'visual, 'node, 'element
                when 'element :> IElement<'prop, 'data, 'visual>
                 and 'prop : equality
                 and 'data : equality>
    (asNode: 'visual -> 'node)
    (cache: Swapper<Position, seq<'prop>, 'data, 'visual>)
    (element: 'element)
        : RendersElement<'prop, 'data, 'node>
    =
    fun (props: seq<'prop>) (data: 'data) (pos: Position) ->
        let (exists, last) = cache.Stale.Remove pos // match .. with syntax doesn't get the correct overload
        
        let visual =
            if exists then
                let (props', data', visual') = last

                let visual' =
                    if data' <> data then
                        element.Update data' data visual'
                    else
                        visual'

                match Difference.compute props' props with
                | Some changes ->
                    element.Change changes visual'
                | None ->
                    visual'
            else
                element.Create props data

        visual
         |> fun el ->
                try
                    cache.Fresh.Add (pos, (props, data, el) )
                with
                | :? System.ArgumentException -> // TODO Is an element existing in Fresh at this position an error?
                    System.Console.Error.WriteLine ("Fresh cache already contained an element at " + pos.ToString())
                el
         |> asNode
