module FSUI.Renderer.Element

open System.Collections.Generic

open FSUI.Renderer.Cache
open FSUI.Renderer.Provider

open FSUI.Difference

type Position =
    | Ordinal of parent: Position * order: int
    | Nominal of parent: Position * name: string
    | Root
    member this.Named name =
        match this with
        | Root | Nominal _      -> this
        | Ordinal (parent, idx) -> Nominal (parent, name)

type IElement<'prop, 'data, 'visual> =
    abstract Create: IReadOnlyCollection<'prop> -> 'data -> 'visual
    abstract Change: Changes<'prop> -> 'visual -> 'visual
    abstract Update: 'data -> 'data -> 'visual -> 'visual

type ElementRecord<'p, 'd, 'v> =
    { create : IReadOnlyCollection<'p> -> 'd -> 'v
      change : Changes<'p> -> 'v -> 'v
      update : 'd -> 'd -> 'v -> 'v
    }
    interface IElement<'p, 'd, 'v> with
        member this.Create props data = this.create props data
        member this.Change changes visual = this.change changes visual
        member this.Update lastData thisData visual = this.update lastData thisData visual

type RendersElement<'prop, 'data, 'node> = IReadOnlyCollection<'prop> -> 'data -> Position -> 'node

let create<'prop, 'data, 'visual, 'node, 'element
                when 'element :> IElement<'prop, 'data, 'visual>
                 and 'prop : equality
                 and 'data : equality
                 >
    (asNode: 'visual -> 'node)
    (cache: Swapper<Position, IReadOnlyCollection<'prop>, 'data, 'visual>)
    (element: 'element)
        : RendersElement<'prop, 'data, 'node>
    =
    fun (props: IReadOnlyCollection<'prop>) (data: 'data) (pos: Position) ->
        let (exists, last) = cache.Stale.Remove pos // match .. with syntax doesn't get the correct overload
        
        let visual =
            if exists then
                let (props', data', visual') = last

                let visual' =
                    if data' <> data then
                        element.Update data' data visual'
                    else
                        visual'

                match difference props' props with
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
