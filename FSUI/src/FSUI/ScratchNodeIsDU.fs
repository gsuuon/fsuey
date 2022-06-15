module ScratchNodeIsDU

module Graph =
    open System
    type Position = Ordinal of int

    type DataNode =
        | Text of string
        | Container of DataNode list

    // Node independent of renderer

    type DataNodeTag =
        | Text
        | Container


module Cache =
    open Graph
    open System.Collections.Generic

    type Cache<'Visual> =
        IDictionary<DataNodeTag, IDictionary<Position, DataNode * 'Visual>>
        // NOTE
        // DataNodeTag and DataNode are not related at all here, so despite the addition of the data tag I get no guarantees about the type of the 
        // data I get out of the positional cache

    let checkPos nodetag position (cache: Cache<_>) =
        match cache.TryGetValue nodetag with
        | true, nodeCache ->
            match nodeCache.TryGetValue position with
            | true, item -> Some item
            | _ -> None
        | _ -> None

    let cachedElement<'visual> element elementTag position (cache: Cache<'visual>) =
        let cached = checkPos elementTag position cache

        element cached

module MockHost =
    type VisualNode =
        abstract member Install : unit -> unit

    type TextNode(content: string) =
        member _.SetContent (content: string) = ()
        interface VisualNode with
            member _.Install() = ()
        

module MockElements =
    let text content =
        Cache.cachedElement<MockHost.VisualNode>
         <| function
            | Some ((oldContent, node) as x) when oldContent = content ->
                // NOTE
                // the data content here is still typed as Node, not the specific node case type
                // so I still need to run-time check to make sure the data is the correct type :(
                x
            | None ->
                Graph.DataNode.Text content, MockHost.TextNode(content)

         <| Graph.DataNodeTag.Text

module MockComponent =
    open MockElements

    type Model = {
        title : string
    }
        
    let mainScreen model =
        text model.title

module MockEntry =
    open Graph
    open MockComponent

    let model = {
        title = "hello"
    }

    let dataRender = mainScreen model
    let cache = dict []

    let x =
        dataRender
         <| Ordinal 0
         <| cache


module Render =
    open Graph
    open MockHost


    let renderNode (makeVisual: INode -> VisualNode) (node: INode) =
        let visualNode = makeVisual node

        visualNode.Install()
