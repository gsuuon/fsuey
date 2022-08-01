namespace FSUI.Renderer.Cache

open System.Collections.Generic

type ISwapper =
    abstract Swap : unit -> unit

type Swapper<'K, 'data, 'props, 'visual>(
    [<InlineIfLambda>]mkCache: unit -> Dictionary<'K, 'data * 'props * 'visual>,
    remove : 'visual -> unit
    ) =
    let cacheA = mkCache()
    let cacheB = mkCache()

    let mutable fresh = cacheA
    let mutable stale = cacheB

    member _.Fresh = fresh
    member _.Stale = stale

    interface ISwapper with
        // Swap after view
        // so that fresh has live, stale has stale before swap
        member this.Swap () = 
            for (KeyValue (_,(_,_,x))) in stale do
                remove x

            stale.Clear()

            let fresh' = fresh
            fresh <- stale
            stale <- fresh'

open System.Collections.Generic

type Swappers() =
    let mutable collection = []

    member _.Create (remove) =
        let swap = Swapper((fun () -> Dictionary()), remove)
        collection <- (swap :> ISwapper) :: collection
        swap

    member _.Swap () =
        collection |> List.iter (fun swapper -> swapper.Swap())
