module Design.LayoutElements

open Design.Common
open Design.Common.Interfaces

type Elements<'Node when 'Node :> INode>() =
    static member inline text x (e: #IText<_>) pos  : 'Node =
        e.Text.Render x pos

let text x (e: #IText<_>) = e.Text.Render x
let button x (e: #IButton<_>) = e.Button.Render x

let div
    (children: List<'env -> Position -> 'node>)
    (env: #IContainer<_,_>)
    (pos: Position) =
        let renderedNodes =
            children
             |> List.map
                (fun x -> x env pos )

        let container = renderedNodes |> env.Container.Create
        ()
