namespace FSUI.Renderer.Provider

open FSUI.Renderer.Cache

[<AbstractClass>]
type Provider() =
    member val Cache = Swappers()
