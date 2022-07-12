namespace Design.Common
// better name

type Position = Ordinal of int

type INode =
    abstract member Attach : INode -> Unit
    // abstract member Detach : Unit -> Unit

module Interfaces =
    type CacheTag =
        | A
        | B

    type CacheEvent = {
        swap: Event<CacheTag>
        wipe: Event<unit>
    }

    let cache () = System.Collections.Generic.Dictionary()
        
    type SwapCache() =
        member val activeCache = A with get, set

        member val cacheEvents = {
            wipe = Event<unit>()
            swap = Event<CacheTag>()
        }
        member x.Wipe() =
            x.cacheEvents.wipe.Trigger()
        member x.Swap() =
            x.activeCache <-
                match x.activeCache with
                | A -> B
                | B -> A

            x.cacheEvents.swap.Trigger(x.activeCache)
    
    type ISwapCache =
        abstract member Cache : SwapCache

    type ICache<'K, 'V> =
        abstract member Cache : System.Collections.Generic.IDictionary<'K, 'V>

    [<AbstractClass>]
    type ElementBase<'Data, 'Visual>(cacheEvent: CacheEvent) =
        let cacheA = cache()
        let cacheB = cache()
        let mutable activeCache = A
        let subscription =
            cacheEvent.wipe.Publish.Subscribe (
                fun _ ->
                    match activeCache with
                    | A -> cacheA.Clear()
                    | B -> cacheB.Clear()
            )
        
        interface System.IDisposable with
            member _.Dispose () = subscription.Dispose()

        interface ICache<Position, 'Data * 'Visual> with
            member _.Cache =
                match activeCache with
                | A -> cacheA
                | B -> cacheB
            
        abstract member Create : 'Data -> 'Visual
        abstract member Update : 'Data -> 'Visual -> 'Data -> 'Visual
        // abstract member Attach<'Parent> : 'Visual -> Unit

        member element.Render content position =
            let cache = (element :> ICache<_,_>).Cache

            match cache.TryGetValue position with
            | true, (lastData, lastVisual) ->
                let el = element.Update lastData lastVisual content
                cache.Add(position, (content, el))
                // return collectible
            | _ ->
                let el = element.Create content
                cache.Add(position, (content, el))

    type IText<'Visual> =
        abstract member Text : ElementBase<string, 'Visual>

    type IContainer<'Node, 'Visual> =
        abstract member Container : ElementBase<List<'Node>, 'Visual>

    type IButton<'Visual> =
        abstract member Button : ElementBase<string * (unit -> unit), 'Visual>
