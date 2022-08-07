module FSUI.Renderer.Element

open FSUI.Renderer.Cache
open FSUI.Renderer.Provider

type Position =
    | Ordinal of parent: Position * int
    | Nominal of parent: Position * name: string * insertAfter: int
    | Root

type IElement<'props, 'data, 'visual> =
    abstract Create: 'props -> 'data -> 'visual
    abstract Update: 'props -> 'data -> 'visual -> 'props -> 'data -> 'visual

type ElementRecord<'d, 'p, 'v> =
    { create : 'd -> 'p -> 'v
      update : 'd -> 'p -> 'v -> 'd -> 'p -> 'v
    }
    interface IElement<'d, 'p, 'v> with
        member this.Create a b = this.create a b
        member this.Update a b c d e = this.update a b c d e

type RendersElement<'props, 'data, 'node> = 'props -> 'data -> Position -> 'node

let create<'props, 'data, 'visual, 'node, 'element when 'element :> IElement<'props, 'data, 'visual> >
    (asNode: 'visual -> 'node)
    (cache: Swapper<Position, 'props, 'data, 'visual>)
    (element: 'element)
        : RendersElement<'props, 'data, 'node>
    =
    fun (props: 'props) (data: 'data) (pos: Position) ->
        let (exists, last) = cache.Stale.Remove pos // match .. with syntax doesn't get the correct overload
        
        let visual =
            if exists then
                let (props', data', visual') = last

                element.Update props' data' visual' props data
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
