namespace Scratch.TopDownSketch

module MockRenderer  =
    open System
    open System.Collections.Generic

    type Position = Ordinal of int
    type Cache<'Data, 'Element> = Dictionary<Position, 'Data * 'Element>

    type FreshStale<'D, 'E>() =
        let cacheA : Cache<'D, 'E> = Dictionary()
        let cacheB : Cache<'D, 'E> = Dictionary()
        let mutable isAFresh = true
        member _.Fresh = if isAFresh then cacheA else cacheB
        member _.Stale = if isAFresh then cacheB else cacheA
        member _.Swap () = isAFresh <- not isAFresh
        override x.ToString() =
            sprintf
                "%A"
                {|  fresh = x.Fresh
                    stale = x.Stale
                |}

    let popDict key (d: Dictionary<_,_>) =
        match d.TryGetValue key with
        | true, x ->
            d.Remove key |> ignore
            Some x
        | _ ->
            None

    let renderer<'Data, 'Element>
        (create: unit -> 'Data * 'Element)
        (update: 'Data -> 'Element -> 'Data * 'Element)
        (position: Position)
        (caches: FreshStale<'Data, 'Element>)
        =
        match popDict position caches.Stale with
        | Some (dat, element) ->
            let fresh = update dat element
            caches.Fresh.Add(position, fresh)
        | None ->
            caches.Fresh.Add(position, create())

module MockHost =
    type IVisual = interface end

module Elements =
    open MockRenderer

    type VisualText(content) =
        member val Text = content with get, set 
        interface MockHost.IVisual 

    let text (content: string) =
        renderer          
         <| fun _ -> content, VisualText(content)
         <| fun prevContent element ->
                element.Text <- content
                content, element

module Component =
    open Elements
    open MockRenderer

    open System.Collections.Generic

    let freshStale = FreshStale()

    let root =
        ( Ordinal 0 )

    let view name =
        text $"Hello {name}"

    let renderView model =
        view model root freshStale
        freshStale.Swap()
