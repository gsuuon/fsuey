module Design.LayoutElements

open Design.Common
open Design.Common.Interfaces

let text x (e: #IText<_>) = e.Text.Render x
let button x (e: #IButton<_>) = e.Button.Render x

let div<'env, 'node, 'container when 'env :> IContainer<'node, 'container>>
    (children: List<'env -> Position -> 'node>)
    (env: 'env)
    (pos: Position) =
        let renderedNodes =
            children
             |> List.map
                (fun x -> x env pos )

        let container = renderedNodes |> env.Container.Create // TODO what do I do with the container?
        ()
