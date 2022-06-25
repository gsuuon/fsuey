﻿namespace FSUI.Renderer

type Position =
    | Ordinal of parent: Position * int
    | Nominal of parent: Position * name: string * insertAfter: int
    | Root

type WrappedNode<'custom, 'data> =
    {
        node : Node<'custom, 'data>
        data : 'data
    }
and Node<'custom, 'data> =
    | Text of string
    | Custom of 'custom

// type PositionedNode<'t> = Position * Node<'t>


module Cache =
    type TypedCache<'k> =
        member _.Get<'t> (key: 'k) =
            // lookup correct cache based on type
            ()
    
    type PositionCache<'node> = Map<string, 'node>

    type Caches<'node> = {
        stale : PositionCache<'node>
        fresh : PositionCache<'node>
    }

    let inline set (cache: PositionCache<'n>) (k, v) = cache.Add (k, v) |> ignore
    let inline get (cache: PositionCache<'n>) k = cache.TryFind k

    let create<'node> () : PositionCache<'node> = Map.empty
    
module Renderer =
    let render<'custom, 'data>
        (customRenderer: 'custom -> unit)
        (cache: Cache.PositionCache<WrappedNode<'custom, 'data>>)
        (node: WrappedNode<'custom, 'data>)
        =
        ()

    let cacheOp (caches: Cache.Caches<_>) position nodeOp =
        let staleNode = Cache.get caches.stale position
        let freshNode = nodeOp staleNode
        Cache.set caches.fresh (position, freshNode)

    let container caches position childOps =
        cacheOp caches position
         <| function
            | Some staleNode ->
                // childOps is always the current set of child ops
                for (childPosition, childOp) in childOps do
                    cacheOp caches childPosition childOp

            | None ->
                ()

module Element =
    ()
    // let text string =
    //     function
    //     | Some oldNode ->
    //         ()

module Domain =
    type Message =
        | Clicked

    type Custom =
        | Spotlight of x : single * y : single
    
    type Style =
        | Green
        | Bold

    
    let update (x: Message) = ()

    let node =
        { node = Node.Text "hi"
          data = [ Bold ]
        }

module Run = 
    let cache = Cache.create()
    let node = Domain.node

    let customRenderer =
        function
        | Domain.Spotlight (x, y) -> ()

    Renderer.render customRenderer cache node
