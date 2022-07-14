module FSUI.Elements.Views

open FSUI.Elements.Interfaces
open FSUI.Renderer.Element

let inline render ([<InlineIfLambda>]readElement: 'env -> Element<_,_,_,_>) data props env pos =
    (readElement env).Render data props pos

let inline text x = render (fun (env:#IText<_,_,_>) -> env.Text) x
let inline image x = render (fun (env: #IImage<_,_,_>) -> env.Image) x

let div children props (env: #IContainer<_,_,_>) pos =
    let nodes : 'node list =
        children |> List.mapi (fun idx fnode -> fnode env (Ordinal (pos, idx)))

    env.Container.Render nodes props pos
