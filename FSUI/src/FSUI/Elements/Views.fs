module FSUI.Elements.Views

open System.Collections.Generic

open FSUI.Elements.Interfaces
open FSUI.Renderer.Element

let inline mkRender<'prop, 'data, 'node, 'env>
    ([<InlineIfLambda>]
        renderer: 'env -> RendersElement<'prop, 'data, 'node>)
    (props: IReadOnlyCollection<'prop>)
    (data: 'data)
    (env: 'env)
    (pos: Position)
    =
    (renderer env) props data pos

let text x = mkRender (fun (env: #IText<'prop, 'node>) -> env.Text) x
let gameObject x = mkRender (fun (env: #IGameObject<_,_,_>) -> env.GameObject) x

let div props children (env: #IContainer<_,_>) pos =
    let nodes : 'node list =
        children |> List.mapi (fun idx fnode -> fnode env (Ordinal (pos, idx)))

    env.Container props nodes pos

let button props (child: 'env -> Position -> 'node, action) (env: #IButton<_,_,_>) pos =
    let childNode = child env (Ordinal (pos, 0))

    env.Button props (childNode, action) pos

// TODO this is the same as div
let join props children (env: #IJoinContain<_,_,_>) pos =
    let nodes : 'node list =
        children |> List.mapi (fun idx fnode -> fnode env (Ordinal (pos, idx)))

    env.JoinContain props nodes pos

let at (name: string) (renderer: 'env -> Position -> 'visual) =
    fun env (pos: Position) ->
        renderer env (pos.Named name)
