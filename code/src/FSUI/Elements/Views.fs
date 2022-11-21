module FSUI.Elements.Views

open FSUI.Types
open FSUI.Elements.Interfaces

let gameObject props data (env: #IGameObject<_,_,_>) pos =
    env.GameObject props data pos

let text props data (env: #IText<'prop, 'node>) pos =
    env.Text props (string data) pos

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

/// Stringly key an element to preserve it against reordering
let at (name: string) (renderer: 'env -> Position -> 'visual) =
    fun env (pos: Position) -> renderer env (pos.Named name)

type Elements<'prop> =
    // We need to annotate the return types for many of these overloads to ensure they're sufficiently generic

    static member inline text (props: 'prop list) : 'a -> 'b -> Position -> 'c =
        fun a -> text props a
    static member inline text a : 'a -> Position -> 'b =
        text [] a

    static member inline button x =
        fun action -> button [] ( text [] x, Keyed (x, action ) )
    static member inline button (x, key) =
        fun action -> button [] ( text [] x, Keyed (key, action ) )
    static member inline button (x: Renders<_,_>, key) =
        fun action -> button [] ( x, Keyed (key, action ) )

    static member inline div (children: Renders<_,_> list) =
        div [] children
    static member inline div (props: 'prop list) =
        fun (children: Renders<_,_> list) -> div props children
