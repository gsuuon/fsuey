module Scratch

type Dict<'k, 'v> = System.Collections.Generic.IDictionary<'k, 'v>

type StaticCache<'v>() =
    static let cache : Dict<string, 'v> = dict []

    member x.Get pos = 
        match cache.TryGetValue pos with
        | true, x -> Some x
        | _ -> None


type Node<'custom when 'custom : equality> = | Text of string | Custom of 'custom
(*
I'm not sure that I can use a DU to represent the node types

Nodes need to be extensible, DU's are closed. I get around this by making the Node DU type generic, the type param becomes the extension point.
Next problem is caching. I want caches only to return a node if it's the same node case. To do this, I'd need to:
    Create a helper DU type which parallels the node DU type, except with data-less cases. I can't treat cases individually in a DU - it's either
    the DU type or not. The case types don't exist in F#. 
    
    type NodeType =
        | Text
        | Foo
    
    type Node =
        | Text of string
        | Foo of int

    cache<NodeType, cache<Pos,Node>>

    or I create a function which disgards the data and returns if the node is the same case

    cache<Pos, Node>

    let isSame =
        function
        | Text _, Text _ -> true
        | Foo _, Foo _ -> true
        | _ -> false


    But this is very inefficient. I should be able to figure out it out at the outset with typed caches. It's also yet another point to touch for every new node.

    I dont' think I care about the actual case types in the plumbing, an interface should work better here..
*)
type Position = | Ord of int

type NodePositionCache<'custom, 'value when 'custom : equality> () =
    member _.NodeCaches : Dict<Node<'custom>, Dict<Position, 'value>> = dict []

    member inline x.Get node key =
        match x.NodeCaches.TryGetValue node with
        | true, cache ->
            match cache.TryGetValue key with
            | true, v -> Some v
            | _ -> None
        | _ ->
            None

    member x.Set node key value =
        match x.NodeCaches.TryGetValue node with
        | true, cache ->
            cache.Add(key, value)
        | _ ->
            let cache = dict [key, value]

            x.NodeCaches[node] <- cache
        

module Domain =
    type Custom =
        | Words of string

let inline getThing<'T> (cache: NodePositionCache<Custom, 'T>) =
    cache.GetThing<'T> "mykey"

[<Interface>]
type Foo =
    abstract member Bar : string

let main () =
    let cache = new Cache<string, Foo>
