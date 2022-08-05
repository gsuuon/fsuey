module FSUI.Renderer.Element

open FSUI.Renderer.Cache
open FSUI.Renderer.Provider

type Position =
    | Ordinal of parent: Position * int
    | Nominal of parent: Position * name: string * insertAfter: int
    | Root

type IElement<'data, 'props, 'visual> =
    abstract member Create: 'data -> 'props -> 'visual
    abstract member Update: 'data -> 'props -> 'visual -> 'data -> 'props -> 'visual

type ElementRecord<'d, 'p, 'v> =
    { create : 'd -> 'p -> 'v
      update : 'd -> 'p -> 'v -> 'd -> 'p -> 'v
    }
    interface IElement<'d, 'p, 'v> with
        member this.Create a b = this.create a b
        member this.Update a b c d e = this.update a b c d e

let create<'data, 'props, 'visual, 'node, 'element when 'element :> IElement<'data, 'props, 'visual>>
    (wrap: 'visual -> 'node)
    (cache: Swapper<Position, 'data, 'props, 'visual>)
    (element: 'element)
        : 'data -> 'props -> Position -> 'node
    =
    fun (data: 'data) (props: 'props) (pos: Position) ->
        let (exists, (data', props', visual')) = cache.Stale.Remove pos // match .. with syntax doesn't work to get the correct overload
        
        let visual =
            if exists then
                element.Update data' props' visual' data props
            else
                element.Create data props

        visual
         |> fun el ->
                try
                    cache.Fresh.Add (pos, (data, props, el) )
                with
                | :? System.ArgumentException -> // TODO Do we care about this?
                    System.Console.Error.WriteLine ("Fresh cache already contained an element at " + pos.ToString())
                el
         |> wrap
