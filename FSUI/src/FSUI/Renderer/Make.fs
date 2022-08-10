module FSUI.Renderer.Make

open FSUI.Renderer.Element
open FSUI.Renderer.Provider

type Renderer<'model, 'msg, 'env, 'node> =
    abstract member Model : 'model with get
    abstract member View : 'model -> 'node
    abstract member Dispatch : 'msg -> unit

let make
    (initialModel: 'model)
    (view: 'model -> ('msg -> unit) -> 'env -> Position -> 'node)
    (update: 'msg -> 'model -> 'model)
    (env: 'env when 'env :> IProvider)
    (pos: Position)
    =
    let mutable model = initialModel

    { new Renderer<'model, 'msg, 'env, 'node> with
        member _.Model = model
        member this.View model = view model this.Dispatch env pos
        member this.Dispatch msg =
            model <- update msg model
            view model this.Dispatch env pos |> ignore
    }
