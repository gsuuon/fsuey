module ScratchNodeIsInterface

module Graph =
    open System
    open System.Collections.Generic

    type Position = Ordinal of int

    type IDataNode = interface end

    // Node independent of renderer

    type Cache = Dictionary<Type, Dictionary<Position, IDataNode>>


module MockHost =
    type VisualNode =
        abstract member Install : unit -> unit


module MockComponent =
    type Text(content: string) =
        interface Graph.INode<string> with
            member _.Data = content

    let text content =
        



module Render =
    open Graph
    open MockHost


    let renderNode (makeVisual: INode -> VisualNode) (node: INode) =
        let visualNode = makeVisual node

        visualNode.Install()
