namespace FSUI.Renderer.Cache

open System.Collections.Generic

type ISwapper =
    abstract Swap : unit -> unit

type Swapper<'K, 'V>([<InlineIfLambda>]mkCache: unit -> IDictionary<'K, 'V>) =
    let cacheA = mkCache()
    let cacheB = mkCache()

    let mutable isA = false

    member _.Cache =
        if isA then cacheA else cacheB

    member this.Find key =
        match this.Cache.TryGetValue key with
        | true, v -> Some v
        | _ -> None

    member this.Save key value =
        this.Cache.Add(key, value)

    interface ISwapper with
        member this.Swap () = 
            this.Cache.Clear()
            isA <- not isA

open System.Collections.Generic

type Swappers() =
    let mutable collection = []

    member _.Create () =
        let swap = Swapper(fun () -> Dictionary())
        collection <- (swap :> ISwapper) :: collection
        swap

    member _.Swap () =
        collection |> List.iter (fun swapper -> swapper.Swap())
