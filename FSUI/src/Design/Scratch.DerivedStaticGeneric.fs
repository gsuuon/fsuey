namespace Scratch.DerivedStaticGeneric

open System.Collections.Generic

type BaseStatic<'T> () =
    static let cache = Dictionary<int, 'T>()
    static member Get key = 
        match cache.TryGetValue key with
        | true, x -> Some x
        | _ -> None
    static member Set key value = cache.Add(key, value)


module Cache =
    let inline retrieve'< ^Cache, 'T when 'Cache : (static member Get: int -> 'T option)> idx =
        ((^Cache) : (static member Get : int -> 'T option) (idx))

    type CacheA<'T>() = inherit BaseStatic<'T>()
    type CacheB<'T>() = inherit BaseStatic<'T>()

    type DefaultSwapper() =
        let mutable isA = false

        member _.Retrieve<'T> idx =
            if isA then
                retrieve'<CacheA<_>, 'T> idx
            else
                retrieve'<CacheB<_>, 'T> idx


    type Foo<'T>() = class end
    type Bar<'T>() = class end

    type Baz<'T>() = class end
