// DU Design

module ScratchDesign

open System
open System.Collections.Generic

type Position = | Ordinal of int

type SuperCache() =
    let caches : IDictionary<Type, IDictionary<Position, obj>> = dict []

    member _.Get<'T> (key: Position) : 'T option =
        match caches.TryGetValue typeof<'T> with
        | true, cache ->
            match cache.TryGetValue key with
            | true, o -> Some (o :?> 'T)
            | _ -> None
        | _ ->
            None

    member _.Set<'T> (key: Position) (value: 'T) =
        match caches.TryGetValue typeof<'T> with
        | true, cache ->
            cache.Add(key, value)
        | _ ->
            let cache : IDictionary<Position, 'T> = dict [key, value]
            let casted = cache :?> IDictionary<Position, obj>
            caches.Add(typeof<'T>, casted)


type Foo = class end

let superCache = SuperCache()
let x : Foo option = superCache.Get<Foo>(Ordinal 0)
