// DU Design

(*
Requirements:
- Get the expected type out of the cache `let x : Foo = cache.Get<Foo> (somePosition)`
- at least 2 caches
- for a given type and position, i get a Some if position has an existing type of same type. Else None
- can iterate through each entire cache, across all the types

Desired:
- No casting
- Decent performance
*)

module ScratchDesign

open System
open System.Collections.Generic

type Position = | Ordinal of int

type InstantiatedTypesGlobalCache() =
    static let mutable instantiatedTypes : Type list = List.empty
    static member Add typ = instantiatedTypes <- typ :: instantiatedTypes
    static member Reset () = instantiatedTypes <- List.empty
    static member InstantiatedTypes = instantiatedTypes

type InstancedGlobalCache<'T>() =
    static let instances : IDictionary<int, IDictionary<Position, 'T>> = dict []
    do
        InstantiatedTypesGlobalCache.Add typeof<'T>

    static member Get<'T> idx (key: Position) : 'T option =
        match instances.TryGetValue idx with
        | true, cache ->
            match cache.TryGetValue key with
            | true, o -> Some o
            | _ -> None
        | _ ->
            None

    static member Set<'T> idx (key: Position) (value: 'T) =
        match instances.TryGetValue idx with
        | true, cache ->
            cache.Add(key, value)
        | _ ->
            instances.Add(idx, dict [key, value])

type CacheAccess(idx: int) =
    let mutable removals : list<unit -> unit> = List.empty

    member _.Get key =
        InstancedGlobalCache.Get idx key
    member cache.Set key value remove =
        removals <-
            (fun _ ->
                match cache.Get key with
                | Some x -> remove x |> ignore
                | None -> ()
            )
            :: removals
        InstancedGlobalCache.Set idx key value
    member _.RemoveAll () =
        for removal in removals do
            removal()

type Foo() =
    member _.Remove () = ()

let cache1 = CacheAccess(1)

let operator (foo: Foo) (cache: CacheAccess) (position: Position) =
    match cache.Get<Foo> position with
    | Some prevFoo -> prevFoo
    | None ->
        cache.Set position foo (fun x -> x.Remove())
        foo

cache1.Set<Foo> (Ordinal 0) (Foo())

let x = cache1.Get<Foo> (Ordinal 0)
