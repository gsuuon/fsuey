// DU Design

type Position = | Ordinal of int

type Node =
    | Text of string
    | Container of Node list

type Cache = Map<System.Type(*NodeCase*), Map<Position, Node>>

let caches : Cache = Map.empty

let aNode = Text hi
let aPosition = Ordinal 0


let find<'T> (node: 'T) (position: Position) = 
    match Map.tryFind typeof<'T> caches with
    | Some cache ->
        cache.TryFind position
    | None ->
        None
(*
val find:
   node    : 'T ->
   position: Position
          -> option<Node>
I need find to return a specific case
so this is impossible?
*)


let text<
