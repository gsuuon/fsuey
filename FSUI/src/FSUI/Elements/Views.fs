module FSUI.Elements.Views

open FSUI.Elements.Interfaces
open FSUI.Renderer.Element

let inline render<'data, 'props, 'visual, 'node, 'env>
    ([<InlineIfLambda>]
        renderer: 'env -> RendersElement<'data,'props,'visual>)
    (props: 'props)
    (data: 'data)
    (env: 'env)
    (pos: Position)
    =
    (renderer env) data props pos

let text x = render (fun (env: #IText<_,_>) -> env.Text) x
let gameObject x = render (fun (env: #IGameObject<_,_,_>) -> env.GameObject) x

let div props children (env: #IContainer<_,_,_>) pos =
    let nodes : 'node list =
        children |> List.mapi (fun idx fnode -> fnode env (Ordinal (pos, idx)))

    env.Container nodes props pos

let button props (child: 'env -> Position -> 'node, action: unit -> unit) (env: #IButton<_,_,_>) pos =
    let childNode = child env (Ordinal (pos, 0))

    env.Button (childNode, action) props pos

let join props children (env: #IJoinContain<_,_,_>) pos =
    let nodes : 'node list =
        children |> List.mapi (fun idx fnode -> fnode env (Ordinal (pos, idx)))

    env.JoinContain nodes props pos
