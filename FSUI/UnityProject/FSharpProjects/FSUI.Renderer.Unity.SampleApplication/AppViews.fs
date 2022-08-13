namespace FSUI.Renderer.Unity.SampleApplication

open FSUI.Types
open FSUI.Renderer.Element
open FSUI.Elements.Interfaces


type AppViews =
    static member inline poly (data: string) =
        fun (env: #IPoly<_,string,_>) (pos: Position) ->
            env.Poly [] data pos

    static member inline poly (data: int) =
        fun (env: #IPoly<_,int,_>) (pos: Position) ->
            env.Poly [] data pos
