module FSUI.Elements.Views

open FSUI.Elements.Interfaces
open FSUI.Renderer.Element

let inline render<'data, 'props, 'visual, 'node, 'env>
    ([<InlineIfLambda>]
        readElement: 'env -> Element<'data,'props,'visual,'node>)
    (props: 'props)
    (data: 'data)
    (env: 'env)
    (pos: Position)
    =
    (readElement env).Render data props pos

let text x = render (fun (env:#IText<_,_,_>) -> env.Text) x
let image x = render (fun (env: #IImage<_,_,_>) -> env.Image) x
let gameObject x = render (fun (env: #IGameObject<_,_,_,_>) -> env.GameObject) x

let div props children (env: #IContainer<_,_,_>) pos =
    let nodes : 'node list =
        children |> List.mapi (fun idx fnode -> fnode env (Ordinal (pos, idx)))

    env.Container.Render nodes props pos

let button props (child: 'env -> Position -> 'node, action: unit -> unit) (env: #IButton<_,_,_,_>) pos =
    let childNode = child env (Ordinal (pos, 0))

    env.Button.Render (childNode, action) props pos

let join props children (env: #IJoinContain<_,_,_,_>) pos =
    let nodes : 'node list =
        children |> List.mapi (fun idx fnode -> fnode env (Ordinal (pos, idx)))

    env.JoinContain.Render nodes props pos
